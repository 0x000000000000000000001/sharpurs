-- | Source binders and the constructor chains eligible for nested matches.
module Sharpurs.CodeGen.Pattern
  ( Env
  , Chain(..)
  , translate
  , forChain
  , chainDepth
  ) where

import Prelude

import Data.Array as Array
import Data.Map (Map)
import Data.Map as Map
import Data.Maybe (Maybe(..))
import PureScript.Backend.Optimizer.CoreFn (Ann, Binder(..), Ident(..), Literal(..), Prop(..))
import Sharpurs.FsAst (FsPattern(..), sanitizeName)
import Sharpurs.Names as Names

-- Patterns only need constructor identity and the current module, independently
-- of native expression selections or the visible recursive workers.
type Env = { arities :: Map String Int, currentModule :: Maybe String }

translate :: Env -> Binder Ann -> FsPattern
translate env = case _ of
  BinderNull _ -> FsPatWildcard
  BinderVar _ (Ident name) -> FsPatIdent (sanitizeName name)
  BinderLit _ lit -> translateLiteral env lit
  BinderConstructor _ _ ident binders ->
    let name = Names.qualified env.currentModule ident
    in if Map.member name env.arities then
      FsPatCtor (Names.constructor name) (map (translate env) binders)
    else
      case Array.head binders of
        Just inner -> translate env inner
        Nothing -> FsPatWildcard
  BinderNamed _ (Ident name) inner ->
    FsPatNamed (sanitizeName name) (translate env inner)

translateLiteral :: Env -> Literal (Binder Ann) -> FsPattern
translateLiteral env = case _ of
  LitBoolean value -> FsPatLitBool value
  LitInt value -> FsPatLitInt value
  LitNumber value -> FsPatLitNumber value
  LitString value -> FsPatLitString value
  LitChar value -> FsPatLitChar value
  LitArray items -> FsPatArray (map (translate env) items)
  LitRecord [] -> FsPatWildcard
  LitRecord props ->
    FsPatRecord (map (\(Prop key value) -> { key, pattern: translate env value }) props)

-- Unary constructor chains can become a series of small F# matches. A shallow
-- subtree stays an ordinary pattern at the leaf of such a chain.
data Chain
  = ChainWildcard
  | ChainVariable String
  | ChainConstructor String (Maybe Chain)
  | ChainLeaf FsPattern

chainDepth :: Chain -> Int
chainDepth = case _ of
  ChainConstructor _ (Just inner) -> 1 + chainDepth inner
  _ -> 1

forChain :: Env -> Binder Ann -> Maybe Chain
forChain env binder = case binder of
  BinderNull _ -> Just ChainWildcard
  BinderVar _ (Ident name) -> Just (ChainVariable (sanitizeName name))
  BinderConstructor _ _ ident binders ->
    let name = Names.qualified env.currentModule ident
    in case Map.lookup name env.arities of
      Just arity | arity == Array.length binders ->
        case binders of
          [] -> Just (ChainConstructor (Names.constructor name) Nothing)
          [ inner ] -> map (\chain -> ChainConstructor (Names.constructor name) (Just chain)) (forChain env inner)
          _ -> shallowLeaf env binder
      _ -> case binders of
        [ inner ] -> forChain env inner
        _ -> shallowLeaf env binder
  _ -> shallowLeaf env binder

-- A small nested pattern is cheap. Reject larger non-chain subtrees so the
-- whole case can use ordinary matching instead of an incomplete chain plan.
shallowLeaf :: Env -> Binder Ann -> Maybe Chain
shallowLeaf env binder =
  if constructorDepth env binder <= maxLeafDepth then
    Just (ChainLeaf (translate env binder))
  else Nothing

maxLeafDepth :: Int
maxLeafDepth = 2

-- Constructor nesting inspected for the chain path; newtype layers are erased.
constructorDepth :: Env -> Binder Ann -> Int
constructorDepth env = case _ of
  BinderConstructor _ _ ident binders ->
    case Map.lookup (Names.qualified env.currentModule ident) env.arities of
      Just arity | arity == Array.length binders ->
        if Array.null binders then 1
        else 1 + Array.foldl (\depth child -> max depth (constructorDepth env child)) 0 binders
      _ -> case binders of
        [ inner ] -> constructorDepth env inner
        _ -> 1 + Array.foldl (\depth child -> max depth (constructorDepth env child)) 0 binders
  BinderNamed _ _ inner -> constructorDepth env inner
  _ -> 0
