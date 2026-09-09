module AdtMultiExternal where

import Prelude
import Partial.Unsafe (unsafePartial)

readZero :: Int -> Int
readZero value = unsafePartial case value of
  0 -> 0

offset :: Int -> Int
offset value = value + 1
