module Sharpurs.IntComparison (fromExpr) where

import Prelude

import Control.Alternative (guard)
import Data.Maybe (Maybe(..))
import Data.Tuple (Tuple(..))
import PureScript.Backend.Optimizer.CoreFn (Ann(..), Expr(..), ExprType, Ident(..), ModuleName(..), Qualified(..), exprAnn)
import PureScript.Backend.Optimizer.CoreFn as C
import PureScript.Backend.Optimizer.Syntax (BackendOperatorOrd(..))

-- Recognize only the canonical Ord Int dictionary at a fully saturated call.
-- Int arguments alone do not establish the ordering of a custom dictionary.
-- Keep both operands intact so the emitter evaluates each once, left to right.
fromExpr :: Expr Ann -> Maybe
  { operator :: BackendOperatorOrd
  , left :: Expr Ann
  , right :: Expr Ann
  }
fromExpr = case _ of
  ExprApp resultAnn (ExprApp partialAnn (ExprApp appliedAnn head dictionary) left) right -> do
    guard (hasType C.Boolean resultAnn)
    guard (hasType (C.Func [ C.Int ] C.Boolean) partialAnn)
    guard (hasType (C.Func [ C.Int, C.Int ] C.Boolean) appliedAnn)
    guard (hasType C.Int (exprAnn left) && hasType C.Int (exprAnn right))
    guard case dictionary of
      ExprVar ann (Qualified (Just (ModuleName "Data.Ord")) (Ident "ordInt")) ->
        hasType (C.ADT "Data.Ord.Ord" [ "Data", "Ord", "Ord" ] [ C.Int ]) ann
      _ -> false
    operator <- fromHead head
    pure { operator, left, right }
  _ -> Nothing

fromHead :: Expr Ann -> Maybe BackendOperatorOrd
fromHead = case _ of
  ExprVar ann qualified -> do
    guard (hasType (comparisonSignature C.Int) ann || isPolymorphic ann)
    fromQualified qualified
  ExprTypeApp ann (ExprVar genericAnn qualified) C.Int -> do
    -- TypeApp instantiates the single quantified variable of the canonical
    -- signature. Extra or conflicting type applications are not discarded.
    guard (isPolymorphic genericAnn)
    guard (hasType (comparisonSignature C.Int) ann)
    fromQualified qualified
  _ -> Nothing

fromQualified :: Qualified Ident -> Maybe BackendOperatorOrd
fromQualified = case _ of
  Qualified (Just (ModuleName "Data.Ord")) (Ident "lessThan") -> Just OpLt
  Qualified (Just (ModuleName "Data.Ord")) (Ident "greaterThan") -> Just OpGt
  _ -> Nothing

comparisonSignature :: ExprType -> ExprType
comparisonSignature ty =
  C.ConstrainedType [ Tuple [ "Data", "Ord", "Ord" ] [ ty ] ]
    (C.Func [ ty, ty ] C.Boolean)

isPolymorphic :: Ann -> Boolean
isPolymorphic (Ann ann) = case ann.type of
  Just (C.ForAll [ variable ] body) -> body == comparisonSignature (C.TypeVar variable)
  _ -> false

hasType :: ExprType -> Ann -> Boolean
hasType expected (Ann ann) = ann.type == Just expected
