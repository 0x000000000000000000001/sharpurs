-- | Per-binding ownership summaries. These are deliberately conservative:
-- | a parameter is unique only when usage analysis proves one non-escaping read,
-- | and a result is fresh only when its constructor is created locally.
module Sharpurs.Native.Ownership
  ( Summary
  , summarize
  , parameterUnique
  , resultFresh
  ) where

import Prelude

import Data.Array as Array
import Data.Map (Map)
import Data.Map as Map
import Data.Maybe (Maybe(..))
import Data.Tuple (Tuple(..))
import Sharpurs.Native.IR (NativeExpr(..), NativeParameter)
import Sharpurs.Native.Usage as Usage

type Summary =
  { parameters :: Map String Boolean
  , resultFresh :: Boolean
  }

summarize :: Array NativeParameter -> NativeExpr -> Summary
summarize parameters body =
  { parameters: Map.fromFoldable
      (map (\parameter -> Tuple parameter.name (Usage.isUnique usage parameter.name)) parameters)
  , resultFresh: fresh body
  }
  where
  usage = Usage.usageBinding parameters body

parameterUnique :: Summary -> NativeParameter -> Boolean
parameterUnique summary parameter =
  case Map.lookup parameter.name summary.parameters of
    Just unique -> unique
    Nothing -> false

resultFresh :: Summary -> Boolean
resultFresh = _.resultFresh

fresh :: NativeExpr -> Boolean
fresh = case _ of
  NativeConstructor _ _ -> true
  NativeIntLit _ -> true
  NativeBooleanLit _ -> true
  NativeStringLit _ -> true
  NativeLambda _ _ -> true
  NativePrimitive _ _ -> true
  NativeLet _ _ _ body -> fresh body
  NativeMatch _ cases fallback ->
    Array.all (fresh <<< _.body) cases
      && case fallback of
          Nothing -> true
          Just expression -> fresh expression
  NativeVar _ _ -> false
  NativeApply _ _ -> false
  NativeReuse _ _ _ -> false
  NativeSetField _ _ _ body -> fresh body
  NativeProject _ _ _ -> false
  NativeBox _ -> false
  NativeCopy _ _ -> true
  NativeCast _ _ -> true
