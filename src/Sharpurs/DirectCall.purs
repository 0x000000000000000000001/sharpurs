module Sharpurs.DirectCall (Candidate, fromBinding, fromCall) where

import Prelude

import Control.Alternative (guard)
import Data.Array as Array
import Data.Foldable (all)
import Data.Map (Map)
import Data.Map as Map
import Data.Maybe (Maybe(..), isJust)
import PureScript.Backend.Optimizer.CoreFn (Ann(..), Bind(..), Binding(..), Expr(..), ExprType, Ident(..), Qualified(..), exprAnn)
import PureScript.Backend.Optimizer.CoreFn as C
import Sharpurs.AdtLayout as Layout
import Sharpurs.FsAst (sanitizeName)

type Candidate =
  { name :: Ident
  , args :: Array String
  , argTypes :: Array ExprType
  , result :: ExprType
  , body :: Expr Ann
  }

-- Arity comes from consecutive source lambdas, checked against every TAST
-- suffix. A function returned after computing the body is not another argument.
-- The body and the public curried ABI remain unchanged by this optimization.
fromBinding :: Maybe Layout.Layout -> Bind Ann -> Maybe Candidate
fromBinding layout = case _ of
  NonRec (Binding bindingAnn name expression) -> do
    signature <- annotation expression
    { argTypes, result } <- case signature of
      C.Func argTypes result -> Just { argTypes, result }
      _ -> Nothing
    let
      chain = lambdas expression
      args = map (sanitizeName <<< _.name) chain.parameters
    guard (Array.length args >= 2 && Array.length args == Array.length argTypes)
    guard (Array.length (Array.nub args) == Array.length args)
    guard (all Layout.validIdentifier args)
    guard (all (supportedType layout) (Array.snoc argTypes result))
    guard (hasType (C.Func argTypes result) bindingAnn)
    guard (all identity (Array.mapWithIndex
      (\index parameter -> hasType (C.Func (Array.drop index argTypes) result) parameter.ann)
      chain.parameters))
    guard (hasType result (exprAnn chain.body))
    pure { name, args, argTypes, result, body: chain.body }
  _ -> Nothing

-- Only exact, qualified, saturated calls can bypass the wrapper. In particular,
-- TypeApp is not peeled off: these closed monomorphic signatures have no type
-- variable to instantiate, so such a call retains the existing generic path.
fromCall :: Map (Qualified Ident) Candidate -> Expr Ann -> Maybe
  { candidate :: Candidate
  , args :: Array (Expr Ann)
  }
fromCall candidates expression = do
  let call = applications expression []
  case call.head of
    ExprVar ann qualified@(Qualified (Just _) name) -> do
      candidate <- Map.lookup qualified candidates
      guard (candidate.name == name)
      guard (Array.length call.arguments == Array.length candidate.argTypes)
      guard (hasType (C.Func candidate.argTypes candidate.result) ann)
      guard (all identity (Array.mapWithIndex
        (\index argument ->
          Array.index candidate.argTypes index == annotation argument.value
            && hasType (remainingType candidate (index + 1)) argument.ann)
        call.arguments))
      pure { candidate, args: map _.value call.arguments }
    _ -> Nothing

type Parameter = { name :: String, ann :: Ann }

lambdas :: Expr Ann -> { parameters :: Array Parameter, body :: Expr Ann }
lambdas = case _ of
  ExprAbs ann (Ident name) body ->
    let rest = lambdas body
    in rest { parameters = Array.cons { name, ann } rest.parameters }
  body -> { parameters: [], body }

type Argument = { value :: Expr Ann, ann :: Ann }

applications :: Expr Ann -> Array Argument -> { head :: Expr Ann, arguments :: Array Argument }
applications expression arguments = case expression of
  ExprApp ann head value -> applications head (Array.cons { value, ann } arguments)
  head -> { head, arguments }

remainingType :: Candidate -> Int -> ExprType
remainingType candidate applied =
  let remaining = Array.drop applied candidate.argTypes
  in if Array.null remaining then candidate.result else C.Func remaining candidate.result

supportedType :: Maybe Layout.Layout -> ExprType -> Boolean
supportedType layout = case _ of
  C.Int -> true
  C.Boolean -> true
  ty -> isJust (layout >>= \accepted -> Layout.nativeType accepted ty)

annotation :: Expr Ann -> Maybe ExprType
annotation expression = case exprAnn expression of
  Ann ann -> ann.type

hasType :: ExprType -> Ann -> Boolean
hasType expected (Ann ann) = ann.type == Just expected
