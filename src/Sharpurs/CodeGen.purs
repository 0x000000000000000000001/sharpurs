module Sharpurs.CodeGen where

import Prelude

import Data.Array as Array
import Data.Maybe (Maybe(..), fromMaybe)
import Data.Newtype (unwrap)
import Data.String as String
import Data.String.Pattern (Pattern(..), Replacement(..))
import PureScript.Backend.Optimizer.CoreFn (Module(..), Bind(..), Binding(..), Expr(..), Ident(..), Literal(..), CaseAlternative(..), CaseGuard(..), Binder(..), Ann, DataDecl, DataConstructor, unQualified, ExprType(..))
import Sharpurs.FsAst (FsDecl(..), FsExpr(..), FsModule(..), FsType(..), FsDUCase(..), FsMatchCase(..), FsPattern(..))

translateModule :: Module Ann -> FsModule
translateModule (Module m) =
  let
    nameStr = String.replaceAll (Pattern ".") (Replacement "_") (unwrap m.name)
    dataDecls = map translateDataDecl m.dataDecls
    decls = Array.concatMap translateBind m.decls
  in
    FsModule nameStr (dataDecls <> decls)

translateDataDecl :: DataDecl -> FsDecl
translateDataDecl decl =
  FsDU decl.typeName (map translateConstructor decl.constructors)

translateType :: ExprType -> FsType
translateType = case _ of
  String -> FsTString
  Boolean -> FsTBool
  Int -> FsTInt
  _ -> FsTCustom "obj"

translateConstructor :: DataConstructor -> FsDUCase
translateConstructor ctor =
  FsDUCase ctor.constructorName (map translateType ctor.fieldTypes)

translateBind :: Bind Ann -> Array FsDecl
translateBind = case _ of
  NonRec (Binding _ (Ident name) expr) ->
    [FsLet name [] (translateExpr expr)]
  Rec bindings ->
    map (\(Binding _ (Ident name) expr) -> FsLet name [] (translateExpr expr)) bindings

flattenApp :: Expr Ann -> { fn :: Expr Ann, args :: Array (Expr Ann) }
flattenApp (ExprApp _ f x) =
  let flat = flattenApp f
  in { fn: flat.fn, args: Array.snoc flat.args x }
flattenApp expr = { fn: expr, args: [] }

translateExpr :: Expr Ann -> FsExpr
translateExpr expr = case expr of
  ExprLit _ (LitString s) -> FsLitString s
  ExprLit _ (LitBoolean b) -> FsLitBool b
  ExprLit _ (LitInt i) -> FsIdent (show i)
  ExprLit _ (LitNumber n) -> FsIdent (show n)
  ExprLit _ _ -> FsLitString "UnsupportedLit"
  ExprVar _ qi -> 
    let name = unwrap (unQualified qi) 
    in if name == "log" then FsIdent "printfn \"%A\"" 
       else if name == "add" then FsIdent "(+)"
       else if name == "eq" then FsIdent "(=)"
       else if name == "conj" then FsIdent "(&&)"
       else if name == "lessThan" then FsIdent "(<)"
       else if name == "bind" then FsIdent "bind'"
       else if name == "pure" then FsIdent "pure'"
       else if name == "discard" then FsIdent "bind'"
       else FsIdent name
  ExprApp _ _ _ -> 
    let flat = flattenApp expr
    in FsApp (translateExpr flat.fn) (map translateExpr flat.args)
  ExprCase _ exprs alts -> FsMatch (fromMaybe (FsLitString "MissingExpr") (map translateExpr (Array.head exprs))) (map translateAlt alts)
  ExprConstructor _ _ (Ident name) _ -> FsIdent name
  ExprAbs _ (Ident arg) body -> FsIdent ("(fun " <> arg <> " -> " <> printExprInline (translateExpr body) <> ")")
  ExprAccessor _ (ExprVar _ record) prop -> FsIdent (unwrap (unQualified record) <> "." <> prop)
  ExprLet _ binds body -> 
    -- For simplicity, just inline them in a quick hack format or wrap in an IIFE.
    -- Better yet, since F# doesn't support let bindings inline easily without line breaks unless we use (let x=y in z),
    -- Actually F# DOES support `let x = y in z` for inline lets! But `in` is deprecated in some forms or fine?
    -- No, F# supports `let x = y in z` perfectly for expressions!
    let 
      bindStrs = map (\(NonRec (Binding _ (Ident n) e)) -> "let " <> n <> " = " <> printExprInline (translateExpr e) <> " in ") binds
    in FsIdent ("(" <> String.joinWith "" bindStrs <> printExprInline (translateExpr body) <> ")")
  _ -> FsLitString "NotImplemented"

printExprInline :: FsExpr -> String
printExprInline = case _ of
  FsLitString s -> "\"" <> s <> "\""
  FsLitBool b -> if b then "true" else "false"
  FsIdent id -> id
  FsApp fn args -> printExprInline fn <> " " <> String.joinWith " " (map (\a -> "(" <> printExprInline a <> ")") args)
  FsMatch e cases -> "match " <> printExprInline e <> " with " <> String.joinWith " " (map (\(FsMatchCase pat exp) -> "| " <> printPatternInline pat <> " -> " <> printExprInline exp) cases)

printPatternInline :: FsPattern -> String
printPatternInline = case _ of
  FsPatCtor name args -> if Array.length args > 0 then name <> "(" <> String.joinWith ", " args <> ")" else name
  FsPatWildcard -> "_"

translateAlt :: CaseAlternative Ann -> FsMatchCase
translateAlt (CaseAlternative binders guard) =
  FsMatchCase (fromMaybe FsPatWildcard (map translateBinder (Array.head binders))) (translateGuard guard)

translateGuard :: CaseGuard Ann -> FsExpr
translateGuard = case _ of
  Unconditional e -> translateExpr e
  Guarded _ -> FsLitString "GuardedNotImplemented"

translateBinder :: Binder Ann -> FsPattern
translateBinder = case _ of
  BinderConstructor _ _ qi binders -> FsPatCtor (unwrap (unQualified qi)) (map binderToIdent binders)
  _ -> FsPatWildcard

binderToIdent :: Binder Ann -> String
binderToIdent = case _ of
  BinderVar _ (Ident name) -> name
  _ -> "_"
