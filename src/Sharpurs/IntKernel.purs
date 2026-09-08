module Sharpurs.IntKernel
  ( IntKernel
  , Parameter
  , IntExpr(..)
  , IntCondition(..)
  , IntOperator(..)
  , fromBinding
  ) where

import Prelude

import Control.Alternative (guard)
import Data.Array as Array
import Data.Array.NonEmpty (NonEmptyArray)
import Data.Array.NonEmpty as NonEmptyArray
import Data.Foldable (all, foldr)
import Data.Maybe (Maybe(..))
import Data.Traversable (traverse)
import Data.Tuple (Tuple(..))
import PureScript.Backend.Optimizer.CoreFn (ExprType, Ident, Qualified)
import PureScript.Backend.Optimizer.CoreFn as CoreFn
import PureScript.Backend.Optimizer.Semantics (NeutralExpr(..))
import PureScript.Backend.Optimizer.Syntax (Level(..), Pair(..))
import PureScript.Backend.Optimizer.Syntax as Syn

-- All parameters and the result are native Int values. Conditions are kept
-- separate so they cannot accidentally become integer operands in the F# IR.
type IntKernel =
  { name :: Qualified Ident
  , args :: NonEmptyArray Parameter
  , body :: IntExpr
  }

type Parameter = { name :: Maybe Ident, level :: Level }

data IntExpr
  = IntLiteral Int
  | IntLocal Level
  | IntBinary IntOperator IntExpr IntExpr
  | IntIf IntCondition IntExpr IntExpr
  | IntTailCall (NonEmptyArray IntExpr)

data IntCondition = IntEqual IntExpr IntExpr

-- Modulo remains a semantic operation here, not an F# remainder expression.
data IntOperator = IntAdd | IntSubtract | IntModulo

derive instance Eq IntExpr
derive instance Eq IntCondition
derive instance Eq IntOperator

type Context =
  { self :: Qualified Ident
  , signature :: ExprType
  , levels :: Array Level
  }

-- Accept only an entirely supported binding. Nothing lets the caller retain
-- the existing generator; unresolved TypeApp, effects and other forms fail shut.
fromBinding :: Qualified Ident -> NeutralExpr -> Maybe IntKernel
fromBinding name expr@(NeutralExpr syntax) = do
  types <- case syntax of
    Syn.Typed (CoreFn.Func args CoreFn.Int) _ -> do
      guard (not (Array.null args) && all (_ == CoreFn.Int) args)
      pure args
    _ -> Nothing
  collected <- collectArguments types [] expr
  args <- NonEmptyArray.fromArray collected.args
  let
    context =
      { self: name
      , signature: CoreFn.Func types CoreFn.Int
      , levels: map _.level collected.args
      }
  body <- fromInt true context collected.body
  pure { name, args, body }

-- Follow the actual Abs spine, including intervening/repeated Typed nodes.
-- The type alone must not turn a returned function into another source binder.
collectArguments
  :: Array ExprType
  -> Array Parameter
  -> NeutralExpr
  -> Maybe { args :: Array Parameter, body :: NeutralExpr }
collectArguments remaining args expr@(NeutralExpr syntax) = case syntax of
  Syn.Typed ty inner -> do
    let expected = if Array.null remaining then CoreFn.Int else CoreFn.Func remaining CoreFn.Int
    guard (ty == expected)
    collectArguments remaining args inner
  Syn.Abs binders body -> do
    let
      next = map (\(Tuple name level) -> { name, level }) (NonEmptyArray.toArray binders)
      combined = args <> next
      levels = map _.level combined
    guard (Array.length next <= Array.length remaining)
    guard (all (\(Level level) -> level >= 0) levels)
    guard (Array.length (Array.nub levels) == Array.length levels)
    collectArguments (Array.drop (Array.length next) remaining) combined body
  _ -> do
    guard (Array.null remaining)
    pure { args, body: expr }

fromInt :: Boolean -> Context -> NeutralExpr -> Maybe IntExpr
fromInt tailPosition context (NeutralExpr syntax) = case syntax of
  Syn.Typed ty inner -> do
    guard (ty == CoreFn.Int)
    fromInt tailPosition context inner
  Syn.Lit (CoreFn.LitInt value) -> pure (IntLiteral value)
  Syn.Local _ level -> do
    guard (Array.elem level context.levels)
    pure (IntLocal level)
  Syn.PrimOp (Syn.Op2 (Syn.OpIntNum op) left right) ->
    IntBinary <$> fromOperator op <*> fromInt false context left <*> fromInt false context right
  Syn.Branch branches fallback -> do
    lowered <- traverse lowerBranch branches
    otherwise <- fromInt tailPosition context fallback
    pure (foldr (\(Tuple condition yes) no -> IntIf condition yes no) otherwise lowered)
    where
    lowerBranch (Pair condition body) =
      Tuple <$> fromCondition context condition <*> fromInt tailPosition context body
  Syn.App fn args -> do
    guard tailPosition
    guard (NonEmptyArray.length args == Array.length context.levels)
    checkSelf context fn
    IntTailCall <$> traverse (fromInt false context) args
  _ -> Nothing

fromCondition :: Context -> NeutralExpr -> Maybe IntCondition
fromCondition context (NeutralExpr syntax) = case syntax of
  Syn.Typed ty inner -> do
    guard (ty == CoreFn.Boolean)
    fromCondition context inner
  Syn.PrimOp (Syn.Op2 (Syn.OpIntOrd Syn.OpEq) left right) ->
    IntEqual <$> fromInt false context left <*> fromInt false context right
  _ -> Nothing

checkSelf :: Context -> NeutralExpr -> Maybe Unit
checkSelf context (NeutralExpr syntax) = case syntax of
  Syn.Typed ty inner -> do
    guard (ty == context.signature)
    checkSelf context inner
  Syn.Var name -> guard (name == context.self)
  _ -> Nothing

fromOperator :: Syn.BackendOperatorNum -> Maybe IntOperator
fromOperator = case _ of
  Syn.OpAdd -> Just IntAdd
  Syn.OpSubtract -> Just IntSubtract
  Syn.OpMod -> Just IntModulo
  _ -> Nothing
