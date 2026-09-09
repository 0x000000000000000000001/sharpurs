module ThunkExternal where

import Partial.Unsafe (unsafePartial)

foreign import track :: Int -> Int

partialSeed :: Int -> Int
partialSeed value = unsafePartial case value of
  0 -> 7
