module Sharpurs.IntArithmetic (fromExpr) where

import Prelude

import Control.Alternative (guard)
import Data.Maybe (Maybe(..))
import Data.Tuple (Tuple(..))
import PureScript.Backend.Optimizer.CoreFn (Ann(..), Expr(..), ExprType, Ident(..), ModuleName(..), Qualified(..), exprAnn)
import PureScript.Backend.Optimizer.CoreFn as C
import PureScript.Backend.Optimizer.Syntax (BackendOperatorNum(..))

type Arithmetic =
  { operator :: BackendOperatorNum
  , className :: String
  , dictionaryName :: String
  }

-- Int operands do not establish the behavior of an arbitrary dictionary.
-- Require the canonical operation, dictionary and every application type.
-- Keep both operands intact for one evaluation each, from left to right.
fromExpr :: Expr Ann -> Maybe
  { operator :: BackendOperatorNum
  , left :: Expr Ann
  , right :: Expr Ann
  }
fromExpr = case _ of
  ExprApp resultAnn (ExprApp partialAnn (ExprApp appliedAnn head dictionary) left) right -> do
    guard (hasType C.Int resultAnn)
    guard (hasType (C.Func [ C.Int ] C.Int) partialAnn)
    guard (hasType (C.Func [ C.Int, C.Int ] C.Int) appliedAnn)
    guard (hasType C.Int (exprAnn left) && hasType C.Int (exprAnn right))
    arithmetic <- fromHead head
    guard case dictionary of
      ExprVar ann (Qualified (Just (ModuleName moduleName)) (Ident name)) ->
        moduleName == "Data." <> arithmetic.className
          && name == arithmetic.dictionaryName
          && hasType
            (C.ADT (moduleName <> "." <> arithmetic.className)
              [ "Data", arithmetic.className, arithmetic.className ] [ C.Int ]) ann
      _ -> false
    pure { operator: arithmetic.operator, left, right }
  _ -> Nothing

fromHead :: Expr Ann -> Maybe Arithmetic
fromHead = case _ of
  ExprVar ann qualified -> do
    arithmetic <- fromQualified qualified
    guard (hasType (signature arithmetic C.Int) ann || isPolymorphic arithmetic ann)
    pure arithmetic
  ExprTypeApp ann (ExprVar genericAnn qualified) C.Int -> do
    arithmetic <- fromQualified qualified
    -- Instantiate exactly the single quantified variable. Contradictory or
    -- additional TypeApp nodes must retain the generic translation.
    guard (isPolymorphic arithmetic genericAnn)
    guard (hasType (signature arithmetic C.Int) ann)
    pure arithmetic
  _ -> Nothing

fromQualified :: Qualified Ident -> Maybe Arithmetic
fromQualified = case _ of
  Qualified (Just (ModuleName "Data.Semiring")) (Ident "add") ->
    Just { operator: OpAdd, className: "Semiring", dictionaryName: "semiringInt" }
  Qualified (Just (ModuleName "Data.Ring")) (Ident "sub") ->
    Just { operator: OpSubtract, className: "Ring", dictionaryName: "ringInt" }
  _ -> Nothing

signature :: Arithmetic -> ExprType -> ExprType
signature arithmetic ty =
  C.ConstrainedType [ Tuple [ "Data", arithmetic.className, arithmetic.className ] [ ty ] ]
    (C.Func [ ty, ty ] ty)

isPolymorphic :: Arithmetic -> Ann -> Boolean
isPolymorphic arithmetic (Ann ann) = case ann.type of
  Just (C.ForAll [ variable ] body) -> body == signature arithmetic (C.TypeVar variable)
  _ -> false

hasType :: ExprType -> Ann -> Boolean
hasType expected (Ann ann) = ann.type == Just expected
