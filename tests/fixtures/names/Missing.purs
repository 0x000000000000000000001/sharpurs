module Naming.Absent where

foreign import unavailable' :: Int -> Int -> Int

partial' :: Int -> Int
partial' = unavailable' 1
