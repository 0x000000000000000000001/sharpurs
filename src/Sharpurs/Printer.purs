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
import Sharpurs.FsAst (FsDecl(..), FsExpr(..), FsModule(..), FsType(..), FsDUCase(..), FsMatchCase(..), FsPattern(..), FsDataCtor(..), sanitizeName, escapeString, escapeChar)

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

-- Declaration expressions adapt direct calls to the boxed boundary. Fragments
-- embedded by CodeGen.Boxed already supply object arguments to _tco/_direct
-- workers. Keep the distinction explicit: merging these paths changes F#
-- inference and the calling convention of nested direct applications.
data DirectCallMode = ConvertArguments | PassArguments

printExpr :: FsExpr -> String
printExpr = printExprWith ConvertArguments

printExprInline :: FsExpr -> String
printExprInline = printExprWith PassArguments

printExprWith :: DirectCallMode -> FsExpr -> String
printExprWith mode = case _ of
  FsLitString s -> "(box " <> escapeString s <> ")"
  FsLitBool b -> if b then "(box true)" else "(box false)"
  FsLitInt value -> "(box " <> show value <> ")"
  FsLitNumber value -> "(box " <> show value <> ")"
  FsLitChar value -> "(box '" <> escapeChar value <> "')"
  FsIdent id -> id
  FsRawExpr source -> source
  FsApp fn args ->
    Array.foldl (\acc arg -> "(sharpurs_apply (box (" <> acc <> ")) (box (" <> render arg <> ")))") (render fn) args
  FsDirectApp name args -> case mode of
    ConvertArguments ->
      if Array.null args then "(box " <> name <> ")"
      else "(box (" <> name <> " " <> String.joinWith " " (map (\arg -> "(unbox (" <> render arg <> "))") args) <> "))"
    PassArguments ->
      if Array.null args then name
      else "(" <> name <> " " <> String.joinWith " " (map (\arg -> "(" <> render arg <> ")") args) <> ")"
  FsCtorApp name args ->
    if Array.length args > 0 then
      "(box (" <> name <> "(" <> String.joinWith ", " (map render args) <> ")))"
    else "(box " <> name <> ")"
  FsMatch expr cases ->
    "(match (" <> render expr <> ") with " <> String.joinWith " " (map (printMatchCaseWith mode) cases) <> ")"
  where
  render expr = printExprWith mode expr

printMatchCase :: FsMatchCase -> String
printMatchCase = printMatchCaseWith ConvertArguments

printMatchCaseWith :: DirectCallMode -> FsMatchCase -> String
printMatchCaseWith mode (FsMatchCase pat g expr) =
  "| " <> printPattern pat <> (case g of
      Just guardExpr -> " when (unbox " <> printExprWith mode guardExpr <> ")"
      Nothing -> "") <> " -> " <> printExprWith mode expr

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
    "(" <> String.joinWith " & " (map (\{ key, pattern } -> "HasProp \"" <> key <> "\" (" <> printNestedPattern pattern <> ")") properties) <> ")"
  FsPatLitBool value -> if value then "LitBool true ()" else "LitBool false ()"
  FsPatLitInt value -> "LitInt " <> show value <> " ()"
  FsPatLitNumber value -> "LitNumber " <> show value <> " ()"
  FsPatLitString value -> "LitString " <> escapeString value <> " ()"
  FsPatLitChar value -> "LitChar '" <> escapeChar value <> "' ()"
