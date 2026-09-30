-- | Module-wide selections and the recursive workers visible in an expression.
module Sharpurs.CodeGen.Context
  ( ModuleEnv
  , Context
  , RecursiveScope(..)
  , RecursiveFunction
  , boxedModule
  , forModule
  , registerRecursive
  , localRecursive
  , recursiveCall
  , qualifiedName
  ) where

import Prelude

import Data.Map (Map)
import Data.Map as Map
import Data.Maybe (Maybe(..))
import Data.Newtype (unwrap)
import Data.Set (Set)
import Data.Set as Set
import Data.String as String
import Data.String.Pattern (Pattern(..), Replacement(..))
import PureScript.Backend.Optimizer.CoreFn (Ident, Qualified(..), unQualified)
import Sharpurs.AdtKernel (UnaryModule)
import Sharpurs.DirectCall as DirectCall
import Sharpurs.FsAst (sanitizeName)
import Sharpurs.ThunkKernel as ThunkKernel

-- Constructor identity/layout remains available for patterns even when
-- expression calls must cross the public object ABI of a native producer.
type ModuleEnv =
  { arities :: Map String Int
  , wrappers :: Set String
  , native :: Maybe UnaryModule
  , direct :: Map (Qualified Ident) DirectCall.Candidate
  , thunks :: Maybe ThunkKernel.ThunkModule
  }

type Context =
  { moduleEnv :: ModuleEnv
  , currentModule :: Maybe String
  , recursive :: Map String RecursiveFunction
  }

-- Top-level workers have a public curried alias. Local workers only have
-- their uncurried entry, so partial calls and value references need adapters.
data RecursiveScope = TopLevel | Local

type RecursiveFunction =
  { arity :: Int
  , scope :: RecursiveScope
  , worker :: String
  }

boxedModule :: Map String Int -> ModuleEnv
boxedModule arities = { arities, wrappers: Set.empty, native: Nothing, direct: Map.empty, thunks: Nothing }

forModule :: ModuleEnv -> Maybe String -> Context
forModule moduleEnv currentModule = { moduleEnv, currentModule, recursive: Map.empty }

-- Keys are escaped binding names; arities count actual parameters, never scope.
-- Recursive values with no parameters have no worker and keep ordinary lookup.
registerRecursive :: RecursiveScope -> String -> Int -> Context -> Context
registerRecursive scope name arity context =
  if arity > 0 then
    context { recursive = Map.insert name { arity, scope, worker: name <> "_tco" } context.recursive }
  else context

localRecursive :: Context -> Qualified Ident -> Maybe RecursiveFunction
localRecursive context ident = case Map.lookup (localName ident) context.recursive of
  Just entry@{ scope: Local } -> Just entry
  _ -> Nothing

-- Keep the source generator's lookup order: local workers first, then the
-- module-qualified entry. CoreFn has already renamed lexical binders.
recursiveCall :: Context -> Qualified Ident -> Maybe RecursiveFunction
recursiveCall context ident = case Map.lookup (localName ident) context.recursive of
  Just entry -> Just entry
  Nothing -> Map.lookup (qualifiedName context.currentModule ident) context.recursive

localName :: Qualified Ident -> String
localName = sanitizeName <<< unwrap <<< unQualified

-- An explicit qualifier wins; otherwise use the supplied current module.
-- Passing Nothing preserves an unqualified reference to a lexical binder.
qualifiedName :: Maybe String -> Qualified Ident -> String
qualifiedName currentModule ident =
  let
    owner = case ident of
      Qualified (Just moduleName) _ -> Just (unwrap moduleName)
      Qualified Nothing _ -> currentModule
    prefix = case owner of
      Just name -> String.replaceAll (Pattern ".") (Replacement "_") name <> "_"
      Nothing -> ""
  in sanitizeName (prefix <> unwrap (unQualified ident))
