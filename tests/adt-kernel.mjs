// Run after npm run build, with the TAST fork on PATH or PURS=/path/to/purs.
// DOTNET=/path/to/dotnet and PRELUDE_SRC=/path/to/prelude/src are optional.
import assert from "node:assert/strict";
import { mkdtemp, mkdir, readdir, readFile, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { spawnSync } from "node:child_process";
import { clone, optimizedBinding, optimizedTyped, optimizedLambda, dataDeclaration,
  firstNode, findNodes, expectNode } from "./support/ast.mjs";
import { fsharpFixture } from "./support/fsharp.mjs";
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
const artifacts = process.env.ADT_KERNEL_ARTIFACTS && resolve(process.env.ADT_KERNEL_ARTIFACTS);
const transcript = [];
const pure = Applicative.pure(Aff.applicativeAff);
const runAff = (action) => new Promise((resolve, reject) => {
  Aff.runAff((result) => () => result instanceof Left ? reject(result.value0) : resolve(result.value0))(action)();
});
function command(program, args, cwd) {
  const result = spawnSync(program, args, { cwd, encoding: "utf8", timeout: 60_000 });
  transcript.push(`$ ${program} ${args.join(" ")}\n${result.stdout || ""}${result.stderr || ""}`);
  if (result.error) throw result.error;
  if (result.status !== 0) throw new Error(`${program} failed (${result.signal || result.status}):\n${result.stdout}\n${result.stderr}`);
  return result;
}

function binding(state, name) {
  return optimizedBinding(state.backend, name);
}
function changeFirst(state, name, type, change, predicate = () => true) {
  change(firstNode(binding(state, name).expression, type, `fixture ${name}`, predicate));
}
function changeLayout(state, change) {
  change(state.core.dataDecls);
  state.backend.dataDecls = clone(state.core.dataDecls);
}
function constructor(decls, type, name, arity) {
  const matches = dataDeclaration({ dataDecls: decls }, type).constructors.filter(ctor => ctor.name === name);
  assert.equal(matches.length, 1, `${type}.${name}: expected one constructor`);
  assert.equal(matches[0].fields.length, arity, `${type}.${name}: expected ${arity} fields`);
  return matches[0];
}
const treeFields = decls => constructor(decls, "Tree", "T", 4).fields;

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
  await runAff(Builder.buildModules(Aff.monadEffectAff)({
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
    assert.notDeepEqual(modified, captured, `${label}: mutation changed its target`);
    assert.ok(fromModule(modified.core)(modified.backend) instanceof Nothing, label);
    checks++;
  };
  reject("absent layout", (s) => changeLayout(s, (decls) => decls.splice(0)));
  reject("polymorphic data declaration", (s) => changeLayout(s, (decls) => { dataDeclaration({ dataDecls: decls }, "Tree").vars = ["a"]; }));
  reject("external ADT field", (s) => changeLayout(s, (decls) => {
    treeFields(decls)[1] = new C.ADT("Elsewhere.Tree", ["Elsewhere", "Tree"], []);
  }));
  reject("missing local ADT", (s) => changeLayout(s, (decls) => {
    treeFields(decls)[1] = new C.ADT("AdtPilot.Missing", ["AdtPilot", "Missing"], []);
  }));
  reject("incorrect qualified type path", (s) => changeLayout(s, (decls) => {
    expectNode(treeFields(decls)[1], C.ADT, "left Tree field").value1 = ["AdtPilot", "Color"];
  }));
  reject("polymorphic type application in layout", (s) => changeLayout(s, (decls) => {
    expectNode(treeFields(decls)[1], C.ADT, "left Tree field").value2 = [C.Int.value];
  }));
  reject("unsupported field type", (s) => changeLayout(s, (decls) => {
    treeFields(decls)[2] = C.String.value;
  }));
  reject("contradictory source and optimized layouts", (s) => { s.backend.dataDecls = []; });
  reject("duplicate type names", (s) => changeLayout(s, (decls) => decls.push(clone(dataDeclaration({ dataDecls: decls }, "Color")))));
  reject("duplicate constructor names", (s) => changeLayout(s, (decls) => { constructor(decls, "Color", "B", 0).name = "R"; }));
  reject("type name collides with emitted constructor", (s) => changeLayout(s, (decls) => {
    decls.push({ name: "Tusd_Ctor", vars: [], constructors: [{ name: "Unused", fields: [] }] });
  }));
  reject("sanitized public binding names collide", (s) => {
    const { group, node } = binding(s, "depth");
    const a = clone(node); a.value0 = "depth'";
    const b = clone(node); b.value0 = "depth_prime";
    group.bindings.push(a, b);
  });
  reject("module names must agree", (s) => { s.backend.name = "DifferentPilot"; });
  reject("stale binding annotation", (s) => {
    optimizedTyped(binding(s, "depth").expression).type = new C.Func([C.Int.value], C.Int.value);
  });
  reject("contradictory body annotation", (s) => changeFirst(s, "depth", S.Typed,
    (node) => { node.value0 = C.String.value; }, (node) => node.value0 instanceof C.Int));
  reject("unknown local level", (s) => changeFirst(s, "depth", S.Local, (node) => { node.value1 = 999; }));
  reject("duplicate parameter levels", (s) => {
    const parameters = findNodes(binding(s, "max").expression, node => node instanceof S.Abs)
      .flatMap(node => optimizedLambda(node).parameters);
    assert.ok(parameters.length > 1, "fixture max has two parameters to collide");
    for (const parameter of parameters.slice(1)) parameter.level = parameters[0].level;
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
    const typed = optimizedTyped(binding(s, "depth").expression);
    typed.expression = new S.TypeApp(typed.expression, C.Int.value);
  });
  reject("foreign binding requires the general backend", (s) => {
    s.backend.foreign = Map.singleton("foreignValue")(new Just(C.Int.value));
  });

  const runtime = await fsharpFixture("adt-kernel/Runtime.fs");
  assert.ok(runtime.trim().length > 0 && generated.trim().length > 0, "FSI source and assertions are nonempty");
  const fsx = join(directory, "adt-kernel.fsx");
  await writeFile(fsx, `${generated}\n\n${runtime}`);
  if (artifacts) {
    await mkdir(artifacts, { recursive: true });
    await writeFile(join(artifacts, "AdtPilot.fs"), generated);
    await writeFile(join(artifacts, "adt-kernel.fsx"), await readFile(fsx));
  }
  const fsi = command(process.env.DOTNET || "dotnet", ["fsi", "--nologo", "--optimize+", "--exec", fsx], directory);
  assert.doesNotMatch(fsi.stderr, /warning FS\d+/, "generated F# compiles without warnings");
  assert.match(fsi.stdout, /adt-kernel runtime: 32 checks passed/, "FSI executed all runtime assertions");
  console.log(fsi.stdout.trim());
  console.log(`adt-kernel converter: ${checks} checks passed`);
} catch (error) {
  transcript.push(error.stack || String(error));
  throw error;
} finally {
  process.chdir(previousCwd);
  if (artifacts) {
    await mkdir(artifacts, { recursive: true });
    await writeFile(join(artifacts, "validation.log"), transcript.join("\n") + "\n");
  }
  await rm(directory, { recursive: true, force: true });
}
