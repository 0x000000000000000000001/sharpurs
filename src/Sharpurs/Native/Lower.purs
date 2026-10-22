-- | Conservative source-to-native lowering. Any unsupported expression rejects
-- | the binding; callers may then use the boxed backend.
module Sharpurs.Native.Lower
  ( Signature
  , lowerBinding
  , lowerExpr
  ) where

import Prelude

import Control.Alternative (guard)
import Data.Array as Array
import Data.Map (Map)
import Data.Map as Map
import Data.Maybe (Maybe(..))
import Data.Traversable (traverse)
import Data.Tuple (Tuple(..))
import PureScript.Backend.Optimizer.CoreFn (Ann(..), Binder(..), CaseAlternative(..), CaseGuard(..), Expr(..), Ident(..), Literal(..), ModuleName(..), Qualified(..))
import PureScript.Backend.Optimizer.CoreFn as C
import PureScript.Backend.Optimizer.Syntax as S
import Sharpurs.Analysis.Source as Source
import Sharpurs.FsAst (sanitizeName)
import Sharpurs.IntArithmetic as IntArithmetic
import Sharpurs.IntComparison as IntComparison
import Sharpurs.Native.IR (NativeExpr(..), NativeParameter, NativePrimitive(..))
import Sharpurs.Native.Layout as NativeLayout
import Sharpurs.Native.Pattern as NativePattern
import Sharpurs.Native.Types (Layout, NativeType(..))
import Sharpurs.Native.Types as NativeTypes

type Signature =
  { name :: String
  , args :: Array NativeType
  , result :: NativeType
  }

type Env =
  { layout :: Layout
  , locals :: Map String NativeType
  , globals :: Array (Tuple (Qualified Ident) Signature)
  }

lowerBinding :: Layout -> Array (Tuple (Qualified Ident) Signature) -> Signature -> Expr Ann -> Maybe { parameters :: Array NativeParameter, body :: NativeExpr }
lowerBinding layout globals signature expression = do
  let lambda = Source.lambdas expression
  guard (Array.length lambda.parameters == Array.length signature.args)
  let
    parameterTypes = Array.zip signature.args lambda.parameters
    parameters = map
      (\(Tuple ty parameter) -> { name: sanitizeParameter parameter.name, type: ty })
      parameterTypes
    locals = Map.fromFoldable
      (map (\(Tuple ty parameter) -> Tuple (sanitizeParameter parameter.name) ty) parameterTypes)
  body <- lowerExpr { layout, locals, globals } lambda.body
  pure { parameters, body }

sanitizeParameter :: Ident -> String
sanitizeParameter (Ident name) = sanitizeName name

lowerExpr :: Env -> Expr Ann -> Maybe NativeExpr
lowerExpr env expression = case stripTypeApp expression of
  ExprVar _ qualified -> variable qualified
  ExprLit _ literal -> literalExpr literal
  ExprConstructor _ _ _ _ -> constructor qualifiedConstructor []
  ExprApp _ _ _ -> application
  ExprCase _ targets alternatives -> caseExpr targets alternatives
  ExprLet _ _ _ -> Nothing
  ExprAbs _ _ _ -> Nothing
  ExprAccessor _ _ _ -> Nothing
  ExprUpdate _ _ _ -> Nothing
  ExprTypeApp _ inner _ -> lowerExpr env inner
  where
  stripTypeApp = case _ of
    ExprTypeApp _ inner _ -> stripTypeApp inner
    value -> value

  qualifiedConstructor = case stripTypeApp expression of
    ExprConstructor _ _ ident _ -> Qualified (Just (moduleName env.layout)) ident
    _ -> Qualified Nothing (Ident "")

  variable qualified = case qualified of
    Qualified Nothing (Ident name) -> NativeVar (sanitizeName name) <$> Map.lookup (sanitizeName name) env.locals
    _ -> do
      signature <- lookupGlobal qualified env.globals
      pure (NativeVar signature.name (NativeTypes.NativeFunction signature.args signature.result))

  literalExpr = case _ of
    LitInt value -> Just (NativeIntLit value)
    LitBoolean value -> Just (NativeBooleanLit value)
    LitString value -> Just (NativeStringLit value)
    _ -> Nothing

  application = case IntComparison.fromExpr expression of
    Just call -> do
      left <- lowerExpr env call.left
      right <- lowerExpr env call.right
      pure (NativePrimitive (case call.operator of
        S.OpLt -> PrimitiveLessThan
        S.OpGt -> PrimitiveGreaterThan
        _ -> PrimitiveLessThan) [ left, right ])
    Nothing -> case IntArithmetic.fromExpr expression of
      Just call -> do
        left <- lowerExpr env call.left
        right <- lowerExpr env call.right
        pure (NativePrimitive (case call.operator of
          S.OpAdd -> PrimitiveAdd
          S.OpSubtract -> PrimitiveSubtract
          _ -> PrimitiveModulo) [ left, right ])
      Nothing -> do
        let call = Source.flattenApp expression
        case stripTypeApp call.fn of
          ExprConstructor _ _ ident _ -> constructor
            (Qualified (Just (moduleName env.layout)) ident)
            call.args
          ExprVar _ qualified -> case lookupCtor env qualified of
            Just _ -> constructor qualified call.args
            Nothing -> do
              signature <- lookupGlobal qualified env.globals
              guard (Array.length signature.args == Array.length call.args)
              arguments <- traverse (lowerExpr env) call.args
              pure (NativeApply (NativeVar signature.name (NativeTypes.NativeFunction signature.args signature.result)) arguments)
          _ -> Nothing

  constructor qualified arguments = do
    ctor <- lookupCtor env qualified
    guard (Array.length arguments == Array.length ctor.fields)
    values <- traverse (lowerExpr env) arguments
    pure (NativeConstructor ctor.name values)

  caseExpr targets alternatives = do
    targetExprs <- traverse (lowerExpr env) targets
    cases <- traverse alternative alternatives
    pure (NativeMatch targetExprs cases Nothing)

  alternative (CaseAlternative binders guard) = case guard of
    Unconditional body -> do
      patterns <- traverse (NativePattern.fromBinder env.layout) binders
      locals <- binderLocals env.layout binders
      body' <- lowerExpr (env { locals = Map.union locals env.locals }) body
      pure { patterns, bindings: locals, body: body' }
    Guarded _ -> Nothing

binderLocals :: Layout -> Array (Binder Ann) -> Maybe (Map String NativeType)
binderLocals layout binders = do
  maps <- traverse (binderTypes layout) binders
  pure (Map.unions maps)

binderTypes :: Layout -> Binder Ann -> Maybe (Map String NativeType)
binderTypes layout = case _ of
  BinderVar ann (Ident name) -> do
    ty <- annotationType layout ann
    pure (Map.singleton (sanitizeName name) ty)
  BinderNamed ann (Ident name) inner -> do
    ty <- annotationType layout ann
    rest <- binderTypes layout inner
    pure (Map.insert (sanitizeName name) ty rest)
  BinderConstructor _ _ qualified binders -> do
    ctor <- NativeLayout.lookupConstructor layout qualified
    guard (Array.length ctor.fields == Array.length binders)
    maps <- traverse (\(Tuple ty binder) -> typedBinder layout ty binder) (Array.zip ctor.fields binders)
    pure (Map.unions maps)
  _ -> Just Map.empty

typedBinder :: Layout -> NativeType -> Binder Ann -> Maybe (Map String NativeType)
typedBinder layout ty = case _ of
  BinderVar _ (Ident name) -> pure (Map.singleton (sanitizeName name) ty)
  BinderNamed _ (Ident name) inner -> Map.insert (sanitizeName name) ty <$> typedBinder layout ty inner
  BinderConstructor _ _ qualified binders -> do
    ctor <- NativeLayout.lookupConstructor layout qualified
    guard (Array.length ctor.fields == Array.length binders)
    maps <- traverse (\(Tuple fieldTy binder) -> typedBinder layout fieldTy binder) (Array.zip ctor.fields binders)
    pure (Map.unions maps)
  _ -> pure Map.empty

annotationType :: Layout -> Ann -> Maybe NativeType
annotationType layout (Ann annotation) = annotation.type >>= NativeLayout.sourceType layout

lookupCtor :: Env -> Qualified Ident -> Maybe NativeTypes.Constructor
lookupCtor env qualified = case NativeLayout.lookupConstructor env.layout qualified of
  Just ctor -> Just ctor
  Nothing -> case qualified of
    Qualified _ (Ident name) -> Array.findMap
      (\(Tuple _ ctor) -> case ctor.sourceName of
        Qualified _ (Ident sourceName) -> if sourceName == name then Just ctor else Nothing)
      (Map.toUnfoldable env.layout.constructors)

lookupGlobal :: Qualified Ident -> Array (Tuple (Qualified Ident) Signature) -> Maybe Signature
lookupGlobal name signatures = Array.findMap
  (\(Tuple key signature) -> if key == name then Just signature else Nothing)
  signatures

moduleName :: Layout -> ModuleName
moduleName layout = ModuleName layout.moduleName
