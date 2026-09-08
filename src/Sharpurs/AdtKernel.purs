module Sharpurs.AdtKernel (UnaryModule, prepareUnary, fromModule) where

import Prelude

import Control.Alternative (guard, (<|>))
import Data.Array as Array
import Data.Array.NonEmpty as NEA
import Data.Foldable (all, foldr)
import Data.Map (Map)
import Data.Map as Map
import Data.Maybe (Maybe(..))
import Data.Newtype (unwrap)
import Data.String as String
import Data.String.Pattern (Pattern(..), Replacement(..))
import Data.Traversable (traverse)
import Data.Tuple (Tuple(..))
import PureScript.Backend.Optimizer.Convert (BackendModule, BackendBindingGroup)
import PureScript.Backend.Optimizer.CoreFn (Ann, ExprType, Ident, Module(..), Qualified(..))
import PureScript.Backend.Optimizer.CoreFn as C
import PureScript.Backend.Optimizer.Semantics (NeutralExpr(..))
import PureScript.Backend.Optimizer.Syntax (Level(..), Pair(..))
import PureScript.Backend.Optimizer.Syntax as S
import Sharpurs.AdtLayout (Layout, Ctor, fromModule, nativeType, lookupCtor, printDeclarations, validIdentifier) as Layout
import Sharpurs.FsAst (FsDecl(..), FsModule(..), escapeString, sanitizeName)

type Signature = { args :: Array ExprType, result :: ExprType, nativeName :: String, publicName :: String }
type Context = { layout :: Layout.Layout, globals :: Map (Qualified Ident) Signature, locals :: Map Level ExprType }
type Parameter = { level :: Level, type :: ExprType }

type UnaryModule =
  { layout :: Layout.Layout
  , bindings :: Map Ident FsDecl
  , nativeNames :: Array Ident
  }

type SourceBinding = { name :: Ident, expr :: C.Expr Ann, recursive :: Boolean, singleton :: Boolean }

-- Admit a closed layout only together with every constructor's public object
-- wrapper. Ordinary functions remain boxed unless one recursive-ADT argument,
-- its result and the complete optimized body can use native representations.
prepareUnary :: Module Ann -> BackendModule -> Maybe UnaryModule
prepareUnary core@(Module source) backend = do
  layout <- Layout.fromModule core
  guard (source.name == backend.name && source.dataDecls == backend.dataDecls)
  guard (Map.isEmpty source.foreign && Map.isEmpty backend.foreign)
  guard (Array.null source.classDecls && Array.null backend.classDecls)
  let
    sourceBindings = Array.concatMap sourceGroup source.decls
    backendBindings = Array.concatMap _.bindings backend.bindings
    publicNames = map (publicName layout <<< _.name) sourceBindings
    constructors = Array.concatMap _.constructors layout.declarations
    qualified name = Qualified (Just source.name) name
    context = { layout, globals: Map.empty, locals: Map.empty }
  guard (unique (map _.name sourceBindings) && unique publicNames)
  guard (unique (map (\(Tuple name _) -> name) backendBindings))
  ctorBindings <- traverse (\ctor -> do
    let name = C.unQualified ctor.sourceName
    sourceBinding <- Array.find (\item -> item.name == name) sourceBindings
    guard (not sourceBinding.recursive)
    backendGroup <- Array.find (\item -> Array.any (\(Tuple ident _) -> ident == name) item.bindings) backend.bindings
    guard (not backendGroup.recursive)
    case sourceBinding.expr of
      C.ExprConstructor _ typeName ctorName fields -> do
        checkCtorIdentity ctor ctor.sourceName typeName ctorName
        guard (Array.length fields == Array.length ctor.fields)
        guard (all identity (Array.mapWithIndex (\i field -> field == "value" <> show i) fields))
      _ -> Nothing
    expression <- lookupExpression name backendBindings
    sig <- signature layout name expression
    guard (sourceAnnotation sourceBinding.expr == Just (signatureType sig))
    definition <- emitBinding context ctor.sourceName sig expression
    wrapper <- printWrapper layout sig
    pure { name, sig, declaration: FsRaw ("let " <> definition <> "\n" <> wrapper), recursive: false }
    ) constructors
  let
    ctorSignatures = Map.fromFoldable (map (\item -> Tuple (qualified item.name) item.sig) ctorBindings)
    candidates = Array.mapMaybe (\binding -> do
      guard binding.singleton
      guard (not (Map.member (qualified binding.name) ctorSignatures))
      group <- Array.find (\item -> Array.any (\(Tuple name _) -> name == binding.name) item.bindings) backend.bindings
      guard (not group.recursive || Array.length group.bindings == 1)
      guard (group.recursive == binding.recursive)
      expression <- lookupExpression binding.name group.bindings
      sig <- signature layout binding.name expression
      guard (sourceAnnotation binding.expr == Just (signatureType sig))
      case sig.args of
        [ argument ] -> guard (recursiveArgument layout argument)
        _ -> Nothing
      let globals = if binding.recursive then Map.insert (qualified binding.name) sig ctorSignatures else ctorSignatures
      definition <- emitBinding (context { globals = globals }) (qualified binding.name) sig expression
      wrapper <- printWrapper layout sig
      bridge <- if binding.recursive then printUnaryBridge layout sig else Just ""
      pure { name: binding.name, sig, recursive: binding.recursive
           , declaration: FsRaw ((if binding.recursive then "let rec " else "let ") <> definition <> "\n" <> wrapper <> bridge) }
      ) sourceBindings
    emitted = ctorBindings <> candidates
    generatedNames = Array.concatMap (\item -> [ item.sig.nativeName ] <> if item.recursive then [ item.sig.publicName <> "_tco" ] else []) emitted
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

sourceGroup :: C.Bind Ann -> Array SourceBinding
sourceGroup = case _ of
  C.NonRec (C.Binding _ name expr) -> [ { name, expr, recursive: false, singleton: true } ]
  C.Rec [ C.Binding _ name expr ] -> [ { name, expr, recursive: true, singleton: true } ]
  C.Rec bindings -> map (\(C.Binding _ name expr) -> { name, expr, recursive: true, singleton: false }) bindings

sourceAnnotation :: C.Expr Ann -> Maybe ExprType
sourceAnnotation expr = case C.exprAnn expr of
  C.Ann ann -> ann.type

lookupExpression :: Ident -> Array (Tuple Ident NeutralExpr) -> Maybe NeutralExpr
lookupExpression name bindings = Array.findMap (\(Tuple ident expr) -> if ident == name then Just expr else Nothing) bindings

signature :: Layout.Layout -> Ident -> NeutralExpr -> Maybe Signature
signature layout name expression = do
  ty <- annotation expression
  let sig = case ty of
        C.Func args result -> { args, result }
        result -> { args: [], result }
  _ <- traverse (Layout.nativeType layout) (Array.snoc sig.args sig.result)
  let public = publicName layout name
  pure { args: sig.args, result: sig.result, nativeName: public <> "_adt_native", publicName: public }

signatureType :: Signature -> ExprType
signatureType sig = if Array.null sig.args then sig.result else C.Func sig.args sig.result

publicName :: Layout.Layout -> Ident -> String
publicName layout name = sanitizeName (String.replaceAll (Pattern ".") (Replacement "_") layout.moduleName <> "_" <> unwrap name)

recursiveArgument :: Layout.Layout -> ExprType -> Boolean
recursiveArgument layout argument = Array.any
  (\ctor -> ctor.sourceType == argument && Array.elem argument ctor.fields)
  (Array.concatMap _.constructors layout.declarations)

printUnaryBridge :: Layout.Layout -> Signature -> Maybe String
printUnaryBridge layout sig = case sig.args of
  [ argument ] -> do
    ty <- Layout.nativeType layout argument
    pure ("\nlet " <> sig.publicName <> "_tco (sharpurs_adt_arg: obj) : obj = box ("
      <> sig.nativeName <> " (unbox<" <> ty <> "> sharpurs_adt_arg))")
  _ -> Nothing

unique :: forall a. Ord a => Array a -> Boolean
unique values = Array.length (Array.nub values) == Array.length values

-- The original whole-module pilot still requires every binding to be closed
-- and supported. Production uses prepareUnary's explicit constructor wrappers
-- when mixing native functions with the object-based fallback.
fromModule :: Module Ann -> BackendModule -> Maybe FsModule
fromModule core@(Module source) backend = do
  layout <- Layout.fromModule core
  guard (source.name == backend.name && source.dataDecls == backend.dataDecls)
  guard (Map.isEmpty source.foreign && Map.isEmpty backend.foreign && Array.null backend.classDecls)
  let bindings = Array.concatMap _.bindings backend.bindings
  signatures <- traverse (\(Tuple name expr) -> do
    ty <- annotation expr
    let sig = case ty of
          C.Func args result -> { args, result }
          result -> { args: [], result }
    _ <- traverse (Layout.nativeType layout) (Array.snoc sig.args sig.result)
    let publicName = sanitizeName (String.replaceAll (Pattern ".") (Replacement "_") layout.moduleName <> "_" <> unwrap name)
    pure (Tuple (Qualified (Just backend.name) name) { args: sig.args, result: sig.result, nativeName: publicName <> "_adt_native", publicName })
    ) bindings
  let names = Array.concatMap (\(Tuple _ sig) -> [ sig.nativeName, sig.publicName ]) signatures
  guard (all Layout.validIdentifier names && Array.length (Array.nub names) == Array.length names)
  guard (all (\decl -> all (\ctor -> not (Array.elem ctor.name names)) decl.constructors) layout.declarations)
  guard (Array.length (Array.nub (map (\(Tuple name _) -> name) signatures)) == Array.length signatures)
  let globals = Map.fromFoldable signatures
  let groups = Array.concatMap (\group -> if group.recursive then [ group ] else map (\binding -> { recursive: false, bindings: [ binding ] }) group.bindings) backend.bindings
  emitted <- emitGroups { layout, globals: Map.empty, locals: Map.empty } globals groups
  pure (FsModule layout.moduleName [ FsRaw (Layout.printDeclarations layout <> "\n\n" <> emitted) ])

annotation :: NeutralExpr -> Maybe ExprType
annotation (NeutralExpr (S.Typed ty _)) = Just ty
annotation _ = Nothing

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
      definition <- emitBinding bodyContext (qualified name) sig expr
      pure { sig, definition }
      ) group.bindings
    let nativeDefinitions = String.joinWith "\n" (Array.mapWithIndex
          (\i item -> (if i == 0 then (if group.recursive then "let rec " else "let ") else "and ") <> item.definition)
          definitions)
    guard (group.recursive || Array.length definitions == 1)
    wrappers <- traverse (printWrapper ctx.layout <<< _.sig) definitions
    rest <- emitGroups (ctx { globals = available }) signatures tail
    pure (nativeDefinitions <> "\n" <> String.joinWith "\n" wrappers <> "\n\n" <> rest)

emitBinding :: Context -> Qualified Ident -> Signature -> NeutralExpr -> Maybe String
emitBinding ctx name sig expr = do
  resultType <- Layout.nativeType ctx.layout sig.result
  case Layout.lookupCtor ctx.layout name of
    Just ctor -> do
      guard (sig.args == ctor.fields && sig.result == ctor.sourceType)
      validateConstructor ctor expr
      let params = Array.mapWithIndex (\i ty -> { level: Level i, type: ty }) sig.args
      declarations <- traverse (printParameter ctx.layout) params
      pure (sig.nativeName <> " " <> String.joinWith " " declarations <> " : " <> resultType <> " = " <> ctorApplication ctor (map (localName <<< _.level) params))
    Nothing -> do
      collected <- collect sig.args sig.result [] expr
      let levels = map _.level collected.args
      guard (all (\(Level level) -> level >= 0) levels && Array.length (Array.nub levels) == Array.length levels)
      let locals = Map.fromFoldable (map (\p -> Tuple p.level p.type) collected.args)
      body <- lower (ctx { locals = locals }) sig.result collected.body
      params <- traverse (printParameter ctx.layout) collected.args
      pure (sig.nativeName <> " " <> String.joinWith " " params <> " : " <> resultType <> " = " <> body)

validateConstructor :: Layout.Ctor -> NeutralExpr -> Maybe Unit
validateConstructor ctor (NeutralExpr syntax) = case syntax of
  S.Typed ty inner -> do
    guard (ty == if Array.null ctor.fields then ctor.sourceType else C.Func ctor.fields ctor.sourceType)
    validateConstructor ctor inner
  S.CtorDef _ (C.ProperName typeName) name fields -> do
    guard (name == C.unQualified ctor.sourceName)
    guard (Array.length fields == Array.length ctor.fields && Array.length (Array.nub fields) == Array.length fields)
    guard (all identity (Array.mapWithIndex (\i field -> field == "value" <> show i) fields))
    case ctor.sourceType of
      C.ADT _ path _ -> guard (Array.last path == Just typeName)
      _ -> Nothing
  S.CtorSaturated name _ typeName ctorName fields -> do
    guard (Array.null fields && Array.null ctor.fields)
    guard (name == ctor.sourceName)
    checkCtorIdentity ctor name typeName ctorName
  _ -> Nothing

collect :: Array ExprType -> ExprType -> Array Parameter -> NeutralExpr -> Maybe { args :: Array Parameter, body :: NeutralExpr }
collect remaining result args expr@(NeutralExpr syntax) = case syntax of
  S.Typed ty inner -> do
    guard (ty == if Array.null remaining then result else C.Func remaining result)
    collect remaining result args inner
  S.Abs parameters body -> do
    let ps = NEA.toArray parameters
    guard (not (Array.null remaining) && Array.length ps <= Array.length remaining)
    let next = Array.zipWith (\(Tuple _ level) ty -> { level, type: ty }) ps remaining
    collect (Array.drop (Array.length ps) remaining) result (args <> next) body
  _ -> do
    guard (Array.null remaining)
    pure { args, body: expr }

printParameter :: Layout.Layout -> Parameter -> Maybe String
printParameter layout p = do
  ty <- Layout.nativeType layout p.type
  pure ("(" <> localName p.level <> ": " <> ty <> ")")

localName :: Level -> String
localName (Level level) = "sharpurs_adt_local_" <> show level

printWrapper :: Layout.Layout -> Signature -> Maybe String
printWrapper layout sig = do
  types <- traverse (Layout.nativeType layout) sig.args
  let args = Array.mapWithIndex (\i ty -> { name: "sharpurs_adt_arg_" <> show i, type: ty }) types
  let call = sig.nativeName <> String.joinWith "" (map (\arg -> " (unbox<" <> arg.type <> "> " <> arg.name <> ")") args)
  pure ("let " <> sig.publicName <> " : obj = " <> foldr (\arg body -> "box (fun (" <> arg.name <> ": obj) -> " <> body <> ")") ("box (" <> call <> ")") args)

ctorApplication :: Layout.Ctor -> Array String -> String
ctorApplication ctor args = ctor.name <> if Array.null args then "" else "(" <> String.joinWith ", " args <> ")"

at :: ExprType -> ExprType -> String -> Maybe String
at expected actual code = guard (expected == actual) $> code

lower :: Context -> ExprType -> NeutralExpr -> Maybe String
lower ctx expected (NeutralExpr syntax) = case syntax of
  S.Typed ty inner -> guard (ty == expected) *> lower ctx expected inner
  S.Lit (C.LitInt value) -> at expected C.Int ("(" <> show value <> ")")
  S.Lit (C.LitBoolean value) -> at expected C.Boolean (if value then "true" else "false")
  S.Local _ level -> do
    ty <- Map.lookup level ctx.locals
    at expected ty (localName level)
  S.Var name -> do
    sig <- Map.lookup name ctx.globals
    guard (Array.null sig.args)
    at expected sig.result sig.nativeName
  S.App fn args -> do
    name <- globalReference fn
    sig <- Map.lookup name ctx.globals
    validateReference sig fn
    guard (expected == sig.result && NEA.length args == Array.length sig.args)
    values <- traverse (\(Tuple ty arg) -> lower ctx ty arg) (Array.zip sig.args (NEA.toArray args))
    pure ("(" <> sig.nativeName <> String.joinWith "" (map (\value -> " (" <> value <> ")") values) <> ")")
  S.CtorSaturated name _ typeName ctorName fields -> do
    ctor <- Layout.lookupCtor ctx.layout name
    checkCtorIdentity ctor name typeName ctorName
    guard (expected == ctor.sourceType && Array.length fields == Array.length ctor.fields)
    guard (all identity (Array.mapWithIndex (\i (Tuple field _) -> field == "value" <> show i) fields))
    values <- traverse (\(Tuple ty (Tuple _ value)) -> lower ctx ty value) (Array.zip ctor.fields fields)
    pure (ctorApplication ctor values)
  S.Accessor value (S.GetCtorField name _ typeName ctorName field index) -> do
    ctor <- Layout.lookupCtor ctx.layout name
    checkCtorIdentity ctor name typeName ctorName
    ty <- Array.index ctor.fields index
    guard (expected == ty && field == "value" <> show index)
    target <- lower ctx ctor.sourceType value
    let patterns = Array.mapWithIndex (\i _ -> if i == index then "sharpurs_adt_field" else "_") ctor.fields
    pure ("(match " <> target <> " with | " <> ctorApplication ctor patterns <> " -> sharpurs_adt_field | _ -> failwith \"Invalid ADT constructor\")")
  S.PrimOp (S.Op1 (S.OpIsTag name) value) -> do
    guard (expected == C.Boolean)
    ctor <- Layout.lookupCtor ctx.layout name
    target <- lower ctx ctor.sourceType value
    pure ("(match " <> target <> " with | " <> ctorApplication ctor (map (const "_") ctor.fields) <> " -> true | _ -> false)")
  S.PrimOp (S.Op2 op left right) -> do
    operation <- primitive op
    guard (expected == operation.result)
    a <- lower ctx C.Int left
    b <- lower ctx C.Int right
    pure ("(" <> a <> " " <> operation.symbol <> " " <> b <> ")")
  S.Let _ level@(Level n) value body -> do
    guard (n >= 0 && not (Map.member level ctx.locals) && all (_ < level) (Map.keys ctx.locals))
    ty <- typeOf ctx value
    native <- Layout.nativeType ctx.layout ty
    rhs <- lower ctx ty value
    rest <- lower (ctx { locals = Map.insert level ty ctx.locals }) expected body
    pure ("(let " <> localName level <> ": " <> native <> " = " <> rhs <> " in " <> rest <> ")")
  S.Branch branches otherwise -> do
    cases <- traverse (\(Pair condition value) -> Tuple <$> lower ctx C.Boolean condition <*> lower ctx expected value) (NEA.toArray branches)
    fallback <- lower ctx expected otherwise
    pure (foldr (\(Tuple condition value) rest -> "(if " <> condition <> " then " <> value <> " else " <> rest <> ")") fallback cases)
  S.Fail message -> Just ("(failwith " <> escapeString message <> ")")
  _ -> Nothing

checkCtorIdentity :: Layout.Ctor -> Qualified Ident -> C.ProperName -> Ident -> Maybe Unit
checkCtorIdentity ctor qualified (C.ProperName typeName) name = do
  guard (C.unQualified qualified == name)
  case ctor.sourceType of
    C.ADT _ path _ -> guard (Array.last path == Just typeName)
    _ -> Nothing

globalReference :: NeutralExpr -> Maybe (Qualified Ident)
globalReference (NeutralExpr (S.Var name)) = Just name
globalReference (NeutralExpr (S.Typed _ inner)) = globalReference inner
globalReference _ = Nothing

validateReference :: Signature -> NeutralExpr -> Maybe Unit
validateReference sig (NeutralExpr (S.Typed ty inner)) = do
  guard (ty == C.Func sig.args sig.result)
  validateReference sig inner
validateReference _ (NeutralExpr (S.Var _)) = Just unit
validateReference _ _ = Nothing

primitive :: S.BackendOperator2 -> Maybe { symbol :: String, result :: ExprType }
primitive = case _ of
  S.OpIntNum S.OpAdd -> Just { symbol: "+", result: C.Int }
  S.OpIntNum S.OpSubtract -> Just { symbol: "-", result: C.Int }
  S.OpIntOrd op -> Just { symbol: case op of
    S.OpEq -> "="
    S.OpNotEq -> "<>"
    S.OpGt -> ">"
    S.OpGte -> ">="
    S.OpLt -> "<"
    S.OpLte -> "<=", result: C.Boolean }
  _ -> Nothing

typeOf :: Context -> NeutralExpr -> Maybe ExprType
typeOf ctx (NeutralExpr syntax) = case syntax of
  S.Typed ty _ -> Just ty
  S.Local _ level -> Map.lookup level ctx.locals
  S.Lit (C.LitInt _) -> Just C.Int
  S.Lit (C.LitBoolean _) -> Just C.Boolean
  S.PrimOp (S.Op1 (S.OpIsTag _) _) -> Just C.Boolean
  S.PrimOp (S.Op2 op _ _) -> _.result <$> primitive op
  S.Var name -> do
    sig <- Map.lookup name ctx.globals
    guard (Array.null sig.args)
    pure sig.result
  S.App fn _ -> globalReference fn >>= flip Map.lookup ctx.globals <#> _.result
  S.CtorSaturated name _ _ _ _ -> _.sourceType <$> Layout.lookupCtor ctx.layout name
  S.Accessor _ (S.GetCtorField name _ _ _ _ index) -> Layout.lookupCtor ctx.layout name >>= \ctor -> Array.index ctor.fields index
  S.Let _ level value body -> do
    ty <- typeOf ctx value
    typeOf (ctx { locals = Map.insert level ty ctx.locals }) body
  S.Branch branches otherwise -> Array.findMap (\(Pair _ value) -> typeOf ctx value) (NEA.toArray branches) <|> typeOf ctx otherwise
  _ -> Nothing
