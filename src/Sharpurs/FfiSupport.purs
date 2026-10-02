-- | Pure native-source adapters: recognize declarations, determine their call
-- | shapes and render boxed wrappers. File precedence and stubs live in Ffi.
module Sharpurs.FfiSupport
  ( appendFfiWrappers
  , appendCsFfiWrappers
  ) where

import Sharpurs.Names as Names

-- JavaScript recognizes native declarations; PureScript supplies target naming.
foreign import appendFfiWrappersImpl :: (String -> String) -> String -> Array String -> String -> String
foreign import appendCsFfiWrappersImpl :: (String -> String) -> (String -> String) -> String -> Array String -> String -> String

appendFfiWrappers :: String -> Array String -> String -> String
appendFfiWrappers owner = appendFfiWrappersImpl (Names.inModule owner) (Names.ffiModule owner)

appendCsFfiWrappers :: String -> Array String -> String -> String
appendCsFfiWrappers owner = appendCsFfiWrappersImpl (Names.inModule owner) Names.nativeMember owner
