module IntArithmetic where

import Prelude

foreign import track :: Int -> Int -> Int
foreign import explode :: Int -> Int
foreign import readCurrent :: Unit -> Int

addInt :: Int -> Int -> Int
addInt x y = x + y

subInt :: Int -> Int -> Int
subInt x y = x - y

visibleAdd :: Int -> Int -> Int
visibleAdd x y = add @Int x y

visibleSub :: Int -> Int -> Int
visibleSub x y = sub @Int x y

annotatedAdd :: Int -> Int -> Int
annotatedAdd x y = (add :: Int -> Int -> Int) x y

annotatedSub :: Int -> Int -> Int
annotatedSub x y = (sub :: Int -> Int -> Int) x y

partialAdd :: Int -> Int
partialAdd = add 5

partialSub :: Int -> Int
partialSub = sub 5

genericAdd :: forall a. Semiring a => a -> a -> a
genericAdd x y = x + y

genericSub :: forall a. Ring a => a -> a -> a
genericSub x y = x - y

numberAdd :: Number -> Number -> Number
numberAdd x y = x + y

numberSub :: Number -> Number -> Number
numberSub x y = x - y

orderedAdd :: Int -> Int -> Int
orderedAdd x y = track 1 x + track 2 y

orderedSub :: Int -> Int -> Int
orderedSub x y = track 1 x - track 2 y

firstFailureAdd :: Int -> Int
firstFailureAdd x = explode (track 1 x) + track 2 x

secondFailureAdd :: Int -> Int
secondFailureAdd x = track 1 x + explode (track 2 x)

firstFailureSub :: Int -> Int
firstFailureSub x = explode (track 1 x) - track 2 x

secondFailureSub :: Int -> Int
secondFailureSub x = track 1 x - explode (track 2 x)

capturedAdd :: Int -> Int -> Int
capturedAdd offset = \value -> offset + value

capturedSub :: Int -> Int -> Int
capturedSub offset = \value -> offset - value

delayedAdd :: Int -> Unit -> Int
delayedAdd offset = \_ -> offset + readCurrent unit

delayedSub :: Int -> Unit -> Int
delayedSub offset = \_ -> offset - readCurrent unit
