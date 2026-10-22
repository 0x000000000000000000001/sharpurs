-- | Minimal F# renderer for the native IR. It is intentionally not yet
-- | connected to the project writer; unsupported expressions reject instead of
-- | silently changing the boxed path.
module Sharpurs.Native.Emit
  ( renderTypeDeclarations
  , renderConstructor
  , renderExpr
  , renderPattern
  , renderDefinition
  , renderWrapper
  ) where

import Prelude

import Data.Array as Array
import Data.Foldable (foldr)
import Data.Map as Map
import Data.Maybe (Maybe(..))
import Data.String as String
import Data.Traversable (traverse)
import Data.Tuple (Tuple(..))
import PureScript.Backend.Optimizer.CoreFn (Ident(..), Qualified(..))
import Sharpurs.Native.IR (NativeCase, NativeExpr(..), NativeParameter, NativePrimitive(..))
import Sharpurs.Native.Pattern (NativePattern(..))
import Sharpurs.Native.Types (Constructor, Layout, NativeType(..), nativeTypeName)
import Sharpurs.Names as Names

renderTypeDeclarations :: Layout -> String
renderTypeDeclarations layout =
  String.joinWith "\n" (Array.mapWithIndex renderDeclaration (Map.toUnfoldable layout.types))
  where
  renderDeclaration index (Tuple name constructors) =
    (if index == 0 then "type " else "and ") <> name <> " =\n"
      <> String.joinWith "\n" (map renderConstructor constructors)

  renderConstructor constructor =
    "    | " <> constructor.name
      <> if Array.null constructor.fields then ""
         else " of " <> String.joinWith " * " (map nativeTypeName constructor.fields)

renderConstructor :: String -> Constructor -> String
renderConstructor moduleName constructor =
  let
    public = Names.inModule moduleName (case constructor.sourceName of Qualified _ (Ident name) -> name)
    native = Names.adtNative public
    parameters = Array.mapWithIndex
      (\index field -> "(sharpurs_ctor_arg_" <> show index <> ": " <> nativeTypeName field <> ")")
      constructor.fields
    arguments = Array.mapWithIndex (\index _ -> "sharpurs_ctor_arg_" <> show index) constructor.fields
    invocation = constructor.name
      <> if Array.null arguments then "" else "(" <> String.joinWith ", " arguments <> ")"
    definition = if Array.null constructor.fields
      then "let " <> native <> " : " <> constructor.typeName <> " = " <> constructor.name
      else "let " <> native <> " " <> String.joinWith " " parameters <> " : " <> constructor.typeName <> " = " <> invocation
  in definition <> "\n" <> renderWrapper public constructor.fields native

renderExpr :: NativeExpr -> Maybe String
renderExpr = case _ of
  NativeVar name _ -> Just name
  NativeIntLit value -> Just (show value)
  NativeBooleanLit value -> Just (if value then "true" else "false")
  NativeStringLit value -> Just (show value)
  NativeLambda parameters body -> do
    bodyCode <- renderExpr body
    let params = map (\(param :: NativeParameter) -> "(" <> param.name <> ": " <> nativeTypeName param.type <> ")") parameters
    pure ("(fun " <> String.joinWith " " params <> " -> " <> bodyCode <> ")")
  NativeApply function arguments -> do
    functionCode <- renderExpr function
    argumentCodes <- traverse renderExpr arguments
    pure ("(" <> functionCode <> String.joinWith "" (map (\argument -> " " <> argument) argumentCodes) <> ")")
  NativeLet name type' value body -> do
    valueCode <- renderExpr value
    bodyCode <- renderExpr body
    pure ("(let " <> name <> ": " <> nativeTypeName type' <> " = " <> valueCode <> " in " <> bodyCode <> ")")
  NativeMatch targets cases fallback -> do
    targetCodes <- traverse renderExpr targets
    caseCodes <- traverse renderCase cases
    fallbackCode <- case fallback of
      Nothing -> pure "failwith \"Failed native match\""
      Just expression' -> renderExpr expression'
    let scrutinee = case targetCodes of
          [ one ] -> one
          many -> "(" <> String.joinWith ", " many <> ")"
    pure ("(match " <> scrutinee <> " with | " <> String.joinWith " | " caseCodes
      <> " | _ -> " <> fallbackCode <> ")")
  NativeConstructor name arguments -> do
    argumentCodes <- traverse renderExpr arguments
    pure (name <> if Array.null argumentCodes then "" else "(" <> String.joinWith ", " argumentCodes <> ")")
  NativeReuse _ _ _ -> Nothing
  NativeProject _ _ _ -> Nothing
  NativePrimitive primitive arguments -> do
    argumentCodes <- traverse renderExpr arguments
    case argumentCodes of
      [ left, right ] -> pure ("(" <> left <> " " <> primitiveOperator primitive <> " " <> right <> ")")
      _ -> Nothing
  NativeBox expression -> do
    code <- renderExpr expression
    pure ("(box (" <> code <> "))")
  where
  renderCase { patterns, body } = do
    bodyCode <- renderExpr body
    let patternCodes = map renderPattern patterns
    let pattern = case patternCodes of
          [ one ] -> one
          many -> "(" <> String.joinWith ", " many <> ")"
    pure (pattern <> " -> " <> bodyCode)

renderPattern :: NativePattern -> String
renderPattern = case _ of
  PatternWildcard -> "_"
  PatternVariable name -> name
  PatternNamed name inner -> "(" <> name <> " as " <> renderPattern inner <> ")"
  PatternInt value -> show value
  PatternBoolean value -> if value then "true" else "false"
  PatternString value -> show value
  PatternConstructor name patterns ->
    name <> if Array.null patterns then "" else "(" <> String.joinWith ", " (map renderPattern patterns) <> ")"

renderDefinition :: Boolean -> String -> Array NativeParameter -> NativeExpr -> Maybe String
renderDefinition recursive name parameters body = do
  bodyCode <- renderExpr body
  let params = map (\(param :: NativeParameter) -> "(" <> param.name <> ": " <> nativeTypeName param.type <> ")") parameters
  pure ((if recursive then "let rec " else "let ") <> name <> " " <> String.joinWith " " params <> " = " <> bodyCode)

renderWrapper :: String -> Array NativeType -> String -> String
renderWrapper publicName argumentTypes nativeName =
  let
    args = Array.mapWithIndex (\index _ -> "sharpurs_native_arg_" <> show index) argumentTypes
    invocation = nativeName <> String.joinWith "" (Array.zipWith
      (\arg ty -> " (unbox<" <> nativeTypeName ty <> "> " <> arg <> ")")
      args
      argumentTypes)
    body = "box (" <> invocation <> ")"
  in let value = foldr (\arg inner -> "box (fun (" <> arg <> ": obj) -> " <> inner <> ")") body args
     in "let " <> publicName <> " : obj = " <> value

primitiveOperator :: NativePrimitive -> String
primitiveOperator = case _ of
  PrimitiveAdd -> "+"
  PrimitiveSubtract -> "-"
  PrimitiveModulo -> "%"
  PrimitiveLessThan -> "<"
  PrimitiveGreaterThan -> ">"
  PrimitiveEqual -> "="
