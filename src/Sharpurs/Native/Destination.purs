-- | Destination planning for the native IR.
-- |
-- | A match on an owned constructor tree can often be answered by rewriting
-- | the input nodes instead of rebuilding the same constructors. The planner
-- | builds an explicit input/output node graph, finds the best injective
-- | mapping between both, and emits field writes on the reused nodes. Any
-- | node that cannot be proved reusable is allocated normally; any caller that
-- | cannot transfer ownership inserts an explicit deep copy first.
module Sharpurs.Native.Destination
  ( Plan
  , lowerReuse
  , lowerExpr
  , ownedVariables
  , planCase
  , rewriteDestinations
  ) where

import Prelude

import Control.Alternative (guard)
import Data.Array as Array
import Data.Map (Map)
import Data.Map as Map
import Data.Maybe (Maybe(..), fromMaybe, maybe)
import Data.Set (Set)
import Data.Set as Set
import Data.Traversable (traverse)
import Data.Tuple (Tuple(..))
import Sharpurs.Native.IR (NativeCase, NativeExpr(..), NativeParameter, NativePrimitive(..))
import Sharpurs.Native.MutableIR (Location(..), MutableExpr(..), MutableStmt(..))
import Sharpurs.Native.Pattern (NativePattern(..))
import Sharpurs.Native.Types (Constructor, Layout, NativeType(..))
import Sharpurs.Native.Types as NativeTypes

type Plan =
  { expression :: NativeExpr
  , mutatedRoots :: Array String
  }

-- | Explicit destination form used by the imperative IR tests and by the
-- | first reuse lowering. Field reads/writes are kept as locations.
lowerReuse :: String -> NativeExpr -> Array NativeExpr -> Maybe MutableExpr
lowerReuse _constructor target arguments = do
  location <- locationOf target
  target' <- lowerExpr target
  arguments' <- traverse lowerExpr arguments
  let
    statements = Array.zipWith
      (\index argument -> Assign (Field location index) argument)
      (Array.range 0 (Array.length arguments' - 1))
      arguments'
  pure (MSeq statements target')
  where
  locationOf = case _ of
    NativeVar name _ -> Just (Local name)
    _ -> Nothing

lowerExpr :: NativeExpr -> Maybe MutableExpr
lowerExpr = case _ of
  NativeVar name _ -> Just (MRead (Local name))
  NativeIntLit value -> Just (MInt value)
  NativeBooleanLit value -> Just (MBool value)
  NativeStringLit value -> Just (MString value)
  NativeConstructor name arguments -> MNew name <$> traverse lowerExpr arguments
  NativeApply function arguments -> do
    name <- case function of
      NativeVar functionName _ -> Just functionName
      _ -> Nothing
    MCall name <$> traverse lowerExpr arguments
  NativeReuse _constructor target arguments -> lowerReuse "" target arguments
  NativePrimitive primitive arguments -> do
    name <- primitiveName primitive
    MCall name <$> traverse lowerExpr arguments
  _ -> Nothing

primitiveName :: NativePrimitive -> Maybe String
primitiveName = case _ of
  PrimitiveAdd -> Just "sharpurs_add"
  PrimitiveSubtract -> Just "sharpurs_subtract"
  PrimitiveModulo -> Just "sharpurs_modulo"
  PrimitiveLessThan -> Just "sharpurs_less_than"
  PrimitiveGreaterThan -> Just "sharpurs_greater_than"
  PrimitiveEqual -> Just "sharpurs_equal"

-- | Variables whose reachable ADT graph is exclusively owned by the current
-- | private function. Every native parameter is owned by contract (callers
-- | transfer a value or insert a deep copy); constructor-pattern variables are
-- | owned when the matched target is owned.
ownedVariables :: Layout -> Array NativeParameter -> NativeExpr -> Set String
ownedVariables _layout parameters body = fixedPoint initial
  where
  initial = Set.fromFoldable
    (Array.mapMaybe (\parameter -> if isAdt parameter.type then Just parameter.name else Nothing) parameters)

  fixedPoint seen =
    let next = Set.union seen (Set.fromFoldable (collect seen body))
    in if Set.size next == Set.size seen then seen else fixedPoint next

  collect owned = case _ of
    NativeVar _ _ -> []
    NativeIntLit _ -> []
    NativeBooleanLit _ -> []
    NativeStringLit _ -> []
    NativeLambda _ body' -> collect owned body'
    NativeApply function arguments -> collect owned function <> Array.concatMap (collect owned) arguments
    NativeLet name _ value body' ->
      let
        isOwned = ownedExpression owned value
        owned' = if isOwned then Set.insert name owned else owned
      in (if isOwned then [ name ] else [])
          <> collect owned value
          <> collect owned' body'
    NativeMatch targets cases fallback ->
      Array.concatMap (collect owned) targets
        <> Array.concatMap (collectCase owned targets) cases
        <> case fallback of
            Nothing -> []
            Just expression -> collect owned expression
    NativeConstructor _ arguments -> Array.concatMap (collect owned) arguments
    NativeReuse _ target arguments -> collect owned target <> Array.concatMap (collect owned) arguments
    NativeSetField target _ value body' ->
      collect owned target <> collect owned value <> collect owned body'
    NativeProject _ _ value -> collect owned value
    NativePrimitive _ arguments -> Array.concatMap (collect owned) arguments
    NativeBox expression -> collect owned expression
    NativeCopy _ expression -> collect owned expression
    NativeCast _ expression -> collect owned expression

  collectCase owned targets case' =
    let
      targetOwned = Array.any (\target -> case target of
        NativeVar name _ -> Set.member name owned
        _ -> false) targets
      bindings =
        if targetOwned
          then Array.mapMaybe
            (\(Tuple name ty) -> if isAdt ty then Just name else Nothing)
            (Map.toUnfoldable case'.bindings :: Array (Tuple String NativeType))
          else []
    in bindings <> collect (Set.union owned (Set.fromFoldable bindings)) case'.body

  ownedExpression owned = case _ of
    NativeVar name _ -> Set.member name owned
    NativeConstructor _ arguments -> Array.all (ownedExpression owned) arguments
    NativeIntLit _ -> true
    NativeBooleanLit _ -> true
    NativeStringLit _ -> true
    NativeLambda _ _ -> true
    NativePrimitive _ _ -> true
    NativeCopy _ expression -> ownedExpression owned expression
    NativeCast _ expression -> ownedExpression owned expression
    NativeReuse _ _ _ -> true
    NativeSetField _ _ _ _ -> true
    _ -> false

  isAdt = case _ of
    NativeAdt _ _ -> true
    _ -> false

-- | Plan one case of a match. The result is `Nothing` when the branch has no
-- | constructor graph worth reusing or when a mutation would touch a value
-- | that is not owned.
planCase :: Layout -> Set String -> Array NativeExpr -> NativeCase -> Maybe Plan
planCase layout owned targets case' = do
  outputTree <- outputOf case'.body
  outputNodes <- flattenOutput layout outputTree
  guard (Array.length outputNodes <= 6)
  rootOutput <- Array.head outputNodes
  guard (not (Array.null rootOutput.fields))
  collected <- collectInputs layout targets case'.patterns
  guard (Array.length collected.nodes <= 10)
  let assignment = bestAssignment collected.aliases outputNodes collected.nodes
  rootInput <- Map.lookup 0 assignment
  guard (Map.size assignment > 0)
  guard (Array.all (\input -> Set.member input.root owned) (mappedInputNodes assignment collected.nodes))
  let
    expression = buildPlan collected.aliases assignment outputNodes collected.nodes
  pure
    { expression
    , mutatedRoots: Array.nub (Array.mapMaybe (mutatedRoot collected.aliases assignment outputNodes collected.nodes)
        (Map.toUnfoldable assignment :: Array (Tuple Int Int)))
    }

type CollectedInput =
  { nodes :: Array InputNode
  , aliases :: Map String Int
  }

type InputNode =
  { index :: Int
  , path :: Array Int
  , root :: String
  , rootType :: NativeType
  , nodeType :: NativeType
  , constructor :: String
  , fields :: Array InputField
  }

data InputField
  = InputChild Int
  | InputVariable String
  | InputLiteral NativeExpr
  | InputWildcard

data InputTree
  = InputTree String NativeType (Array String) (Array InputTreeField)

data InputTreeField
  = TreeChild InputTree
  | TreeVariable String
  | TreeLiteral NativeExpr
  | TreeWildcard

data OutputTree
  = OutputTree String (Array OutputTreeField)

data OutputTreeField
  = OutputTreeChild OutputTree
  | OutputTreeExpression NativeExpr

type OutputNode =
  { index :: Int
  , constructor :: String
  , nodeType :: NativeType
  , fields :: Array OutputField
  }

data OutputField
  = OutputChild Int
  | OutputExpression NativeExpr

collectInputs :: Layout -> Array NativeExpr -> Array NativePattern -> Maybe CollectedInput
collectInputs layout targets patterns =
  go 0 (Array.zip targets patterns) { nodes: [], aliases: Map.empty }
  where
  go _ [] collected = Just collected
  go offset remaining collected = case Array.uncons remaining of
    Nothing -> Just collected
    Just { head: Tuple target pattern, tail: rest } -> do
      let numbered = case targetTree layout target pattern of
            Just tree -> numberInput offset [] targetName targetType tree
            Nothing -> { nodes: [], aliases: Map.empty }
      next <- go (offset + Array.length numbered.nodes) rest
        { nodes: collected.nodes <> numbered.nodes
        , aliases: Map.union collected.aliases numbered.aliases
        }
      pure next
      where
      targetName = case target of
        NativeVar name _ -> name
        _ -> ""
      targetType = case target of
        NativeVar _ ty -> ty
        _ -> NativeBoxed

targetTree :: Layout -> NativeExpr -> NativePattern -> Maybe InputTree
targetTree layout target pattern = case target of
  NativeVar _ ty -> case pattern of
    PatternConstructor _ _ -> patternTree layout ty pattern
    PatternNamed _ (PatternConstructor _ _) -> patternTree layout ty pattern
    _ -> Nothing
  _ -> Nothing

patternTree :: Layout -> NativeType -> NativePattern -> Maybe InputTree
patternTree layout nodeType = case _ of
  PatternConstructor constructor patterns -> do
    info <- findConstructor layout constructor
    guard (Array.length info.fields == Array.length patterns)
    fields <- traverse
      (\(Tuple fieldType pattern) -> treeField layout fieldType pattern)
      (Array.zip info.fields patterns)
    pure (InputTree constructor nodeType [] fields)
  PatternNamed name inner -> do
    InputTree constructor ty aliases fields <- patternTree layout nodeType inner
    pure (InputTree constructor ty (Array.cons name aliases) fields)
  _ -> Nothing

treeField :: Layout -> NativeType -> NativePattern -> Maybe InputTreeField
treeField layout ty pattern = case pattern of
  PatternWildcard -> Just TreeWildcard
  PatternVariable name -> Just (TreeVariable name)
  PatternNamed name inner -> do
    field <- treeField layout ty inner
    case field of
      TreeChild (InputTree constructor nodeType aliases fields) ->
        Just (TreeChild (InputTree constructor nodeType (Array.cons name aliases) fields))
      other -> Just other
  PatternInt value -> Just (TreeLiteral (NativeIntLit value))
  PatternBoolean value -> Just (TreeLiteral (NativeBooleanLit value))
  PatternString value -> Just (TreeLiteral (NativeStringLit value))
  PatternConstructor _ _ -> TreeChild <$> patternTree layout ty pattern
  _ -> Nothing

numberInput :: Int -> Array Int -> String -> NativeType -> InputTree -> CollectedInput
numberInput start path rootName rootType (InputTree constructor nodeType aliases treeFields) =
  let
    children = numberFields (start + 1) 0 treeFields
    node =
      { index: start
      , path
      , root: rootName
      , rootType
      , nodeType
      , constructor
      , fields: children.fields
      }
    allAliases = Map.union
      (Map.fromFoldable (map (\name -> Tuple name start) aliases))
      children.aliases
  in { nodes: Array.cons node children.nodes, aliases: allAliases }
  where
  numberFields offset index fields =
    if index >= Array.length fields then { fields: [], nodes: [], aliases: Map.empty }
    else case Array.index fields index of
      Nothing -> { fields: [], nodes: [], aliases: Map.empty }
      Just field -> case field of
        TreeChild child ->
          let
            numbered = numberInput offset (Array.snoc path index) rootName rootType child
            rest = numberFields (offset + Array.length numbered.nodes) (index + 1) fields
          in
            { fields: Array.cons (InputChild (offset + 0)) rest.fields
            , nodes: numbered.nodes <> rest.nodes
            , aliases: Map.union numbered.aliases rest.aliases
            }
        TreeVariable name ->
          let rest = numberFields offset (index + 1) fields
          in rest { fields = Array.cons (InputVariable name) rest.fields }
        TreeLiteral value ->
          let rest = numberFields offset (index + 1) fields
          in rest { fields = Array.cons (InputLiteral value) rest.fields }
        TreeWildcard ->
          let rest = numberFields offset (index + 1) fields
          in rest { fields = Array.cons InputWildcard rest.fields }

outputOf :: NativeExpr -> Maybe OutputTree
outputOf expression = case expression of
  NativeConstructor constructor arguments -> do
    fields <- traverse outputField arguments
    pure (OutputTree constructor fields)
  _ -> Nothing
  where
  outputField value = case value of
    NativeConstructor _ _ -> OutputTreeChild <$> outputOf value
    NativeVar _ _ -> Just (OutputTreeExpression value)
    NativeIntLit _ -> Just (OutputTreeExpression value)
    NativeBooleanLit _ -> Just (OutputTreeExpression value)
    NativeStringLit _ -> Just (OutputTreeExpression value)
    _ -> Nothing

flattenOutput :: Layout -> OutputTree -> Maybe (Array OutputNode)
flattenOutput layout = numberOutput layout 0

numberOutput :: Layout -> Int -> OutputTree -> Maybe (Array OutputNode)
numberOutput layout start (OutputTree constructor fields) = do
  info <- findConstructor layout constructor
  numbered <- numberFields (start + 1) 0 fields
  let
    node =
      { index: start
      , constructor
      , nodeType: NativeAdt info.typeName []
      , fields: numbered.fields
      }
  pure (Array.cons node numbered.nodes)
  where
  numberFields offset index fields =
    if index >= Array.length fields then Just { fields: [], nodes: [] }
    else case Array.index fields index of
      Nothing -> Just { fields: [], nodes: [] }
      Just field -> case field of
        OutputTreeExpression expression -> do
          rest <- numberFields offset (index + 1) fields
          pure rest { fields = Array.cons (OutputExpression expression) rest.fields }
        OutputTreeChild child -> do
          numbered <- numberOutput layout offset child
          rest <- numberFields (offset + Array.length numbered) (index + 1) fields
          pure
            { fields: Array.cons (OutputChild offset) rest.fields
            , nodes: numbered <> rest.nodes
            }

findConstructor :: Layout -> String -> Maybe Constructor
findConstructor layout name = Array.findMap
  (\(Tuple _ constructor) -> if constructor.name == name then Just constructor else Nothing)
  (Map.toUnfoldable layout.constructors :: Array (Tuple _ Constructor))

bestAssignment :: Map String Int -> Array OutputNode -> Array InputNode -> Map Int Int
bestAssignment aliases outputs inputs = (search 0 Set.empty Map.empty).assignment
  where
  search outputIndex used assignment =
    if outputIndex >= Array.length outputs then
      { score: scoreAssignment aliases outputs inputs assignment
      , assignment
      }
    else
      let
        skipped = search (outputIndex + 1) used assignment
        output = fromMaybe emptyOutput (Array.index outputs outputIndex)
      in Array.foldl
        (\best candidate ->
          if Set.member candidate.index used || candidate.constructor /= output.constructor then best
          else
            let
              result = search (outputIndex + 1)
                (Set.insert candidate.index used)
                (Map.insert outputIndex candidate.index assignment)
            in if result.score > best.score then result else best)
        skipped
        inputs

  emptyOutput = { index: 0, constructor: "", nodeType: NativeAdt "" [], fields: [] }

scoreAssignment :: Map String Int -> Array OutputNode -> Array InputNode -> Map Int Int -> Int
scoreAssignment aliases outputs inputs assignment =
  let
    rootBonus = if Map.member 0 assignment then 50 else 0
    exact = Array.foldl
      (\total (Tuple outputIndex inputIndex) ->
        case Array.index outputs outputIndex, Array.index inputs inputIndex of
          Just output, Just input -> total + fieldScore aliases assignment output input
          _, _ -> total)
      0
      (Map.toUnfoldable assignment :: Array (Tuple Int Int))
  in Map.size assignment * 100 + rootBonus + exact

fieldScore :: Map String Int -> Map Int Int -> OutputNode -> InputNode -> Int
fieldScore aliases assignment output input =
  Array.foldl
    (\total index -> case Array.index output.fields index, Array.index input.fields index of
      Just outputField, Just inputField ->
        total + if sameField aliases assignment outputField inputField then 10 else 0
      _, _ -> total)
    0
    (Array.range 0 (min (Array.length output.fields) (Array.length input.fields) - 1))

sameField :: Map String Int -> Map Int Int -> OutputField -> InputField -> Boolean
sameField aliases assignment output input = case output, input of
  OutputChild outputIndex, InputChild inputIndex ->
    Map.lookup outputIndex assignment == Just inputIndex
  OutputExpression (NativeVar name _), InputVariable inputName -> name == inputName
  OutputExpression (NativeVar name _), InputChild inputIndex ->
    Map.lookup name aliases == Just inputIndex
  OutputExpression (NativeIntLit left), InputLiteral (NativeIntLit right) -> left == right
  OutputExpression (NativeBooleanLit left), InputLiteral (NativeBooleanLit right) -> left == right
  OutputExpression (NativeStringLit left), InputLiteral (NativeStringLit right) -> left == right
  _, _ -> false

mappedInputNodes :: Map Int Int -> Array InputNode -> Array InputNode
mappedInputNodes assignment inputs =
  Array.mapMaybe (\index -> Array.index inputs index) (Array.fromFoldable (Map.values assignment) :: Array Int)

mutatedRoot :: Map String Int -> Map Int Int -> Array OutputNode -> Array InputNode -> Tuple Int Int -> Maybe String
mutatedRoot aliases assignment outputs inputs (Tuple outputIndex inputIndex) = do
  output <- Array.index outputs outputIndex
  input <- Array.index inputs inputIndex
  if hasOutputWrites aliases assignment output input then Just input.root else Nothing

hasOutputWrites :: Map String Int -> Map Int Int -> OutputNode -> InputNode -> Boolean
hasOutputWrites aliases assignment output input =
  Array.foldl
    (\changed fieldIndex -> changed || case Array.index output.fields fieldIndex, Array.index input.fields fieldIndex of
      Just outputField, Just inputField -> not (sameField aliases assignment outputField inputField)
      Just _, Nothing -> true
      _, _ -> false)
    false
    (Array.range 0 (max (Array.length output.fields) (Array.length input.fields) - 1))

buildPlan :: Map String Int -> Map Int Int -> Array OutputNode -> Array InputNode -> NativeExpr
buildPlan aliases assignment outputs inputs =
  let
    usedInputs = usedInputClosure assignment inputs
    inputLets rest = Array.foldr
      (\input inner ->
        if Array.length input.path == 0 || not (Set.member input.index usedInputs)
          then inner
          else case parentPathIndex input.root input.path inputs of
            Nothing -> inner
            Just parent ->
              let parentName = fromMaybe "" (Map.lookup parent.index inputAliases)
              in NativeLet (inputAlias input) input.nodeType
                  (NativeProject parent.constructor (fromMaybe 0 (Array.last input.path))
                    (NativeVar parentName parent.nodeType))
                  inner)
      rest
      inputs
    newLets rest = Array.foldr
      (\output inner ->
        if Map.member output.index assignment then inner
        else NativeLet (fromMaybe "" (Map.lookup output.index outputAliases)) output.nodeType
          (NativeConstructor output.constructor (map (outputFieldValue outputAliases) output.fields))
          inner)
      rest
      (Array.reverse outputs)
    rootValue = NativeVar (fromMaybe "" (Map.lookup 0 outputAliases))
      (case Array.head outputs of
        Just output -> output.nodeType
        Nothing -> NativeBoxed)
    writes = Array.foldr writeNode rootValue (Map.toUnfoldable assignment :: Array (Tuple Int Int))
  in inputLets (newLets writes)
  where
  _unused = aliases

  inputAliases = Map.fromFoldable
    (map (\input -> Tuple input.index (inputAlias input)) inputs)

  outputAliases = Map.fromFoldable
    (map (\output -> Tuple output.index (outputAlias output)) outputs)

  inputAlias input =
    if Array.length input.path == 0 then input.root
    else "sharpurs_destination_node_" <> show input.index

  outputAlias output =
    case Map.lookup output.index assignment of
      Just inputIndex -> fromMaybe "sharpurs_destination_missing" (Map.lookup inputIndex inputAliases)
      Nothing -> "sharpurs_destination_new_" <> show output.index

  outputFieldValue outputAliases' field = case field of
    OutputChild childIndex -> NativeVar (fromMaybe "" (Map.lookup childIndex outputAliases'))
      (case Array.index outputs childIndex of
        Just output -> output.nodeType
        Nothing -> NativeBoxed)
    OutputExpression expression -> expression

  writeNode (Tuple outputIndex inputIndex) rest =
    case Array.index outputs outputIndex, Array.index inputs inputIndex of
      Just output, Just input ->
        Array.foldr
          (\fieldIndex inner ->
            case Array.index output.fields fieldIndex of
              Nothing -> inner
              Just outputField ->
                case Array.index input.fields fieldIndex of
                  Just inputField
                    | sameField aliases assignment outputField inputField -> inner
                  _ -> NativeSetField (NativeCast input.constructor (NativeVar (inputAlias input) input.nodeType)) fieldIndex
                        (outputFieldValue outputAliases outputField) inner)
          rest
          (Array.range 0 (Array.length output.fields - 1))
      _, _ -> rest

usedInputClosure :: Map Int Int -> Array InputNode -> Set Int
usedInputClosure assignment inputs =
  Array.foldl
    (\set input ->
      if Set.member input.index set
        then case parentPathIndex input.root input.path inputs of
          Just parent -> Set.insert parent.index set
          Nothing -> set
        else set)
    (Set.fromFoldable (Map.values assignment))
    inputs

parentPathIndex :: String -> Array Int -> Array InputNode -> Maybe InputNode
parentPathIndex root path inputs =
  if Array.null path then Nothing
  else Array.find (\input -> input.root == root && input.path == Array.take (Array.length path - 1) path) inputs

-- | Rewrite every match whose constructor graph can be destination-passed.
-- | `owned` is computed by `ownedVariables` before this pass.
rewriteDestinations :: Layout -> Set String -> NativeExpr -> { expression :: NativeExpr, mutatedRoots :: Array String }
rewriteDestinations layout owned expression = go expression
  where
  go = case _ of
    NativeVar name ty -> { expression: NativeVar name ty, mutatedRoots: [] }
    NativeIntLit value -> { expression: NativeIntLit value, mutatedRoots: [] }
    NativeBooleanLit value -> { expression: NativeBooleanLit value, mutatedRoots: [] }
    NativeStringLit value -> { expression: NativeStringLit value, mutatedRoots: [] }
    NativeLambda parameters body ->
      let next = go body
      in { expression: NativeLambda parameters next.expression, mutatedRoots: next.mutatedRoots }
    NativeApply function arguments ->
      let
        function' = go function
        arguments' = map go arguments
      in
        { expression: NativeApply function'.expression (map _.expression arguments')
        , mutatedRoots: Array.nub (function'.mutatedRoots <> Array.concatMap _.mutatedRoots arguments')
        }
    NativeLet name ty value body ->
      let
        value' = go value
        body' = go body
      in
        { expression: NativeLet name ty value'.expression body'.expression
        , mutatedRoots: Array.nub (value'.mutatedRoots <> body'.mutatedRoots)
        }
    NativeMatch targets cases fallback ->
      let
        targets' = map go targets
        plans = map (plan targets) cases
        fallback' = map go fallback
      in
        { expression: NativeMatch (map _.expression targets') (map _.caseExpression plans) (map _.expression fallback')
        , mutatedRoots: Array.nub
            (Array.concatMap _.mutatedRoots targets'
              <> Array.concatMap _.mutatedRoots plans
              <> case fallback' of
                  Nothing -> []
                  Just value -> value.mutatedRoots)
        }
    NativeConstructor name arguments ->
      let arguments' = map go arguments
      in { expression: NativeConstructor name (map _.expression arguments')
         , mutatedRoots: Array.concatMap _.mutatedRoots arguments' }
    NativeReuse name target arguments ->
      let
        target' = go target
        arguments' = map go arguments
      in { expression: NativeReuse name target'.expression (map _.expression arguments')
         , mutatedRoots: Array.nub (target'.mutatedRoots <> Array.concatMap _.mutatedRoots arguments') }
    NativeSetField target index value body ->
      let
        target' = go target
        value' = go value
        body' = go body
      in { expression: NativeSetField target'.expression index value'.expression body'.expression
         , mutatedRoots: Array.nub (target'.mutatedRoots <> value'.mutatedRoots <> body'.mutatedRoots) }
    NativeProject name index value ->
      let value' = go value
      in { expression: NativeProject name index value'.expression, mutatedRoots: value'.mutatedRoots }
    NativePrimitive primitive arguments ->
      let arguments' = map go arguments
      in { expression: NativePrimitive primitive (map _.expression arguments')
         , mutatedRoots: Array.concatMap _.mutatedRoots arguments' }
    NativeBox expression' ->
      let next = go expression'
      in { expression: NativeBox next.expression, mutatedRoots: next.mutatedRoots }
    NativeCopy ty value ->
      let value' = go value
      in { expression: NativeCopy ty value'.expression, mutatedRoots: value'.mutatedRoots }
    NativeCast constructor value ->
      let value' = go value
      in { expression: NativeCast constructor value'.expression, mutatedRoots: value'.mutatedRoots }

  plan targets case' =
    case planCase layout owned targets case' of
      Just planned ->
        { caseExpression: case' { body = planned.expression }
        , mutatedRoots: planned.mutatedRoots
        }
      Nothing ->
        let body = go case'.body
        in { caseExpression: case' { body = body.expression }
           , mutatedRoots: body.mutatedRoots
           }

