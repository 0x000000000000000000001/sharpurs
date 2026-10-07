// Compile a real TAST fixture, check conservative selection, then execute F#.
import assert from "node:assert/strict";
import { mkdir, readFile, writeFile } from "node:fs/promises";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";
import { clone, annotation, annotatedType, sourceBinding, sourceApplication,
  sourceTypeApp, sourceVariable, findNodes } from "./support/ast.mjs";
import { fsharpFixture } from "./support/fsharp.mjs";
import { compileFixtures, packageSource, runFsharp, withFixtureDirectory } from "./support/fixtures.mjs";
import { readCoreFn } from "./support/corefn.mjs";
import { helpers as preludeFs } from "../output/Sharpurs.Runtime/index.js";
import * as C from "../output/PureScript.Backend.Optimizer.CoreFn/index.js";
import { Just, Nothing } from "../output/Data.Maybe/index.js";
import * as Map from "../output/Data.Map.Internal/index.js";
import { fromExpr } from "../output/Sharpurs.IntComparison/index.js";
import { translateModule } from "../output/Sharpurs.CodeGen/index.js";
import { printModule } from "../output/Sharpurs.Printer/index.js";

const backend = resolve(dirname(fileURLToPath(import.meta.url)), "..");
const artifacts = process.env.INT_COMPARISON_ARTIFACTS && resolve(process.env.INT_COMPARISON_ARTIFACTS);
const transcript = [];
const selections = value => findNodes(value, node => node instanceof C.ExprApp && fromExpr(node) instanceof Just);
await withFixtureDirectory("sharpurs-int-comparison-", { artifacts, transcript }, async directory => {
  const prelude = await packageSource(backend, "prelude");
  await writeFile(join(directory, "IntCompare.purs"), await readFile(join(backend, "tests/fixtures/IntCompare.purs")));
  await writeFile(join(directory, "IntCompare.js"), "export const track = _ => value => value;\n");
  await writeFile(join(directory, "package.json"), '{"type":"module"}\n');
  compileFixtures(directory, [join(directory, "IntCompare.purs"), join(prelude, "**/*.purs")], { transcript });
  const core = (await readCoreFn(directory)).get("IntCompare");
  assert.ok(core, "fixture parsed by PBO's TAST reader");
  const binding = name => sourceBinding(core, name).node;
  let checks = 0;
  for (const name of ["less", "greater", "ordered"]) {
    assert.equal(selections(binding(name)).length, 1, `${name}: one native comparison`); checks++;
  }
  for (const name of ["annotated", "partial", "genericLess", "stringLess", "numberLess", "custom"]) {
    assert.equal(selections(binding(name)).length, 0, `${name}: generic semantics retained`); checks++;
  }
  const original = selections(binding("less"))[0];
  const parts = expr => {
    const call = sourceApplication(expr, 3, "comparison dictionary call");
    const [dictionary, left, right] = call.args;
    return { call, dictionary, left, right, head: call.head,
      instantiated: sourceTypeApp(call.head), dictionaryCall: call.nodes[0].node, partial: call.nodes[1].node };
  };
  const reject = (label, edit) => {
    const expr = clone(original); edit(expr, parts(expr));
    assert.notDeepEqual(expr, original, `${label}: mutation changed its target`);
    assert.ok(fromExpr(expr) instanceof Nothing, label); checks++;
  };
  reject("missing result type", e => { annotation(e).type = Nothing.value; });
  reject("wrong result type", e => { annotation(e).type = new Just(C.Int.value); });
  reject("missing operand type", (_, p) => { annotation(p.left).type = Nothing.value; });
  reject("wrong operand type", (_, p) => { annotation(p.right).type = new Just(C.Number.value); });
  reject("contradictory partial signature", (_, p) => { annotation(p.partial).type = new Just(C.Int.value); });
  reject("contradictory dictionary application", (_, p) => { annotation(p.dictionaryCall).type = new Just(C.Int.value); });
  reject("local dictionary", (_, p) => { sourceVariable(p.dictionary).owner = Nothing.value; });
  reject("custom Int dictionary", (_, p) => { sourceVariable(p.dictionary).name = "customOrdInt"; });
  reject("wrong dictionary owner", (_, p) => { sourceVariable(p.dictionary).owner = new Just("Other.Ord"); });
  reject("missing dictionary type", (_, p) => { annotation(p.dictionary).type = Nothing.value; });
  reject("inconsistent dictionary path", (_, p) => { annotatedType(p.dictionary, C.ADT).value1 = ["Other", "Ord"]; });
  reject("local function shadows lessThan", (_, p) => { sourceVariable(p.instantiated.expression).owner = Nothing.value; });
  reject("wrong function owner", (_, p) => { sourceVariable(p.instantiated.expression).owner = new Just("Other.Ord"); });
  reject("wrong TypeApp argument", (_, p) => { p.instantiated.argument = C.String.value; });
  reject("contradictory instantiated signature", (_, p) => { annotation(p.head).type = new Just(C.Int.value); });
  reject("missing polymorphic signature", (_, p) => { annotation(p.instantiated.expression).type = Nothing.value; });
  reject("extra TypeApp", (_, p) => { p.call.head = new C.ExprTypeApp(annotation(p.head), p.head, C.Int.value); });
  assert.ok(fromExpr(parts(original).partial) instanceof Nothing, "partial application rejected"); checks++;
  assert.ok(fromExpr(new C.ExprApp(annotation(original), original, parts(original).right)) instanceof Nothing,
    "over-application rejected"); checks++;
  const generated = printModule(translateModule(Map.empty)(core));
  // The direct-call pass may move a monomorphic function's body behind its
  // public curried wrapper. Inspect that body when checking dictionary use.
  const declarations = generated.split("\n\n");
  const byName = name => declarations.find(line => line.startsWith(`let IntCompare_${name}_direct `))
    ?? declarations.find(line => line.startsWith(`let IntCompare_${name} `));
  for (const name of ["less", "greater"]) {
    assert.doesNotMatch(byName(name), /sharpurs_apply/); checks++;
  }
  assert.match(byName("genericLess"), /Data_Ord_lessThan/); checks++;
  assert.match(byName("custom"), /IntCompare_ordReverse/); checks++;
  const js = await import(pathToFileURL(join(directory, "output/IntCompare/index.js")));
  const values = [-2147483648, -2147483647, -1, 0, 1, 2147483646, 2147483647];
  const cases = values.flatMap(x => values.map(y => [x, y, js.less(x)(y), js.greater(x)(y), js.custom(x)(y)]));
  const fsCases = cases.map(row => `    (${row.join(", ")})`).join(";\n");
  // Instrumented object-ABI dependencies: fallback calls must still dispatch
  // through their supplied dictionary, including the reversed custom order.
  const support = await fsharpFixture("int-comparison/Support.fs");
  const runtime = await fsharpFixture("int-comparison/Runtime.fs", { CASES: fsCases });
  const source = [preludeFs, support, generated, runtime].join("\n\n");
  await writeFile(join(directory, "comparison.fsx"), source);
  if (artifacts) {
    await mkdir(artifacts, { recursive: true });
    await writeFile(join(artifacts, "IntCompare.fs"), generated);
    await writeFile(join(artifacts, "comparison.fsx"), source);
  }
  const result = runFsharp(directory, "comparison.fsx", { optimize: true, transcript });
  assert.doesNotMatch(result.stderr, /warning FS\d+/);
  console.log(result.stdout.trim());
  console.log(`int-comparison converter: ${checks} checks passed`);
});
