-- | Ordinary case translation and the nested-match path for deep unary patterns.
-- | Expression translation is supplied by the caller, keeping this module
-- | independent of expression dispatch and its recursive environments.
module Sharpurs.CodeGen.Case (translate) where

import Prelude

import Data.Array as Array
import Data.Maybe (Maybe(..), fromMaybe)
import Data.Traversable (traverse)
import PureScript.Backend.Optimizer.CoreFn (Ann, Expr, CaseAlternative(..), CaseGuard(..), Guard(..))
import Sharpurs.CodeGen.Boxed as Boxed
import Sharpurs.CodeGen.Pattern (Chain(..))
import Sharpurs.CodeGen.Pattern as Pattern
import Sharpurs.FsAst (FsExpr(..), FsMatchCase(..), FsPattern(..))

translate :: Pattern.Env -> (Expr Ann -> FsExpr) -> Array (Expr Ann) -> Array (CaseAlternative Ann) -> FsExpr
translate env emit expressions alternatives
  -- A case over an uninhabited type is unreachable and keeps the ordinary failure.
  | Array.null alternatives = Boxed.patternFailure
  | otherwise =
      let
        value = Boxed.matchValue (map emit expressions)
        plan = case expressions of
          [ _ ] -> nestedMatchPlan env alternatives
          _ -> Nothing
      in case plan of
        Just branches -> compileLevel { depth: 0, boxedValue: value, scrutinee: value }
          (map (\branch -> { pattern: branch.pattern, body: emit branch.body }) branches)
          Boxed.patternFailure
        Nothing -> FsMatch value (Array.concatMap (translateAlternative env emit) alternatives)

translateAlternative :: Pattern.Env -> (Expr Ann -> FsExpr) -> CaseAlternative Ann -> Array FsMatchCase
translateAlternative env emit (CaseAlternative binders guards) =
  let
    pattern = case map (Pattern.translate env) binders of
      [] -> FsPatWildcard
      [ single ] -> single
      patterns -> FsPatTuple patterns
  in case guards of
    Unconditional body -> [ FsMatchCase pattern Nothing (Boxed.parenthesize (emit body)) ]
    Guarded branches -> map (\(Guard guard body) ->
      FsMatchCase pattern (Just (emit guard)) (Boxed.parenthesize (emit body))) branches

type Branch body = { pattern :: Chain, body :: body }

-- Deep F# active patterns compile extremely slowly (notably the derived Generic
-- dictionaries of large sums). Split eligible unary chains into small matches.
-- Guards, multiple scrutinees and unsupported subtrees retain ordinary matching.
nestedMatchPlan :: Pattern.Env -> Array (CaseAlternative Ann) -> Maybe (Array (Branch (Expr Ann)))
nestedMatchPlan env alternatives = do
  branches <- traverse planAlternative alternatives
  let depth = Array.foldl (\deepest branch -> max deepest (Pattern.chainDepth branch.pattern)) 0 branches
  if depth < minChainDepth then Nothing else Just branches
  where
  planAlternative = case _ of
    CaseAlternative [ binder ] (Unconditional body) -> do
      pattern <- Pattern.forChain env binder
      pure { pattern, body }
    _ -> Nothing

minChainDepth :: Int
minChainDepth = 4

type ConstructorBranch = { inner :: Maybe Chain, body :: FsExpr }

type ConstructorGroup = { name :: String, branches :: Array ConstructorBranch }

-- A catch-all optionally binds the matched value. Constructor and leaf patterns
-- cannot enter this slot, so compiling it always yields exactly one match case.
type CatchAll = { name :: Maybe String, body :: FsExpr }

type ScannedBranches =
  { constructors :: Array ConstructorGroup
  , leaves :: Array FsMatchCase
  , catchAll :: Maybe CatchAll
  }

scanBranches :: Array (Branch FsExpr) -> ScannedBranches
scanBranches = go { constructors: [], leaves: [], catchAll: Nothing }
  where
  -- A catch-all ends the scan. Preserve first-occurrence order of constructor
  -- groups and source order within each group; shallow leaves follow the groups.
  go acc remaining = case Array.uncons remaining of
    Nothing -> acc
    Just { head: branch, tail } -> case branch.pattern of
      ChainWildcard -> acc { catchAll = Just { name: Nothing, body: branch.body } }
      ChainVariable name -> acc { catchAll = Just { name: Just name, body: branch.body } }
      ChainLeaf pattern -> go acc { leaves = Array.snoc acc.leaves (FsMatchCase pattern Nothing branch.body) } tail
      ChainConstructor name inner -> go
        acc { constructors = appendConstructor name { inner, body: branch.body } acc.constructors } tail

appendConstructor :: String -> ConstructorBranch -> Array ConstructorGroup -> Array ConstructorGroup
appendConstructor name branch groups = case Array.findIndex (\group -> group.name == name) groups of
  Just index -> fromMaybe groups (Array.modifyAt index
    (\group -> group { branches = Array.snoc group.branches branch }) groups)
  Nothing -> Array.snoc groups { name, branches: [ branch ] }

type MatchTarget = { depth :: Int, boxedValue :: FsExpr, scrutinee :: FsExpr }

compileLevel :: MatchTarget -> Array (Branch FsExpr) -> FsExpr -> FsExpr
compileLevel target branches fallback =
  let
    scanned = scanBranches branches
    fallbackBody = fromMaybe fallback (map _.body scanned.catchAll)
    constructors = Array.mapWithIndex (compileGroup target.depth fallbackBody) scanned.constructors
  in FsMatch target.scrutinee
    (constructors <> scanned.leaves <> [ compileCatchAll target fallback scanned.catchAll ])

compileCatchAll :: MatchTarget -> FsExpr -> Maybe CatchAll -> FsMatchCase
compileCatchAll target fallback = case _ of
  Nothing -> FsMatchCase FsPatWildcard Nothing fallback
  Just { name: Nothing, body } -> FsMatchCase FsPatWildcard Nothing body
  Just { name: Just name, body } ->
    if target.depth == 0 then
      FsMatchCase (FsPatIdent name) Nothing body
    else
      -- Inner constructor matches unbox their scrutinee. Bind the original
      -- boxed field when a source variable captures the whole value instead.
      FsMatchCase FsPatWildcard Nothing (Boxed.letIn [ Boxed.LocalValue name target.boxedValue ] body)

compileGroup :: Int -> FsExpr -> Int -> ConstructorGroup -> FsMatchCase
compileGroup depth fallback index group = case Array.head group.branches of
  Just { inner: Nothing, body } -> FsMatchCase (FsPatCtor group.name []) Nothing body
  Just { inner: Just ChainWildcard, body } -> FsMatchCase (FsPatCtor group.name [ FsPatWildcard ]) Nothing body
  Just { inner: Just (ChainVariable name), body } -> FsMatchCase (FsPatCtor group.name [ FsPatIdent name ]) Nothing body
  _ ->
    let
      variable = "usd_case_" <> show depth <> "_" <> show index
      target = { depth: depth + 1, boxedValue: FsIdent variable, scrutinee: Boxed.unbox (FsIdent variable) }
      branches = map (\branch -> { pattern: fromMaybe ChainWildcard branch.inner, body: branch.body }) group.branches
    in FsMatchCase (FsPatCtor group.name [ FsPatIdent variable ]) Nothing (compileLevel target branches fallback)
