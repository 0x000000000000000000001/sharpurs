-- @inline export assemble never
-- @inline export readNonEmpty never
-- @inline export combine never
-- @inline export score never
module AdtMulti where

import Prelude
import AdtMultiExternal (offset, readZero)
import Partial.Unsafe (unsafePartial)

data Color = Ruby | Onyx
data Tree = Tip | Branch Color Tree Int Tree

assemble :: Color -> Tree -> Int -> Tree -> Tree
assemble color left value right = Branch color left value right

insert :: Int -> Tree -> Tree
insert value Tip = assemble Ruby Tip value Tip
insert value tree@(Branch color left old right)
  | value < old = assemble color (insert value left) old right
  | value > old = assemble color left old (insert value right)
  | otherwise = tree

choose :: Boolean -> Tree -> Int -> Tree -> Tree
choose flag left value right =
  if flag then assemble Onyx left (value + 1) right
  else assemble Ruby right (value - 1) left

shift :: Int -> Tree -> Tree
shift _ Tip = Tip
shift amount (Branch color left value right) =
  assemble color (shift amount left) (value + amount) (shift amount right)

depth :: Tree -> Int
depth Tip = 0
depth (Branch _ left _ right) =
  let a = depth left
      b = depth right
  in 1 + if a > b then a else b

readNonEmpty :: Tree -> Int -> Int
readNonEmpty tree increment = unsafePartial case tree of
  Branch _ _ value _ -> value + increment

bodyFailure :: Tree -> Int -> Int
bodyFailure tree increment = readNonEmpty tree increment + 1

argumentFailure :: Tree -> Tree -> Int
argumentFailure outer inner = readNonEmpty outer (readNonEmpty inner 0)

combine :: Tree -> Int -> Tree -> Int -> Int
combine first a second b = readNonEmpty first a + readNonEmpty second b

argumentOrder :: Tree -> Tree -> Int
argumentOrder first second = combine first (readNonEmpty first 0) second (readNonEmpty second 0)

unused :: Tree -> Int -> Tree
unused tree _ = tree

-- A let between source lambdas remains an observable application boundary.
returned :: Tree -> Int -> Int
returned tree =
  let value = readNonEmpty tree 0
  in \increment -> value + increment

partial :: Tree -> Int -> Tree -> Tree
partial = assemble Onyx

generic :: forall a. Tree -> a -> a
generic _ value = value

booleanOnly :: Boolean -> Int -> Int
booleanOnly flag value = if flag then value else 0

shortAnd :: Boolean -> Tree -> Boolean
shortAnd flag tree = if flag then readNonEmpty tree 0 > 0 else false

shortOr :: Boolean -> Tree -> Boolean
shortOr flag tree = if flag then true else readNonEmpty tree 0 > 0

nested :: Tree -> Int -> Int
nested (Branch Onyx (Branch Ruby _ value _) _ _) increment = value + increment
nested _ increment = increment

score :: Tree -> Int -> Int
score Tip increment = increment
score (Branch _ _ value _) increment = value + increment

shortAndSafe :: Boolean -> Tree -> Boolean
shortAndSafe flag tree = if flag then score tree 0 > 0 else false

shortOrSafe :: Boolean -> Tree -> Boolean
shortOrSafe flag tree = if flag then true else score tree 0 > 0

argumentNative :: Tree -> Tree -> Int
argumentNative first second = score second (score first 0)

readNonEmptyInline :: Tree -> Int -> Int
readNonEmptyInline tree increment = unsafePartial case tree of
  Branch _ _ value _ -> value + increment

bodyFailureInline :: Tree -> Int -> Int
bodyFailureInline tree increment = readNonEmptyInline tree increment + 1

importedFailure :: Tree -> Int -> Int
importedFailure Tip increment = readZero increment
importedFailure (Branch _ _ value _) increment = readZero (value + increment)

importedTotal :: Tree -> Int -> Int
importedTotal Tip increment = offset increment
importedTotal (Branch _ _ value _) increment = offset (value + increment)

throughImported :: Tree -> Int -> Int
throughImported tree increment = importedFailure tree increment + 1
