// Run after npm run build: DOTNET=/path/to/dotnet node tests/local-int-kernel.mjs
import assert from "node:assert/strict";
import { mkdtemp, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { spawnSync } from "node:child_process";
import { optimizedTyped } from "./support/ast.mjs";
import { fsharpFixture } from "./support/fsharp.mjs";
import * as C from "../output/PureScript.Backend.Optimizer.CoreFn/index.js";
import * as S from "../output/PureScript.Backend.Optimizer.Syntax/index.js";
import { Tuple } from "../output/Data.Tuple/index.js";
import { Just, Nothing } from "../output/Data.Maybe/index.js";
import * as Map from "../output/Data.Map/index.js";
import { fromLocal, fromBinding as fromGlobalBinding } from "../output/Sharpurs.IntKernel/index.js";
import { printKernel, printLocalKernel } from "../output/Sharpurs.IntKernel.CodeGen/index.js";
import { fromBinding } from "../output/Sharpurs.Optimized/index.js";
import { translateBind, translateBindWithOptimizations } from "../output/Sharpurs.CodeGen/index.js";
import { FsLet } from "../output/Sharpurs.FsAst/index.js";
import { printDecl, printExpr } from "../output/Sharpurs.Printer/index.js";
import { helpers as prelude } from "../output/Sharpurs.Runtime/index.js";

const int = C.Int.value;
const string = C.String.value;
const variable = new C.TypeVar("a");
const func = (args, result = int) => new C.Func(args, result);
const typed = (type, expression) => new S.Typed(type, expression);
const lit = (value) => typed(int, new S.Lit(new C.LitInt(value)));
const local = (level, name = "value") => new S.Local(new Just(name), level);
const call = (fn, ...args) => new S.App(fn, args);
const abs = (args, body) => new S.Abs(args.map(([name, level]) => new Tuple(new Just(name), level)), body);
const operator = (op, left, right) => new S.PrimOp(new S.Op2(new S.OpIntNum(op), left, right));
const add = (left, right) => operator(S.OpAdd.value, left, right);
const subtract = (left, right) => operator(S.OpSubtract.value, left, right);
const equal = (left, right) => new S.PrimOp(new S.Op2(new S.OpIntOrd(S.OpEq.value), left, right));
const branch = (condition, yes, no) => new S.Branch([new S.Pair(condition, yes)], no);
const global = (name) => new S.Var(new C.Qualified(new Just("FixtureFx"), name));
const effect = (type) => new C.ADT("Effect", "Effect", [type]);

// NeutralExpr, Ident, Level and NonEmptyArray are erased newtypes in compiled PBO.
// The fixture is deliberately independent of all benchmark binding names.
function loop(options = {}) {
  const recLevel = options.recLevel ?? 1;
  const levels = options.levels ?? [recLevel + 1, recLevel + 2];
  const names = options.names ?? ["remaining", "accumulator"];
  const self = local(options.selfLevel ?? recLevel, options.selfName ?? "recur");
  const n = local(levels[0], names[0]);
  const acc = local(levels[1], names[1]);
  const next = options.next ?? call(self, subtract(n, lit(1)), add(acc, lit(1)));
  const body = options.body ?? branch(equal(n, lit(0)), acc, next);
  const signature = options.signature ?? func([int, int]);
  const fn = typed(signature, abs([[names[0], levels[0]]],
    typed(options.innerSignature ?? func([int]), abs([[names[1], levels[1]]], typed(int, body)))));
  const bindings = [new Tuple("recur", fn), ...(options.extraBindings ?? [])];
  const entry = options.entry ?? call(local(recLevel, "recur"), typed(int, local(0)), lit(0));
  return typed(int, new S.LetRec(recLevel, bindings, entry));
}

let checks = 0;
function accepted(label, result) {
  assert.ok(result instanceof Just, label);
  checks += 1;
  return result.value0;
}
function rejected(label, result) {
  assert.ok(result instanceof Nothing, label);
  checks += 1;
}
const acceptLocal = (label, expression, scope = [0]) => accepted(label, fromLocal(scope)(expression));
const rejectLocal = (label, expression, scope = [0]) => rejected(label, fromLocal(scope)(expression));
const rejectBinding = (label, expression) => rejected(label, fromBinding(expression));

const kernel = acceptLocal("typed singleton local loop", loop());
acceptLocal("repeated consistent annotations", typed(int, loop()));
acceptLocal("closed literal entry", loop({ entry: call(local(1, "recur"), lit(10), lit(-7)) }), []);
acceptLocal("gaps between binder levels", loop({ recLevel: 3, levels: [8, 12] }));
acceptLocal("repeated binder names still use levels", loop({ names: ["same", "same"] }));
const nativeText = printLocalKernel(kernel);
assert.ok(nativeText.includes(": int"), "local loop has native Int parameters and result");
assert.ok(!nativeText.includes("sharpurs_apply"), "local kernel has no dynamic application");
checks += 2;

rejectLocal("recursive body cannot capture an outer object", loop({ body: local(0) }));
rejectLocal("self identifier must match recursive binding", loop({ selfName: "unrelated" }));
rejectLocal("self level must match recursive binding", loop({ selfLevel: 0 }));
rejectLocal("mutual recursion falls back as a whole", loop({ extraBindings: [new Tuple("other", lit(0))] }));
rejectLocal("partial recursive call", loop({ next: call(local(1, "recur"), lit(0)) }));
rejectLocal("over-applied recursive call", loop({ next: call(local(1, "recur"), lit(0), lit(0), lit(0)) }));
rejectLocal("non-tail recursive call", loop({ next: add(call(local(1, "recur"), lit(0), lit(0)), lit(1)) }));
rejectLocal("partial entry call", loop({ entry: call(local(1, "recur"), lit(0)) }));
rejectLocal("over-applied entry call", loop({ entry: call(local(1, "recur"), lit(0), lit(0), lit(0)) }));
rejectLocal("entry identifier mismatch", loop({ entry: call(local(1, "other"), lit(0), lit(0)) }));
rejectLocal("entry self level mismatch", loop({ entry: call(local(0, "recur"), lit(0), lit(0)) }));
rejectLocal("unproven entry local", loop(), []);
rejectLocal("entry cannot see recursive parameters", loop({ entry: call(local(1, "recur"), local(2), lit(0)) }));
rejectLocal("stale polymorphic signature", loop({ signature: func([int, variable], variable) }));
rejectLocal("stale nested signature", loop({ innerSignature: func([variable], variable) }));
rejectLocal("stale body annotation", loop({ body: typed(variable, local(3)) }));
rejectLocal("String specialization is not an Int loop", loop({ signature: func([int, string], string) }));
rejectLocal("untyped LetRec is not enough evidence", optimizedTyped(loop()).expression);
rejectLocal("contradictory root annotation", typed(string, loop()));
rejectLocal("parameter levels must be distinct", loop({ levels: [2, 2] }));
rejectLocal("parameter cannot shadow recursive group level", loop({ levels: [1, 3] }));
rejectLocal("parameter cannot collide with enclosing scope", loop({ levels: [0, 3] }));
rejectLocal("negative recursive level", loop({ recLevel: -1 }), []);
rejectLocal("outer levels cannot include recursive binding", loop(), [0, 1]);
rejectLocal("outer levels cannot be duplicated", loop(), [0, 0]);
rejectLocal("outer levels cannot be negative", loop(), [-1]);
rejectLocal("unsupported operation falls back", loop({ body: operator(S.OpMultiply.value, local(2), local(3)) }));

const unary = (type, body) => typed(func([type]), abs([["input", 0]], body));
const plain = typed(func([int, int]), abs([["input", 0]], typed(func([int]), abs([["initial", 1]],
  loop({ recLevel: 2, entry: call(local(2, "recur"), local(0), local(1)) })))));
const plainOutput = printExpr(accepted("boxed lambda with proven Int locals", fromBinding(plain)));
accepted("opaque global call around a native loop", fromBinding(call(global("consume"), plain)));
accepted("opaque TypeApp global head", fromBinding(call(new S.TypeApp(global("consume"), int), plain)));
accepted("standalone closed native local loop", fromBinding(loop({ entry: call(local(1, "recur"), lit(3), lit(4)) })));
rejectBinding("ordinary binding without a local native loop uses old generator", unary(int, local(0)));
rejectBinding("untyped outer lambda cannot prove entry local", abs([["input", 0]], loop()));
rejectBinding("String outer lambda cannot be unboxed as Int", unary(string, loop()));
rejectBinding("generic outer lambda cannot be unboxed as Int", unary(variable, loop()));
rejectBinding("entry Typed Int cannot override a proven String local", unary(string,
  loop({ entry: call(local(1, "recur"), typed(int, local(0)), lit(0)) })));
rejectBinding("inconsistent enclosing function signatures", typed(func([string]), unary(int, loop())));
rejectBinding("declared String result cannot become a native Int result", typed(func([int], string), abs([["input", 0]], loop())));
rejectBinding("TypeApp around a native loop is unresolved", unary(int, new S.TypeApp(loop(), int)));
rejectBinding("TypeApp around a lambda is unresolved", new S.TypeApp(plain, int));
rejectBinding("unknown outer local cannot escape scope", call(global("consume"), plain, local(99)));
rejectBinding("unsupported enclosing syntax rejects entire binding", new S.EffectPure(plain));
rejectBinding("unsupported sibling rejects entire binding", call(global("consume"), plain,
  new S.EffectPure(lit(0))));
rejectBinding("outer lambda cannot duplicate binder levels", typed(func([int, int]), abs([["x", 0], ["y", 0]], loop())));
rejectBinding("outer lambda cannot reuse a captured level", unary(int, unary(int, loop())));

// Exercise production routing with both candidate maps populated. A local-loop
// replacement is valid only for NonRec; the established global kernel wins.
const annotation = { span: C.emptySpan, meta: Nothing.value, type: Nothing.value };
const original = new C.Binding(annotation, "value", new C.ExprLit(annotation, new C.LitInt(41)));
const otherOriginal = new C.Binding(annotation, "other", new C.ExprLit(annotation, new C.LitInt(42)));
const expression = accepted("routing fixture has a local loop", fromBinding(plain));
const expressions = Map.singleton("value")(expression);
const globalKernel = accepted("routing fixture has a global kernel", fromGlobalBinding(
  new C.Qualified(new Just("Route"), "value"))(unary(int, local(0))));
const kernels = Map.singleton("value")(globalKernel);
const render = (decls) => decls.map(printDecl).join("\n");
const route = (binding, globalCandidates = Map.empty, localCandidates = expressions) =>
  render(translateBindWithOptimizations(Map.empty)(globalCandidates)(localCandidates)("Route")(binding));
const fallback = (binding) => render(translateBind(Map.empty)(new Just("Route"))(binding));
const nonRec = new C.NonRec(original);
const singletonRec = new C.Rec([original]);
const mutualRec = new C.Rec([original, otherOriginal]);
function routed(label, actual, expected) {
  assert.equal(actual, expected, label);
  checks += 1;
}
routed("NonRec uses accepted local-loop expression", route(nonRec), printDecl(new FsLet("Route_value", [], expression)));
routed("unmatched NonRec retains original generator", route(new C.NonRec(otherOriginal)), fallback(new C.NonRec(otherOriginal)));
routed("singleton Rec ignores local-loop replacement", route(singletonRec), fallback(singletonRec));
routed("mutual Rec ignores local-loop replacement", route(mutualRec), fallback(mutualRec));
routed("mutual Rec stays intact even with a global candidate", route(mutualRec, kernels), fallback(mutualRec));
const globalOutput = printDecl(printKernel("Route_value")(globalKernel));
routed("global kernel has priority for NonRec", route(nonRec, kernels), globalOutput);
routed("global kernel has priority for singleton Rec", route(singletonRec, kernels), globalOutput);
const rejectedExpression = fromBinding(unary(string, loop()));
const rejectedCandidates = rejectedExpression instanceof Just ? Map.singleton("value")(rejectedExpression.value0) : Map.empty;
routed("rejected complete expression retains original generator", route(nonRec, Map.empty, rejectedCandidates), fallback(nonRec));

function effectFixture(iterations, initial) {
  const nativeLoop = loop({ entry: call(local(1, "recur"), local(0), lit(initial)) });
  const continuation = typed(func([int], effect(string)), abs([["boundInput", 0]],
    typed(effect(string), call(global("pure"), typed(string, call(global("show"), nativeLoop))))));
  return typed(effect(string), call(global("bind"),
    typed(effect(int), call(global("opaque"), lit(iterations))), continuation));
}

const cases = [[0, 0], [1, 0], [10, 0], [10, -7], [1, 2147483647], [1000000, 9]];
const effects = cases.map(([iterations, initial], index) => {
  const output = printExpr(accepted(`effect envelope ${iterations}/${initial}`, fromBinding(effectFixture(iterations, initial))));
  return `let effect${index} : obj = ${output}`;
});
const doubles = await fsharpFixture("local-int-kernel/Support.fs");
const fsCases = cases.map(([n, acc]) => `(${n}, ${acc}, ${(n + acc) | 0})`).join("; ");
const runtime = await fsharpFixture("local-int-kernel/Runtime.fs", {
  CASES: fsCases,
  EFFECTS: cases.map(([n, acc], index) => `(effect${index}, "${(n + acc) | 0}")`).join("; "),
});

const directory = await mkdtemp(join(tmpdir(), "sharpurs-local-int-kernel-"));
try {
  const script = join(directory, "local-int-kernel.fsx");
  await writeFile(script, [prelude, doubles, `let plain : obj = ${plainOutput}`, ...effects, runtime].join("\n\n"));
  const result = spawnSync(process.env.DOTNET || "dotnet", [
    "fsi", "--nologo", "--optimize+", "--exec", script,
  ], { stdio: "inherit", timeout: 30_000 });
  if (result.error) throw result.error;
  if (result.status !== 0) throw new Error(`Local IntKernel runtime assertions failed (${result.signal || result.status})`);
} finally {
  await rm(directory, { recursive: true, force: true });
}
console.log(`local-int-kernel converter: ${checks} checks passed`);
