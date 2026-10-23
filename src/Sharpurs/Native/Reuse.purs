-- | Conservative reuse candidates for the native IR. This pass does not
-- | rewrite code yet; it identifies matches whose scrutinees are unique and
-- | whose body reconstructs the same constructor. A later pass can turn those
-- | candidates into in-place updates with a copy fallback.
module Sharpurs.Native.Reuse
  ( Candidate
  , findCandidates
  , hasCandidates
  , rewrite
  , rewriteWith
  , rewriteWithSelf
  ) where

import Prelude

import Data.Array as Array
import Data.Map (Map)
import Data.Map as Map
import Data.Maybe (Maybe(..))
import Data.Set (Set)
import Data.Set as Set
import Data.Traversable (traverse)
import Data.Tuple (Tuple(..))
import Sharpurs.Native.IR (NativeCase, NativeExpr(..))
import Sharpurs.Native.Pattern (NativePattern(..), variables)
import Sharpurs.Native.Types (NativeType(..))
import Sharpurs.Native.Usage as Usage

-- | One match that can become a mutation if the scrutinees stay unique.
-- | `scrutinees` are the source variables, `bindings` the variables bound by
-- | the constructor pattern.
type Candidate =
  { scrutinees :: Array String
  , constructor :: String
  , bindings :: Array String
  }

findCandidates :: Usage.Summary -> NativeExpr -> Array Candidate
findCandidates usage expression = case expression of
  NativeVar _ _ -> []
  NativeIntLit _ -> []
  NativeBooleanLit _ -> []
  NativeStringLit _ -> []
  NativeLambda _ body -> findCandidates usage body
  NativeApply function arguments ->
    findCandidates usage function <> Array.concatMap (findCandidates usage) arguments
  NativeLet _ _ value body ->
    findCandidates usage value <> findCandidates usage body
  NativeMatch targets cases fallback ->
    candidateFromMatch usage targets cases
      <> Array.concatMap (findCandidates usage) targets
      <> Array.concatMap (findCandidates usage <<< _.body) cases
      <> case fallback of
          Nothing -> []
          Just body -> findCandidates usage body
  NativeConstructor _ arguments -> Array.concatMap (findCandidates usage) arguments
  NativeReuse _ target arguments ->
    findCandidates usage target <> Array.concatMap (findCandidates usage) arguments
  NativeSetField target _ value body ->
    findCandidates usage target <> findCandidates usage value <> findCandidates usage body
  NativeProject _ _ value -> findCandidates usage value
  NativePrimitive _ arguments -> Array.concatMap (findCandidates usage) arguments
  NativeBox expression' -> findCandidates usage expression'
  NativeCopy _ expression' -> findCandidates usage expression'
  NativeCast _ expression' -> findCandidates usage expression'

hasCandidates :: Usage.Summary -> NativeExpr -> Boolean
hasCandidates usage = not <<< Array.null <<< findCandidates usage

candidateFromMatch :: Usage.Summary -> Array NativeExpr -> Array NativeCase -> Array Candidate
candidateFromMatch usage targets cases = case traverse targetName targets of
  Nothing -> []
  Just scrutinees
    | Array.all (Usage.isMutable usage) scrutinees ->
        Array.mapMaybe (candidateFromCase scrutinees) cases
    | otherwise -> []
  where
  targetName = case _ of
    NativeVar name _ -> Just name
    _ -> Nothing

candidateFromCase :: Array String -> NativeCase -> Maybe Candidate
candidateFromCase scrutinees { patterns, body } = do
  pattern <- case patterns of
    [ one ] -> Just one
    _ -> Nothing
  constructor <- case pattern of
    PatternConstructor name bindings -> Just { name, bindings }
    _ -> Nothing
  if reconstructsSameConstructor body constructor.name then
    pure
      { scrutinees
      , constructor: constructor.name
      , bindings: variables pattern
      }
  else Nothing

reconstructsSameConstructor :: NativeExpr -> String -> Boolean
reconstructsSameConstructor expression constructor = case expression of
  NativeConstructor name _ -> name == constructor
  _ -> false

rewrite :: Usage.Summary -> NativeExpr -> NativeExpr
rewrite = rewriteWith Set.empty

rewriteWithSelf :: String -> Set String -> Usage.Summary -> NativeExpr -> NativeExpr
rewriteWithSelf self mutableNames usage expression = case expression of
  NativeVar _ _ -> expression
  NativeIntLit _ -> expression
  NativeBooleanLit _ -> expression
  NativeStringLit _ -> expression
  NativeLambda parameters body ->
    NativeLambda parameters (rewriteWithSelf self mutableNames usage body)
  NativeApply function arguments ->
    NativeApply (rewriteWithSelf self mutableNames usage function) (map (rewriteWithSelf self mutableNames usage) arguments)
  NativeLet name type' value body ->
    NativeLet name type' (rewriteWithSelf self mutableNames usage value) (rewriteWithSelf self mutableNames usage body)
  NativeMatch [ target@(NativeVar name _) ] [ case' ] Nothing
    | Usage.isMutable usage name || Set.member name mutableNames ->
        case case'.patterns of
          [ PatternConstructor constructor _ ] ->
            case case'.body of
              NativeConstructor rebuilt arguments
                | rebuilt == constructor ->
                    NativeReuse constructor target (map (rewriteWithSelf self mutableNames usage) arguments)
              _ -> rewriteMatch self mutableNames usage [ target ] [ case' ] Nothing
          _ -> rewriteMatch self mutableNames usage [ target ] [ case' ] Nothing
  NativeMatch targets cases fallback ->
    rewriteMatch self mutableNames usage targets cases fallback
  NativeConstructor name arguments ->
    NativeConstructor name (map (rewriteWithSelf self mutableNames usage) arguments)
  NativeReuse name target arguments ->
    NativeReuse name (rewriteWithSelf self mutableNames usage target) (map (rewriteWithSelf self mutableNames usage) arguments)
  NativeSetField target index value body ->
    NativeSetField
      (rewriteWithSelf self mutableNames usage target)
      index
      (rewriteWithSelf self mutableNames usage value)
      (rewriteWithSelf self mutableNames usage body)
  NativeProject name index value ->
    NativeProject name index (rewriteWithSelf self mutableNames usage value)
  NativePrimitive primitive arguments ->
    NativePrimitive primitive (map (rewriteWithSelf self mutableNames usage) arguments)
  NativeBox expression' ->
    NativeBox (rewriteWithSelf self mutableNames usage expression')
  NativeCopy ty expression' ->
    NativeCopy ty (rewriteWithSelf self mutableNames usage expression')
  NativeCast type' expression' ->
    NativeCast type' (rewriteWithSelf self mutableNames usage expression')

rewriteWith :: Set String -> Usage.Summary -> NativeExpr -> NativeExpr
rewriteWith = rewriteWithSelf ""

rewriteMatch :: String -> Set String -> Usage.Summary -> Array NativeExpr -> Array NativeCase -> Maybe NativeExpr -> NativeExpr
rewriteMatch self mutableNames usage targets cases fallback =
  NativeMatch
    (map (rewriteWithSelf self mutableNames usage) targets)
    (map rewriteCase cases)
    (map (rewriteWithSelf self mutableNames usage) fallback)
  where
  rewriteCase case' =
    let
      body = case branchTargets mutableNames usage targets case'.patterns case'.bindings of
        [] -> case'.body
        branchTargets' ->
          rewriteSelfCalls self mutableNames usage targets case'.patterns branchTargets' case'.body
    in case' { body = rewriteWithSelf self mutableNames usage body }

branchTargets :: Set String -> Usage.Summary -> Array NativeExpr -> Array NativePattern -> Map String NativeType -> Array Target
branchTargets mutableNames usage targets patterns _ =
  case Array.findMap candidate (Array.range 0 (Array.length targets - 1)) of
    Nothing -> []
    Just target -> [ target ]
  where
  candidate index = do
    target <- Array.index targets index
    pattern <- Array.index patterns index
    case target of
      NativeVar name type' | Usage.isMutable usage name || Set.member name mutableNames ->
        case pattern of
          PatternConstructor _ fields | not (Array.null fields) ->
            Just { expression: target, name, type': type', index }
          _ -> Nothing
      _ -> Nothing

type Target =
  { expression :: NativeExpr
  , name :: String
  , type' :: NativeType
  , index :: Int
  }

rewriteSelfCalls :: String -> Set String -> Usage.Summary -> Array NativeExpr -> Array NativePattern -> Array Target -> NativeExpr -> NativeExpr
rewriteSelfCalls self mutableNames usage targets patterns branchTargets body =
  case Array.uncons branchTargets of
    Nothing -> body
    Just { head: target } ->
      let
        fieldIndex = case Array.index patterns target.index of
          Just (PatternConstructor _ fields) -> Array.findIndex (\pattern -> Array.elem target.name (variables pattern)) fields
          _ -> Nothing
      in case fieldIndex of
        Nothing -> body
        Just index -> rewriteSelfCall self mutableNames usage targets target index body

rewriteSelfCall :: String -> Set String -> Usage.Summary -> Array NativeExpr -> Target -> Int -> NativeExpr -> NativeExpr
rewriteSelfCall self mutableNames usage targets target fieldIndex expression = case expression of
  NativeApply function@(NativeVar name _) arguments
    | name == self ->
        case Array.findMap fieldArgument arguments of
          Just _ ->
            NativeLet "sharpurs_recursive_result" target.type'
              (NativeApply function (map (rewriteSelfCall self mutableNames usage targets target fieldIndex) arguments))
              (NativeSetField target.expression fieldIndex
                (NativeVar "sharpurs_recursive_result" target.type')
                (NativeVar "sharpurs_recursive_result" target.type'))
          Nothing -> expression
  NativeVar _ _ -> expression
  NativeIntLit _ -> expression
  NativeBooleanLit _ -> expression
  NativeStringLit _ -> expression
  NativeLambda parameters body -> NativeLambda parameters (rewriteSelfCall self mutableNames usage targets target fieldIndex body)
  NativeApply function arguments -> NativeApply
    (rewriteSelfCall self mutableNames usage targets target fieldIndex function)
    (map (rewriteSelfCall self mutableNames usage targets target fieldIndex) arguments)
  NativeLet name type' value body -> NativeLet name type'
    (rewriteSelfCall self mutableNames usage targets target fieldIndex value)
    (rewriteSelfCall self mutableNames usage targets target fieldIndex body)
  NativeMatch targetExprs cases fallback -> NativeMatch
    (map (rewriteSelfCall self mutableNames usage targets target fieldIndex) targetExprs)
    (map (\case' -> case' { body = rewriteSelfCall self mutableNames usage targets target fieldIndex case'.body }) cases)
    (map (rewriteSelfCall self mutableNames usage targets target fieldIndex) fallback)
  NativeConstructor name arguments -> NativeConstructor name (map (rewriteSelfCall self mutableNames usage targets target fieldIndex) arguments)
  NativeReuse name value arguments -> NativeReuse name
    (rewriteSelfCall self mutableNames usage targets target fieldIndex value)
    (map (rewriteSelfCall self mutableNames usage targets target fieldIndex) arguments)
  NativeSetField value index value' body -> NativeSetField
    (rewriteSelfCall self mutableNames usage targets target fieldIndex value)
    index
    (rewriteSelfCall self mutableNames usage targets target fieldIndex value')
    (rewriteSelfCall self mutableNames usage targets target fieldIndex body)
  NativeProject name index value -> NativeProject name index (rewriteSelfCall self mutableNames usage targets target fieldIndex value)
  NativePrimitive primitive arguments -> NativePrimitive primitive (map (rewriteSelfCall self mutableNames usage targets target fieldIndex) arguments)
  NativeBox expression' -> NativeBox (rewriteSelfCall self mutableNames usage targets target fieldIndex expression')
  NativeCopy ty expression' -> NativeCopy ty (rewriteSelfCall self mutableNames usage targets target fieldIndex expression')
  NativeCast type' expression' -> NativeCast type' (rewriteSelfCall self mutableNames usage targets target fieldIndex expression')
  where
  fieldArgument argument = case argument of
    NativeVar name _ | name == target.name -> Just unit
    _ -> Nothing

rewriteConstructors :: Set String -> Usage.Summary -> Array Target -> NativeExpr -> NativeExpr
rewriteConstructors mutableNames usage targets expression = case expression of
  NativeConstructor constructor arguments ->
    case Array.uncons targets of
      Nothing -> NativeConstructor constructor (map (rewriteConstructors mutableNames usage []) arguments)
      Just { head: target, tail: rest } ->
        NativeReuse constructor target.expression
          (map (rewriteConstructors mutableNames usage rest) arguments)
  NativeVar _ _ -> expression
  NativeIntLit _ -> expression
  NativeBooleanLit _ -> expression
  NativeStringLit _ -> expression
  NativeLambda parameters body ->
    NativeLambda parameters (rewriteConstructors mutableNames usage targets body)
  NativeApply function arguments ->
    NativeApply
      (rewriteConstructors mutableNames usage targets function)
      (map (rewriteConstructors mutableNames usage targets) arguments)
  NativeLet name type' value body ->
    NativeLet name type' (rewriteConstructors mutableNames usage targets value) (rewriteConstructors mutableNames usage targets body)
  NativeMatch targetExprs cases fallback ->
    NativeMatch
      (map (rewriteConstructors mutableNames usage targets) targetExprs)
      (map (\case' -> case' { body = rewriteConstructors mutableNames usage targets case'.body }) cases)
      (map (rewriteConstructors mutableNames usage targets) fallback)
  NativeReuse constructor value arguments ->
    NativeReuse constructor
      (rewriteConstructors mutableNames usage targets value)
      (map (rewriteConstructors mutableNames usage targets) arguments)
  NativeSetField target index value body ->
    NativeSetField
      (rewriteConstructors mutableNames usage targets target)
      index
      (rewriteConstructors mutableNames usage targets value)
      (rewriteConstructors mutableNames usage targets body)
  NativeProject name index value ->
    NativeProject name index (rewriteConstructors mutableNames usage targets value)
  NativePrimitive primitive arguments ->
    NativePrimitive primitive (map (rewriteConstructors mutableNames usage targets) arguments)
  NativeBox expression' -> NativeBox (rewriteConstructors mutableNames usage targets expression')
  NativeCopy ty expression' -> NativeCopy ty (rewriteConstructors mutableNames usage targets expression')
  NativeCast type' expression' -> NativeCast type' (rewriteConstructors mutableNames usage targets expression')
