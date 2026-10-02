-- | Project I/O policy. Only ENOENT means absence; every other failure reports
-- | its path and keeps Node's code, operation and original error as the cause.
module Sharpurs.Project.FileSystem
  ( ensureDirectory
  , readOptionalText
  , writeTextIfChanged
  , removeIfExists
  ) where

import Prelude

import Data.Either (Either(..))
import Data.Maybe (Maybe(..))
import Effect.Aff (Aff, Error, attempt, catchError, throwError)
import Node.Encoding (Encoding(..))
import Node.FS.Aff as FS
import Node.FS.Stats as Stats

foreign import hasErrorCode :: String -> Error -> Boolean
foreign import pathError :: String -> Error -> Error

atPath :: forall a. String -> Aff a -> Aff a
atPath path action = catchError action (throwError <<< pathError path)

ensureDirectory :: String -> Aff Unit
ensureDirectory path = atPath path do
  created <- attempt (FS.mkdir path)
  case created of
    Right _ -> pure unit
    Left err | hasErrorCode "EEXIST" err -> do
      existing <- FS.stat path
      unless (Stats.isDirectory existing) (throwError err)
    Left err -> throwError err

readOptionalText :: String -> Aff (Maybe String)
readOptionalText path = atPath path do
  result <- attempt (FS.readTextFile UTF8 path)
  case result of
    Right content -> pure (Just content)
    Left err | hasErrorCode "ENOENT" err -> pure Nothing
    Left err -> throwError err

writeTextIfChanged :: String -> String -> Aff Unit
writeTextIfChanged path content = do
  previous <- readOptionalText path
  -- Preserve timestamps so an unchanged backend run does not trigger MSBuild.
  unless (previous == Just content) (atPath path (FS.writeTextFile UTF8 path content))

removeIfExists :: String -> Aff Unit
removeIfExists path = atPath path do
  result <- attempt (FS.unlink path)
  case result of
    Right _ -> pure unit
    Left err | hasErrorCode "ENOENT" err -> pure unit
    Left err -> throwError err
