-- | Resolve native implementations and assemble the PureScript-facing wrappers.
-- | Source recognition lives in FfiSupport; project file writes live in Project.
module Sharpurs.Ffi
  ( ModuleFfi
  , loadModule
  ) where

import Prelude

import Data.Array as Array
import Data.Map as Map
import Data.Maybe (Maybe(..), isJust)
import Data.Newtype (unwrap)
import Data.String as String
import Data.Traversable (traverse)
import Effect.Aff (Aff)
import Effect.Class (liftEffect)
import Node.Encoding (Encoding(..))
import Node.FS.Aff as FS
import PureScript.Backend.Optimizer.CoreFn (Ann, ExprType(..), Ident(..), Module(..))
import PureScript.Backend.Optimizer.FfiSupport (findFfiFile)
import Sharpurs.FfiSupport (appendCsFfiWrappers, appendFfiWrappers)
import Sharpurs.FsAst (modulePrefix)

type ModuleFfi =
  { fsharp :: String
  , csharp :: Maybe String
  }

loadModule :: Maybe String -> Module Ann -> Aff ModuleFfi
loadModule ffiDirectory (Module source) = do
  let
    name = unwrap source.name
    requiredForeigns = map unwrap (Array.fromFoldable (Map.keys source.foreign))
    find extension = liftEffect $ findFfiFile extension overrideDirectories ffiDirectory name (Just source.path)
    foreignArity foreignName = case Map.lookup (Ident foreignName) source.foreign of
      Just (Just ty) -> typeArity ty
      _ -> 0

  fsPath <- find ".fs"
  csPath <- find ".cs"
  fsWrappers <- case fsPath of
    Just path -> do
      content <- FS.readTextFile UTF8 path
      pure (appendFfiWrappers name requiredForeigns content <> "\n\n")
    Nothing
      | Array.null requiredForeigns || isJust csPath -> pure ""
      | otherwise -> pure (Array.foldMap (\foreignName -> stubForeign name foreignName (foreignArity foreignName)) requiredForeigns <> "\n\n")

  -- F# owns the public wrappers when both files exist. The C# source is still
  -- compiled so that the F# implementation can call its helpers explicitly.
  csharp <- traverse (FS.readTextFile UTF8) csPath
  let csWrappers = case fsPath, csharp of
        Nothing, Just content -> appendCsFfiWrappers name requiredForeigns content <> "\n\n"
        _, _ -> ""
  pure { fsharp: fsWrappers <> csWrappers, csharp }

-- Application-maintained overrides, also reachable from the fixture runner.
overrideDirectories :: Array String
overrideDirectories = [ "../../bak/spago.d/fs/p", "bak/spago.d/fs/p" ]

typeArity :: ExprType -> Int
typeArity = case _ of
  Func args _ -> Array.length args
  ForAll _ inner -> typeArity inner
  ConstrainedType _ inner -> typeArity inner
  TypeApp inner _ -> typeArity inner
  _ -> 0

-- Missing implementations remain callable placeholders so unused imports do
-- not prevent compilation. Curry to the known arity: partial application at
-- module initialization must not throw. Without a function type, keep a thunk.
stubForeign :: String -> String -> Int -> String
stubForeign name foreignName arity =
  let
    parameters = if arity > 0 then
      map (\index -> "arg" <> show index) (Array.range 0 (arity - 1))
    else [ "_" ]
    opens = String.joinWith "" (map (\arg -> "(fun (" <> arg <> ": obj) -> ") parameters)
    closes = String.joinWith "" (map (const ")") parameters)
  in
    "let " <> modulePrefix name <> "_" <> foreignName <> " = box (" <> opens
      <> "failwith \"FFI not implemented: " <> name <> "." <> foreignName <> "\"" <> closes <> ")\n"
