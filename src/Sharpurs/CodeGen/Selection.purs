-- | Implementation policy, before rendering: collect candidates, reserve direct
-- | entries only for emitted bindings, and choose one route for each source node.
module Sharpurs.CodeGen.Selection
  ( BindingCandidates
  , emptyCandidates
  , fromBackend
  , ModulePlan
  , prepareModule
  , BindingPlan(..)
  , forBinding
  , ExpressionPlan(..)
  , forExpression
  ) where

import Prelude

import Control.Alternative (guard)
import Data.Array as Array
import Data.Map (Map)
import Data.Map as Map
import Data.Maybe (Maybe(..), isJust)
import Data.Newtype (unwrap)
import Data.Set (Set)
import Data.Set as Set
import Data.Tuple (Tuple(..))
import PureScript.Backend.Optimizer.Convert (BackendModule)
import PureScript.Backend.Optimizer.CoreFn (Ann, Bind(..), Binding(..), Expr, Ident, Module(..), Qualified(..))
import PureScript.Backend.Optimizer.Syntax (BackendOperatorOrd(..), BackendOperatorNum(..))
import Sharpurs.Analysis.Source as Source
import Sharpurs.CodeGen.Boxed as Boxed
import Sharpurs.CodeGen.Context (ModuleEnv)
import Sharpurs.ConstructorCall as ConstructorCall
import Sharpurs.DirectCall as DirectCall
import Sharpurs.FsAst (FsDecl, FsExpr)
import Sharpurs.IntArithmetic as IntArithmetic
import Sharpurs.IntComparison as IntComparison
import Sharpurs.IntKernel (IntKernel)
import Sharpurs.IntKernel as IntKernel
import Sharpurs.Names as Names
import Sharpurs.Optimized as Optimized
import Sharpurs.ThunkKernel as ThunkKernel

-- Recognition does not authorize replacement of an individual member of a
-- source recursive group. forBinding applies that group-level policy below.
type BindingCandidates =
  { kernels :: Map Ident IntKernel
  , expressions :: Map Ident FsExpr
  }

emptyCandidates :: BindingCandidates
emptyCandidates = { kernels: Map.empty, expressions: Map.empty }

fromBackend :: BackendModule -> BindingCandidates
fromBackend backend =
  let bindings = Array.concatMap _.bindings backend.bindings
  in
    { kernels: Map.fromFoldable (Array.mapMaybe
        (\(Tuple name expr) -> Tuple name <$> IntKernel.fromBinding (Qualified (Just backend.name) name) expr)
        bindings)
    , expressions: Map.fromFoldable (Array.mapMaybe
        (\(Tuple name expr) -> Tuple name <$> Optimized.fromBinding expr)
        bindings)
    }

type ModulePlan = { env :: ModuleEnv, bindings :: Array BindingPlan }

data BindingPlan
  = NativeAdt FsDecl
  | NativeInt { name :: Ident, kernel :: IntKernel }
  | OptimizedExpression { name :: Ident, expression :: FsExpr }
  | DirectFunction DirectCall.Candidate
  | GenericBinding (Bind Ann)

-- Registration and emission use the same replacement decision. A direct call
-- must never refer to a helper suppressed by a higher-priority implementation.
prepareModule :: ModuleEnv -> BindingCandidates -> Module Ann -> ModulePlan
prepareModule env candidates (Module source) =
  let
    bindingNames = map _.name (Array.concatMap Source.bindings source.decls)
    emittedName = Names.inModule (unwrap source.name) <<< unwrap
    sourceNames = Set.fromFoldable (map emittedName
      (bindingNames <> Array.fromFoldable (Map.keys source.foreign)))
    select binding = do
      entry <- DirectCall.fromBinding (map _.layout env.native) binding
      guard (not (isJust (replacement env candidates binding)))
      guard (not (directCollision sourceNames (emittedName entry.name) entry))
      pure (Tuple (Qualified (Just source.name) entry.name) entry)
    selectedEnv = env { direct = Map.fromFoldable (Array.mapMaybe select source.decls) }
  in { env: selectedEnv, bindings: map (forBinding selectedEnv candidates) source.decls }

-- Compare emitted identifiers, including escaping, against all source bindings,
-- foreign declarations and the candidate's own parameters.
directCollision :: Set String -> String -> DirectCall.Candidate -> Boolean
directCollision sourceNames name entry = Array.any
  (\helper -> Set.member helper sourceNames || Array.elem helper entry.args)
  [ Names.direct name, Names.directApply name ]

-- Binding priority: native ADT > Int kernel > optimized NonRec expression >
-- direct NonRec function > generic source group. Mutual groups stay intact.
forBinding :: ModuleEnv -> BindingCandidates -> Bind Ann -> BindingPlan
forBinding env candidates binding = case replacement env candidates binding of
  Just selected -> selected
  Nothing -> case binding of
    NonRec (Binding _ name _) ->
      case Array.find (\entry -> entry.name == name) (Array.fromFoldable (Map.values env.direct)) of
        Just entry -> DirectFunction entry
        Nothing -> GenericBinding binding
    _ -> GenericBinding binding

replacement :: ModuleEnv -> BindingCandidates -> Bind Ann -> Maybe BindingPlan
replacement env candidates binding = do
  name <- case binding of
    NonRec (Binding _ name _) -> Just name
    Rec [ Binding _ name _ ] -> Just name
    _ -> Nothing
  case env.native >>= \selected -> Map.lookup name selected.bindings of
    Just declaration -> Just (NativeAdt declaration)
    Nothing -> case Map.lookup name candidates.kernels of
      Just kernel -> Just (NativeInt { name, kernel })
      Nothing -> case binding of
        NonRec _ -> map (\expression -> OptimizedExpression { name, expression })
          (Map.lookup name candidates.expressions)
        _ -> Nothing

data ExpressionPlan
  = NativeThunk FsExpr
  | DirectInvocation { name :: Ident, args :: Array (Expr Ann) }
  | IntBinary { operator :: Boxed.IntOperation, left :: Expr Ann, right :: Expr Ann }
  | ConstructorInvocation ConstructorCall.Call
  | GenericExpression (Expr Ann)

-- Expression priority is separate from whole-binding replacement. Only the
-- chosen route translates operands; recognizers preserve their source order.
-- Thunk > direct call > Int comparison > Int arithmetic > constructor > generic.
forExpression :: ModuleEnv -> Maybe String -> Expr Ann -> ExpressionPlan
forExpression env currentModule expr =
  case env.thunks >>= \selected -> ThunkKernel.fromExpr selected expr of
    Just native -> NativeThunk native
    Nothing -> case DirectCall.fromCall env.direct expr of
      Just call -> DirectInvocation { name: call.candidate.name, args: call.args }
      Nothing -> comparison expr
  where
  comparison expression = case IntComparison.fromExpr expression of
    Just call -> case call.operator of
      OpLt -> IntBinary { operator: Boxed.LessThan, left: call.left, right: call.right }
      OpGt -> IntBinary { operator: Boxed.GreaterThan, left: call.left, right: call.right }
      _ -> constructor expression
    Nothing -> arithmetic expression

  arithmetic expression = case IntArithmetic.fromExpr expression of
    Just call -> case call.operator of
      OpAdd -> IntBinary { operator: Boxed.Add, left: call.left, right: call.right }
      OpSubtract -> IntBinary { operator: Boxed.Subtract, left: call.left, right: call.right }
      _ -> constructor expression
    Nothing -> constructor expression

  constructor expression = case ConstructorCall.fromExpr env.arities currentModule expression of
    Just call -> ConstructorInvocation call
    Nothing -> GenericExpression expression
