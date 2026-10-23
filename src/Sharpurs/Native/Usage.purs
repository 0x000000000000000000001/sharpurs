-- | Conservative usage analysis for the native IR. A variable is considered
-- | unique when it is read exactly once and never escapes into a constructor,
-- | closure, function argument, boxed value or other unknown owner.
module Sharpurs.Native.Usage
  ( Usage
  , Summary
  , usageExpr
  , usageBinding
  , isUnique
  , isMutable
  , isUnused
  , useCount
  ) where

import Prelude

import Data.Array as Array
import Data.Map (Map)
import Data.Map as Map
import Data.Maybe (Maybe(..))
import Data.Tuple (Tuple(..))
import Sharpurs.Native.IR (NativeCase, NativeExpr(..), NativeParameter)
import Sharpurs.Native.Pattern (variables)

type Usage =
  { uses :: Int
  , escaped :: Boolean
  , fresh :: Boolean
  }

type Summary = Map String Usage

usageExpr :: NativeExpr -> Summary
usageExpr = case _ of
  NativeVar name _ -> addUseName false name Map.empty
  NativeIntLit _ -> Map.empty
  NativeBooleanLit _ -> Map.empty
  NativeStringLit _ -> Map.empty
  NativeLambda parameters body ->
    Map.difference
      (markEscaped body)
      (Map.fromFoldable (map (\parameter -> Tuple parameter.name unit) parameters))
  NativeApply function arguments ->
    mergeSummary
      (usageExpr function)
      (foldSummaries (map markEscaped arguments))
  NativeLet name _ value body ->
    let
      bodyUsage = usageExpr body
      marked = if freshExpression value then Map.alter (map markFresh) name bodyUsage else bodyUsage
    in mergeSummary (usageExpr value) marked
  NativeMatch targets cases fallback ->
    mergeSummary
      (foldSummaries (map usageExpr targets))
      ( mergeSummary
          (foldSummaries (map usageCase cases))
          (case fallback of
            Nothing -> Map.empty
            Just expression -> usageExpr expression
          )
      )
  NativeConstructor _ arguments ->
    foldSummaries (map markEscaped arguments)
  NativeReuse _ target arguments ->
    mergeSummary (usageExpr target) (foldSummaries (map usageExpr arguments))
  NativeSetField target _ value body ->
    mergeSummary
      (mergeSummary (usageExpr target) (usageExpr value))
      (usageExpr body)
  NativeProject _ _ value -> usageExpr value
  NativePrimitive _ arguments ->
    foldSummaries (map usageExpr arguments)
  NativeBox expression ->
    markEscaped expression
  NativeCopy _ expression ->
    usageExpr expression
  NativeCast _ expression ->
    usageExpr expression

usageBinding :: Array NativeParameter -> NativeExpr -> Summary
usageBinding _ body = usageExpr body

usageCase :: NativeCase -> Summary
usageCase { patterns, body } =
  Map.difference
    (usageExpr body)
    (Map.fromFoldable (map (\name -> Tuple name unit) (Array.concatMap variables patterns)))

isUnique :: Summary -> String -> Boolean
isUnique summary name = case Map.lookup name summary of
  Just usage -> usage.uses == 1 && not usage.escaped
  Nothing -> false

isMutable :: Summary -> String -> Boolean
isMutable summary name = case Map.lookup name summary of
  Just usage -> usage.uses == 1 && not usage.escaped && usage.fresh
  Nothing -> false

isUnused :: Summary -> String -> Boolean
isUnused summary name = useCount summary name == 0

useCount :: Summary -> String -> Int
useCount summary name = case Map.lookup name summary of
  Nothing -> 0
  Just usage -> usage.uses

markEscaped :: NativeExpr -> Summary
markEscaped expression =
  map (\usage -> usage { escaped = true }) (usageExpr expression)

addUseName :: Boolean -> String -> Summary -> Summary
addUseName escaped name summary =
  Map.alter (Just <<< combine) name summary
  where
  combine Nothing = { uses: 1, escaped, fresh: false }
  combine (Just usage) = { uses: usage.uses + 1, escaped: usage.escaped || escaped, fresh: usage.fresh }

foldSummaries :: Array Summary -> Summary
foldSummaries = Array.foldl mergeSummary Map.empty

mergeSummary :: Summary -> Summary -> Summary
mergeSummary left right = Map.unionWith combine left right
  where
  combine a b =
    { uses: a.uses + b.uses
    , escaped: a.escaped || b.escaped
    , fresh: a.fresh || b.fresh
    }

markFresh :: Usage -> Usage
markFresh usage = usage { fresh = true }

freshExpression :: NativeExpr -> Boolean
freshExpression = case _ of
  NativeConstructor _ _ -> true
  NativeIntLit _ -> true
  NativeBooleanLit _ -> true
  NativeStringLit _ -> true
  NativeLambda _ _ -> true
  NativePrimitive _ _ -> true
  NativeCopy _ _ -> true
  NativeCast _ _ -> true
  _ -> false
