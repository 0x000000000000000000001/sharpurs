module Sharpurs.AdtInterop
  ( NativeProducer
  , prepareProducer
  , producerModule
  , translateConsumer
  ) where

import Prelude

import Control.Alternative (guard)
import Data.Array as Array
import Data.Foldable (all)
import Data.Map (Map)
import Data.Map as Map
import Data.Maybe (Maybe(..))
import Data.Set as Set
import Data.String as String
import Data.String.Pattern (Pattern(..), Replacement(..))
import Data.Tuple (Tuple(..))
import PureScript.Backend.Optimizer.Convert (BackendModule)
import PureScript.Backend.Optimizer.CoreFn (Ann, Ident(..), Module(..), ModuleName(..), unQualified)
import Sharpurs.AdtKernel as Kernel
import Sharpurs.AdtLayout as Layout
import Sharpurs.CodeGen as CodeGen
import Sharpurs.FsAst (FsModule, sanitizeName)

-- The constructor is private: a layout cannot be registered without the whole
-- producer having passed the native emitter, including its public wrappers.
data NativeProducer = NativeProducer FsModule Layout.Layout

prepareProducer :: Module Ann -> BackendModule -> Maybe NativeProducer
prepareProducer source backend = do
  layout <- Layout.fromModule source
  emitted <- Kernel.fromModule source backend
  let bindings = Array.concatMap (map (\(Tuple name _) -> name) <<< _.bindings) backend.bindings
  guard (all (\ctor -> Array.elem (unQualified ctor.sourceName) bindings) (constructors layout))
  pure (NativeProducer emitted layout)

producerModule :: NativeProducer -> FsModule
producerModule (NativeProducer emitted _) = emitted

translateConsumer :: Array NativeProducer -> Map String Int -> Module Ann -> Maybe FsModule
translateConsumer producers arities source@(Module consumer) = do
  let
    layouts = map (\(NativeProducer _ layout) -> layout) producers
    moduleNames = map _.moduleName layouts
    ModuleName consumerName = consumer.name
    entries = Array.concatMap
      (\layout -> map (\ctor -> { name: wrapperName layout ctor, arity: Array.length ctor.fields }) (constructors layout))
      layouts
    wrapperNames = map _.name entries
    producerNames = Array.concatMap layoutNames layouts
    consumerNames = Array.concatMap
      (\decl -> Array.cons (qualifiedName consumerName decl.name)
        (Array.concatMap (\ctor -> let name = qualifiedName consumerName ctor.name in [ name, name <> "usd_Ctor" ]) decl.constructors))
      consumer.dataDecls
  guard (unique moduleNames && not (Array.elem consumerName moduleNames))
  guard (unique (map modulePrefix (Array.snoc moduleNames consumerName)))
  guard (unique wrapperNames && unique producerNames)
  guard (all (\entry -> Map.lookup entry.name arities == Just entry.arity) entries)
  guard (all (\name -> not (Array.elem name producerNames)) consumerNames)
  pure (CodeGen.translateModuleWithConstructorWrappers (Set.fromFoldable wrapperNames) arities source)

constructors :: Layout.Layout -> Array Layout.Ctor
constructors = Array.concatMap _.constructors <<< _.declarations

wrapperName :: Layout.Layout -> Layout.Ctor -> String
wrapperName layout ctor = case unQualified ctor.sourceName of
  Ident name -> qualifiedName layout.moduleName name

-- A type and its value constructor may intentionally have the same name in
-- one module. Cross-module collisions after F# name flattening are rejected.
layoutNames :: Layout.Layout -> Array String
layoutNames layout = Array.nub
  (map _.name layout.declarations
    <> Array.concatMap (\ctor -> [ ctor.name, wrapperName layout ctor ]) (constructors layout))

qualifiedName :: String -> String -> String
qualifiedName moduleName name =
  sanitizeName (modulePrefix moduleName <> "_" <> name)

modulePrefix :: String -> String
modulePrefix = String.replaceAll (Pattern ".") (Replacement "_")

unique :: forall a. Ord a => Array a -> Boolean
unique values = Array.length (Array.nub values) == Array.length values
