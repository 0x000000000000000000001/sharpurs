-- | Lower optimized worker bodies. Nested annotations, lexical levels, exact
-- | saturation and the small set of callable references are checked at use.
module Sharpurs.ThunkKernel.Lower (binding) where

import Prelude

import Control.Alternative (guard)
import Data.Array as Array
import Data.Array.NonEmpty as NEA
import Data.Foldable (all)
import Data.Map (Map)
import Data.Map as Map
import Data.Maybe (Maybe(..))
import Data.Traversable (traverse)
import Data.Tuple (Tuple(..))
import PureScript.Backend.Optimizer.CoreFn (ExprType, Ident, Qualified)
import PureScript.Backend.Optimizer.CoreFn as C
import PureScript.Backend.Optimizer.Semantics (NeutralExpr(..))
import PureScript.Backend.Optimizer.Syntax (Level(..), Pair(..))
import PureScript.Backend.Optimizer.Syntax as S
import Sharpurs.ThunkKernel.Analysis (Helper(..), Worker, arrow, thunk, unitReference, helperType, nativeType, referenceType, strip)
import Sharpurs.ThunkKernel.Analysis as Analysis
import Sharpurs.ThunkKernel.Emit as Emit

type Context = { worker :: Worker, helpers :: Map (Qualified Ident) Helper, locals :: Map Level ExprType }
type Primitive = { symbol :: String, result :: ExprType }

binding :: Worker -> Map (Qualified Ident) Helper -> NeutralExpr -> Maybe Emit.Definition
binding worker helpers expr = do
  collected <- Analysis.collectParameters worker.args worker.result [] expr
  let levels = map _.level collected.parameters
  guard (all (\(Level level) -> level >= 0) levels && Analysis.unique levels)
  let locals = Map.fromFoldable (map (\p -> Tuple p.level p.type) collected.parameters)
  body <- expression { worker, helpers, locals } worker.result collected.body
  pure { parameters: collected.parameters, body }

expression :: Context -> ExprType -> NeutralExpr -> Maybe String
expression ctx expected (NeutralExpr syntax) = case syntax of
  S.Typed ty inner -> guard (ty == expected) *> expression ctx expected inner
  S.Lit (C.LitInt n) -> guard (expected == C.Int) $> Emit.intLiteral n
  S.Lit (C.LitBoolean b) -> guard (expected == C.Boolean) $> Emit.booleanLiteral b
  S.Local _ level -> do
    guard (Map.lookup level ctx.locals == Just expected)
    pure (Emit.localName level)
  S.Var name -> guard (expected == C.Unit && unitReference name) $> Emit.unitValue
  S.Abs parameters body -> do
    guard (expected == thunk)
    case NEA.toArray parameters of
      [ Tuple _ level@(Level n) ] -> do
        guard (n >= 0 && not (Map.member level ctx.locals))
        bodyCode <- expression (ctx { locals = Map.insert level C.Unit ctx.locals }) C.Int body
        pure (Emit.lambda level bodyCode)
      _ -> Nothing
  S.App fn args -> case strip fn of
    NeutralExpr (S.Local _ level) -> do
      guard (expected == C.Int && Map.lookup level ctx.locals == Just thunk)
      referenceType thunk fn
      case NEA.toArray args of
        [ arg ] -> Emit.localCall level <$> expression ctx C.Unit arg
        _ -> Nothing
    NeutralExpr (S.Var name) | name == ctx.worker.name -> do
      guard (expected == ctx.worker.result && NEA.length args == Array.length ctx.worker.args)
      referenceType (arrow ctx.worker.args ctx.worker.result) fn
      values <- traverse (\(Tuple ty arg) -> expression ctx ty arg) (Array.zip ctx.worker.args (NEA.toArray args))
      pure (Emit.call ctx.worker.nativeName values)
    NeutralExpr (S.Var name) -> do
      helper <- Map.lookup name ctx.helpers
      referenceType (helperType helper C.Int) fn
      case helper, NEA.toArray args of
        Identity, [ arg ] -> guard (expected == thunk) *> expression ctx thunk arg
        Force, [ arg ] -> do
          guard (expected == C.Int)
          Emit.force <$> expression ctx thunk arg
        _, _ -> Nothing
    _ -> Nothing
  S.PrimOp (S.Op2 op left right) -> do
    prim <- primitive op
    guard (expected == prim.result)
    a <- expression ctx C.Int left
    b <- expression ctx C.Int right
    pure (Emit.binary prim.symbol a b)
  S.Let _ level@(Level n) value body -> do
    guard (n >= 0 && not (Map.member level ctx.locals) && all (_ < level) (Map.keys ctx.locals))
    ty <- typeOf ctx value
    native <- nativeType ty
    rhs <- expression ctx ty value
    rest <- expression (ctx { locals = Map.insert level ty ctx.locals }) expected body
    pure (Emit.letIn level native rhs rest)
  S.Branch branches otherwise -> do
    cases <- traverse (\(Pair condition value) -> do
      conditionCode <- expression ctx C.Boolean condition
      body <- expression ctx expected value
      pure { condition: conditionCode, body }
      ) (NEA.toArray branches)
    fallback <- expression ctx expected otherwise
    pure (Emit.branch cases fallback)
  -- Failures, foreigns, unknown calls and other callback shapes stay boxed.
  _ -> Nothing

primitive :: S.BackendOperator2 -> Maybe Primitive
primitive = case _ of
  S.OpIntNum S.OpAdd -> Just { symbol: "+", result: C.Int }
  S.OpIntNum S.OpSubtract -> Just { symbol: "-", result: C.Int }
  S.OpIntOrd op -> Just { symbol: case op of
    S.OpEq -> "="
    S.OpNotEq -> "<>"
    S.OpLt -> "<"
    S.OpLte -> "<="
    S.OpGt -> ">"
    S.OpGte -> ">=", result: C.Boolean }
  _ -> Nothing

-- Expected type for a let RHS, followed by complete validation in expression.
typeOf :: Context -> NeutralExpr -> Maybe ExprType
typeOf ctx (NeutralExpr syntax) = case syntax of
  S.Typed ty _ -> Just ty
  S.Local _ level -> Map.lookup level ctx.locals
  S.Lit (C.LitInt _) -> Just C.Int
  S.Lit (C.LitBoolean _) -> Just C.Boolean
  S.Var name | unitReference name -> Just C.Unit
  S.PrimOp (S.Op2 op _ _) -> _.result <$> primitive op
  S.Abs _ _ -> Just thunk
  S.App fn _ -> case strip fn of
    NeutralExpr (S.Local _ level) -> guard (Map.lookup level ctx.locals == Just thunk) $> C.Int
    NeutralExpr (S.Var name) | name == ctx.worker.name -> Just ctx.worker.result
    NeutralExpr (S.Var name) -> case Map.lookup name ctx.helpers of
      Just Identity -> Just thunk
      Just Force -> Just C.Int
      _ -> Nothing
    _ -> Nothing
  _ -> Nothing
