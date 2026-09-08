module AdtConsumer where

import AdtPilot (Color(..), Tree(..), depth)

data ConsumerBox = ConsumerBox Tree Int

foreign import trackColor :: Int -> Color -> Color
foreign import trackTree :: Int -> Tree -> Tree
foreign import trackInt :: Int -> Int -> Int

applyValue :: forall a b. (a -> b) -> a -> b
applyValue f value = f value

saturated :: Color -> Tree -> Int -> Tree -> Tree
saturated color left value right = T color left value right

partial :: Tree -> Int -> Tree -> Tree
partial = T B

throughGeneric :: Int -> Tree
throughGeneric value = applyValue T R E value E

throughPartial :: Int -> Tree
throughPartial value = applyValue (T B E) value E

rootValue :: Tree -> Int
rootValue E = 0
rootValue (T _ _ value _) = value

leftChild :: Tree -> Tree
leftChild E = E
leftChild (T _ child _ _) = child

rootColor :: Tree -> Color
rootColor E = B
rootColor (T color _ _ _) = color

deepPattern :: Tree -> Int
deepPattern (T B (T R E 7 E) 11 (T B E value E)) = value
deepPattern _ = 0

namedChild :: Tree -> Tree
namedChild (T _ child@(T R _ _ _) _ _) = child
namedChild _ = E

shared :: Tree -> Tree
shared child = T B child 11 child

roundTrip :: Tree -> Int
roundTrip child = depth (T B child 11 child)

wrap :: Tree -> ConsumerBox
wrap child = ConsumerBox child 42

unwrap :: ConsumerBox -> Tree
unwrap (ConsumerBox child _) = child

boxedValue :: ConsumerBox -> Int
boxedValue (ConsumerBox _ value) = value

boxedPattern :: ConsumerBox -> Int
boxedPattern (ConsumerBox (T B _ 11 _) 42) = 7
boxedPattern _ = 0

orderedConstruction :: Tree -> Tree
orderedConstruction child = T (trackColor 1 B) (trackTree 2 child) (trackInt 3 11) (trackTree 4 child)

orderedPartial :: Tree -> Int -> Tree -> Tree
orderedPartial child = T (trackColor 1 B) (trackTree 2 child)
