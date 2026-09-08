module Sharpurs.Optimized (fromBinding) where

import Prelude

import Control.Alternative (guard)
import Data.Array as Array
import Data.Array.NonEmpty as NonEmptyArray
import Data.Foldable (any, foldr)
import Data.Map (Map)
import Data.Map as Map
import Data.Maybe (Maybe(..))
import Data.Newtype (unwrap)
import Data.String as String
import Data.String.Pattern (Pattern(..), Replacement(..))
import Data.Traversable (traverse)
import Data.Tuple (Tuple(..))
import PureScript.Backend.Optimizer.CoreFn (ExprType, Ident(..), Qualified(..))
import PureScript.Backend.Optimizer.CoreFn as C
import PureScript.Backend.Optimizer.Semantics (NeutralExpr(..))
import PureScript.Backend.Optimizer.Syntax (Level(..))
import PureScript.Backend.Optimizer.Syntax as S
import Sharpurs.FsAst (FsExpr(..), sanitizeName)
import Sharpurs.IntKernel (fromLocal)
import Sharpurs.IntKernel.CodeGen (printLocalKernel)
import Sharpurs.Printer (printExpr)

type Lowered = { expr :: FsExpr, hasKernel :: Boolean }

-- Keep the existing obj ABI around a closed native loop. Unsupported syntax
-- rejects the complete binding, and bindings without a native loop stay on the
-- established CoreFn generator.
fromBinding :: NeutralExpr -> Maybe FsExpr
fromBinding expression = do
  result <- lower Map.empty Nothing expression
  guard result.hasKernel
  pure result.expr

lower :: Map Level ExprType -> Maybe ExprType -> NeutralExpr -> Maybe Lowered
lower locals expected expression@(NeutralExpr syntax) =
  case fromLocal (Map.keys (Map.filter (_ == C.Int) locals) # Array.fromFoldable) expression of
    Just kernel -> do
      guard case expected of
        Just ty -> ty == C.Int
        Nothing -> true
      pure { expr: FsIdent (printLocalKernel kernel), hasKernel: true }
    Nothing -> case syntax of
      S.Typed ty body -> do
        -- Repeated function annotations must agree before they establish the
        -- types of lambda parameters used at the native boundary.
        guard case expected of
          Just previous@(C.Func _ _) -> previous == ty
          _ -> true
        lower locals (Just ty) body
      S.TypeApp fn _ -> do
        guard (isGlobalReference fn)
        lower locals expected fn
      S.Var (Qualified (Just moduleName) (Ident name)) -> pure
        { expr: FsIdent (sanitizeName (String.replaceAll (Pattern ".") (Replacement "_") (unwrap moduleName) <> "_" <> name))
        , hasKernel: false
        }
      S.Local _ level -> do
        guard (Map.member level locals)
        pure { expr: FsIdent (localName level), hasKernel: false }
      S.Lit (C.LitInt value) -> pure { expr: FsIdent ("(box (" <> show value <> "))"), hasKernel: false }
      S.Lit (C.LitString value) -> pure { expr: FsLitString value, hasKernel: false }
      S.Lit (C.LitBoolean value) -> pure { expr: FsLitBool value, hasKernel: false }
      S.App fn args -> do
        head <- lower locals Nothing fn
        arguments <- traverse (lower locals Nothing) (NonEmptyArray.toArray args)
        pure
          { expr: FsApp head.expr (map _.expr arguments)
          , hasKernel: head.hasKernel || any _.hasKernel arguments
          }
      S.Abs binders body -> do
        signature <- case expected of
          Just (C.Func args result) -> Just { args, result }
          _ -> Nothing
        let
          levels = map (\(Tuple _ level) -> level) (NonEmptyArray.toArray binders)
          count = Array.length levels
        guard (count <= Array.length signature.args)
        guard (Array.length (Array.nub levels) == count)
        guard (not (any (\level@(Level n) -> n < 0 || Map.member level locals) levels))
        let
          scope = Map.union (Map.fromFoldable (Array.zip levels signature.args)) locals
          remaining = Array.drop count signature.args
          nextType = Just (if Array.null remaining then signature.result else C.Func remaining signature.result)
        result <- lower scope nextType body
        let
          lambda = foldr (\level inner -> "(box (fun (" <> localName level <> ": obj) -> " <> inner <> "))")
            (printExpr result.expr) levels
        pure { expr: FsIdent lambda, hasKernel: result.hasKernel }
      _ -> Nothing

localName :: Level -> String
localName (Level level) = "sharpurs_o_" <> show level

isGlobalReference :: NeutralExpr -> Boolean
isGlobalReference (NeutralExpr syntax) = case syntax of
  S.Var (Qualified (Just _) _) -> true
  S.Typed _ inner -> isGlobalReference inner
  S.TypeApp inner _ -> isGlobalReference inner
  _ -> false
