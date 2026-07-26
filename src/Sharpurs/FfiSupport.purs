module Sharpurs.FfiSupport
  ( appendFfiWrappers
  ) where

foreign import appendFfiWrappersImpl :: String -> Array String -> String -> String

appendFfiWrappers :: String -> Array String -> String -> String
appendFfiWrappers = appendFfiWrappersImpl
