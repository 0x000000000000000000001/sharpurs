-- | Signature, parameter and capture evidence for the closed thunk subset.
-- | An annotation supplies an expected type; body/call lowering must still prove
-- | the implementation and the provenance of every callback.
module Sharpurs.ThunkKernel.Analysis
  ( Helper(..)
  , Worker
  , Parameter
  , Parameters
  , Capture
  , CapturePlan
  , thunk
  , arrow
  , annotation
  , strip
  , peel
  , lookupExpression
  , helperType
  , helperShape
  , helperResult
  , unitReference
  , worker
  , nativeType
  , collectParameters
  , referenceType
  , validateApplications
  , planCaptures
  , sourceNames
  , unique
  ) where

import Prelude

import Control.Alternative (guard)
import Data.Array as Array
import Data.Array.NonEmpty as NEA
import Data.Foldable (all, foldMap)
import Data.Map (Map)
import Data.Map as Map
import Data.Maybe (Maybe(..))
import Data.Newtype (unwrap)
import Data.Traversable (traverse)
import Data.Tuple (Tuple(..))
import PureScript.Backend.Optimizer.Convert (BackendModule)
import PureScript.Backend.Optimizer.CoreFn (Ann, ExprType, Ident, Qualified(..))
import PureScript.Backend.Optimizer.CoreFn as C
import PureScript.Backend.Optimizer.Semantics (NeutralExpr(..))
import PureScript.Backend.Optimizer.Syntax (Level)
import PureScript.Backend.Optimizer.Syntax as S
import Sharpurs.Analysis.Source as Source
import Sharpurs.FsAst (sanitizeName)

data Helper = Identity | Force

derive instance eqHelper :: Eq Helper

type Worker = { name :: Qualified Ident, nativeName :: String, args :: Array ExprType, result :: ExprType }
type Parameter = { level :: Level, type :: ExprType }
type Parameters = { parameters :: Array Parameter, body :: NeutralExpr }
type Capture = { name :: Ident, target :: String }
type CapturePlan = { captures :: Array Capture, locals :: Map Ident String }

thunk :: ExprType
thunk = C.Func [ C.Unit ] C.Int

-- Source signatures flatten a function-valued result. Worker arity comes from
-- source lambdas, so this suffix remains the returned thunk, not an extra arg.
arrow :: Array ExprType -> ExprType -> ExprType
arrow args result = if Array.null args then result else case result of
  C.Func tail ret -> C.Func (args <> tail) ret
  _ -> C.Func args result

annotation :: NeutralExpr -> Maybe ExprType
annotation (NeutralExpr (S.Typed ty _)) = Just ty
annotation _ = Nothing

-- Shape inspection only; peel/referenceType check the annotations at use.
strip :: NeutralExpr -> NeutralExpr
strip (NeutralExpr (S.Typed _ inner)) = strip inner
strip expr = expr

peel :: ExprType -> NeutralExpr -> Maybe NeutralExpr
peel expected (NeutralExpr (S.Typed ty inner)) = guard (expected == ty) *> peel expected inner
peel _ expr = Just expr

lookupExpression :: Ident -> BackendModule -> Maybe NeutralExpr
lookupExpression name backend = Array.findMap
  (\(Tuple ident expr) -> if ident == name then Just expr else Nothing)
  (Array.concatMap _.bindings backend.bindings)

helperType :: Helper -> ExprType -> ExprType
helperType Identity result = arrow [ C.Func [ C.Unit ] result ] (C.Func [ C.Unit ] result)
helperType Force result = C.Func [ C.Func [ C.Unit ] result ] result

helperShape :: Helper -> ExprType -> Boolean
helperShape kind ty = case ty of
  C.ForAll [ variable ] body -> body == helperType kind (C.TypeVar variable)
  C.Func _ (C.TypeVar variable) -> ty == helperType kind (C.TypeVar variable)
  _ -> ty == helperType kind C.Int

helperResult :: Helper -> ExprType -> Maybe ExprType
helperResult kind ty = case ty of
  C.ForAll [ variable ] body -> guard (body == helperType kind (C.TypeVar variable)) $> C.TypeVar variable
  C.Func _ result@(C.TypeVar _) -> guard (ty == helperType kind result) $> result
  _ -> guard (ty == helperType kind C.Int) $> C.Int

unitReference :: Qualified Ident -> Boolean
unitReference = case _ of
  Qualified (Just (C.ModuleName "Data.Unit")) (C.Ident "unit") -> true
  _ -> false

worker :: Qualified Ident -> String -> Map (Qualified Ident) Helper -> Source.SourceBinding Ann -> NeutralExpr -> Maybe Worker
worker name nativeName helpers binding expression = do
  sourceType <- Source.annotation binding.expr
  guard (annotation expression == Just sourceType)
  let count = Array.length (Source.lambdas binding.expr).parameters
  selected <- case sourceType of
    C.Func arguments result -> do
      guard (count > 0 && count < Array.length arguments)
      let args = Array.take count arguments
          returned = arrow (Array.drop count arguments) result
      guard (returned == thunk && Array.elem thunk args)
      _ <- traverse nativeType args
      pure { name, nativeName, args, result: returned }
    _ -> Nothing
  validateSource selected.args selected.result binding.expr
  let references = Source.references binding.expr
  guard (all (allowedReference selected helpers) references)
  guard (binding.recursive || not (Array.elem selected.name references))
  pure selected

allowedReference :: Worker -> Map (Qualified Ident) Helper -> Qualified Ident -> Boolean
allowedReference selected helpers = case _ of
  Qualified Nothing _ -> true
  name | name == selected.name || Map.member name helpers || unitReference name -> true
  Qualified (Just (C.ModuleName owner)) (C.Ident name) -> case owner of
    "Data.Semiring" -> Array.elem name [ "add", "semiringInt" ]
    "Data.Ring" -> Array.elem name [ "sub", "ringInt" ]
    "Data.Eq" -> Array.elem name [ "eq", "notEq", "eqInt" ]
    "Data.Ord" -> Array.elem name [ "lessThan", "lessThanOrEq", "greaterThan", "greaterThanOrEq", "ordInt" ]
    _ -> false

-- Stop at the worker's arity, leaving a returned lambda in the body. This is
-- deliberately different from the ADT kernel's complete source lambda spine.
validateSource :: Array ExprType -> ExprType -> C.Expr Ann -> Maybe Unit
validateSource remaining result expr = do
  guard (Source.annotation expr == Just (arrow remaining result))
  if Array.null remaining then pure unit else case expr of
    C.ExprAbs _ _ body -> validateSource (Array.drop 1 remaining) result body
    _ -> Nothing

nativeType :: ExprType -> Maybe String
nativeType = case _ of
  C.Int -> Just "int"
  C.Boolean -> Just "bool"
  C.Unit -> Just "unit"
  ty | ty == thunk -> Just "(unit -> int)"
  _ -> Nothing

collectParameters :: Array ExprType -> ExprType -> Array Parameter -> NeutralExpr -> Maybe Parameters
collectParameters remaining result parameters expr@(NeutralExpr syntax) = case syntax of
  S.Typed ty inner -> guard (ty == arrow remaining result) *> collectParameters remaining result parameters inner
  _ | Array.null remaining -> Just { parameters, body: expr }
  S.Abs args body -> do
    let ps = NEA.toArray args
    guard (NEA.length args <= Array.length remaining)
    let next = Array.zipWith (\(Tuple _ level) ty -> { level, type: ty }) ps remaining
    collectParameters (Array.drop (Array.length ps) remaining) result (parameters <> next) body
  _ -> Nothing

referenceType :: ExprType -> NeutralExpr -> Maybe Unit
referenceType expected (NeutralExpr syntax) = case syntax of
  S.Typed ty inner -> guard (ty == expected) *> referenceType expected inner
  S.Var _ -> Just unit
  S.Local _ _ -> Just unit
  _ -> Nothing

validateApplications :: Array ExprType -> ExprType -> C.Expr Ann -> Maybe Unit
validateApplications args result expr = do
  guard (Source.annotation expr == Just result)
  case Array.unsnoc args, expr of
    Just { init, last }, C.ExprApp _ fn arg -> do
      guard (Source.annotation arg == Just last)
      validateApplications init (arrow [ last ] result) fn
    Nothing, _ -> pure unit
    _, _ -> Nothing

-- Reserve fresh native names in first-reference order. Call lowering resolves
-- their values before installing these aliases and before entering the thunk.
-- This traversal finds candidates; it does not authorize unsupported body forms.
planCaptures :: Map Ident String -> Ident -> C.Expr Ann -> CapturePlan
planCaptures locals parameter body =
  let
    names = Array.nub (Array.filter (_ /= parameter) (intReferences body))
    reserved = map (sanitizeName <<< unwrap) names <> sourceNames body <> Array.fromFoldable (Map.values locals)
    fresh i = "sharpurs_thunk_capture_" <> show i
    available = Array.filter (\candidate -> not (Array.elem candidate reserved))
      (map fresh (Array.range 0 (Array.length names + Array.length reserved)))
    bindings = Array.zip names available
  in
    { captures: map (\(Tuple name target) -> { name, target }) bindings
    , locals: Map.union (Map.fromFoldable bindings) locals
    }

intReferences :: C.Expr Ann -> Array Ident
intReferences expr = case expr of
  C.ExprVar _ (Qualified Nothing name) | Source.annotation expr == Just C.Int -> [ name ]
  _ -> foldMap intReferences (Source.children expr)

sourceNames :: C.Expr Ann -> Array String
sourceNames expr = map (sanitizeName <<< unwrap) (case expr of
  C.ExprAbs _ name _ -> [ name ]
  C.ExprLet _ bindings _ -> map _.name (Array.concatMap Source.bindings bindings)
  C.ExprCase _ _ branches -> foldMap (\(C.CaseAlternative binders _) -> foldMap binderNames binders) branches
  _ -> []) <> foldMap sourceNames (Source.children expr)
  where
  binderNames = case _ of
    C.BinderVar _ name -> [ name ]
    C.BinderNamed _ name inner -> [ name ] <> binderNames inner
    C.BinderLit _ literal -> foldMap binderNames literal
    C.BinderConstructor _ _ _ binders -> foldMap binderNames binders
    _ -> []

unique :: forall a. Ord a => Array a -> Boolean
unique values = Array.length (Array.nub values) == Array.length values
