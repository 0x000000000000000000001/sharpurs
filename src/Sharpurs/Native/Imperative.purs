-- | SSA-style imperative IR and a lowering from `Native.IR`.
-- |
-- | Values are produced once, ADT nodes are locations whose fields can be
-- | loaded/stored, and matches become explicit blocks with branches and a join
-- | value. The runtime emitter currently consumes `Native.Expr` directly; this
-- | IR is the checked intermediate representation for destination plans and
-- | rotations.
module Sharpurs.Native.Imperative
  ( Value(..)
  , ImperativeType(..)
  , Instr(..)
  , Terminator(..)
  , Block
  , Program
  , emptyProgram
  , validate
  , lowerExpr
  , lowerBinding
  ) where

import Prelude

import Data.Array as Array
import Data.Map (Map)
import Data.Map as Map
import Data.Maybe (Maybe(..))
import Data.Set as Set
import Data.Tuple (Tuple(..))
import Sharpurs.Native.IR (NativeCase, NativeExpr(..), NativeParameter, NativePrimitive(..))
import Sharpurs.Native.Pattern (NativePattern(..))

newtype Value = Value Int

derive newtype instance Eq Value
derive newtype instance Ord Value
derive newtype instance Show Value

data ImperativeType
  = TInt
  | TBool
  | TString
  | TAdt String
  | TBoxed

derive instance Eq ImperativeType
derive instance Ord ImperativeType

data Instr
  = Argument Int Value
  | IntLit Int Value
  | BoolLit Boolean Value
  | StringLit String Value
  | Alloc String (Array Value) Value
  | Load Value Int Value
  | Store Value Int Value
  | Project Value Int Value
  | Call String (Array Value) Value
  | IsConstructor String Value Value
  | Primitive String (Array Value) Value
  | Copy Value Value
  | Cast String Value Value
  | Reuse String Value (Array Value) Value
  | Move Value Value

data Terminator
  = Return Value
  | Jump String
  | Branch Value String String
  | Fail String

type Block =
  { name :: String
  , instructions :: Array Instr
  , terminator :: Terminator
  }

type Program =
  { blocks :: Array Block
  , entry :: String
  }

emptyProgram :: Program
emptyProgram = { blocks: [], entry: "entry" }

-- | Structural validation: values are defined once, block targets exist, and
-- | the entry block exists.
validate :: Program -> Maybe Unit
validate program = do
  let names = map _.name program.blocks
  if Array.length names /= Set.size (Set.fromFoldable names) then Nothing else pure unit
  if Set.member program.entry (Set.fromFoldable names) then pure unit else Nothing
  let targets = Array.concatMap targetsOf program.blocks
  if Array.all (\name -> Set.member name (Set.fromFoldable names)) targets then pure unit else Nothing
  pure unit
  where
  targetsOf block = case block.terminator of
    Jump name -> [ name ]
    Branch _ yes no -> [ yes, no ]
    _ -> []

-- | Lower a closed expression. Free variables are rejected.
lowerExpr :: NativeExpr -> Maybe Program
lowerExpr = lowerBinding []

-- | Lower a function body. Parameters become explicit SSA arguments.
lowerBinding :: Array NativeParameter -> NativeExpr -> Maybe Program
lowerBinding parameters expression = do
  let
    initial = { nextValue: 0, nextBlock: 1, blocks: [], label: "entry", instructions: [] }
    seeded = Array.foldl
      (\state (Tuple index parameter) ->
        let allocated = freshValue state.builder
        in { values: Array.snoc state.values allocated.value
           , builder: allocated.builder
               { instructions = Array.snoc allocated.builder.instructions (Argument index allocated.value)
               }
           })
      { values: [], builder: initial }
      (Array.zip (Array.range 0 (Array.length parameters - 1)) parameters)
    env = Map.fromFoldable
      (Array.zipWith (\parameter value -> Tuple parameter.name value) parameters seeded.values)
  final <- lowerWithEnv env expression seeded.builder
  let finished = finish (Return final.value) final.builder
  pure { blocks: finished.blocks, entry: "entry" }

type Builder =
  { nextValue :: Int
  , nextBlock :: Int
  , blocks :: Array Block
  , label :: String
  , instructions :: Array Instr
  }

freshValue :: Builder -> { value :: Value, builder :: Builder }
freshValue builder =
  { value: Value builder.nextValue
  , builder: builder { nextValue = builder.nextValue + 1 }
  }

freshLabel :: Builder -> { label :: String, builder :: Builder }
freshLabel builder =
  { label: "block_" <> show builder.nextBlock
  , builder: builder { nextBlock = builder.nextBlock + 1 }
  }

start :: String -> Builder -> Builder
start label builder = builder { label = label, instructions = [] }

finish :: Terminator -> Builder -> Builder
finish terminator builder =
  builder
    { blocks = Array.snoc builder.blocks
        { name: builder.label
        , instructions: builder.instructions
        , terminator
        }
    , instructions = []
    }

lowerWithEnv :: Map String Value -> NativeExpr -> Builder -> Maybe { value :: Value, builder :: Builder }
lowerWithEnv env expression builder = case expression of
  NativeVar name _ -> case Map.lookup name env of
    Just value -> Just { value, builder }
    Nothing -> Nothing
  NativeIntLit value ->
    let next = freshValue builder
    in Just { value: next.value, builder: next.builder { instructions = Array.snoc next.builder.instructions (IntLit value next.value) } }
  NativeBooleanLit value ->
    let next = freshValue builder
    in Just { value: next.value, builder: next.builder { instructions = Array.snoc next.builder.instructions (BoolLit value next.value) } }
  NativeStringLit value ->
    let next = freshValue builder
    in Just { value: next.value, builder: next.builder { instructions = Array.snoc next.builder.instructions (StringLit value next.value) } }
  NativeLambda _ _ -> Nothing
  NativeApply function arguments -> do
    callee <- case function of
      NativeVar name _ -> Just name
      _ -> Nothing
    lowered <- lowerMany env arguments builder
    let next = freshValue lowered.builder
    Just
      { value: next.value
      , builder: next.builder { instructions = Array.snoc next.builder.instructions (Call callee lowered.values next.value) }
      }
  NativeLet name _ value body -> do
    lowered <- lowerWithEnv env value builder
    lowerWithEnv (Map.insert name lowered.value env) body lowered.builder
  NativeMatch targets cases fallback -> lowerMatch env targets cases fallback builder
  NativeConstructor name arguments -> do
    lowered <- lowerMany env arguments builder
    let next = freshValue lowered.builder
    Just
      { value: next.value
      , builder: next.builder { instructions = Array.snoc next.builder.instructions (Alloc name lowered.values next.value) }
      }
  NativeReuse name target arguments -> do
    loweredTarget <- lowerWithEnv env target builder
    loweredArguments <- lowerMany env arguments loweredTarget.builder
    let next = freshValue loweredArguments.builder
    Just
      { value: next.value
      , builder: next.builder { instructions = Array.snoc next.builder.instructions (Reuse name loweredTarget.value loweredArguments.values next.value) }
      }
  NativeSetField target index value body -> do
    loweredTarget <- lowerWithEnv env target builder
    loweredValue <- lowerWithEnv env value loweredTarget.builder
    let stored = loweredValue.builder { instructions = Array.snoc loweredValue.builder.instructions (Store loweredTarget.value index loweredValue.value) }
    lowerWithEnv env body stored
  NativeProject _ index value -> do
    lowered <- lowerWithEnv env value builder
    let next = freshValue lowered.builder
    Just
      { value: next.value
      , builder: next.builder { instructions = Array.snoc next.builder.instructions (Project lowered.value index next.value) }
      }
  NativePrimitive primitive arguments -> do
    lowered <- lowerMany env arguments builder
    let next = freshValue lowered.builder
    Just
      { value: next.value
      , builder: next.builder { instructions = Array.snoc next.builder.instructions (Primitive (primitiveName primitive) lowered.values next.value) }
      }
  NativeBox _ -> Nothing
  NativeCopy _ value -> do
    lowered <- lowerWithEnv env value builder
    let next = freshValue lowered.builder
    Just
      { value: next.value
      , builder: next.builder { instructions = Array.snoc next.builder.instructions (Copy lowered.value next.value) }
      }
  NativeCast constructor value -> do
    lowered <- lowerWithEnv env value builder
    let next = freshValue lowered.builder
    Just
      { value: next.value
      , builder: next.builder { instructions = Array.snoc next.builder.instructions (Cast constructor lowered.value next.value) }
      }

lowerMany :: Map String Value -> Array NativeExpr -> Builder -> Maybe { values :: Array Value, builder :: Builder }
lowerMany env expressions builder = go expressions builder []
  where
  go remaining current values = case Array.uncons remaining of
    Nothing -> Just { values, builder: current }
    Just { head, tail } -> do
      lowered <- lowerWithEnv env head current
      go tail lowered.builder (Array.snoc values lowered.value)

lowerMatch :: Map String Value -> Array NativeExpr -> Array NativeCase -> Maybe NativeExpr -> Builder -> Maybe { value :: Value, builder :: Builder }
lowerMatch env targets cases fallback builder = do
  loweredTargets <- lowerMany env targets builder
  let
    result = freshValue loweredTargets.builder
    join = freshLabel result.builder
  compiled <- compileCases env loweredTargets.values cases fallback result.value join.label (start "match_test" join.builder)
  pure { value: result.value, builder: start join.label compiled }
  where
  compileCases currentEnv targetValues remaining remainingFallback resultValue joinLabel current =
    case Array.uncons remaining of
      Nothing -> case remainingFallback of
        Nothing -> Just (finish (Fail "Failed native match") current)
        Just fallbackExpression -> do
          lowered <- lowerWithEnv currentEnv fallbackExpression current
          let
            moved = lowered.builder { instructions = Array.snoc lowered.builder.instructions (Move lowered.value resultValue) }
          Just (finish (Jump joinLabel) moved)
      Just { head: case', tail } -> do
        tested <- lowerPatterns currentEnv targetValues case'.patterns current
        let
          bodyLabel = freshLabel tested.builder
          nextLabel = freshLabel bodyLabel.builder
          branched = finish (Branch tested.condition bodyLabel.label nextLabel.label) tested.builder
        loweredBody <- lowerWithEnv (Map.union tested.bindings currentEnv) case'.body (start bodyLabel.label branched)
        let
          bodyMoved = loweredBody.builder { instructions = Array.snoc loweredBody.builder.instructions (Move loweredBody.value resultValue) }
          bodyFinished = finish (Jump joinLabel) bodyMoved
        compileCases currentEnv targetValues tail remainingFallback resultValue joinLabel (start nextLabel.label bodyFinished)

lowerPatterns :: Map String Value -> Array Value -> Array NativePattern -> Builder -> Maybe { condition :: Value, bindings :: Map String Value, builder :: Builder }
lowerPatterns env targets patterns builder = case go targets patterns builder [] Map.empty of
  Nothing -> Nothing
  Just lowered ->
    let combined = combineConditions lowered.conditions lowered.builder
    in Just { condition: combined.value, bindings: lowered.bindings, builder: combined.builder }
  where
  go :: Array Value -> Array NativePattern -> Builder -> Array Value -> Map String Value -> Maybe { conditions :: Array Value, bindings :: Map String Value, builder :: Builder }
  go remainingTargets remainingPatterns current conditions bindings =
    case Array.uncons remainingTargets, Array.uncons remainingPatterns of
      Nothing, _ -> Just { conditions, bindings, builder: current }
      _, Nothing -> Just { conditions, bindings, builder: current }
      Just { head: target, tail: restTargets }, Just { head: pattern, tail: restPatterns } -> do
        lowered <- lowerPattern target pattern current bindings
        go restTargets restPatterns lowered.builder (conditions <> lowered.conditions) lowered.bindings

  combineConditions :: Array Value -> Builder -> { value :: Value, builder :: Builder }
  combineConditions conditions current =
    case Array.uncons conditions of
      Nothing ->
        let next = freshValue current
        in { value: next.value, builder: next.builder { instructions = Array.snoc next.builder.instructions (BoolLit true next.value) } }
      Just { head, tail } ->
        Array.foldl
          (\state condition ->
            let next = freshValue state.builder
            in { value: next.value
               , builder: next.builder
                   { instructions = Array.snoc next.builder.instructions
                       (Primitive "and" [ state.value, condition ] next.value)
                   }
               })
          { value: head, builder: current }
          tail

lowerPattern :: Value -> NativePattern -> Builder -> Map String Value -> Maybe { conditions :: Array Value, bindings :: Map String Value, builder :: Builder }
lowerPattern target pattern builder bindings = case pattern of
  PatternWildcard -> Just { conditions: [], bindings, builder }
  PatternVariable name -> Just { conditions: [], bindings: Map.insert name target bindings, builder }
  PatternNamed name inner -> do
    lowered <- lowerPattern target inner builder (Map.insert name target bindings)
    pure lowered
  PatternInt value -> do
    condition <- literalCondition (IntLit value) builder
    pure { conditions: [ condition.value ], bindings, builder: condition.builder }
  PatternBoolean value -> do
    condition <- literalCondition (BoolLit value) builder
    pure { conditions: [ condition.value ], bindings, builder: condition.builder }
  PatternString value -> do
    condition <- literalCondition (StringLit value) builder
    pure { conditions: [ condition.value ], bindings, builder: condition.builder }
  PatternConstructor name fields -> do
    let
      condition = freshValue builder
      withCondition = condition.builder { instructions = Array.snoc condition.builder.instructions (IsConstructor name target condition.value) }
    fieldsLowered <- lowerFields target fields withCondition bindings
    pure
      { conditions: [ condition.value ]
      , bindings: fieldsLowered.bindings
      , builder: fieldsLowered.builder
      }
  where
  literalCondition :: (Value -> Instr) -> Builder -> Maybe { value :: Value, builder :: Builder }
  literalCondition instruction current =
    let
      next = freshValue current
      withLiteral = next.builder { instructions = Array.snoc next.builder.instructions (instruction next.value) }
    in compareValue next.value target withLiteral

  compareValue :: Value -> Value -> Builder -> Maybe { value :: Value, builder :: Builder }
  compareValue left right current =
    let next = freshValue current
    in pure { value: next.value, builder: next.builder { instructions = Array.snoc next.builder.instructions (Primitive "eq" [ left, right ] next.value) } }

  lowerFields :: Value -> Array NativePattern -> Builder -> Map String Value -> Maybe { bindings :: Map String Value, builder :: Builder }
  lowerFields root fields current bindings' = go 0 fields current bindings'
    where
    go index remaining currentBindings bindingsAcc = case Array.uncons remaining of
      Nothing -> Just { bindings: bindingsAcc, builder: currentBindings }
      Just { head: field, tail } -> do
        let
          loaded = freshValue currentBindings
          loadedBuilder = loaded.builder { instructions = Array.snoc loaded.builder.instructions (Project root index loaded.value) }
        lowered <- lowerPattern loaded.value field loadedBuilder bindingsAcc
        go (index + 1) tail lowered.builder lowered.bindings

primitiveName :: NativePrimitive -> String
primitiveName = case _ of
  PrimitiveAdd -> "add"
  PrimitiveSubtract -> "subtract"
  PrimitiveModulo -> "modulo"
  PrimitiveLessThan -> "less_than"
  PrimitiveGreaterThan -> "greater_than"
  PrimitiveEqual -> "equal"
