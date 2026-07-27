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
import PureScript.Backend.Optimizer.CoreFn (Module(..), Ident(..), Qualified(..), ModuleName(..))
import Data.List (List(..))
import PureScript.Backend.Optimizer.App (coreFnModulesFromOutput, parseCLIArgs, checkCache, writeCache, loadDirectives)
import PureScript.Backend.Optimizer.Builder (buildModules)
import PureScript.Backend.Optimizer.Semantics.Foreign (coreForeignSemantics)
import Sharpurs.FsAst (FsModule(..), sanitizeName)
import Sharpurs.CodeGen (translateModule)
import Sharpurs.Printer (printModule)
import PureScript.Backend.Optimizer.FfiSupport (findFfiFile)
import Sharpurs.FfiSupport (appendFfiWrappers)
import Data.Newtype (unwrap)
import Data.Maybe (Maybe(..))
import Data.String (joinWith)

fsPrelude :: String
fsPrelude = """
let objMap = Map.empty<string, obj>
let unbox<'a> (x: obj) : 'a = unbox x
let (|Unbox|) (x: obj) = unbox x

let undefined = Unchecked.defaultof<obj>
let Prim_undefined = undefined
let intMod a b = unbox<int> a % unbox<int> b
let semiringInt = 0

let sharpurs_apply (func: obj) (arg: obj) : obj =
    if isNull func then failwith "sharpurs_apply: func is null!"
    let method = func.GetType().GetMethods() |> Array.find (fun m -> m.Name = "Invoke" && m.GetParameters().Length = 1)
    method.Invoke(func, [| arg |])
"""

fsHeader :: String
fsHeader = "let (|LitBool|_|) (expected: bool) (value: obj) = if value :? bool && unbox value = expected then Some() else None\nlet (|LitInt|_|) (expected: int) (value: obj) = if value :? int && unbox value = expected then Some() else None\nlet (|LitNumber|_|) (expected: float) (value: obj) = if value :? float && unbox value = expected then Some() else None\nlet (|LitString|_|) (expected: string) (value: obj) = if value :? string && unbox value = expected then Some() else None\nlet (|LitChar|_|) (expected: char) (value: obj) = if value :? char && unbox value = expected then Some() else None\nlet (|HasProp|_|) (key: string) (value: obj) = if value :? Map<string, obj> then Map.tryFind key (unbox<Map<string, obj>> value) else None\n\n"

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
            ctors = Array.concatMap (\d -> map (\c -> Tuple (sanitizeName (modPrefix <> "_" <> c.constructorName)) (Array.length c.fieldTypes)) d.constructors) mod.dataDecls
          in
            Map.union acc (Map.fromFoldable ctors)
      )
      Map.empty
      modulesArr

  _ <- attempt (FS.mkdir "output/Main")
  
  -- Write Sharpurs_Prelude.fs
  let preludeContent = "[<AutoOpen>]\nmodule Sharpurs_Prelude\n\n#nowarn \"25\"\n#nowarn \"46\"\n#nowarn \"66\"\n#nowarn \"67\"\n#nowarn \"3370\"\n\nopen System\nopen System.Collections.Generic\n\n" <> fsHeader <> fsPrelude
  FS.writeTextFile UTF8 ("output/Main/Sharpurs_Prelude.fs") preludeContent

  directives <- loadDirectives
  let cacheVersion = "1.0.0"

  -- Generate and write each module
  buildModules
    { directives
    , analyzeCustom: \_ _ -> Nothing
    , foreignSemantics: Map.filterKeys (\(Qualified mbMod _) -> case mbMod of
        Just (ModuleName m) -> not (String.contains (String.Pattern "Effect") m) && not (String.contains (String.Pattern "Control.Monad.ST") m)
        _ -> true
      ) coreForeignSemantics
    , traceIdents: Set.empty
    , onPrepareModule: \_ m -> pure m
    , onSkipModule: \_ (Module coreFnMod) -> do
        let modNameStr = unwrap coreFnMod.name
        checkCache cacheVersion coreFnMod.path ("output/Main/" <> modNameStr <> ".sharpurs-cache.json")
    , onCodegenModule: \_ (Module coreFnMod) backendMod _ -> do
        let modNameStr = unwrap backendMod.name
        writeCache cacheVersion ("output/Main/" <> modNameStr <> ".sharpurs-cache.json") backendMod
        
        let (FsModule _ decls) = translateModule globalAdtCtors (Module coreFnMod)
        let fsCode = printModule (FsModule modNameStr decls)
        
        ffiPathMb <- liftEffect $ findFfiFile ".fs" ["../../bak/spago.d/fs/p", "bak/spago.d/fs/p"] args.mbFfiDir modNameStr (Just coreFnMod.path)
        ffiContent <- case ffiPathMb of
          Nothing -> pure ""
          Just ffiPath -> do
            content <- FS.readTextFile UTF8 ffiPath
            let requiredForeigns = map (\(Ident f) -> f) coreFnMod.foreign
            let wrappers = appendFfiWrappers modNameStr requiredForeigns content
            pure (wrappers <> "\n\n")
            
        let safeModName = String.replaceAll (String.Pattern ".") (String.Replacement "_") modNameStr
        let moduleContent = "[<AutoOpen>]\nmodule PureScript_" <> safeModName <> "\n\nopen System\nopen System.Collections.Generic\n\n" <> ffiContent <> fsCode <> "\n"
        
        FS.writeTextFile UTF8 ("output/Main/" <> modNameStr <> ".fs") moduleContent
    }
    finalModules
    
  -- Write EntryPoint.fs
  let entryPointContent = "module Sharpurs_EntryPoint\n\nlet _ = (unbox<obj -> obj> Main_main) undefined\n"
  FS.writeTextFile UTF8 ("output/Main/EntryPoint.fs") entryPointContent
  
  -- Write Program.fsproj
  let projHeader = "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <OutputType>Exe</OutputType>\n    <TargetFramework>net8.0</TargetFramework>\n    <WarningsAsErrors>false</WarningsAsErrors>\n    <NoWarn>40,25,46,66,67,3370</NoWarn>\n  </PropertyGroup>\n  <ItemGroup>\n"
  let projFooter = "  </ItemGroup>\n</Project>\n"
  let projFiles = Array.concat [ ["Sharpurs_Prelude.fs"], map (\(Module m) -> unwrap m.name <> ".fs") modulesArr, ["EntryPoint.fs"] ]
  let projIncludes = String.joinWith "\n" (map (\f -> "    <Compile Include=\"" <> f <> "\" />") projFiles)
  FS.writeTextFile UTF8 ("output/Main/Program.fsproj") (projHeader <> projIncludes <> "\n" <> projFooter)

  pure unit
