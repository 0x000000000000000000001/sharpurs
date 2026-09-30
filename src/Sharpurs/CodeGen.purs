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
import Data.Traversable (traverse)
import Data.Tuple (Tuple(..))
import PureScript.Backend.Optimizer.Convert (BackendModule)
import PureScript.Backend.Optimizer.CoreFn (Module(..), Bind(..), Binding(..), Expr(..), Ident(..), Literal(..), CaseAlternative(..), CaseGuard(..), Guard(..), Binder(..), Ann, DataDecl, DataConstructor, unQualified, ExprType(..), Prop(..), Qualified(..))
import PureScript.Backend.Optimizer.Syntax (BackendOperatorOrd(..), BackendOperatorNum(..))
import Sharpurs.AdtKernel (UnaryModule)
import Sharpurs.AdtLayout as AdtLayout
import Sharpurs.CodeGen.Boxed as Boxed
import Sharpurs.ConstructorCall as ConstructorCall
import Sharpurs.DirectCall as DirectCall
import Sharpurs.FsAst (FsDecl(..), FsExpr(..), FsModule(..), FsType(..), FsDUCase(..), FsMatchCase(..), FsPattern(..), FsDataCtor(..), sanitizeName)
import Sharpurs.IntArithmetic as IntArithmetic
import Sharpurs.IntComparison as IntComparison
import Sharpurs.IntKernel (IntKernel, fromBinding)
import Sharpurs.IntKernel.CodeGen (printKernel)
import Sharpurs.Optimized as Optimized
import Sharpurs.ThunkKernel as ThunkKernel

-- Keep constructor identity/layout knowledge for patterns even when expression
-- calls must cross the public object ABI of a native producer.
type ConstructorEnv =
  { arities :: Map String Int
  , wrappers :: Set String
  , native :: Maybe UnaryModule
  , direct :: Map (Qualified Ident) DirectCall.Candidate
  , thunks :: Maybe ThunkKernel.ThunkModule
  }

boxedConstructors :: Map String Int -> ConstructorEnv
boxedConstructors arities = { arities, wrappers: Set.empty, native: Nothing, direct: Map.empty, thunks: Nothing }

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
  translateModuleUsing (boxedConstructors arities) kernels expressions

translateModuleUsing :: ConstructorEnv -> Map Ident IntKernel -> Map Ident FsExpr -> Module Ann -> FsModule
translateModuleUsing adtCtors kernels expressions (Module m) =
  let
    modNameStr = unwrap m.name
    modPrefix = String.replaceAll (Pattern ".") (Replacement "_") modNameStr
    translateDataCtor c = FsDataCtor (modPrefix <> "_" <> sanitizeName c.name <> "usd_Ctor") (Array.length c.fields)
    translateBoxedDataDecl decl = FsDeclData (modPrefix <> "_" <> sanitizeName decl.name) (map translateDataCtor decl.constructors)
    nameStr = sanitizeName (String.replaceAll (Pattern ".") (Replacement "_") modNameStr)
    dataDecls = case adtCtors.native of
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
      entry <- DirectCall.fromBinding (map _.layout adtCtors.native) binding
      let
        name = emittedName entry.name
        replaced = Map.member entry.name kernels || Map.member entry.name expressions
          || fromMaybe false (map (Map.member entry.name <<< _.bindings) adtCtors.native)
        collision = Set.member (name <> "_direct") sourceNames
          || Set.member (name <> "_direct_apply") sourceNames
          || Array.elem (name <> "_direct") entry.args
          || Array.elem (name <> "_direct_apply") entry.args
      if replaced || collision then Nothing
      else Just (Tuple (Qualified (Just m.name) entry.name) entry)
    direct = Map.fromFoldable (Array.mapMaybe select m.decls)
    env = adtCtors { direct = direct }
    decls = Array.concatMap (translateBindUsingOptimizations env kernels expressions modPrefix) m.decls
  in
    FsModule nameStr (dataDecls <> fromMaybe [] (map _.declarations adtCtors.thunks) <> decls)

translateBindWithKernels :: Map String Int -> Map Ident IntKernel -> String -> Bind Ann -> Array FsDecl
translateBindWithKernels adtCtors kernels = translateBindWithOptimizations adtCtors kernels Map.empty

translateBindWithOptimizations :: Map String Int -> Map Ident IntKernel -> Map Ident FsExpr -> String -> Bind Ann -> Array FsDecl
translateBindWithOptimizations arities kernels expressions =
  translateBindUsingOptimizations (boxedConstructors arities) kernels expressions

translateBindUsingOptimizations :: ConstructorEnv -> Map Ident IntKernel -> Map Ident FsExpr -> String -> Bind Ann -> Array FsDecl
translateBindUsingOptimizations adtCtors kernels expressions modPrefix binding =
  case candidate >>= (\name -> adtCtors.native >>= \selected -> Map.lookup name selected.bindings) of
    Just declaration -> [ declaration ]
    Nothing ->
      case candidate >>= (\name -> Tuple name <$> Map.lookup name kernels) of
        Just (Tuple (Ident name) kernel) -> [ printKernel (sanitizeName (modPrefix <> "_" <> name)) kernel ]
        Nothing -> case binding of
          NonRec (Binding _ ident@(Ident name) _) | Just expr <- Map.lookup ident expressions ->
            [ FsLet (sanitizeName (modPrefix <> "_" <> name)) [] expr ]
          NonRec (Binding _ name _) ->
            case Array.find (\entry -> entry.name == name) (Array.fromFoldable (Map.values adtCtors.direct)) of
              Just entry -> translateDirectBinding adtCtors modPrefix entry
              Nothing -> translateBindUsing adtCtors (Just modPrefix) binding
          _ -> translateBindUsing adtCtors (Just modPrefix) binding
  where
  -- Keep mutual groups intact: their fallback bodies may call each other's
  -- generated _tco entry points. A singleton has no such external dependency.
  candidate = case binding of
    NonRec (Binding _ name _) -> Just name
    Rec [ Binding _ name _ ] -> Just name
    _ -> Nothing

translateDirectBinding :: ConstructorEnv -> String -> DirectCall.Candidate -> Array FsDecl
translateDirectBinding env modPrefix entry =
  [ Boxed.directBinding
      { name: sanitizeName (modPrefix <> "_" <> unwrap entry.name)
      , args: entry.args
      , body: translateExpr env Map.empty (Just modPrefix) entry.body
      }
  ]

translateBind :: Map String Int -> Maybe String -> Bind Ann -> Array FsDecl
translateBind arities = translateBindUsing (boxedConstructors arities)

translateBindUsing :: ConstructorEnv -> Maybe String -> Bind Ann -> Array FsDecl
translateBindUsing adtCtors currentMod = case _ of
  NonRec binding -> expandBind binding
  Rec bindings ->
    let
      recArities = Array.foldl (\acc (Binding _ (Ident name) expr) ->
        let arity = Array.length (extractArgs expr).args
        in if arity > 0 then Map.insert (sanitizeName (fromMaybe "" currentMod <> "_" <> name)) arity acc
           else acc) Map.empty bindings
      translateRecursive (Binding _ (Ident name) expr) =
        let
          extracted = extractArgs expr
          prefix = fromMaybe "" (map (_ <> "_") currentMod)
        in
          { name: sanitizeName (prefix <> name)
          , args: extracted.args
          , body: translateExpr adtCtors recArities currentMod extracted.body
          }
    in [ Boxed.recursiveBindings (map translateRecursive bindings) ]
  where
    expandBind :: Binding Ann -> Array FsDecl
    expandBind (Binding _ (Ident name) val) =
      let prefix = case currentMod of
            Just m -> m <> "_"
            Nothing -> ""
      in [FsLet (sanitizeName (prefix <> name)) [] (translateExpr adtCtors Map.empty currentMod val)]

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

translateLit :: ConstructorEnv -> Map String Int -> Maybe String -> Literal (Expr Ann) -> FsExpr
translateLit adtCtors localEnv currentMod lit = case lit of
  LitInt value -> FsLitInt value
  LitNumber value -> FsLitNumber value
  LitString value -> FsLitString value
  LitChar value -> FsLitChar value
  LitBoolean value -> FsLitBool value
  LitArray items -> Boxed.array (map (translateExpr adtCtors localEnv currentMod) items)
  LitRecord props -> Boxed.record (translateRecordFields adtCtors localEnv currentMod props)

translateRecordFields :: ConstructorEnv -> Map String Int -> Maybe String -> Array (Prop (Expr Ann)) -> Array { key :: String, value :: FsExpr }
translateRecordFields adtCtors localEnv currentMod =
  map (\(Prop key value) -> { key, value: translateExpr adtCtors localEnv currentMod value })

generateConstructorCall :: ConstructorEnv -> String -> Int -> Array FsExpr -> FsExpr
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

translateExpr :: ConstructorEnv -> Map String Int -> Maybe String -> Expr Ann -> FsExpr
translateExpr adtCtors localEnv currentMod expr =
  case adtCtors.thunks >>= \selected -> ThunkKernel.fromExpr selected expr of
    Just native -> native
    Nothing -> translateDirectCall adtCtors localEnv currentMod expr

translateDirectCall :: ConstructorEnv -> Map String Int -> Maybe String -> Expr Ann -> FsExpr
translateDirectCall adtCtors localEnv currentMod expr =
  case DirectCall.fromCall adtCtors.direct expr of
    Just call ->
      let
        name = sanitizeName (fromMaybe "" currentMod <> "_" <> unwrap call.candidate.name)
        argument value = Boxed.box (translateExpr adtCtors localEnv currentMod value)
      in FsDirectApp (name <> "_direct_apply") (map argument call.args)
    Nothing -> translateIntComparison adtCtors localEnv currentMod expr

translateIntComparison :: ConstructorEnv -> Map String Int -> Maybe String -> Expr Ann -> FsExpr
translateIntComparison adtCtors localEnv currentMod expr =
  case IntComparison.fromExpr expr of
    Just comparison ->
      let
        emit operator = Boxed.intBinary operator
          (translateExpr adtCtors localEnv currentMod comparison.left)
          (translateExpr adtCtors localEnv currentMod comparison.right)
      in case comparison.operator of
        OpLt -> emit Boxed.LessThan
        OpGt -> emit Boxed.GreaterThan
        _ -> translateExprFallback adtCtors localEnv currentMod expr
    Nothing -> translateIntArithmetic adtCtors localEnv currentMod expr

translateIntArithmetic :: ConstructorEnv -> Map String Int -> Maybe String -> Expr Ann -> FsExpr
translateIntArithmetic adtCtors localEnv currentMod expr =
  case IntArithmetic.fromExpr expr of
    Just arithmetic ->
      let
        emit operator = Boxed.intBinary operator
          (translateExpr adtCtors localEnv currentMod arithmetic.left)
          (translateExpr adtCtors localEnv currentMod arithmetic.right)
      in case arithmetic.operator of
        OpAdd -> emit Boxed.Add
        OpSubtract -> emit Boxed.Subtract
        _ -> translateExprFallback adtCtors localEnv currentMod expr
    Nothing -> translateExprFallback adtCtors localEnv currentMod expr

translateExprFallback :: ConstructorEnv -> Map String Int -> Maybe String -> Expr Ann -> FsExpr
translateExprFallback adtCtors localEnv currentMod expr =
  case ConstructorCall.fromExpr adtCtors.arities currentMod expr of
    Just call -> generateConstructorCall adtCtors call.name call.arity
      (map (translateExpr adtCtors localEnv currentMod) call.args)
    Nothing -> translateExprGeneric adtCtors localEnv currentMod expr

translateExprGeneric :: ConstructorEnv -> Map String Int -> Maybe String -> Expr Ann -> FsExpr
translateExprGeneric adtCtors localEnv currentMod expr = case expr of
  ExprLit _ lit -> translateLit adtCtors localEnv currentMod lit
  ExprConstructor _ _ (Ident name) _ ->
    let fqName = case currentMod of
                   Just cmod -> String.replaceAll (Pattern ".") (Replacement "_") cmod <> "_" <> name
                   Nothing -> name
    in case Map.lookup (sanitizeName fqName) adtCtors.arities of
      Just arity -> generateConstructorCall adtCtors (sanitizeName fqName) arity []
      Nothing -> Boxed.reference (sanitizeName name)
  ExprVar _ qi -> 
    let nameStr = unwrap (unQualified qi) in
    let fqName = case qi of
          Qualified (Just modName) _ -> String.replaceAll (Pattern ".") (Replacement "_") (unwrap modName) <> "_" <> nameStr
          Qualified Nothing _ -> case currentMod of
                                   Just cmod -> String.replaceAll (Pattern ".") (Replacement "_") cmod <> "_" <> nameStr
                                   Nothing -> nameStr
    in case Map.lookup (sanitizeName nameStr) localEnv of
      Just arity | arity < 0 ->
        -- Local recursive binding used as a value: it only has a `_tco`
        -- entry, so build the curried function from it.
        Boxed.etaExpand (sanitizeName nameStr <> "_tco") (negate arity) []
      _ ->
        case Map.lookup (sanitizeName fqName) adtCtors.arities of
          Just arity -> generateConstructorCall adtCtors (sanitizeName fqName) arity []
          Nothing ->
            case qi of
              Qualified (Just modName) (Ident name) -> 
                let mname = String.replaceAll (Pattern ".") (Replacement "_") (unwrap modName)
                in Boxed.reference (sanitizeName (mname <> "_" <> name))
              Qualified Nothing (Ident name) -> Boxed.reference (sanitizeName name)
  ExprApp _ _ _ -> 
    let flat = flattenApp expr
    in case flat.fn of
      ExprConstructor _ _ (Ident name) _ -> 
        let fqName = case currentMod of
                       Just cmod -> String.replaceAll (Pattern ".") (Replacement "_") cmod <> "_" <> name
                       Nothing -> name
        in case Map.lookup (sanitizeName fqName) adtCtors.arities of
           Just arity -> generateConstructorCall adtCtors (sanitizeName fqName) arity (map (translateExpr adtCtors localEnv currentMod) flat.args)
           Nothing -> FsCtorApp (sanitizeName fqName <> "usd_Ctor") (map (translateExpr adtCtors localEnv currentMod) flat.args)
      ExprVar _ qi ->
        let nameStr = unwrap (unQualified qi) in
        let fqName = case qi of
              Qualified (Just modName) _ -> String.replaceAll (Pattern ".") (Replacement "_") (unwrap modName) <> "_" <> nameStr
              Qualified Nothing _ -> case currentMod of
                                       Just cmod -> String.replaceAll (Pattern ".") (Replacement "_") cmod <> "_" <> nameStr
                                       Nothing -> nameStr
        in let mArity = case Map.lookup (sanitizeName nameStr) localEnv of
                  Just a -> Just { arity: a, targetName: sanitizeName nameStr }
                  Nothing -> case Map.lookup (sanitizeName fqName) localEnv of
                    Just a -> Just { arity: a, targetName: sanitizeName fqName }
                    Nothing -> Nothing
        in case mArity of
          Just { arity: arity, targetName: targetName } ->
             let arity' = if arity < 0 then negate arity else arity in
             if Array.length flat.args == arity' then
                FsDirectApp (targetName <> "_tco") (map (translateExpr adtCtors localEnv currentMod) flat.args)
             else if Array.length flat.args > arity' then
                let tcoArgs = Array.take arity' flat.args
                    restArgs = Array.drop arity' flat.args
                    baseCall = FsDirectApp (targetName <> "_tco") (map (translateExpr adtCtors localEnv currentMod) tcoArgs)
                in Array.foldl (\acc arg -> FsApp acc [translateExpr adtCtors localEnv currentMod arg]) baseCall restArgs
             else if arity < 0 then
                -- Local recursive binding: only its uncurried `_tco` entry
                -- exists, so an unsaturated use builds the curried value.
                Boxed.etaExpand (targetName <> "_tco") (arity' - Array.length flat.args)
                  (map (translateExpr adtCtors localEnv currentMod) flat.args)
             else
                FsApp (translateExpr adtCtors localEnv currentMod flat.fn) (map (translateExpr adtCtors localEnv currentMod) flat.args)
          Nothing ->
            case Map.lookup (sanitizeName fqName) adtCtors.arities of
              Just arity -> generateConstructorCall adtCtors (sanitizeName fqName) arity (map (translateExpr adtCtors localEnv currentMod) flat.args)
              Nothing -> FsApp (translateExpr adtCtors localEnv currentMod flat.fn) (map (translateExpr adtCtors localEnv currentMod) flat.args)
      _ -> FsApp (translateExpr adtCtors localEnv currentMod flat.fn) (map (translateExpr adtCtors localEnv currentMod) flat.args)
  -- A case over an uninhabited type has no alternatives at all. The branch is
  -- unreachable, so it keeps the ordinary pattern-match failure.
  ExprCase _ _ alts | Array.null alts -> Boxed.patternFailure
  ExprCase _ exprs alts -> 
    let 
      matchExpr = Boxed.matchValue (map (translateExpr adtCtors localEnv currentMod) exprs)
      plainMatch = FsMatch matchExpr (Array.concatMap (translateCaseAlternative adtCtors localEnv currentMod) alts)
    in if Array.length exprs == 1 then
         fromMaybe plainMatch (translateCaseTrie adtCtors localEnv currentMod matchExpr alts)
       else plainMatch
  ExprAbs _ (Ident arg) body -> Boxed.lambda (sanitizeName arg) (translateExpr adtCtors localEnv currentMod body)
  ExprAccessor _ obj prop -> Boxed.accessRecord prop (translateExpr adtCtors localEnv currentMod obj)
  ExprTypeApp _ inner _ -> translateExpr adtCtors localEnv currentMod inner
  ExprLet _ binds body -> 
    let
      newEnv = Array.foldl (\acc b -> case b of
        Rec bindings -> Array.foldl (\acc2 (Binding _ (Ident n) e) -> 
          let ext = extractArgs e 
          in if Array.length ext.args > 0 then Map.insert (sanitizeName n) (negate (Array.length ext.args)) acc2 else acc2
        ) acc bindings
        _ -> acc
      ) localEnv binds

      translateLocal = case _ of
        NonRec (Binding _ (Ident name) value) ->
          Boxed.LocalValue (sanitizeName name) (translateExpr adtCtors newEnv currentMod value)
        Rec bindings -> Boxed.LocalRecursive (map translateRecursive bindings)
      translateRecursive (Binding _ (Ident name) value) =
        let extracted = extractArgs value
        in
          { name: sanitizeName name
          , args: extracted.args
          , body: translateExpr adtCtors newEnv currentMod extracted.body
          }
    in Boxed.letIn (map translateLocal binds) (translateExpr adtCtors newEnv currentMod body)
  ExprUpdate _ obj props ->
    Boxed.updateRecord (translateExpr adtCtors localEnv currentMod obj)
      (translateRecordFields adtCtors localEnv currentMod props)

-- | Patterns made only of nullary/var leaves and unary constructor chains.
-- | They can be compiled to nested matches instead of nested patterns: a
-- | nested pattern becomes `Unbox(...)` in F#, and F# compiles deeply nested
-- | active patterns extremely slowly (the derived `Generic` dictionaries of
-- | big sum types make it allocate gigabytes), while the equivalent chain of
-- | small matches is cheap.
data SimpleBinder
  = SimpleNull
  | SimpleVar String
  | SimpleCtor String (Maybe SimpleBinder)
  | SimpleRaw FsPattern

simpleBinderDepth :: SimpleBinder -> Int
simpleBinderDepth = case _ of
  SimpleCtor _ (Just inner) -> 1 + simpleBinderDepth inner
  _ -> 1

moduleNamePrefix :: Maybe String -> String
moduleNamePrefix = case _ of
  Just name -> String.replaceAll (Pattern ".") (Replacement "_") name <> "_"
  Nothing -> ""

constructorFqName :: Maybe String -> Qualified Ident -> String
constructorFqName currentMod qi =
  let name = unwrap (unQualified qi) in
    case qi of
      Qualified (Just modName) _ -> String.replaceAll (Pattern ".") (Replacement "_") (unwrap modName) <> "_" <> name
      Qualified Nothing _ -> moduleNamePrefix currentMod <> name

-- | Nesting depth of the pattern as emitted (newtype layers are erased).
binderNestingDepth :: ConstructorEnv -> Maybe String -> Binder Ann -> Int
binderNestingDepth adtCtors currentMod = case _ of
  BinderConstructor _ _ qi binders ->
    case Map.lookup (sanitizeName (constructorFqName currentMod qi)) adtCtors.arities of
      Just arity | arity == Array.length binders ->
        if Array.null binders then 1
        else 1 + Array.foldl (\acc child -> max acc (binderNestingDepth adtCtors currentMod child)) 0 binders
      _ -> case binders of
        [ inner ] -> binderNestingDepth adtCtors currentMod inner
        _ -> 1 + Array.foldl (\acc child -> max acc (binderNestingDepth adtCtors currentMod child)) 0 binders
  BinderNamed _ _ inner -> binderNestingDepth adtCtors currentMod inner
  _ -> 0

normalizeSimpleBinder :: ConstructorEnv -> Map String Int -> Maybe String -> Binder Ann -> Maybe SimpleBinder
normalizeSimpleBinder adtCtors localEnv currentMod binder = case binder of
  BinderNull _ -> Just SimpleNull
  BinderVar _ (Ident name) -> Just (SimpleVar (sanitizeName name))
  BinderConstructor _ _ qi binders ->
    case Map.lookup (sanitizeName (constructorFqName currentMod qi)) adtCtors.arities of
      Just arity | arity == Array.length binders ->
        case binders of
          [] -> Just (SimpleCtor (sanitizeName (constructorFqName currentMod qi) <> "usd_Ctor") Nothing)
          [ inner ] -> map (\simple -> SimpleCtor (sanitizeName (constructorFqName currentMod qi) <> "usd_Ctor") (Just simple)) (normalizeSimpleBinder adtCtors localEnv currentMod inner)
          _ -> simpleRawBinder adtCtors localEnv currentMod binder
      _ -> case binders of
        [ inner ] -> normalizeSimpleBinder adtCtors localEnv currentMod inner
        _ -> simpleRawBinder adtCtors localEnv currentMod binder
  _ -> simpleRawBinder adtCtors localEnv currentMod binder

-- | Keep a small subtree with its existing nested pattern instead of failing
-- | the whole case; nested patterns only become expensive when they are deep.
simpleRawBinder :: ConstructorEnv -> Map String Int -> Maybe String -> Binder Ann -> Maybe SimpleBinder
simpleRawBinder adtCtors localEnv currentMod binder =
  if binderNestingDepth adtCtors currentMod binder <= 2 then
    Just (SimpleRaw (translateBinder adtCtors localEnv currentMod binder))
  else Nothing

simpleCaseShape :: ConstructorEnv -> Map String Int -> Maybe String -> CaseAlternative Ann -> Maybe SimpleBinder
simpleCaseShape adtCtors localEnv currentMod = case _ of
  CaseAlternative binders (Unconditional _) | Array.length binders == 1 -> do
    binder <- Array.head binders
    normalizeSimpleBinder adtCtors localEnv currentMod binder
  _ -> Nothing

type SimpleCaseEntry = Tuple SimpleBinder FsExpr

type SimpleCaseGroup = Tuple String (Array (Tuple (Maybe SimpleBinder) FsExpr))

type SimpleCaseGroups = { groups :: Array SimpleCaseGroup, raws :: Array (Tuple FsPattern FsExpr), default :: Maybe SimpleCaseEntry }

scanSimpleCaseEntries :: SimpleCaseGroups -> Array SimpleCaseEntry -> SimpleCaseGroups
scanSimpleCaseEntries acc remaining = case Array.uncons remaining of
  Nothing -> acc
  Just { head: Tuple simple body, tail } -> case simple of
    SimpleNull -> acc { default = Just (Tuple SimpleNull body) }
    SimpleVar name -> acc { default = Just (Tuple (SimpleVar name) body) }
    SimpleRaw pattern -> scanSimpleCaseEntries acc { raws = Array.snoc acc.raws (Tuple pattern body) } tail
    SimpleCtor name inner ->
      let
        entry = Tuple inner body
        groups = case Array.findIndex (\(Tuple groupName _) -> groupName == name) acc.groups of
          Just index -> fromMaybe acc.groups (Array.modifyAt index (\(Tuple groupName groupEntries) -> Tuple groupName (Array.snoc groupEntries entry)) acc.groups)
          Nothing -> Array.snoc acc.groups (Tuple name [ entry ])
      in scanSimpleCaseEntries acc { groups = groups } tail

-- | Compile the alternatives of a single-scrutinee `Case` into nested matches
-- | when their binders form deep unary constructor chains.
translateCaseTrie :: ConstructorEnv -> Map String Int -> Maybe String -> FsExpr -> Array (CaseAlternative Ann) -> Maybe FsExpr
translateCaseTrie adtCtors localEnv currentMod matchExpr alts = do
  shape <- traverse (simpleCaseShape adtCtors localEnv currentMod) alts
  let maxDepth = Array.foldl (\acc simple -> max acc (simpleBinderDepth simple)) 0 shape
  if maxDepth < 4 then Nothing
  else do
    bodies <- traverse (case _ of
      CaseAlternative _ (Unconditional expr) -> Just (translateExpr adtCtors localEnv currentMod expr)
      _ -> Nothing) alts
    pure (compileSimpleCaseLevel 0 matchExpr matchExpr (Array.zipWith Tuple shape bodies) Boxed.patternFailure)

compileSimpleCaseLevel :: Int -> FsExpr -> FsExpr -> Array SimpleCaseEntry -> FsExpr -> FsExpr
compileSimpleCaseLevel depth boxedValue scrutinee entries fallback =
  let
    scanned = scanSimpleCaseEntries { groups: [], raws: [], default: Nothing } entries
    fallbackBody = case scanned.default of
      Just (Tuple _ body) -> body
      Nothing -> fallback
    cases = Array.mapWithIndex (compileSimpleCaseGroup depth fallbackBody) scanned.groups
    rawCases = map (\(Tuple pattern body) -> FsMatchCase pattern Nothing body) scanned.raws
    defaultCases = case scanned.default of
      Just (Tuple SimpleNull body) -> [ FsMatchCase FsPatWildcard Nothing body ]
      Just (Tuple (SimpleVar name) body) ->
        if depth == 0 then
          [ FsMatchCase (FsPatIdent name) Nothing body ]
        else
          [ FsMatchCase FsPatWildcard Nothing (Boxed.letIn [ Boxed.LocalValue name boxedValue ] body) ]
      Just _ -> []
      Nothing -> [ FsMatchCase FsPatWildcard Nothing fallback ]
  in FsMatch scrutinee (cases <> rawCases <> defaultCases)

compileSimpleCaseGroup :: Int -> FsExpr -> Int -> SimpleCaseGroup -> FsMatchCase
compileSimpleCaseGroup depth fallbackBody index (Tuple name groupEntries) = case Array.head groupEntries of
  Just (Tuple Nothing body) -> FsMatchCase (FsPatCtor name []) Nothing body
  Just (Tuple (Just SimpleNull) body) -> FsMatchCase (FsPatCtor name [ FsPatWildcard ]) Nothing body
  Just (Tuple (Just (SimpleVar boundName)) body) -> FsMatchCase (FsPatCtor name [ FsPatIdent boundName ]) Nothing body
  _ ->
    let
      variable = "usd_case_" <> show depth <> "_" <> show index
      innerEntries = map (\(Tuple inner body) -> Tuple (fromMaybe SimpleNull inner) body) groupEntries
    in FsMatchCase (FsPatCtor name [ FsPatIdent variable ]) Nothing
         (compileSimpleCaseLevel (depth + 1) (FsIdent variable) (Boxed.unbox (FsIdent variable)) innerEntries fallbackBody)

translateCaseAlternative :: ConstructorEnv -> Map String Int -> Maybe String -> CaseAlternative Ann -> Array FsMatchCase
translateCaseAlternative adtCtors localEnv currentMod (CaseAlternative binders guards) =
  let
    fsPatterns = map (translateBinder adtCtors localEnv currentMod) binders
    combinedPat = case Array.length fsPatterns of
      0 -> FsPatWildcard
      1 -> fromMaybe FsPatWildcard (Array.head fsPatterns)
      _ -> FsPatTuple fsPatterns
  in
    case guards of
      Unconditional expr -> [FsMatchCase combinedPat Nothing (Boxed.parenthesize (translateExpr adtCtors localEnv currentMod expr))]
      Guarded array ->
        map (\(Guard guard expr) -> FsMatchCase combinedPat (Just (translateExpr adtCtors localEnv currentMod guard)) (Boxed.parenthesize (translateExpr adtCtors localEnv currentMod expr))) array

translateBinder :: ConstructorEnv -> Map String Int -> Maybe String -> Binder Ann -> FsPattern
translateBinder adtCtors localEnv currentMod = case _ of
  BinderNull _ -> FsPatWildcard
  BinderVar _ (Ident name) -> FsPatIdent (sanitizeName name)
  BinderLit _ lit -> translateLitBinder adtCtors localEnv currentMod lit
  BinderConstructor _ _ qi binders ->
    let name = unwrap (unQualified qi) in
    let modPrefix = case currentMod of
          Just m -> String.replaceAll (Pattern ".") (Replacement "_") m <> "_"
          Nothing -> ""
    in
    let fqName = case qi of
          Qualified (Just modName) _ -> String.replaceAll (Pattern ".") (Replacement "_") (unwrap modName) <> "_" <> name
          Qualified Nothing _ -> modPrefix <> name
    in if Map.member (sanitizeName fqName) adtCtors.arities then
      FsPatCtor (sanitizeName fqName <> "usd_Ctor") (map (translateBinder adtCtors localEnv currentMod) binders)
    else
      case Array.index binders 0 of
        Just inner -> translateBinder adtCtors localEnv currentMod inner
        Nothing -> FsPatWildcard
  BinderNamed _ (Ident name) inner ->
    FsPatNamed (sanitizeName name) (translateBinder adtCtors localEnv currentMod inner)

translateLitBinder :: ConstructorEnv -> Map String Int -> Maybe String -> Literal (Binder Ann) -> FsPattern
translateLitBinder adtCtors localEnv currentMod = case _ of
    LitBoolean b -> FsPatLitBool b
    LitInt i -> FsPatLitInt i
    LitNumber n -> FsPatLitNumber n
    LitString s -> FsPatLitString s
    LitChar c -> FsPatLitChar c
    LitArray items -> FsPatArray (map (translateBinder adtCtors localEnv currentMod) items)
    LitRecord props ->
      if Array.length props == 0 then
        FsPatWildcard
      else
        FsPatRecord (map (\(Prop key value) -> { key, pattern: translateBinder adtCtors localEnv currentMod value }) props)
