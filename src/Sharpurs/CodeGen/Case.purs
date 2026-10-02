-- | Ordinary case translation and the nested-match path for deep unary patterns.
-- | Expression translation is supplied by the caller, keeping this module
-- | independent of expression dispatch and its recursive environments.
module Sharpurs.CodeGen.Case (Translators, translate) where

import Prelude

import Data.Array as Array
import Data.Maybe (Maybe(..), fromMaybe)
import Data.Traversable (traverse)
import PureScript.Backend.Optimizer.CoreFn (Ann, Expr, CaseAlternative(..), CaseGuard(..), Guard(..))
import Sharpurs.CodeGen.Boxed as Boxed
import Sharpurs.CodeGen.Pattern (Chain(..))
import Sharpurs.CodeGen.Pattern as Pattern
import Sharpurs.FsAst (FsExpr(..), FsMatchCase(..), FsPattern(..))

-- Scrutinees and boxed body templates consume obj. Guards and structural
-- branches use the enclosing expression boundary. Keep source bodies until
-- their placement is known, so each call is constructed with its own adapters.
type Translators =
  { expression :: Expr Ann -> FsExpr
  , object :: Expr Ann -> FsExpr
  }

translate :: Pattern.Env -> Translators -> Array (Expr Ann) -> Array (CaseAlternative Ann) -> FsExpr
translate env emit expressions alternatives
  -- A case over an uninhabited type is unreachable and keeps the ordinary failure.
  | Array.null alternatives = Boxed.patternFailure
  | otherwise =
      let
        value = Boxed.matchValue (map emit.object expressions)
        plan = case expressions of
          [ _ ] -> nestedMatchPlan env alternatives
          _ -> Nothing
      in case plan of
        Just branches -> compileLevel emit { depth: 0, boxedValue: value, scrutinee: value } branches Nothing
        Nothing -> FsMatch value (Array.concatMap (translateAlternative env emit) alternatives)

translateAlternative :: Pattern.Env -> Translators -> CaseAlternative Ann -> Array FsMatchCase
translateAlternative env emit (CaseAlternative binders guards) =
  let
    pattern = case map (Pattern.translate env) binders of
      [] -> FsPatWildcard
      [ single ] -> single
      patterns -> FsPatTuple patterns
  in case guards of
    Unconditional body -> [ FsMatchCase pattern Nothing (Boxed.parenthesize (emit.object body)) ]
    Guarded branches -> map (\(Guard guard body) ->
      FsMatchCase pattern (Just (emit.expression guard)) (Boxed.parenthesize (emit.object body))) branches

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

type ConstructorBranch = { inner :: Maybe Chain, body :: Expr Ann }

type ConstructorGroup = { name :: String, branches :: Array ConstructorBranch }

-- A catch-all optionally binds the matched value. Constructor and leaf patterns
-- cannot enter this slot, so compiling it always yields exactly one match case.
type CatchAll = { name :: Maybe String, body :: Expr Ann }

type ScannedBranches =
  { constructors :: Array ConstructorGroup
  , leaves :: Array { pattern :: FsPattern, body :: Expr Ann }
  , catchAll :: Maybe CatchAll
  }

scanBranches :: Array (Branch (Expr Ann)) -> ScannedBranches
scanBranches = go { constructors: [], leaves: [], catchAll: Nothing }
  where
  -- A catch-all ends the scan. Preserve first-occurrence order of constructor
  -- groups and source order within each group; shallow leaves follow the groups.
  go acc remaining = case Array.uncons remaining of
    Nothing -> acc
    Just { head: branch, tail } -> case branch.pattern of
      ChainWildcard -> acc { catchAll = Just { name: Nothing, body: branch.body } }
      ChainVariable name -> acc { catchAll = Just { name: Just name, body: branch.body } }
      ChainLeaf pattern -> go acc { leaves = Array.snoc acc.leaves { pattern, body: branch.body } } tail
      ChainConstructor name inner -> go
        acc { constructors = appendConstructor name { inner, body: branch.body } acc.constructors } tail

appendConstructor :: String -> ConstructorBranch -> Array ConstructorGroup -> Array ConstructorGroup
appendConstructor name branch groups = case Array.findIndex (\group -> group.name == name) groups of
  Just index -> fromMaybe groups (Array.modifyAt index
    (\group -> group { branches = Array.snoc group.branches branch }) groups)
  Nothing -> Array.snoc groups { name, branches: [ branch ] }

type MatchTarget = { depth :: Int, boxedValue :: FsExpr, scrutinee :: FsExpr }

compileLevel :: Translators -> MatchTarget -> Array (Branch (Expr Ann)) -> Maybe (Expr Ann) -> FsExpr
compileLevel emit target branches fallback =
  let
    scanned = scanBranches branches
    fallbackBody = case scanned.catchAll of
      Just branch -> Just branch.body
      Nothing -> fallback
    constructors = Array.mapWithIndex (compileGroup emit target.depth fallbackBody) scanned.constructors
    leaves = map (\branch -> FsMatchCase branch.pattern Nothing (emit.expression branch.body)) scanned.leaves
  in FsMatch target.scrutinee
    (constructors <> leaves <> [ compileCatchAll emit target fallback scanned.catchAll ])

compileCatchAll :: Translators -> MatchTarget -> Maybe (Expr Ann) -> Maybe CatchAll -> FsMatchCase
compileCatchAll emit target fallback = case _ of
  Nothing -> FsMatchCase FsPatWildcard Nothing (fromMaybe Boxed.patternFailure (map emit.expression fallback))
  Just { name: Nothing, body } -> FsMatchCase FsPatWildcard Nothing (emit.expression body)
  Just { name: Just name, body } ->
    if target.depth == 0 then
      FsMatchCase (FsPatIdent name) Nothing (emit.expression body)
    else
      -- Inner constructor matches unbox their scrutinee. Bind the original
      -- boxed field when a source variable captures the whole value instead.
      FsMatchCase FsPatWildcard Nothing (Boxed.letIn [ Boxed.LocalValue name target.boxedValue ] (emit.object body))

compileGroup :: Translators -> Int -> Maybe (Expr Ann) -> Int -> ConstructorGroup -> FsMatchCase
compileGroup emit depth fallback index group = case Array.head group.branches of
  Just { inner: Nothing, body } -> FsMatchCase (FsPatCtor group.name []) Nothing (emit.expression body)
  Just { inner: Just ChainWildcard, body } -> FsMatchCase (FsPatCtor group.name [ FsPatWildcard ]) Nothing (emit.expression body)
  Just { inner: Just (ChainVariable name), body } -> FsMatchCase (FsPatCtor group.name [ FsPatIdent name ]) Nothing (emit.expression body)
  _ ->
    let
      variable = "usd_case_" <> show depth <> "_" <> show index
      target = { depth: depth + 1, boxedValue: FsIdent variable, scrutinee: Boxed.unbox (FsIdent variable) }
      branches = map (\branch -> { pattern: fromMaybe ChainWildcard branch.inner, body: branch.body }) group.branches
    in FsMatchCase (FsPatCtor group.name [ FsPatIdent variable ]) Nothing (compileLevel emit target branches fallback)
