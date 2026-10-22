-- | Typed native expression IR. Patterns live in `Native.Pattern`; this module
-- | only describes executable expressions and their native types.
module Sharpurs.Native.IR
  ( NativeExpr(..)
  , NativeCase
  , NativeParameter
  , NativePrimitive(..)
  ) where

import Prelude

import Data.Map (Map)
import Data.Maybe (Maybe(..))
import Sharpurs.Native.Pattern (NativePattern)
import Sharpurs.Native.Types (NativeType)

type NativeParameter = { name :: String, type :: NativeType }
type NativeCase =
  { patterns :: Array NativePattern
  , bindings :: Map String NativeType
  , body :: NativeExpr
  }

data NativePrimitive
  = PrimitiveAdd
  | PrimitiveSubtract
  | PrimitiveModulo
  | PrimitiveLessThan
  | PrimitiveGreaterThan
  | PrimitiveEqual

data NativeExpr
  = NativeVar String NativeType
  | NativeIntLit Int
  | NativeBooleanLit Boolean
  | NativeStringLit String
  | NativeLambda (Array NativeParameter) NativeExpr
  | NativeApply NativeExpr (Array NativeExpr)
  | NativeLet String NativeType NativeExpr NativeExpr
  | NativeMatch (Array NativeExpr) (Array NativeCase) (Maybe NativeExpr)
  | NativeConstructor String (Array NativeExpr)
  | NativeReuse String NativeExpr (Array NativeExpr)
  | NativeProject String Int NativeExpr
  | NativePrimitive NativePrimitive (Array NativeExpr)
  | NativeBox NativeExpr
