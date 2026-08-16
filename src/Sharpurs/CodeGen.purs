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


translateModule :: Map String Int -> Module Ann -> FsModule
translateModule adtCtors (Module m) =
  let
    modNameStr = unwrap m.name
    modPrefix = String.replaceAll (Pattern ".") (Replacement "_") modNameStr
    translateDataCtor c = FsDataCtor (modPrefix <> "_" <> sanitizeName c.name <> "usd_Ctor") (Array.length c.fields)
    translateDataDecl decl = FsDeclData (modPrefix <> "_" <> sanitizeName decl.name) (map translateDataCtor decl.constructors)
    nameStr = sanitizeName (String.replaceAll (Pattern ".") (Replacement "_") modNameStr)
    dataDecls = map translateDataDecl m.dataDecls
    decls = Array.concatMap (translateBind adtCtors (Just modPrefix)) m.decls
  in
    FsModule nameStr (dataDecls <> decls)

translateBind :: Map String Int -> Maybe String -> Bind Ann -> Array FsDecl
translateBind adtCtors currentMod = case _ of
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
             in keyword <> sName <> "_tco " <> argStrs <> " : obj = box (" <> bodyStr <> ")\n" <> "and " <> wrapper <> "\n"
           else
             let keyword = if isFirst then "let rec " else " and "
             in keyword <> sName <> " : obj = box (" <> printExprInline (translateExpr adtCtors recArities currentMod e) <> ")\n"
      
      recStr = Array.mapWithIndex (\i bd -> makeRec bd (i == 0)) bindings
    in [FsRaw (String.joinWith "" recStr)]
  where
    expandBind :: Map String Int -> Maybe String -> Binding Ann -> Array FsDecl
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

translateLit :: Map String Int -> Map String Int -> Maybe String -> Literal (Expr Ann) -> FsExpr
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
      in FsIdent (Array.foldr mapAdd "Map.empty" props)

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
      FsIdent (Array.foldr (\arg acc -> "(fun (" <> arg <> ": obj) -> " <> acc <> ")") (printExprInline body) argNames)

extractArgs :: Expr Ann -> { args :: Array String, body :: Expr Ann }
extractArgs (ExprAbs _ (Ident arg) body) = 
  let next = extractArgs body
  in { args: Array.cons (sanitizeName arg) next.args, body: next.body }
extractArgs e = { args: [], body: e }

translateExpr :: Map String Int -> Map String Int -> Maybe String -> Expr Ann -> FsExpr
translateExpr adtCtors localEnv currentMod expr = case expr of
  ExprLit _ lit -> translateLit adtCtors localEnv currentMod lit
  ExprConstructor _ _ (Ident name) _ ->
    let fqName = case currentMod of
                   Just cmod -> String.replaceAll (Pattern ".") (Replacement "_") cmod <> "_" <> name
                   Nothing -> name
    in case Map.lookup (sanitizeName fqName) adtCtors of
      Just arity -> generateConstructorLambda (sanitizeName fqName) arity []
      Nothing -> FsIdent (sanitizeName name)
  ExprVar _ qi -> 
    let nameStr = unwrap (unQualified qi) in
    let fqName = case qi of
          Qualified (Just modName) _ -> String.replaceAll (Pattern ".") (Replacement "_") (unwrap modName) <> "_" <> nameStr
          Qualified Nothing _ -> case currentMod of
                                   Just cmod -> String.replaceAll (Pattern ".") (Replacement "_") cmod <> "_" <> nameStr
                                   Nothing -> nameStr
    in case Map.lookup (sanitizeName fqName) adtCtors of
      Just arity -> generateConstructorLambda (sanitizeName fqName) arity []
      Nothing ->
        case qi of
          Qualified (Just modName) (Ident name) -> 
            let mname = String.replaceAll (Pattern ".") (Replacement "_") (unwrap modName)
            in FsIdent (sanitizeName (mname <> "_" <> name))
          Qualified Nothing (Ident name) -> FsIdent (sanitizeName name)
  ExprApp _ _ _ -> 
    let flat = flattenApp expr
    in case flat.fn of
      ExprConstructor _ _ (Ident name) _ -> 
        let fqName = case currentMod of
                       Just cmod -> String.replaceAll (Pattern ".") (Replacement "_") cmod <> "_" <> name
                       Nothing -> name
        in case Map.lookup (sanitizeName fqName) adtCtors of
           Just arity -> generateConstructorLambda (sanitizeName fqName) arity (map (translateExpr adtCtors localEnv currentMod) flat.args)
           Nothing -> FsCtorApp (sanitizeName fqName <> "usd_Ctor") (map (translateExpr adtCtors localEnv currentMod) flat.args)
      ExprVar _ qi ->
        let nameStr = unwrap (unQualified qi) in
        let fqName = case qi of
              Qualified (Just modName) _ -> String.replaceAll (Pattern ".") (Replacement "_") (unwrap modName) <> "_" <> nameStr
              Qualified Nothing _ -> case currentMod of
                                       Just cmod -> String.replaceAll (Pattern ".") (Replacement "_") cmod <> "_" <> nameStr
                                       Nothing -> nameStr
        in case Map.lookup (sanitizeName nameStr) localEnv of
          Just arity ->
             if Array.length flat.args == arity then
                FsDirectApp (sanitizeName nameStr <> "_tco") (map (translateExpr adtCtors localEnv currentMod) flat.args)
             else if Array.length flat.args > arity then
                let tcoArgs = Array.take arity flat.args
                    restArgs = Array.drop arity flat.args
                    baseCall = FsDirectApp (sanitizeName nameStr <> "_tco") (map (translateExpr adtCtors localEnv currentMod) tcoArgs)
                in Array.foldl (\acc arg -> FsApp acc [translateExpr adtCtors localEnv currentMod arg]) baseCall restArgs
             else
                FsApp (translateExpr adtCtors localEnv currentMod flat.fn) (map (translateExpr adtCtors localEnv currentMod) flat.args)
          Nothing ->
            case Map.lookup (sanitizeName fqName) adtCtors of
              Just arity -> generateConstructorLambda (sanitizeName fqName) arity (map (translateExpr adtCtors localEnv currentMod) flat.args)
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
  ExprAbs _ (Ident arg) body -> FsIdent ("(fun (" <> sanitizeName arg <> ": obj) -> " <> printExprInline (translateExpr adtCtors localEnv currentMod body) <> ")")
  ExprAccessor _ obj prop -> FsIdent ("(Map.find \"" <> prop <> "\" (unbox<Map<string, obj>> (" <> printExprInline (translateExpr adtCtors localEnv currentMod obj) <> ")))")
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
                in [ keyword <> sName <> "_tco " <> argStrs <> " : obj = box (" <> bodyStr <> ") ", "\n" <> indent <> "and " <> wrapper <> " " ]
              else
                let
                  keyword = if idx == 0 then "\n" <> indent <> "let rec " else "\n" <> indent <> "and "
                in [ keyword <> sName <> " : obj = box (" <> printExprInline (translateExpr adtCtors newEnv currentMod e) <> ") " ]
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
    in FsIdent (Array.foldr mapAdd ("(unbox<Map<string, obj>> " <> printExprInline (translateExpr adtCtors localEnv currentMod obj) <> ")") props)

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

translateCaseAlternative :: Map String Int -> Map String Int -> Maybe String -> CaseAlternative Ann -> Array FsMatchCase
translateCaseAlternative adtCtors localEnv currentMod (CaseAlternative binders guards) =
  let
    fsPatterns = map (translateBinder adtCtors localEnv currentMod) binders
    combinedPat = case Array.length fsPatterns of
      0 -> FsPatWildcard
      1 -> fromMaybe FsPatWildcard (Array.head fsPatterns)
      _ -> FsPatRaw ("(" <> String.joinWith ", " (map printPatternInline fsPatterns) <> ")")
  in
    case guards of
      Unconditional expr -> [FsMatchCase combinedPat Nothing (FsIdent ("(box (" <> printExprInline (translateExpr adtCtors localEnv currentMod expr) <> "))"))]
      Guarded array ->
        map (\(Guard guard expr) -> FsMatchCase combinedPat (Just (translateExpr adtCtors localEnv currentMod guard)) (FsIdent ("(box (" <> printExprInline (translateExpr adtCtors localEnv currentMod expr) <> "))"))) array

translateBinder :: Map String Int -> Map String Int -> Maybe String -> Binder Ann -> FsPattern
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
    in if Map.member fqName adtCtors then
      FsPatCtor (sanitizeName fqName <> "usd_Ctor") (map (translateBinder adtCtors localEnv currentMod) binders)
    else
      case Array.index binders 0 of
        Just inner -> translateBinder adtCtors localEnv currentMod inner
        Nothing -> FsPatWildcard
  BinderNamed _ (Ident name) inner ->
    FsPatRaw ("(" <> printPatternInline (translateBinder adtCtors localEnv currentMod inner) <> " as " <> sanitizeName name <> ")")

translateLitBinder :: Map String Int -> Map String Int -> Maybe String -> Literal (Binder Ann) -> FsPattern
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
