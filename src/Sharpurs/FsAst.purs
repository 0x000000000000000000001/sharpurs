module Sharpurs.FsAst where

import Prelude

import Data.Maybe (Maybe)

data FsModule = FsModule String (Array FsDecl)

data FsDecl
  = FsLet String (Array String) FsExpr
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
  | FsMatch FsExpr (Array FsMatchCase)

data FsMatchCase = FsMatchCase FsPattern FsExpr

data FsPattern
  = FsPatCtor String (Array String)
  | FsPatWildcard
