module Sharpurs.CodeGen where

import Prelude

import Data.Array as Array
import Data.Maybe (Maybe(..), fromMaybe)
import Data.Newtype (unwrap)
import Data.String as String
import Data.String.CodeUnits as CU
import Data.String.Pattern (Pattern(..), Replacement(..))
import PureScript.Backend.Optimizer.CoreFn (Module(..), Bind(..), Binding(..), Expr(..), Ident(..), Literal(..), CaseAlternative(..), CaseGuard(..), Guard(..), Binder(..), Ann, DataDecl, DataConstructor, unQualified, ExprType(..), Prop(..), Qualified(..))
import Sharpurs.FsAst (FsDecl(..), FsExpr(..), FsModule(..), FsType(..), FsDUCase(..), FsMatchCase(..), FsPattern(..), FsDataCtor(..), sanitizeName)
import Data.Set as Set
import Data.Set (Set)
import Data.Tuple (Tuple(..))
import Data.Maybe (Maybe(..), fromMaybe)


translateModule :: Set String -> Module Ann -> FsModule
translateModule adtCtors (Module m) =
  let
    modNameStr = String.replaceAll (Pattern ".") (Replacement "_") (unwrap m.name)
    translateDataCtor ctor = FsDataCtor (sanitizeName ctor.constructorName) (Array.length ctor.fieldTypes)
    translateDataDecl decl = FsDeclData (sanitizeName decl.typeName) (map translateDataCtor decl.constructors)
    nameStr = sanitizeName modNameStr
    dataDecls = map translateDataDecl m.dataDecls
    decls = Array.concatMap (translateBind adtCtors (Just modNameStr)) m.decls
  in
    FsModule nameStr (dataDecls <> decls)

translateBind :: Set String -> Maybe String -> Bind Ann -> Array FsDecl
translateBind adtCtors currentMod = case _ of
  NonRec b -> expandBind adtCtors currentMod b
  Rec binds -> [FsLetRec (map (\(Binding _ (Ident n) e) -> 
                  let prefix = case currentMod of
                        Just m -> m <> "_"
                        Nothing -> ""
                  in { name: sanitizeName (prefix <> n), args: [], expr: translateExpr adtCtors e }) binds)]
  where
    expandBind :: Set String -> Maybe String -> Binding Ann -> Array FsDecl
    expandBind adtCtors currentMod (Binding _ (Ident name) val) =
      let prefix = case currentMod of
            Just m -> m <> "_"
            Nothing -> ""
      in [FsLet (sanitizeName (prefix <> name)) [] (translateExpr adtCtors val)]

translateDataDecl :: DataDecl -> FsDecl
translateDataDecl decl =
  FsDU (sanitizeName decl.typeName) (map translateConstructor decl.constructors)

translateType :: ExprType -> FsType
translateType = case _ of
  String -> FsTString
  Boolean -> FsTBool
  Int -> FsTInt
  _ -> FsTCustom "obj"

translateConstructor :: DataConstructor -> FsDUCase
translateConstructor ctor =
  FsDUCase (sanitizeName ctor.constructorName) (map translateType ctor.fieldTypes)



flattenApp :: Expr Ann -> { fn :: Expr Ann, args :: Array (Expr Ann) }
flattenApp (ExprApp _ f x) =
  let flat = flattenApp f
  in { fn: flat.fn, args: Array.snoc flat.args x }
flattenApp expr = { fn: expr, args: [] }

translateLit :: Set String -> Literal (Expr Ann) -> FsExpr
translateLit adtCtors lit = case lit of
    LitInt i -> FsIdent ("(box " <> show i <> ")")
    LitNumber n -> FsIdent ("(box " <> show n <> ")")
    LitString s -> FsLitString s
    LitChar c -> FsIdent ("(box '" <> CU.singleton c <> "')")
    LitBoolean b -> FsLitBool b
    LitArray arr -> FsIdent ("(box [|" <> String.joinWith "; " (map (printExprInline <<< translateExpr adtCtors) arr) <> "|])")
    LitRecord props ->
      let
        mapAdd (Prop key val) acc = "(Map.add \"" <> key <> "\" (box (" <> printExprInline (translateExpr adtCtors val) <> ")) " <> acc <> ")"
      in FsIdent (Array.foldr mapAdd "Map.empty" props)

translateExpr :: Set String -> Expr Ann -> FsExpr
translateExpr adtCtors expr = case expr of
  ExprLit _ lit -> translateLit adtCtors lit
  ExprVar _ qi -> 
    case qi of
      Qualified (Just modName) (Ident name) -> 
        let mname = String.replaceAll (Pattern ".") (Replacement "_") (unwrap modName)
        in FsIdent (sanitizeName (mname <> "_" <> name))
      Qualified Nothing (Ident name) -> FsIdent (sanitizeName name)
  ExprApp _ _ _ -> 
    let flat = flattenApp expr
    in case flat.fn of
      ExprConstructor _ _ (Ident name) _ -> FsCtorApp (sanitizeName name) (map (translateExpr adtCtors) flat.args)
      ExprVar _ qi ->
        let name = unwrap (unQualified qi)
        in if Set.member name adtCtors then
             FsCtorApp (sanitizeName name) (map (translateExpr adtCtors) flat.args)
           else
             FsApp (translateExpr adtCtors flat.fn) (map (translateExpr adtCtors) flat.args)
      _ -> FsApp (translateExpr adtCtors flat.fn) (map (translateExpr adtCtors) flat.args)
  ExprCase _ exprs alts -> 
    let 
      fsExprs = map (translateExpr adtCtors) exprs
      matchExpr = case Array.length fsExprs of
        0 -> FsLitString "MissingExpr"
        1 -> FsIdent ("(unbox (" <> printExprInline (fromMaybe (FsLitString "MissingExpr") (Array.head fsExprs)) <> "))")
        _ -> FsIdent ("(" <> String.joinWith ", " (map (\e -> "(unbox (" <> printExprInline e <> "))") fsExprs) <> ")")
    in FsMatch matchExpr (map (translateCaseAlternative adtCtors) alts)
  ExprConstructor _ _ (Ident name) _ -> FsIdent (sanitizeName name)
  ExprAbs _ (Ident arg) body -> FsIdent ("(fun (" <> sanitizeName arg <> ": obj) -> " <> printExprInline (translateExpr adtCtors body) <> ")")
  ExprAccessor _ obj prop -> FsIdent ("(Map.find \"" <> prop <> "\" (unbox<Map<string, obj>> (" <> printExprInline (translateExpr adtCtors obj) <> ")))")
  ExprLet _ binds body -> 
    let
      bindStrs = Array.concatMap (\b -> case b of
        NonRec (Binding _ (Ident n) e) -> ["let " <> sanitizeName n <> " = " <> printExprInline (translateExpr adtCtors e) <> " in "]
        Rec bindings -> 
          let strs = Array.mapWithIndex (\i (Binding _ (Ident n) e) -> (if i == 0 then "let rec " else "and ") <> sanitizeName n <> " = " <> printExprInline (translateExpr adtCtors e) <> " ") bindings
          in [String.joinWith "" strs <> "in "]
      ) binds
    in FsIdent ("(" <> String.joinWith "" bindStrs <> printExprInline (translateExpr adtCtors body) <> ")")
  ExprUpdate _ obj props ->
    let
      mapAdd (Prop k v) prev = "(Map.add \"" <> k <> "\" (box (" <> printExprInline (translateExpr adtCtors v) <> ")) " <> prev <> ")"
    in FsIdent (Array.foldr mapAdd ("(unbox<Map<string, obj>> " <> printExprInline (translateExpr adtCtors obj) <> ")") props)
  _ -> FsLitString "NotImplemented"

printExprInline :: FsExpr -> String
printExprInline = case _ of
  FsLitString s -> "(box \"" <> s <> "\")"
  FsLitBool b -> if b then "(box true)" else "(box false)"
  FsIdent id -> id
  FsApp fn args -> Array.foldl (\acc arg -> "(sharpurs_apply (box (" <> acc <> ")) (box (" <> printExprInline arg <> ")))") (printExprInline fn) args
  FsCtorApp name args -> if Array.length args > 0 then "(box (" <> name <> "(" <> String.joinWith ", " (map printExprInline args) <> ")))" else "(box " <> name <> ")"
  FsMatch e cases -> "match (" <> printExprInline e <> ") with " <> String.joinWith " " (map (\(FsMatchCase pat exp) -> "| " <> printPatternInline pat <> " -> " <> printExprInline exp) cases)

printPatternInline :: FsPattern -> String
printPatternInline = case _ of
  FsPatCtor name args -> if Array.length args > 0 then name <> "(" <> String.joinWith ", " (map printPatternInline args) <> ")" else name
  FsPatWildcard -> "_"
  FsPatIdent name -> name
  FsPatRaw s -> s

translateCaseAlternative :: Set String -> CaseAlternative Ann -> FsMatchCase
translateCaseAlternative adtCtors (CaseAlternative binders guards) =
  let
    fsPatterns = map (translateBinder adtCtors) binders
    combinedPat = case Array.length fsPatterns of
      0 -> FsPatWildcard
      1 -> fromMaybe FsPatWildcard (Array.head fsPatterns)
      _ -> FsPatRaw ("(" <> String.joinWith ", " (map printPatternInline fsPatterns) <> ")")
  in
    case guards of
      Unconditional expr -> FsMatchCase combinedPat (translateExpr adtCtors expr)
      Guarded array -> 
        case Array.head array of
          Just (Guard guard expr) -> FsMatchCase combinedPat (translateExpr adtCtors expr)
          Nothing -> FsMatchCase combinedPat (FsIdent "undefined")

translateBinder :: Set String -> Binder Ann -> FsPattern
translateBinder adtCtors = case _ of
  BinderNull _ -> FsPatWildcard
  BinderVar _ (Ident name) -> FsPatIdent (sanitizeName name)
  BinderLit _ lit -> translateLitBinder adtCtors lit
  BinderConstructor _ _ qi binders ->
    let name = unwrap (unQualified qi) in
    if Set.member name adtCtors then
      FsPatCtor (sanitizeName name) (map (translateBinder adtCtors) binders)
    else
      case Array.index binders 0 of
        Just inner -> translateBinder adtCtors inner
        Nothing -> FsPatWildcard
  _ -> FsPatWildcard

translateLitBinder :: Set String -> Literal (Binder Ann) -> FsPattern
translateLitBinder adtCtors = case _ of
  LitInt i -> FsPatRaw (show i)
  LitNumber n -> FsPatRaw (show n)
  LitString s -> FsPatRaw ("\"" <> s <> "\"")
  LitChar c -> FsPatRaw ("'" <> CU.singleton c <> "'")
  LitBoolean b -> FsPatRaw (if b then "true" else "false")
  LitArray items -> 
    FsPatRaw ("[| " <> String.joinWith "; " (map (printPatternInline <<< translateBinder adtCtors) items) <> " |]")
  LitRecord props ->
    FsPatWildcard -- F# doesn't support pattern matching Map directly
