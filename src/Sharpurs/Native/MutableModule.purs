-- | Whole-module compilation with the mutable ADT representation.
-- | `compileMixed` keeps unsupported bindings from an already validated
-- | `AdtKernel` module, while native-compatible bindings use the mutable path.
module Sharpurs.Native.MutableModule (compile, compileMixed, constructorNames) where

import Prelude

import Data.Array as Array
import Data.Map (Map)
import Data.Map as Map
import Data.Maybe (Maybe(..))
import Data.Set (Set)
import Data.Set as Set
import Data.String as String
import Data.Traversable (traverse)
import Data.Tuple (Tuple(..))
import PureScript.Backend.Optimizer.CoreFn (Ann(..), Bind, Ident(..), Module(..), ModuleName(..), Qualified(..))
import PureScript.Backend.Optimizer.CoreFn as C
import Sharpurs.Analysis.Source as Source
import Sharpurs.CodeGen as CodeGen
import Sharpurs.FsAst (FsDecl(..), FsModule(..), modulePrefix, sanitizeName)
import Sharpurs.Native.Emit as Emit
import Sharpurs.Native.IR (NativeExpr, NativeParameter)
import Sharpurs.Native.Interprocedural as Interprocedural
import Sharpurs.Native.Layout as NativeLayout
import Sharpurs.Native.Lower as NativeLower
import Sharpurs.Native.Specialized as Specialized
import Sharpurs.Native.Reuse as Reuse
import Sharpurs.Native.Types (Layout)
import Sharpurs.Native.Types as NativeTypes
import Sharpurs.Native.Usage as Usage
import Sharpurs.Names as Names

type Lowered =
  { binding :: Source.SourceBinding Ann
  , signature :: NativeLower.Signature
  , lowered :: { parameters :: Array NativeParameter, body :: NativeExpr }
  , exported :: Boolean
  }

compile :: Module Ann -> Maybe FsModule
compile source@(Module module') = do
  guardEmptyForeign module'
  layout <- NativeLayout.fromModule source
  let
    bindings = Array.filter (not <<< isConstructor layout) (Array.concatMap Source.bindings module'.decls)
    exportedNames = exportedNamesOf module'
  signatures <- traverse (signatureFor layout) bindings
  let allSignatures = signatures <> constructorSignatures layout
  lowered <- traverse (loweredBinding layout allSignatures exportedNames) bindings
  definitions <- renderDefinitions layout lowered
  constructors <- renderConstructors layout
  pure (FsModule (sanitizeName layout.moduleName)
    [ FsRaw (String.joinWith "\n" (Array.cons (Specialized.renderTypeDeclarations layout) (constructors <> definitions))) ])

compileMixed :: Module Ann -> Map String Int -> Maybe FsModule
compileMixed source@(Module module') arities = do
  guardEmptyForeign module'
  layout <- NativeLayout.fromModule source
  let
    bindings = Array.filter (not <<< isConstructor layout) (Array.concatMap Source.bindings module'.decls)
    exportedNames = exportedNamesOf module'
    signatures = Array.mapMaybe (signatureFor layout) bindings <> constructorSignatures layout
    attempted = map (\binding ->
      { binding
      , lowered: do
          signature <- Array.findMap
            (\(Tuple key value) -> if key == Qualified (Just (moduleName layout)) binding.name then Just value else Nothing)
            signatures
          value <- NativeLower.lowerBinding layout signatures signature binding.expr
          pure { binding, signature, lowered: value, exported: Set.member (case binding.name of Ident name -> name) exportedNames }
      }) bindings
    nativeItems = Array.mapMaybe _.lowered attempted
    fallbackItems = Array.filter (\item -> case item.lowered of
      Nothing -> true
      Just _ -> false) attempted
  definitions <- renderDefinitions layout nativeItems
  fallbackGroups <- traverse (\item -> findBindingGroup source item.binding.name) fallbackItems
  let
    prefix = modulePrefix layout.moduleName
    fallbackDecls = Array.concatMap (CodeGen.translateBind arities (Just prefix)) fallbackGroups
  constructors <- renderConstructors layout
  pure (FsModule (sanitizeName layout.moduleName)
    (Array.cons (FsRaw (String.joinWith "\n" (Array.cons (Specialized.renderTypeDeclarations layout) (constructors <> definitions)))) fallbackDecls))

constructorNames :: Module Ann -> Array String
constructorNames source = case NativeLayout.fromModule source of
  Nothing -> []
  Just layout -> Array.concatMap
    (\(Tuple _ ctors) -> map (\ctor -> Names.inModule layout.moduleName (case ctor.sourceName of Qualified _ (Ident name) -> name)) ctors)
    (Map.toUnfoldable layout.types)

renderDefinitions :: Layout -> Array Lowered -> Maybe (Array String)
renderDefinitions layout items = do
  let
    functions = map (\item ->
      { name: item.signature.name
      , parameters: item.lowered.parameters
      , body: item.lowered.body
      }) items
    exported = Set.fromFoldable (map _.signature.name (Array.filter _.exported items))
    summaries = Interprocedural.analyze exported functions
  let classes = Specialized.constructorClasses layout
  traverse (definition layout classes summaries) items

renderConstructors :: Layout -> Maybe (Array String)
renderConstructors layout = pure
  (Array.concatMap
    (\(Tuple _ ctors) -> map (Specialized.renderConstructor layout.moduleName) ctors)
    (Map.toUnfoldable layout.types))

signatureFor :: Layout -> Source.SourceBinding Ann -> Maybe (Tuple (Qualified Ident) NativeLower.Signature)
signatureFor layout binding = do
  let lambda = Source.lambdas binding.expr
  args <- traverse parameterType lambda.parameters
  result <- Source.annotation lambda.body >>= NativeLayout.sourceType layout
  let public = Names.inModule layout.moduleName (case binding.name of Ident name -> name)
  pure (Tuple (Qualified (Just (moduleName layout)) binding.name)
    { name: Names.adtNative public, args, result })
  where
  parameterType parameter = case annType parameter.ann of
    Just (C.Func args _) -> case Array.uncons args of
      Just { head } -> NativeLayout.sourceType layout head
      Nothing -> Nothing
    _ -> Nothing
  annType (Ann annotation) = annotation.type

constructorSignatures :: Layout -> Array (Tuple (Qualified Ident) NativeLower.Signature)
constructorSignatures layout = Array.concatMap
  (\(Tuple _ ctors) -> map
    (\ctor -> Tuple ctor.sourceName
      { name: Names.adtNative (Names.inModule layout.moduleName (case ctor.sourceName of Qualified _ (Ident name) -> name))
      , args: ctor.fields
      , result: NativeTypes.NativeAdt ctor.typeName []
      })
    ctors)
  (Map.toUnfoldable layout.types)

loweredBinding :: Layout -> Array (Tuple (Qualified Ident) NativeLower.Signature) -> Set String -> Source.SourceBinding Ann -> Maybe Lowered
loweredBinding layout signatures exportedNames binding = do
  signature <- Array.findMap
    (\(Tuple key value) -> if key == Qualified (Just (moduleName layout)) binding.name then Just value else Nothing)
    signatures
  lowered <- NativeLower.lowerBinding layout signatures signature binding.expr
  pure { binding, signature, lowered, exported: Set.member (case binding.name of Ident name -> name) exportedNames }

definition :: Layout -> Map String String -> Map String Interprocedural.Summary -> Lowered -> Maybe String
definition layout classes summaries item = do
  let
    usage = Usage.usageExpr item.lowered.body
    mutableNames = case Map.lookup item.signature.name summaries of
      Nothing -> Set.empty
      Just summary -> Set.fromFoldable
        (Array.mapMaybe
          (\(Tuple parameter mutable) -> if mutable then Just parameter.name else Nothing)
          (Array.zip item.lowered.parameters summary.mutableParameters))
    rewritten = Reuse.rewriteWith mutableNames usage item.lowered.body
    public = Names.inModule layout.moduleName (case item.binding.name of Ident name -> name)
  native <- Specialized.renderDefinition classes item.binding.recursive item.signature.name item.lowered.parameters rewritten
  pure (native <> "\n" <> Emit.renderWrapper public item.signature.args item.signature.name)

findBindingGroup :: Module Ann -> Ident -> Maybe (Bind Ann)
findBindingGroup (Module module') name =
  Array.find (\group -> Array.any (\binding -> binding.name == name) (Source.bindings group)) module'.decls

isConstructor :: Layout -> Source.SourceBinding Ann -> Boolean
isConstructor layout binding =
  case NativeLayout.lookupConstructor layout (Qualified (Just (moduleName layout)) binding.name) of
    Just _ -> true
    Nothing -> false

guardEmptyForeign :: forall f c. { foreign :: Map Ident f, classDecls :: Array c | _ } -> Maybe Unit
guardEmptyForeign source =
  if Map.isEmpty source.foreign && Array.null source.classDecls then Just unit else Nothing

exportedNamesOf :: { exports :: Array Ident | _ } -> Set String
exportedNamesOf module' = Set.fromFoldable (map (\(Ident name) -> name) module'.exports)

moduleName :: Layout -> ModuleName
moduleName layout = ModuleName layout.moduleName
