-- | Type and dependency evidence for the closed ADT subset. Reading a type is
-- | not validation of the body: lowering checks each nested annotation at use.
module Sharpurs.AdtKernel.Analysis
  ( Signature
  , Parameter
  , Context
  , signature
  , signatureType
  , publicName
  , recursiveArgument
  , unsupportedBindings
  , validateSourceParameters
  , validateSourceConstructor
  , validateConstructor
  , collectParameters
  , checkCtorIdentity
  , globalReference
  , validateReference
  , Operation(..)
  , Primitive
  , primitive
  , typeOf
  , unique
  ) where

import Prelude

import Control.Alternative (guard, (<|>))
import Data.Array as Array
import Data.Array.NonEmpty as NEA
import Data.Foldable (all)
import Data.Map (Map)
import Data.Map as Map
import Data.Maybe (Maybe(..))
import Data.Newtype (unwrap)
import Data.Traversable (traverse)
import Data.Tuple (Tuple(..))
import PureScript.Backend.Optimizer.CoreFn (Ann, ExprType, Ident, Qualified(..))
import PureScript.Backend.Optimizer.CoreFn as C
import PureScript.Backend.Optimizer.Semantics (NeutralExpr(..))
import PureScript.Backend.Optimizer.Syntax (Level, Pair(..))
import PureScript.Backend.Optimizer.Syntax as S
import Sharpurs.AdtLayout as Layout
import Sharpurs.Analysis.Source as Source
import Sharpurs.Names as Names

type Signature = { args :: Array ExprType, result :: ExprType, nativeName :: String, publicName :: String }
type Parameter = { level :: Level, type :: ExprType }

-- Mixed modules guard calls between native functions. Whole-native modules use
-- direct calls; constructors and self-recursion never add invocation wrappers.
type Context =
  { layout :: Layout.Layout
  , globals :: Map (Qualified Ident) Signature
  , locals :: Map Level ExprType
  , guardedCalls :: Boolean
  , self :: Maybe (Qualified Ident)
  }

signature :: Layout.Layout -> Ident -> NeutralExpr -> Maybe Signature
signature layout name (NeutralExpr syntax) = do
  ty <- case syntax of
    S.Typed ty _ -> Just ty
    _ -> Nothing
  let sig = case ty of
        C.Func args result -> { args, result }
        result -> { args: [], result }
  _ <- traverse (Layout.nativeType layout) (Array.snoc sig.args sig.result)
  let public = publicName layout name
  pure { args: sig.args, result: sig.result, nativeName: Names.adtNative public, publicName: public }

signatureType :: Signature -> ExprType
signatureType sig = if Array.null sig.args then sig.result else C.Func sig.args sig.result

publicName :: Layout.Layout -> Ident -> String
publicName layout name = Names.inModule layout.moduleName (unwrap name)

recursiveArgument :: Layout.Layout -> ExprType -> Boolean
recursiveArgument layout argument = Array.any
  (\ctor -> ctor.sourceType == argument && Array.elem argument ctor.fields)
  (Array.concatMap _.constructors layout.declarations)

-- PBO may inline an imported call and erase its observable invocation boundary.
-- Block both that source binding and its transitive local dependants, even when
-- the optimized body no longer mentions the import. Only canonical primitives
-- may cross this boundary. One pass per binding bounds the transitive closure.
unsupportedBindings :: C.ModuleName -> Array (Source.SourceBinding Ann) -> Array (Qualified Ident)
unsupportedBindings current bindings =
  let
    references = map (\binding -> { name: Qualified (Just current) binding.name, references: Source.references binding.expr }) bindings
    directlyUnsupported = map _.name (Array.filter (Array.any (unsupportedReference current) <<< _.references) references)
  in Array.foldl (\blocked _ -> Array.nub (blocked <> map _.name
    (Array.filter (Array.any (flip Array.elem blocked) <<< _.references) references))) directlyUnsupported bindings

unsupportedReference :: C.ModuleName -> Qualified Ident -> Boolean
unsupportedReference current = case _ of
  Qualified Nothing _ -> false
  Qualified (Just owner) _ | owner == current -> false
  Qualified (Just (C.ModuleName owner)) (C.Ident name) -> not (case owner of
    "Data.Semiring" -> Array.elem name [ "add", "semiringInt" ]
    "Data.Ring" -> Array.elem name [ "sub", "ringInt" ]
    "Data.Eq" -> Array.elem name [ "eq", "notEq", "eqInt" ]
    "Data.Ord" -> Array.elem name [ "lessThan", "lessThanOrEq", "greaterThan", "greaterThanOrEq", "ordInt" ]
    "Data.HeytingAlgebra" -> Array.elem name [ "conj", "disj", "heytingAlgebraBoolean" ]
    "Data.Boolean" -> name == "otherwise"
    _ -> false)

-- The complete source Abs spine must agree with the signature. A function-valued
-- annotation does not authorize adding arguments across a let/case/TypeApp.
validateSourceParameters :: Array ExprType -> ExprType -> C.Expr Ann -> Maybe Unit
validateSourceParameters remaining result expr = do
  guard (Source.annotation expr == Just (if Array.null remaining then result else C.Func remaining result))
  case expr of
    C.ExprAbs _ _ body -> do
      { tail } <- Array.uncons remaining
      validateSourceParameters tail result body
    _ -> guard (Array.null remaining)

validateSourceConstructor :: Layout.Ctor -> C.Expr Ann -> Maybe Unit
validateSourceConstructor ctor = case _ of
  C.ExprConstructor _ typeName ctorName fields -> do
    checkCtorIdentity ctor ctor.sourceName typeName ctorName
    guard (Array.length fields == Array.length ctor.fields)
    guard (all identity (Array.mapWithIndex (\i field -> field == "value" <> show i) fields))
  _ -> Nothing

validateConstructor :: Layout.Ctor -> NeutralExpr -> Maybe Unit
validateConstructor ctor (NeutralExpr syntax) = case syntax of
  S.Typed ty inner -> do
    guard (ty == if Array.null ctor.fields then ctor.sourceType else C.Func ctor.fields ctor.sourceType)
    validateConstructor ctor inner
  S.CtorDef _ (C.ProperName typeName) name fields -> do
    guard (name == C.unQualified ctor.sourceName)
    guard (Array.length fields == Array.length ctor.fields && unique fields)
    guard (all identity (Array.mapWithIndex (\i field -> field == "value" <> show i) fields))
    case ctor.sourceType of
      C.ADT _ path _ -> guard (Array.last path == Just typeName)
      _ -> Nothing
  S.CtorSaturated name _ typeName ctorName fields -> do
    guard (Array.null fields && Array.null ctor.fields)
    guard (name == ctor.sourceName)
    checkCtorIdentity ctor name typeName ctorName
  _ -> Nothing

collectParameters :: Array ExprType -> ExprType -> Array Parameter -> NeutralExpr -> Maybe { args :: Array Parameter, body :: NeutralExpr }
collectParameters remaining result args expr@(NeutralExpr syntax) = case syntax of
  S.Typed ty inner -> do
    guard (ty == if Array.null remaining then result else C.Func remaining result)
    collectParameters remaining result args inner
  S.Abs parameters body -> do
    let ps = NEA.toArray parameters
    guard (not (Array.null remaining) && Array.length ps <= Array.length remaining)
    let next = Array.zipWith (\(Tuple _ level) ty -> { level, type: ty }) ps remaining
    collectParameters (Array.drop (Array.length ps) remaining) result (args <> next) body
  _ -> do
    guard (Array.null remaining)
    pure { args, body: expr }

checkCtorIdentity :: Layout.Ctor -> Qualified Ident -> C.ProperName -> Ident -> Maybe Unit
checkCtorIdentity ctor qualified (C.ProperName typeName) name = do
  guard (C.unQualified qualified == name)
  case ctor.sourceType of
    C.ADT _ path _ -> guard (Array.last path == Just typeName)
    _ -> Nothing

globalReference :: NeutralExpr -> Maybe (Qualified Ident)
globalReference (NeutralExpr (S.Var name)) = Just name
globalReference (NeutralExpr (S.Typed _ inner)) = globalReference inner
globalReference _ = Nothing

validateReference :: Signature -> NeutralExpr -> Maybe Unit
validateReference sig (NeutralExpr (S.Typed ty inner)) = do
  guard (ty == C.Func sig.args sig.result)
  validateReference sig inner
validateReference _ (NeutralExpr (S.Var _)) = Just unit
validateReference _ _ = Nothing

-- Admitted operations carry semantic identities and type contracts only.
-- Emit owns their target-language spelling and evaluation syntax.
data Operation
  = BooleanAnd
  | BooleanOr
  | IntAdd
  | IntSubtract
  | IntEqual
  | IntNotEqual
  | IntGreaterThan
  | IntGreaterThanOrEqual
  | IntLessThan
  | IntLessThanOrEqual

type Primitive = { operation :: Operation, operand :: ExprType, result :: ExprType }

primitive :: S.BackendOperator2 -> Maybe Primitive
primitive = case _ of
  S.OpBooleanAnd -> Just { operation: BooleanAnd, operand: C.Boolean, result: C.Boolean }
  S.OpBooleanOr -> Just { operation: BooleanOr, operand: C.Boolean, result: C.Boolean }
  S.OpIntNum S.OpAdd -> Just { operation: IntAdd, operand: C.Int, result: C.Int }
  S.OpIntNum S.OpSubtract -> Just { operation: IntSubtract, operand: C.Int, result: C.Int }
  S.OpIntOrd op -> Just { operation: case op of
    S.OpEq -> IntEqual
    S.OpNotEq -> IntNotEqual
    S.OpGt -> IntGreaterThan
    S.OpGte -> IntGreaterThanOrEqual
    S.OpLt -> IntLessThan
    S.OpLte -> IntLessThanOrEqual, operand: C.Int, result: C.Boolean }
  _ -> Nothing

-- This supplies an expected type for a let RHS; it does not validate that RHS.
-- Lower.expression subsequently checks its complete structure and annotations.
typeOf :: Context -> NeutralExpr -> Maybe ExprType
typeOf ctx (NeutralExpr syntax) = case syntax of
  S.Typed ty _ -> Just ty
  S.Local _ level -> Map.lookup level ctx.locals
  S.Lit (C.LitInt _) -> Just C.Int
  S.Lit (C.LitBoolean _) -> Just C.Boolean
  S.PrimOp (S.Op1 (S.OpIsTag _) _) -> Just C.Boolean
  S.PrimOp (S.Op2 op _ _) -> _.result <$> primitive op
  S.Var name -> do
    sig <- Map.lookup name ctx.globals
    guard (Array.null sig.args)
    pure sig.result
  S.App fn _ -> globalReference fn >>= flip Map.lookup ctx.globals <#> _.result
  S.CtorSaturated name _ _ _ _ -> _.sourceType <$> Layout.lookupCtor ctx.layout name
  S.Accessor _ (S.GetCtorField name _ _ _ _ index) -> Layout.lookupCtor ctx.layout name >>= \ctor -> Array.index ctor.fields index
  S.Let _ level value body -> do
    ty <- typeOf ctx value
    typeOf (ctx { locals = Map.insert level ty ctx.locals }) body
  S.Branch branches otherwise -> Array.findMap (\(Pair _ value) -> typeOf ctx value) (NEA.toArray branches) <|> typeOf ctx otherwise
  _ -> Nothing

unique :: forall a. Ord a => Array a -> Boolean
unique values = Array.length (Array.nub values) == Array.length values
