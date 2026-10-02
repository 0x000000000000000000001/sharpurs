module Naming.Valéurs where

foreign import answer' :: Int -> Int
foreign import café :: Int -> Int
foreign import match :: Int -> Int

data System = System Int
data Box' = Box' Int
data Étage = Étage Int

answer :: Int
answer = answer' 40

unicode :: Int
unicode = café 41

reserved :: Int
reserved = match 40

unwrap :: System -> Int
unwrap (System value) = value

unwrap' :: Box' -> Int
unwrap' (Box' value) = value

étage :: Étage -> Int
étage (Étage value) = value

constructor :: Int
constructor = unwrap (System 43)

primedConstructor :: Int
primedConstructor = unwrap' (Box' 44)

unicodeConstructor :: Int
unicodeConstructor = étage (Étage 45)

recursive' :: Int -> Int
recursive' value = case value of
  0 -> answer' 40
  _ -> recursive' 0

direct' :: Int -> Int -> Int
direct' value _ = answer' value
