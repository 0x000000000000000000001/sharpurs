module Sharpurs.ConstructorCall (Call, fromExpr) where

import Prelude

import Control.Alternative (guard)
import Data.Array as Array
import Data.Map (Map)
import Data.Map as Map
import Data.Maybe (Maybe(..))
import Data.Newtype (unwrap)
import Data.String as String
import Data.String.Pattern (Pattern(..), Replacement(..))
import PureScript.Backend.Optimizer.CoreFn (Ann(..), Expr(..), Ident(..), Meta(..), Qualified(..))
import Sharpurs.FsAst (sanitizeName)

type Call =
  { name :: String
  , arity :: Int
  , args :: Array (Expr Ann)
  }

-- The registry comes from dataDecls and already selects the emitted layout.
-- TypeApp has no runtime argument: this only recovers constructor saturation,
-- without changing field representation or requiring monomorphic ann.type.
-- Keep this traversal separate from ordinary function application flattening.
fromExpr :: Map String Int -> Maybe String -> Expr Ann -> Maybe Call
fromExpr arities currentMod expression = do
  let call = applications expression [] false
  guard (call.hasTypeApp && not (Array.null call.args))
  target <- case call.head of
    ExprVar (Ann ann) (Qualified (Just moduleName) (Ident name)) -> do
      fields <- case ann.meta of
        Just (IsConstructor _ fields) -> Just fields
        _ -> Nothing
      pure { name: qualifiedName (unwrap moduleName) name, fields: Array.length fields }
    ExprConstructor _ _ (Ident name) fields -> do
      moduleName <- currentMod
      pure { name: qualifiedName moduleName name, fields: Array.length fields }
    -- An unqualified variable can refer to a local binder, including one that
    -- shadows a constructor name. Only explicit constructor nodes are local.
    _ -> Nothing
  arity <- Map.lookup target.name arities
  guard (arity == target.fields && arity == Array.length call.args)
  pure { name: target.name, arity, args: call.args }

qualifiedName :: String -> String -> String
qualifiedName moduleName name =
  sanitizeName (String.replaceAll (Pattern ".") (Replacement "_") moduleName <> "_" <> name)

applications :: Expr Ann -> Array (Expr Ann) -> Boolean ->
  { head :: Expr Ann, args :: Array (Expr Ann), hasTypeApp :: Boolean }
applications expression args hasTypeApp = case expression of
  ExprApp _ head argument -> applications head (Array.cons argument args) hasTypeApp
  ExprTypeApp _ head _ -> applications head args true
  head -> { head, args, hasTypeApp }
