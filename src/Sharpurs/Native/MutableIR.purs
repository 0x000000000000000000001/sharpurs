-- | Low-level mutable IR for destination-passing. This is the layer where an
-- | ADT node can be read, updated and returned without reconstructing it.
module Sharpurs.Native.MutableIR
  ( Location(..)
  , MutableStmt(..)
  , MutableExpr(..)
  , renderLocation
  , renderExpr
  , renderStmt
  ) where

import Prelude

import Data.Array as Array
import Data.String as String

data Location
  = Local String
  | Field Location Int

data MutableStmt
  = Assign Location MutableExpr

data MutableExpr
  = MInt Int
  | MBool Boolean
  | MString String
  | MRead Location
  | MNew String (Array MutableExpr)
  | MCall String (Array MutableExpr)
  | MIf MutableExpr MutableExpr MutableExpr
  | MLet String MutableExpr MutableExpr
  | MSeq (Array MutableStmt) MutableExpr

renderLocation :: Location -> String
renderLocation = case _ of
  Local name -> name
  Field location index -> "(" <> renderLocation location <> ").Field" <> show index

renderExpr :: MutableExpr -> String
renderExpr = case _ of
  MInt value -> show value
  MBool value -> if value then "true" else "false"
  MString value -> show value
  MRead location -> renderLocation location
  MNew name arguments ->
    name <> if Array.null arguments then ""
      else " " <> String.joinWith " " (map (\argument -> "(" <> renderExpr argument <> ")") arguments)
  MCall name arguments ->
    name <> if Array.null arguments then ""
      else " " <> String.joinWith " " (map (\argument -> "(" <> renderExpr argument <> ")") arguments)
  MIf condition yes no ->
    "(if " <> renderExpr condition <> " then " <> renderExpr yes <> " else " <> renderExpr no <> ")"
  MLet name value body ->
    "(let " <> name <> " = " <> renderExpr value <> " in " <> renderExpr body <> ")"
  MSeq statements value ->
    if Array.null statements then renderExpr value
    else "(" <> String.joinWith "; " (map renderStmt statements) <> "; " <> renderExpr value <> ")"

renderStmt :: MutableStmt -> String
renderStmt (Assign location value) =
  renderLocation location <> " <- " <> renderExpr value
