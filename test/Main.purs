module Test.Main where

import Prelude hiding (one)

import Data.Array (length)
import Data.Array.NonEmpty (NonEmptyArray)
import Data.Array.NonEmpty as NEA
import Data.Foldable (traverse_)
import Data.Maybe (Maybe(..), isNothing)
import Data.Map as Map
import Data.String (joinWith)
import Data.Tuple (Tuple(..))
import Effect (Effect)
import Effect.Console (log)
import Effect.Exception (throw)
import PureScript.Backend.Optimizer.CoreFn as C
import PureScript.Backend.Optimizer.Semantics (NeutralExpr(..))
import PureScript.Backend.Optimizer.Syntax as B
import Sharpurs.IntKernel (IntCondition(..), IntExpr(..), IntOperator(..), fromBinding)
import Sharpurs.CodeGen (translateBind, translateBindWithKernels)
import Sharpurs.IntKernel.CodeGen (printKernel)
import Sharpurs.Printer (printDecl)

main :: Effect Unit
main = do
  let
    expected name = Just
      { name
      , args: two
          { name: Just (C.Ident "v"), level: B.Level 0 }
          { name: Just (C.Ident "v1"), level: B.Level 1 }
      , body: IntIf (IntEqual (IntLocal (B.Level 0)) (IntLiteral 0))
          (IntLocal (B.Level 1))
          (IntTailCall (two
            (IntBinary IntSubtract (IntLocal (B.Level 0)) (IntLiteral 1))
            (IntBinary IntAdd (IntLocal (B.Level 1))
              (IntBinary IntModulo (IntLocal (B.Level 0)) (IntLiteral 3)))))
      }
    renamed = qualified "Another.Module" "count"
  assert "TCO dump preserves parameters, primitives and terminal recursion"
    (fromBinding self (tco self) == expected self)
  assert "recognition does not depend on a benchmark name"
    (fromBinding renamed (tco renamed) == expected renamed)

  let
    sameNames = typed sig2 $ abs (two (param "x" 0) (param "x" 1)) (local "other" 1)
  assert "local identity uses levels, not names"
    (map _.body (fromBinding self sameNames) == Just (IntLocal (B.Level 1)))
  assert "anonymous parameter remains usable by its level"
    (map _.body (fromBinding self (typed sig1 $ abs (one (Tuple Nothing (B.Level 0))) (local "ignored" 0)))
      == Just (IntLocal (B.Level 0)))
  assert "Boolean annotation on integer equality is accepted"
    (map _.body (fromBinding self (fun2 (branch (typed C.Boolean equalZero) (lit 1) (lit 2))))
      == Just (IntIf (IntEqual (IntLocal (B.Level 0)) (IntLiteral 0)) (IntLiteral 1) (IntLiteral 2)))
  assert "multiple branches retain their order"
    (map _.body (fromBinding self (fun2 (node (B.Branch
      (two (B.Pair equalZero (lit 1)) (B.Pair equalZero (lit 2))) (lit 3)))))
      == Just (IntIf condition (IntLiteral 1) (IntIf condition (IntLiteral 2) (IntLiteral 3))))
  traverse_ (\value -> assert ("integer literal is preserved: " <> show value)
    (map _.body (fromBinding self (fun2 (lit value))) == Just (IntLiteral value)))
    [ 0, -1, -2147483648, 2147483647 ]

  let
    unsupported = node (B.EffectPure (lit 1))
    recursive = call self (two (lit 1) (lit 2))
    rejected =
      [ Tuple "missing global signature" (abs (two (param "v" 0) (param "v1" 1)) (lit 1))
      , Tuple "zero-argument signature" (typed (C.Func [] C.Int) (lit 1))
      , Tuple "non-Int parameter" (typed (C.Func [ C.Number, C.Int ] C.Int) (fun2 (lit 1)))
      , Tuple "non-Int result" (typed (C.Func [ C.Int, C.Int ] C.Boolean) (fun2 (lit 1)))
      , Tuple "uninstantiated quantifier" (typed (C.ForAll [ "a" ] sig2) (fun2 (lit 1)))
      , Tuple "constrained signature" (typed (C.ConstrainedType [] sig2) (fun2 (lit 1)))
      , Tuple "contradictory global annotations" (typed sig1 (fun2 (lit 1)))
      , Tuple "contradictory intermediate signature"
          (typed sig2 $ abs (one (param "v" 0)) $ typed (C.Func [ C.Number ] C.Int) $ abs (one (param "v1" 1)) (lit 1))
      , Tuple "contradictory nested value annotation" (fun2 (int (typed C.Number (lit 1))))
      , Tuple "missing parameter" (typed sig2 $ abs (one (param "v" 0)) (lit 1))
      , Tuple "too many parameters" (typed sig1 $ abs (two (param "v" 0) (param "v1" 1)) (lit 1))
      , Tuple "duplicate parameter level" (typed sig2 $ abs (two (param "v" 0) (param "v1" 0)) (lit 1))
      , Tuple "unbound local level" (fun2 (local "v" 2))
      , Tuple "partially applied self reference" (fun2 (call self (one (lit 1))))
      , Tuple "overapplied self reference" (fun2 (call self (NEA.cons (lit 0) (two (lit 1) (lit 2)))))
      , Tuple "homonymous foreign module" (fun2 (call (qualified "Other" "deepTailRec") (two (lit 1) (lit 2))))
      , Tuple "contradictory self-reference annotation"
          (fun2 (node (B.App (typed sig1 (node (B.Var self))) (two (lit 1) (lit 2)))))
      , Tuple "residual TypeApp" (fun2 (node (B.TypeApp (lit 1) C.Int)))
      , Tuple "effect wrapped in an Int annotation" (fun2 (int unsupported))
      , Tuple "unsupported fallback branch" (fun2 (branch equalZero (lit 1) unsupported))
      , Tuple "unsupported later branch" (fun2 (node (B.Branch
          (two (B.Pair equalZero (lit 1)) (B.Pair equalZero unsupported)) (lit 2))))
      , Tuple "unsupported condition" (fun2 (branch (lit 0) (lit 1) (lit 2)))
      , Tuple "contradictory condition annotation" (fun2 (branch (int equalZero) (lit 1) (lit 2)))
      , Tuple "Boolean arithmetic operand" (fun2 (binary (B.OpIntNum B.OpAdd) equalZero (lit 1)))
      , Tuple "Number arithmetic" (fun2 (binary (B.OpNumberNum B.OpAdd) (lit 1) (lit 2)))
      , Tuple "unsupported Int arithmetic" (fun2 (binary (B.OpIntNum B.OpMultiply) (lit 1) (lit 2)))
      , Tuple "unsupported Int comparison" (fun2 (branch (binary (B.OpIntOrd B.OpLt) (lit 0) (lit 1)) (lit 1) (lit 2)))
      , Tuple "self call in arithmetic operand" (fun2 (binary (B.OpIntNum B.OpAdd) recursive (lit 1)))
      , Tuple "self call in condition operand" (fun2 (branch (binary (B.OpIntOrd B.OpEq) recursive (lit 0)) (lit 1) (lit 2)))
      , Tuple "self call in another call's argument" (fun2 (call self (two recursive (lit 2))))
      , Tuple "branch cannot restore terminal context in an operand"
          (fun2 (binary (B.OpIntNum B.OpAdd) (branch equalZero recursive (lit 1)) (lit 2)))
      , Tuple "lambda in the function body" (fun2 (abs (one (param "extra" 2)) (lit 1)))
      , Tuple "local let outside the subset" (fun2 (node (B.Let Nothing (B.Level 2) (lit 1) (lit 2))))
      ]
  traverse_ (\(Tuple label expr) -> assert ("reject: " <> label) (isNothing (fromBinding self expr))) rejected
  log ("IntKernel: " <> show (10 + length rejected) <> " assertions passed")
  testRouting

testRouting :: Effect Unit
testRouting = case fromBinding self (tco self) of
  Nothing -> throw "IntKernel routing fixture was rejected"
  Just kernel -> do
    let
      ident = C.Ident "deepTailRec"
      ann = C.Ann { span: C.emptySpan, meta: Nothing, type: Nothing }
      binding = C.Binding ann ident (C.ExprLit ann (C.LitInt 1))
      other = C.Binding ann (C.Ident "other") (C.ExprLit ann (C.LitInt 2))
      kernels = Map.singleton ident kernel
      render = joinWith "\n" <<< map printDecl
      route candidates = render <<< translateBindWithKernels Map.empty candidates "Test_TCO"
      fallback = render <<< translateBind Map.empty (Just "Test_TCO")
      native = printDecl (printKernel "Test_TCO_deepTailRec" kernel)
    assert "nonrecursive singleton uses the optimized kernel"
      (route kernels (C.NonRec binding) == native)
    assert "recursive singleton uses the optimized kernel"
      (route kernels (C.Rec [ binding ]) == native)
    assert "missing optimized binding retains nonrecursive fallback"
      (route Map.empty (C.NonRec binding) == fallback (C.NonRec binding))
    assert "missing optimized binding retains recursive fallback"
      (route Map.empty (C.Rec [ binding ]) == fallback (C.Rec [ binding ]))
    assert "other source binding retains fallback"
      (route kernels (C.NonRec other) == fallback (C.NonRec other))
    assert "mutual group remains intact even with a recognized member"
      (route kernels (C.Rec [ binding, other ]) == fallback (C.Rec [ binding, other ]))
    log "IntKernel routing: 6 assertions passed"

-- Fixture follows the observed PBO shape: repeated Typed wrappers, two curried
-- Abs nodes, and synthetic equality/modulo/self-reference nodes without Typed.
tco :: C.Qualified C.Ident -> NeutralExpr
tco name = typed sig2 $ fun2 $ int $ branch
  equalZero
  (int (int (local "v1" 1)))
  (int (int (call name (two
    (int (int (binary (B.OpIntNum B.OpSubtract) (int (local "v" 0)) (int (lit 1)))))
    (int (int (binary (B.OpIntNum B.OpAdd) (local "v1" 1)
      (binary (B.OpIntNum B.OpMod) (int (local "v" 0)) (int (lit 3))))))))))

self :: C.Qualified C.Ident
self = qualified "Test.TCO" "deepTailRec"

qualified :: String -> String -> C.Qualified C.Ident
qualified moduleName name = C.Qualified (Just (C.ModuleName moduleName)) (C.Ident name)

sig1 :: C.ExprType
sig1 = C.Func [ C.Int ] C.Int

sig2 :: C.ExprType
sig2 = C.Func [ C.Int, C.Int ] C.Int

fun2 :: NeutralExpr -> NeutralExpr
fun2 body = typed sig2 $ abs (one (param "v" 0)) $ typed sig1 $ abs (one (param "v1" 1)) body

param :: String -> Int -> Tuple (Maybe C.Ident) B.Level
param name level = Tuple (Just (C.Ident name)) (B.Level level)

node :: B.BackendSyntax NeutralExpr -> NeutralExpr
node = NeutralExpr

typed :: C.ExprType -> NeutralExpr -> NeutralExpr
typed ty expr = node (B.Typed ty expr)

int :: NeutralExpr -> NeutralExpr
int = typed C.Int

abs :: NonEmptyArray (Tuple (Maybe C.Ident) B.Level) -> NeutralExpr -> NeutralExpr
abs args body = node (B.Abs args body)

lit :: Int -> NeutralExpr
lit value = node (B.Lit (C.LitInt value))

local :: String -> Int -> NeutralExpr
local name level = node (B.Local (Just (C.Ident name)) (B.Level level))

binary :: B.BackendOperator2 -> NeutralExpr -> NeutralExpr -> NeutralExpr
binary op left right = node (B.PrimOp (B.Op2 op left right))

call :: C.Qualified C.Ident -> NonEmptyArray NeutralExpr -> NeutralExpr
call name args = node (B.App (node (B.Var name)) args)

branch :: NeutralExpr -> NeutralExpr -> NeutralExpr -> NeutralExpr
branch conditionExpr yes no = node (B.Branch (one (B.Pair conditionExpr yes)) no)

equalZero :: NeutralExpr
equalZero = binary (B.OpIntOrd B.OpEq) (int (local "v" 0)) (lit 0)

condition :: IntCondition
condition = IntEqual (IntLocal (B.Level 0)) (IntLiteral 0)

one :: forall a. a -> NonEmptyArray a
one = NEA.singleton

two :: forall a. a -> a -> NonEmptyArray a
two a b = NEA.cons a (one b)

assert :: String -> Boolean -> Effect Unit
assert label passed = unless passed (throw ("IntKernel: " <> label))
