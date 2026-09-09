module AdtMultiConsumer where

import AdtMulti (Color(..), Tree, assemble, choose, insert, readNonEmpty, returned)

foreign import trackColor :: Int -> Color -> Color
foreign import trackTree :: Int -> Tree -> Tree
foreign import trackInt :: Int -> Int -> Int

ordered :: Tree -> Tree
ordered tree = assemble (trackColor 1 Onyx) (trackTree 2 tree) (trackInt 3 7) (trackTree 4 tree)

orderedPartial :: Tree -> Int -> Tree -> Tree
orderedPartial tree = assemble (trackColor 1 Onyx) (trackTree 2 tree)

firstArgumentFailure :: Tree -> Tree -> Tree
firstArgumentFailure bad tree =
  choose true tree (trackInt 1 (readNonEmpty bad 0)) (trackTree 2 tree)

lastArgumentFailure :: Tree -> Tree -> Int
lastArgumentFailure tree bad = readNonEmpty (trackTree 1 tree) (trackInt 2 (readNonEmpty bad 0))

viaReturned :: Tree -> Int -> Int
viaReturned = returned

captured :: Int -> Tree -> Tree
captured = insert
