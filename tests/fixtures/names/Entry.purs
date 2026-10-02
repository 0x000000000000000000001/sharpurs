module Naming.Entrée where

import Naming.Csharp as C
import Naming.Absent as M
import Naming.Native as N
import Naming.Records as R
import Naming.Types (Effect, Unit)
import Naming.Valéurs as V

type Results =
  { fsharp :: Int, unicode :: Int, reserved :: Int
  , constructor :: Int, primedConstructor :: Int, unicodeConstructor :: Int
  , recursive :: Int, direct :: Int, native :: Int
  , csBase :: Int, csMatch :: Int, csUnicode :: Int
  , original :: R.Fields, changed :: R.Fields
  , originalAccess :: Array Int, changedAccess :: Array Int
  , originalPattern :: Array Int, changedPattern :: Array Int
  , missing :: Int -> Int
  }

foreign import verify :: Results -> Effect Unit

main :: Effect Unit
main = verify
  { fsharp: V.answer, unicode: V.unicode, reserved: V.reserved
  , constructor: V.constructor, primedConstructor: V.primedConstructor
  , unicodeConstructor: V.unicodeConstructor
  , recursive: V.recursive' 1, direct: V.direct' 40 9, native: N.value
  , csBase: C.base 38, csMatch: C.match 37, csUnicode: C.café 36
  , original: R.original, changed: R.changed
  , originalAccess: R.accessors R.original, changedAccess: R.accessors R.changed
  , originalPattern: R.patterns R.original, changedPattern: R.patterns R.changed
  , missing: M.partial'
  }
