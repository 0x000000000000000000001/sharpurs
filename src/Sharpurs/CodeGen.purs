module Sharpurs.CodeGen where

import Prelude

import Data.Array as Array
import Data.Maybe (Maybe(..), fromMaybe)
import Data.Newtype (unwrap)
import Data.String as String
import Data.String.CodeUnits as CU
import Data.String.Pattern (Pattern(..), Replacement(..))
import PureScript.Backend.Optimizer.CoreFn (Module(..), Bind(..), Binding(..), Expr(..), Ident(..), Literal(..), CaseAlternative(..), CaseGuard(..), Guard(..), Binder(..), Ann, DataDecl, DataConstructor, unQualified, ExprType(..), Prop(..), Qualified(..))
import Sharpurs.FsAst (FsDecl(..), FsExpr(..), FsModule(..), FsType(..), FsDUCase(..), FsMatchCase(..), FsPattern(..), FsDataCtor(..), sanitizeName, escapeString, escapeChar)
import Data.Set as Set
import Data.Set (Set)
import Data.Map (Map)
import Data.Map as Map
import Data.Tuple (Tuple(..))
import Data.Maybe (Maybe(..), fromMaybe)

import PureScript.Backend.Optimizer.Convert (BackendModule)
import Sharpurs.IntKernel (IntKernel, fromBinding)
import Sharpurs.IntKernel.CodeGen (printKernel)
import Sharpurs.Optimized as Optimized
import Sharpurs.AdtKernel (UnaryModule)
import Sharpurs.AdtLayout as AdtLayout
import Sharpurs.IntComparison as IntComparison
import Sharpurs.IntArithmetic as IntArithmetic
import Sharpurs.DirectCall as DirectCall
import PureScript.Backend.Optimizer.Syntax (BackendOperatorOrd(..), BackendOperatorNum(..))

-- Keep constructor identity/layout knowledge for patterns even when expression
-- calls must cross the public object ABI of a native producer.
type ConstructorEnv =
  { arities :: Map String Int
  , wrappers :: Set String
  , native :: Maybe UnaryModule
  , direct :: Map (Qualified Ident) DirectCall.Candidate
  }

boxedConstructors :: Map String Int -> ConstructorEnv
boxedConstructors arities = { arities, wrappers: Set.empty, native: Nothing, direct: Map.empty }

translateModuleWithConstructorWrappers :: Set String -> Map String Int -> Module Ann -> FsModule
translateModuleWithConstructorWrappers wrappers arities =
  translateModuleUsing { arities, wrappers, native: Nothing, direct: Map.empty } Map.empty Map.empty

translateModule :: Map String Int -> Module Ann -> FsModule
translateModule adtCtors = translateModuleWithKernels adtCtors Map.empty

translateOptimizedModule :: Map String Int -> BackendModule -> Module Ann -> FsModule
translateOptimizedModule = translateOptimizedModuleWithAdts Set.empty Nothing

translateOptimizedModuleWithAdts :: Set String -> Maybe UnaryModule -> Map String Int -> BackendModule -> Module Ann -> FsModule
translateOptimizedModuleWithAdts wrappers native arities backendMod =
  translateModuleUsing { arities, wrappers, native, direct: Map.empty } kernels expressions
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
    translateDataDecl decl = FsDeclData (modPrefix <> "_" <> sanitizeName decl.name) (map translateDataCtor decl.constructors)
    nameStr = sanitizeName (String.replaceAll (Pattern ".") (Replacement "_") modNameStr)
    dataDecls = case adtCtors.native of
      Just selected -> [ FsRaw (AdtLayout.printDeclarations selected.layout) ]
      Nothing -> map translateDataDecl m.dataDecls
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
    FsModule nameStr (dataDecls <> decls)

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
              Just entry -> printDirectBinding adtCtors modPrefix entry
              Nothing -> translateBindUsing adtCtors (Just modPrefix) binding
          _ -> translateBindUsing adtCtors (Just modPrefix) binding
  where
  -- Keep mutual groups intact: their fallback bodies may call each other's
  -- generated _tco entry points. A singleton has no such external dependency.
  candidate = case binding of
    NonRec (Binding _ name _) -> Just name
    Rec [ Binding _ name _ ] -> Just name
    _ -> Nothing

printDirectBinding :: ConstructorEnv -> String -> DirectCall.Candidate -> Array FsDecl
printDirectBinding env modPrefix entry =
  let
    name = sanitizeName (modPrefix <> "_" <> unwrap entry.name)
    args = entry.args
    parameters = String.joinWith " " (map (\arg -> "(" <> arg <> ": obj)") args)
    invocation = name <> "_direct " <> String.joinWith " " args
    body = printExprInline (translateExpr env Map.empty (Just modPrefix) entry.body)
    prefix = String.joinWith "" (map (\arg -> "(box (fun (" <> arg <> ": obj) -> ") args)
    suffix = String.joinWith "" (map (const "))") args)
  in [ FsRaw ("let " <> name <> "_direct " <> parameters <> " : obj = " <> body
    -- Arguments are evaluated before entering this method. Only failures of
    -- the saturated body receive the wrapper that sharpurs_apply would add.
    <> "\n\nlet " <> name <> "_direct_apply " <> parameters <> " : obj =\n"
    <> "    try " <> invocation <> "\n"
    <> "    with ex -> raise (System.Reflection.TargetInvocationException(ex))\n"
    -- The public curried entry already has that boundary in sharpurs_apply.
    <> "\nlet " <> name <> " = " <> prefix <> "(" <> invocation <> ")" <> suffix
    ) ]

translateBind :: Map String Int -> Maybe String -> Bind Ann -> Array FsDecl
translateBind arities = translateBindUsing (boxedConstructors arities)

translateBindUsing :: ConstructorEnv -> Maybe String -> Bind Ann -> Array FsDecl
translateBindUsing adtCtors currentMod = case _ of
  NonRec b -> expandBind adtCtors currentMod b
  Rec bindings -> 
    let
      recArities = Array.foldl (\acc (Binding _ (Ident n) e) -> 
          let ext = extractArgs e
          in Map.insert (sanitizeName (fromMaybe "" currentMod <> "_" <> n)) (Array.length ext.args) acc
        ) Map.empty bindings

      makeRec (Binding _ (Ident n) e) isFirst =
        let ext = extractArgs e
            arity = Array.length ext.args
            prefix = case currentMod of
                  Just m -> m <> "_"
                  Nothing -> ""
            sName = sanitizeName (prefix <> n)
        in if arity > 0 then
             let 
               argStrs = String.joinWith " " (map (\a -> "(" <> a <> ": obj)") ext.args)
               keyword = if isFirst then "let rec " else " and "
               bodyStr = printExprInline (translateExpr adtCtors recArities currentMod ext.body)
               curriedArgs = String.joinWith " " (map (\a -> "(fun (" <> a <> ": obj) -> ") ext.args)
               closes = String.joinWith "" (map (\_ -> ")") ext.args)
               wrapper = sName <> " = box " <> curriedArgs <> sName <> "_tco " <> String.joinWith " " ext.args <> closes
             in keyword <> sName <> "_tco " <> argStrs <> " : obj = (" <> bodyStr <> ")\n" <> "and " <> wrapper <> "\n"
           else 
             let keyword = if isFirst then "let rec " else " and "
             in keyword <> sName <> " : obj = (" <> printExprInline (translateExpr adtCtors recArities currentMod e) <> ")\n"
      
      recStr = Array.mapWithIndex (\i bd -> makeRec bd (i == 0)) bindings
    in [FsRaw (String.joinWith "" recStr)]
  where
    expandBind :: ConstructorEnv -> Maybe String -> Binding Ann -> Array FsDecl
    expandBind adtCtors currentMod (Binding _ (Ident name) val) =
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
    LitInt i -> FsIdent ("(box " <> show i <> ")")
    LitNumber n -> FsIdent ("(box " <> show n <> ")")
    LitString s -> FsLitString s
    LitChar c -> FsIdent ("(box '" <> escapeChar c <> "')")
    LitBoolean b -> FsLitBool b
    LitArray arr -> FsIdent ("(box [|" <> String.joinWith "; " (map (printExprInline <<< translateExpr adtCtors localEnv currentMod) arr) <> "|])")
    LitRecord props ->
      let
        mapAdd (Prop key val) acc = "(Map.add \"" <> key <> "\" (box (" <> printExprInline (translateExpr adtCtors localEnv currentMod val) <> ")) " <> acc <> ")"
      in FsIdent ("(box (" <> Array.foldr mapAdd "Map.empty" props <> "))")

generateConstructorCall :: ConstructorEnv -> String -> Int -> Array FsExpr -> FsExpr
generateConstructorCall env name arity args =
  if Set.member name env.wrappers then
    if Array.length args == arity then
      -- The registered producer provides a typed native factory. Its F#
      -- signature fixes each unbox type; saturation avoids curried closures
      -- while retaining the object ABI and left-to-right argument evaluation.
      FsIdent ("(box (" <> name <> "_adt_native"
        <> String.joinWith "" (map (\arg -> " (unbox (" <> printExprInline arg <> "))") args) <> "))")
    else FsApp (FsIdent ("(box " <> name <> ")")) args
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
      FsIdent ("(box (" <> Array.foldr (\arg acc -> "(fun (" <> arg <> ": obj) -> " <> acc <> ")") (printExprInline body) argNames <> "))")

extractArgs :: Expr Ann -> { args :: Array String, body :: Expr Ann }
extractArgs (ExprAbs _ (Ident arg) body) = 
  let next = extractArgs body
  in { args: Array.cons (sanitizeName arg) next.args, body: next.body }
extractArgs e = { args: [], body: e }

translateExpr :: ConstructorEnv -> Map String Int -> Maybe String -> Expr Ann -> FsExpr
translateExpr adtCtors localEnv currentMod expr =
  case DirectCall.fromCall adtCtors.direct expr of
    Just call ->
      let
        name = sanitizeName (fromMaybe "" currentMod <> "_" <> unwrap call.candidate.name)
        argument value = FsIdent ("(box (" <> printExprInline (translateExpr adtCtors localEnv currentMod value) <> "))")
      in FsDirectApp (name <> "_direct_apply") (map argument call.args)
    Nothing -> translateIntComparison adtCtors localEnv currentMod expr

translateIntComparison :: ConstructorEnv -> Map String Int -> Maybe String -> Expr Ann -> FsExpr
translateIntComparison adtCtors localEnv currentMod expr =
  case IntComparison.fromExpr expr of
    Just comparison ->
      let
        operand value = "(unbox<int> (box (" <> printExprInline (translateExpr adtCtors localEnv currentMod value) <> ")))"
        emit operator = FsIdent ("(box (" <> operand comparison.left <> operator <> operand comparison.right <> "))")
      in case comparison.operator of
        OpLt -> emit " < "
        OpGt -> emit " > "
        _ -> translateExprFallback adtCtors localEnv currentMod expr
    Nothing -> translateIntArithmetic adtCtors localEnv currentMod expr

translateIntArithmetic :: ConstructorEnv -> Map String Int -> Maybe String -> Expr Ann -> FsExpr
translateIntArithmetic adtCtors localEnv currentMod expr =
  case IntArithmetic.fromExpr expr of
    Just arithmetic ->
      let
        operand value = "(unbox<int> (box (" <> printExprInline (translateExpr adtCtors localEnv currentMod value) <> ")))"
        emit operator = FsIdent ("(box (" <> operand arithmetic.left <> operator <> operand arithmetic.right <> "))")
      in case arithmetic.operator of
        OpAdd -> emit " + "
        OpSubtract -> emit " - "
        _ -> translateExprFallback adtCtors localEnv currentMod expr
    Nothing -> translateExprFallback adtCtors localEnv currentMod expr

translateExprFallback :: ConstructorEnv -> Map String Int -> Maybe String -> Expr Ann -> FsExpr
translateExprFallback adtCtors localEnv currentMod expr = case expr of
  ExprLit _ lit -> translateLit adtCtors localEnv currentMod lit
  ExprConstructor _ _ (Ident name) _ ->
    let fqName = case currentMod of
                   Just cmod -> String.replaceAll (Pattern ".") (Replacement "_") cmod <> "_" <> name
                   Nothing -> name
    in case Map.lookup (sanitizeName fqName) adtCtors.arities of
      Just arity -> generateConstructorCall adtCtors (sanitizeName fqName) arity []
      Nothing -> FsIdent ("(box " <> sanitizeName name <> ")")
  ExprVar _ qi -> 
    let nameStr = unwrap (unQualified qi) in
    let fqName = case qi of
          Qualified (Just modName) _ -> String.replaceAll (Pattern ".") (Replacement "_") (unwrap modName) <> "_" <> nameStr
          Qualified Nothing _ -> case currentMod of
                                   Just cmod -> String.replaceAll (Pattern ".") (Replacement "_") cmod <> "_" <> nameStr
                                   Nothing -> nameStr
    in case Map.lookup (sanitizeName fqName) adtCtors.arities of
      Just arity -> generateConstructorCall adtCtors (sanitizeName fqName) arity []
      Nothing ->
        case qi of
          Qualified (Just modName) (Ident name) -> 
            let mname = String.replaceAll (Pattern ".") (Replacement "_") (unwrap modName)
            in FsIdent ("(box " <> sanitizeName (mname <> "_" <> name) <> ")")
          Qualified Nothing (Ident name) -> FsIdent ("(box " <> sanitizeName name <> ")")
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
             if Array.length flat.args == arity then
                FsDirectApp (targetName <> "_tco") (map (translateExpr adtCtors localEnv currentMod) flat.args)
             else if Array.length flat.args > arity then
                let tcoArgs = Array.take arity flat.args
                    restArgs = Array.drop arity flat.args
                    baseCall = FsDirectApp (targetName <> "_tco") (map (translateExpr adtCtors localEnv currentMod) tcoArgs)
                in Array.foldl (\acc arg -> FsApp acc [translateExpr adtCtors localEnv currentMod arg]) baseCall restArgs
             else
                FsApp (translateExpr adtCtors localEnv currentMod flat.fn) (map (translateExpr adtCtors localEnv currentMod) flat.args)
          Nothing ->
            case Map.lookup (sanitizeName fqName) adtCtors.arities of
              Just arity -> generateConstructorCall adtCtors (sanitizeName fqName) arity (map (translateExpr adtCtors localEnv currentMod) flat.args)
              Nothing -> FsApp (translateExpr adtCtors localEnv currentMod flat.fn) (map (translateExpr adtCtors localEnv currentMod) flat.args)
      _ -> FsApp (translateExpr adtCtors localEnv currentMod flat.fn) (map (translateExpr adtCtors localEnv currentMod) flat.args)
  ExprCase _ exprs alts -> 
    let 
      fsExprs = map (translateExpr adtCtors localEnv currentMod) exprs
      matchExpr = case Array.length fsExprs of
        0 -> FsLitString "MissingExpr"
        1 -> FsIdent ("(unbox (" <> printExprInline (fromMaybe (FsLitString "MissingExpr") (Array.head fsExprs)) <> "))")
        _ -> FsIdent ("(" <> String.joinWith ", " (map (\e -> "(unbox (" <> printExprInline e <> "))") fsExprs) <> ")")
    in FsMatch matchExpr (Array.concatMap (translateCaseAlternative adtCtors localEnv currentMod) alts)
  ExprAbs _ (Ident arg) body -> FsIdent ("(box (fun (" <> sanitizeName arg <> ": obj) -> " <> printExprInline (translateExpr adtCtors localEnv currentMod body) <> "))")
  ExprAccessor _ obj prop -> FsIdent ("(Map.find \"" <> prop <> "\" (unbox<Map<string, obj>> (" <> printExprInline (translateExpr adtCtors localEnv currentMod obj) <> ")))")
  ExprTypeApp _ expr _ -> translateExpr adtCtors localEnv currentMod expr
  ExprLet _ binds body -> 
    let
      newEnv = Array.foldl (\acc b -> case b of
        Rec bindings -> Array.foldl (\acc2 (Binding _ (Ident n) e) -> 
          let ext = extractArgs e 
          in if Array.length ext.args > 0 then Map.insert (sanitizeName n) (Array.length ext.args) acc2 else acc2
        ) acc bindings
        _ -> acc
      ) localEnv binds

      bindStrs = Array.concatMap (\b -> case b of
        NonRec (Binding _ (Ident n) e) -> ["let " <> sanitizeName n <> " = " <> printExprInline (translateExpr adtCtors newEnv currentMod e) <> " in "]
        Rec bindings -> 
          let
            indent = "                                                                                                                                                                                                        "
            makeLocalRec (Binding _ (Ident n) e) idx =
              let
                ext = extractArgs e
                sName = sanitizeName n
              in if Array.length ext.args > 0 then
                let
                  argStrs = String.joinWith " " (map (\a -> "(" <> a <> ": obj)") ext.args)
                  bodyStr = printExprInline (translateExpr adtCtors newEnv currentMod ext.body)
                  curriedArgs = String.joinWith "" (map (\a -> "(fun (" <> a <> ": obj) -> ") ext.args)
                  closes = String.joinWith "" (map (const ")") ext.args)
                  keyword = if idx == 0 then "\n" <> indent <> "let rec " else "\n" <> indent <> "and "
                  wrapper = sName <> " = box (" <> curriedArgs <> sName <> "_tco " <> String.joinWith " " ext.args <> closes <> ")"
                in [ keyword <> sName <> "_tco " <> argStrs <> " : obj = (" <> bodyStr <> ") ", "\n" <> indent <> "and " <> wrapper <> " " ]
              else
                let
                  keyword = if idx == 0 then "\n" <> indent <> "let rec " else "\n" <> indent <> "and "
                in [ keyword <> sName <> " : obj = (" <> printExprInline (translateExpr adtCtors newEnv currentMod e) <> ") " ]
            recStrs = Array.concat (Array.mapWithIndex (\i b -> makeLocalRec b i) bindings)
          in ["\n" <> indent <> String.joinWith "" recStrs <> "\n" <> indent <> "in\n" <> indent]
      ) binds
      
      bodyTerm = case binds of
        [Rec _] -> printExprInline (translateExpr adtCtors newEnv currentMod body) <> "\n                                                                                                                                                                                                        )"
        _ -> printExprInline (translateExpr adtCtors newEnv currentMod body) <> ")"
    in FsIdent ("(" <> String.joinWith "" bindStrs <> bodyTerm)
  ExprUpdate _ obj props ->
    let
      mapAdd (Prop k v) prev = "(Map.add \"" <> k <> "\" (box (" <> printExprInline (translateExpr adtCtors localEnv currentMod v) <> ")) " <> prev <> ")"
    in FsIdent ("(box (" <> Array.foldr mapAdd ("(unbox<Map<string, obj>> " <> printExprInline (translateExpr adtCtors localEnv currentMod obj) <> ")") props <> "))")

printExprInline :: FsExpr -> String
printExprInline = case _ of
  FsLitString s -> "(box " <> escapeString s <> ")"
  FsLitBool b -> if b then "(box true)" else "(box false)"
  FsIdent id -> id
  FsApp fn args -> Array.foldl (\acc arg -> "(sharpurs_apply (box (" <> acc <> ")) (box (" <> printExprInline arg <> ")))") (printExprInline fn) args
  FsDirectApp name args -> if Array.length args > 0 then "(" <> name <> " " <> String.joinWith " " (map (\a -> "(" <> printExprInline a <> ")") args) <> ")" else name
  FsCtorApp name args -> if Array.length args > 0 then "(box (" <> name <> "(" <> String.joinWith ", " (map printExprInline args) <> ")))" else "(box " <> name <> ")"
  FsMatch e cases -> "(match (" <> printExprInline e <> ") with " <> String.joinWith " " (map (\(FsMatchCase pat g exp) -> "| " <> printPatternInline pat <> (case g of
      Just guardExpr -> " when (unbox " <> printExprInline guardExpr <> ")"
      Nothing -> "") <> " -> " <> printExprInline exp) cases) <> ")"

printNestedPatternInline :: FsPattern -> String
printNestedPatternInline = case _ of
  FsPatWildcard -> "_"
  FsPatIdent name -> name
  other -> "Unbox(" <> printPatternInline other <> ")"

printPatternInline :: FsPattern -> String
printPatternInline = case _ of
  FsPatCtor name args -> if Array.length args > 0 then name <> "(" <> String.joinWith ", " (map printNestedPatternInline args) <> ")" else name
  FsPatWildcard -> "_"
  FsPatIdent name -> name
  FsPatRaw s -> s

translateCaseAlternative :: ConstructorEnv -> Map String Int -> Maybe String -> CaseAlternative Ann -> Array FsMatchCase
translateCaseAlternative adtCtors localEnv currentMod (CaseAlternative binders guards) =
  let
    fsPatterns = map (translateBinder adtCtors localEnv currentMod) binders
    combinedPat = case Array.length fsPatterns of
      0 -> FsPatWildcard
      1 -> fromMaybe FsPatWildcard (Array.head fsPatterns)
      _ -> FsPatRaw ("(" <> String.joinWith ", " (map printPatternInline fsPatterns) <> ")")
  in
    case guards of
      Unconditional expr -> [FsMatchCase combinedPat Nothing (FsIdent ("(" <> printExprInline (translateExpr adtCtors localEnv currentMod expr) <> ")"))]
      Guarded array ->
        map (\(Guard guard expr) -> FsMatchCase combinedPat (Just (translateExpr adtCtors localEnv currentMod guard)) (FsIdent ("(" <> printExprInline (translateExpr adtCtors localEnv currentMod expr) <> ")"))) array

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
    in if Map.member fqName adtCtors.arities then
      FsPatCtor (sanitizeName fqName <> "usd_Ctor") (map (translateBinder adtCtors localEnv currentMod) binders)
    else
      case Array.index binders 0 of
        Just inner -> translateBinder adtCtors localEnv currentMod inner
        Nothing -> FsPatWildcard
  BinderNamed _ (Ident name) inner ->
    FsPatRaw ("(" <> printPatternInline (translateBinder adtCtors localEnv currentMod inner) <> " as " <> sanitizeName name <> ")")

translateLitBinder :: ConstructorEnv -> Map String Int -> Maybe String -> Literal (Binder Ann) -> FsPattern
translateLitBinder adtCtors localEnv currentMod = case _ of
    LitBoolean b -> FsPatRaw (if b then "LitBool true ()" else "LitBool false ()")
    LitInt i -> FsPatRaw ("LitInt " <> show i <> " ()")
    LitNumber n -> FsPatRaw ("LitNumber " <> show n <> " ()")
    LitString s -> FsPatRaw ("LitString " <> escapeString s <> " ()")
    LitChar c -> FsPatRaw ("LitChar '" <> escapeChar c <> "' ()")
    LitArray items -> 
      FsPatRaw ("[| " <> String.joinWith "; " (map (printPatternInline <<< translateBinder adtCtors localEnv currentMod) items) <> " |]")
    LitRecord props ->
      if Array.length props == 0 then
        FsPatWildcard
      else
        let
          propToPat (Prop key val) = 
            "HasProp \"" <> key <> "\" (" <> printPatternInline (translateBinder adtCtors localEnv currentMod val) <> ")"
        in FsPatRaw ("(" <> String.joinWith " & " (map propToPat props) <> ")")
