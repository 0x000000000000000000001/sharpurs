-- | F# source templates for the generic object ABI.
-- | Inputs are already translated expressions and escaped target identifiers;
-- | source-AST inspection and native-path selection belong to CodeGen.
module Sharpurs.CodeGen.Boxed
  ( Binding
  , LocalBinding(..)
  , IntOperation(..)
  , reference
  , box
  , parenthesize
  , unbox
  , array
  , record
  , updateRecord
  , accessRecord
  , lambda
  , curry
  , etaExpand
  , nativeConstructor
  , intBinary
  , matchValue
  , patternFailure
  , directBinding
  , recursiveBindings
  , letIn
  ) where

import Prelude

import Data.Array as Array
import Data.String as String
import Sharpurs.FsAst (FsDecl(..), FsExpr(..))
import Sharpurs.Printer (printExprInline)
import Sharpurs.Printer.Layout as Layout

-- An empty argument list denotes a value binding.
type Binding = { name :: String, args :: Array String, body :: FsExpr }

data LocalBinding
  = LocalValue String FsExpr
  | LocalRecursive (Array Binding)

data IntOperation = Add | Subtract | LessThan | GreaterThan

reference :: String -> FsExpr
reference name = FsRawExpr ("(box " <> name <> ")")

box :: FsExpr -> FsExpr
box expr = FsRawExpr ("(box (" <> printExprInline expr <> "))")

parenthesize :: FsExpr -> FsExpr
parenthesize expr = FsRawExpr ("(" <> printExprInline expr <> ")")

unbox :: FsExpr -> FsExpr
unbox expr = FsRawExpr ("(unbox " <> printExprInline expr <> ")")

array :: Array FsExpr -> FsExpr
array items = FsRawExpr ("(box [|" <> String.joinWith "; " (map printExprInline items) <> "|])")

type RecordField = { key :: String, value :: FsExpr }

record :: Array RecordField -> FsExpr
record fields = FsRawExpr ("(box (" <> Array.foldr addField "objMap" fields <> "))")

updateRecord :: FsExpr -> Array RecordField -> FsExpr
updateRecord source fields = FsRawExpr
  ("(box (" <> Array.foldr addField ("(unbox<Map<string, obj>> " <> printExprInline source <> ")") fields <> "))")

addField :: RecordField -> String -> String
addField { key, value } rest =
  "(Map.add \"" <> key <> "\" (box (" <> printExprInline value <> ")) " <> rest <> ")"

accessRecord :: String -> FsExpr -> FsExpr
accessRecord key source = FsRawExpr
  ("(Map.find \"" <> key <> "\" (unbox<Map<string, obj>> (" <> printExprInline source <> ")))")

lambda :: String -> FsExpr -> FsExpr
lambda arg body = FsRawExpr ("(box (fun (" <> arg <> ": obj) -> " <> printExprInline body <> "))")

-- Constructor and recursive-function adapters box the outer closure; their
-- inner stages are native F# functions, also handled by sharpurs_apply.
curry :: Array String -> FsExpr -> FsExpr
curry args body = FsRawExpr
  ("(box (" <> Array.foldr (\arg rest -> "(fun (" <> arg <> ": obj) -> " <> rest <> ")") (printExprInline body) args <> "))")

etaExpand :: String -> Int -> Array FsExpr -> FsExpr
etaExpand target missing supplied =
  let
    names = map (\index -> "usd_eta_" <> show index) (Array.range 0 (missing - 1))
    arguments = supplied <> map FsIdent names
    invocation = "(" <> target <> " " <> String.joinWith " " (map (\arg -> "(" <> printExprInline arg <> ")") arguments) <> ")"
  in curry names (FsRawExpr invocation)

nativeConstructor :: String -> Array FsExpr -> FsExpr
nativeConstructor name args = FsRawExpr
  ("(box (" <> name <> "_adt_native" <> String.joinWith "" (map (\arg -> " (unbox (" <> printExprInline arg <> "))") args) <> "))")

intBinary :: IntOperation -> FsExpr -> FsExpr -> FsExpr
intBinary operation left right =
  let
    operator = case operation of
      Add -> " + "
      Subtract -> " - "
      LessThan -> " < "
      GreaterThan -> " > "
    operand expr = "(unbox<int> (box (" <> printExprInline expr <> ")))"
  in FsRawExpr ("(box (" <> operand left <> operator <> operand right <> "))")

matchValue :: Array FsExpr -> FsExpr
matchValue = case _ of
  [] -> FsLitString "MissingExpr"
  [ value ] -> unbox (parenthesize value)
  values -> FsRawExpr ("(" <> String.joinWith ", " (map (printExprInline <<< unbox <<< parenthesize) values) <> ")")

patternFailure :: FsExpr
patternFailure = FsRawExpr "(failwith \"Failed pattern match\")"

directBinding :: Binding -> FsDecl
directBinding { name, args, body } =
  let
    parameters = objectParameters args
    invocation = name <> "_direct " <> String.joinWith " " args
    prefix = String.joinWith "" (map (\arg -> "(box (fun (" <> arg <> ": obj) -> ") args)
    suffix = String.joinWith "" (map (const "))") args)
  in FsRaw
    ( "let " <> name <> "_direct " <> parameters <> " : obj = " <> printExprInline body
        -- Arguments are evaluated before entering the worker. Only failures of
        -- its body receive the boundary sharpurs_apply would add.
        <> "\n\nlet " <> name <> "_direct_apply " <> parameters <> " : obj =\n"
        <> "    try " <> invocation <> "\n"
        <> "    with ex -> raise (System.Reflection.TargetInvocationException(\"" <> name <> "_direct: \" + ex.Message, ex))\n"
        -- The public curried entry gets that boundary from sharpurs_apply.
        <> "\nlet " <> name <> " = " <> prefix <> "(" <> invocation <> ")" <> suffix
    )

recursiveBindings :: Array Binding -> FsDecl
recursiveBindings bindings = FsRaw (String.joinWith "" (Array.mapWithIndex render bindings))
  where
  render index { name, args, body } =
    let keyword = if index == 0 then "let rec " else " and "
    in if Array.null args then
      keyword <> name <> " : obj = (" <> printExprInline body <> ")\n"
    else
      let
        opens = String.joinWith " " (map (\arg -> "(fun (" <> arg <> ": obj) -> ") args)
        closes = String.joinWith "" (map (const ")") args)
        wrapper = name <> " = box " <> opens <> name <> "_tco " <> String.joinWith " " args <> closes
      in keyword <> name <> "_tco " <> objectParameters args <> " : obj = (" <> printExprInline body <> ")\nand " <> wrapper <> "\n"

letIn :: Array LocalBinding -> FsExpr -> FsExpr
letIn bindings body = FsRawExpr ("(" <> Array.foldMap printLocalBinding bindings <> printExprInline body <> ")")

printLocalBinding :: LocalBinding -> String
printLocalBinding = case _ of
  LocalValue name value -> "let " <> name <> " = " <> printExprInline value <> " in "
  LocalRecursive bindings ->
    -- Resolve continuation indentation only after the enclosing expression is
    -- in its final column. No curried alias is emitted for local recursion.
    Layout.recStart <> "\n" <> String.joinWith "" (Array.mapWithIndex render bindings)
      <> "\n" <> Layout.recIndent <> "in\n" <> Layout.recIndent <> Layout.recEnd
  where
  render index { name, args, body } =
    let
      keyword = if index == 0 then Layout.recIndent <> "let rec " else "\n" <> Layout.recIndent <> "and "
      target = if Array.null args then name else name <> "_tco " <> objectParameters args
    in keyword <> target <> " : obj = (" <> printExprInline body <> ") "

objectParameters :: Array String -> String
objectParameters = String.joinWith " " <<< map (\arg -> "(" <> arg <> ": obj)")
