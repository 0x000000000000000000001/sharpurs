module ConstructorTypeApp where

import Prelude
import ConstructorNative as Native
import ConstructorImported as Imported

-- Ordinary polymorphic data, including a recursive payload.
data Box a = Box a
data Maybe a = Nothing | Just a
data Tuple a b = Tuple a b
data List a = Nil | Cons a (List a)

foreign import track :: forall a. Int -> a -> a
foreign import explode :: forall a. Int -> a

boxInt :: Int -> Box Int
boxInt value = Box value

importedBox :: Int -> Imported.Envelope Int
importedBox value = Imported.Envelope value

pair :: Int -> String -> Tuple Int String
pair left right = Tuple left right

polyPair :: forall a b. a -> b -> Tuple a b
polyPair left right = Tuple left right

explicitPair :: Int -> String -> Tuple Int String
explicitPair left right = Tuple @Int @String left right

partialPair :: Int -> String -> Tuple Int String
partialPair left = Tuple left

justInt :: Int -> Maybe Int
justInt value = Just value

nothingInt :: Maybe Int
nothingInt = Nothing

list :: Int -> List Int
list value = Cons value (Cons (value + 1) Nil)

prepend :: forall a. a -> List a -> List a
prepend value tail = Cons value tail

ordinary :: forall @a. a -> a -> a
ordinary first _ = first

ordinaryCall :: Int -> Int -> Int
ordinaryCall first second = ordinary @Int first second

ordered :: Int -> Int -> Tuple Int Int
ordered left right = Tuple (track 1 left) (track 2 right)

throwFirst :: Int -> Tuple Int Int
throwFirst value = Tuple (explode 1) (track 2 value)

throwSecond :: Int -> Tuple Int Int
throwSecond value = Tuple (track 1 value) (explode 2)

capturedArgument :: Int -> Int -> Tuple Int Int
capturedArgument first =
  let saved = Tuple (track 1 first)
  in \second -> saved (track 2 second)

nativePair :: Int -> Native.Tree -> Tuple Native.Tree Native.Tree
nativePair value tail = Tuple (Native.Node value tail) tail
