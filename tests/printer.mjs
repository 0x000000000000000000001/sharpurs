// Run after building the compiler. A constructed call keeps its own argument
// and result adapters in every placement, including mixed nested conventions.
import assert from "node:assert/strict";
import { mkdtemp, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { spawnSync } from "node:child_process";
import * as F from "../output/Sharpurs.FsAst/index.js";
import * as Boxed from "../output/Sharpurs.CodeGen.Boxed/index.js";
import { printExpr, printExprInline, printDecl } from "../output/Sharpurs.Printer/index.js";
import { normalizeRecIndent } from "../output/Sharpurs.Printer.Layout/index.js";
import { prelude } from "../output/Sharpurs.Runtime/index.js";
import { Just, Nothing } from "../output/Data.Maybe/index.js";

const int = value => new F.FsLitInt(value);
const nativeInt = value => new F.FsRawExpr(String(value));
const ident = name => new F.FsIdent(name);
const call = (name, ...args) => F.directCall(name)(args);
const boxedCall = (name, ...args) => F.boxedNativeCall(name)(args);
const branch = (pattern, body, guard = Nothing.value) => new F.FsMatchCase(pattern, guard, body);
const matches = (value, pattern) => printExpr(new F.FsMatch(value, [
  branch(pattern, new F.FsLitBool(true)),
  branch(F.FsPatWildcard.value, new F.FsLitBool(false)),
]));

const convertedCall = boxedCall("add", int(20), boxedCall("add", int(10), int(12)));
const inlineCall = call("add", nativeInt(20), call("add", nativeInt(10), nativeInt(12)));
const objectCall = call("objectAdd", int(20), call("objectAdd", int(10), int(12)));
const mixedCall = boxedCall("add", call("objectIdentity", int(20)), call("objectIdentity", boxedCall("add", int(10), int(12))));
const guarded = (value, positive, result, fallback) => new F.FsMatch(value, [
  branch(F.FsPatWildcard.value, result, new Just(positive)),
  branch(F.FsPatWildcard.value, fallback),
]);
const convertedMatch = guarded(int(1), boxedCall("positive", int(1)), convertedCall, int(0));
const inlineMatch = guarded(nativeInt(1), call("positive", nativeInt(1)), inlineCall, nativeInt(0));

// Reuse the exact same AST object in declarations and templates. Compilation
// checks its F# types; execution checks the boundary conversions and result.
const placements = [];
for (const [label, expr] of [["native", inlineCall], ["adapted", convertedCall], ["object", objectCall], ["mixed", mixedCall]]) {
  assert.equal(printExpr(expr), printExprInline(expr), `${label}: renderer entrypoint is irrelevant`);
  placements.push(printDecl(new F.FsLet(`${label}Value`, [], expr)));
  const contexts = [
    ["declaration", ident(`${label}Value`)],
    ["lambda", new F.FsApp(Boxed.lambda("ignored")(expr), [int(0)])],
    ["constructor", new F.FsMatch(Boxed.unbox(new F.FsCtorApp("BoxedValue", [Boxed.box(expr)])), [
      branch(new F.FsPatCtor("BoxedValue", [new F.FsPatIdent("value")]), ident("value")),
    ])],
    ["native factory", new F.FsMatch(Boxed.unbox(Boxed.nativeConstructor("single")([Boxed.box(expr)])), [
      branch(new F.FsPatCtor("BoxedValue", [new F.FsPatIdent("value")]), ident("value")),
    ])],
    ["guard", guarded(int(0), boxedCall("positive", Boxed.box(expr)), int(42), int(0))],
    ["branch", guarded(int(0), new F.FsLitBool(true), expr, expr)],
    ["boxed branch", Boxed.letIn([])(guarded(int(0), new F.FsLitBool(true), expr, expr))],
    ["generic application", new F.FsApp(ident("boxedIdentity"), [expr])],
    ["record", Boxed.accessRecord("value")(Boxed.record([{ key: "value", value: expr }]))],
  ];
  for (const [placement, placed] of contexts) {
    assert.equal(printExpr(placed), printExprInline(placed), `${label}/${placement}: stable nested adapters`);
    placements.push(`check "${label}/${placement}" (unbox<int> (box (${printExpr(placed)})) = 42)`);
  }
}

const partials = [
  ["adapted native partial", boxedCall("add", int(20)), [22], [23]],
  ["native partial", call("add", nativeInt(20)), [22], [23]],
  ["object partial", call("objectAdd", int(20)), [22], [23]],
  ["boxed function value", boxedCall("add"), [20, 22], [21, 22]],
  ["native function value", call("add"), [20, 22], [21, 22]],
];
const orderedCall = boxedCall("add",
  call("objectObserve", int(1), int(20)),
  boxedCall("observe", int(2), boxedCall("add", int(10), int(12))));

const literalPatterns = [
  ["integer", int(-2147483648), new F.FsPatLitInt(-2147483648)],
  ["number", new F.FsLitNumber(1.5), new F.FsPatLitNumber(1.5)],
  ["boolean", new F.FsLitBool(true), new F.FsPatLitBool(true)],
  ["escaped string", new F.FsLitString('a"\\\n🙂'), new F.FsPatLitString('a"\\\n🙂')],
  ["lone surrogate string", new F.FsLitString('\ud800'), new F.FsPatLitString('\ud800')],
  ["escaped character", new F.FsLitChar("'"), new F.FsPatLitChar("'")],
];
const record = Boxed.record([
  { key: "name", value: new F.FsLitString("entry") },
  { key: "value", value: int(42) },
]);
const recordPattern = new F.FsPatRecord([
  { key: "name", pattern: new F.FsPatLitString("entry") },
  { key: "value", pattern: new F.FsPatIdent("value") },
]);
const recordMatch = new F.FsMatch(record, [branch(recordPattern, ident("value"))]);
const arrayPattern = new F.FsPatArray([
  new F.FsPatNamed("first", new F.FsPatLitInt(41)),
  new F.FsPatIdent("second"),
]);
const arrayMatch = new F.FsMatch(Boxed.unbox(Boxed.array([int(41), int(42)])), [
  branch(arrayPattern, ident("second")),
]);
const tuplePattern = new F.FsPatTuple([new F.FsPatLitInt(42), new F.FsPatLitString("entry")]);

// Both recursive groups are embedded inside a lambda. Their final columns
// depend on the surrounding expression, including the preceding string value.
const inner = Boxed.letIn([new Boxed.LocalRecursive([
  { name: "inner", args: ["value"], body: new F.FsRawExpr("(box (unbox<int> value + 1))") },
])])(call("inner_tco", ident("argument")));
const outer = Boxed.letIn([
  new Boxed.LocalValue("label", new F.FsLitString("🙂")),
  new Boxed.LocalRecursive([{ name: "outer", args: ["argument"], body: inner }]),
])(call("outer_tco", int(41)));
const nested = printDecl(new F.FsLet("nested", [], Boxed.lambda("ignored")(outer)));

const script = normalizeRecIndent(`
#load "Sharpurs_Prelude.fs"
open Sharpurs_Prelude
let mutable checks = 0
let check name passed =
    if not passed then failwith name
    checks <- checks + 1
let add (left: int) (right: int) = left + right
let positive (value: int) = value > 0
let constant = 42
let objectAdd (left: obj) (right: obj) : obj = box (unbox<int> left + unbox<int> right)
let objectIdentity (value: obj) = value
let boxedIdentity : obj = box objectIdentity
let objectConstant : obj = box 42
let returnAdder (value: int) : obj = box (fun (other: obj) -> box (value + unbox<int> other))
let events = ResizeArray<int>()
let observe (label: int) (value: int) = events.Add(label); value
let objectObserve (label: obj) (value: obj) = events.Add(unbox<int> label); value
type Pair = Pair of int * int
type BoxedValue = BoxedValue of obj
let single_adt_native (value: int) = BoxedValue (box value)
let nullary_adt_native = BoxedValue (box 42)

let boxedResult: obj = ${printExpr(convertedCall)}
let inlineResult: int = ${printExprInline(inlineCall)}
check "converted direct calls" (unbox<int> boxedResult = 42)
check "inline direct calls" (inlineResult = 42)
let inlineZero: int = ${printExprInline(call("constant"))}
check "zero-argument direct value" (inlineZero = unbox<int> (${printExpr(boxedCall("constant"))}))
check "zero-argument object value" (unbox<int> (${printExpr(call("objectConstant"))}) = 42)
check "zero-argument native factory" (unbox<BoxedValue> (${printExpr(Boxed.nativeConstructor("nullary")([]))}) = BoxedValue (box 42))
check "converted match guard and body" (unbox<int> (${printExpr(convertedMatch)}) = 42)
let inlineGuarded: int = ${printExprInline(inlineMatch)}
check "inline match guard and body" (inlineGuarded = 42)
let pair = unbox<Pair> (${printExprInline(new F.FsCtorApp("Pair", [inlineCall, nativeInt(7)]))})
check "inline calls inside a constructor" (pair = Pair(42, 7))
${placements.join("\n")}
${partials.map(([label, expr, first, reused], index) => `
let partial${index} : obj = box (${printExpr(expr)})
check "${label} first use" (unbox<int> (${printExpr(new F.FsApp(ident(`partial${index}`), first.map(int)))}) = 42)
check "${label} reused" (unbox<int> (${printExpr(new F.FsApp(ident(`partial${index}`), reused.map(int)))}) = 43)
`).join("\n")}
check "over-applied native call" (unbox<int> (${printExpr(new F.FsApp(boxedCall("returnAdder", int(20)), [int(22)]))}) = 42)
check "mixed nested argument result" (unbox<int> (${printExpr(orderedCall)}) = 42)
check "mixed nested evaluation order and count" (List.ofSeq events = [1; 2])
${literalPatterns.map(([label, value, pattern]) => `check "${label} pattern" (unbox<bool> (${matches(value, pattern)}))`).join("\n")}
check "record pattern" (unbox<int> (${printExpr(recordMatch)}) = 42)
check "named array pattern" (unbox<int> (${printExpr(arrayMatch)}) = 42)
check "tuple pattern" (unbox<bool> (${matches(Boxed.matchValue([int(42), new F.FsLitString("entry")]), tuplePattern)}))
${nested}
check "nested recursive layout" (unbox<int> (sharpurs_apply nested null) = 42)
printfn "printer: %d runtime checks passed" checks
`);

const directory = await mkdtemp(join(tmpdir(), "sharpurs-printer-"));
try {
  await writeFile(join(directory, "Sharpurs_Prelude.fs"), prelude);
  const path = join(directory, "printer.fsx");
  await writeFile(path, script);
  const result = spawnSync(process.env.DOTNET || "dotnet", ["fsi", "--nologo", "--exec", path], {
    stdio: "inherit", timeout: 30_000,
  });
  if (result.error) throw result.error;
  if (result.status !== 0) throw new Error(`Printer assertions failed (${result.signal || result.status})`);
} finally {
  await rm(directory, { recursive: true, force: true });
}
