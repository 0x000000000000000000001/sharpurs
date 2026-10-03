// Conflicting, already-recognized candidates exercise production registration
// and emission together. Recognizer eligibility/runtime checks live in the
// corresponding focused suites; this test fixes the policy between those paths.
import assert from "node:assert/strict";
import { annotation, sourceBinding, sourceApplication, sourceVariable } from "./support/ast.mjs";
import * as C from "../output/PureScript.Backend.Optimizer.CoreFn/index.js";
import { Just, Nothing } from "../output/Data.Maybe/index.js";
import * as Map from "../output/Data.Map.Internal/index.js";
import * as Set from "../output/Data.Set/index.js";
import * as F from "../output/Sharpurs.FsAst/index.js";
import * as K from "../output/Sharpurs.IntKernel/index.js";
import * as Boxed from "../output/Sharpurs.CodeGen.Boxed/index.js";
import * as CodeGen from "../output/Sharpurs.CodeGen/index.js";
import { printKernel } from "../output/Sharpurs.IntKernel.CodeGen/index.js";
import { printDecl, printModule } from "../output/Sharpurs.Printer/index.js";

const int = C.Int.value;
const signature = new C.Func([int, int], int);
const suffix = new C.Func([int], int);
const moduleName = "Selection.Module";
const prefix = "Selection_Module";
const qualified = name => new C.Qualified(new Just(moduleName), name);
const ann = type => ({ span: C.emptySpan, meta: Nothing.value, type: new Just(type), sourceUsage: Nothing.value });
const lit = value => new C.ExprLit(ann(int), new C.LitInt(value));
const fn = (name = "choose", args = ["left", "right"]) => new C.Binding(ann(signature), name,
  new C.ExprAbs(ann(signature), args[0], new C.ExprAbs(ann(suffix), args[1], lit(44))));
const called = (name = "choose") => new C.NonRec(new C.Binding(ann(int), "called",
  new C.ExprApp(ann(int),
    new C.ExprApp(ann(suffix), new C.ExprVar(ann(signature), qualified(name)), lit(1)), lit(2))));
const other = new C.Binding(ann(int), "other", lit(55));
const source = (decls, foreign = Map.empty) => ({
  name: moduleName, path: "Selection.purs", span: C.emptySpan, imports: [], exports: [], reExports: [],
  dataDecls: [], classDecls: [], decls, foreign, comments: [],
});
const layout = { moduleName, types: Map.empty, constructors: Map.empty, declarations: [] };
const nativeDeclaration = new F.FsRaw(`let ${prefix}_choose : obj = box (fun (_: obj) -> box (fun (_: obj) -> box 11))`);
const kernel = {
  name: qualified("choose"),
  args: [{ name: new Just("left"), level: 0 }, { name: new Just("right"), level: 1 }],
  body: new K.IntLiteral(22),
};
const expression = new F.FsRawExpr("(box (fun (_: obj) -> box (fun (_: obj) -> box 33)))");
const emptyCandidates = { kernels: Map.empty, expressions: Map.empty };
const environment = (native = false) => ({
  arities: Map.empty, wrappers: Set.empty, direct: Map.empty, thunks: Nothing.value,
  native: native ? new Just({ layout, bindings: Map.singleton("choose")(nativeDeclaration), nativeNames: ["choose"] }) : Nothing.value,
});
const emit = (core, candidates = emptyCandidates, env = environment()) =>
  printModule(CodeGen.translateModuleUsing(env)(candidates)(core));
const render = decls => decls.map(printDecl).join("\n");
const fallback = group => render(CodeGen.translateBind(Map.empty)(new Just(prefix))(group));
const direct = printDecl(Boxed.directBinding({ name: `${prefix}_choose`, args: ["left", "right"], body: new F.FsLitInt(44) }));
const emitted = {
  adt: printDecl(nativeDeclaration),
  kernel: printDecl(printKernel(`${prefix}_choose`)(kernel)),
  expression: printDecl(new F.FsLet(`${prefix}_choose`, [], expression)),
  direct,
};
let checks = 0;
function check(label, condition) { assert.ok(condition, label); checks++; }
function includes(label, text, fragment) { check(label, text.includes(fragment)); }

// Each row declares the expected winner for NonRec and singleton Rec. Every
// combination is also applied to a mutual group, which must remain generic.
const priorities = [
  { adt: true, kernel: true, expression: true, nonrec: "adt", recursive: "adt" },
  { adt: true, kernel: true, expression: false, nonrec: "adt", recursive: "adt" },
  { adt: true, kernel: false, expression: true, nonrec: "adt", recursive: "adt" },
  { adt: true, kernel: false, expression: false, nonrec: "adt", recursive: "adt" },
  { adt: false, kernel: true, expression: true, nonrec: "kernel", recursive: "kernel" },
  { adt: false, kernel: true, expression: false, nonrec: "kernel", recursive: "kernel" },
  { adt: false, kernel: false, expression: true, nonrec: "expression", recursive: "generic" },
  { adt: false, kernel: false, expression: false, nonrec: "direct", recursive: "generic" },
];
for (const [index, row] of priorities.entries()) {
  const candidates = {
    kernels: row.kernel ? Map.singleton("choose")(kernel) : Map.empty,
    expressions: row.expression ? Map.singleton("choose")(expression) : Map.empty,
  };
  for (const [kind, group, winner] of [
    ["NonRec", new C.NonRec(fn()), row.nonrec],
    ["Rec singleton", new C.Rec([fn()]), row.recursive],
    ["Rec mutual", new C.Rec([fn(), other]), "generic"],
  ]) {
    const label = `candidate set ${index}, ${kind}`;
    const generated = emit(source([group, called()]), candidates, environment(row.adt));
    includes(`${label}: emitted winning implementation`, generated,
      winner === "generic" ? fallback(group) : emitted[winner]);
    const caller = generated.split("\n").find(line => line.startsWith(`let ${prefix}_called `));
    check(`${label}: caller uses only an emitted direct helper`,
      caller.includes(`${prefix}_choose_direct_apply`) === (winner === "direct"));
    check(`${label}: suppressed direct implementation is absent`,
      generated.includes(`let ${prefix}_choose_direct `) === (winner === "direct"));
  }
}

function rejectsDirect(label, binding, extra = [], foreign = Map.empty) {
  const core = source([new C.NonRec(binding), ...extra, called(binding.value1)], foreign);
  const generated = emit(core);
  const name = `${prefix}_${F.sanitizeName(binding.value1)}`;
  includes(`${label}: original function retained`, generated, fallback(new C.NonRec(binding)));
  const caller = generated.split("\n").find(line => line.startsWith(`let ${prefix}_called `));
  check(`${label}: call retains public ABI`, !caller.includes(`${name}_direct_apply`));
}
for (const suffix of ["_direct", "_direct_apply"]) {
  const collision = new C.Binding(ann(int), `choose${suffix}`, lit(66));
  rejectsDirect(`nonrecursive source collision ${suffix}`, fn(), [new C.NonRec(collision)]);
  rejectsDirect(`recursive source collision ${suffix}`, fn(), [new C.Rec([collision])]);
  rejectsDirect(`foreign source collision ${suffix}`, fn(), [], Map.singleton(`choose${suffix}`)(new Just(int)));
  rejectsDirect(`parameter capture ${suffix}`, fn("choose", [`${prefix}_choose${suffix}`, "right"]));
  rejectsDirect(`collision after identifier escaping ${suffix}`, fn("choose'"),
    [new C.NonRec(new C.Binding(ann(int), `choose_prime${suffix}`, lit(77)))]);
}

const core = source([new C.NonRec(fn()), called()]);
const missingType = fn();
annotation(missingType).type = Nothing.value;
rejectsDirect("unproven source signature", missingType);

const unrelated = {
  kernels: Map.singleton("absent")(kernel),
  expressions: Map.singleton("alsoAbsent")(expression),
};
check("candidates without source bindings neither emit code nor suppress direct entries", emit(core, unrelated) === emit(core));
const unrelatedNative = environment(true);
unrelatedNative.native.value0.bindings = Map.singleton("absent")(nativeDeclaration);
check("unrelated native binding does not suppress the source direct function", emit(core, emptyCandidates, unrelatedNative).includes(direct));

const unqualifiedCall = called();
const importedCall = called();
function callee(group) {
  const binding = sourceBinding(source([group]), "called");
  return sourceVariable(sourceApplication(binding.expression, 2, "called fixture").head);
}
callee(unqualifiedCall).owner = Nothing.value;
callee(importedCall).owner = new Just("Other.Module");
for (const [label, call] of [["lexical shadow", unqualifiedCall], ["other owner", importedCall]]) {
  const generated = emit(source([new C.NonRec(fn()), call]));
  const caller = generated.split("\n").find(line => line.startsWith(`let ${prefix}_called `));
  includes(`${label}: helper still emitted for original binding`, generated, direct);
  check(`${label}: qualified registry cannot capture the call`, !caller.includes(`${prefix}_choose_direct_apply`));
}

console.log(`selection: ${checks} registration/emission checks passed`);
