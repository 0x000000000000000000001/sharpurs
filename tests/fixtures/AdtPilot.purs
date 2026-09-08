module AdtPilot where

import Prelude hiding (max)

data Color = R | B
data Tree = E | T Color Tree Int Tree

depth :: Tree -> Int
depth E = 0
depth (T _ left _ right) = 1 + max (depth left) (depth right)

max :: Int -> Int -> Int
max x y = if x > y then x else y

empty :: Tree
empty = E

singleton :: Tree
singleton = T B E 2147483647 E

asymmetric :: Tree
asymmetric = T B (T R E (-2147483648) E) 0 (T B E 42 (T R E 42 E))

singletonWith :: Color -> Int -> Tree
singletonWith color value = T color E value E

rootValue :: Tree -> Int
rootValue E = 0
rootValue (T _ _ value _) = value

leftChild :: Tree -> Tree
leftChild E = E
leftChild (T _ child _ _) = child

isRed :: Color -> Boolean
isRed R = true
isRed B = false

rootColor :: Tree -> Color
rootColor E = B
rootColor (T color _ _ _) = color
