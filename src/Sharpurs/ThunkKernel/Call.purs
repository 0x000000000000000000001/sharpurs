-- | Prove a source force call has closed, supported seeds before routing it to
-- | native workers. Opaque callbacks keep the generic invocation boundary.
module Sharpurs.ThunkKernel.Call (Registry, fromExpr) where

import Prelude

import Control.Alternative (guard)
import Data.Array as Array
import Data.Map (Map)
import Data.Map as Map
import Data.Maybe (Maybe(..))
import Data.Traversable (traverse)
import Data.Tuple (Tuple(..))
import PureScript.Backend.Optimizer.CoreFn (Ann, Ident, Qualified(..))
import PureScript.Backend.Optimizer.CoreFn as C
import PureScript.Backend.Optimizer.Syntax as S
import Sharpurs.Analysis.Source as Source
import Sharpurs.FsAst (FsExpr)
import Sharpurs.IntArithmetic as IntArithmetic
import Sharpurs.ThunkKernel.Analysis (Helper(..), Worker, arrow, thunk)
import Sharpurs.ThunkKernel.Analysis as Analysis
import Sharpurs.ThunkKernel.Emit as Emit
import Sharpurs.ThunkKernel.Helpers as Helpers

type Registry = { helpers :: Map (Qualified Ident) Helper, workers :: Map (Qualified Ident) Worker }

fromExpr :: Registry -> C.Expr Ann -> Maybe FsExpr
fromExpr selected expr = do
  guard (Source.annotation expr == Just C.Int)
  case expr of
    C.ExprApp _ fn value -> do
      name <- Helpers.reference Force fn
      guard (Map.lookup name selected.helpers == Just Force)
      Emit.boxForced <$> sourceThunk selected Map.empty value
    _ -> Nothing

sourceThunk :: Registry -> Map Ident String -> C.Expr Ann -> Maybe String
sourceThunk selected locals expr = do
  guard (Source.annotation expr == Just thunk)
  case expr of
    C.ExprAbs _ name body -> do
      let plan = Analysis.planCaptures locals name body
      values <- traverse (\capture -> do
        value <- localInt locals capture.name
        pure { target: capture.target, value }) plan.captures
      bodyCode <- sourceInt (Map.insert name Emit.unitValue plan.locals) body
      pure (Emit.capturedThunk values bodyCode)
    C.ExprApp _ fn value | Just name <- Helpers.reference Identity fn -> do
      guard (Map.lookup name selected.helpers == Just Identity)
      sourceThunk selected locals value
    _ -> do
      let flat = Source.flattenApp expr
      name <- case flat.fn of
        C.ExprVar _ name -> Just name
        _ -> Nothing
      worker <- Map.lookup name selected.workers
      guard (Source.annotation flat.fn == Just (arrow worker.args worker.result))
      guard (Array.length flat.args == Array.length worker.args)
      Analysis.validateApplications worker.args worker.result expr
      values <- traverse (\(Tuple ty arg) -> if ty == thunk then sourceThunk selected locals arg
        else if ty == C.Int then sourceInt locals arg else Nothing) (Array.zip worker.args flat.args)
      pure (Emit.call worker.nativeName values)

sourceInt :: Map Ident String -> C.Expr Ann -> Maybe String
sourceInt locals expr = do
  guard (Source.annotation expr == Just C.Int)
  case expr of
    C.ExprLit _ (C.LitInt n) -> Just (Emit.intLiteral n)
    C.ExprVar _ (Qualified Nothing name) -> localInt locals name
    _ -> do
      arithmetic <- IntArithmetic.fromExpr expr
      a <- sourceInt locals arithmetic.left
      b <- sourceInt locals arithmetic.right
      symbol <- case arithmetic.operator of
        S.OpAdd -> Just "+"
        S.OpSubtract -> Just "-"
        _ -> Nothing
      pure (Emit.binary symbol a b)

localInt :: Map Ident String -> Ident -> Maybe String
localInt locals name = case Map.lookup name locals of
  Just value | value == Emit.unitValue -> Nothing
  Just value -> Just value
  Nothing -> Just (Emit.capturedInt name)
