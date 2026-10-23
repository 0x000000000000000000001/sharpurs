-- | Mutable ADT emitter backed by F# classes with mutable fields. Constructors
-- | are plain functions, patterns use generated active patterns, and `_reuse`
-- | functions perform in-place updates with a constructor-copy fallback.
module Sharpurs.Native.Mutable
  ( renderTypeDeclarations
  , renderConstructor
  , renderExpr
  , renderExprWithTags
  , renderPattern
  , renderDefinition
  , renderDefinitionWithTags
  ) where

import Prelude

import Data.Array as Array
import Data.Map as Map
import Data.Maybe (Maybe(..), fromMaybe)
import Data.String as String
import Data.Traversable (traverse)
import Data.Tuple (Tuple(..))
import PureScript.Backend.Optimizer.CoreFn (Ident(..), Qualified(..))
import Sharpurs.Native.Emit as Emit
import Sharpurs.Native.IR (NativeCase, NativeExpr(..), NativeParameter, NativePrimitive(..))
import Sharpurs.Native.Pattern (NativePattern(..))
import Sharpurs.Native.Types (Constructor, Layout, NativeType(..), nativeTypeName)
import Sharpurs.Names as Names

renderTypeDeclarations :: Layout -> String
renderTypeDeclarations layout =
  let
    declarations = Array.mapWithIndex renderTypeDeclaration (Map.toUnfoldable layout.types)
    constructors = Array.concatMap (\(Tuple typeName ctors) ->
      Array.mapWithIndex (renderConstructorParts typeName ctors) ctors) (Map.toUnfoldable layout.types)
  in String.joinWith "\n" declarations <> "\n\n" <> String.joinWith "\n" constructors
  where
  renderTypeDeclaration index (Tuple typeName constructors) =
    (if index == 0 then "type " else "and ") <> typeName <> "() =\n"
      <> "    member val Tag: int = 0 with get, set\n"
      <> String.joinWith "\n"
          (Array.mapWithIndex (\fieldIndex _ ->
            "    member val Field" <> show fieldIndex <> ": "
              <> nativeTypeName (fieldTypeAt constructors fieldIndex)
              <> " = Unchecked.defaultof<" <> nativeTypeName (fieldTypeAt constructors fieldIndex) <> "> with get, set")
            (Array.range 0 (maxArity constructors - 1)))

  maxArity constructors = Array.foldl (\maximum constructor -> max maximum (Array.length constructor.fields)) 0 constructors

  fieldTypeAt constructors index =
    case Array.findMap (\constructor -> Array.index constructor.fields index) constructors of
      Just fieldType -> fieldType
      Nothing -> NativeBoxed

  renderConstructorParts typeName constructors index constructor =
    let
      tag = index
      fields = constructor.fields
      params = Array.mapWithIndex (\fieldIndex fieldType -> "(sharpurs_mutable_arg_" <> show fieldIndex <> ": " <> nativeTypeName fieldType <> ")") fields
      args = Array.mapWithIndex (\fieldIndex _ -> "sharpurs_mutable_arg_" <> show fieldIndex) fields
      body =
        if Array.null fields then
          "let value = " <> typeName <> "() in value.Tag <- " <> show tag <> "; value"
        else
          "let value = " <> typeName <> "() in value.Tag <- " <> show tag <> "; "
            <> String.joinWith "; " (Array.zipWith (\fieldIndex argument -> "value.Field" <> show fieldIndex <> " <- " <> argument) (Array.range 0 (Array.length fields - 1)) args)
            <> "; value"
      definition =
        if Array.null fields then
          "let " <> constructor.name <> " : " <> typeName <> " = " <> body
        else
          "let " <> constructor.name <> " " <> String.joinWith " " params <> " : " <> typeName <> " = " <> body
      activePattern =
        "let (|" <> constructor.name <> "_match|_|) (value: " <> typeName <> ") =\n"
          <> "    if value.Tag = " <> show tag <> " then "
          <> (if Array.null fields then "Some ()"
              else "Some (" <> String.joinWith ", " (Array.mapWithIndex (\fieldIndex _ -> "value.Field" <> show fieldIndex) fields) <> ")")
          <> " else None"
      reuse =
        if Array.null fields then
          "let private " <> constructor.name <> "_reuse (value: " <> typeName <> ") : " <> typeName <> " = value"
        else
          "let private " <> constructor.name <> "_reuse (value: " <> typeName <> ") "
            <> String.joinWith " " params <> " : " <> typeName <> " =\n"
            <> "    if value.Tag = " <> show tag <> " then\n"
            <> "        " <> String.joinWith "\n        " (Array.zipWith (\fieldIndex argument -> "value.Field" <> show fieldIndex <> " <- " <> argument) (Array.range 0 (Array.length fields - 1)) args) <> "\n"
            <> "        value\n"
            <> "    else " <> constructor.name <> " " <> String.joinWith " " args
    in definition <> "\n" <> activePattern <> "\n" <> reuse

renderConstructor :: String -> Constructor -> String
renderConstructor moduleName constructor =
  let
    public = Names.inModule moduleName (case constructor.sourceName of Qualified _ (Ident name) -> name)
    native = Names.adtNative public
    parameters = Array.mapWithIndex
      (\index field -> "(sharpurs_ctor_arg_" <> show index <> ": " <> nativeTypeName field <> ")")
      constructor.fields
    arguments = Array.mapWithIndex (\index _ -> "sharpurs_ctor_arg_" <> show index) constructor.fields
    invocation = constructor.name <> if Array.null arguments then ""
      else " " <> String.joinWith " " (map (\argument -> "(" <> argument <> ")") arguments)
    definition = if Array.null constructor.fields
      then "let private " <> native <> " : " <> constructor.typeName <> " = " <> constructor.name
      else "let private " <> native <> " " <> String.joinWith " " parameters <> " : " <> constructor.typeName <> " = " <> invocation
    wrapper = if Array.null constructor.fields
      then "let " <> public <> " : obj = box " <> native
      else Emit.renderWrapper public constructor.fields native
  in definition <> "\n" <> wrapper

renderExpr :: NativeExpr -> Maybe String
renderExpr = case _ of
  NativeVar name _ -> Just name
  NativeIntLit value -> Just (show value)
  NativeBooleanLit value -> Just (if value then "true" else "false")
  NativeStringLit value -> Just (show value)
  NativeLambda parameters body -> do
    bodyCode <- renderExpr body
    let params = map (\(parameter :: NativeParameter) -> "(" <> parameter.name <> ": " <> nativeTypeName parameter.type <> ")") parameters
    pure ("(fun " <> String.joinWith " " params <> " -> " <> bodyCode <> ")")
  NativeApply function arguments -> do
    functionCode <- renderExpr function
    argumentCodes <- traverse renderExpr arguments
    pure ("(" <> functionCode <> String.joinWith "" (map (\argument -> " (" <> argument <> ")") argumentCodes) <> ")")
  NativeLet name type' value body -> do
    valueCode <- renderExpr value
    bodyCode <- renderExpr body
    pure ("(let " <> name <> ": " <> nativeTypeName type' <> " = " <> valueCode <> " in " <> bodyCode <> ")")
  NativeMatch targets cases fallback -> do
    targetCodes <- traverse renderExpr targets
    caseCodes <- traverse renderCase cases
    fallbackCode <- case fallback of
      Nothing -> pure "failwith \"Failed native match\""
      Just expression -> renderExpr expression
    let scrutinee = case targetCodes of
          [ one ] -> one
          many -> "(" <> String.joinWith ", " many <> ")"
    pure ("(match " <> scrutinee <> " with | " <> String.joinWith " | " caseCodes
      <> " | _ -> " <> fallbackCode <> ")")
  NativeConstructor name arguments -> do
    argumentCodes <- traverse renderExpr arguments
    pure (name <> if Array.null argumentCodes then "" else " " <> String.joinWith " " (map (\argument -> "(" <> argument <> ")") argumentCodes))
  NativeReuse constructor target arguments -> do
    targetCode <- renderExpr target
    argumentCodes <- traverse renderExpr arguments
    pure (constructor <> "_reuse " <> String.joinWith " " (map (\argument -> "(" <> argument <> ")") (Array.cons targetCode argumentCodes)))
  NativeSetField target index value body -> do
    targetCode <- renderExpr target
    valueCode <- renderExpr value
    bodyCode <- renderExpr body
    pure ("(let sharpurs_field_target = " <> targetCode <> " in sharpurs_field_target.Field" <> show index <> " <- " <> valueCode <> "; " <> bodyCode <> ")")
  NativeProject _ index value -> do
    code <- renderExpr value
    pure ("(" <> code <> ").Field" <> show index)
  NativeCopy type' expression -> do
    code <- renderExpr expression
    pure ("(sharpurs_copy_" <> nativeTypeName type' <> " (" <> code <> "))")
  NativeCast _ expression -> renderExpr expression
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
    name <> "_match" <> if Array.null patterns then ""
      else "(" <> String.joinWith ", " (map renderPattern patterns) <> ")"

renderDefinition :: Boolean -> String -> Array NativeParameter -> NativeExpr -> Maybe String
renderDefinition recursive name parameters body = do
  bodyCode <- renderExpr body
  let params = map (\(parameter :: NativeParameter) -> "(" <> parameter.name <> ": " <> nativeTypeName parameter.type <> ")") parameters
  pure ((if recursive then "let rec private " else "let private ") <> name <> " " <> String.joinWith " " params <> " = " <> bodyCode)

type MatchBinding =
  { condition :: String
  , bindings :: Array (Tuple String String)
  }

renderDefinitionWithTags :: Map.Map String Int -> Boolean -> String -> Array NativeParameter -> NativeExpr -> Maybe String
renderDefinitionWithTags tags recursive name parameters body = do
  bodyCode <- renderExprWithTags tags body
  let params = map (\(parameter :: NativeParameter) -> "(" <> parameter.name <> ": " <> nativeTypeName parameter.type <> ")") parameters
  pure ((if recursive then "let rec private " else "let private ") <> name <> " " <> String.joinWith " " params <> " = " <> bodyCode)

renderExprWithTags :: Map.Map String Int -> NativeExpr -> Maybe String
renderExprWithTags tags = case _ of
  NativeVar name _ -> Just name
  NativeIntLit value -> Just (show value)
  NativeBooleanLit value -> Just (if value then "true" else "false")
  NativeStringLit value -> Just (show value)
  NativeLambda parameters body -> do
    bodyCode <- renderExprWithTags tags body
    let params = map (\(parameter :: NativeParameter) -> "(" <> parameter.name <> ": " <> nativeTypeName parameter.type <> ")") parameters
    pure ("(fun " <> String.joinWith " " params <> " -> " <> bodyCode <> ")")
  NativeApply function arguments -> do
    functionCode <- renderExprWithTags tags function
    argumentCodes <- traverse (renderExprWithTags tags) arguments
    pure ("(" <> functionCode <> String.joinWith "" (map (\argument -> " (" <> argument <> ")") argumentCodes) <> ")")
  NativeLet name type' value body -> do
    valueCode <- renderExprWithTags tags value
    bodyCode <- renderExprWithTags tags body
    pure ("(let " <> name <> ": " <> nativeTypeName type' <> " = " <> valueCode <> " in " <> bodyCode <> ")")
  NativeMatch targets cases fallback -> do
    targetCodes <- traverse (renderExprWithTags tags) targets
    caseCodes <- traverse (renderDirectCase tags) cases
    let
      targetNames = Array.mapWithIndex (\index _ -> "sharpurs_match_target_" <> show index) targets
      targetBindings = Array.zipWith (\name code -> Tuple name code) targetNames targetCodes
      fallbackCode = case fallback of
        Nothing -> "failwith \"Failed native match\""
        Just expression -> fromMaybe "failwith \"Failed native match\"" (renderExprWithTags tags expression)
      body = Array.foldr renderBranch fallbackCode caseCodes
      bodyWithTargets = Array.foldr
        (\(Tuple name code) rest -> "(let " <> name <> " = " <> code <> " in " <> rest <> ")")
        body
        targetBindings
    pure ("(" <> bodyWithTargets <> ")")
    where
    renderBranch case' rest =
      "(if " <> case'.condition <> " then " <> wrapBindings case'.bindings case'.body <> " else " <> rest <> ")"
  NativeConstructor name arguments -> do
    argumentCodes <- traverse (renderExprWithTags tags) arguments
    pure (name <> if Array.null argumentCodes then "" else " " <> String.joinWith " " (map (\argument -> "(" <> argument <> ")") argumentCodes))
  NativeReuse constructor target arguments -> do
    targetCode <- renderExprWithTags tags target
    argumentCodes <- traverse (renderExprWithTags tags) arguments
    pure (constructor <> "_reuse " <> String.joinWith " " (map (\argument -> "(" <> argument <> ")") (Array.cons targetCode argumentCodes)))
  NativeSetField target index value body -> do
    targetCode <- renderExprWithTags tags target
    valueCode <- renderExprWithTags tags value
    bodyCode <- renderExprWithTags tags body
    pure ("(let sharpurs_field_target = (" <> targetCode <> ") in sharpurs_field_target.Field" <> show index <> " <- " <> valueCode <> "; " <> bodyCode <> ")")
  NativeProject _ index value -> do
    code <- renderExpr value
    pure ("(" <> code <> ").Field" <> show index)
  NativeCopy type' expression -> do
    code <- renderExpr expression
    pure ("(sharpurs_copy_" <> nativeTypeName type' <> " (" <> code <> "))")
  NativeCast _ expression -> renderExpr expression
  NativePrimitive primitive arguments -> do
    argumentCodes <- traverse (renderExprWithTags tags) arguments
    case argumentCodes of
      [ left, right ] -> pure ("(" <> left <> " " <> primitiveOperator primitive <> " " <> right <> ")")
      _ -> Nothing
  NativeBox expression -> do
    code <- renderExprWithTags tags expression
    pure ("(box (" <> code <> "))")

renderDirectCase :: Map.Map String Int -> NativeCase -> Maybe { condition :: String, bindings :: Array (Tuple String String), body :: String }
renderDirectCase tags { patterns, body } = do
  parts <- traverse (patternBinding tags) (Array.zipWith (\index pattern -> Tuple ("sharpurs_match_target_" <> show index) pattern) (Array.range 0 (Array.length patterns - 1)) patterns)
  bodyCode <- renderExprWithTags tags body
  pure
    { condition: if Array.null parts then "true" else String.joinWith " && " (map _.condition parts)
    , bindings: Array.concatMap _.bindings parts
    , body: bodyCode
    }

patternBinding :: Map.Map String Int -> Tuple String NativePattern -> Maybe MatchBinding
patternBinding tags (Tuple scrutinee pattern) = case pattern of
  PatternWildcard -> Just { condition: "true", bindings: [] }
  PatternVariable name -> Just { condition: "true", bindings: [ Tuple name scrutinee ] }
  PatternNamed name inner -> do
    nested <- patternBinding tags (Tuple scrutinee inner)
    pure nested { bindings = Array.cons (Tuple name scrutinee) nested.bindings }
  PatternInt value -> Just { condition: "(" <> scrutinee <> " = " <> show value <> ")", bindings: [] }
  PatternBoolean value -> Just { condition: "(" <> scrutinee <> " = " <> (if value then "true" else "false") <> ")", bindings: [] }
  PatternString value -> Just { condition: "(" <> scrutinee <> " = " <> show value <> ")", bindings: [] }
  PatternConstructor name patterns -> do
    tag <- Map.lookup name tags
    let
      fieldPatterns = Array.mapWithIndex
        (\index fieldPattern -> Tuple ("(" <> scrutinee <> ".Field" <> show index <> ")") fieldPattern)
        patterns
    nested <- traverse (patternBinding tags) fieldPatterns
    pure
      { condition: String.joinWith " && "
          (Array.cons (scrutinee <> ".Tag = " <> show tag) (map _.condition nested))
      , bindings: Array.concatMap _.bindings nested
      }

wrapBindings :: Array (Tuple String String) -> String -> String
wrapBindings bindings body =
  Array.foldr (\(Tuple name expression) rest -> "(let " <> name <> " = " <> expression <> " in " <> rest <> ")") body bindings

primitiveOperator :: NativePrimitive -> String
primitiveOperator = case _ of
  PrimitiveAdd -> "+"
  PrimitiveSubtract -> "-"
  PrimitiveModulo -> "%"
  PrimitiveLessThan -> "<"
  PrimitiveGreaterThan -> ">"
  PrimitiveEqual -> "="
