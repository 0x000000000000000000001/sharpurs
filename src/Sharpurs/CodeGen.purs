module Sharpurs.CodeGen where

import Prelude

import Data.Array as Array
import Data.Map (Map)
import Data.Map as Map
import Data.Maybe (Maybe(..), fromMaybe)
import Data.Newtype (unwrap)
import Data.Set (Set)
import Data.Set as Set
import Data.String as String
import Data.String.Pattern (Pattern(..), Replacement(..))
import Data.Tuple (Tuple(..))
import PureScript.Backend.Optimizer.Convert (BackendModule)
import PureScript.Backend.Optimizer.CoreFn (Module(..), Bind(..), Binding(..), Expr(..), Ident(..), Literal(..), Ann, DataDecl, DataConstructor, ExprType(..), Prop(..), Qualified(..))
import PureScript.Backend.Optimizer.Syntax (BackendOperatorOrd(..), BackendOperatorNum(..))
import Sharpurs.AdtKernel (UnaryModule)
import Sharpurs.AdtLayout as AdtLayout
import Sharpurs.CodeGen.Boxed as Boxed
import Sharpurs.CodeGen.Case as Case
import Sharpurs.CodeGen.Context (Context, ModuleEnv, RecursiveFunction, RecursiveScope(..))
import Sharpurs.CodeGen.Context as Context
import Sharpurs.ConstructorCall as ConstructorCall
import Sharpurs.DirectCall as DirectCall
import Sharpurs.FsAst (FsDecl(..), FsExpr(..), FsModule(..), FsType(..), FsDUCase(..), FsDataCtor(..), sanitizeName)
import Sharpurs.IntArithmetic as IntArithmetic
import Sharpurs.IntComparison as IntComparison
import Sharpurs.IntKernel (IntKernel, fromBinding)
import Sharpurs.IntKernel.CodeGen (printKernel)
import Sharpurs.Optimized as Optimized
import Sharpurs.ThunkKernel as ThunkKernel

translateModuleWithConstructorWrappers :: Set String -> Map String Int -> Module Ann -> FsModule
translateModuleWithConstructorWrappers wrappers arities =
  translateModuleUsing { arities, wrappers, native: Nothing, direct: Map.empty, thunks: Nothing } Map.empty Map.empty

translateModule :: Map String Int -> Module Ann -> FsModule
translateModule adtCtors = translateModuleWithKernels adtCtors Map.empty

translateOptimizedModule :: Map String Int -> BackendModule -> Module Ann -> FsModule
translateOptimizedModule = translateOptimizedModuleWithAdts Set.empty Nothing

translateOptimizedModuleWithAdts :: Set String -> Maybe UnaryModule -> Map String Int -> BackendModule -> Module Ann -> FsModule
translateOptimizedModuleWithAdts wrappers native = translateOptimizedModuleWithThunks wrappers native Nothing

translateOptimizedModuleWithThunks :: Set String -> Maybe UnaryModule -> Maybe ThunkKernel.ThunkModule -> Map String Int -> BackendModule -> Module Ann -> FsModule
translateOptimizedModuleWithThunks wrappers native thunks arities backendMod =
  translateModuleUsing { arities, wrappers, native, direct: Map.empty, thunks } kernels expressions
  where
  kernels = Map.fromFoldable $ Array.mapMaybe
    (\(Tuple name expr) -> Tuple name <$> fromBinding (Qualified (Just backendMod.name) name) expr)
    (Array.concatMap _.bindings backendMod.bindings)
  expressions = Map.fromFoldable $ Array.mapMaybe
    (\(Tuple name expr) -> Tuple name <$> Optimized.fromBinding expr)
    (Array.concatMap _.bindings backendMod.bindings)

translateModuleWithKernels :: Map String Int -> Map Ident IntKernel -> Module Ann -> FsModule
translateModuleWithKernels adtCtors kernels = translateModuleWithOptimizations adtCtors kernels Map.empty

translateModuleWithOptimizations :: Map String Int -> Map Ident IntKernel -> Map Ident FsExpr -> Module Ann -> FsModule
translateModuleWithOptimizations arities kernels expressions =
  translateModuleUsing (Context.boxedModule arities) kernels expressions

translateModuleUsing :: ModuleEnv -> Map Ident IntKernel -> Map Ident FsExpr -> Module Ann -> FsModule
translateModuleUsing env kernels expressions (Module m) =
  let
    modNameStr = unwrap m.name
    modPrefix = String.replaceAll (Pattern ".") (Replacement "_") modNameStr
    translateDataCtor c = FsDataCtor (modPrefix <> "_" <> sanitizeName c.name <> "usd_Ctor") (Array.length c.fields)
    translateBoxedDataDecl decl = FsDeclData (modPrefix <> "_" <> sanitizeName decl.name) (map translateDataCtor decl.constructors)
    nameStr = sanitizeName (String.replaceAll (Pattern ".") (Replacement "_") modNameStr)
    dataDecls = case env.native of
      Just selected -> [ FsRaw (AdtLayout.printDeclarations selected.layout) ]
      Nothing -> map translateBoxedDataDecl m.dataDecls
    -- Register only entries that will actually be emitted by the generic path.
    -- Qualified keys keep local binders and imported functions out of this table.
    bindingNames = Array.concatMap (case _ of
      NonRec (Binding _ name _) -> [ name ]
      Rec bindings -> map (\(Binding _ name _) -> name) bindings) m.decls
    emittedName name = sanitizeName (modPrefix <> "_" <> unwrap name)
    sourceNames = Set.fromFoldable (map emittedName
      (bindingNames <> Array.fromFoldable (Map.keys m.foreign)))
    select binding = do
      entry <- DirectCall.fromBinding (map _.layout env.native) binding
      let
        name = emittedName entry.name
        replaced = Map.member entry.name kernels || Map.member entry.name expressions
          || fromMaybe false (map (Map.member entry.name <<< _.bindings) env.native)
        collision = Set.member (name <> "_direct") sourceNames
          || Set.member (name <> "_direct_apply") sourceNames
          || Array.elem (name <> "_direct") entry.args
          || Array.elem (name <> "_direct_apply") entry.args
      if replaced || collision then Nothing
      else Just (Tuple (Qualified (Just m.name) entry.name) entry)
    direct = Map.fromFoldable (Array.mapMaybe select m.decls)
    selectedEnv = env { direct = direct }
    decls = Array.concatMap (translateBindUsingOptimizations selectedEnv kernels expressions modPrefix) m.decls
  in
    FsModule nameStr (dataDecls <> fromMaybe [] (map _.declarations env.thunks) <> decls)

translateBindWithKernels :: Map String Int -> Map Ident IntKernel -> String -> Bind Ann -> Array FsDecl
translateBindWithKernels adtCtors kernels = translateBindWithOptimizations adtCtors kernels Map.empty

translateBindWithOptimizations :: Map String Int -> Map Ident IntKernel -> Map Ident FsExpr -> String -> Bind Ann -> Array FsDecl
translateBindWithOptimizations arities kernels expressions =
  translateBindUsingOptimizations (Context.boxedModule arities) kernels expressions

translateBindUsingOptimizations :: ModuleEnv -> Map Ident IntKernel -> Map Ident FsExpr -> String -> Bind Ann -> Array FsDecl
translateBindUsingOptimizations env kernels expressions modPrefix binding =
  case candidate >>= (\name -> env.native >>= \selected -> Map.lookup name selected.bindings) of
    Just declaration -> [ declaration ]
    Nothing ->
      case candidate >>= (\name -> Tuple name <$> Map.lookup name kernels) of
        Just (Tuple (Ident name) kernel) -> [ printKernel (sanitizeName (modPrefix <> "_" <> name)) kernel ]
        Nothing -> case binding of
          NonRec (Binding _ ident@(Ident name) _) | Just expr <- Map.lookup ident expressions ->
            [ FsLet (sanitizeName (modPrefix <> "_" <> name)) [] expr ]
          NonRec (Binding _ name _) ->
            case Array.find (\entry -> entry.name == name) (Array.fromFoldable (Map.values env.direct)) of
              Just entry -> translateDirectBinding env modPrefix entry
              Nothing -> translateBindUsing env (Just modPrefix) binding
          _ -> translateBindUsing env (Just modPrefix) binding
  where
  -- Keep mutual groups intact: their fallback bodies may call each other's
  -- generated _tco entry points. A singleton has no such external dependency.
  candidate = case binding of
    NonRec (Binding _ name _) -> Just name
    Rec [ Binding _ name _ ] -> Just name
    _ -> Nothing

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

translateDataDecl :: DataDecl -> FsDecl
translateDataDecl decl =
  FsDU (sanitizeName decl.name) (map translateConstructor decl.constructors)

translateType :: ExprType -> FsType
translateType = case _ of
  String -> FsTString
  Boolean -> FsTBool
  Int -> FsTInt
  _ -> FsTCustom "obj"

translateConstructor :: DataConstructor -> FsDUCase
translateConstructor ctor =
  FsDUCase (sanitizeName ctor.name <> "usd_Ctor") (map translateType ctor.fields)

flattenApp :: Expr Ann -> { fn :: Expr Ann, args :: Array (Expr Ann) }
flattenApp (ExprApp _ f x) =
  let flat = flattenApp f
  in { fn: flat.fn, args: Array.snoc flat.args x }
flattenApp expr = { fn: expr, args: [] }

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
extractArgs (ExprAbs _ (Ident arg) body) = 
  let next = extractArgs body
  in { args: Array.cons (sanitizeName arg) next.args, body: next.body }
extractArgs e = { args: [], body: e }

registerRecursiveBindings :: RecursiveScope -> (String -> String) -> Array (Binding Ann) -> Context -> Context
registerRecursiveBindings scope bindingName bindings context =
  Array.foldl (\acc (Binding _ (Ident name) expr) ->
    Context.registerRecursive scope (bindingName name) (Array.length (extractArgs expr).args) acc
  ) context bindings

translateExpr :: Context -> Expr Ann -> FsExpr
translateExpr context expr =
  case context.moduleEnv.thunks >>= \selected -> ThunkKernel.fromExpr selected expr of
    Just native -> native
    Nothing -> translateDirectCall context expr

translateDirectCall :: Context -> Expr Ann -> FsExpr
translateDirectCall context expr =
  case DirectCall.fromCall context.moduleEnv.direct expr of
    Just call ->
      let
        name = sanitizeName (fromMaybe "" context.currentModule <> "_" <> unwrap call.candidate.name)
        argument value = Boxed.box (translateExpr context value)
      in FsDirectApp (name <> "_direct_apply") (map argument call.args)
    Nothing -> translateIntComparison context expr

translateIntComparison :: Context -> Expr Ann -> FsExpr
translateIntComparison context expr =
  case IntComparison.fromExpr expr of
    Just comparison ->
      let
        emit operator = Boxed.intBinary operator
          (translateExpr context comparison.left)
          (translateExpr context comparison.right)
      in case comparison.operator of
        OpLt -> emit Boxed.LessThan
        OpGt -> emit Boxed.GreaterThan
        _ -> translateExprFallback context expr
    Nothing -> translateIntArithmetic context expr

translateIntArithmetic :: Context -> Expr Ann -> FsExpr
translateIntArithmetic context expr =
  case IntArithmetic.fromExpr expr of
    Just arithmetic ->
      let
        emit operator = Boxed.intBinary operator
          (translateExpr context arithmetic.left)
          (translateExpr context arithmetic.right)
      in case arithmetic.operator of
        OpAdd -> emit Boxed.Add
        OpSubtract -> emit Boxed.Subtract
        _ -> translateExprFallback context expr
    Nothing -> translateExprFallback context expr

translateExprFallback :: Context -> Expr Ann -> FsExpr
translateExprFallback context expr =
  case ConstructorCall.fromExpr context.moduleEnv.arities context.currentModule expr of
    Just call -> generateConstructorCall context.moduleEnv call.name call.arity
      (map (translateExpr context) call.args)
    Nothing -> translateExprGeneric context expr

translateExprGeneric :: Context -> Expr Ann -> FsExpr
translateExprGeneric context expr = case expr of
  ExprLit _ lit -> translateLit context lit
  ExprConstructor _ _ ident@(Ident name) _ ->
    let fqName = Context.qualifiedName context.currentModule (Qualified Nothing ident)
    in case Map.lookup fqName context.moduleEnv.arities of
      Just arity -> generateConstructorCall context.moduleEnv fqName arity []
      Nothing -> Boxed.reference (sanitizeName name)
  ExprVar _ qi -> translateVariable context qi
  ExprApp _ _ _ -> translateApplication context (flattenApp expr)
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
    let name = Context.qualifiedName context.currentModule ident
    in case Map.lookup name context.moduleEnv.arities of
      Just arity -> generateConstructorCall context.moduleEnv name arity []
      Nothing -> Boxed.reference (Context.qualifiedName Nothing ident)

translateApplication :: Context -> { fn :: Expr Ann, args :: Array (Expr Ann) } -> FsExpr
translateApplication context call =
  let
    args = map (translateExpr context) call.args
    fallback = FsApp (translateExpr context call.fn) args
  in case call.fn of
    ExprConstructor _ _ ident _ ->
      let name = Context.qualifiedName context.currentModule (Qualified Nothing ident)
      in case Map.lookup name context.moduleEnv.arities of
        Just arity -> generateConstructorCall context.moduleEnv name arity args
        Nothing -> FsCtorApp (name <> "usd_Ctor") args
    ExprVar _ ident -> case Context.recursiveCall context ident of
      Just entry -> translateRecursiveCall entry args fallback
      Nothing ->
        let name = Context.qualifiedName context.currentModule ident
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
