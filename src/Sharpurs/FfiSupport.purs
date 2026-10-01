-- | Pure native-source adapters: recognize declarations, determine their call
-- | shapes and render boxed wrappers. File precedence and stubs live in Ffi.
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
