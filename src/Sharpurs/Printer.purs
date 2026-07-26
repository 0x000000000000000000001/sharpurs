module Sharpurs.Printer where

import Prelude

import Data.Array as Array
import Data.String as String
import Sharpurs.FsAst (FsDecl(..), FsExpr(..), FsModule(..), FsType(..), FsDUCase(..), FsMatchCase(..), FsPattern(..))

printModule :: FsModule -> String
printModule (FsModule name decls) =
  let
    header = "module " <> name <> "\n\n"
    body = String.joinWith "\n\n" (map printDecl decls)
  in
    header <> body <> "\n"

printDecl :: FsDecl -> String
printDecl = case _ of
  FsLet name args expr ->
    let
      argsStr = if Array.length args > 0 then " " <> String.joinWith " " args else ""
    in
      "let " <> name <> argsStr <> " =\n    " <> printExpr expr
  FsDU name cases ->
    "type " <> name <> " =\n" <> String.joinWith "\n" (map printDUCase cases)
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

printExpr :: FsExpr -> String
printExpr = case _ of
  FsLitString s -> "\"" <> s <> "\""
  FsLitBool b -> if b then "true" else "false"
  FsIdent id -> id
  FsApp fn args ->
    let
      -- Very basic print, for constructors like Person("John", true) vs normal calls fn arg1 arg2
      -- In F# constructors are usually called with tuples, normal functions with spaces.
      -- We'll assume tuple style for simplicity if it starts with uppercase, else space.
      isCtor = case fn of
        FsIdent id -> String.take 1 id == String.toUpper (String.take 1 id)
        _ -> false
    in
      if isCtor then
        printExpr fn <> "(" <> String.joinWith ", " (map printExpr args) <> ")"
      else
        printExpr fn <> " " <> String.joinWith " " (map (\a -> "(" <> printExpr a <> ")") args)
  FsMatch expr cases ->
    "match " <> printExpr expr <> " with\n    " <> String.joinWith "\n    " (map printMatchCase cases)

printMatchCase :: FsMatchCase -> String
printMatchCase (FsMatchCase pat expr) =
  "| " <> printPattern pat <> " -> " <> printExpr expr

printPattern :: FsPattern -> String
printPattern = case _ of
  FsPatCtor name args ->
    if Array.length args > 0 then
      name <> "(" <> String.joinWith ", " args <> ")"
    else
      name
  FsPatWildcard -> "_"
