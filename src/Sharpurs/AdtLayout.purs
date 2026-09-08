module Sharpurs.AdtLayout
  ( Layout
  , Ctor
  , NativeDecl
  , fromModule
  , nativeType
  , lookupCtor
  , printDeclarations
  , validIdentifier
  ) where

import Prelude

import Control.Alternative (guard)
import Data.Array as Array
import Data.Foldable (all)
import Data.Map (Map)
import Data.Map as Map
import Data.Maybe (Maybe(..))
import Data.String as String
import Data.String.CodeUnits as CU
import Data.String.Pattern (Pattern(..), Replacement(..))
import Data.Traversable (traverse)
import Data.Tuple (Tuple(..))
import PureScript.Backend.Optimizer.CoreFn (ExprType, Ident(..), Module(..), ModuleName(..), Qualified(..))
import PureScript.Backend.Optimizer.CoreFn as C
import Sharpurs.FsAst (sanitizeName)

type Ctor =
  { name :: String
  , sourceName :: Qualified Ident
  , typeName :: String
  , sourceType :: ExprType
  , fields :: Array ExprType
  , nativeFields :: Array String
  }

type NativeDecl = { name :: String, constructors :: Array Ctor }

type Layout =
  { moduleName :: String
  , types :: Map String String
  , constructors :: Map (Qualified Ident) Ctor
  , declarations :: Array NativeDecl
  }

-- This closed pilot takes layouts exclusively from TAST dataDecls. A module
-- containing polymorphic, external or unsupported fields is rejected as a
-- whole. Production needs a separate policy for shared module boundaries.
fromModule :: forall a. Module a -> Maybe Layout
fromModule (Module mod) = do
  let
    ModuleName moduleName = mod.name
    qualify name = moduleName <> "." <> name
    nativeName name = sanitizeName (String.replaceAll (Pattern ".") (Replacement "_") moduleName <> "_" <> name)
    typeNames = map (nativeName <<< _.name) mod.dataDecls
    ctorNames = Array.concatMap (\decl -> map (\ctor -> nativeName ctor.name <> "usd_Ctor") decl.constructors) mod.dataDecls
    names = typeNames <> ctorNames
    initial =
      { moduleName
      , types: Map.fromFoldable (map (\decl -> Tuple (qualify decl.name) (nativeName decl.name)) mod.dataDecls)
      , constructors: Map.empty
      , declarations: []
      }
  guard (not (Array.null mod.dataDecls))
  guard (all validModulePart (String.split (Pattern ".") moduleName))
  guard (all (\decl -> validIdentifier (sanitizeName decl.name)
    && Array.null decl.vars && not (Array.null decl.constructors)
    && all (validIdentifier <<< sanitizeName <<< _.name) decl.constructors) mod.dataDecls)
  guard (all validIdentifier names && Array.length (Array.nub names) == Array.length names)
  declarations <- traverse
    (\decl -> do
      let
        typeName = nativeName decl.name
        fullName = qualify decl.name
        sourceType = C.ADT fullName (String.split (Pattern ".") fullName) []
      constructors <- traverse
        (\ctor -> do
          nativeFields <- traverse (nativeType initial) ctor.fields
          pure
            { name: nativeName ctor.name <> "usd_Ctor"
            , sourceName: Qualified (Just mod.name) (Ident ctor.name)
            , typeName
            , sourceType
            , fields: ctor.fields
            , nativeFields
            }
        ) decl.constructors
      pure { name: typeName, constructors }
    ) mod.dataDecls
  let constructors = Array.concatMap _.constructors declarations
  pure initial
    { declarations = declarations
    , constructors = Map.fromFoldable (map (\ctor -> Tuple ctor.sourceName ctor) constructors)
    }

nativeType :: Layout -> ExprType -> Maybe String
nativeType layout = case _ of
  C.Int -> Just "int"
  C.Boolean -> Just "bool"
  C.ADT name path args -> do
    guard (Array.null args && path == String.split (Pattern ".") name)
    Map.lookup name layout.types
  _ -> Nothing

lookupCtor :: Layout -> Qualified Ident -> Maybe Ctor
lookupCtor layout name = Map.lookup name layout.constructors

-- One ordinary DU group supports recursive fields and forward references.
-- Native fields were all proven during fromModule; there is no obj fallback.
printDeclarations :: Layout -> String
printDeclarations layout = String.joinWith "\n" (Array.mapWithIndex printDecl layout.declarations)
  where
  printDecl index decl =
    (if index == 0 then "type " else "and ") <> decl.name <> " =\n"
      <> String.joinWith "\n" (map printCtor decl.constructors)
  printCtor ctor = "    | " <> ctor.name
    <> if Array.null ctor.nativeFields then ""
       else " of " <> String.joinWith " * " ctor.nativeFields

validModulePart :: String -> Boolean
validModulePart name = case CU.uncons name of
  Just { head } -> head >= 'A' && head <= 'Z' && validIdentifier name
  Nothing -> false

validIdentifier :: String -> Boolean
validIdentifier name = case CU.uncons name of
  Just { head, tail } -> letter head && all (\char -> letter char || char >= '0' && char <= '9') (CU.toCharArray tail)
  Nothing -> false
  where
  letter char = char >= 'a' && char <= 'z' || char >= 'A' && char <= 'Z' || char == '_'
