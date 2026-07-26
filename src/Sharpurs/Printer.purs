module Sharpurs.Printer where

import Prelude

import Data.Array as Array
import Data.String as String
import Sharpurs.FsAst (FsDecl(..), FsExpr(..), FsModule(..), FsType(..), FsDUCase(..), FsMatchCase(..), FsPattern(..), FsDataCtor(..), sanitizeName, escapeString)

fsPrelude :: String
fsPrelude = """
open System
open System.Collections.Generic

let objMap = Map.empty<string, obj>
let unbox<'a> (x: obj) : 'a = unbox x
let (|Unbox|) (x: obj) = unbox x

let undefined = Unchecked.defaultof<obj>
let Prim_undefined = undefined
let intMod a b = unbox<int> a % unbox<int> b
let semiringInt = 0

let sharpurs_apply (func: obj) (arg: obj) : obj =
    if isNull func then failwith "sharpurs_apply: func is null!"
    let method = func.GetType().GetMethods() |> Array.find (fun m -> m.Name = "Invoke" && m.GetParameters().Length = 1)
    method.Invoke(func, [| arg |])
"""

fsHeader :: String
fsHeader = "let (|LitBool|_|) (expected: bool) (value: obj) = if value :? bool && unbox value = expected then Some() else None\nlet (|LitInt|_|) (expected: int) (value: obj) = if value :? int && unbox value = expected then Some() else None\nlet (|LitNumber|_|) (expected: float) (value: obj) = if value :? float && unbox value = expected then Some() else None\nlet (|LitString|_|) (expected: string) (value: obj) = if value :? string && unbox value = expected then Some() else None\nlet (|LitChar|_|) (expected: char) (value: obj) = if value :? char && unbox value = expected then Some() else None\n\n"

printModule :: FsModule -> String
printModule (FsModule name decls) =
  let
    body = String.joinWith "\n\n" (map printDecl decls)
  in
    body <> "\n\nlet _ = (unbox<obj -> obj> Main_main) undefined\n"

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

printExpr :: FsExpr -> String
printExpr = case _ of
  FsLitString s -> "(box \"" <> escapeString s <> "\")"
  FsLitBool b -> if b then "(box true)" else "(box false)"
  FsIdent id -> id
  FsApp fn args ->
    Array.foldl (\acc arg -> "(sharpurs_apply (box (" <> acc <> ")) (box (" <> printExpr arg <> ")))") (printExpr fn) args
  FsCtorApp name args ->
    if Array.length args > 0 then
      "(box (" <> name <> "(" <> String.joinWith ", " (map printExpr args) <> ")))"
    else "(box " <> name <> ")"
  FsMatch expr cases ->
    "(match (" <> printExpr expr <> ") with " <> String.joinWith " " (map printMatchCase cases) <> ")"

printMatchCase :: FsMatchCase -> String
printMatchCase (FsMatchCase pat expr) =
  "| " <> printPattern pat <> " -> " <> printExpr expr

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
