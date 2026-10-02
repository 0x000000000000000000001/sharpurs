module Main where

import Project.Alpha as Alpha

foreign import data Effect :: Type -> Type
foreign import data Unit :: Type
foreign import verify :: Int -> Effect Unit

main :: Effect Unit
main = verify Alpha.value
