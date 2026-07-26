module Main where

import Prelude

import Effect (Effect)
import Effect.Aff (launchAff_, attempt)
import Effect.Class (liftEffect)
import Node.FS.Aff as FS
import Node.Encoding (Encoding(..))
import Node.Process as Process
import Data.Array as Array
import Data.Set as Set
import Data.Tuple (Tuple(..))
import Data.Map as Map
import Data.String as String
import PureScript.Backend.Optimizer.CoreFn (Module(..), Ident(..))
import Data.List (List(..))
import PureScript.Backend.Optimizer.App (coreFnModulesFromOutput, parseCLIArgs)
import Sharpurs.FsAst (FsModule(..))
import Sharpurs.CodeGen (translateModule)
import Sharpurs.Printer (printModule, fsPrelude, fsHeader)
import PureScript.Backend.Optimizer.FfiSupport (findFfiFile)
import Sharpurs.FfiSupport (appendFfiWrappers)
import Data.Newtype (unwrap)
import Data.Maybe (Maybe(..))
import Data.String (joinWith)
import Data.Traversable (traverse)

main :: Effect Unit
main = launchAff_ do
  argsRaw <- liftEffect Process.argv
  let args = parseCLIArgs argsRaw

  finalModules <- coreFnModulesFromOutput "output"
  let modulesArr = Array.fromFoldable finalModules

  let
    globalAdtCtors = Array.foldl
      ( \acc coreFnMod ->
          let
            (Module mod) = coreFnMod
            modNameStr = unwrap mod.name
            modPrefix = String.replaceAll (String.Pattern ".") (String.Replacement "_") modNameStr
            ctors = Array.concatMap (\d -> map (\c -> Tuple (modPrefix <> "_" <> c.constructorName) (Array.length c.fieldTypes)) d.constructors) mod.dataDecls
          in
            Map.union acc (Map.fromFoldable ctors)
      )
      Map.empty
      modulesArr

  let
    allDecls = Array.concatMap
      ( \coreFnMod ->
          let
            (FsModule _ decls) = translateModule globalAdtCtors coreFnMod
          in
            decls
      )
      modulesArr

  ffiParts <- traverse
    ( \coreFnMod -> do
        let (Module mod) = coreFnMod
        let modNameStr = unwrap mod.name
        ffiPathMb <- liftEffect $ findFfiFile ".fs" ["../../bak/spago.d/fs/p", "bak/spago.d/fs/p"] args.mbFfiDir modNameStr (Just mod.path)
        case ffiPathMb of
          Nothing -> pure ""
          Just ffiPath -> do
            content <- FS.readTextFile UTF8 ffiPath
            let requiredForeigns = map (\(Ident f) -> f) mod.foreign
            let wrappers = appendFfiWrappers modNameStr requiredForeigns content
            pure (content <> "\n\n" <> wrappers <> "\n\n")
    )
    modulesArr

  let combinedMod = FsModule "Main" allDecls
  let fsCode = printModule combinedMod
  
  let finalScript = "#nowarn \"40\"\n\n" <> fsHeader <> fsPrelude <> joinWith "\n" ffiParts <> "\n" <> fsCode

  _ <- attempt (FS.mkdir "output/Main")
  FS.writeTextFile UTF8 ("output/Main/Program.fsx") finalScript

  pure unit
