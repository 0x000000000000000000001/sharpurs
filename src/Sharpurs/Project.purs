-- | Write the generated sources and their ordered .NET project files.
module Sharpurs.Project
  ( prepare
  , writeModule
  , finalize
  ) where

import Prelude

import Data.Array as Array
import Data.Either (Either(..))
import Data.Maybe (isJust)
import Data.String as String
import Data.String.Pattern (Pattern(..))
import Data.Traversable (traverse_)
import Effect.Aff (Aff, attempt)
import Node.Encoding (Encoding(..))
import Node.FS.Aff as FS
import Sharpurs.Ffi (ModuleFfi)
import Sharpurs.FsAst (modulePrefix)
import Sharpurs.Printer.Layout (normalizeRecIndent)
import Sharpurs.Runtime as Runtime

outputDirectory :: String
outputDirectory = "output/Main"

prepare :: Aff Unit
prepare = do
  void $ attempt (FS.mkdir outputDirectory)
  writeOutput "Sharpurs_Prelude.fs" Runtime.prelude

writeModule :: String -> ModuleFfi -> String -> Aff Unit
writeModule name ffi declarations = do
  traverse_ (writeOutput (name <> ".cs")) ffi.csharp
  let content = String.joinWith "\n"
        [ "[<AutoOpen>]"
        , "module PureScript_" <> modulePrefix name
        , ""
        , "open System"
        , "open System.Collections.Generic"
        , ""
        , ffi.fsharp <> declarations
        , ""
        ]
  -- Recursive-let indentation depends on the final column in the whole file.
  writeOutput (name <> ".fs") (normalizeRecIndent content)

-- Module names arrive in dependency order; F# requires that same compile order.
finalize :: { mainModule :: String, moduleNames :: Array String } -> Aff Unit
finalize { mainModule, moduleNames } = do
  writeOutput "EntryPoint.fs" (Runtime.entryPoint mainModule)
  csharpReference <- writeCSharpProject
  packageReferences <- readPackageReferences
  let
    files = [ "Sharpurs_Prelude.fs" ] <> map (_ <> ".fs") moduleNames <> [ "EntryPoint.fs" ]
    project = fsharpProjectHeader <> csharpReference <> packageReferences <> compileItems files <> projectFooter
  writeOutput "Program.fsproj" project
  writeOutput "Directory.Build.props" intermediateOutputProps

writeCSharpProject :: Aff String
writeCSharpProject = do
  files <- FS.readdir outputDirectory
  -- Match the existing output-directory policy: stale C# files are included
  -- until the application cleans its generated output.
  let csharpFiles = Array.sort (Array.filter (isJust <<< String.stripSuffix (Pattern ".cs")) files)
  if Array.null csharpFiles then pure ""
  else do
    writeOutput "FFI.CSharp.csproj" (csharpProjectHeader <> compileItems csharpFiles <> projectFooter)
    pure "    <ProjectReference Include=\"FFI.CSharp.csproj\" />\n"

readPackageReferences :: Aff String
readPackageReferences = do
  -- This optional file is an ItemGroup fragment, inserted verbatim. It is not
  -- an MSBuild <Project> document despite the .props suffix.
  fragment <- attempt (FS.readTextFile UTF8 "sharp.packages.props")
  pure case fragment of
    Left _ -> ""
    Right content -> content <> "\n"

compileItems :: Array String -> String
compileItems files =
  String.joinWith "\n" (map (\file -> "    <Compile Include=\"" <> file <> "\" />") files) <> "\n"

writeOutput :: String -> String -> Aff Unit
writeOutput file content = do
  let path = outputDirectory <> "/" <> file
  previous <- attempt (FS.readTextFile UTF8 path)
  -- Preserve timestamps so an unchanged backend run does not trigger MSBuild.
  case previous of
    Right old | old == content -> pure unit
    _ -> FS.writeTextFile UTF8 path content

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
