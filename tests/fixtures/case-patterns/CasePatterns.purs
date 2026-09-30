module Case.Patterns where

foreign import track :: forall a. Int -> a -> a

data Node = End | A Node | B Node | Pair Node Node | Leaf Int

newtype Wrapped = Wrapped Node

tag :: Node -> Int
tag value = case value of
  End -> 0
  A _ -> 1
  B _ -> 2
  Pair _ _ -> 3
  Leaf number -> number

-- Interleaved constructor groups share prefixes. The binary constructor is a
-- shallow leaf, while nested failures must reach the enclosing wildcard.
deep :: Node -> Int
deep value = case track 1 value of
  A (A (A End)) -> 11
  B (A (A End)) -> 12
  A (A (B End)) -> 13
  A (A (Leaf 0)) -> 14
  Pair End (Leaf number) -> number
  _ -> 99

shallow :: Node -> Int
shallow value = case value of
  A (A End) -> 21
  _ -> 22

-- The leaf fallback captures a boxed field after matching its unboxed value.
bound :: Node -> Int
bound value = case value of
  A (A (A End)) -> 31
  A (A (A rest)) -> tag rest
  _ -> 32

-- A deep named pattern keeps the ordinary matcher and its whole-value binding.
named :: Node -> Int
named value = case value of
  whole@(A (A (A End))) -> tag whole
  _ -> 42

wrapped :: Wrapped -> Int
wrapped value = case value of
  Wrapped (A (A (A End))) -> 51
  Wrapped _ -> 52

guarded :: Boolean -> Node -> Int
guarded enabled value = case track 1 value of
  A (A (A End)) | track 2 enabled -> 61
  A (A (A End)) | track 3 true -> 62
  _ -> 63

multiple :: Node -> Node -> Int
multiple left right = case track 1 left, track 2 right of
  A (A (A End)), B (Leaf number) -> number
  _, _ -> 71
