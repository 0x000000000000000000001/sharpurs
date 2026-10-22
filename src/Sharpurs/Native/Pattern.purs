-- | Source binders to native patterns. Only closed layouts with known fields
-- | are accepted; unsupported binders reject the whole pattern.
module Sharpurs.Native.Pattern
  ( NativePattern(..)
  , fromBinder
  , variables
  ) where

import Prelude

import Data.Array as Array
import Data.Maybe (Maybe(..))
import Data.Traversable (traverse)
import PureScript.Backend.Optimizer.CoreFn (Ann, Binder(..), Ident(..), Literal(..))
import Sharpurs.FsAst (sanitizeName)
import Sharpurs.Native.Layout as NativeLayout
import Sharpurs.Native.Types (Layout)

data NativePattern
  = PatternWildcard
  | PatternVariable String
  | PatternNamed String NativePattern
  | PatternInt Int
  | PatternBoolean Boolean
  | PatternString String
  | PatternConstructor String (Array NativePattern)

derive instance Eq NativePattern

fromBinder :: Layout -> Binder Ann -> Maybe NativePattern
fromBinder layout = case _ of
  BinderNull _ -> Just PatternWildcard
  BinderVar _ (Ident name) -> Just (PatternVariable (sanitizeName name))
  BinderNamed _ (Ident name) inner -> do
    pattern <- fromBinder layout inner
    pure (PatternNamed (sanitizeName name) pattern)
  BinderLit _ literal -> case literal of
    LitInt value -> Just (PatternInt value)
    LitBoolean value -> Just (PatternBoolean value)
    LitString value -> Just (PatternString value)
    _ -> Nothing
  BinderConstructor _ _ qualified binders -> do
    constructor <- NativeLayout.lookupConstructor layout qualified
    patterns <- traverse (fromBinder layout) binders
    if Array.length patterns == Array.length constructor.fields
      then Just (PatternConstructor constructor.name patterns)
      else Nothing

variables :: NativePattern -> Array String
variables = case _ of
  PatternWildcard -> []
  PatternVariable name -> [ name ]
  PatternNamed name inner -> Array.cons name (variables inner)
  PatternInt _ -> []
  PatternBoolean _ -> []
  PatternString _ -> []
  PatternConstructor _ patterns -> Array.concatMap variables patterns
