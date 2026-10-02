-- | The backend's complete command-line contract. Receives user arguments only;
-- | the shell/Spago owns tokenization, so values are never split on whitespace.
module Sharpurs.CLI
  ( Config
  , Command(..)
  , parse
  , help
  ) where

import Prelude

import Data.Array as Array
import Data.Either (Either(..))
import Data.List (List(..))
import Data.List as List
import Data.Maybe (Maybe(..), fromMaybe, isJust)
import Data.String as String
import Data.String.CodeUnits as CodeUnits
import Data.String.Pattern (Pattern(..))

type Config =
  { mainModule :: String
  , ffiDirectory :: Maybe String
  }

data Command = Compile Config | ShowHelp

data Option = MainModule | FfiDirectory

type Options =
  { mainModule :: Maybe String
  , ffiDirectory :: Maybe String
  , helpRequested :: Boolean
  }

parse :: Array String -> Either String Command
parse arguments = do
  options <- consume { mainModule: Nothing, ffiDirectory: Nothing, helpRequested: false } (List.fromFoldable arguments)
  pure if options.helpRequested then ShowHelp
  else Compile { mainModule: fromMaybe "Main" options.mainModule, ffiDirectory: options.ffiDirectory }

consume :: Options -> List String -> Either String Options
consume options = case _ of
  Nil -> Right options
  Cons "--" Nil -> Right options
  Cons "--" (Cons argument _) -> Left ("Unexpected positional argument: " <> show argument)
  Cons argument rest ->
    let { name, value } = splitOption argument
    in
      if name == "--help" || name == "-h" then case value of
        Nothing -> consume (options { helpRequested = true }) rest
        Just _ -> Left ("Option " <> name <> " does not take a value.")
      else do
        option <- recognize name
        case value, rest of
          Just supplied, _ -> withValue option supplied rest
          Nothing, Cons supplied remaining | not (hasLeadingDash supplied) ->
            withValue option supplied remaining
          _, _ -> Left ("Missing value for " <> optionName option <> ".")
    where
    withValue option value remaining = do
      next <- recordOption option value options
      consume next remaining

-- Split only the first '=', keeping every character of the supplied value.
splitOption :: String -> { name :: String, value :: Maybe String }
splitOption argument = case String.indexOf (Pattern "=") argument of
  Nothing -> { name: argument, value: Nothing }
  Just index -> { name: CodeUnits.take index argument, value: Just (CodeUnits.drop (index + 1) argument) }

recognize :: String -> Either String Option
recognize = case _ of
  "--main" -> Right MainModule
  "--ffi" -> Right FfiDirectory
  name
    | Array.elem name [ "--bundle", "--output", "--rewrite-limit", "--autoload-path" ] ->
        Left ("Option " <> name <> " is not supported by Sharpurs; it was previously ignored.")
    | hasLeadingDash name -> Left ("Unknown option: " <> show name)
    | otherwise -> Left ("Unexpected positional argument: " <> show name)

hasLeadingDash :: String -> Boolean
hasLeadingDash = isJust <<< String.stripPrefix (Pattern "-")

recordOption :: Option -> String -> Options -> Either String Options
recordOption option value options
  | value == "" = Left ("Empty value for " <> optionName option <> ".")
  | otherwise = case option of
      MainModule | isJust options.mainModule -> duplicate
      MainModule -> Right (options { mainModule = Just value })
      FfiDirectory | isJust options.ffiDirectory -> duplicate
      FfiDirectory -> Right (options { ffiDirectory = Just value })
  where
  duplicate = Left ("Option " <> optionName option <> " may only be specified once.")

optionName :: Option -> String
optionName = case _ of
  MainModule -> "--main"
  FfiDirectory -> "--ffi"

help :: String
help = """Usage: sharpurs [--main MODULE] [--ffi DIRECTORY]
       sharpurs --help

Compile typed PureScript CoreFn from output/ into output/Main/Program.fsproj.

Options:
  --main MODULE    Module whose main is called (default: Main).
  --ffi DIRECTORY  Additional native FFI search directory.
  -h, --help       Print this help without reading or writing project files.

Valued options also accept --name=value and may be specified only once.
Quote values containing spaces; each option/value must be a separate argument.
There are no positional arguments. Invalid arguments exit with code 2.
"""
