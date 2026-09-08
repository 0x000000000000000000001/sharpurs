module Sharpurs.IntKernel.CodeGen (printKernel) where

import Prelude

import Data.Array.NonEmpty as NonEmptyArray
import Data.Foldable (foldr)
import Data.String (joinWith)
import PureScript.Backend.Optimizer.Syntax (Level(..))
import Sharpurs.FsAst (FsDecl(..))
import Sharpurs.IntKernel (IntKernel, IntExpr(..), IntCondition(..), IntOperator(..))

-- The native function is local to its public binding, so no generated name can
-- collide with another source declaration. Every curry stage remains obj -> obj.
printKernel :: String -> IntKernel -> FsDecl
printKernel publicName kernel = FsRaw $
  "let " <> publicName <> " : obj =\n"
    <> "    let rec sharpurs_int_kernel " <> joinWith " " parameters <> " : int =\n"
    <> "        " <> printInt kernel.body <> "\n"
    <> "    " <> wrapper
  where
  names = map (localName <<< _.level) (NonEmptyArray.toArray kernel.args)
  parameters = map (\name -> "(" <> name <> ": int)") names
  call = "sharpurs_int_kernel " <> joinWith " " (map (\name -> "(unbox<int> " <> name <> ")") names)
  wrapper = foldr (\name body -> "box (fun (" <> name <> ": obj) -> " <> body <> ")")
    ("box (" <> call <> ")") names

localName :: Level -> String
localName (Level level) = "sharpurs_i_" <> show level

printInt :: IntExpr -> String
printInt = case _ of
  IntLiteral value -> "(" <> show value <> ")"
  IntLocal level -> localName level
  IntBinary IntAdd left right -> "(" <> printInt left <> " + " <> printInt right <> ")"
  IntBinary IntSubtract left right -> "(" <> printInt left <> " - " <> printInt right <> ")"
  IntBinary IntModulo left right -> "(sharpurs_int_mod " <> printInt left <> " " <> printInt right <> ")"
  IntIf condition yes no -> "(if " <> printCondition condition <> " then " <> printInt yes <> " else " <> printInt no <> ")"
  IntTailCall args -> "(sharpurs_int_kernel " <> joinWith " " (map printInt (NonEmptyArray.toArray args)) <> ")"

printCondition :: IntCondition -> String
printCondition (IntEqual left right) = "(" <> printInt left <> " = " <> printInt right <> ")"
