module Sharpurs.FfiSupport
  ( appendFfiWrappers
  , appendCsFfiWrappers
  ) where

foreign import appendFfiWrappersImpl :: String -> Array String -> String -> String
foreign import appendCsFfiWrappersImpl :: String -> Array String -> String -> String

appendFfiWrappers :: String -> Array String -> String -> String
appendFfiWrappers = appendFfiWrappersImpl

appendCsFfiWrappers :: String -> Array String -> String -> String
appendCsFfiWrappers = appendCsFfiWrappersImpl
