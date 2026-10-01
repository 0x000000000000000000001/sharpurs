-- | Select private native workers and expose the closed source-call recognizer.
-- | Public bindings keep their object ABI; a worker alone never authorizes an
-- | opaque callback to cross into the native path.
module Sharpurs.ThunkKernel (ThunkModule, Helper, Worker, prepareModule, fromExpr) where

import Prelude

import Control.Alternative (guard)
import Data.Array as Array
import Data.Foldable (foldMap)
import Data.Map (Map)
import Data.Map as Map
import Data.Maybe (Maybe(..))
import Data.Newtype (unwrap)
import Data.Tuple (Tuple(..))
import PureScript.Backend.Optimizer.Convert (BackendModule)
import PureScript.Backend.Optimizer.CoreFn (Ann, Ident, Module(..), Qualified(..))
import PureScript.Backend.Optimizer.CoreFn as C
import Sharpurs.AdtLayout (validIdentifier)
import Sharpurs.Analysis.Source as Source
import Sharpurs.FsAst (FsDecl, FsExpr)
import Sharpurs.Names as Names
import Sharpurs.ThunkKernel.Analysis as Analysis
import Sharpurs.ThunkKernel.Call as Call
import Sharpurs.ThunkKernel.Emit as Emit
import Sharpurs.ThunkKernel.Helpers as Helpers
import Sharpurs.ThunkKernel.Lower as Lower

type Helper = Analysis.Helper
type Worker = Analysis.Worker
type Candidate = { worker :: Worker, declaration :: FsDecl }

type ThunkModule =
  { declarations :: Array FsDecl
  , nativeNames :: Array Ident
  , helpers :: Map (Qualified Ident) Helper
  , workers :: Map (Qualified Ident) Worker
  }

prepareModule :: Module Ann -> BackendModule -> Maybe ThunkModule
prepareModule (Module source) backend = do
  guard (source.name == backend.name)
  let
    bindings = Array.concatMap Source.bindings source.decls
    qualify = Qualified (Just source.name)
    publicName = Names.inModule (unwrap source.name) <<< unwrap
    publicNames = map publicName
      (map _.name bindings <> Array.fromFoldable (Map.keys source.foreign))
    localNames = foldMap (Analysis.sourceNames <<< _.expr) bindings
    helpers = Helpers.select source.name bindings backend
    select :: Source.SourceBinding Ann -> Maybe Candidate
    select binding = do
      guard binding.singleton
      group <- Array.find (Array.any (\(Tuple name _) -> name == binding.name) <<< _.bindings) backend.bindings
      guard (group.recursive == binding.recursive && (not group.recursive || Array.length group.bindings == 1))
      expression <- Analysis.lookupExpression binding.name backend
      worker <- Analysis.worker (qualify binding.name) (publicName binding.name <> "_thunk_native") helpers binding expression
      lowered <- Lower.binding worker helpers expression
      declaration <- Emit.worker worker binding.recursive lowered
      guard (validIdentifier worker.nativeName && not (Array.elem worker.nativeName (publicNames <> localNames)))
      pure { worker, declaration }
    selected = Array.mapMaybe select bindings
  guard (not (Array.null selected) && Analysis.unique publicNames && Analysis.unique (map (_.nativeName <<< _.worker) selected))
  pure
    { declarations: map _.declaration selected
    , nativeNames: map (C.unQualified <<< _.name <<< _.worker) selected
    , workers: Map.fromFoldable (map (\item -> Tuple item.worker.name item.worker) selected)
    , helpers
    }

fromExpr :: ThunkModule -> C.Expr Ann -> Maybe FsExpr
fromExpr selected = Call.fromExpr { helpers: selected.helpers, workers: selected.workers }
