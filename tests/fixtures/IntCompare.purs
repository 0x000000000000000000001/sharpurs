module IntCompare where

import Prelude
import Data.Ord (lessThan)

foreign import track :: Int -> Int -> Int

less :: Int -> Int -> Boolean
less x y = x < y

greater :: Int -> Int -> Boolean
greater x y = x > y

annotated :: Int -> Int -> Boolean
annotated x y = (lessThan :: Int -> Int -> Boolean) x y

partial :: Int -> Boolean
partial = lessThan 5

genericLess :: forall a. Ord a => a -> a -> Boolean
genericLess x y = x < y

stringLess :: String -> String -> Boolean
stringLess x y = x < y

numberLess :: Number -> Number -> Boolean
numberLess x y = x < y

ordered :: Int -> Int -> Boolean
ordered x y = track 1 x < track 2 y

newtype Reverse = Reverse Int

derive instance eqReverse :: Eq Reverse

instance ordReverse :: Ord Reverse where
  compare (Reverse x) (Reverse y) = compare y x

custom :: Int -> Int -> Boolean
custom x y = Reverse x < Reverse y
