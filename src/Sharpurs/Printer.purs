module Sharpurs.Printer
  ( printModule
  , printDecl
  , printDUCase
  , printType
  , printExpr
  , printExprInline
  , printMatchCase
  , printNestedPattern
  , printPattern
  ) where

import Prelude

import Data.Array as Array
import Data.String as String
import Data.Maybe (Maybe(..))
import Sharpurs.FsAst (FsDecl(..), FsExpr(..), FsModule(..), FsType(..), FsDUCase(..), FsMatchCase(..), FsPattern(..), FsDataCtor(..), CallConvention(..), sanitizeName, escapeString, escapeChar, patternString)

printModule :: FsModule -> String
printModule (FsModule _ decls) =
  String.joinWith "\n\n" (map printDecl decls)

printDecl :: FsDecl -> String
printDecl = case _ of
  FsLet name args body ->
    "let " <> name <> " " <> String.joinWith " " args <> " = " <> printExpr body
  FsLetRec bindings ->
    let strs = Array.mapWithIndex (\i b -> (if i == 0 then "let rec " else "and ") <> b.name <> " " <> String.joinWith " " b.args <> " = " <> printExpr b.expr) bindings
    in String.joinWith "\n" strs
  FsDeclData name ctors ->
    if Array.length ctors == 0 then
      "type " <> name <> " = | Dummy_" <> name
    else
      "type " <> name <> " =\n" <>
      String.joinWith "\n" (map (\(FsDataCtor ctorName count) -> "  | " <> sanitizeName ctorName <> (if count > 0 then " of " <> String.joinWith " * " (Array.replicate count "obj") else "")) ctors)
  FsDU name cases ->
    "type " <> name <> " =\n" <>
    String.joinWith "\n" (map (\(FsDUCase ctor fields) -> "  | " <> ctor <> (if Array.length fields > 0 then " of " <> String.joinWith " * " (map printType fields) else "")) cases)
  FsRaw str -> str

printDUCase :: FsDUCase -> String
printDUCase (FsDUCase name types) =
  let
    typesStr = if Array.length types > 0 then " of " <> String.joinWith " * " (map printType types) else ""
  in
    "    | " <> name <> typesStr

printType :: FsType -> String
printType = case _ of
  FsTString -> "string"
  FsTBool -> "bool"
  FsTInt -> "int"
  FsTCustom name -> name

-- Compatibility alias for consumers embedding an expression in a template.
-- Placement never changes argument or result adaptation.
printExprInline :: FsExpr -> String
printExprInline = printExpr

printExpr :: FsExpr -> String
printExpr = case _ of
  FsLitString s -> "(box " <> escapeString s <> ")"
  FsLitBool b -> if b then "(box true)" else "(box false)"
  FsLitInt value -> "(box " <> show value <> ")"
  FsLitNumber value -> "(box " <> show value <> ")"
  FsLitChar value -> "(box '" <> escapeChar value <> "')"
  FsIdent id -> id
  FsRawExpr source -> source
  FsApp fn args ->
    Array.foldl (\acc arg -> "(sharpurs_apply (box (" <> acc <> ")) (box (" <> printExpr arg <> ")))") (printExpr fn) args
  FsDirectApp convention name args -> case convention of
    UnboxArgumentsBoxResult ->
      if Array.null args then "(box " <> name <> ")"
      else "(box (" <> name <> " " <> String.joinWith " " (map (\arg -> "(unbox (" <> printExpr arg <> "))") args) <> "))"
    PassValues ->
      if Array.null args then name
      else "(" <> name <> " " <> String.joinWith " " (map (\arg -> "(" <> printExpr arg <> ")") args) <> ")"
  FsCtorApp name args ->
    if Array.length args > 0 then
      "(box (" <> name <> "(" <> String.joinWith ", " (map printExpr args) <> ")))"
    else "(box " <> name <> ")"
  FsMatch expr cases ->
    "(match (" <> printExpr expr <> ") with " <> String.joinWith " " (map printMatchCase cases) <> ")"

printMatchCase :: FsMatchCase -> String
printMatchCase (FsMatchCase pat g expr) =
  "| " <> printPattern pat <> (case g of
      Just guardExpr -> " when (unbox " <> printExpr guardExpr <> ")"
      Nothing -> "") <> " -> " <> printExpr expr

printNestedPattern :: FsPattern -> String
printNestedPattern = case _ of
  FsPatWildcard -> "_"
  FsPatIdent name -> name
  other -> "Unbox(" <> printPattern other <> ")"

printPattern :: FsPattern -> String
printPattern = case _ of
  FsPatCtor name args ->
    if Array.length args > 0 then
      name <> "(" <> String.joinWith ", " (map printNestedPattern args) <> ")"
    else
      name
  FsPatWildcard -> "_"
  FsPatIdent name -> name
  FsPatRaw s -> s
  FsPatTuple patterns -> "(" <> String.joinWith ", " (map printPattern patterns) <> ")"
  FsPatNamed name inner -> "(" <> printPattern inner <> " as " <> name <> ")"
  FsPatArray patterns -> "[| " <> String.joinWith "; " (map printNestedPattern patterns) <> " |]"
  FsPatRecord properties ->
    "(" <> String.joinWith " & " (map (\{ key, pattern } -> stringMatcher "HasProp" key <> " (" <> printNestedPattern pattern <> ")") properties) <> ")"
  FsPatLitBool value -> if value then "LitBool true ()" else "LitBool false ()"
  FsPatLitInt value -> "LitInt " <> show value <> " ()"
  FsPatLitNumber value -> "LitNumber " <> show value <> " ()"
  FsPatLitString value -> stringMatcher "LitString" value <> " ()"
  FsPatLitChar value -> "LitChar '" <> escapeChar value <> "' ()"

stringMatcher :: String -> String -> String
stringMatcher name value =
  let argument = patternString value
  in name <> (if argument.utf16 then "Utf16" else "") <> " " <> argument.code
