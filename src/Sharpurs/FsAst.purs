module Sharpurs.FsAst where

import Prelude

import Data.Maybe (Maybe)
import Data.String.Pattern (Pattern(..), Replacement(..))
import Data.String.Common as String
import Data.Array as Array

sanitizeName :: String -> String
sanitizeName s = 
  let
    reserved = ["module", "type", "let", "in", "match", "with", "fun", "if", "then", "else", "true", "false", "pure", "bind", "return", "global", "as", "val", "const", "void", "when", "class", "struct", "interface", "object", "default", "to", "do", "done", "end", "open", "new", "null", "base", "base_", "finally", "try", "catch", "throw", "upcast", "downcast", "mod", "or", "and", "not", "member", "override", "static", "mutable", "rec", "exception"]
    s1 = String.replaceAll (Pattern "$") (Replacement "usd_") s
    s2 = String.replaceAll (Pattern "'") (Replacement "_prime") s1
    s3 = String.replaceAll (Pattern "+") (Replacement "_plus_") s2
    s4 = String.replaceAll (Pattern "-") (Replacement "_minus_") s3
    s5 = String.replaceAll (Pattern "*") (Replacement "_times_") s4
    s6 = String.replaceAll (Pattern "/") (Replacement "_div_") s5
    s7 = String.replaceAll (Pattern "=") (Replacement "_eq_") s6
    s8 = String.replaceAll (Pattern "<") (Replacement "_lt_") s7
    s9 = String.replaceAll (Pattern ">") (Replacement "_gt_") s8
    s10 = String.replaceAll (Pattern "!") (Replacement "_bang_") s9
    s11 = String.replaceAll (Pattern "&") (Replacement "_amp_") s10
    s12 = String.replaceAll (Pattern "|") (Replacement "_bar_") s11
    s13 = String.replaceAll (Pattern "%") (Replacement "_percent_") s12
    s14 = String.replaceAll (Pattern "^") (Replacement "_caret_") s13
    s15 = String.replaceAll (Pattern "~") (Replacement "_tilde_") s14
    s16 = String.replaceAll (Pattern "?") (Replacement "_qmark_") s15
    s17 = String.replaceAll (Pattern "@") (Replacement "_at_") s16
    sanitized = s17
  in if Array.elem sanitized reserved then sanitized <> "_" else sanitized

escapeString :: String -> String
escapeString s =
  let
    s1 = String.replaceAll (Pattern "\\") (Replacement "\\\\") s
    s2 = String.replaceAll (Pattern "\"") (Replacement "\\\"") s1
    s3 = String.replaceAll (Pattern "\n") (Replacement "\\n") s2
    s4 = String.replaceAll (Pattern "\r") (Replacement "\\r") s3
    s5 = String.replaceAll (Pattern "\t") (Replacement "\\t") s4
  in s5

data FsModule = FsModule String (Array FsDecl)

data FsDataCtor = FsDataCtor String Int

data FsDecl
  = FsLet String (Array String) FsExpr
  | FsLetRec (Array { name :: String, args :: Array String, expr :: FsExpr })
  | FsDeclData String (Array FsDataCtor)
  | FsDU String (Array FsDUCase)
  | FsRaw String

data FsDUCase = FsDUCase String (Array FsType)

data FsType
  = FsTString
  | FsTBool
  | FsTInt
  | FsTCustom String

data FsExpr
  = FsLitString String
  | FsLitBool Boolean
  | FsIdent String
  | FsApp FsExpr (Array FsExpr)
  | FsCtorApp String (Array FsExpr)
  | FsMatch FsExpr (Array FsMatchCase)

data FsMatchCase = FsMatchCase FsPattern FsExpr

data FsPattern
  = FsPatCtor String (Array FsPattern)
  | FsPatIdent String
  | FsPatRaw String
  | FsPatWildcard
