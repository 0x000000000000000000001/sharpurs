-- | F# templates for checked native bodies, definitions and object-ABI boundaries.
-- | These templates do not inspect source or optimized expressions.
module Sharpurs.AdtKernel.Emit
  ( Definition
  , Branch
  , constructorBinding
  , functionBinding
  , group
  , ctorApplication
  , localName
  , intLiteral
  , booleanLiteral
  , call
  , projectField
  , isTag
  , binary
  , letIn
  , branch
  , failure
  ) where

import Prelude

import Data.Array as Array
import Data.Foldable (foldr)
import Data.Maybe (Maybe(..))
import Data.String as String
import Data.Traversable (traverse)
import PureScript.Backend.Optimizer.Syntax (Level(..))
import Sharpurs.AdtKernel.Analysis (Operation(..), Parameter, Signature)
import Sharpurs.AdtLayout as Layout
import Sharpurs.FsAst (FsDecl(..), escapeString)
import Sharpurs.Names as Names

type Definition = { parameters :: Array Parameter, body :: String }
type Branch = { condition :: String, body :: String }

constructorBinding :: Layout.Layout -> Signature -> Definition -> Maybe FsDecl
constructorBinding layout sig lowered = do
  native <- definition layout sig lowered
  public <- wrapper layout sig
  pure (FsRaw ("let " <> native <> "\n" <> public))

functionBinding :: Layout.Layout -> Signature -> Boolean -> Definition -> Maybe FsDecl
functionBinding layout sig recursive lowered = do
  native <- definition layout sig lowered
  public <- wrapper layout sig
  guarded <- guardedCall layout sig
  recursiveBridge <- if recursive then bridge layout sig else Just ""
  pure (FsRaw ((if recursive then "let rec " else "let ") <> native <> "\n" <> guarded <> "\n" <> public <> recursiveBridge))

-- Whole-native groups publish their object wrappers only after all mutually
-- recursive definitions. Their native calls do not use the mixed-module guard.
group :: Layout.Layout -> Boolean -> Array { signature :: Signature, definition :: Definition } -> Maybe String
group layout recursive definitions = do
  natives <- traverse (\item -> definition layout item.signature item.definition) definitions
  wrappers <- traverse (wrapper layout <<< _.signature) definitions
  let nativeDefinitions = String.joinWith "\n" (Array.mapWithIndex
        (\i native -> (if i == 0 then (if recursive then "let rec " else "let ") else "and ") <> native)
        natives)
  pure (nativeDefinitions <> "\n" <> String.joinWith "\n" wrappers <> "\n\n")

definition :: Layout.Layout -> Signature -> Definition -> Maybe String
definition layout sig lowered = do
  result <- Layout.nativeType layout sig.result
  parameters <- traverse (printParameter layout) lowered.parameters
  pure (sig.nativeName <> " " <> String.joinWith " " parameters <> " : " <> result <> " = " <> lowered.body)

printParameter :: Layout.Layout -> Parameter -> Maybe String
printParameter layout p = do
  ty <- Layout.nativeType layout p.type
  pure ("(" <> localName p.level <> ": " <> ty <> ")")

localName :: Level -> String
localName (Level level) = "sharpurs_adt_local_" <> show level

wrapper :: Layout.Layout -> Signature -> Maybe String
wrapper layout sig = do
  types <- traverse (Layout.nativeType layout) sig.args
  let args = Array.mapWithIndex (\i ty -> { name: "sharpurs_adt_arg_" <> show i, type: ty }) types
  let invocation = sig.nativeName <> String.joinWith "" (map (\arg -> " (unbox<" <> arg.type <> "> " <> arg.name <> ")") args)
  pure ("let " <> sig.publicName <> " : obj = " <> foldr (\arg body -> "box (fun (" <> arg.name <> ": obj) -> " <> body <> ")") ("box (" <> invocation <> ")") args)

bridge :: Layout.Layout -> Signature -> Maybe String
bridge layout sig = do
  types <- traverse (Layout.nativeType layout) sig.args
  let args = Array.mapWithIndex (\i ty -> { name: "sharpurs_adt_arg_" <> show i, type: ty }) types
  let parameters = String.joinWith " " (map (\arg -> "(" <> arg.name <> ": obj)") args)
  let invocation = sig.nativeName <> String.joinWith "" (map (\arg -> " (unbox<" <> arg.type <> "> " <> arg.name <> ")") args)
  pure ("\nlet " <> Names.recursive sig.publicName <> " " <> parameters <> " : obj = box (" <> invocation <> ")")

-- Arguments evaluate before this guard. Public curried wrappers, constructors
-- and self-recursion invoke the unguarded definition instead.
guardedCall :: Layout.Layout -> Signature -> Maybe String
guardedCall layout sig = do
  let args = Array.mapWithIndex (\i ty -> { level: Level i, type: ty }) sig.args
  parameters <- traverse (printParameter layout) args
  result <- Layout.nativeType layout sig.result
  pure ("let " <> Names.guarded sig.nativeName <> " " <> String.joinWith " " parameters <> " : " <> result <> " =\n"
    <> "    try " <> sig.nativeName <> String.joinWith "" (map (\arg -> " " <> localName arg.level) args) <> "\n"
    <> "    with ex -> raise (System.Reflection.TargetInvocationException(ex))")

ctorApplication :: Layout.Ctor -> Array String -> String
ctorApplication ctor args = ctor.name <> if Array.null args then "" else "(" <> String.joinWith ", " args <> ")"

intLiteral :: Int -> String
intLiteral value = "(" <> show value <> ")"

booleanLiteral :: Boolean -> String
booleanLiteral value = if value then "true" else "false"

-- Lower selects the guarded or unguarded target. Arguments stay outside the
-- callee's exception boundary and keep their source evaluation order.
call :: { target :: String, arguments :: Array String } -> String
call { target, arguments } = "(" <> target <> String.joinWith "" (map (\value -> " (" <> value <> ")") arguments) <> ")"

-- Constructor identity, index and field type have already been validated.
projectField :: { constructor :: Layout.Ctor, value :: String, index :: Int } -> String
projectField { constructor, value, index } =
  let patterns = Array.mapWithIndex (\i _ -> if i == index then "sharpurs_adt_field" else "_") constructor.fields
  in "(match " <> value <> " with | " <> ctorApplication constructor patterns <> " -> sharpurs_adt_field | _ -> failwith \"Invalid ADT constructor\")"

isTag :: { constructor :: Layout.Ctor, value :: String } -> String
isTag { constructor, value } =
  "(match " <> value <> " with | " <> ctorApplication constructor (map (const "_") constructor.fields) <> " -> true | _ -> false)"

binary :: { operation :: Operation, left :: String, right :: String } -> String
binary { operation, left, right } = "(" <> left <> " " <> operator operation <> " " <> right <> ")"

operator :: Operation -> String
operator = case _ of
  BooleanAnd -> "&&"
  BooleanOr -> "||"
  IntAdd -> "+"
  IntSubtract -> "-"
  IntEqual -> "="
  IntNotEqual -> "<>"
  IntGreaterThan -> ">"
  IntGreaterThanOrEqual -> ">="
  IntLessThan -> "<"
  IntLessThanOrEqual -> "<="

letIn :: { level :: Level, nativeType :: String, value :: String, body :: String } -> String
letIn { level, nativeType, value, body } =
  "(let " <> localName level <> ": " <> nativeType <> " = " <> value <> " in " <> body <> ")"

branch :: { cases :: Array Branch, fallback :: String } -> String
branch { cases, fallback } = foldr
  (\item rest -> "(if " <> item.condition <> " then " <> item.body <> " else " <> rest <> ")") fallback cases

failure :: String -> String
failure message = "(failwith " <> escapeString message <> ")"
