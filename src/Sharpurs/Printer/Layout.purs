-- | Delayed layout for recursive groups embedded in an enclosing expression.
module Sharpurs.Printer.Layout
  ( recStart
  , recIndent
  , recEnd
  , normalizeRecIndent
  ) where

-- Markers and their decoder share a single definition in Layout.js.
foreign import recStart :: String
foreign import recIndent :: String
foreign import recEnd :: String
foreign import normalizeRecIndent :: String -> String
