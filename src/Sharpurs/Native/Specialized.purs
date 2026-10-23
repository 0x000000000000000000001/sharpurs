-- | Specialized mutable ADT emitter: one class per constructor, direct type
-- | tests, and in-place `_reuse` functions. Native definitions are private;
-- | public wrappers remain the only boxed boundary.
module Sharpurs.Native.Specialized
  ( constructorClasses
  , renderTypeDeclarations
  , renderCopyFunctions
  , renderConstructor
  , renderConstructorWithLayout
  , renderExpr
  , renderDefinition
  , renderWrapperWithCopies
  ) where

import Prelude

import Data.Array as Array
import Data.Map (Map)
import Data.Foldable (foldr)
import Data.Map as Map
import Data.Maybe (Maybe(..), fromMaybe)
import Data.Set (Set)
import Data.Set as Set
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
    (\(Tuple _ constructors) ->
      let kind = typeKind constructors
      in map (\ctor -> Tuple ctor.name (constructorRepresentation kind ctor)) constructors)
    (Map.toUnfoldable layout.types))

data TypeKind = AllNullary | SentinelNullary | ClassConstructors

typeKind :: Array Constructor -> TypeKind
typeKind constructors
  | Array.all (Array.null <<< _.fields) constructors = AllNullary
  | Array.length (Array.filter (Array.null <<< _.fields) constructors) == 1 = SentinelNullary
  | otherwise = ClassConstructors

constructorRepresentation :: TypeKind -> Constructor -> String
constructorRepresentation kind constructor = case kind, Array.null constructor.fields of
  AllNullary, _ -> "#du"
  SentinelNullary, true -> "#null"
  SentinelNullary, false -> "#node:" <> classOf constructor
  _, _ -> classOf constructor

classOf :: Constructor -> String
classOf constructor = constructor.name <> "_Node"

renderTypeDeclarations :: Layout -> String
renderTypeDeclarations layout =
  let
    entries = Map.toUnfoldable layout.types
  in String.joinWith "\n" (Array.concatMap renderType entries)
  where
  renderType (Tuple typeName constructors) = case typeKind constructors of
    AllNullary -> [ duDeclaration typeName constructors ]
    SentinelNullary ->
      [ baseDeclaration typeName ]
        <> map (derivedDeclaration typeName) (Array.filter (not <<< Array.null <<< _.fields) constructors)
        <> map (constructorFunctions typeName) constructors
    ClassConstructors ->
      [ baseDeclaration typeName ]
        <> map (derivedDeclaration typeName) constructors
        <> map (constructorFunctions typeName) constructors

  baseDeclaration typeName =
    "[<AbstractClass; AllowNullLiteral>]\n"
      <> "type " <> typeName <> "() = class end"

  duDeclaration typeName constructors =
    "type " <> typeName <> " =\n"
      <> String.joinWith "\n" (map (\constructor -> "    | " <> constructor.name) constructors)

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
      kind = typeKindForName typeName
      params = Array.mapWithIndex
        (\index field -> "(arg" <> show index <> ": " <> nativeTypeName field <> ")")
        constructor.fields
      args = Array.mapWithIndex (\index _ -> "arg" <> show index) constructor.fields
      invocation = classOf constructor <> if Array.null args then "()" else "(" <> String.joinWith ", " args <> ")"
      definition = case kind, Array.null constructor.fields of
        AllNullary, _ -> ""
        SentinelNullary, true -> "let " <> constructor.name <> " : " <> typeName <> " = null"
        _, true -> "let " <> constructor.name <> " : " <> typeName <> " = " <> invocation
        _, false -> "let " <> constructor.name <> " " <> String.joinWith " " params <> " : " <> typeName <> " = " <> invocation
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
    in if String.null definition then reuse else definition <> "\n" <> reuse

  typeKindForName typeName = case Map.lookup typeName layout.types of
    Just constructors -> typeKind constructors
    Nothing -> ClassConstructors

renderCopyFunctions :: Layout -> String
renderCopyFunctions layout =
  let entries = Map.toUnfoldable layout.types
  in if Array.null entries then ""
     else String.joinWith "\n"
       (Array.mapWithIndex
         (\index (Tuple typeName constructors) ->
           (if index == 0 then "let rec private " else "and private ")
             <> copyFunctionName typeName <> " (value: " <> typeName <> ") : " <> typeName <> " =\n"
             <> "    match value with\n"
             <> String.joinWith "\n" (Array.mapMaybe (copyCase typeName) constructors)
             <> "\n    | _ -> value")
         entries)

copyFunctionName :: String -> String
copyFunctionName typeName = "sharpurs_copy_" <> typeName

copyCase :: String -> Constructor -> Maybe String
copyCase _typeName constructor =
  if Array.null constructor.fields then Nothing
  else
    let
      className = classOf constructor
      copiedFields = Array.mapWithIndex
        (\index field -> copyField field ("node.Field" <> show index))
        constructor.fields
      rebuilt = constructor.name
        <> if Array.null copiedFields then "" else " " <> String.joinWith " " (map (\field -> "(" <> field <> ")") copiedFields)
    in Just ("    | :? " <> className <> " as node -> " <> rebuilt)

copyField :: NativeType -> String -> String
copyField typeName value = case typeName of
  NativeAdt name _ -> copyFunctionName name <> " (" <> value <> ")"
  _ -> value

renderConstructorWithLayout :: Layout -> Constructor -> String
renderConstructorWithLayout layout constructor =
  let
    public = Names.inModule layout.moduleName (case constructor.sourceName of Qualified _ (Ident name) -> name)
    kind = case Map.lookup constructor.typeName layout.types of
      Just constructors -> typeKind constructors
      Nothing -> ClassConstructors
  in case kind, Array.null constructor.fields of
      AllNullary, _ ->
        "let private " <> Names.adtNative public <> " : " <> constructor.typeName <> " = " <> constructor.name
          <> "\nlet " <> public <> " : obj = box " <> Names.adtNative public
      SentinelNullary, true ->
        "let private " <> Names.adtNative public <> " : " <> constructor.typeName <> " = null"
          <> "\nlet " <> public <> " : obj = box " <> Names.adtNative public
      _, true -> renderConstructor layout.moduleName constructor
      _, false -> renderConstructor layout.moduleName constructor

renderWrapperWithCopies :: String -> Array NativeType -> String -> Set Int -> String
renderWrapperWithCopies publicName argumentTypes nativeName copied =
  let
    args = Array.mapWithIndex (\index _ -> "sharpurs_native_arg_" <> show index) argumentTypes
    argument index =
      let value = "(unbox<" <> nativeTypeName (fromMaybeType (Array.index argumentTypes index)) <> "> " <> (fromMaybe "" (Array.index args index)) <> ")"
      in if Set.member index copied
           then "(" <> copyFunctionName (copyTypeName (fromMaybeType (Array.index argumentTypes index))) <> " " <> value <> ")"
           else value
    invocation = nativeName <> String.joinWith "" (Array.mapWithIndex (\index _ -> " " <> argument index) argumentTypes)
    body = "box (" <> invocation <> ")"
  in case Array.uncons args of
       Nothing -> "let " <> publicName <> " : obj = " <> body
       Just _ -> "let " <> publicName <> " : obj = "
         <> foldr (\arg inner -> "box (fun (" <> arg <> ": obj) -> " <> inner <> ")") body args

fromMaybeType :: Maybe NativeType -> NativeType
fromMaybeType = case _ of
  Just value -> value
  Nothing -> NativeBoxed

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
  NativeSetField target index value body -> do
    targetCode <- renderExpr classes target
    valueCode <- renderExpr classes value
    bodyCode <- renderExpr classes body
    pure ("(let sharpurs_field_target = (" <> targetCode <> ") in sharpurs_field_target.Field" <> show index <> " <- " <> valueCode <> "; " <> bodyCode <> ")")
  NativeProject constructor index value -> do
    code <- renderExpr classes value
    pure ("((" <> code <> " :?> " <> classFor constructor <> ").Field" <> show index <> ")")
  NativeCopy type' expression -> do
    code <- renderExpr classes expression
    pure ("(sharpurs_copy_" <> copyTypeName type' <> " (" <> code <> "))")
  NativeCast constructor expression -> do
    code <- renderExpr classes expression
    pure ("((" <> code <> " :?> " <> classFor constructor <> "))")
  NativePrimitive primitive arguments -> do
    argumentCodes <- traverse (renderExpr classes) arguments
    case argumentCodes of
      [ left, right ] -> pure ("(" <> left <> " " <> primitiveOperator primitive <> " " <> right <> ")")
      _ -> Nothing
  NativeBox expression -> do
    code <- renderExpr classes expression
    pure ("(box (" <> code <> "))")

compileCases :: Map String String -> Array String -> Array NativeCase -> String -> Maybe String
compileCases classes targets cases fallback = do
  compiled <- traverse compileCase (Array.mapWithIndex Tuple cases)
  pure (compileTree compiled fallback)
  where
  compileCase (Tuple index case') = do
    body <- renderExpr classes case'.body
    let
      prefix = "sharpurs_case_" <> show index
      bindings = Array.concatMap
        (\(Tuple name pattern) -> bindingPattern classes prefix name pattern)
        (Array.zip targets case'.patterns)
      bound = Array.foldr
        (\(Tuple name value) rest -> "(let " <> name <> " = " <> value <> " in " <> rest <> ")")
        body
        bindings
    pure
      { conditions: conditionLists classes targets case'.patterns
      , body: bound
      }

type CompiledCase =
  { conditions :: Array Condition
  , body :: String
  }

type Condition =
  { key :: String
  , condition :: String
  }

compileTree :: Array CompiledCase -> String -> String
compileTree cases fallback = case Array.uncons cases of
  Nothing -> fallback
  Just { head: first, tail: rest } ->
    case Array.head first.conditions of
      Nothing -> first.body
      Just condition ->
        let
          allCases = Array.cons first rest
          hasKey item = Array.any (\candidate -> candidate.key == condition.key) item.conditions
          requires item = Array.any (\candidate -> candidate.condition == condition.condition) item.conditions
          trueCases = Array.mapMaybe
            (\item ->
              if requires item
                then Just item { conditions = Array.filter (\candidate -> candidate.condition /= condition.condition) item.conditions }
                else if hasKey item then Nothing
                else Just item)
            allCases
          falseCases = Array.filter (not <<< requires) allCases
          falseBranch = compileTree falseCases fallback
          trueBranch = if Array.null falseCases
            then compileTree trueCases fallback
            else compileTree trueCases falseBranch
        in "(if " <> condition.condition <> " then " <> trueBranch <> " else " <> falseBranch <> ")"

conditionLists :: Map String String -> Array String -> Array NativePattern -> Array Condition
conditionLists classes targets patterns =
  Array.concatMap identity (Array.zipWith (conditionList classes) targets patterns)

conditionList :: Map String String -> String -> NativePattern -> Array Condition
conditionList classes target pattern = case pattern of
  PatternWildcard -> []
  PatternVariable _ -> []
  PatternNamed _ inner -> conditionList classes target inner
  PatternInt value -> [ { key: target, condition: "(" <> target <> " = " <> show value <> ")" } ]
  PatternBoolean value -> [ { key: target, condition: "(" <> target <> " = " <> (if value then "true" else "false") <> ")" } ]
  PatternString value -> [ { key: target, condition: "(" <> target <> " = " <> show value <> ")" } ]
  PatternConstructor name fields ->
    case Map.lookup name classes of
      Nothing -> [ { key: target, condition: "false" } ]
      Just representation -> case representation, Array.null fields of
        "#du", _ -> [ { key: target, condition: "(" <> target <> " = " <> name <> ")" } ]
        "#null", _ -> [ { key: target, condition: "(isNull " <> target <> ")" } ]
        _, true -> [ { key: target, condition: "(" <> target <> " :? " <> representation <> ")" } ]
        _, false ->
          let
            className = classOfRepresentation representation
            parent = if isNodeRepresentation representation
              then { key: target, condition: "(not (isNull " <> target <> "))" }
              else { key: target, condition: "(" <> target <> " :? " <> className <> ")" }
            node = "(" <> target <> " :?> " <> className <> ")"
            children = Array.concatMap identity
              (Array.mapWithIndex
                (\index field -> conditionList classes (node <> ".Field" <> show index) field)
                fields)
          in Array.cons parent children

bindingPattern :: Map String String -> String -> String -> NativePattern -> Array (Tuple String String)
bindingPattern classes prefix target pattern = case pattern of
  PatternWildcard -> []
  PatternVariable name -> [ Tuple name target ]
  PatternNamed name inner -> Array.cons (Tuple name target) (go prefix target inner)
  PatternInt _ -> []
  PatternBoolean _ -> []
  PatternString _ -> []
  PatternConstructor name fields ->
    if Array.null fields then []
    else case Map.lookup name classes of
      Nothing -> []
      Just representation ->
        let
          node = "(" <> target <> " :?> " <> classOfRepresentation representation <> ")"
          nodeName = prefix <> "_node"
          children = Array.concatMap
            (\(Tuple index field) -> go (prefix <> "_" <> show index) ("(" <> nodeName <> ").Field" <> show index) field)
            (Array.mapWithIndex Tuple fields)
        in Array.cons (Tuple nodeName node) children
  where
  go localPrefix localTarget = case _ of
    PatternWildcard -> []
    PatternVariable name -> [ Tuple name localTarget ]
    PatternNamed name inner -> Array.cons (Tuple name localTarget) (go localPrefix localTarget inner)
    PatternInt _ -> []
    PatternBoolean _ -> []
    PatternString _ -> []
    PatternConstructor name fields ->
      if Array.null fields then []
      else case Map.lookup name classes of
        Nothing -> []
        Just representation ->
          let
            node = "(" <> localTarget <> " :?> " <> classOfRepresentation representation <> ")"
            nodeName = localPrefix <> "_node"
            children = Array.concatMap
              (\(Tuple index field) -> go (localPrefix <> "_" <> show index) ("(" <> nodeName <> ").Field" <> show index) field)
              (Array.mapWithIndex Tuple fields)
          in Array.cons (Tuple nodeName node) children

classFor :: String -> String
classFor constructor = constructor <> "_Node"

isNodeRepresentation :: String -> Boolean
isNodeRepresentation representation = String.take 6 representation == "#node:"

classOfRepresentation :: String -> String
classOfRepresentation representation =
  if isNodeRepresentation representation then String.drop 6 representation else representation

copyTypeName :: NativeType -> String
copyTypeName = case _ of
  NativeAdt name _ -> name
  _ -> "sharpurs_boxed"

primitiveOperator :: NativePrimitive -> String
primitiveOperator = case _ of
  PrimitiveAdd -> "+"
  PrimitiveSubtract -> "-"
  PrimitiveModulo -> "%"
  PrimitiveLessThan -> "<"
  PrimitiveGreaterThan -> ">"
  PrimitiveEqual -> "="
