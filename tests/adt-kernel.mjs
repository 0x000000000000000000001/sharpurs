// Run after npm run build, with the TAST fork on PATH or PURS=/path/to/purs.
// DOTNET=/path/to/dotnet and PRELUDE_SRC=/path/to/prelude/src are optional.
import assert from "node:assert/strict";
import { mkdtemp, readdir, readFile, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { spawnSync } from "node:child_process";
import * as C from "../output/PureScript.Backend.Optimizer.CoreFn/index.js";
import * as S from "../output/PureScript.Backend.Optimizer.Syntax/index.js";
import * as Aff from "../output/Effect.Aff/index.js";
import * as Applicative from "../output/Control.Applicative/index.js";
import { Left } from "../output/Data.Either/index.js";
import { Just, Nothing } from "../output/Data.Maybe/index.js";
import * as Map from "../output/Data.Map.Internal/index.js";
import * as Set from "../output/Data.Set/index.js";
import * as App from "../output/PureScript.Backend.Optimizer.App/index.js";
import * as Builder from "../output/PureScript.Backend.Optimizer.Builder/index.js";
import * as Foreign from "../output/PureScript.Backend.Optimizer.Semantics.Foreign/index.js";
import { fromModule } from "../output/Sharpurs.AdtKernel/index.js";
import { printModule } from "../output/Sharpurs.Printer/index.js";

const backend = resolve(dirname(fileURLToPath(import.meta.url)), "..");
const pure = Applicative.pure(Aff.applicativeAff);
const runAff = (action) => new Promise((resolve, reject) => {
  Aff.runAff((result) => () => result instanceof Left ? reject(result.value0) : resolve(result.value0))(action)();
});
function command(program, args, cwd) {
  const result = spawnSync(program, args, { cwd, encoding: "utf8", timeout: 60_000 });
  if (result.error) throw result.error;
  if (result.status !== 0) throw new Error(`${program} failed (${result.signal || result.status}):\n${result.stdout}\n${result.stderr}`);
  return result;
}

// Preserve the actual PureScript ADT constructors: structuredClone loses them.
function clone(value) {
  if (Array.isArray(value)) return value.map(clone);
  if (!value || typeof value !== "object") return value;
  return Object.assign(Object.create(Object.getPrototypeOf(value)), Object.fromEntries(
    Object.entries(value).map(([key, child]) => [key, clone(child)])));
}
function first(value, predicate, change) {
  if (!value || typeof value !== "object") return false;
  if (predicate(value)) { change(value); return true; }
  return Object.values(value).some((child) => first(child, predicate, change));
}
function binding(state, name) {
  const found = state.backend.bindings.flatMap((group) => group.bindings).find((item) => item.value0 === name);
  assert.ok(found, `fixture contains ${name}`);
  return found;
}
function changeFirst(state, name, type, change, predicate = () => true) {
  assert.ok(first(binding(state, name).value1, (node) => node instanceof type && predicate(node), change),
    `fixture ${name} contains the node selected for mutation`);
}
function changeLayout(state, change) {
  change(state.core.dataDecls);
  state.backend.dataDecls = clone(state.core.dataDecls);
}

let checks = 0;
const directory = await mkdtemp(join(tmpdir(), "sharpurs-adt-kernel-"));
const previousCwd = process.cwd();
try {
  const prelude = process.env.PRELUDE_SRC || join(backend, ".spago/p",
    (await readdir(join(backend, ".spago/p"))).find((name) => /^prelude-/.test(name)) || "prelude-not-installed", "src");
  const fixture = join(directory, "AdtPilot.purs");
  await writeFile(fixture, await readFile(join(backend, "tests/fixtures/AdtPilot.purs")));
  command(process.env.PURS || "purs", ["compile", fixture, join(prelude, "**/*.purs"),
    "--output", join(directory, "output"), "--codegen", "corefn,js"], directory);
  const raw = JSON.parse(await readFile(join(directory, "output/AdtPilot/corefn.json"), "utf8"));
  assert.ok(Array.isArray(raw.dataDecls) && raw.dataDecls.length === 2,
    "PURS must be the TAST fork preserving dataDecls");
  checks++;

  process.chdir(directory); // Keep Builder .purmeta and directives lookup isolated.
  let captured;
  await runAff(Builder.buildModules(Aff.monadAff)({
    directives: await runAff(App.loadDirectives),
    rewriteLimit: 10000,
    analyzeCustom: (_) => (_) => Nothing.value,
    foreignSemantics: Map.filterKeys(C.ordQualified(C.ordIdent))((qualified) => {
      if (qualified.value0 instanceof Just) {
        const name = qualified.value0.value0;
        return !name.includes("Effect") && !name.includes("Control.Monad.ST");
      }
      return true;
    })(Foreign.coreForeignSemantics),
    traceIdents: Set.empty,
    onPrepareModule: (_) => (module) => pure(module),
    onSkipModule: (_) => (_) => pure(Nothing.value),
    onCodegenModule: (_) => (core) => (module) => (_) => {
      if (module.name === "AdtPilot") {
        assert.equal(captured, undefined, "exactly one fixture callback");
        captured = { core, backend: module };
      }
      return pure(undefined);
    },
  })(await runAff(App.coreFnModulesFromOutput(join(directory, "output")))));
  assert.ok(captured, "the real Builder reached AdtPilot");
  const result = fromModule(captured.core)(captured.backend);
  assert.ok(result instanceof Just, "the complete real Color/Tree fixture is accepted");
  checks += 2;
  const generated = printModule(result.value0);
  assert.match(generated, /AdtPilot_Tusd_Ctor of AdtPilot_Color \* AdtPilot_Tree \* int \* AdtPilot_Tree/);
  const nativeLines = generated.split("\n").filter((line) => /^(?:let(?: rec)?|and) AdtPilot_\w+_adt_native\b/.test(line));
  assert.equal(nativeLines.length, 14, "every real binding has a native definition");
  assert.ok(nativeLines.every((line) => !/sharpurs_apply|\bunbox\b|\bbox\s*\(|\bobj\b/.test(line)),
    "native definitions have no object conversion or dynamic application");
  checks += 3;

  const reject = (label, change) => {
    const modified = clone(captured);
    change(modified);
    assert.ok(fromModule(modified.core)(modified.backend) instanceof Nothing, label);
    checks++;
  };
  reject("absent layout", (s) => changeLayout(s, (decls) => decls.splice(0)));
  reject("polymorphic data declaration", (s) => changeLayout(s, (decls) => { decls[1].vars = ["a"]; }));
  reject("external ADT field", (s) => changeLayout(s, (decls) => {
    decls[1].constructors[1].fields[1] = new C.ADT("Elsewhere.Tree", ["Elsewhere", "Tree"], []);
  }));
  reject("missing local ADT", (s) => changeLayout(s, (decls) => {
    decls[1].constructors[1].fields[1] = new C.ADT("AdtPilot.Missing", ["AdtPilot", "Missing"], []);
  }));
  reject("incorrect qualified type path", (s) => changeLayout(s, (decls) => {
    decls[1].constructors[1].fields[1].value1 = ["AdtPilot", "Color"];
  }));
  reject("polymorphic type application in layout", (s) => changeLayout(s, (decls) => {
    decls[1].constructors[1].fields[1].value2 = [C.Int.value];
  }));
  reject("unsupported field type", (s) => changeLayout(s, (decls) => {
    decls[1].constructors[1].fields[2] = C.String.value;
  }));
  reject("contradictory source and optimized layouts", (s) => { s.backend.dataDecls = []; });
  reject("duplicate type names", (s) => changeLayout(s, (decls) => decls.push(clone(decls[0]))));
  reject("duplicate constructor names", (s) => changeLayout(s, (decls) => { decls[0].constructors[1].name = "R"; }));
  reject("type name collides with emitted constructor", (s) => changeLayout(s, (decls) => {
    decls.push({ name: "Tusd_Ctor", vars: [], constructors: [{ name: "Unused", fields: [] }] });
  }));
  reject("sanitized public binding names collide", (s) => {
    const group = s.backend.bindings.find((group) => group.bindings.some((item) => item.value0 === "depth"));
    const a = clone(binding(s, "depth")); a.value0 = "depth'";
    const b = clone(binding(s, "depth")); b.value0 = "depth_prime";
    group.bindings.push(a, b);
  });
  reject("module names must agree", (s) => { s.backend.name = "DifferentPilot"; });
  reject("stale binding annotation", (s) => {
    binding(s, "depth").value1.value0 = new C.Func([C.Int.value], C.Int.value);
  });
  reject("contradictory body annotation", (s) => changeFirst(s, "depth", S.Typed,
    (node) => { node.value0 = C.String.value; }, (node) => node.value0 instanceof C.Int));
  reject("unknown local level", (s) => changeFirst(s, "depth", S.Local, (node) => { node.value1 = 999; }));
  reject("duplicate parameter levels", (s) => {
    let seen;
    let parameters = 0;
    const expression = binding(s, "max").value1;
    const walk = (node) => {
      if (!node || typeof node !== "object") return;
      if (node instanceof S.Abs) for (const parameter of node.value0) {
        parameters++;
        if (seen === undefined) seen = parameter.value1;
        else parameter.value1 = seen;
      }
      Object.values(node).forEach(walk);
    };
    walk(expression);
    assert.ok(parameters > 1, "fixture max has two parameters to collide");
  });
  reject("invalid constructor field index", (s) => changeFirst(s, "depth", S.GetCtorField, (node) => {
    node.value5 = 9; node.value4 = "value9";
  }));
  reject("constructor field label must match its index", (s) => changeFirst(s, "depth", S.GetCtorField,
    (node) => { node.value4 = "value2"; }));
  reject("constructor projection type must agree", (s) => changeFirst(s, "depth", S.GetCtorField,
    (node) => { node.value2 = "Color"; }));
  reject("constructor field annotation must agree", (s) => changeFirst(s, "rootValue", S.Typed,
    (node) => { node.value0 = C.Boolean.value; }, (node) => node.value0 instanceof C.Int));
  reject("constructor application must be saturated", (s) => changeFirst(s, "singleton", S.CtorSaturated,
    (node) => { node.value4.pop(); }, (node) => node.value4.length > 0));
  reject("constructor argument order must match metadata", (s) => changeFirst(s, "singleton", S.CtorSaturated,
    (node) => { node.value4[0].value0 = "value3"; }, (node) => node.value4.length > 0));
  reject("native recursive call cannot be partial", (s) => changeFirst(s, "depth", S.App, (node) => { node.value1 = []; }));
  reject("native recursive call cannot be over-applied", (s) => changeFirst(s, "depth", S.App,
    (node) => { node.value1.push(new S.Lit(new C.LitInt(0))); }));
  reject("unresolved TypeApp", (s) => {
    const item = binding(s, "depth");
    item.value1.value1 = new S.TypeApp(item.value1.value1, C.Int.value);
  });
  reject("foreign binding requires the general backend", (s) => {
    s.backend.foreign = Map.singleton("foreignValue")(new Just(C.Int.value));
  });

  const runtime = `
open Microsoft.FSharp.Reflection
let mutable checks = 0
let check label ok = if not ok then failwith label else checks <- checks + 1
let apply (fn: obj) (arg: obj) : obj = (unbox<obj -> obj> fn) arg
let publicDepth tree = unbox<int> (apply AdtPilot_depth (box tree))
let publicRoot tree = unbox<int> (apply AdtPilot_rootValue (box tree))
let nativeEmpty = AdtPilot_empty_adt_native
let nativeOne = AdtPilot_singleton_adt_native
let nativeAsymmetric = AdtPilot_asymmetric_adt_native
check "native empty" (AdtPilot_depth_adt_native nativeEmpty = 0)
check "native singleton" (AdtPilot_depth_adt_native nativeOne = 1)
check "native asymmetry" (AdtPilot_depth_adt_native nativeAsymmetric = 3)
check "public empty" (publicDepth nativeEmpty = 0)
check "public singleton" (publicDepth nativeOne = 1)
check "public asymmetry" (publicDepth nativeAsymmetric = 3)
check "empty root" (publicRoot nativeEmpty = 0)
check "maximum Int" (publicRoot nativeOne = System.Int32.MaxValue)
check "minimum Int" (AdtPilot_rootValue_adt_native (AdtPilot_leftChild_adt_native nativeAsymmetric) = System.Int32.MinValue)
check "empty color" (not (AdtPilot_isRed_adt_native (AdtPilot_rootColor_adt_native nativeEmpty)))
check "singleton color" (not (AdtPilot_isRed_adt_native (AdtPilot_rootColor_adt_native nativeOne)))
check "nested red" (AdtPilot_isRed_adt_native (AdtPilot_rootColor_adt_native (AdtPilot_leftChild_adt_native nativeAsymmetric)))
let makeRed = apply AdtPilot_singletonWith AdtPilot_R
let redA = unbox<AdtPilot_Tree> (apply makeRed (box 7))
let redB = unbox<AdtPilot_Tree> (apply makeRed (box -9))
check "reused partial constructor first" (publicRoot redA = 7)
check "reused partial constructor second" (publicRoot redB = -9)
check "partial constructor color" (AdtPilot_isRed_adt_native (AdtPilot_rootColor_adt_native redA))
check "partial constructor depth" (publicDepth redA = 1)
let publicT = apply AdtPilot_T AdtPilot_B
let withLeft = apply publicT (box redA)
let withValue = apply withLeft (box 7)
let shared = unbox<AdtPilot_Tree> (apply withValue (box redA))
let sharedOther = unbox<AdtPilot_Tree> (apply withValue (box redB))
check "public constructor uses native layout" (AdtPilot_depth_adt_native shared = 2)
check "shared subtree identity" (System.Object.ReferenceEquals(AdtPilot_leftChild_adt_native shared, redA))
check "shared subtree survives other construction" (System.Object.ReferenceEquals(AdtPilot_leftChild_adt_native sharedOther, redA))
check "shared value unchanged" (publicRoot redA = 7)
check "public projection preserves identity" (System.Object.ReferenceEquals(apply AdtPilot_leftChild (box shared), box redA))
check "public Color result" (unbox<bool> (apply AdtPilot_isRed (apply AdtPilot_rootColor (box redA))))
check "public singleton reuses native value" (System.Object.ReferenceEquals(AdtPilot_singleton, box nativeOne))
let mutable skewed = AdtPilot_E_adt_native
for index in 0 .. 999 do skewed <- AdtPilot_T_adt_native AdtPilot_R_adt_native skewed index AdtPilot_E_adt_native
check "native recursion at depth 1000" (AdtPilot_depth_adt_native skewed = 1000)
check "public recursion at depth 1000" (publicDepth skewed = 1000)
check "native recursive construction keeps Int" (publicRoot skewed = 999)
let treeCases = FSharpType.GetUnionCases(typeof<AdtPilot_Tree>)
let fields = (treeCases |> Array.find (fun item -> item.Name = "AdtPilot_Tusd_Ctor")).GetFields()
check "Tree has exactly two constructors" (treeCases.Length = 2)
check "T has exactly four native fields" (fields.Length = 4)
check "native color field" (fields.[0].PropertyType = typeof<AdtPilot_Color>)
check "native left field" (fields.[1].PropertyType = typeof<AdtPilot_Tree>)
check "native Int field" (fields.[2].PropertyType = typeof<int>)
check "native right field" (fields.[3].PropertyType = typeof<AdtPilot_Tree>)
printfn "adt-kernel runtime: %d checks passed" checks
`;
  assert.ok(runtime.trim().length > 0 && generated.trim().length > 0, "FSI source and assertions are nonempty");
  const fsx = join(directory, "adt-kernel.fsx");
  await writeFile(fsx, `${generated}\n\n${runtime}`);
  const fsi = command(process.env.DOTNET || "dotnet", ["fsi", "--nologo", "--optimize+", "--exec", fsx], directory);
  assert.doesNotMatch(fsi.stderr, /warning FS\d+/, "generated F# compiles without warnings");
  assert.match(fsi.stdout, /adt-kernel runtime: 32 checks passed/, "FSI executed all runtime assertions");
  console.log(fsi.stdout.trim());
  console.log(`adt-kernel converter: ${checks} checks passed`);
} finally {
  process.chdir(previousCwd);
  await rm(directory, { recursive: true, force: true });
}
