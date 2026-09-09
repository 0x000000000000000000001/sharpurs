module TypedThunks where

import Prelude
import Partial.Unsafe (unsafePartial)
import ThunkExternal as External

newtype Susp a = Susp (Unit -> a)

delay :: forall @a. (Unit -> a) -> Susp a
delay = Susp

force :: forall a. Susp a -> a
force (Susp callback) = callback unit

chain :: Int -> Susp Int -> Susp Int
chain 0 acc = acc
chain n acc = chain (n - 1) (delay \_ -> force acc + 1)

run :: Int -> Int -> Int
run depth seed = force (chain depth (delay \_ -> seed))

runLiteral :: Int -> Int
runLiteral depth = force (chain depth (delay \_ -> 7))

runTwo :: Int -> Int -> Int -> Int
runTwo depth left right =
  force (chain depth (delay \_ -> left)) + force (chain depth (delay \_ -> right))

chainBy :: Int -> Int -> Susp Int -> Susp Int
chainBy _ 0 acc = acc
chainBy step n acc = chainBy step (n - 1) (delay \_ -> force acc + step)

runBy :: Int -> Int -> Int -> Int
runBy step depth seed = force (chainBy step depth (delay \_ -> seed))

escaped :: Int -> Int -> Susp Int
escaped depth seed = chain depth (delay \_ -> seed)

unknown :: Int -> Susp Int -> Int
unknown depth seed = force (chain depth seed)

unknownCallback :: Int -> (Unit -> Int) -> Int
unknownCallback depth callback = force (chain depth (delay callback))

partialChain :: Susp Int -> Susp Int
partialChain = chain 3

visibleDelay :: Int -> Int
visibleDelay depth = force (chain depth (delay @Int \_ -> 4))

partialSeed :: Int -> Int
partialSeed value = unsafePartial case value of
  0 -> 7

partialRun :: Int -> Int -> Int
partialRun depth seed = force (chain depth (delay \_ -> partialSeed seed))

importedRun :: Int -> Int -> Int
importedRun depth seed = force (chain depth (delay \_ -> External.partialSeed seed))

opaqueRun :: Int -> Int -> Int
opaqueRun depth seed = force (chain depth (delay \_ -> External.track seed))

numberChain :: Int -> Susp Number -> Susp Number
numberChain 0 acc = acc
numberChain n acc = numberChain (n - 1) (delay \_ -> force acc + 0.5)

numberRun :: Int -> Number -> Number
numberRun depth seed = force (numberChain depth (delay \_ -> seed))
