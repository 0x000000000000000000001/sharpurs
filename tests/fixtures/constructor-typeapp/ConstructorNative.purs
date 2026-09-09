module ConstructorNative where

import Prelude

data Tree = Leaf | Node Int Tree

depth :: Tree -> Int
depth Leaf = 0
depth (Node _ tail) = 1 + depth tail
