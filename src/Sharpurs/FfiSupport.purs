module Sharpurs.FfiSupport
  ( appendFfiWrappers
  , appendCsFfiWrappers
  , normalizeRecIndent
  ) where

foreign import appendFfiWrappersImpl :: String -> Array String -> String -> String
foreign import appendCsFfiWrappersImpl :: String -> Array String -> String -> String
foreign import normalizeRecIndentImpl :: String -> String

appendFfiWrappers :: String -> Array String -> String -> String
appendFfiWrappers = appendFfiWrappersImpl

appendCsFfiWrappers :: String -> Array String -> String -> String
appendCsFfiWrappers = appendCsFfiWrappersImpl

-- | Lay out the local `let rec` groups of a module (see FfiSupport.js).
normalizeRecIndent :: String -> String
normalizeRecIndent = normalizeRecIndentImpl
