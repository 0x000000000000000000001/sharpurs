module Main where

import Prelude

import Effect (Effect)
import Effect.Aff (launchAff_, attempt)
import Effect.Class (liftEffect)
import Effect.Console as Console
import Node.FS.Aff as FS
import Node.Encoding (Encoding(..))
import Node.Process as Process
import Data.Array as Array
import Data.Maybe (Maybe(..))
import Data.Set as Set
import Data.Set (Set)
import Data.Newtype (unwrap)
import PureScript.Backend.Optimizer.CoreFn (Module(..))
import Data.String.Pattern (Pattern(..), Replacement(..))
import Data.String as String
import Data.List (List(..))
import Data.Newtype (unwrap)
import PureScript.Backend.Optimizer.CoreFn (Module(..))
import PureScript.Backend.Optimizer.App (coreFnModulesFromOutput, parseCLIArgs)
import Sharpurs.FsAst (FsModule(..))
import Sharpurs.CodeGen (translateModule)
import Sharpurs.Printer (printModule)

main :: Effect Unit
main = launchAff_ do
  argsRaw <- liftEffect Process.argv
  let args = parseCLIArgs argsRaw

  finalModules <- coreFnModulesFromOutput "output"

  -- We just process all modules and dump them or just 'Main' for the test.
  let modulesArr = Array.fromFoldable finalModules
  
  -- Actually, in F#, order matters. We will just find Main and generate it for the test.
  -- For a real compiler, we'd  let modulesArr = Array.fromFoldable finalModules
  
  let globalAdtCtors = Array.foldl (\acc coreFnMod ->
        let 
          (Module mod) = coreFnMod
          ctors = Array.concatMap (\d -> map (\c -> c.constructorName) d.constructors) mod.dataDecls
        in Set.union acc (Set.fromFoldable ctors)
      ) Set.empty modulesArr
      
  let allDecls = Array.concatMap (\coreFnMod -> 
        let (FsModule _ decls) = translateModule globalAdtCtors coreFnMod
        in decls
      ) modulesArr
      
  let combinedMod = FsModule "Main" allDecls
  let fsCode = printModule combinedMod
  
  _ <- attempt (FS.mkdir "output/Main")
  FS.writeTextFile UTF8 ("output/Main/Program.fs") fsCode
  
  let fsproj = "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <OutputType>Exe</OutputType>\n    <TargetFramework>net8.0</TargetFramework>\n  </PropertyGroup>\n  <ItemGroup>\n    <Compile Include=\"Program.fs\" />\n  </ItemGroup>\n</Project>"
  FS.writeTextFile UTF8 ("output/Main/Main.fsproj") fsproj

  pure unit

-- traverse for array helper
traverse :: forall a b m. Applicative m => (a -> m b) -> Array a -> m (Array b)
traverse f arr = Array.fromFoldable <$> (traverseList f (Array.toUnfoldable arr))

traverseList :: forall a b m. Applicative m => (a -> m b) -> List a -> m (List b)
traverseList _ Nil = pure Nil
traverseList f (Cons x xs) = Cons <$> f x <*> traverseList f xs
