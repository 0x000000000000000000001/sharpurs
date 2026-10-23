-- | Mixed native compilation with an ownership-transfer boundary.
-- |
-- | Every private native definition owns its ADT parameters. Public wrappers
-- | deep-copy parameters that the private body may mutate; internal direct
-- | calls transfer an owned value when it has a single use, and otherwise
-- | insert an explicit deep copy. No user option controls this choice.
module Sharpurs.Native.MutableModule (compile, compileMixed, compileMixedFor, classSafeModules, constructorNames) where

import Prelude

import Control.Alternative (guard)
import Data.Array as Array
import Data.Map (Map)
import Data.Map as Map
import Data.Maybe (Maybe(..))
import Data.Set (Set)
import Data.Set as Set
import Data.String as String
import Data.Traversable (traverse)
import Data.Tuple (Tuple(..), snd)
import PureScript.Backend.Optimizer.CoreFn (Ann(..), Bind, Ident(..), Module(..), ModuleName(..), Qualified(..))
import PureScript.Backend.Optimizer.CoreFn as C
import Sharpurs.Analysis.Source as Source
import Sharpurs.CodeGen as CodeGen
import Sharpurs.FsAst (FsDecl(..), FsModule(..), modulePrefix, sanitizeName)
import Sharpurs.Native.Destination as Destination
import Sharpurs.Native.Emit as Emit
import Sharpurs.Native.IR (NativeCase, NativeExpr(..), NativeParameter)
import Sharpurs.Native.Interprocedural as Interprocedural
import Sharpurs.Native.Layout as NativeLayout
import Sharpurs.Native.Lower as NativeLower
import Sharpurs.Native.Pattern as Pattern
import Sharpurs.Native.Specialized as Specialized
import Sharpurs.Native.Types (Constructor, Layout, NativeType(..), nativeTypeName)
import Sharpurs.Native.Types as NativeTypes
import Sharpurs.Native.Usage as Usage
import Sharpurs.Names as Names

type Lowered =
  { binding :: Source.SourceBinding Ann
  , signature :: NativeLower.Signature
  , lowered :: { parameters :: Array NativeParameter, body :: NativeExpr }
  , exported :: Boolean
  , emitWrapper :: Boolean
  , owned :: Set String
  , usage :: Usage.Summary
  }

type Planned =
  { item :: Lowered
  , body :: NativeExpr
  , mutatedRoots :: Array String
  , owned :: Set String
  , usage :: Usage.Summary
  }

compile :: Module Ann -> Maybe FsModule
compile source@(Module module') = do
  guardEmptyForeign module'
  layout <- NativeLayout.fromModule source
  let
    bindings = Array.filter (not <<< isConstructor layout) (Array.concatMap Source.bindings module'.decls)
    exportedNames = exportedNamesOf module'
  signatures <- traverse (signatureFor layout) bindings
  let allSignatures = signatures <> constructorSignatures layout
  lowered <- traverse (loweredBinding layout allSignatures exportedNames) bindings
  definitions <- renderDefinitions layout (specializeNodes layout lowered)
  constructors <- renderConstructors layout
  pure (FsModule (sanitizeName layout.moduleName)
    [ FsRaw (String.joinWith "\n" (Array.cons (typePrelude layout) (constructors <> definitions))) ])

compileMixed :: Module Ann -> Map String Int -> Maybe FsModule
compileMixed source@(Module module') arities = do
  guardEmptyForeign module'
  layout <- NativeLayout.fromModule source
  let
    bindings = Array.filter (not <<< isConstructor layout) (Array.concatMap Source.bindings module'.decls)
    exportedNames = exportedNamesOf module'
    signatures = Array.mapMaybe (signatureFor layout) bindings <> constructorSignatures layout
    attempted = map (\binding ->
      { binding
      , lowered: do
          signature <- Array.findMap
            (\(Tuple key value) -> if key == Qualified (Just (moduleName layout)) binding.name then Just value else Nothing)
            signatures
          value <- NativeLower.lowerBinding layout signatures signature binding.expr
          let
            owned = Destination.ownedVariables layout value.parameters value.body
            usage = Usage.usageBinding value.parameters value.body
          pure { binding, signature, lowered: value, exported: Set.member (case binding.name of Ident name -> name) exportedNames, emitWrapper: true, owned, usage }
      }) bindings
    nativeItems = Array.mapMaybe _.lowered attempted
    fallbackItems = Array.filter (\item -> case item.lowered of
      Nothing -> true
      Just _ -> false) attempted
  guard (Array.any (\item -> not (Array.null (planItem layout item).mutatedRoots)) nativeItems)
  fallbackGroups <- traverse (\item -> findBindingGroup source item.binding.name) fallbackItems
  guard (Array.all (not <<< bindUsesOwnBinder layout module') fallbackGroups)
  definitions <- renderDefinitions layout (specializeNodes layout nativeItems)
  let
    prefix = modulePrefix layout.moduleName
    fallbackDecls = Array.concatMap (CodeGen.translateBind arities (Just prefix)) fallbackGroups
  constructors <- renderConstructors layout
  pure (FsModule (sanitizeName layout.moduleName)
    (Array.cons (FsRaw (String.joinWith "\n" (Array.cons (typePrelude layout) (constructors <> definitions)))) fallbackDecls))

constructorNames :: Module Ann -> Array String
constructorNames source = case NativeLayout.fromModule source of
  Nothing -> []
  Just layout -> Array.concatMap
    (\(Tuple _ ctors) -> map (\ctor -> Names.inModule layout.moduleName (case ctor.sourceName of Qualified _ (Ident name) -> name)) ctors)
    (Map.toUnfoldable layout.types)

typePrelude :: Layout -> String
typePrelude layout =
  String.joinWith "\n"
    [ Specialized.renderTypeDeclarations layout
    , Specialized.renderCopyFunctions layout
    ]

renderDefinitions :: Layout -> Array Lowered -> Maybe (Array String)
renderDefinitions layout items = do
  let
    planned = map (planItem layout) items
    functions = map (\item ->
      { name: item.item.signature.name
      , parameters: item.item.lowered.parameters
      , body: item.body
      }) planned
    exported = Set.fromFoldable (map _.item.signature.name (Array.filter _.item.exported planned))
    summaries = Interprocedural.analyze exported functions
    mutating = mutatingParameters layout planned
  let classes = Specialized.constructorClasses layout
  natives <- traverse (nativeDefinition classes mutating) planned
  wrappers <- traverse (wrapperDefinition layout mutating) (Array.filter (\item -> item.item.emitWrapper) planned)
  let
    group = case Array.uncons natives of
      Nothing -> ""
      Just { head, tail } ->
        "let rec private " <> head
          <> String.joinWith "" (map (\item -> "\nand private " <> item) tail)
  pure (Array.cons group wrappers)

planItem :: Layout -> Lowered -> Planned
planItem layout item =
  let rewritten = Destination.rewriteDestinations layout item.owned item.lowered.body
  in
    { item
    , body: rewritten.expression
    , mutatedRoots: rewritten.mutatedRoots
    , owned: item.owned
    , usage: item.usage
    }

-- | Fixpoint over direct mutations and calls into mutating definitions. The
-- | result is intentionally conservative: a caller that can reach a mutation
-- | treats its ADT parameters as mutable, which forces a copy at the public
-- | boundary and ownership transfer/copy at internal calls.
mutatingParameters :: Layout -> Array Planned -> Map String (Set Int)
mutatingParameters _layout planned = fixedPoint initial
  where
  initial = Map.fromFoldable (map direct planned)

  direct item =
    Tuple item.item.signature.name
      (Set.fromFoldable
        (Array.mapWithIndex
          (\index parameter ->
            if isAdt parameter.type && Array.elem parameter.name item.mutatedRoots
              then Just index
              else Nothing)
          item.item.lowered.parameters
          # Array.mapMaybe identity))

  fixedPoint current =
    let next = Map.fromFoldable (map (update current) planned)
    in if next == current then current else fixedPoint next

  update current item =
    let
      direct' = case Map.lookup item.item.signature.name current of
        Just names -> names
        Nothing -> Set.empty
      reachesMutation = Array.any (calleeMutates current) (callsIn item.body)
      propagated = if reachesMutation
        then Set.fromFoldable
          (Array.mapWithIndex
            (\index parameter -> if isAdt parameter.type then Just index else Nothing)
            item.item.lowered.parameters
            # Array.mapMaybe identity)
        else Set.empty
    in Tuple item.item.signature.name (Set.union direct' propagated)

  calleeMutates current call =
    case Map.lookup call.callee current of
      Just names -> not (Set.isEmpty names) && Array.any isAdtArgument call.arguments
      Nothing -> false

  isAdtArgument argument = case argument of
    NativeVar _ ty -> isAdt ty
    NativeConstructor _ _ -> true
    NativeApply (NativeVar _ (NativeTypes.NativeFunction _ result)) _ -> isAdt result
    _ -> false

  isAdt = case _ of
    NativeTypes.NativeAdt _ _ -> true
    _ -> false

type Call =
  { callee :: String
  , arguments :: Array NativeExpr
  }

callsIn :: NativeExpr -> Array Call
callsIn = case _ of
  NativeVar _ _ -> []
  NativeIntLit _ -> []
  NativeBooleanLit _ -> []
  NativeStringLit _ -> []
  NativeLambda _ body -> callsIn body
  NativeApply function arguments ->
    (case function of
      NativeVar callee _ -> [ { callee, arguments } ]
      _ -> [])
      <> callsIn function
      <> Array.concatMap callsIn arguments
  NativeLet _ _ value body -> callsIn value <> callsIn body
  NativeMatch targets cases fallback ->
    Array.concatMap callsIn targets
      <> Array.concatMap (callsIn <<< _.body) cases
      <> case fallback of
          Nothing -> []
          Just body -> callsIn body
  NativeConstructor _ arguments -> Array.concatMap callsIn arguments
  NativeReuse _ target arguments -> callsIn target <> Array.concatMap callsIn arguments
  NativeSetField target _ value body -> callsIn target <> callsIn value <> callsIn body
  NativeProject _ _ value -> callsIn value
  NativePrimitive _ arguments -> Array.concatMap callsIn arguments
  NativeBox expression -> callsIn expression
  NativeCopy _ expression -> callsIn expression
  NativeCast _ expression -> callsIn expression

aliasedVariables :: NativeExpr -> Set String
aliasedVariables = go
  where
  go = case _ of
    NativeVar _ _ -> Set.empty
    NativeIntLit _ -> Set.empty
    NativeBooleanLit _ -> Set.empty
    NativeStringLit _ -> Set.empty
    NativeLambda _ body -> go body
    NativeApply function arguments -> Set.unions (Array.cons (go function) (map go arguments))
    NativeLet name _ value body ->
      let
        aliases = case value of
          NativeVar other _ -> Set.fromFoldable [ name, other ]
          _ -> Set.empty
      in Set.unions [ go value, go body, aliases ]
    NativeMatch targets cases fallback ->
      Set.unions
        ( map go targets
            <> map caseAliases cases
            <> case fallback of
                Nothing -> []
                Just body -> [ go body ]
        )
    NativeConstructor _ arguments -> Set.unions (map go arguments)
    NativeReuse _ target arguments -> Set.unions (Array.cons (go target) (map go arguments))
    NativeSetField target _ value body -> Set.unions [ go target, go value, go body ]
    NativeProject _ _ value -> go value
    NativePrimitive _ arguments -> Set.unions (map go arguments)
    NativeBox expression -> go expression
    NativeCopy _ expression -> go expression
    NativeCast _ expression -> go expression

  caseAliases case' =
    Set.unions
      ( map patternAliases case'.patterns
          <> [ go case'.body ]
      )

  patternAliases = case _ of
    Pattern.PatternWildcard -> Set.empty
    Pattern.PatternVariable _ -> Set.empty
    Pattern.PatternNamed name inner ->
      let
        group = case inner of
          Pattern.PatternVariable other -> Set.fromFoldable [ name, other ]
          Pattern.PatternNamed other _ -> Set.fromFoldable [ name, other ]
          _ -> Set.empty
      in Set.union group (patternAliases inner)
    Pattern.PatternInt _ -> Set.empty
    Pattern.PatternBoolean _ -> Set.empty
    Pattern.PatternString _ -> Set.empty
    Pattern.PatternConstructor _ fields -> Set.unions (map patternAliases fields)

ensureOwnedCalls :: Set String -> Set String -> Usage.Summary -> Map String (Set Int) -> NativeExpr -> NativeExpr
ensureOwnedCalls owned aliased usage mutating = go
  where
  go = case _ of
    NativeVar name ty -> NativeVar name ty
    NativeIntLit value -> NativeIntLit value
    NativeBooleanLit value -> NativeBooleanLit value
    NativeStringLit value -> NativeStringLit value
    NativeLambda parameters body -> NativeLambda parameters (go body)
    NativeApply function arguments ->
      let
        function' = go function
        arguments' = Array.mapWithIndex (argument function) arguments
      in NativeApply function' arguments'
    NativeLet name ty value body -> NativeLet name ty (go value) (go body)
    NativeMatch targets cases fallback ->
      NativeMatch (map go targets)
        (map (\case' -> case' { body = go case'.body }) cases)
        (map go fallback)
    NativeConstructor name arguments -> NativeConstructor name (map go arguments)
    NativeReuse name target arguments -> NativeReuse name (go target) (map go arguments)
    NativeSetField target index value body -> NativeSetField (go target) index (go value) (go body)
    NativeProject name index value -> NativeProject name index (go value)
    NativePrimitive primitive arguments -> NativePrimitive primitive (map go arguments)
    NativeBox expression -> NativeBox (go expression)
    NativeCopy ty expression -> NativeCopy ty (go expression)
    NativeCast constructor expression -> NativeCast constructor (go expression)

  argument function index argument' =
    let
      callee = case function of
        NativeVar name _ -> name
        _ -> ""
      parameterTypes = case function of
        NativeVar _ (NativeTypes.NativeFunction args _) -> args
        _ -> []
      mutatingIndices = case Map.lookup callee mutating of
        Just indices -> indices
        Nothing -> Set.empty
      parameterType = case Array.index parameterTypes index of
        Just ty -> ty
        Nothing -> NativeTypes.NativeBoxed
      copies = Set.member index mutatingIndices
    in
      if not copies || not (isAdt parameterType)
        then go argument'
        else
          let value = go argument'
          in if transferable value then value else NativeCopy parameterType value

  transferable expression = case expression of
    NativeVar name _ -> Set.member name owned && not (Set.member name aliased) && Usage.useCount usage name <= 1
    NativeConstructor _ arguments -> Array.all transferable arguments
    NativeIntLit _ -> true
    NativeBooleanLit _ -> true
    NativeStringLit _ -> true
    NativeLambda _ _ -> true
    NativePrimitive _ _ -> true
    NativeCopy _ _ -> true
    NativeCast _ value -> transferable value
    NativeApply (NativeVar _ (NativeTypes.NativeFunction _ result)) _ -> isAdt result
    NativeReuse _ _ _ -> true
    NativeSetField _ _ _ _ -> true
    _ -> false

  isAdt = case _ of
    NativeTypes.NativeAdt _ _ -> true
    _ -> false

nativeDefinition :: Map String String -> Map String (Set Int) -> Planned -> Maybe String
nativeDefinition classes mutating planned = do
  let
    item = planned.item
    body = ensureOwnedCalls planned.owned (aliasedVariables planned.body) planned.usage mutating planned.body
    parameters = map (\parameter -> "(" <> parameter.name <> ": " <> nativeTypeName parameter.type <> ")") item.lowered.parameters
  bodyCode <- Specialized.renderExpr classes body
  pure (item.signature.name <> " " <> String.joinWith " " parameters <> " = " <> bodyCode)

wrapperDefinition :: Layout -> Map String (Set Int) -> Planned -> Maybe String
wrapperDefinition layout mutating planned =
  let
    item = planned.item
    public = Names.inModule layout.moduleName (case item.binding.name of Ident name -> name)
    copiedIndices = case Map.lookup item.signature.name mutating of
      Just indices -> indices
      Nothing -> Set.empty
  in pure (Specialized.renderWrapperWithCopies public item.signature.args item.signature.name copiedIndices)

findBindingGroup :: Module Ann -> Ident -> Maybe (Bind Ann)
findBindingGroup (Module module') name =
  Array.find (\group -> Array.any (\binding -> binding.name == name) (Source.bindings group)) module'.decls

isConstructor :: Layout -> Source.SourceBinding Ann -> Boolean
isConstructor layout binding =
  case NativeLayout.lookupConstructor layout (Qualified (Just (moduleName layout)) binding.name) of
    Just _ -> true
    Nothing -> false

guardEmptyForeign :: forall f c. { foreign :: Map Ident f, classDecls :: Array c | _ } -> Maybe Unit
guardEmptyForeign source =
  if Map.isEmpty source.foreign && Array.null source.classDecls then Just unit else Nothing

exportedNamesOf :: { exports :: Array Ident | _ } -> Set String
exportedNamesOf module' = Set.fromFoldable (map (\(Ident name) -> name) module'.exports)

moduleName :: Layout -> ModuleName
moduleName layout = ModuleName layout.moduleName


-- | Private node variants turn a call of the form `f color left key right`,
-- | where the arguments are exactly the fields of an owned constructor node,
-- | into `f_node node`. The caller writes changed fields first. This is the
-- | destination-passing shape needed for in-place rotations.
specializeNodes :: Layout -> Array Lowered -> Array Lowered
specializeNodes layout items =
  let
    variants = Array.mapMaybe (nodeVariant layout) items
    byOriginal = Map.fromFoldable (map (\variant -> Tuple variant.original variant) variants)
    rewritten = map (\item ->
      let
        owned = Destination.ownedVariables layout item.lowered.parameters item.lowered.body
        body = rewriteNodeCalls layout byOriginal owned item.lowered.body
      in item { lowered = item.lowered { body = body } }) items
  in rewritten <> map _.lowered variants

type NodeVariant =
  { original :: String
  , name :: String
  , constructor :: Constructor
  , treeType :: NativeType
  , lowered :: Lowered
  }

nodeVariant :: Layout -> Lowered -> Maybe NodeVariant
nodeVariant layout item = do
  match <- case item.lowered.body of
    NativeMatch targets cases fallback
      | Array.length targets == Array.length item.lowered.parameters -> Just { targets, cases, fallback }
    _ -> Nothing
  guard (Array.all (\(Tuple target parameter) -> case target of
    NativeVar name _ -> name == parameter.name
    _ -> false) (Array.zip match.targets item.lowered.parameters))
  let parameterTypes = map _.type item.lowered.parameters
  constructor <- findConstructorFor layout parameterTypes
  guard (not (Array.null parameterTypes))
  cases <- traverse (combineCase constructor item.lowered.parameters) match.cases
  let
    treeType = NativeTypes.NativeAdt constructor.typeName []
    nodeParameter = { name: "sharpurs_node", type: treeType }
    body = NativeMatch [ NativeVar nodeParameter.name treeType ] cases match.fallback
    lowered =
      { binding: item.binding
      , signature:
          { name: item.signature.name <> "_node"
          , args: [ treeType ]
          , result: item.signature.result
          }
      , lowered: { parameters: [ nodeParameter ], body }
      , exported: false
      , emitWrapper: false
      , owned: Destination.ownedVariables layout [ nodeParameter ] body
      , usage: Usage.usageBinding [ nodeParameter ] body
      }
  pure { original: item.signature.name, name: lowered.signature.name, constructor, treeType, lowered }

combineCase :: Constructor -> Array NativeParameter -> NativeCase -> Maybe NativeCase
combineCase constructor parameters case' = do
  guard (Array.length case'.patterns == Array.length parameters)
  let
    combined = Pattern.PatternConstructor constructor.name
      (Array.zipWith
        (\parameter pattern ->
          case pattern of
            Pattern.PatternNamed name _ -> Pattern.PatternNamed name pattern
            _ -> Pattern.PatternNamed parameter.name pattern)
        parameters
        case'.patterns)
    bindings = Array.foldl
      (\acc parameter -> Map.insert parameter.name parameter.type acc)
      case'.bindings
      parameters
  pure { patterns: [ combined ], bindings, body: case'.body }

findConstructorFor :: Layout -> Array NativeType -> Maybe Constructor
findConstructorFor layout parameterTypes = Array.findMap
  (\(Tuple _ constructors) -> Array.findMap
    (\constructor -> if constructor.fields == parameterTypes then Just constructor else Nothing)
    constructors)
  (Map.toUnfoldable layout.types :: Array (Tuple String (Array Constructor)))

type AvailableNode =
  { variable :: String
  , type :: NativeType
  , constructor :: Constructor
  , binders :: Array (Maybe String)
  }

rewriteNodeCalls :: Layout -> Map String NodeVariant -> Set String -> NativeExpr -> NativeExpr
rewriteNodeCalls layout variants owned = go Map.empty
  where
  go available expression = case expression of
    NativeVar name ty -> NativeVar name ty
    NativeIntLit value -> NativeIntLit value
    NativeBooleanLit value -> NativeBooleanLit value
    NativeStringLit value -> NativeStringLit value
    NativeLambda parameters body -> NativeLambda parameters (go available body)
    NativeApply function arguments ->
      let
        function' = go available function
        arguments' = map (go available) arguments
      in case function' of
          NativeVar callee (NativeTypes.NativeFunction args result) ->
            case Map.lookup callee variants of
              Just variant -> case rewriteCall available variant arguments' result of
                Just rewritten -> rewritten
                Nothing -> NativeApply function' arguments'
              Nothing -> NativeApply function' arguments'
          _ -> NativeApply function' arguments'
    NativeLet name ty value body -> NativeLet name ty (go available value) (go available body)
    NativeMatch targets cases fallback ->
      NativeMatch
        (map (go available) targets)
        (map (rewriteCase available targets) cases)
        (map (go available) fallback)
    NativeConstructor name arguments -> NativeConstructor name (map (go available) arguments)
    NativeReuse name target arguments -> NativeReuse name (go available target) (map (go available) arguments)
    NativeSetField target index value body -> NativeSetField (go available target) index (go available value) (go available body)
    NativeProject name index value -> NativeProject name index (go available value)
    NativePrimitive primitive arguments -> NativePrimitive primitive (map (go available) arguments)
    NativeBox expression' -> NativeBox (go available expression')
    NativeCopy ty value -> NativeCopy ty (go available value)
    NativeCast constructor value -> NativeCast constructor (go available value)

  rewriteCall available variant arguments result = do
    let
      candidates = Array.filter (\node -> node.constructor.name == variant.constructor.name && Set.member node.variable owned) (Array.fromFoldable (Map.values available) :: Array AvailableNode)
    node <- Array.head candidates
    changes <- traverse (argField node) (Array.zip (Array.range 0 (Array.length arguments - 1)) arguments)
    let
      changed = Array.mapMaybe identity changes
      target = NativeCast node.constructor.name (NativeVar node.variable node.type)
      call = NativeApply (NativeVar variant.name (NativeTypes.NativeFunction [ variant.treeType ] result)) [ NativeVar node.variable node.type ]
    pure (Array.foldr
      (\(Tuple index argument) rest ->
        if Array.elem index changed
          then NativeSetField target index argument rest
          else rest)
      call
      (Array.zip (Array.range 0 (Array.length arguments - 1)) arguments))

  argField node (Tuple index argument) =
    case Array.index node.binders index of
      Just (Just binder) -> case argument of
        NativeVar name _ | name == binder -> Just Nothing
        _ | Array.elem binder (freeVariables argument) -> Just (Just index)
          | otherwise -> Nothing
      _ -> Nothing

  fieldType node index = case Array.index node.constructor.fields index of
    Just ty -> ty
    Nothing -> NativeBoxed

  rewriteCase available targets case' =
    let
      extended = Array.foldl (extendAvailable targets) available (Array.zip targets case'.patterns)
    in case' { body = go extended case'.body }

  extendAvailable targets available (Tuple target pattern) = case target, pattern of
    NativeVar name ty, Pattern.PatternConstructor constructorName fields -> case findConstructor layout constructorName of
      Just constructor ->
        Map.insert name
          { variable: name
          , type: ty
          , constructor
          , binders: map binderName fields
          }
          available
      Nothing -> available
    _, _ -> available

  binderName = case _ of
    Pattern.PatternVariable name -> Just name
    Pattern.PatternNamed name _ -> Just name
    _ -> Nothing

findConstructor :: Layout -> String -> Maybe Constructor
findConstructor layout name = Array.findMap
  (\(Tuple _ constructor) -> if constructor.name == name then Just constructor else Nothing)
  (Map.toUnfoldable layout.constructors :: Array (Tuple _ Constructor))

freeVariables :: NativeExpr -> Array String
freeVariables = case _ of
  NativeVar name _ -> [ name ]
  NativeIntLit _ -> []
  NativeBooleanLit _ -> []
  NativeStringLit _ -> []
  NativeLambda parameters body -> Array.filter (\name -> not (Array.elem name (map _.name parameters))) (freeVariables body)
  NativeApply function arguments -> Array.nub (freeVariables function <> Array.concatMap freeVariables arguments)
  NativeLet name _ value body -> Array.nub (freeVariables value <> Array.filter (_ /= name) (freeVariables body))
  NativeMatch targets cases fallback ->
    Array.nub
      (Array.concatMap freeVariables targets
        <> Array.concatMap (\case' -> Array.filter (\name -> not (Array.elem name (Array.concatMap Pattern.variables case'.patterns))) (freeVariables case'.body)) cases
        <> case fallback of
            Nothing -> []
            Just body -> freeVariables body)
  NativeConstructor _ arguments -> Array.concatMap freeVariables arguments
  NativeReuse _ target arguments -> Array.nub (freeVariables target <> Array.concatMap freeVariables arguments)
  NativeSetField target _ value body -> Array.nub (freeVariables target <> freeVariables value <> freeVariables body)
  NativeProject _ _ value -> freeVariables value
  NativePrimitive _ arguments -> Array.concatMap freeVariables arguments
  NativeBox expression -> freeVariables expression
  NativeCopy _ expression -> freeVariables expression
  NativeCast _ expression -> freeVariables expression

renderConstructors :: Layout -> Maybe (Array String)
renderConstructors layout = pure
  (Array.concatMap
    (\(Tuple _ ctors) -> map (Specialized.renderConstructorWithLayout layout) ctors)
    (Map.toUnfoldable layout.types))

signatureFor :: Layout -> Source.SourceBinding Ann -> Maybe (Tuple (Qualified Ident) NativeLower.Signature)
signatureFor layout binding = do
  let lambda = Source.lambdas binding.expr
  args <- traverse parameterType lambda.parameters
  result <- Source.annotation lambda.body >>= NativeLayout.sourceType layout
  let public = Names.inModule layout.moduleName (case binding.name of Ident name -> name)
  pure (Tuple (Qualified (Just (moduleName layout)) binding.name)
    { name: Names.adtNative public, args, result })
  where
  parameterType parameter = case annType parameter.ann of
    Just (C.Func args _) -> case Array.uncons args of
      Just { head } -> NativeLayout.sourceType layout head
      Nothing -> Nothing
    _ -> Nothing
  annType (Ann annotation) = annotation.type

constructorSignatures :: Layout -> Array (Tuple (Qualified Ident) NativeLower.Signature)
constructorSignatures layout = Array.concatMap
  (\(Tuple _ ctors) -> map
    (\ctor -> Tuple ctor.sourceName
      { name: Names.adtNative (Names.inModule layout.moduleName (case ctor.sourceName of Qualified _ (Ident name) -> name))
      , args: ctor.fields
      , result: NativeTypes.NativeAdt ctor.typeName []
      })
    ctors)
  (Map.toUnfoldable layout.types)

loweredBinding :: Layout -> Array (Tuple (Qualified Ident) NativeLower.Signature) -> Set String -> Source.SourceBinding Ann -> Maybe Lowered
loweredBinding layout signatures exportedNames binding = do
  signature <- Array.findMap
    (\(Tuple key value) -> if key == Qualified (Just (moduleName layout)) binding.name then Just value else Nothing)
    signatures
  lowered <- NativeLower.lowerBinding layout signatures signature binding.expr
  let
    owned = Destination.ownedVariables layout lowered.parameters lowered.body
    usage = Usage.usageBinding lowered.parameters lowered.body
  pure { binding, signature, lowered, exported: Set.member (case binding.name of Ident name -> name) exportedNames, emitWrapper: true, owned, usage }



compileMixedFor :: Set String -> Module Ann -> Map String Int -> Maybe FsModule
compileMixedFor safe source arities =
  if Set.member (moduleNameString source) safe then compileMixed source arities else Nothing

moduleNameString :: Module Ann -> String
moduleNameString (Module module') = case module'.name of ModuleName name -> name

-- | A class-based producer is only sound when no other module destructures
-- | its constructors. Construction through public wrappers remains compatible,
-- | but a foreign binder would expect an F# union case. This scan is structural
-- | and independent of benchmark or module names.
classSafeModules :: Array (Module Ann) -> Set String
classSafeModules modules =
  let
    consumers = Array.concatMap (\source -> map (Tuple (moduleNameString source)) (moduleBinderOwners source)) modules
    unsafe = Set.fromFoldable
      (map snd (Array.filter (\(Tuple consumer owner) -> consumer /= owner) consumers))
  in Set.fromFoldable (map moduleNameString modules) `Set.difference` unsafe

bindUsesOwnBinder :: Layout -> { decls :: Array (Bind Ann) | _ } -> Bind Ann -> Boolean
bindUsesOwnBinder layout module' group =
  Array.elem layout.moduleName (moduleBinderOwners (Module (module' { decls = [ group ] })))

moduleBinderOwners :: Module Ann -> Array String
moduleBinderOwners (Module module') = Array.concatMap bindingOwners module'.decls
  where
  bindingOwners group = Array.concatMap (exprOwners <<< (\(C.Binding _ _ expression) -> expression)) (bindingsOf group)

  bindingsOf = case _ of
    C.NonRec binding -> [ binding ]
    C.Rec bindings -> bindings

  exprOwners = case _ of
    C.ExprVar _ _ -> []
    C.ExprLit _ literal -> literalOwners literal
    C.ExprConstructor _ _ _ _ -> []
    C.ExprAccessor _ value _ -> exprOwners value
    C.ExprUpdate _ value props -> exprOwners value <> Array.concatMap (\(C.Prop _ item) -> exprOwners item) props
    C.ExprAbs _ _ body -> exprOwners body
    C.ExprApp _ function argument -> exprOwners function <> exprOwners argument
    C.ExprCase _ targets alternatives ->
      Array.concatMap exprOwners targets
        <> Array.concatMap alternativeOwners alternatives
    C.ExprLet _ groups body -> Array.concatMap bindingOwners groups <> exprOwners body
    C.ExprTypeApp _ value _ -> exprOwners value

  literalOwners = case _ of
    C.LitArray items -> Array.concatMap exprOwners items
    C.LitRecord props -> Array.concatMap (\(C.Prop _ item) -> exprOwners item) props
    _ -> []

  alternativeOwners (C.CaseAlternative binders guard) =
    Array.concatMap binderOwners binders
      <> case guard of
          C.Unconditional body -> exprOwners body
          C.Guarded guards -> Array.concatMap (\(C.Guard condition body) -> exprOwners condition <> exprOwners body) guards

  binderOwners = case _ of
    C.BinderNull _ -> []
    C.BinderVar _ _ -> []
    C.BinderNamed _ _ inner -> binderOwners inner
    C.BinderLit _ literal -> literalBinderOwners literal
    C.BinderConstructor _ _ qualified binders ->
      (case qualified of
        Qualified (Just (ModuleName owner)) _ -> [ owner ]
        _ -> [])
        <> Array.concatMap binderOwners binders

  literalBinderOwners = case _ of
    C.LitArray items -> Array.concatMap binderOwners items
    C.LitRecord props -> Array.concatMap (\(C.Prop _ item) -> binderOwners item) props
    _ -> []
