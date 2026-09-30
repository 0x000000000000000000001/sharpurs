// Run after building the compiler. Exercise the two direct-call conventions
// and structured patterns by compiling and executing their generated F#.
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
const call = (name, ...args) => new F.FsDirectApp(name, args);
const branch = (pattern, body, guard = Nothing.value) => new F.FsMatchCase(pattern, guard, body);
const matches = (value, pattern) => printExpr(new F.FsMatch(value, [
  branch(pattern, new F.FsLitBool(true)),
  branch(F.FsPatWildcard.value, new F.FsLitBool(false)),
]));

const convertedCall = call("add", int(20), call("add", int(10), int(12)));
const inlineCall = call("add", nativeInt(20), call("add", nativeInt(10), nativeInt(12)));
const guarded = (value, positive, result, fallback) => new F.FsMatch(value, [
  branch(F.FsPatWildcard.value, result, new Just(positive)),
  branch(F.FsPatWildcard.value, fallback),
]);
const convertedMatch = guarded(int(1), call("positive", int(1)), convertedCall, int(0));
const inlineMatch = guarded(nativeInt(1), call("positive", nativeInt(1)), inlineCall, nativeInt(0));

const literalPatterns = [
  ["integer", int(-2147483648), new F.FsPatLitInt(-2147483648)],
  ["number", new F.FsLitNumber(1.5), new F.FsPatLitNumber(1.5)],
  ["boolean", new F.FsLitBool(true), new F.FsPatLitBool(true)],
  ["escaped string", new F.FsLitString('a"\\\n🙂'), new F.FsPatLitString('a"\\\n🙂')],
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
type Pair = Pair of int * int

let boxedResult: obj = ${printExpr(convertedCall)}
let inlineResult: int = ${printExprInline(inlineCall)}
check "converted direct calls" (unbox<int> boxedResult = 42)
check "inline direct calls" (inlineResult = 42)
let inlineZero: int = ${printExprInline(call("constant"))}
check "zero-argument direct value" (inlineZero = unbox<int> (${printExpr(call("constant"))}))
check "converted match guard and body" (unbox<int> (${printExpr(convertedMatch)}) = 42)
let inlineGuarded: int = ${printExprInline(inlineMatch)}
check "inline match guard and body" (inlineGuarded = 42)
let pair = unbox<Pair> (${printExprInline(new F.FsCtorApp("Pair", [inlineCall, nativeInt(7)]))})
check "inline calls inside a constructor" (pair = Pair(42, 7))
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
