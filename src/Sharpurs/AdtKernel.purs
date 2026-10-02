-- | Select native ADT bindings in source order. Analysis proves eligibility,
-- | Lower translates optimized bodies, and Emit supplies their ABI boundaries.
module Sharpurs.AdtKernel (AdtModule, prepareModule, fromModule) where

import Prelude

import Control.Alternative (guard)
import Data.Array as Array
import Data.Foldable (all)
import Data.Map (Map)
import Data.Map as Map
import Data.Maybe (Maybe(..))
import Data.Traversable (traverse)
import Data.Tuple (Tuple(..))
import PureScript.Backend.Optimizer.Convert (BackendModule, BackendBindingGroup)
import PureScript.Backend.Optimizer.CoreFn (Ann, Ident, Module(..), Qualified(..))
import PureScript.Backend.Optimizer.CoreFn as C
import PureScript.Backend.Optimizer.Semantics (NeutralExpr)
import Sharpurs.AdtKernel.Analysis (Context, Signature, unique)
import Sharpurs.AdtKernel.Analysis as Analysis
import Sharpurs.AdtKernel.Emit as Emit
import Sharpurs.AdtKernel.Lower as Lower
import Sharpurs.AdtLayout as Layout
import Sharpurs.Analysis.Source as Source
import Sharpurs.FsAst (FsDecl(..), FsModule(..))
import Sharpurs.Names as Names

-- Contains all public constructor wrappers and the selected multi-argument
-- workers. nativeNames lists functions only; other source bindings stay boxed.
type AdtModule =
  { layout :: Layout.Layout
  , bindings :: Map Ident FsDecl
  , nativeNames :: Array Ident
  }

type Candidate = { name :: Ident, signature :: Signature, declaration :: FsDecl, recursive :: Boolean }
type SelectedBindings = { globals :: Map (Qualified Ident) Signature, candidates :: Array Candidate }

-- Admit a closed layout only together with every constructor's public object
-- wrapper. Functions need a recursive-ADT argument, closed native signatures
-- and supported bodies. Dependencies become available in source binding order.
prepareModule :: Module Ann -> BackendModule -> Maybe AdtModule
prepareModule core@(Module source) backend = do
  layout <- Layout.fromModule core
  guard (source.name == backend.name && source.dataDecls == backend.dataDecls)
  guard (Map.isEmpty source.foreign && Map.isEmpty backend.foreign)
  guard (Array.null source.classDecls && Array.null backend.classDecls)
  let
    sourceBindings = Array.concatMap Source.bindings source.decls
    backendBindings = Array.concatMap _.bindings backend.bindings
    publicNames = map (Analysis.publicName layout <<< _.name) sourceBindings
    constructors = Array.concatMap _.constructors layout.declarations
    qualified name = Qualified (Just source.name) name
    context = { layout, globals: Map.empty, locals: Map.empty, guardedCalls: true, self: Nothing }
    blocked = Analysis.unsupportedBindings source.name sourceBindings
  guard (unique (map _.name sourceBindings) && unique publicNames)
  guard (unique (map (\(Tuple name _) -> name) backendBindings))
  ctorBindings <- traverse (\ctor -> do
    let name = C.unQualified ctor.sourceName
    sourceBinding <- Array.find (\item -> item.name == name) sourceBindings
    guard (not sourceBinding.recursive)
    backendGroup <- Array.find (\item -> Array.any (\(Tuple ident _) -> ident == name) item.bindings) backend.bindings
    guard (not backendGroup.recursive)
    Analysis.validateSourceConstructor ctor sourceBinding.expr
    expression <- lookupExpression name backendBindings
    sig <- Analysis.signature layout name expression
    guard (Source.annotation sourceBinding.expr == Just (Analysis.signatureType sig))
    definition <- Lower.binding context ctor.sourceName sig expression
    declaration <- Emit.constructorBinding layout sig definition
    pure { name, signature: sig, declaration, recursive: false }
    ) constructors
  let
    ctorSignatures = Map.fromFoldable (map (\item -> Tuple (qualified item.name) item.signature) ctorBindings)
    select :: SelectedBindings -> Source.SourceBinding Ann -> SelectedBindings
    select selected binding = case do
      guard binding.singleton
      guard (not (Map.member (qualified binding.name) ctorSignatures))
      guard (not (Array.elem (qualified binding.name) blocked))
      group <- Array.find (Array.any (\(Tuple name _) -> name == binding.name) <<< _.bindings) backend.bindings
      guard (not group.recursive || Array.length group.bindings == 1)
      guard (group.recursive == binding.recursive)
      expression <- lookupExpression binding.name group.bindings
      sig <- Analysis.signature layout binding.name expression
      guard (Source.annotation binding.expr == Just (Analysis.signatureType sig))
      Analysis.validateSourceParameters sig.args sig.result binding.expr
      guard (Array.any (Analysis.recursiveArgument layout) sig.args)
      let globals = if binding.recursive then Map.insert (qualified binding.name) sig selected.globals else selected.globals
      definition <- Lower.binding
        (context { globals = globals, self = if binding.recursive then Just (qualified binding.name) else Nothing })
        (qualified binding.name) sig expression
      declaration <- Emit.functionBinding layout sig binding.recursive definition
      pure { name: binding.name, signature: sig, recursive: binding.recursive, declaration }
      of
        Nothing -> case Map.lookup (qualified binding.name) ctorSignatures of
          Nothing -> selected
          Just sig -> selected { globals = Map.insert (qualified binding.name) sig selected.globals }
        Just candidate ->
          { globals: Map.insert (qualified candidate.name) candidate.signature selected.globals
          , candidates: Array.snoc selected.candidates candidate
          }
    candidates = (Array.foldl select { globals: Map.empty, candidates: [] } sourceBindings).candidates
    emitted = ctorBindings <> candidates
    generatedNames = Array.concatMap (\item -> [ item.signature.nativeName ] <> if item.recursive then [ Names.recursive item.signature.publicName ] else []) emitted
      <> map (Names.guarded <<< _.signature.nativeName) candidates
    ctorNames = map _.name constructors
  guard (not (Array.null candidates))
  guard (unique generatedNames && all Layout.validIdentifier (publicNames <> generatedNames))
  guard (all (\name -> not (Array.elem name publicNames) && not (Array.elem name ctorNames)) generatedNames)
  guard (all (\name -> not (Array.elem name publicNames)) ctorNames)
  pure
    { layout
    , bindings: Map.fromFoldable (map (\item -> Tuple item.name item.declaration) emitted)
    , nativeNames: map _.name candidates
    }

lookupExpression :: Ident -> Array (Tuple Ident NeutralExpr) -> Maybe NeutralExpr
lookupExpression name bindings = Array.findMap (\(Tuple ident expr) -> if ident == name then Just expr else Nothing) bindings

-- All-or-nothing whole-native generation, used by AdtInterop's closed producer.
-- Unlike prepareModule's mixed path, every optimized binding must be supported;
-- native recursive groups may be mutual and their internal calls are unguarded.
fromModule :: Module Ann -> BackendModule -> Maybe FsModule
fromModule core@(Module source) backend = do
  layout <- Layout.fromModule core
  guard (source.name == backend.name && source.dataDecls == backend.dataDecls)
  guard (Map.isEmpty source.foreign && Map.isEmpty backend.foreign && Array.null backend.classDecls)
  let bindings = Array.concatMap _.bindings backend.bindings
  signatures <- traverse (\(Tuple name expr) -> Tuple (Qualified (Just backend.name) name) <$> Analysis.signature layout name expr) bindings
  let names = Array.concatMap (\(Tuple _ sig) -> [ sig.nativeName, sig.publicName ]) signatures
  guard (all Layout.validIdentifier names && unique names)
  guard (all (\decl -> all (\ctor -> not (Array.elem ctor.name names)) decl.constructors) layout.declarations)
  guard (unique (map (\(Tuple name _) -> name) signatures))
  let globals = Map.fromFoldable signatures
  let groups = Array.concatMap (\group -> if group.recursive then [ group ] else map (\binding -> { recursive: false, bindings: [ binding ] }) group.bindings) backend.bindings
  emitted <- emitGroups { layout, globals: Map.empty, locals: Map.empty, guardedCalls: false, self: Nothing } globals groups
  pure (FsModule layout.moduleName [ FsRaw (Layout.printDeclarations layout <> "\n\n" <> emitted) ])

emitGroups :: Context -> Map (Qualified Ident) Signature -> Array (BackendBindingGroup Ident NeutralExpr) -> Maybe String
emitGroups ctx signatures groups = case Array.uncons groups of
  Nothing -> Just ""
  Just { head: group, tail } -> do
    let qualified name = Qualified (Just (C.ModuleName ctx.layout.moduleName)) name
    current <- traverse (\(Tuple name _) -> Tuple (qualified name) <$> Map.lookup (qualified name) signatures) group.bindings
    guard (not (Array.null current))
    let available = Map.union (Map.fromFoldable current) ctx.globals
    let bodyContext = ctx { globals = if group.recursive then available else ctx.globals }
    definitions <- traverse (\(Tuple name expr) -> do
      sig <- Map.lookup (qualified name) signatures
      guard (not group.recursive || not (Array.null sig.args))
      definition <- Lower.binding bodyContext (qualified name) sig expr
      pure { signature: sig, definition }
      ) group.bindings
    guard (group.recursive || Array.length definitions == 1)
    emitted <- Emit.group ctx.layout group.recursive definitions
    rest <- emitGroups (ctx { globals = available }) signatures tail
    pure (emitted <> rest)
