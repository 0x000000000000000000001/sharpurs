-- | Specialized mutable ADT emitter: one class per constructor, direct type
-- | tests, and in-place `_reuse` functions. Native definitions are private;
-- | public wrappers remain the only boxed boundary.
module Sharpurs.Native.Specialized
  ( constructorClasses
  , renderTypeDeclarations
  , renderConstructor
  , renderExpr
  , renderDefinition
  ) where

import Prelude

import Data.Array as Array
import Data.Map (Map)
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

constructorClasses :: Layout -> Map String String
constructorClasses layout = Map.fromFoldable
  (Array.concatMap
    (\(Tuple _ constructors) -> map (\ctor -> Tuple ctor.name (classOf ctor)) constructors)
    (Map.toUnfoldable layout.types))

classOf :: Constructor -> String
classOf constructor = constructor.name <> "_Node"

renderTypeDeclarations :: Layout -> String
renderTypeDeclarations layout =
  let
    entries = Map.toUnfoldable layout.types
    bases = Array.concatMap (\(Tuple typeName _) -> baseDeclaration typeName) entries
    derived = Array.concatMap (\(Tuple typeName constructors) -> map (derivedDeclaration typeName) constructors) entries
    constructors = Array.concatMap (\(Tuple typeName constructors) -> map (constructorFunctions typeName) constructors) entries
  in String.joinWith "\n" (bases <> derived <> constructors)
  where
  baseDeclaration typeName =
    [ "[<AbstractClass; AllowNullLiteral>]\n"
        <> "type " <> typeName <> "() = class end"
    ]

  derivedDeclaration typeName constructor =
    if Array.null constructor.fields then
      "type " <> classOf constructor <> "() =\n"
        <> "    inherit " <> typeName <> "()"
    else
      "type " <> classOf constructor <> "("
        <> String.joinWith ", "
            (Array.mapWithIndex
              (\index field -> "arg" <> show index <> ": " <> nativeTypeName field)
              constructor.fields)
        <> ") =\n"
        <> "    inherit " <> typeName <> "()\n"
        <> String.joinWith "\n"
            (Array.mapWithIndex
              (\index field -> "    member val Field" <> show index <> ": " <> nativeTypeName field <> " = arg" <> show index <> " with get, set")
              constructor.fields)

  constructorFunctions typeName constructor =
    let
      params = Array.mapWithIndex
        (\index field -> "(arg" <> show index <> ": " <> nativeTypeName field <> ")")
        constructor.fields
      args = Array.mapWithIndex (\index _ -> "arg" <> show index) constructor.fields
      invocation = classOf constructor <> if Array.null args then "()" else "(" <> String.joinWith ", " args <> ")"
      definition = if Array.null constructor.fields
        then "let " <> constructor.name <> " : " <> typeName <> " = " <> invocation
        else "let " <> constructor.name <> " " <> String.joinWith " " params <> " : " <> typeName <> " = " <> invocation
      reuse =
        if Array.null constructor.fields then
          "let private " <> constructor.name <> "_reuse (value: " <> typeName <> ") : " <> typeName <> " = value"
        else
          "let private " <> constructor.name <> "_reuse (value: " <> typeName <> ") "
            <> String.joinWith " " params <> " : " <> typeName <> " =\n"
            <> "    match value with\n"
            <> "    | :? " <> classOf constructor <> " as node ->\n"
            <> String.joinWith "\n" (Array.mapWithIndex (\index _ -> "        node.Field" <> show index <> " <- arg" <> show index) constructor.fields) <> "\n"
            <> "        node\n"
            <> "    | _ -> " <> constructor.name <> (if Array.null args then "" else " " <> String.joinWith " " args)
    in definition <> "\n" <> reuse

renderConstructor :: String -> Constructor -> String
renderConstructor moduleName constructor =
  let
    public = Names.inModule moduleName (case constructor.sourceName of Qualified _ (Ident name) -> name)
    native = Names.adtNative public
    parameters = Array.mapWithIndex
      (\index field -> "(sharpurs_ctor_arg_" <> show index <> ": " <> nativeTypeName field <> ")")
      constructor.fields
    arguments = Array.mapWithIndex (\index _ -> "sharpurs_ctor_arg_" <> show index) constructor.fields
    invocation = constructor.name <> if Array.null arguments then "" else " " <> String.joinWith " " (map (\argument -> "(" <> argument <> ")") arguments)
    definition = if Array.null constructor.fields
      then "let private " <> native <> " : " <> constructor.typeName <> " = " <> constructor.name
      else "let private " <> native <> " " <> String.joinWith " " parameters <> " : " <> constructor.typeName <> " = " <> invocation
    wrapper = if Array.null constructor.fields
      then "let " <> public <> " : obj = box " <> native
      else Emit.renderWrapper public constructor.fields native
  in definition <> "\n" <> wrapper

renderDefinition :: Map String String -> Boolean -> String -> Array NativeParameter -> NativeExpr -> Maybe String
renderDefinition classes recursive name parameters body = do
  bodyCode <- renderExpr classes body
  let params = map (\(parameter :: NativeParameter) -> "(" <> parameter.name <> ": " <> nativeTypeName parameter.type <> ")") parameters
  pure ((if recursive then "let rec private " else "let private ") <> name <> " " <> String.joinWith " " params <> " = " <> bodyCode)

renderExpr :: Map String String -> NativeExpr -> Maybe String
renderExpr classes = case _ of
  NativeVar name _ -> Just name
  NativeIntLit value -> Just (show value)
  NativeBooleanLit value -> Just (if value then "true" else "false")
  NativeStringLit value -> Just (show value)
  NativeLambda parameters body -> do
    bodyCode <- renderExpr classes body
    let params = map (\(parameter :: NativeParameter) -> "(" <> parameter.name <> ": " <> nativeTypeName parameter.type <> ")") parameters
    pure ("(fun " <> String.joinWith " " params <> " -> " <> bodyCode <> ")")
  NativeApply function arguments -> do
    functionCode <- renderExpr classes function
    argumentCodes <- traverse (renderExpr classes) arguments
    pure ("(" <> functionCode <> String.joinWith "" (map (\argument -> " (" <> argument <> ")") argumentCodes) <> ")")
  NativeLet name type' value body -> do
    valueCode <- renderExpr classes value
    bodyCode <- renderExpr classes body
    pure ("(let " <> name <> ": " <> nativeTypeName type' <> " = " <> valueCode <> " in " <> bodyCode <> ")")
  NativeMatch targets cases fallback -> do
    targetCodes <- traverse (renderExpr classes) targets
    fallbackCode <- case fallback of
      Nothing -> pure "failwith \"Failed native match\""
      Just expression -> renderExpr classes expression
    let
      targetNames = Array.mapWithIndex (\index _ -> "sharpurs_match_target_" <> show index) targets
      targetBindings = Array.zipWith (\name code -> Tuple name code) targetNames targetCodes
      bodyCode = compileCases classes targetNames cases fallbackCode
      withTargets = Array.foldr
        (\(Tuple name code) rest -> "(let " <> name <> " = " <> code <> " in " <> rest <> ")")
        (fromMaybe fallbackCode bodyCode)
        targetBindings
    pure ("(" <> withTargets <> ")")
  NativeConstructor name arguments -> do
    argumentCodes <- traverse (renderExpr classes) arguments
    pure (name <> if Array.null argumentCodes then "" else " " <> String.joinWith " " (map (\argument -> "(" <> argument <> ")") argumentCodes))
  NativeReuse constructor target arguments -> do
    targetCode <- renderExpr classes target
    argumentCodes <- traverse (renderExpr classes) arguments
    pure (constructor <> "_reuse " <> String.joinWith " " (map (\argument -> "(" <> argument <> ")") (Array.cons targetCode argumentCodes)))
  NativeProject _ _ _ -> Nothing
  NativePrimitive primitive arguments -> do
    argumentCodes <- traverse (renderExpr classes) arguments
    case argumentCodes of
      [ left, right ] -> pure ("(" <> left <> " " <> primitiveOperator primitive <> " " <> right <> ")")
      _ -> Nothing
  NativeBox expression -> do
    code <- renderExpr classes expression
    pure ("(box (" <> code <> "))")

compileCases :: Map String String -> Array String -> Array NativeCase -> String -> Maybe String
compileCases classes targets cases fallback = case Array.uncons cases of
  Nothing -> Just fallback
  Just { head, tail } -> do
    next <- compileCases classes targets tail fallback
    body <- renderExpr classes head.body
    pure (compilePatterns classes targets head.patterns body next)

compilePatterns :: Map String String -> Array String -> Array NativePattern -> String -> String -> String
compilePatterns classes targets patterns body fallback = case Array.uncons targets of
  Nothing -> body
  Just { head: target, tail: restTargets } -> case Array.uncons patterns of
    Nothing -> body
    Just { head: pattern, tail: restPatterns } ->
      compilePattern classes target pattern
        (compilePatterns classes restTargets restPatterns body fallback)
        fallback

compilePattern :: Map String String -> String -> NativePattern -> String -> String -> String
compilePattern classes target pattern rest fallback = case pattern of
  PatternWildcard -> rest
  PatternVariable name -> "(let " <> name <> " = " <> target <> " in " <> rest <> ")"
  PatternNamed name inner ->
    "(let " <> name <> " = " <> target <> " in " <> compilePattern classes target inner rest fallback <> ")"
  PatternInt value -> "(if " <> target <> " = " <> show value <> " then " <> rest <> " else " <> fallback <> ")"
  PatternBoolean value -> "(if " <> target <> " = " <> (if value then "true" else "false") <> " then " <> rest <> " else " <> fallback <> ")"
  PatternString value -> "(if " <> target <> " = " <> show value <> " then " <> rest <> " else " <> fallback <> ")"
  PatternConstructor name fields ->
    case Map.lookup name classes of
      Nothing -> fallback
      Just className ->
        if Array.null fields then
          "(if (" <> target <> " :? " <> className <> ") then " <> rest <> " else " <> fallback <> ")"
        else
          let
            node = "sharpurs_pattern_node"
            fieldBody = compileFieldPatterns classes node (Array.mapWithIndex (\index field -> Tuple (node <> ".Field" <> show index) field) fields) rest fallback
          in "(if (" <> target <> " :? " <> className <> ") then (let " <> node <> " = (" <> target <> " :?> " <> className <> ") in " <> fieldBody <> ") else " <> fallback <> ")"

compileFieldPatterns :: Map String String -> String -> Array (Tuple String NativePattern) -> String -> String -> String
compileFieldPatterns classes _ fields body fallback = case Array.uncons fields of
  Nothing -> body
  Just { head: Tuple target pattern, tail: rest } ->
    compilePattern classes target pattern
      (compileFieldPatterns classes "" rest body fallback)
      fallback

primitiveOperator :: NativePrimitive -> String
primitiveOperator = case _ of
  PrimitiveAdd -> "+"
  PrimitiveSubtract -> "-"
  PrimitiveModulo -> "%"
  PrimitiveLessThan -> "<"
  PrimitiveGreaterThan -> ">"
  PrimitiveEqual -> "="
