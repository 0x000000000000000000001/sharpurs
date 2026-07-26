module Sharpurs.Printer where

import Prelude

import Data.Array as Array
import Data.String as String
import Sharpurs.FsAst (FsDecl(..), FsExpr(..), FsModule(..), FsType(..), FsDUCase(..), FsMatchCase(..), FsPattern(..), FsDataCtor(..), sanitizeName)

printModule :: FsModule -> String
printModule (FsModule name decls) =
  let
    prelude = """
open System
open System.Collections.Generic

let objMap = Map.empty<string, obj>
let unbox<'a> (x: obj) : 'a = unbox x

let undefined = Unchecked.defaultof<obj>

let Data_Semiring_intAdd a b = (unbox<int> a) + (unbox<int> b)
let Data_Ring_intSub a b = (unbox<int> a) - (unbox<int> b)
let Data_Semiring_intMul a b = (unbox<int> a) * (unbox<int> b)
let Data_EuclideanRing_intDiv a b = (unbox<int> a) / (unbox<int> b)
let Data_EuclideanRing_intDegree a = Math.Abs(unbox<int> a)

let Data_Semiring_numAdd a b = (unbox<float> a) + (unbox<float> b)
let Data_Ring_numSub a b = (unbox<float> a) - (unbox<float> b)
let Data_Semiring_numMul a b = (unbox<float> a) * (unbox<float> b)
let Data_EuclideanRing_numDiv a b = (unbox<float> a) / (unbox<float> b)

let Data_Eq_eqIntImpl (a: obj) (b: obj) = unbox<int> a = unbox<int> b
let Data_Eq_eqNumberImpl (a: obj) (b: obj) = unbox<float> a = unbox<float> b
let Data_Eq_eqStringImpl (a: obj) (b: obj) = unbox<string> a = unbox<string> b
let Data_Eq_eqCharImpl (a: obj) (b: obj) = unbox<char> a = unbox<char> b
let Data_Eq_eqBooleanImpl (a: obj) (b: obj) = unbox<bool> a = unbox<bool> b

let Data_Ord_ordIntImpl lt eq gt x y = let x' = unbox<int> x in let y' = unbox<int> y in if x' < y' then lt else if x' = y' then eq else gt
let Data_Ord_ordNumberImpl lt eq gt x y = let x' = unbox<float> x in let y' = unbox<float> y in if x' < y' then lt else if x' = y' then eq else gt
let Data_Ord_ordStringImpl lt eq gt x y = let x' = unbox<string> x in let y' = unbox<string> y in if x' < y' then lt else if x' = y' then eq else gt
let Data_Ord_ordCharImpl lt eq gt x y = let x' = unbox<char> x in let y' = unbox<char> y in if x' < y' then lt else if x' = y' then eq else gt
let Data_Ord_ordBooleanImpl lt eq gt x y = let x' = unbox<bool> x in let y' = unbox<bool> y in if x' < y' then lt else if x' = y' then eq else gt

let Record_Unsafe_unsafeGet k map = Map.find (unbox<string> k) (unbox<Map<string, obj>> map)
let Record_Unsafe_unsafeSet k v map = Map.add (unbox<string> k) v (unbox<Map<string, obj>> map)
let Prim_undefined = undefined
let Data_Ord_ordArrayImpl = undefined
let Data_Unit_unit = undefined
let Data_Functor_arrayMap = undefined
let Control_Apply_arrayApply = undefined
let Control_Bind_arrayBind = undefined
let Data_Show_showIntImpl (x: obj) : obj = box (string (unbox<int> x))
let Data_Show_showNumberImpl (x: obj) : obj = box (string (unbox<float> x))
let Data_Show_showStringImpl (x: obj) : obj = box ("\"" + unbox<string> x + "\"")
let Effect_Console_log (s: obj) = box (fun _ -> printfn "%s" (unbox<string> s); undefined)
let Effect_Console_time _ = undefined
let Effect_Console_timeLog _ = undefined
let Effect_Console_timeEnd _ = undefined
let Effect_Console_info _ = undefined
let Effect_Console_groupEnd _ = undefined
let Effect_Console_groupCollapsed _ = undefined
let Effect_Console_group _ = undefined
let Effect_Console_error _ = undefined
let Effect_Console_debug _ = undefined
let Effect_Console_clear _ = undefined
let Effect_Console_warn _ = undefined

let Effect_Uncurried_mkEffectFn1 _ = undefined
let Effect_Uncurried_mkEffectFn2 _ = undefined
let Effect_Uncurried_mkEffectFn3 _ = undefined
let Effect_Uncurried_mkEffectFn4 _ = undefined
let Effect_Uncurried_mkEffectFn5 _ = undefined
let Effect_Uncurried_mkEffectFn6 _ = undefined
let Effect_Uncurried_mkEffectFn7 _ = undefined
let Effect_Uncurried_mkEffectFn8 _ = undefined
let Effect_Uncurried_mkEffectFn9 _ = undefined
let Effect_Uncurried_mkEffectFn10 _ = undefined
let Effect_Uncurried_runEffectFn1 _ _ = undefined
let Effect_Uncurried_runEffectFn2 _ _ _ = undefined
let Effect_Uncurried_runEffectFn3 _ _ _ _ = undefined
let Effect_Uncurried_runEffectFn4 _ _ _ _ _ = undefined
let Effect_Uncurried_runEffectFn5 _ _ _ _ _ _ = undefined
let Effect_Uncurried_runEffectFn6 _ _ _ _ _ _ _ = undefined
let Effect_Uncurried_runEffectFn7 _ _ _ _ _ _ _ _ = undefined
let Effect_Uncurried_runEffectFn8 _ _ _ _ _ _ _ _ _ = undefined
let Effect_Uncurried_runEffectFn9 _ _ _ _ _ _ _ _ _ _ = undefined
let Effect_Uncurried_runEffectFn10 _ _ _ _ _ _ _ _ _ _ _ = undefined""" <> """
let sharpurs_apply (func: obj) (arg: obj) : obj =
    if isNull func then failwith "sharpurs_apply: func is null!"
    let method = func.GetType().GetMethods() |> Array.find (fun m -> m.Name = "Invoke" && m.GetParameters().Length = 1)
    method.Invoke(func, [| arg |])
let intMod a b = unbox<int> a % unbox<int> b
let semiringInt = 0
let Data_Symbol_unsafeCoerce x = x
let Data_HeytingAlgebra_boolConj (a: obj) (b: obj) = box ((unbox<bool> a) && (unbox<bool> b))
let Data_HeytingAlgebra_boolDisj (a: obj) (b: obj) = box ((unbox<bool> a) || (unbox<bool> b))
let Data_HeytingAlgebra_boolNot (a: obj) = box (not (unbox<bool> a))
let Data_Eq_eqArrayImpl (a: obj) (b: obj) = undefined
let Data_Bounded_topChar = '\uffff'
let Data_Bounded_bottomChar = '\u0000'
let Data_EuclideanRing_intMod a b = unbox<int> a % unbox<int> b
let Data_Reflectable_unsafeCoerce x = x
let Data_Show_Generic_intercalate = undefined
let Effect_bindE = undefined
let Effect_pureE = undefined
let Data_Semigroup_concatString a b = (unbox<string> a) + (unbox<string> b)
let Data_Semigroup_concatArray a b = undefined
let Data_Show_showCharImpl c = string (unbox<char> c)
let Data_Show_showArrayImpl a = undefined
let Data_Bounded_topNumber = System.Double.MaxValue
let Data_Bounded_bottomNumber = System.Double.MinValue
let Data_Bounded_topInt = System.Int32.MaxValue
let Data_Bounded_bottomInt = System.Int32.MinValue
"""
    header = "module " <> name <> "\n\n"
    body = String.joinWith "\n\n" (map printDecl decls)
  in
    header <> prelude <> body <> "\n\nlet _ = (unbox<obj -> obj> Main_main) undefined\n"

printDecl :: FsDecl -> String
printDecl = case _ of
  FsLet name args body ->
    "let " <> name <> " " <> String.joinWith " " args <> " = " <> printExpr body
  FsLetRec bindings ->
    let strs = Array.mapWithIndex (\i b -> (if i == 0 then "let rec " else "and ") <> b.name <> " " <> String.joinWith " " b.args <> " = " <> printExpr b.expr) bindings
    in String.joinWith "\n" strs
  FsDeclData name ctors ->
    if Array.length ctors == 0 then
      "type " <> name <> " = | Dummy_" <> name
    else
      "type " <> name <> " =\n" <>
      String.joinWith "\n" (map (\(FsDataCtor ctorName count) -> "  | " <> sanitizeName ctorName <> (if count > 0 then " of " <> String.joinWith " * " (Array.replicate count "obj") else "")) ctors)
  FsDU name cases ->
    "type " <> name <> " =\n" <>
    String.joinWith "\n" (map (\(FsDUCase ctor fields) -> "  | " <> ctor <> (if Array.length fields > 0 then " of " <> String.joinWith " * " (map printType fields) else "")) cases)
  FsRaw str -> str

printDUCase :: FsDUCase -> String
printDUCase (FsDUCase name types) =
  let
    typesStr = if Array.length types > 0 then " of " <> String.joinWith " * " (map printType types) else ""
  in
    "    | " <> name <> typesStr

printType :: FsType -> String
printType = case _ of
  FsTString -> "string"
  FsTBool -> "bool"
  FsTInt -> "int"
  FsTCustom name -> name

printExpr :: FsExpr -> String
printExpr = case _ of
  FsLitString s -> "(box \"" <> s <> "\")"
  FsLitBool b -> if b then "(box true)" else "(box false)"
  FsIdent id -> id
  FsApp fn args ->
    Array.foldl (\acc arg -> "(sharpurs_apply (box (" <> acc <> ")) (box (" <> printExpr arg <> ")))") (printExpr fn) args
  FsCtorApp name args ->
    if Array.length args > 0 then
      "(box (" <> name <> "(" <> String.joinWith ", " (map printExpr args) <> ")))"
    else "(box " <> name <> ")"
  FsMatch expr cases ->
    "match (" <> printExpr expr <> ") with " <> String.joinWith " " (map printMatchCase cases)

printMatchCase :: FsMatchCase -> String
printMatchCase (FsMatchCase pat expr) =
  "| " <> printPattern pat <> " -> " <> printExpr expr

printPattern :: FsPattern -> String
printPattern = case _ of
  FsPatCtor name args ->
    if Array.length args > 0 then
      name <> "(" <> String.joinWith ", " (map printPattern args) <> ")"
    else
      name
  FsPatWildcard -> "_"
  FsPatIdent name -> name
  FsPatRaw s -> s
