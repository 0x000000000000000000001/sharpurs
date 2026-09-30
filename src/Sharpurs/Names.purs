-- | Shared F# value-name qualification. Flatten the owner before escaping the
-- | complete identifier; source identities remain Qualified Ident values.
module Sharpurs.Names (inModule, qualified) where

import Prelude

import Data.Maybe (Maybe(..))
import Data.Newtype (unwrap)
import PureScript.Backend.Optimizer.CoreFn (Ident, Qualified(..), unQualified)
import Sharpurs.FsAst (modulePrefix, sanitizeName)

inModule :: String -> String -> String
inModule owner name = sanitizeName (modulePrefix owner <> "_" <> name)

-- An explicit qualifier wins; otherwise use the supplied current module.
-- Passing Nothing preserves an unqualified reference to a lexical binder.
qualified :: Maybe String -> Qualified Ident -> String
qualified currentModule ident =
  let name = unwrap (unQualified ident)
  in case ident of
    Qualified (Just owner) _ -> inModule (unwrap owner) name
    Qualified Nothing _ -> case currentModule of
      Just owner -> inModule owner name
      Nothing -> sanitizeName name
