-- | Recognize identity/delay and force implementations using source references
-- | and optimized bodies together. A matching function type alone is not proof.
module Sharpurs.ThunkKernel.Helpers (select, reference) where

import Prelude

import Control.Alternative (guard)
import Data.Array as Array
import Data.Array.NonEmpty as NEA
import Data.Foldable (all)
import Data.Map (Map)
import Data.Map as Map
import Data.Maybe (Maybe(..))
import Data.Traversable (traverse)
import Data.Tuple (Tuple(..))
import PureScript.Backend.Optimizer.Convert (BackendModule)
import PureScript.Backend.Optimizer.CoreFn (Ann, Ident, ModuleName, Qualified(..))
import PureScript.Backend.Optimizer.CoreFn as C
import PureScript.Backend.Optimizer.Semantics (NeutralExpr(..))
import PureScript.Backend.Optimizer.Syntax as S
import Sharpurs.Analysis.Source as Source
import Sharpurs.ThunkKernel.Analysis (Helper(..), helperType, helperShape, helperResult, unitReference, peel)
import Sharpurs.ThunkKernel.Analysis as Analysis

-- Only previously recognized helpers can support an alias or a source reference.
select :: ModuleName -> Array (Source.SourceBinding Ann) -> BackendModule -> Map (Qualified Ident) Helper
select current bindings backend = Array.foldl (\known binding -> case do
  guard (binding.singleton && not binding.recursive)
  expr <- Analysis.lookupExpression binding.name backend
  sourceType <- Source.annotation binding.expr
  optimizedType <- Analysis.annotation expr
  let direct kind = helperShape kind sourceType && helperShape kind optimizedType && body kind expr && source kind binding.expr
      allowed name = case name of
        Qualified Nothing _ -> true
        _ -> unitReference name || Map.member name known
  guard (all allowed (Source.references binding.expr))
  kind <- if direct Identity then Just Identity else if direct Force then Just Force else case Analysis.strip expr, binding.expr of
    NeutralExpr (S.Var name), C.ExprVar _ sourceName | name == sourceName -> do
      kind <- Map.lookup name known
      guard (helperShape kind sourceType && helperShape kind optimizedType)
      pure kind
    _, _ -> Nothing
  pure kind
  of
    Just kind -> Map.insert (Qualified (Just current) binding.name) kind known
    Nothing -> known) Map.empty bindings

body :: Helper -> NeutralExpr -> Boolean
body kind expr = case do
  ty <- Analysis.annotation expr
  result <- helperResult kind ty
  let callback = C.Func [ C.Unit ] result
      returned = if kind == Identity then callback else result
      unquantified = case expr of
        NeutralExpr (S.Typed (C.ForAll _ _) inner) -> inner
        _ -> expr
  fn <- peel (helperType kind result) unquantified
  case fn of
    NeutralExpr (S.Abs parameters expression) -> case NEA.toArray parameters of
      [ Tuple _ parameter ] -> do
        returnedBody <- peel returned expression
        case kind, returnedBody of
          Identity, NeutralExpr (S.Local _ level) -> guard (level == parameter)
          Force, NeutralExpr (S.App target args) -> do
            targetBody <- peel callback target
            arguments <- traverse (peel C.Unit) (NEA.toArray args)
            case targetBody, arguments of
              NeutralExpr (S.Local _ level), [ NeutralExpr (S.Var name) ] -> guard (level == parameter && unitReference name)
              _, _ -> Nothing
          _, _ -> Nothing
      _ -> Nothing
    _ -> Nothing
  of
    Just _ -> true
    Nothing -> false

source :: Helper -> C.Expr Ann -> Boolean
source kind expr = case do
  ty <- Source.annotation expr
  result <- helperResult kind ty
  case expr of
    C.ExprAbs (C.Ann ann) parameter expression -> do
      let returned = if kind == Identity then C.Func [ C.Unit ] result else result
          -- The erased newtype identity alone may lack its body's annotation.
          newtypeIdentity = kind == Identity && ann.meta == Just C.IsNewtype && case expression of
            C.ExprVar _ (Qualified Nothing name) -> name == parameter && Source.annotation expression == Nothing
            _ -> false
      guard (Source.annotation expression == Just returned || newtypeIdentity)
    _ -> Nothing
  of
    Just _ -> true
    Nothing -> false

-- Only this helper boundary accepts explicit TypeApp Int; an arbitrary type
-- application never authorizes a native worker or callback invocation.
reference :: Helper -> C.Expr Ann -> Maybe (Qualified Ident)
reference kind = case _ of
  expr@(C.ExprVar _ name) -> guard (Source.annotation expr == Just (helperType kind C.Int)) $> name
  expr@(C.ExprTypeApp _ (C.ExprVar genericAnn name) C.Int) -> do
    guard (Source.annotation expr == Just (helperType kind C.Int))
    guard (Source.hasPolymorphicType (helperType kind) genericAnn)
    pure name
  _ -> Nothing
