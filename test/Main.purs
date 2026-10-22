module Test.Main where

import Prelude hiding (one)

import Data.Array (length)
import Data.Array.NonEmpty (NonEmptyArray)
import Data.Array.NonEmpty as NEA
import Data.Foldable (traverse_)
import Data.Maybe (Maybe(..), isJust, isNothing)
import Data.Map as Map
import Data.Set as Set
import Data.String (joinWith)
import Data.String as String
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
import Sharpurs.Native.Emit as NativeEmit
import Sharpurs.Native.IR as NativeIR
import Sharpurs.Native.Interprocedural as NativeInterprocedural
import Sharpurs.Native.Layout as NativeLayout
import Sharpurs.Native.Lower as NativeLower
import Sharpurs.Native.Module as NativeModule
import Sharpurs.Native.Mutable as NativeMutable
import Sharpurs.Native.MutableModule as NativeMutableModule
import Sharpurs.Native.Ownership as NativeOwnership
import Sharpurs.Native.Pattern as NativePattern
import Sharpurs.Native.Reuse as NativeReuse
import Sharpurs.Native.Specialized as NativeSpecialized
import Sharpurs.Native.Types as NativeTypes
import Sharpurs.Native.Usage as NativeUsage
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
      , Tuple "lambda in the function body" (fun2 (abs (one (param "extra" 2)) (lit 1)))
      , Tuple "local let outside the subset" (fun2 (node (B.Let Nothing (B.Level 2) (lit 1) (lit 2))))
      ]
  traverse_ (\(Tuple label expr) -> assert ("reject: " <> label) (isNothing (fromBinding self expr))) rejected

  let
    nonTailCalls =
      [ Tuple "self call in arithmetic operand"
          (fun2 (binary (B.OpIntNum B.OpAdd) recursive (lit 1)))
      , Tuple "self call in condition operand"
          (fun2 (branch (binary (B.OpIntOrd B.OpEq) recursive (lit 0)) (lit 1) (lit 2)))
      , Tuple "self call in another call's argument"
          (fun2 (call self (two recursive (lit 2))))
      , Tuple "branch in an arithmetic operand"
          (fun2 (binary (B.OpIntNum B.OpAdd) (branch equalZero recursive (lit 1)) (lit 2)))
      ]
  traverse_ (\(Tuple label expr) -> assert ("accept: " <> label) (isJust (fromBinding self expr))) nonTailCalls
  assert "non-tail arithmetic self call uses IntCall"
    (case fromBinding self (fun2 (binary (B.OpIntNum B.OpAdd) recursive (lit 1))) of
      Just { body: IntBinary IntAdd (IntCall _) (IntLiteral 1) } -> true
      _ -> false)
  log ("IntKernel: " <> show (11 + length rejected + length nonTailCalls) <> " assertions passed")
  testRouting
  testNativeLayout
  testNativeLower
  testNativeEmit
  testNativeModule
  testNativeOwnership
  testNativeReuse
  testNativeMutable
  testNativeSpecialized
  testNativeMutableModule
  testNativeInterprocedural

testRouting :: Effect Unit
testRouting = case fromBinding self (tco self) of
  Nothing -> throw "IntKernel routing fixture was rejected"
  Just kernel -> do
    let
      ident = C.Ident "deepTailRec"
      ann = C.Ann { span: C.emptySpan, meta: Nothing, type: Nothing, sourceUsage: Nothing }
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

nativeAnn :: C.Ann
nativeAnn = C.Ann { span: C.emptySpan, meta: Nothing, type: Nothing, sourceUsage: Nothing }

nativeIntAnn :: C.Ann
nativeIntAnn = C.Ann { span: C.emptySpan, meta: Nothing, type: Just C.Int, sourceUsage: Nothing }

nativeIdentity :: C.Bind C.Ann
nativeIdentity = C.NonRec
  (C.Binding nativeAnn (C.Ident "identity")
    (C.ExprAbs
      (C.Ann { span: C.emptySpan, meta: Nothing, type: Just (C.Func [ C.Int ] C.Int), sourceUsage: Nothing })
      (C.Ident "x")
      (C.ExprVar nativeIntAnn (C.Qualified Nothing (C.Ident "x")))))

nativeFixture :: C.Module C.Ann
nativeFixture = C.Module
  { name: C.ModuleName "NativeFixture"
  , path: "NativeFixture.purs"
  , span: C.emptySpan
  , imports: []
  , exports: []
  , reExports: []
  , dataDecls:
      [ { name: "Tree"
        , vars: []
        , constructors:
            [ { name: "Leaf", fields: [] }
            , { name: "Node", fields: [ C.Int, C.ADT "NativeFixture.Tree" [ "NativeFixture", "Tree" ] [] ] }
            ]
        }
      ]
  , classDecls: []
  , decls: [ nativeIdentity ]
  , foreign: Map.empty
  , comments: []
  }

testNativeLayout :: Effect Unit
testNativeLayout = case NativeLayout.fromModule nativeFixture of
  Nothing -> throw "Native.Layout: fixture rejected"
  Just layout ->
    case NativeLayout.lookupConstructor layout
      (C.Qualified (Just (C.ModuleName "NativeFixture")) (C.Ident "Node")) of
      Nothing -> throw "Native.Layout: Node constructor missing"
      Just ctor -> do
        assert "native layout keeps native fields"
          (ctor.fields == [ NativeTypes.NativeInt
                          , NativeTypes.NativeAdt "NativeFixture_Tree" []
                          ])
        let
          ann = C.Ann { span: C.emptySpan, meta: Nothing, type: Nothing, sourceUsage: Nothing }
          binder = C.BinderConstructor ann
            (C.Qualified Nothing (C.ProperName "Tree"))
            (C.Qualified (Just (C.ModuleName "NativeFixture")) (C.Ident "Node"))
            [ C.BinderVar ann (C.Ident "l")
            , C.BinderVar ann (C.Ident "r")
            ]
        case NativePattern.fromBinder layout binder of
          Nothing -> throw "Native.Pattern: Node binder rejected"
          Just pattern -> do
            assert "native pattern keeps constructor and fields"
              (NativePattern.variables pattern == [ "l", "r" ])
            log "Native.Pattern: 1 assertion passed"
        log "Native.Layout: 2 assertions passed"

testNativeOwnership :: Effect Unit
testNativeOwnership = do
  let
    x = NativeIR.NativeVar "x" NativeTypes.NativeInt
    y = NativeIR.NativeVar "y" NativeTypes.NativeInt
    parameter = { name: "x", type: NativeTypes.NativeInt }
    primitive = NativeIR.NativePrimitive NativeIR.PrimitiveAdd [ x, y ]
    shared = NativeIR.NativeConstructor "NativeFixture_Node" [ x, x ]
    nativeCall = NativeIR.NativeApply
      (NativeIR.NativeVar "f" (NativeTypes.NativeFunction [ NativeTypes.NativeInt ] NativeTypes.NativeInt))
      [ x ]
    primitiveUsage = NativeUsage.usageExpr primitive
    sharedUsage = NativeUsage.usageExpr shared
    callUsage = NativeUsage.usageExpr nativeCall
    closure = NativeIR.NativeLambda [ { name: "z", type: NativeTypes.NativeInt } ]
      (NativeIR.NativePrimitive NativeIR.PrimitiveAdd [ x, NativeIR.NativeVar "z" NativeTypes.NativeInt ])
    closureUsage = NativeUsage.usageExpr closure
    uniqueSummary = NativeOwnership.summarize [ parameter ]
      (NativeIR.NativePrimitive NativeIR.PrimitiveAdd [ x, NativeIR.NativeIntLit 1 ])
    escapedSummary = NativeOwnership.summarize [ parameter ] shared
  assert "usage keeps two independent reads"
    (NativeUsage.isUnique primitiveUsage "x" && NativeUsage.isUnique primitiveUsage "y")
  assert "usage detects repeated constructor fields"
    (not (NativeUsage.isUnique sharedUsage "x"))
  assert "usage detects function-argument escape"
    (not (NativeUsage.isUnique callUsage "x"))
  assert "usage marks closure capture as escaping"
    (not (NativeUsage.isUnique closureUsage "x"))
  assert "ownership accepts a unique primitive parameter"
    (NativeOwnership.parameterUnique uniqueSummary parameter)
  assert "ownership rejects a parameter stored in a constructor"
    (not (NativeOwnership.parameterUnique escapedSummary parameter))
  assert "ownership marks primitive results fresh"
    (NativeOwnership.resultFresh uniqueSummary)
  log "Native.Ownership: 7 assertions passed"

testNativeReuse :: Effect Unit
testNativeReuse = do
  let
    tree = NativeTypes.NativeAdt "NativeFixture_Tree" []
    scrutinee = NativeIR.NativeVar "t" tree
    field = NativeIR.NativeVar "x" NativeTypes.NativeInt
    pattern = NativePattern.PatternConstructor "NativeFixture_Tusd_Ctor"
      [ NativePattern.PatternVariable "x" ]
    rebuild = NativeIR.NativeConstructor "NativeFixture_Tusd_Ctor" [ field ]
    oneCase = { patterns: [ pattern ], bindings: Map.empty, body: rebuild }
    candidateMatch = NativeIR.NativeMatch [ scrutinee ] [ oneCase ] Nothing
    freshLet = NativeIR.NativeLet "t" tree
      (NativeIR.NativeConstructor "NativeFixture_Tusd_Ctor" [])
      candidateMatch
    freshUsage = NativeUsage.usageExpr freshLet
    freshCandidates = NativeReuse.findCandidates freshUsage freshLet
    rewritten = NativeReuse.rewrite freshUsage freshLet
    parameterCandidates = NativeReuse.findCandidates (NativeUsage.usageExpr candidateMatch) candidateMatch
    parameterRewrite = NativeReuse.rewrite (NativeUsage.usageExpr candidateMatch) candidateMatch
    parameterAllowed = NativeReuse.rewriteWith (Set.singleton "t")
      (NativeUsage.usageExpr candidateMatch) candidateMatch
    aliasedLet = NativeIR.NativeLet "old" tree scrutinee candidateMatch
    aliasedCandidates = NativeReuse.findCandidates (NativeUsage.usageExpr aliasedLet) aliasedLet
    sharedMatch = NativeIR.NativeMatch [ scrutinee, scrutinee ] [ oneCase ] Nothing
    otherConstructor = NativeIR.NativeConstructor "NativeFixture_Other" [ field ]
    wrongMatch = NativeIR.NativeMatch [ scrutinee ]
      [ { patterns: [ pattern ], bindings: Map.empty, body: otherConstructor } ] Nothing
  assert "reuse finds a fresh local reconstructed constructor" (length freshCandidates == 1)
  assert "reuse rewrites only a fresh local target"
    (case rewritten of
      NativeIR.NativeLet _ _ _ (NativeIR.NativeReuse _ _ _) -> true
      _ -> false)
  assert "reuse rejects a function parameter target"
    (length parameterCandidates == 0 && case parameterRewrite of
      NativeIR.NativeMatch _ _ _ -> true
      _ -> false)
  assert "reuse accepts a parameter proved by interprocedural analysis"
    (case parameterAllowed of
      NativeIR.NativeReuse _ _ _ -> true
      _ -> false)
  assert "reuse rejects an aliased old reference"
    (length aliasedCandidates == 0)
  assert "reuse rejects repeated scrutinees"
    (not (NativeReuse.hasCandidates (NativeUsage.usageExpr sharedMatch) sharedMatch))
  assert "reuse rejects a different constructor"
    (not (NativeReuse.hasCandidates (NativeUsage.usageExpr wrongMatch) wrongMatch))
  log "Native.Reuse: 7 assertions passed"

testNativeMutable :: Effect Unit
testNativeMutable = case NativeLayout.fromModule nativeFixture of
  Nothing -> throw "Native.Mutable: fixture rejected"
  Just layout -> do
    let
      tree = NativeTypes.NativeAdt "NativeFixture_Tree" []
      target = NativeIR.NativeVar "t" tree
      reuse = NativeIR.NativeReuse "NativeFixture_Tusd_Ctor" target
        [ NativeIR.NativeIntLit 1, NativeIR.NativeIntLit 2 ]
      pattern = NativePattern.PatternConstructor "NativeFixture_Tusd_Ctor"
        [ NativePattern.PatternVariable "x" ]
    assert "mutable layout uses mutable class fields"
      (String.contains (String.Pattern "member val Field0: int") (NativeMutable.renderTypeDeclarations layout))
    case NativeMutable.renderExpr reuse of
      Nothing -> throw "Native.Mutable: reuse rejected"
      Just code -> do
        assert "mutable reuse emits a reuse call"
          (String.contains (String.Pattern "_reuse") code)
        assert "mutable pattern uses an active pattern"
          (String.contains (String.Pattern "_match") (NativeMutable.renderPattern pattern))
        log "Native.Mutable: 3 assertions passed"

testNativeInterprocedural :: Effect Unit
testNativeInterprocedural = do
  let
    tree = NativeTypes.NativeAdt "NativeFixture_Tree" []
    tParam = { name: "t", type: tree }
    sharedParam = { name: "shared", type: tree }
    x = NativeIR.NativeVar "x" NativeTypes.NativeInt
    pattern = NativePattern.PatternConstructor "NativeFixture_Tusd_Ctor"
      [ NativePattern.PatternVariable "x" ]
    consumeBody = NativeIR.NativeMatch
      [ NativeIR.NativeVar "t" tree ]
      [ { patterns: [ pattern ]
        , bindings: Map.empty
        , body: NativeIR.NativeConstructor "NativeFixture_Tusd_Ctor" [ x ]
        } ]
      Nothing
    makeBody = NativeIR.NativeConstructor "NativeFixture_Tusd_Ctor" []
    callConsume argument = NativeIR.NativeApply
      (NativeIR.NativeVar "consume" (NativeTypes.NativeFunction [ tree ] tree))
      [ argument ]
    mainBody = callConsume
      (NativeIR.NativeApply (NativeIR.NativeVar "make" (NativeTypes.NativeFunction [] tree)) [])
    badBody = callConsume (NativeIR.NativeVar "shared" tree)
    make = { name: "make", parameters: [], body: makeBody }
    consume = { name: "consume", parameters: [ tParam ], body: consumeBody }
    entry = { name: "entry", parameters: [], body: mainBody }
    bad = { name: "bad", parameters: [ sharedParam ], body: badBody }
    analysis = NativeInterprocedural.analyze Set.empty [ make, consume, entry ]
    exportedAnalysis = NativeInterprocedural.analyze (Set.singleton "consume") [ make, consume, entry ]
    badAnalysis = NativeInterprocedural.analyze Set.empty [ make, consume, entry, bad ]
  assert "interprocedural marks a private parameter mutable"
    (NativeInterprocedural.parameterMutable analysis "consume" tParam)
  assert "interprocedural rejects an exported parameter"
    (not (NativeInterprocedural.parameterMutable exportedAnalysis "consume" tParam))
  assert "interprocedural rejects a shared caller argument"
    (not (NativeInterprocedural.parameterMutable badAnalysis "consume" tParam))
  assert "interprocedural preserves fresh results"
    (case Map.lookup "make" analysis of
      Just summary -> summary.resultFresh
      Nothing -> false)
  log "Native.Interprocedural: 4 assertions passed"

testNativeSpecialized :: Effect Unit
testNativeSpecialized = case NativeLayout.fromModule nativeFixture of
  Nothing -> throw "Native.Specialized: fixture rejected"
  Just layout -> do
    let
      classes = NativeSpecialized.constructorClasses layout
      tree = NativeTypes.NativeAdt "NativeFixture_Tree" []
      target = NativeIR.NativeVar "t" tree
      reuse = NativeIR.NativeReuse "NativeFixture_Tusd_Ctor" target
        [ NativeIR.NativeIntLit 1, NativeIR.NativeIntLit 2 ]
      pattern = NativePattern.PatternConstructor "NativeFixture_Tusd_Ctor"
        [ NativePattern.PatternVariable "x" ]
    assert "specialized layout emits one class per constructor"
      (String.contains (String.Pattern "inherit NativeFixture_Tree") (NativeSpecialized.renderTypeDeclarations layout))
    case NativeSpecialized.renderExpr classes reuse of
      Nothing -> throw "Native.Specialized: reuse rejected"
      Just code -> do
        assert "specialized reuse uses private reuse function"
          (String.contains (String.Pattern "_reuse") code)
        log "Native.Specialized: 2 assertions passed"

testNativeMutableModule :: Effect Unit
testNativeMutableModule = case NativeMutableModule.compile nativeFixture of
  Nothing -> throw "Native.MutableModule: fixture rejected"
  Just _ ->
    assert "mutable module registers constructors"
      (length (NativeMutableModule.constructorNames nativeFixture) > 0)
    >>= \_ -> log "Native.MutableModule: 1 assertion passed"

testNativeModule :: Effect Unit
testNativeModule = case NativeModule.compile nativeFixture of
  Nothing -> throw "Native.Module: fixture rejected"
  Just _ -> log "Native.Module: 1 assertion passed"

testNativeEmit :: Effect Unit
testNativeEmit = case NativeLayout.fromModule nativeFixture of
  Nothing -> throw "Native.Emit: fixture rejected"
  Just layout -> do
    assert "native emit keeps type declaration"
      (String.contains (String.Pattern "type NativeFixture_Tree") (NativeEmit.renderTypeDeclarations layout))
    assert "native emit renders integer expression"
      (NativeEmit.renderExpr (NativeIR.NativeIntLit 7) == Just "7")
    log "Native.Emit: 2 assertions passed"

testNativeLower :: Effect Unit
testNativeLower = case NativeLayout.fromModule nativeFixture of
  Nothing -> throw "Native.Lower: fixture rejected"
  Just layout ->
    let
      ann = C.Ann { span: C.emptySpan, meta: Nothing, type: Nothing, sourceUsage: Nothing }
      env = { layout, locals: Map.empty, globals: [] }
    in case NativeLower.lowerExpr env (C.ExprLit ann (C.LitInt 42)) of
      Just (NativeIR.NativeIntLit 42) -> log "Native.Lower: 1 assertion passed"
      _ -> throw "Native.Lower: integer literal was not lowered"

assert :: String -> Boolean -> Effect Unit
assert label passed = unless passed (throw ("IntKernel: " <> label))
