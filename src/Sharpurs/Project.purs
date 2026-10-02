-- | Write the generated sources and their ordered .NET project files.
module Sharpurs.Project
  ( ModuleFiles
  , prepare
  , writeModule
  , finalize
  ) where

import Prelude

import Data.Array as Array
import Data.Maybe (Maybe, maybe)
import Data.String as String
import Data.Traversable (traverse_)
import Effect.Aff (Aff)
import Sharpurs.Ffi (ModuleFfi)
import Sharpurs.Names as Names
import Sharpurs.Printer.Layout (normalizeRecIndent)
import Sharpurs.Project.FileSystem as FileSystem
import Sharpurs.Runtime as Runtime

-- | Source paths relative to output/Main, returned only after successful writes.
-- | Unchanged files still belong to this invocation's inventory.
type ModuleFiles =
  { fsharp :: String
  , csharp :: Maybe String
  }

outputDirectory :: String
outputDirectory = "output/Main"

prepare :: Aff Unit
prepare = do
  FileSystem.ensureDirectory outputDirectory
  writeOutput "Sharpurs_Prelude.fs" Runtime.prelude

writeModule :: String -> ModuleFfi -> String -> Aff ModuleFiles
writeModule name ffi declarations = do
  traverse_ (writeOutput (name <> ".cs")) ffi.csharp
  let content = String.joinWith "\n"
        [ "[<AutoOpen>]"
        , "module " <> Names.generatedModule name
        , ""
        , "open System"
        , "open System.Collections.Generic"
        , ""
        , ffi.fsharp <> declarations
        , ""
        ]
  -- Recursive-let indentation depends on the final column in the whole file.
  writeOutput (name <> ".fs") (normalizeRecIndent content)
  pure { fsharp: name <> ".fs", csharp: (\_ -> name <> ".cs") <$> ffi.csharp }

-- The emitter's inventory arrives in dependency order; F# requires that order.
-- Never discover sources by scanning output/Main: it also holds application
-- inputs (notably Main/corefn.json) and files from previous invocations.
finalize :: { mainModule :: String, modules :: Array ModuleFiles } -> Aff Unit
finalize { mainModule, modules } = do
  writeOutput "EntryPoint.fs" (Runtime.entryPoint mainModule)
  csharpReference <- writeCSharpProject (Array.sort (Array.mapMaybe _.csharp modules))
  packageReferences <- readPackageReferences
  let
    files = [ "Sharpurs_Prelude.fs" ] <> map _.fsharp modules <> [ "EntryPoint.fs" ]
    project = fsharpProjectHeader <> csharpReference <> packageReferences <> compileItems files <> projectFooter
  writeOutput "Program.fsproj" project
  writeOutput "Directory.Build.props" intermediateOutputProps

writeCSharpProject :: Array String -> Aff String
writeCSharpProject csharpFiles =
  if Array.null csharpFiles then do
    -- This fixed project is generator-owned; old sources are left alone.
    FileSystem.removeIfExists (outputDirectory <> "/FFI.CSharp.csproj")
    pure ""
  else do
    writeOutput "FFI.CSharp.csproj" (csharpProjectHeader <> compileItems csharpFiles <> projectFooter)
    pure "    <ProjectReference Include=\"FFI.CSharp.csproj\" />\n"

readPackageReferences :: Aff String
readPackageReferences = do
  -- This optional file is an ItemGroup fragment, inserted verbatim. It is not
  -- an MSBuild <Project> document despite the .props suffix.
  fragment <- FileSystem.readOptionalText "sharp.packages.props"
  pure (maybe "" (_ <> "\n") fragment)

compileItems :: Array String -> String
compileItems files =
  String.joinWith "\n" (map (\file -> "    <Compile Include=\"" <> file <> "\" />") files) <> "\n"

writeOutput :: String -> String -> Aff Unit
writeOutput file = FileSystem.writeTextIfChanged (outputDirectory <> "/" <> file)

fsharpProjectHeader :: String
fsharpProjectHeader = """<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <LangVersion>7.0</LangVersion>
    <WarningsAsErrors>false</WarningsAsErrors>
    <NoWarn>40,25,46,58,66,67,3370</NoWarn>
  </PropertyGroup>
  <ItemGroup>
"""

csharpProjectHeader :: String
csharpProjectHeader = """<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <WarningsAsErrors>false</WarningsAsErrors>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <OutputPath>bin/csharp/</OutputPath>
  </PropertyGroup>
  <ItemGroup>
"""

projectFooter :: String
projectFooter = """  </ItemGroup>
</Project>
"""

intermediateOutputProps :: String
intermediateOutputProps = """<Project>
  <PropertyGroup>
    <BaseIntermediateOutputPath>obj/$(MSBuildProjectName)/</BaseIntermediateOutputPath>
    <MSBuildProjectExtensionsPath>obj/$(MSBuildProjectName)/</MSBuildProjectExtensionsPath>
  </PropertyGroup>
</Project>
"""
