// Real fork-Purs TAST -> PBO -> F#, checked against generated JS and a generic
// F# oracle. Run after the backend build; no compiler rebuild occurs here.
import assert from "node:assert/strict";
import { mkdtemp, mkdir, readFile, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";
import { compileFixtures, copyFixtures, packageSource, runFsharp } from "./support/fixtures.mjs";
import { readCoreFn } from "./support/corefn.mjs";
import { clone, annotation, annotatedType, sourceBinding, sourceLambdas, sourceApplication,
  sourceTypeApp, sourceVariable, findNodes } from "./support/ast.mjs";
import { fsharpFixture } from "./support/fsharp.mjs";
import { helpers as preludeFs } from "../output/Sharpurs.Runtime/index.js";
import * as C from "../output/PureScript.Backend.Optimizer.CoreFn/index.js";
import { Just, Nothing } from "../output/Data.Maybe/index.js";
import * as Map from "../output/Data.Map.Internal/index.js";
import { fromExpr } from "../output/Sharpurs.IntArithmetic/index.js";
import { translateModule } from "../output/Sharpurs.CodeGen/index.js";
import { printModule } from "../output/Sharpurs.Printer/index.js";

const backend = resolve(dirname(fileURLToPath(import.meta.url)), "..");
const artifacts = process.env.INT_ARITHMETIC_ARTIFACTS && resolve(process.env.INT_ARITHMETIC_ARTIFACTS);
const transcript = [];
const selections = value => findNodes(value, node => node instanceof C.ExprApp && fromExpr(node) instanceof Just);
function parts(expr) {
  const call = sourceApplication(expr, 3, "arithmetic dictionary call");
  const [dictionary, left, right] = call.args;
  return { call, dictionary, left, right, head: call.head,
    instantiated: sourceTypeApp(call.head), dictionaryCall: call.nodes[0].node, partial: call.nodes[1].node };
}
const generate = core => printModule(translateModule(Map.empty)(core));
const directory = await mkdtemp(join(tmpdir(), "sharpurs-int-arithmetic-"));
try {
  const prelude = await packageSource(backend, "prelude");
  await copyFixtures(join(backend, "tests/fixtures/int-arithmetic"), directory, ["IntArithmetic.purs", "IntArithmetic.js"]);
  compileFixtures(directory, [join(directory, "IntArithmetic.purs"), join(prelude, "**/*.purs")], { transcript });
  const core = (await readCoreFn(directory)).get("IntArithmetic");
  assert.ok(core, "real fork-Purs fixture parsed by PBO's TAST reader");
  if (artifacts) {
    await mkdir(artifacts, { recursive: true });
    for (const name of ["IntArithmetic.purs", "IntArithmetic.js"]) {
      await writeFile(join(artifacts, name), await readFile(join(directory, name)));
    }
    for (const name of ["corefn.json", "index.js"]) {
      await writeFile(join(artifacts, name), await readFile(join(directory, "output/IntArithmetic", name)));
    }
  }
  const binding = name => sourceBinding(core, name).node;
  let checks = 0;
  const yes = (condition, label) => { assert.ok(condition, label); checks++; };
  for (const name of ["addInt", "subInt", "orderedAdd", "orderedSub",
    "capturedAdd", "capturedSub", "delayedAdd", "delayedSub", "firstFailureAdd", "secondFailureAdd",
    "firstFailureSub", "secondFailureSub"]) {
    assert.equal(selections(binding(name)).length, 1, `${name}: one native Int arithmetic site`); checks++;
  }
  for (const name of ["visibleAdd", "visibleSub", "annotatedAdd", "annotatedSub", "partialAdd", "partialSub", "genericAdd", "genericSub",
    "numberAdd", "numberSub"]) {
    assert.equal(selections(binding(name)).length, 0, `${name}: generic semantics retained`); checks++;
  }
  for (const name of ["addInt", "subInt"]) {
    yes(parts(selections(binding(name))[0]).head instanceof C.ExprTypeApp, `${name}: coherent TAST v3 TypeApp Int`);
  }
  // In this fork, visible @Int wraps the dictionary application and leaves the
  // original function annotation unavailable. It must retain generic dispatch.
  for (const name of ["visibleAdd", "visibleSub"]) {
    const fn = sourceLambdas(sourceBinding(core, name).expression, 2, name);
    const applied = sourceTypeApp(sourceApplication(fn.body, 2, name).head);
    yes(applied.node instanceof C.ExprTypeApp && applied.argument === C.Int.value,
      `${name}: visible TypeApp Int preserved by the real fork`);
    const dictionaryCall = sourceApplication(applied.expression, 1, `${name} dictionary application`);
    yes(annotation(dictionaryCall.head).type instanceof Nothing,
      `${name}: unavailable canonical head annotation explains fallback`);
  }
  for (const name of ["addInt", "subInt"]) {
    const original = selections(binding(name))[0];
    const reject = (label, edit) => {
      const expr = clone(original); edit(expr, parts(expr));
      assert.notDeepEqual(expr, original, `${name}: ${label}: mutation changed its target`);
      yes(fromExpr(expr) instanceof Nothing, `${name}: ${label}`);
    };
    reject("missing result type", e => { annotation(e).type = Nothing.value; });
    reject("wrong result type", e => { annotation(e).type = new Just(C.Boolean.value); });
    reject("missing operand type", (_, p) => { annotation(p.left).type = Nothing.value; });
    reject("wrong operand type", (_, p) => { annotation(p.right).type = new Just(C.Number.value); });
    reject("contradictory partial signature", (_, p) => { annotation(p.partial).type = new Just(C.Int.value); });
    reject("missing dictionary application type", (_, p) => { annotation(p.dictionaryCall).type = Nothing.value; });
    reject("contradictory dictionary application", (_, p) => { annotation(p.dictionaryCall).type = new Just(C.Int.value); });
    reject("local dictionary", (_, p) => { sourceVariable(p.dictionary).owner = Nothing.value; });
    reject("custom Int dictionary", (_, p) => { sourceVariable(p.dictionary).name = "customInt"; });
    reject("wrong dictionary owner", (_, p) => { sourceVariable(p.dictionary).owner = new Just("Other.Numeric"); });
    reject("missing dictionary type", (_, p) => { annotation(p.dictionary).type = Nothing.value; });
    reject("inconsistent dictionary path", (_, p) => { annotatedType(p.dictionary, C.ADT).value1 = ["Other", "Numeric"]; });
    reject("local function shadow", (_, p) => { sourceVariable(p.instantiated.expression).owner = Nothing.value; });
    reject("wrong function owner", (_, p) => { sourceVariable(p.instantiated.expression).owner = new Just("Other.Numeric"); });
    reject("unsupported numeric operation", (_, p) => { sourceVariable(p.instantiated.expression).name = "mul"; });
    reject("wrong TypeApp argument", (_, p) => { p.instantiated.argument = C.Number.value; });
    reject("contradictory instantiated signature", (_, p) => { annotation(p.head).type = new Just(C.Int.value); });
    reject("missing polymorphic signature", (_, p) => { annotation(p.instantiated.expression).type = Nothing.value; });
    reject("extra TypeApp", (_, p) => { p.call.head = new C.ExprTypeApp(annotation(p.head), p.head, C.Int.value); });
    yes(fromExpr(parts(original).partial) instanceof Nothing, `${name}: partial application rejected`);
    yes(fromExpr(new C.ExprApp(annotation(original), original, parts(original).right)) instanceof Nothing,
      `${name}: over-application rejected`);
    const concrete = clone(original);
    const concreteParts = parts(concrete);
    concreteParts.call.head = new C.ExprVar(annotation(concreteParts.head), sourceVariable(concreteParts.instantiated.expression).node);
    yes(fromExpr(concrete) instanceof Just, `${name}: concrete canonical signature without TypeApp`);
    const polymorphic = clone(original);
    const polymorphicParts = parts(polymorphic);
    polymorphicParts.call.head = polymorphicParts.instantiated.expression;
    yes(fromExpr(polymorphic) instanceof Just, `${name}: exact ForAll signature without TypeApp`);
  }
  const add = clone(selections(binding("addInt"))[0]);
  parts(add).call.nodes[0].argument = clone(parts(selections(binding("subInt"))[0]).dictionary);
  yes(fromExpr(add) instanceof Nothing, "add must not accept canonical Ring dictionary");
  const sub = clone(selections(binding("subInt"))[0]);
  parts(sub).call.nodes[0].argument = clone(parts(selections(binding("addInt"))[0]).dictionary);
  yes(fromExpr(sub) instanceof Nothing, "sub must not accept canonical Semiring dictionary");

  const generated = generate(core);
  const declarations = generated.split("\n\n");
  const byName = name => declarations.find(line => line.startsWith(`let IntArithmetic_${name}_direct `))
    ?? declarations.find(line => line.startsWith(`let IntArithmetic_${name} `));
  for (const name of ["addInt", "subInt", "capturedAdd", "capturedSub"]) {
    yes(!/sharpurs_apply/.test(byName(name)), `${name}: arithmetic bypasses generic dispatch`);
    yes(/unbox<int>/.test(byName(name)), `${name}: native Int operands`);
  }
  for (const name of ["genericAdd", "numberAdd", "partialAdd"]) {
    yes(/Data_Semiring_add/.test(byName(name)), `${name}: addition dictionary dispatch retained`);
  }
  for (const name of ["genericSub", "numberSub", "partialSub"]) {
    yes(/Data_Ring_sub/.test(byName(name)), `${name}: subtraction dictionary dispatch retained`);
  }
  // Hide only each arithmetic head's annotation, retaining all call/operand
  // annotations so the rest of CodeGen (including direct wrappers) is identical.
  const fallbackCore = clone(core, value => typeof value === "string" ? value.replaceAll("IntArithmetic", "ArithmeticFallback") : value);
  for (const expr of selections(fallbackCore)) annotation(parts(expr).head).type = Nothing.value;
  yes(selections(fallbackCore).length === 0, "generic oracle contains no recognized arithmetic");
  const fallback = generate(fallbackCore);
  const js = await import(pathToFileURL(join(directory, "output/IntArithmetic/index.js")));
  const values = [-2147483648, -2147483647, -65536, -1, 0, 1, 65536, 2147483646, 2147483647];
  const pairs = values.flatMap(x => values.map(y => [x, y]));
  let seed = 0x729b15;
  const random = () => (seed = (Math.imul(seed, 1664525) + 1013904223) | 0);
  for (let n = 0; n < 32; n++) pairs.push([random(), random()]);
  const cases = pairs.map(([x, y]) => {
    const sum = js.addInt(x)(y), difference = js.subInt(x)(y);
    assert.equal(sum, Number(BigInt.asIntN(32, BigInt(x) + BigInt(y)))); checks++;
    assert.equal(difference, Number(BigInt.asIntN(32, BigInt(x) - BigInt(y)))); checks++;
    return [x, y, sum, difference];
  });
  const fsCases = cases.map(row => `    (${row.join(", ")})`).join(";\n");
  const support = await fsharpFixture("int-arithmetic/Support.fs");
  const runtime = await fsharpFixture("int-arithmetic/Runtime.fs", { CASES: fsCases });
  const source = [preludeFs, support, generated, fallback, runtime].join("\n\n");
  await writeFile(join(directory, "arithmetic.fsx"), source);
  if (artifacts) {
    await mkdir(artifacts, { recursive: true });
    for (const [name, contents] of [["IntArithmetic.fs", generated], ["ArithmeticFallback.fs", fallback],
      ["arithmetic.fsx", source], ["oracle-cases.json", JSON.stringify(cases, null, 2) + "\n"]]) {
      await writeFile(join(artifacts, name), contents);
    }
  }
  const result = runFsharp(directory, "arithmetic.fsx", { optimize: true, transcript });
  assert.doesNotMatch(result.stderr, /warning FS\d+/, "arithmetic fixture compiles without warnings");
  console.log(result.stdout.trim());
  const summary = `int-arithmetic converter: ${checks} checks passed`;
  transcript.push(summary);
  console.log(summary);
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
