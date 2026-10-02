-- | Shared F# value-name qualification. Flatten the owner before escaping the
-- | complete identifier; source identities remain Qualified Ident values.
module Sharpurs.Names
  ( inModule
  , binding
  , qualified
  , generatedModule
  , ffiModule
  , nativeMember
  , constructor
  , recursive
  , direct
  , directApply
  , adtNative
  , guarded
  , thunkNative
  ) where

import Prelude

import Data.Maybe (Maybe(..))
import Data.Newtype (unwrap)
import PureScript.Backend.Optimizer.CoreFn (Ident, Qualified(..), unQualified)
import Sharpurs.FsAst (modulePrefix, sanitizeName)

inModule :: String -> String -> String
inModule owner name = sanitizeName (modulePrefix owner <> "_" <> name)

binding :: Maybe String -> String -> String
binding owner name = case owner of
  Just current -> inModule current name
  Nothing -> sanitizeName name

-- An explicit qualifier wins; otherwise use the supplied current module.
-- Passing Nothing preserves an unqualified reference to a lexical binder.
qualified :: Maybe String -> Qualified Ident -> String
qualified currentModule ident =
  let name = unwrap (unQualified ident)
  in case ident of
    Qualified (Just owner) _ -> inModule (unwrap owner) name
    Qualified Nothing _ -> binding currentModule name

generatedModule :: String -> String
generatedModule owner = inModule "PureScript" (modulePrefix owner)

ffiModule :: String -> String
ffiModule owner = inModule owner "FFI"

-- Native members keep their spelling/case. Quoting, unlike sanitizing, refers
-- to the original F#/C# symbol even when it is a reserved/generated local name.
nativeMember :: String -> String
nativeMember name = if sanitizeName name == name then name else "``" <> name <> "``"

-- Inputs below are already qualified and escaped public names. Apply suffixes
-- afterwards, identically in declarations, calls and collision checks.
constructor :: String -> String
constructor name = name <> "usd_Ctor"

recursive :: String -> String
recursive name = name <> "_tco"

direct :: String -> String
direct name = name <> "_direct"

directApply :: String -> String
directApply = guarded <<< direct

adtNative :: String -> String
adtNative name = name <> "_adt_native"

-- The input is a native/direct worker name, rather than a public source name.
guarded :: String -> String
guarded name = name <> "_apply"

thunkNative :: String -> String
thunkNative name = name <> "_thunk_native"
