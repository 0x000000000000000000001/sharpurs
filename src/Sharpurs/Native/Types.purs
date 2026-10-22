-- | Native data representations for the TAST-native backend.
-- | This module is intentionally independent of code generation and of the
-- | existing boxed ABI; `Boxed` is the explicit fallback boundary.
module Sharpurs.Native.Types
  ( NativeType(..)
  , Constructor
  , Layout
  , isBoxed
  , nativeTypeName
  ) where

import Prelude

import Data.Map (Map)
import Data.String (joinWith)
import PureScript.Backend.Optimizer.CoreFn (Ident, Qualified)

-- | A value type that can be emitted without the object ABI.
data NativeType
  = NativeInt
  | NativeBoolean
  | NativeString
  | NativeAdt String (Array NativeType)
  | NativeFunction (Array NativeType) NativeType
  | NativeBoxed

derive instance Eq NativeType

type Constructor =
  { name :: String
  , sourceName :: Qualified Ident
  , typeName :: String
  , fields :: Array NativeType
  }

type Layout =
  { moduleName :: String
  , names :: Map String String
  , types :: Map String (Array Constructor)
  , constructors :: Map (Qualified Ident) Constructor
  }

isBoxed :: NativeType -> Boolean
isBoxed = case _ of
  NativeBoxed -> true
  _ -> false

nativeTypeName :: NativeType -> String
nativeTypeName = case _ of
  NativeInt -> "int"
  NativeBoolean -> "bool"
  NativeString -> "string"
  NativeAdt name _ -> name
  NativeFunction args result -> "(" <> joinWith " -> " (map nativeTypeName (args <> [ result ])) <> ")"
  NativeBoxed -> "obj"
