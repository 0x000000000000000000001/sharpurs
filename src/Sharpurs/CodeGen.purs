module Sharpurs.CodeGen
  ( translateModule
  , translateModuleWithConstructorWrappers
  , translateOptimizedModule
  , translateOptimizedModuleWithAdts
  , translateOptimizedModuleWithThunks
  , translateModuleWithKernels
  , translateModuleWithOptimizations
  , translateModuleUsing
  , translateBind
  , translateBindWithKernels
  , translateBindWithOptimizations
  ) where

import Prelude

import Data.Array as Array
import Data.Map (Map)
import Data.Map as Map
import Data.Maybe (Maybe(..), fromMaybe)
import Data.Newtype (unwrap)
import Data.Set (Set)
import Data.Set as Set
import PureScript.Backend.Optimizer.Convert (BackendModule)
import PureScript.Backend.Optimizer.CoreFn (Module(..), Bind(..), Binding(..), Expr(..), Ident(..), Literal(..), Ann, Prop(..), Qualified(..))
import Sharpurs.AdtKernel (AdtModule)
import Sharpurs.AdtLayout as AdtLayout
import Sharpurs.Analysis.Source as Source
import Sharpurs.CodeGen.Boxed as Boxed
import Sharpurs.CodeGen.Case as Case
import Sharpurs.CodeGen.Context (Context, ModuleEnv, RecursiveFunction, RecursiveScope(..))
import Sharpurs.CodeGen.Context as Context
import Sharpurs.CodeGen.Selection as Selection
import Sharpurs.DirectCall as DirectCall
import Sharpurs.FsAst (FsDecl(..), FsExpr(..), FsModule(..), FsDataCtor(..), modulePrefix, sanitizeName)
import Sharpurs.IntKernel (IntKernel)
import Sharpurs.IntKernel.CodeGen (printKernel)
import Sharpurs.Names as Names
import Sharpurs.ThunkKernel as ThunkKernel

translateModuleWithConstructorWrappers :: Set String -> Map String Int -> Module Ann -> FsModule
translateModuleWithConstructorWrappers wrappers arities =
  translateModuleUsing { arities, wrappers, native: Nothing, direct: Map.empty, thunks: Nothing } Selection.emptyCandidates

translateModule :: Map String Int -> Module Ann -> FsModule
translateModule adtCtors = translateModuleWithKernels adtCtors Map.empty

translateOptimizedModule :: Map String Int -> BackendModule -> Module Ann -> FsModule
translateOptimizedModule = translateOptimizedModuleWithAdts Set.empty Nothing

translateOptimizedModuleWithAdts :: Set String -> Maybe AdtModule -> Map String Int -> BackendModule -> Module Ann -> FsModule
translateOptimizedModuleWithAdts wrappers native = translateOptimizedModuleWithThunks wrappers native Nothing

translateOptimizedModuleWithThunks :: Set String -> Maybe AdtModule -> Maybe ThunkKernel.ThunkModule -> Map String Int -> BackendModule -> Module Ann -> FsModule
translateOptimizedModuleWithThunks wrappers native thunks arities backendMod =
  translateModuleUsing { arities, wrappers, native, direct: Map.empty, thunks } (Selection.fromBackend backendMod)

translateModuleWithKernels :: Map String Int -> Map Ident IntKernel -> Module Ann -> FsModule
translateModuleWithKernels adtCtors kernels = translateModuleWithOptimizations adtCtors kernels Map.empty

translateModuleWithOptimizations :: Map String Int -> Map Ident IntKernel -> Map Ident FsExpr -> Module Ann -> FsModule
translateModuleWithOptimizations arities kernels expressions =
  translateModuleUsing (Context.boxedModule arities) { kernels, expressions }

-- Explicit candidate injection uses the same selection policy as the CLI.
translateModuleUsing :: ModuleEnv -> Selection.BindingCandidates -> Module Ann -> FsModule
translateModuleUsing env candidates source@(Module m) =
  let
    plan = Selection.prepareModule env candidates source
    modNameStr = unwrap m.name
    modPrefix = modulePrefix modNameStr
    translateDataCtor c = FsDataCtor (modPrefix <> "_" <> sanitizeName c.name <> "usd_Ctor") (Array.length c.fields)
    translateBoxedDataDecl decl = FsDeclData (modPrefix <> "_" <> sanitizeName decl.name) (map translateDataCtor decl.constructors)
    nameStr = sanitizeName modPrefix
    dataDecls = case env.native of
      Just selected -> [ FsRaw (AdtLayout.printDeclarations selected.layout) ]
      Nothing -> map translateBoxedDataDecl m.dataDecls
    decls = Array.concatMap (translateBindingPlan plan.env modPrefix) plan.bindings
  in
    FsModule nameStr (dataDecls <> fromMaybe [] (map _.declarations env.thunks) <> decls)

translateBindWithKernels :: Map String Int -> Map Ident IntKernel -> String -> Bind Ann -> Array FsDecl
translateBindWithKernels adtCtors kernels = translateBindWithOptimizations adtCtors kernels Map.empty

translateBindWithOptimizations :: Map String Int -> Map Ident IntKernel -> Map Ident FsExpr -> String -> Bind Ann -> Array FsDecl
translateBindWithOptimizations arities kernels expressions =
  translateBindUsingOptimizations (Context.boxedModule arities) { kernels, expressions }

translateBindUsingOptimizations :: ModuleEnv -> Selection.BindingCandidates -> String -> Bind Ann -> Array FsDecl
translateBindUsingOptimizations env candidates modPrefix =
  translateBindingPlan env modPrefix <<< Selection.forBinding env candidates

translateBindingPlan :: ModuleEnv -> String -> Selection.BindingPlan -> Array FsDecl
translateBindingPlan env modPrefix = case _ of
  Selection.NativeAdt declaration -> [ declaration ]
  Selection.NativeInt { name, kernel } -> [ printKernel (sanitizeName (modPrefix <> "_" <> unwrap name)) kernel ]
  Selection.OptimizedExpression { name, expression } -> [ FsLet (sanitizeName (modPrefix <> "_" <> unwrap name)) [] expression ]
  Selection.DirectFunction entry -> translateDirectBinding env modPrefix entry
  Selection.GenericBinding binding -> translateBindUsing env (Just modPrefix) binding

translateDirectBinding :: ModuleEnv -> String -> DirectCall.Candidate -> Array FsDecl
translateDirectBinding env modPrefix entry =
  [ Boxed.directBinding
      { name: sanitizeName (modPrefix <> "_" <> unwrap entry.name)
      , args: entry.args
      , body: translateExpr (Context.forModule env (Just modPrefix)) entry.body
      }
  ]

translateBind :: Map String Int -> Maybe String -> Bind Ann -> Array FsDecl
translateBind arities = translateBindUsing (Context.boxedModule arities)

translateBindUsing :: ModuleEnv -> Maybe String -> Bind Ann -> Array FsDecl
translateBindUsing env currentMod = case _ of
  NonRec binding -> expandBind binding
  Rec bindings ->
    let
      recursiveContext = registerRecursiveBindings TopLevel
        (\name -> sanitizeName (fromMaybe "" currentMod <> "_" <> name)) bindings context
      translateRecursive (Binding _ (Ident name) expr) =
        let
          extracted = extractArgs expr
          prefix = fromMaybe "" (map (_ <> "_") currentMod)
        in
          { name: sanitizeName (prefix <> name)
          , args: extracted.args
          , body: translateExpr recursiveContext extracted.body
          }
    in [ Boxed.recursiveBindings (map translateRecursive bindings) ]
  where
    context = Context.forModule env currentMod

    expandBind :: Binding Ann -> Array FsDecl
    expandBind (Binding _ (Ident name) val) =
      let prefix = case currentMod of
            Just m -> m <> "_"
            Nothing -> ""
      in [FsLet (sanitizeName (prefix <> name)) [] (translateExpr context val)]

translateLit :: Context -> Literal (Expr Ann) -> FsExpr
translateLit context lit = case lit of
  LitInt value -> FsLitInt value
  LitNumber value -> FsLitNumber value
  LitString value -> FsLitString value
  LitChar value -> FsLitChar value
  LitBoolean value -> FsLitBool value
  LitArray items -> Boxed.array (map (translateExpr context) items)
  LitRecord props -> Boxed.record (translateRecordFields context props)

translateRecordFields :: Context -> Array (Prop (Expr Ann)) -> Array { key :: String, value :: FsExpr }
translateRecordFields context =
  map (\(Prop key value) -> { key, value: translateExpr context value })

generateConstructorCall :: ModuleEnv -> String -> Int -> Array FsExpr -> FsExpr
generateConstructorCall env name arity args =
  if Set.member name env.wrappers then
    if Array.length args == arity then
      -- The registered producer provides a typed native factory. Its F#
      -- signature fixes each unbox type; saturation avoids curried closures
      -- while retaining the object ABI and left-to-right argument evaluation.
      Boxed.nativeConstructor name args
    else FsApp (Boxed.reference name) args
  else generateConstructorLambda name arity args

generateConstructorLambda :: String -> Int -> Array FsExpr -> FsExpr
generateConstructorLambda name arity args =
  let argsLen = Array.length args in
  if argsLen >= arity then
    FsCtorApp (name <> "usd_Ctor") args
  else
    let
      missing = arity - argsLen
      argNames = map (\i -> "usd__arg" <> show i) (Array.range 1 missing)
      argExprs = map FsIdent argNames
      allArgs = args <> argExprs
      body = FsCtorApp (name <> "usd_Ctor") allArgs
    in
      Boxed.curry argNames body

extractArgs :: Expr Ann -> { args :: Array String, body :: Expr Ann }
extractArgs expr =
  let chain = Source.lambdas expr
  in { args: map (sanitizeName <<< unwrap <<< _.name) chain.parameters, body: chain.body }

registerRecursiveBindings :: RecursiveScope -> (String -> String) -> Array (Binding Ann) -> Context -> Context
registerRecursiveBindings scope bindingName bindings context =
  Array.foldl (\acc (Binding _ (Ident name) expr) ->
    Context.registerRecursive scope (bindingName name) (Array.length (extractArgs expr).args) acc
  ) context bindings

translateExpr :: Context -> Expr Ann -> FsExpr
translateExpr context expr = case Selection.forExpression context.moduleEnv context.currentModule expr of
  Selection.NativeThunk native -> native
  Selection.DirectInvocation call ->
    let
      name = sanitizeName (fromMaybe "" context.currentModule <> "_" <> unwrap call.name)
      argument value = Boxed.box (translateExpr context value)
    in FsDirectApp (name <> "_direct_apply") (map argument call.args)
  Selection.IntBinary call -> Boxed.intBinary call.operator
    (translateExpr context call.left) (translateExpr context call.right)
  Selection.ConstructorInvocation call -> generateConstructorCall context.moduleEnv call.name call.arity
    (map (translateExpr context) call.args)
  Selection.GenericExpression source -> translateExprGeneric context source

translateExprGeneric :: Context -> Expr Ann -> FsExpr
translateExprGeneric context expr = case expr of
  ExprLit _ lit -> translateLit context lit
  ExprConstructor _ _ ident@(Ident name) _ ->
    let fqName = Names.qualified context.currentModule (Qualified Nothing ident)
    in case Map.lookup fqName context.moduleEnv.arities of
      Just arity -> generateConstructorCall context.moduleEnv fqName arity []
      Nothing -> Boxed.reference (sanitizeName name)
  ExprVar _ qi -> translateVariable context qi
  ExprApp _ _ _ -> translateApplication context (Source.flattenApp expr)
  ExprCase _ exprs alts -> Case.translate
    { arities: context.moduleEnv.arities, currentModule: context.currentModule }
    (translateExpr context) exprs alts
  ExprAbs _ (Ident arg) body -> Boxed.lambda (sanitizeName arg) (translateExpr context body)
  ExprAccessor _ obj prop -> Boxed.accessRecord prop (translateExpr context obj)
  ExprTypeApp _ inner _ -> translateExpr context inner
  ExprLet _ binds body -> translateLet context binds body
  ExprUpdate _ obj props ->
    Boxed.updateRecord (translateExpr context obj) (translateRecordFields context props)

translateVariable :: Context -> Qualified Ident -> FsExpr
translateVariable context ident = case Context.localRecursive context ident of
  Just entry -> Boxed.etaExpand entry.worker entry.arity []
  Nothing ->
    let name = Names.qualified context.currentModule ident
    in case Map.lookup name context.moduleEnv.arities of
      Just arity -> generateConstructorCall context.moduleEnv name arity []
      Nothing -> Boxed.reference (Names.qualified Nothing ident)

translateApplication :: Context -> { fn :: Expr Ann, args :: Array (Expr Ann) } -> FsExpr
translateApplication context call =
  let
    args = map (translateExpr context) call.args
    fallback = FsApp (translateExpr context call.fn) args
  in case call.fn of
    ExprConstructor _ _ ident _ ->
      let name = Names.qualified context.currentModule (Qualified Nothing ident)
      in case Map.lookup name context.moduleEnv.arities of
        Just arity -> generateConstructorCall context.moduleEnv name arity args
        Nothing -> FsCtorApp (name <> "usd_Ctor") args
    ExprVar _ ident -> case Context.recursiveCall context ident of
      Just entry -> translateRecursiveCall entry args fallback
      Nothing ->
        let name = Names.qualified context.currentModule ident
        in case Map.lookup name context.moduleEnv.arities of
          Just arity -> generateConstructorCall context.moduleEnv name arity args
          Nothing -> fallback
    _ -> fallback

translateRecursiveCall :: RecursiveFunction -> Array FsExpr -> FsExpr -> FsExpr
translateRecursiveCall entry args fallback = case compare (Array.length args) entry.arity of
  EQ -> FsDirectApp entry.worker args
  GT ->
    let baseCall = FsDirectApp entry.worker (Array.take entry.arity args)
    in Array.foldl (\acc arg -> FsApp acc [ arg ]) baseCall (Array.drop entry.arity args)
  LT -> case entry.scope of
    Local -> Boxed.etaExpand entry.worker (entry.arity - Array.length args) args
    TopLevel -> fallback

translateLet :: Context -> Array (Bind Ann) -> Expr Ann -> FsExpr
translateLet context binds body =
  let
    localContext = Array.foldl (\acc binding -> case binding of
      Rec bindings -> registerRecursiveBindings Local sanitizeName bindings acc
      NonRec _ -> acc
    ) context binds
    translateLocal = case _ of
      NonRec (Binding _ (Ident name) value) ->
        Boxed.LocalValue (sanitizeName name) (translateExpr localContext value)
      Rec bindings -> Boxed.LocalRecursive (map translateRecursive bindings)
    translateRecursive (Binding _ (Ident name) value) =
      let extracted = extractArgs value
      in
        { name: sanitizeName name
        , args: extracted.args
        , body: translateExpr localContext extracted.body
        }
  in Boxed.letIn (map translateLocal binds) (translateExpr localContext body)
