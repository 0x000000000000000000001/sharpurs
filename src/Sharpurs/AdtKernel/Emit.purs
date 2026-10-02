-- | Native definition headers and object-ABI boundaries. Bodies are already
-- | lowered; these templates do not inspect source or optimized expressions.
module Sharpurs.AdtKernel.Emit
  ( Definition
  , constructorBinding
  , functionBinding
  , group
  , ctorApplication
  , localName
  ) where

import Prelude

import Data.Array as Array
import Data.Foldable (foldr)
import Data.Maybe (Maybe(..))
import Data.String as String
import Data.Traversable (traverse)
import PureScript.Backend.Optimizer.Syntax (Level(..))
import Sharpurs.AdtKernel.Analysis (Parameter, Signature)
import Sharpurs.AdtLayout as Layout
import Sharpurs.FsAst (FsDecl(..))
import Sharpurs.Names as Names

type Definition = { parameters :: Array Parameter, body :: String }

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
  let call = sig.nativeName <> String.joinWith "" (map (\arg -> " (unbox<" <> arg.type <> "> " <> arg.name <> ")") args)
  pure ("let " <> sig.publicName <> " : obj = " <> foldr (\arg body -> "box (fun (" <> arg.name <> ": obj) -> " <> body <> ")") ("box (" <> call <> ")") args)

bridge :: Layout.Layout -> Signature -> Maybe String
bridge layout sig = do
  types <- traverse (Layout.nativeType layout) sig.args
  let args = Array.mapWithIndex (\i ty -> { name: "sharpurs_adt_arg_" <> show i, type: ty }) types
  let parameters = String.joinWith " " (map (\arg -> "(" <> arg.name <> ": obj)") args)
  let call = sig.nativeName <> String.joinWith "" (map (\arg -> " (unbox<" <> arg.type <> "> " <> arg.name <> ")") args)
  pure ("\nlet " <> Names.recursive sig.publicName <> " " <> parameters <> " : obj = box (" <> call <> ")")

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
