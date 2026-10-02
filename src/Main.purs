module Main (main) where

import Prelude

import Data.Array as Array
import Data.Either (Either(..))
import Data.List (List(..))
import Data.Map (Map)
import Data.Map as Map
import Data.Maybe (Maybe(..))
import Data.Newtype (unwrap)
import Data.Set (Set)
import Data.Set as Set
import Data.String as String
import Data.Tuple (Tuple(..))
import Effect (Effect)
import Effect.Aff (Aff, launchAff_)
import Effect.Class (liftEffect)
import Effect.Console as Console
import Effect.Ref as Ref
import Node.Process as Process
import PureScript.Backend.Optimizer.App (coreFnModulesFromOutput, loadDirectives)
import PureScript.Backend.Optimizer.Builder (buildModules)
import PureScript.Backend.Optimizer.Convert (BackendModule)
import PureScript.Backend.Optimizer.CoreFn (Ann, Ident(..), Module(..), ModuleName(..), Qualified(..))
import PureScript.Backend.Optimizer.Semantics.Foreign (ForeignEval, coreForeignSemantics)
import Sharpurs.AdtKernel as AdtKernel
import Sharpurs.CLI as CLI
import Sharpurs.CodeGen (translateOptimizedModuleWithThunks)
import Sharpurs.Ffi as Ffi
import Sharpurs.Metrics as Metrics
import Sharpurs.Names as Names
import Sharpurs.Printer (printModule)
import Sharpurs.Project as Project
import Sharpurs.ThunkKernel as ThunkKernel

main :: Effect Unit
main = do
  arguments <- Array.drop 2 <$> Process.argv
  case CLI.parse arguments of
    Left diagnostic -> do
      Console.error ("sharpurs: " <> diagnostic <> "\nRun sharpurs --help for usage.")
      Process.setExitCode 2
    Right CLI.ShowHelp -> Console.log CLI.help
    Right (CLI.Compile config) -> launchAff_ (compile config)

compile :: CLI.Config -> Aff Unit
compile config = Metrics.measure "backend total" \_ -> do
  modules <- Metrics.measure "load TAST + sort" \_ -> coreFnModulesFromOutput "output"
  let modulesArray = Array.fromFoldable modules

  { directives, context, moduleFiles } <- Metrics.measure "prepare" \_ -> do
    Project.prepare
    directives <- loadDirectives
    nativeConstructors <- liftEffect (Ref.new Set.empty)
    moduleFiles <- liftEffect (Ref.new Nil)
    pure
      { directives
      , moduleFiles
      , context:
          { ffiDirectory: config.ffiDirectory
          , constructorArities: collectConstructorArities modulesArray
          , nativeConstructors
          }
      }

  Metrics.measure "optimize + emit" \_ ->
    buildModules
      { directives
      , rewriteLimit: 10000
      , analyzeCustom: \_ _ -> Nothing
      , foreignSemantics
      , traceIdents: Set.empty
      , onPrepareModule: \_ source -> pure source
      -- No optimizer cache: every module must register its native constructors
      -- and emit its files in dependency order on each invocation.
      , onSkipModule: \_ _ -> pure Nothing
      , onCodegenModule: \_ source optimized _ -> do
          files <- emitModule context source optimized
          liftEffect (Ref.modify_ (Cons files) moduleFiles)
      }
      modules

  Metrics.measure "finalize" \_ -> do
    -- Accumulate in constant time, then restore the builder's dependency order.
    emitted <- Array.reverse <<< Array.fromFoldable <$> liftEffect (Ref.read moduleFiles)
    Project.finalize
      { mainModule: config.mainModule
      , modules: emitted
      }

type EmitContext =
  { ffiDirectory :: Maybe String
  , constructorArities :: Map String Int
  , nativeConstructors :: Ref.Ref (Set String)
  }

emitModule :: EmitContext -> Module Ann -> BackendModule -> Aff Project.ModuleFiles
emitModule context source optimized = do
  let
    name = unwrap optimized.name
    native = AdtKernel.prepareModule source optimized
    thunks = ThunkKernel.prepareModule source optimized
  -- The builder visits dependencies first. Register a producer only after its
  -- layout and constructor wrappers pass validation, before its mixed bindings.
  wrappers <- liftEffect (Ref.modify (Set.union (nativeConstructorNames native)) context.nativeConstructors)
  let generated = translateOptimizedModuleWithThunks wrappers native thunks context.constructorArities optimized source
  ffi <- Ffi.loadModule context.ffiDirectory source
  Project.writeModule name ffi (printModule generated)

collectConstructorArities :: Array (Module Ann) -> Map String Int
collectConstructorArities = Array.foldl collect Map.empty
  where
  collect arities (Module source) =
    let
      constructors = Array.concatMap
        (\decl -> map (\ctor -> Tuple (Names.inModule (unwrap source.name) ctor.name) (Array.length ctor.fields)) decl.constructors)
        source.dataDecls
    in Map.union arities (Map.fromFoldable constructors)

nativeConstructorNames :: Maybe AdtKernel.AdtModule -> Set String
nativeConstructorNames = case _ of
  Nothing -> Set.empty
  Just selected -> Set.fromFoldable (Array.concatMap (map publicName <<< _.constructors) selected.layout.declarations)
  where
  publicName ctor = case ctor.sourceName of
    Qualified (Just (ModuleName owner)) (Ident name) -> Names.inModule owner name
    _ -> ctor.name

-- Keep Effect and ST calls on the native FFI path rather than lowering them
-- through the optimizer's effect-specific semantics.
foreignSemantics :: Map (Qualified Ident) ForeignEval
foreignSemantics = Map.filterKeys supported coreForeignSemantics
  where
  supported (Qualified owner _) = case owner of
    Just (ModuleName name) ->
      not (String.contains (String.Pattern "Effect") name)
        && not (String.contains (String.Pattern "Control.Monad.ST") name)
    Nothing -> true
