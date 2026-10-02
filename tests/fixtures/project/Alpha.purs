module Project.Alpha where

import Project.Zebra as Zebra

foreign import identity :: Int -> Int

value :: Int
value = identity (Zebra.value 40)
