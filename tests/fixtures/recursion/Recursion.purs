module Recursion.Context where

foreign import decrement :: Int -> Int
foreign import increment :: Int -> Int

applyOne :: forall a b. (a -> b) -> a -> b
applyOne fn value = fn value

applyTwo :: forall a b c. (a -> b -> c) -> a -> b -> c
applyTwo fn first second = fn first second

topSaturated :: Int -> Int -> Int
topSaturated remaining value = case remaining of
  0 -> value
  _ -> topSaturated (decrement remaining) (increment value)

topPartial :: Int -> Int -> Int
topPartial remaining value = case remaining of
  0 -> value
  _ -> applyOne (topPartial (decrement remaining)) (increment value)

topValue :: Int -> Int -> Int
topValue remaining value = case remaining of
  0 -> value
  _ -> applyTwo topValue (decrement remaining) (increment value)

-- Only the first argument precedes the case. The recursive call supplies two.
topOver :: Int -> Int -> Int
topOver remaining = case remaining of
  0 -> \value -> value
  _ -> \value -> topOver (decrement remaining) (increment value)

-- A mutual group with different worker arities exercises both directions.
topMutual :: Int -> Int -> Int
topMutual remaining value = case remaining of
  0 -> value
  _ -> topResume (decrement remaining) (increment value)

topResume :: Int -> Int -> Int
topResume remaining = topMutual remaining

localSaturated :: Int -> Int -> Int
localSaturated remaining value =
  let
    walk' count acc = case count of
      0 -> acc
      _ -> walk' (decrement count) (increment acc)
  in walk' remaining value

localPartial :: Int -> Int -> Int
localPartial remaining =
  let
    walk' count acc = case count of
      0 -> acc
      _ -> applyOne (walk' (decrement count)) (increment acc)
  in walk' remaining

localValue :: Int -> Int -> Int
localValue remaining value =
  let
    walk' count acc = case count of
      0 -> acc
      _ -> walk' (decrement count) (increment acc)
  in applyTwo walk' remaining value

localOver :: Int -> Int -> Int
localOver remaining value =
  let
    walk' count = case count of
      0 -> \acc -> acc
      _ -> \acc -> walk' (decrement count) (increment acc)
  in walk' remaining value

localMutual :: Int -> Int -> Int
localMutual remaining value =
  let
    walk' count acc = case count of
      0 -> acc
      _ -> resume (decrement count) (increment acc)
    resume count = walk' count
  in walk' remaining value

nested :: Int -> Int -> Int
nested remaining value =
  let
    outer count acc = case count of
      0 -> acc
      _ ->
        let
          inner steps result = case steps of
            0 -> outer (decrement count) result
            _ -> inner (decrement steps) (increment result)
        in inner 2 acc
  in outer remaining value

-- The lexical binder intentionally has the same name as a top-level worker.
shadowed :: Int -> Int -> Int
shadowed remaining value =
  let
    topSaturated count acc = case count of
      0 -> acc
      _ -> topSaturated (decrement count) (increment (increment acc))
  in topSaturated remaining value

data Link = Link (Int -> Link)

topRing :: Link
topRing = Link \_ -> topRing

recursiveValues :: Int
recursiveValues =
  let ring = Link \_ -> ring
  in case topRing, ring of
    Link top, Link local -> case top 0, local 0 of
      Link _, Link _ -> 42
