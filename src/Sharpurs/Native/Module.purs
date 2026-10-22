-- | Whole-module native compilation. A module is accepted only when every
-- | source binding has a proven native signature and lowers to the native IR.
-- | Any failure returns Nothing so the existing boxed backend can emit it.
module Sharpurs.Native.Module (compile, constructorNames) where

import Prelude

import Data.Array as Array
import Data.Map (Map)
import Data.Map as Map
import Data.String as String
import Data.Maybe (Maybe(..))
import Data.Traversable (traverse)
import Data.Tuple (Tuple(..))
import PureScript.Backend.Optimizer.CoreFn (Ann(..), Ident(..), Module(..), ModuleName(..), Qualified(..))
import PureScript.Backend.Optimizer.CoreFn as C
import Sharpurs.Analysis.Source as Source
import Sharpurs.FsAst (FsDecl(..), FsModule(..), sanitizeName)
import Sharpurs.Native.Emit as Emit
import Sharpurs.Native.Layout as NativeLayout
import Sharpurs.Native.Lower as NativeLower
import Sharpurs.Native.Types (Layout)
import Sharpurs.Names as Names

compile :: Module Ann -> Maybe FsModule
compile source@(Module module') = do
  guardEmptyForeign module'
  layout <- NativeLayout.fromModule source
  let bindings = Array.concatMap Source.bindings module'.decls
  signatures <- traverse (signatureFor layout) bindings
  definitions <- traverse (definition layout signatures) bindings
  let
    constructors = Array.concatMap
      (\(Tuple _ ctors) -> map (Emit.renderConstructor layout.moduleName) ctors)
      (Map.toUnfoldable layout.types)
  pure (FsModule (sanitizeName layout.moduleName)
    [ FsRaw (String.joinWith "\n" (Array.cons (Emit.renderTypeDeclarations layout) (constructors <> definitions))) ])

constructorNames :: Module Ann -> Array String
constructorNames source = case NativeLayout.fromModule source of
  Nothing -> []
  Just layout -> Array.concatMap
    (\(Tuple _ ctors) -> map (\ctor -> Names.inModule layout.moduleName (case ctor.sourceName of Qualified _ (Ident name) -> name)) ctors)
    (Map.toUnfoldable layout.types)

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

definition :: Layout -> Array (Tuple (Qualified Ident) NativeLower.Signature) -> Source.SourceBinding Ann -> Maybe String
definition layout signatures binding = do
  signature <- Array.findMap (\(Tuple key value) -> if key == Qualified (Just (moduleName layout)) binding.name then Just value else Nothing) signatures
  lowered <- NativeLower.lowerBinding layout signatures signature binding.expr
  native <- Emit.renderDefinition binding.recursive signature.name lowered.parameters lowered.body
  let public = Names.inModule layout.moduleName (case binding.name of Ident name -> name)
  pure (native <> "\n" <> Emit.renderWrapper public signature.args signature.name)

guardEmptyForeign :: forall f c. { foreign :: Map Ident f, classDecls :: Array c | _ } -> Maybe Unit
guardEmptyForeign source =
  if Map.isEmpty source.foreign && Array.null source.classDecls then Just unit else Nothing

moduleName :: Layout -> ModuleName
moduleName layout = ModuleName layout.moduleName
