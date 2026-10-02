module Naming.Native where

data Tree' = Empty' | Link' Int Tree'

read' :: Tree' -> Int
read' tree = case tree of
  Empty' -> 0
  Link' item Empty' -> item
  Link' _ rest -> read' rest

value :: Int
value = read' (Link' 0 (Link' 46 Empty'))
