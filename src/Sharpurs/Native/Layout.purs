-- | Build a native layout from the closed ADT subset already validated by
-- | `AdtKernel.AdtLayout`. Unsupported layouts return `Nothing`; callers keep
-- | the boxed backend.
module Sharpurs.Native.Layout
  ( fromModule
  , nativeType
  , sourceType
  , lookupConstructor
  ) where

import Prelude

import Data.Array as Array
import Data.Map (Map)
import Data.Map as Map
import Data.Maybe (Maybe(..))
import Data.Traversable (traverse)
import Data.Tuple (Tuple(..))
import PureScript.Backend.Optimizer.CoreFn (ExprType(..), Ident, Module, Qualified)
import Sharpurs.AdtLayout as AdtLayout
import Sharpurs.Native.Types (Constructor, Layout, NativeType(..))

fromModule :: forall a. Module a -> Maybe Layout
fromModule module' = do
  adt <- AdtLayout.fromModule module'
  declarations <- traverse (convertDeclaration adt) adt.declarations
  let
    types = Map.fromFoldable (map
      (\item -> Tuple item.name (map (\constructor -> { name: constructor.name, sourceName: constructor.source, typeName: item.name, fields: constructor.fields }) item.constructors))
      declarations)
    constructors = Map.fromFoldable
      (Array.concatMap
        (\declaration -> map
          (\constructor -> Tuple constructor.source
            { name: constructor.name, sourceName: constructor.source, typeName: declaration.name, fields: constructor.fields })
          declaration.constructors)
        declarations)
  pure { moduleName: adt.moduleName, names: adt.types, types, constructors }

convertDeclaration layout declaration = do
  constructors <- traverse (convertConstructor layout) declaration.constructors
  pure { name: declaration.name, constructors }

convertConstructor layout constructor = do
  fields <- traverse (nativeType layout) constructor.fields
  pure { name: constructor.name, fields, source: constructor.sourceName }

nativeType :: AdtLayout.Layout -> ExprType -> Maybe NativeType
nativeType layout = case _ of
  Int -> Just NativeInt
  Boolean -> Just NativeBoolean
  String -> Just NativeString
  ADT name _ args -> do
    guardNoArgs args
    nativeName <- Map.lookup name layout.types
    pure (NativeAdt nativeName [])
  _ -> Nothing

sourceType :: Layout -> ExprType -> Maybe NativeType
sourceType layout = case _ of
  Int -> Just NativeInt
  Boolean -> Just NativeBoolean
  String -> Just NativeString
  ADT name _ args -> do
    guardNoArgs args
    nativeName <- Map.lookup name layout.names
    pure (NativeAdt nativeName [])
  _ -> Nothing

lookupConstructor :: Layout -> Qualified Ident -> Maybe Constructor
lookupConstructor layout name = Map.lookup name layout.constructors

guardNoArgs :: Array ExprType -> Maybe Unit
guardNoArgs args = if Array.null args then Just unit else Nothing
