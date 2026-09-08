module Sharpurs.IntKernel.CodeGen (printKernel, printLocalKernel) where

import Prelude

import Data.Array.NonEmpty as NonEmptyArray
import Data.Foldable (foldr)
import Data.String (joinWith)
import PureScript.Backend.Optimizer.Syntax (Level(..))
import Sharpurs.FsAst (FsDecl(..))
import Sharpurs.IntKernel (IntKernel, LocalIntKernel, IntExpr(..), IntCondition(..), IntOperator(..))

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

printLocalKernel :: LocalIntKernel -> String
printLocalKernel kernel =
  "(let rec sharpurs_int_kernel " <> joinWith " " parameters <> " : int = "
    <> printInt kernel.body <> " in box (sharpurs_int_kernel "
    <> joinWith " " (map (printIntWith outerLocal) (NonEmptyArray.toArray kernel.entry)) <> "))"
  where
  parameters = map (\arg -> "(" <> localName arg.level <> ": int)") (NonEmptyArray.toArray kernel.args)
  outerLocal (Level level) = "(unbox<int> sharpurs_o_" <> show level <> ")"

localName :: Level -> String
localName (Level level) = "sharpurs_i_" <> show level

printInt :: IntExpr -> String
printInt = printIntWith localName

printIntWith :: (Level -> String) -> IntExpr -> String
printIntWith printLocal = case _ of
  IntLiteral value -> "(" <> show value <> ")"
  IntLocal level -> printLocal level
  IntBinary IntAdd left right -> "(" <> recur left <> " + " <> recur right <> ")"
  IntBinary IntSubtract left right -> "(" <> recur left <> " - " <> recur right <> ")"
  IntBinary IntModulo left right -> "(sharpurs_int_mod " <> recur left <> " " <> recur right <> ")"
  IntIf condition yes no -> "(if " <> printConditionWith printLocal condition <> " then " <> recur yes <> " else " <> recur no <> ")"
  IntTailCall args -> "(sharpurs_int_kernel " <> joinWith " " (map recur (NonEmptyArray.toArray args)) <> ")"
  where
  recur expr = printIntWith printLocal expr

printConditionWith :: (Level -> String) -> IntCondition -> String
printConditionWith printLocal (IntEqual left right) =
  "(" <> printIntWith printLocal left <> " = " <> printIntWith printLocal right <> ")"
