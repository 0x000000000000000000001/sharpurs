module DirectCall where

import Prelude ((<))
import DirectRemote (remote)

foreign import track :: Int -> Int -> Int
foreign import explode :: Int -> Int

seed :: Int
seed = 17

ordinal :: Int -> Int -> Int -> Int -> Int
ordinal a b c d = if a < b then c else d

guarded :: Boolean -> Int -> Boolean -> Int -> Int
guarded first x second y = if first then x else if second then y else seed

captured :: Int -> Int -> Int
captured x y = ordinal x y seed 29

saturated :: Int -> Int
saturated x = ordinal x 10 20 30

ordered :: Int -> Int
ordered x = ordinal (track 1 x) (track 2 10) (track 3 20) (track 4 30)

partial1 :: Int -> Int -> Int -> Int
partial1 = ordinal 1

partial2 :: Int -> Int -> Int
partial2 = ordinal 1 2

partial3 :: Int -> Int
partial3 = ordinal 1 2 3

orderedPartial :: Int -> Int -> Int -> Int -> Int
orderedPartial x = ordinal (track 1 x)

asValue :: Int -> Int -> Int -> Int -> Int
asValue = ordinal

unary :: Int -> Int
unary x = x

generic :: forall a. a -> a -> a
generic x _ = x

stringPair :: String -> String -> String
stringPair x _ = x

numberPair :: Number -> Number -> Number
numberPair x _ = x

higher :: (Int -> Int) -> Int -> Int
higher fn x = fn x

shadowed :: (Int -> Int -> Int -> Int -> Int) -> Int
shadowed ordinal = ordinal 4 3 2 1

localShadow :: Int -> Int
localShadow x =
  let ordinal a _ _ _ = a
  in ordinal x 2 3 4

returned :: Boolean -> Int -> Int -> Int
returned flag value =
  let saved = track 90 value
  in \other -> if flag then saved else other

imported :: Int -> Int -> Int
imported x y = remote x y

failBody :: Int -> Int -> Int
failBody x _ = explode x

bodyCall :: Int -> Int
bodyCall x = failBody x 0

argumentCall :: Int -> Int
argumentCall x = ordinal (explode x) (track 2 10) (track 3 20) (track 4 30)

data Color = Warm | Cool
data Tree = Tip | Fork Color Tree Int Tree

arrange :: Color -> Tree -> Int -> Tree -> Tree
arrange color left value right = Fork color left value right

return :: Int -> Int -> Int
return x _ = x

return_direct :: Int
return_direct = 7
