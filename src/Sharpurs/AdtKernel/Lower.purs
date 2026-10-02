-- | Lower supported optimized bodies, validating nested annotations and lexical
-- | levels as they are consumed. Unsupported syntax rejects the whole binding.
module Sharpurs.AdtKernel.Lower (binding) where

import Prelude

import Control.Alternative (guard)
import Data.Array as Array
import Data.Array.NonEmpty as NEA
import Data.Foldable (all)
import Data.Map as Map
import Data.Maybe (Maybe(..))
import Data.Traversable (traverse)
import Data.Tuple (Tuple(..))
import PureScript.Backend.Optimizer.CoreFn (ExprType, Ident, Qualified)
import PureScript.Backend.Optimizer.CoreFn as C
import PureScript.Backend.Optimizer.Semantics (NeutralExpr(..))
import PureScript.Backend.Optimizer.Syntax (Level(..), Pair(..))
import PureScript.Backend.Optimizer.Syntax as S
import Sharpurs.AdtKernel.Analysis (Context, Signature)
import Sharpurs.AdtKernel.Analysis as Analysis
import Sharpurs.AdtKernel.Emit as Emit
import Sharpurs.AdtLayout as Layout
import Sharpurs.Names as Names

binding :: Context -> Qualified Ident -> Signature -> NeutralExpr -> Maybe Emit.Definition
binding ctx name sig expr = do
  _ <- Layout.nativeType ctx.layout sig.result
  case Layout.lookupCtor ctx.layout name of
    Just ctor -> do
      guard (sig.args == ctor.fields && sig.result == ctor.sourceType)
      Analysis.validateConstructor ctor expr
      let parameters = Array.mapWithIndex (\i ty -> { level: Level i, type: ty }) sig.args
      pure { parameters, body: Emit.ctorApplication ctor (map (Emit.localName <<< _.level) parameters) }
    Nothing -> do
      collected <- Analysis.collectParameters sig.args sig.result [] expr
      let levels = map _.level collected.args
      guard (all (\(Level level) -> level >= 0) levels && Analysis.unique levels)
      let locals = Map.fromFoldable (map (\p -> Tuple p.level p.type) collected.args)
      body <- expression (ctx { locals = locals }) sig.result collected.body
      pure { parameters: collected.args, body }

at :: ExprType -> ExprType -> String -> Maybe String
at expected actual code = guard (expected == actual) $> code

expression :: Context -> ExprType -> NeutralExpr -> Maybe String
expression ctx expected (NeutralExpr syntax) = case syntax of
  S.Typed ty inner -> guard (ty == expected) *> expression ctx expected inner
  S.Lit (C.LitInt value) -> at expected C.Int (Emit.intLiteral value)
  S.Lit (C.LitBoolean value) -> at expected C.Boolean (Emit.booleanLiteral value)
  S.Local _ level -> do
    ty <- Map.lookup level ctx.locals
    at expected ty (Emit.localName level)
  S.Var name -> do
    sig <- Map.lookup name ctx.globals
    guard (Array.null sig.args)
    at expected sig.result sig.nativeName
  S.App fn args -> do
    name <- Analysis.globalReference fn
    sig <- Map.lookup name ctx.globals
    Analysis.validateReference sig fn
    guard (expected == sig.result && NEA.length args == Array.length sig.args)
    values <- traverse (\(Tuple ty arg) -> expression ctx ty arg) (Array.zip sig.args (NEA.toArray args))
    let target = if ctx.guardedCalls && ctx.self /= Just name && Layout.lookupCtor ctx.layout name == Nothing
          then Names.guarded sig.nativeName
          else sig.nativeName
    pure (Emit.call { target, arguments: values })
  S.CtorSaturated name _ typeName ctorName fields -> do
    ctor <- Layout.lookupCtor ctx.layout name
    Analysis.checkCtorIdentity ctor name typeName ctorName
    guard (expected == ctor.sourceType && Array.length fields == Array.length ctor.fields)
    guard (all identity (Array.mapWithIndex (\i (Tuple field _) -> field == "value" <> show i) fields))
    values <- traverse (\(Tuple ty (Tuple _ value)) -> expression ctx ty value) (Array.zip ctor.fields fields)
    pure (Emit.ctorApplication ctor values)
  S.Accessor value (S.GetCtorField name _ typeName ctorName field index) -> do
    ctor <- Layout.lookupCtor ctx.layout name
    Analysis.checkCtorIdentity ctor name typeName ctorName
    ty <- Array.index ctor.fields index
    guard (expected == ty && field == "value" <> show index)
    target <- expression ctx ctor.sourceType value
    pure (Emit.projectField { constructor: ctor, value: target, index })
  S.PrimOp (S.Op1 (S.OpIsTag name) value) -> do
    guard (expected == C.Boolean)
    ctor <- Layout.lookupCtor ctx.layout name
    target <- expression ctx ctor.sourceType value
    pure (Emit.isTag { constructor: ctor, value: target })
  S.PrimOp (S.Op2 op left right) -> do
    primitive <- Analysis.primitive op
    guard (expected == primitive.result)
    a <- expression ctx primitive.operand left
    b <- expression ctx primitive.operand right
    pure (Emit.binary { operation: primitive.operation, left: a, right: b })
  S.Let _ level@(Level n) value body -> do
    guard (n >= 0 && not (Map.member level ctx.locals) && all (_ < level) (Map.keys ctx.locals))
    ty <- Analysis.typeOf ctx value
    native <- Layout.nativeType ctx.layout ty
    rhs <- expression ctx ty value
    rest <- expression (ctx { locals = Map.insert level ty ctx.locals }) expected body
    pure (Emit.letIn { level, nativeType: native, value: rhs, body: rest })
  S.Branch branches otherwise -> do
    cases <- traverse (\(Pair condition value) -> do
      conditionCode <- expression ctx C.Boolean condition
      body <- expression ctx expected value
      pure { condition: conditionCode, body }
      ) (NEA.toArray branches)
    fallback <- expression ctx expected otherwise
    pure (Emit.branch { cases, fallback })
  S.Fail message -> Just (Emit.failure message)
  _ -> Nothing
