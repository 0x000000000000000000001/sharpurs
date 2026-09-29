module Sharpurs.FsAst where

import Prelude

import Data.Char (toCharCode)
import Data.Maybe (Maybe, fromMaybe)
import Data.String.Pattern (Pattern(..), Replacement(..))
import Data.String.Common as String
import Data.Array as Array
import Data.String.CodeUnits as CU

sanitizeName :: String -> String
sanitizeName s = 
  let
    reserved = ["abstract", "and", "as", "assert", "base", "base_", "begin", "class", "default", "delegate", "do", "done", "downcast", "downto", "elif", "else", "end", "exception", "extern", "false", "finally", "for", "fun", "function", "global", "if", "in", "inherit", "inline", "interface", "internal", "lazy", "let", "match", "member", "module", "mutable", "namespace", "new", "not", "null", "of", "open", "or", "override", "private", "public", "rec", "return", "sig", "static", "struct", "then", "to", "true", "try", "type", "upcast", "use", "val", "void", "when", "while", "with", "yield", "pure", "bind", "const", "object", "mod", "box", "unbox", "sharpurs_apply", "failwith", "obj", "string", "float", "int", "int64", "bool", "char", "unit", "System"]
    s0 = if s == "$__unused" then "usd__unused" else s
    s1 = String.replaceAll (Pattern "$") (Replacement "usd_") s0
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
    s18 = String.replaceAll (Pattern "\\") (Replacement "_bslash_") s17
    -- Symbol instances and monomorphized names can embed arbitrary text
    -- (e.g. IsSymbol "2:30"), which must not leak into F# identifiers.
    s19 = String.replaceAll (Pattern ":") (Replacement "_colon_") s18
    s20 = String.replaceAll (Pattern "#") (Replacement "_hash_") s19
    s21 = String.replaceAll (Pattern "\"") (Replacement "_quote_") s20
    s22 = String.replaceAll (Pattern " ") (Replacement "_space_") s21
    s23 = String.replaceAll (Pattern ",") (Replacement "_comma_") s22
    sanitizeChar c =
      if (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_'
        then CU.singleton c
        else "_u" <> hex4 (toCharCode c) <> "_"
    s24 = String.joinWith "" (map sanitizeChar (CU.toCharArray s23))
    sanitized0 = s24
    firstChar = CU.take 1 sanitized0
    sanitized = if firstChar >= "0" && firstChar <= "9" then "X_" <> sanitized0 else sanitized0
  in if Array.elem sanitized reserved then sanitized <> "_var" else sanitized

foreign import escapeString :: String -> String

hex4 :: Int -> String
hex4 n =
  let
    digits = "0123456789abcdef"
    nibble k = fromMaybe '0' (CU.charAt (k `mod` 16) digits)
  in CU.singleton (nibble (n `div` 4096)) <> CU.singleton (nibble (n `div` 256)) <> CU.singleton (nibble (n `div` 16)) <> CU.singleton (nibble n)

escapeChar :: Char -> String
escapeChar '\n' = "\\n"
escapeChar '\r' = "\\r"
escapeChar '\t' = "\\t"
escapeChar '\\' = "\\\\"
escapeChar '\'' = "\\'"
escapeChar c =
  let
    code = toCharCode c
    digits = "0123456789abcdef"
    nibble n = fromMaybe '0' (CU.charAt (n `mod` 16) digits)
  in if code < 0x20 || code == 0x7f || (code >= 0xd800 && code <= 0xdfff) || code == 0x85 || code == 0x2028 || code == 0x2029
       then "\\u" <> CU.singleton (nibble (code `div` 4096)) <> CU.singleton (nibble (code `div` 256)) <> CU.singleton (nibble (code `div` 16)) <> CU.singleton (nibble code)
       else CU.singleton c

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
  | FsDirectApp String (Array FsExpr)
  | FsMatch FsExpr (Array FsMatchCase)

data FsMatchCase = FsMatchCase FsPattern (Maybe FsExpr) FsExpr

data FsPattern
  = FsPatCtor String (Array FsPattern)
  | FsPatIdent String
  | FsPatRaw String
  | FsPatWildcard
