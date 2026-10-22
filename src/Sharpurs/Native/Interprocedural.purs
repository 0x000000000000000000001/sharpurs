-- | Conservative inter-procedural ownership summaries. A private function may
-- | mutate a parameter only when it uses it once and every known call site
-- | passes a fresh or already-unique value. Exported functions never mutate
-- | their parameters because external callers are unknown.
module Sharpurs.Native.Interprocedural
  ( FunctionInfo
  , Summary
  , analyze
  , parameterMutable
  ) where

import Prelude

import Data.Array as Array
import Data.Map (Map)
import Data.Map as Map
import Data.Maybe (Maybe(..))
import Data.Set (Set)
import Data.Set as Set
import Data.Tuple (Tuple(..))
import Sharpurs.Native.IR (NativeExpr(..), NativeParameter)
import Sharpurs.Native.Ownership as Ownership
import Sharpurs.Native.Usage as Usage

type FunctionInfo =
  { name :: String
  , parameters :: Array NativeParameter
  , body :: NativeExpr
  }

type Summary =
  { parameters :: Array String
  , mutableParameters :: Array Boolean
  , resultFresh :: Boolean
  }

type Call =
  { callee :: String
  , arguments :: Array NativeExpr
  }

analyze :: Set String -> Array FunctionInfo -> Map String Summary
analyze exported functions = go 0 (Map.fromFoldable (map (initial) functions))
  where
  initial function =
    Tuple function.name
      { parameters: map _.name function.parameters
      , mutableParameters: map (const false) function.parameters
      , resultFresh: Ownership.summarize function.parameters function.body # Ownership.resultFresh
      }

  go iteration summaries
    | iteration > Array.length functions + 1 = summaries
    | otherwise =
        let next = Map.fromFoldable (map (summarize summaries) functions)
        in if next == summaries then summaries else go (iteration + 1) next

  summarize summaries function =
    Tuple function.name
      { parameters: map _.name function.parameters
      , mutableParameters: Array.mapWithIndex (mutableParameter summaries function) function.parameters
      , resultFresh: Ownership.summarize function.parameters function.body # Ownership.resultFresh
      }

  mutableParameter summaries function index _ =
    not (Set.member function.name exported)
      && usedOnce function index
      && allCallersSafe summaries function index

  usedOnce function index =
    case Array.index function.parameters index of
      Nothing -> false
      Just parameter ->
        Usage.isUnique (Usage.usageBinding function.parameters function.body) parameter.name

  allCallersSafe summaries function index =
    let calls = Array.concatMap (callsIn <<< _.body) functions
        targets = Array.filter (\call -> call.callee == function.name) calls
    in not (Array.null targets)
      && Array.all (safeCall summaries function index) targets

  safeCall summaries function index call =
    case Array.index call.arguments index of
      Nothing -> false
      Just argument ->
        safeArgument summaries function.name argument

  safeArgument summaries callerName argument = case argument of
    NativeConstructor _ _ -> true
    NativeIntLit _ -> true
    NativeBooleanLit _ -> true
    NativeStringLit _ -> true
    NativeLambda _ _ -> true
    NativePrimitive _ _ -> true
    NativeVar name _ ->
      usageIsMutable callerName name || callerParameterMutable summaries callerName name
    NativeApply (NativeVar callee _) _ -> resultFresh summaries callee
    NativeReuse _ target _ -> safeArgument summaries callerName target
    NativeLet _ _ value body -> safeArgument summaries callerName value || safeArgument summaries callerName body
    _ -> false

  usageIsMutable callerName name =
    case Array.find (\function -> function.name == callerName) functions of
      Nothing -> false
      Just function -> Usage.isMutable (Usage.usageBinding function.parameters function.body) name

  callerParameterMutable summaries callerName name =
    case Map.lookup callerName summaries of
      Nothing -> false
      Just summary -> case Array.findIndex (_ == name) summary.parameters of
        Nothing -> false
        Just index -> case Array.index summary.mutableParameters index of
          Just value -> value
          Nothing -> false

  resultFresh summaries callee = case Map.lookup callee summaries of
    Just summary -> summary.resultFresh
    Nothing -> false

callsIn :: NativeExpr -> Array Call
callsIn = case _ of
  NativeVar _ _ -> []
  NativeIntLit _ -> []
  NativeBooleanLit _ -> []
  NativeStringLit _ -> []
  NativeLambda _ body -> callsIn body
  NativeApply function arguments ->
    (case function of
      NativeVar callee _ -> [ { callee, arguments } ]
      _ -> [])
      <> callsIn function
      <> Array.concatMap callsIn arguments
  NativeLet _ _ value body -> callsIn value <> callsIn body
  NativeMatch targets cases fallback ->
    Array.concatMap callsIn targets
      <> Array.concatMap (callsIn <<< _.body) cases
      <> case fallback of
          Nothing -> []
          Just body -> callsIn body
  NativeConstructor _ arguments -> Array.concatMap callsIn arguments
  NativeReuse _ target arguments -> callsIn target <> Array.concatMap callsIn arguments
  NativeProject _ _ value -> callsIn value
  NativePrimitive _ arguments -> Array.concatMap callsIn arguments
  NativeBox expression -> callsIn expression

parameterMutable :: Map String Summary -> String -> NativeParameter -> Boolean
parameterMutable summaries functionName parameter =
  case Map.lookup functionName summaries of
    Nothing -> false
    Just summary -> case Array.findIndex (_ == parameter.name) summary.parameters of
      Nothing -> false
      Just index -> case Array.index summary.mutableParameters index of
        Just value -> value
        Nothing -> false
