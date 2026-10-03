// Run after npm run build. PURS must select the compiler with TAST annotations.
import assert from "node:assert/strict";
import { mkdtemp, mkdir, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { compileFixtures, copyFixtures, packageSource, runFsharp } from "./support/fixtures.mjs";
import { readCoreFn } from "./support/corefn.mjs";
import { clone, annotation, sourceBinding, sourceBindings, sourceLambdas, sourceLambda,
  sourceApplication, sourceVariable, findNodes, expectNode } from "./support/ast.mjs";
import { fsharpFixture } from "./support/fsharp.mjs";
import { helpers as preludeFs } from "../output/Sharpurs.Runtime/index.js";
import * as C from "../output/PureScript.Backend.Optimizer.CoreFn/index.js";
import { Just, Nothing } from "../output/Data.Maybe/index.js";
import * as Map from "../output/Data.Map.Internal/index.js";
import * as Ord from "../output/Data.Ord/index.js";
import { fromBinding, fromCall } from "../output/Sharpurs.DirectCall/index.js";
import { fromModule } from "../output/Sharpurs.AdtLayout/index.js";
import { translateModule } from "../output/Sharpurs.CodeGen/index.js";
import { printModule } from "../output/Sharpurs.Printer/index.js";

const backend = resolve(dirname(fileURLToPath(import.meta.url)), "..");
const artifacts = process.env.DIRECT_CALL_ARTIFACTS && resolve(process.env.DIRECT_CALL_ARTIFACTS);
const transcript = [];
function generate(core) {
  let constructors = Map.empty;
  for (const decl of core.dataDecls) for (const ctor of decl.constructors) {
    constructors = Map.insert(Ord.ordString)(`${core.name.replaceAll(".", "_")}_${ctor.name}`)(ctor.fields.length)(constructors);
  }
  return printModule(translateModule(constructors)(core));
}
const directory = await mkdtemp(join(tmpdir(), "sharpurs-direct-call-"));
try {
  const prelude = await packageSource(backend, "prelude");
  await copyFixtures(join(backend, "tests/fixtures/direct-call"), directory, ["DirectCall.purs", "DirectCall.js", "DirectRemote.purs"]);
  compileFixtures(directory, [join(directory, "*.purs"), join(prelude, "**/*.purs")], { transcript });
  const core = (await readCoreFn(directory)).get("DirectCall");
  assert.ok(core, "real compiler TAST parsed by PBO");
  const binding = name => expectNode(sourceBinding(core, name).group, C.NonRec, `fixture ${name}`);
  let checks = 0;
  const yes = (condition, label) => { assert.ok(condition, label); checks++; };
  for (const name of ["ordinal", "guarded", "captured", "imported", "failBody"]) {
    yes(fromBinding(Nothing.value)(binding(name)) instanceof Just, `${name}: supported saturated source lambdas`);
  }
  for (const name of ["unary", "generic", "stringPair", "numberPair", "higher", "shadowed", "localShadow",
    "returned", "partial1", "partial2", "partial3", "asValue", "orderedPartial"]) {
    yes(fromBinding(Nothing.value)(binding(name)) instanceof Nothing, `${name}: conservative fallback`);
  }
  const layout = fromModule(core);
  yes(layout instanceof Just, "TAST supplies closed local ADT layout");
  yes(fromBinding(layout)(binding("arrange")) instanceof Just, "closed accepted local ADT signature");
  yes(fromBinding(Nothing.value)(binding("arrange")) instanceof Nothing, "ADT needs an accepted native layout");
  const original = binding("ordinal");
  const selected = fromBinding(Nothing.value)(original).value0;
  assert.equal(selected.args.length, 4); checks++;
  yes(fromBinding(Nothing.value)(new C.Rec([sourceBinding(core, "ordinal").node])) instanceof Nothing, "recursive group retained");
  const rejectBinding = (label, edit) => {
    const copy = clone(original);
    const b = sourceBinding({ decls: [copy] }, "ordinal");
    const fn = sourceLambdas(b.expression, 4, "ordinal");
    edit(b, fn);
    assert.notDeepEqual(copy, original, `${label}: mutation changed its target`);
    yes(fromBinding(Nothing.value)(copy) instanceof Nothing, label);
  };
  rejectBinding("missing declaration annotation", b => { annotation(b.node).type = Nothing.value; });
  rejectBinding("contradictory declaration signature", b => { annotation(b.node).type = new Just(C.Int.value); });
  rejectBinding("missing first lambda annotation", (_, fn) => { annotation(fn.lambdas[0].node).type = Nothing.value; });
  rejectBinding("missing inner lambda annotation", (_, fn) => { annotation(fn.lambdas[1].node).type = Nothing.value; });
  rejectBinding("contradictory lambda suffix", (_, fn) => { annotation(fn.lambdas[1].node).type = new Just(C.Int.value); });
  rejectBinding("missing body annotation", (_, fn) => { annotation(fn.body).type = Nothing.value; });
  rejectBinding("contradictory body result", (_, fn) => { annotation(fn.body).type = new Just(C.Boolean.value); });
  rejectBinding("explicit polymorphic declaration", b => {
    const ann = annotation(b.node);
    ann.type = new Just(new C.ForAll(["a"], expectNode(ann.type, Just).value0));
  });
  rejectBinding("duplicate sanitized argument names", (_, fn) => {
    fn.lambdas[0].parameter = "arg'"; fn.lambdas[1].parameter = "arg_prime";
  });
  const registry = Map.singleton(new C.Qualified(new Just("DirectCall"), "ordinal"))(selected);
  const calls = value => findNodes(value, e => e instanceof C.ExprApp && fromCall(registry)(e) instanceof Just);
  for (const name of ["saturated", "ordered", "captured", "argumentCall"]) {
    assert.equal(calls(binding(name)).length, 1, `${name}: exactly saturated qualified call`); checks++;
  }
  for (const name of ["partial1", "partial2", "partial3", "orderedPartial", "shadowed", "localShadow", "imported"]) {
    assert.equal(calls(binding(name)).length, 0, `${name}: no direct ordinal dispatch`); checks++;
  }
  const call = calls(binding("saturated"))[0];
  const rejectCall = (label, edit) => {
    const copy = clone(call); edit(copy, sourceApplication(copy, 4, "ordinal call"));
    assert.notDeepEqual(copy, call, `${label}: mutation changed its target`);
    yes(fromCall(registry)(copy) instanceof Nothing, label);
  };
  rejectCall("missing call result annotation", e => { annotation(e).type = Nothing.value; });
  rejectCall("contradictory call result", e => { annotation(e).type = new Just(C.Boolean.value); });
  rejectCall("missing argument type", (_, p) => { annotation(p.args[0]).type = Nothing.value; });
  rejectCall("contradictory argument type", (_, p) => { annotation(p.args[2]).type = new Just(C.Number.value); });
  rejectCall("missing callee signature", (_, p) => { annotation(p.head).type = Nothing.value; });
  rejectCall("contradictory callee signature", (_, p) => { annotation(p.head).type = new Just(C.Int.value); });
  rejectCall("missing intermediate application type", (_, p) => { annotation(p.nodes[1].node).type = Nothing.value; });
  rejectCall("contradictory intermediate signature", (_, p) => { annotation(p.nodes[1].node).type = new Just(C.Int.value); });
  rejectCall("unqualified local shadow", (_, p) => { sourceVariable(p.head).owner = Nothing.value; });
  rejectCall("imported owner", (_, p) => { sourceVariable(p.head).owner = new Just("DirectRemote"); });
  rejectCall("different function", (_, p) => { sourceVariable(p.head).name = "unknown"; });
  rejectCall("explicit TypeApp boundary", (_, p) => {
    p.head = new C.ExprTypeApp(annotation(p.head), p.head, C.Int.value);
  });
  const applied = sourceApplication(call, 4);
  yes(fromCall(registry)(applied.nodes[3].fn) instanceof Nothing, "partial application rejected");
  yes(fromCall(registry)(new C.ExprApp(annotation(call), call, applied.args[0])) instanceof Nothing,
    "over-application rejected");

  const generated = generate(core);
  const declaration = name => generated.split("\n\n").find(line => line.startsWith(`let DirectCall_${name} `));
  yes(generated.includes("let DirectCall_ordinal_direct "), "generic named function gets direct implementation");
  yes(generated.includes("let DirectCall_ordinal_direct_apply "), "direct invocation preserves exception envelope");
  yes(/DirectCall_ordinal_direct_apply/.test(declaration("saturated")), "saturated call emitted directly");
  yes(!/DirectCall_ordinal_direct_apply/.test(declaration("shadowed")), "shadowed function dispatch remains dynamic");
  yes(!/DirectCall_ordinal_direct_apply/.test(declaration("localShadow")), "local function keeps its own body");
  yes(/sharpurs_apply/.test(declaration("partial3")), "partial application retains object ABI");
  yes(!generated.includes("let DirectCall_returned_direct "), "returned function does not cross a let boundary");
  yes(!generated.includes("let DirectCall_return_direct_apply "), "source binding blocks reserved-name helper collision");
  const parameterCollision = clone(core);
  sourceLambda(sourceBinding(parameterCollision, "ordinal").expression).parameter = "DirectCall_ordinal_direct";
  yes(!generate(parameterCollision).includes("let DirectCall_ordinal_direct "),
    "argument matching helper name prevents capture");
  // The same source with unavailable declaration types is a generic-dispatch
  // oracle, including the exact exception wrapping and partial-application ABI.
  const fallbackCore = clone(core, value => typeof value === "string" ? value.replaceAll("DirectCall", "DirectFallback") : value);
  for (const b of sourceBindings(fallbackCore)) if (b.group instanceof C.NonRec) {
    annotation(b.node).type = Nothing.value;
    if (b.expression instanceof C.ExprAbs) annotation(b.expression).type = Nothing.value;
  }
  const fallback = generate(fallbackCore);
  yes(!/let DirectFallback_\w+_direct \(/.test(fallback), "fallback oracle has no direct entry points");
  const support = await fsharpFixture("direct-call/Support.fs");
  const runtime = await fsharpFixture("direct-call/Runtime.fs");
  const source = [preludeFs, support, generated, fallback, runtime].join("\n\n");
  await writeFile(join(directory, "direct-call.fsx"), source);
  if (artifacts) {
    const destination = artifacts;
    await mkdir(destination, { recursive: true });
    await writeFile(join(destination, "DirectCall.fs"), generated);
    await writeFile(join(destination, "DirectFallback.fs"), fallback);
    await writeFile(join(destination, "direct-call.fsx"), source);
  }
  const result = runFsharp(directory, "direct-call.fsx", { optimize: true, transcript });
  assert.doesNotMatch(result.stderr, /warning FS\d+/, "direct-call fixtures compile without warnings");
  console.log(result.stdout.trim());
  console.log(`direct-call converter: ${checks} checks passed`);
} catch (error) {
  transcript.push(error.stack || String(error));
  throw error;
} finally {
  if (artifacts) {
    await mkdir(artifacts, { recursive: true });
    await writeFile(join(artifacts, "validation.log"), transcript.join("\n") + "\n");
  }
  await rm(directory, { recursive: true, force: true });
}
