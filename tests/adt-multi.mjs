// Run after npm run build with the typed compiler fork selected by PURS.
import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { mkdtemp, mkdir, readFile, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { compileFixtures, copyFixtures, packageSource, runFsharp } from "./support/fixtures.mjs";
import { optimizeCoreFn } from "./support/corefn.mjs";
import { clone, annotation, sourceBinding, sourceLambdas, optimizedBinding, optimizedTyped,
  optimizedLambda, dataDeclaration, firstNode, findNodes, expectNode } from "./support/ast.mjs";
import { fsharpFixture } from "./support/fsharp.mjs";
import { helpers as preludeFs } from "../output/Sharpurs.Runtime/index.js";
import * as C from "../output/PureScript.Backend.Optimizer.CoreFn/index.js";
import * as S from "../output/PureScript.Backend.Optimizer.Syntax/index.js";
import { Just, Nothing } from "../output/Data.Maybe/index.js";
import { Tuple } from "../output/Data.Tuple/index.js";
import * as Map from "../output/Data.Map.Internal/index.js";
import * as Set from "../output/Data.Set/index.js";
import * as Ord from "../output/Data.Ord/index.js";
import { prepareModule } from "../output/Sharpurs.AdtKernel/index.js";
import { translateModule, translateModuleWithConstructorWrappers, translateOptimizedModuleWithAdts } from "../output/Sharpurs.CodeGen/index.js";
import { printModule } from "../output/Sharpurs.Printer/index.js";

const backend = resolve(dirname(fileURLToPath(import.meta.url)), "..");
const artifacts = process.env.ADT_MULTI_ARTIFACTS && resolve(process.env.ADT_MULTI_ARTIFACTS);
const transcript = [];
function expression(state, name) {
  return optimizedBinding(state.backend, name).expression;
}
function assembly(state) {
  return sourceLambdas(sourceBinding(state.core, "assemble").expression, 4, "assemble");
}

const directory = await mkdtemp(join(tmpdir(), "sharpurs-adt-multi-"));
try {
  const prelude = await packageSource(backend, "prelude");
  const partial = await packageSource(backend, "partial");
  const fixtureFiles = ["AdtMulti.purs", "AdtMultiConsumer.purs", "AdtMultiConsumer.js", "AdtMultiExternal.purs"];
  await copyFixtures(join(backend, "tests/fixtures/adt-multi"), directory, fixtureFiles, { javascript: false });
  const compiled = compileFixtures(directory, [join(directory, "*.purs"), join(prelude, "**/*.purs"), join(partial, "**/*.purs")], { transcript });
  assert.doesNotMatch(compiled.stdout + compiled.stderr, /Warning \d+ of/, "fixture has no PureScript warnings");
  const captured = await optimizeCoreFn(directory, ["AdtMulti", "AdtMultiConsumer", "AdtMultiExternal"]);
  assert.equal(captured.size, 3, "real fork compiler and PBO prepared producer, consumer and external dependency");
  const producer = captured.get("AdtMulti");
  const selected = prepareModule(producer.core)(producer.backend);
  assert.ok(selected instanceof Just, "closed ADT module has native candidates");
  let checks = 0;
  const yes = (condition, label) => { assert.ok(condition, label); checks++; };
  const chosen = new globalThis.Set(selected.value0.nativeNames);
  for (const name of ["assemble", "insert", "choose", "shift", "depth", "unused", "nested", "score", "shortAndSafe", "shortOrSafe", "argumentNative"]) {
    yes(chosen.has(name), `${name}: typed closed native body selected`);
  }
  for (const name of ["returned", "partial", "generic", "booleanOnly", "readNonEmpty", "bodyFailure", "argumentFailure", "combine", "argumentOrder",
    "shortAnd", "shortOr", "readNonEmptyInline", "bodyFailureInline", "importedFailure", "importedTotal", "throughImported"]) {
    yes(!chosen.has(name), `${name}: source application boundary or unsupported signature retained`);
  }
  yes(findNodes(expression(producer, "importedFailure"), node => node instanceof S.Fail).length > 0,
    "real optimizer inlined imported partial helper into a failure node");
  yes(findNodes(expression(producer, "importedFailure"), node => {
    if (!(node instanceof S.Var)) return false;
    const qualified = expectNode(node.value0, C.Qualified, "optimized imported variable");
    return qualified.value0 instanceof Just && qualified.value0.value0 === "AdtMultiExternal";
  }).length === 0, "imported failure proof survives disappearance of its optimized reference");
  let constructors = Map.empty;
  let wrappers = Set.empty;
  const ctorNames = producer.core.dataDecls.flatMap(decl => decl.constructors.map(ctor => ctor.name));
  for (const { core } of captured.values()) for (const decl of core.dataDecls) for (const ctor of decl.constructors) {
    const name = `${core.name.replaceAll(".", "_")}_${ctor.name}`;
    constructors = Map.insert(Ord.ordString)(name)(ctor.fields.length)(constructors);
    if (core.name === "AdtMulti") wrappers = Set.insert(Ord.ordString)(name)(wrappers);
  }
  const generate = candidate => printModule(translateOptimizedModuleWithAdts(wrappers)(candidate)
    (constructors)(producer.backend)(producer.core));
  const generated = generate(selected);
  // Keep exactly the same closed layout and constructor bridges. Only native
  // function replacement is disabled, so the oracle uses real generic bodies.
  const oracleSelection = new Just({ ...selected.value0,
    nativeNames: [], bindings: Map.filterKeys(Ord.ordString)(name => ctorNames.includes(name))(selected.value0.bindings) });
  const oracle = generate(oracleSelection);
  // Fault injection stays in isolated copies of generated F#, after all real
  // source-oracle generation. Both raw implementations throw the same sentinel;
  // every caller, argument expression and invocation guard is left unchanged.
  const inject = (source, entry) => {
    let replacements = 0;
    const result = source.replace(new RegExp(`^(let ${entry} .+? = ).+$`, "m"), (_, header) => {
      replacements++; return `${header}events.Add(77); raise injectedFailure`;
    });
    yes(replacements === 1, `${entry}: exactly one raw body fault injection`);
    return result;
  };
  const injectedNative = inject(generated, "AdtMulti_score_adt_native");
  const injectedOracle = inject(oracle, "AdtMulti_score_direct");
  const consumer = printModule(translateModuleWithConstructorWrappers(wrappers)(constructors)(captured.get("AdtMultiConsumer").core));
  const external = printModule(translateModule(constructors)(captured.get("AdtMultiExternal").core));
  yes(/let AdtMulti_assemble_adt_native .*: AdtMulti_Tree/.test(generated), "four-argument native ADT result");
  yes(/let rec AdtMulti_insert_adt_native .*: AdtMulti_Tree/.test(generated), "two-argument recursive native result");
  yes(/let AdtMulti_insert_tco .*: obj/.test(generated), "generic callers retain recursive object bridge");
  yes(/AdtMulti_assemble_adt_native_apply/.test(generated), "typed cross-call exception boundary present");
  yes(/let AdtMulti_shortAndSafe_adt_native .* && /.test(generated), "source conditional lowered to native short-circuit And");
  yes(/let AdtMulti_shortOrSafe_adt_native .* \|\| /.test(generated), `source conditional lowered to native short-circuit Or: ${generated.split("\n").find(line => line.startsWith("let AdtMulti_shortOrSafe_adt_native "))}`);
  yes(!/let (?:rec )?AdtMulti_(?:assemble|insert|choose|shift)_adt_native /.test(oracle),
    "oracle has no native function replacements");
  yes(/let rec AdtMulti_insert_tco/.test(oracle) && /sharpurs_apply/.test(oracle),
    "oracle retains generated generic recursion and dispatch");
  const rejectModule = (label, edit) => {
    const state = clone(producer); edit(state);
    assert.notDeepEqual(state, producer, `${label}: mutation changed its target`);
    yes(prepareModule(state.core)(state.backend) instanceof Nothing, label);
  };
  const rejectFunction = (name, label, edit) => {
    const state = clone(producer); edit(state);
    assert.notDeepEqual(state, producer, `${label}: mutation changed its target`);
    const candidate = prepareModule(state.core)(state.backend);
    yes(candidate instanceof Just && !candidate.value0.nativeNames.includes(name), label);
  };
  rejectModule("polymorphic layout rejected", state => {
    dataDeclaration(state.core, "Tree").vars = ["a"];
    state.backend.dataDecls = clone(state.core.dataDecls);
  });
  rejectModule("missing constructor wrapper rejected", state => {
    optimizedBinding(state.backend, "Branch");
    state.backend.bindings = state.backend.bindings.map(group => ({ ...group,
      bindings: group.bindings.filter(binding => binding.value0 !== "Branch") }));
  });
  rejectModule("native apply helper collision rejected", state => {
    const binding = clone(sourceBinding(state.core, "booleanOnly").node); binding.value1 = "assemble_adt_native_apply";
    state.core.decls.push(new C.NonRec(binding));
  });
  rejectModule("foreign module retains generic code", state => { state.backend.foreign = Map.singleton("unknown")(new Just(C.Int.value)); });
  rejectFunction("assemble", "missing source type rejects function", state => { annotation(assembly(state).lambdas[0].node).type = Nothing.value; });
  rejectFunction("assemble", "missing inner lambda type rejects function", state => { annotation(assembly(state).lambdas[1].node).type = Nothing.value; });
  rejectFunction("assemble", "contradictory inner lambda type rejects function", state => { annotation(assembly(state).lambdas[1].node).type = new Just(C.Int.value); });
  rejectFunction("assemble", "missing source body type rejects function", state => {
    annotation(assembly(state).body).type = Nothing.value;
  });
  rejectFunction("assemble", "ForAll signature retains generic function", state => {
    const typed = optimizedTyped(expression(state, "assemble"));
    const signature = new C.ForAll(["a"], typed.type);
    typed.type = signature;
    annotation(assembly(state).lambdas[0].node).type = new Just(signature);
  });
  rejectFunction("assemble", "constrained signature retains generic dictionary ABI", state => {
    const typed = optimizedTyped(expression(state, "assemble"));
    const signature = new C.ConstrainedType([new Tuple(["Data", "Eq", "Eq"], [C.Int.value])], typed.type);
    typed.type = signature;
    annotation(assembly(state).lambdas[0].node).type = new Just(signature);
  });
  rejectFunction("assemble", "contradictory optimized signature rejected", state => { optimizedTyped(expression(state, "assemble")).type = C.Int.value; });
  rejectFunction("assemble", "unsupported TypeApp retains function", state => {
    const typed = optimizedTyped(expression(state, "assemble"));
    typed.expression = new S.TypeApp(typed.expression, C.Int.value);
  });
  rejectFunction("assemble", "missing optimized annotation rejected", state => {
    const binding = optimizedBinding(state.backend, "assemble");
    binding.expression = optimizedTyped(binding.expression).expression;
  });
  rejectFunction("assemble", "source lambda arity contradiction rejected", state => {
    const fn = assembly(state);
    fn.lambdas[0].body = fn.lambdas[1].body;
  });
  rejectFunction("insert", "mutual source recursion retained", state => {
    const binding = sourceBinding(state.core, "insert");
    const group = expectNode(binding.group, C.Rec, "insert source recursion");
    assert.equal(group.value0.length, 1, "insert is initially self-recursive");
    const extra = clone(binding.node); extra.value1 = "otherInsert"; group.value0.push(extra);
  });
  rejectFunction("assemble", "duplicate optimized parameter levels rejected", state => {
    const outer = optimizedLambda(firstNode(expression(state, "assemble"), S.Abs, "assemble outer lambda"));
    const inner = optimizedLambda(firstNode(outer.body, S.Abs, "assemble inner lambda"));
    assert.notEqual(inner.parameters[0].level, outer.parameters[0].level, "initial argument levels are distinct");
    inner.parameters[0].level = outer.parameters[0].level;
    yes(inner.parameters[0].level === outer.parameters[0].level, "real optimized nested argument lambdas found");
  });
  rejectFunction("shortAndSafe", "Boolean primitive rejects contradictory operand annotation", state => {
    const node = firstNode(expression(state, "shortAndSafe"), S.Op2, "shortAndSafe Boolean And",
      node => node.value0 instanceof S.OpBooleanAnd);
    node.value1 = new S.Typed(C.Int.value, node.value1);
    yes(node.value1 instanceof S.Typed, "real optimized Boolean And found");
  });
  const failedDependency = clone(producer);
  annotation(assembly(failedDependency).lambdas[0].node).type = Nothing.value;
  const reduced = prepareModule(failedDependency.core)(failedDependency.backend);
  yes(reduced instanceof Just && !reduced.value0.nativeNames.includes("insert") && reduced.value0.nativeNames.includes("depth"),
    "unsupported native dependency keeps dependent function generic without dropping independent functions");

  const support = await fsharpFixture("adt-multi/Support.fs");
  const scoped = (name, body) => `module ${name} =\n${body.split("\n").map(line => `    ${line}`).join("\n")}\n`;
  const runtime = await fsharpFixture("adt-multi/Runtime.fs");
  const script = [preludeFs, support, external, scoped("Native", generated + "\n\n" + consumer),
    scoped("Oracle", oracle + "\n\n" + consumer), scoped("InjectedNative", injectedNative), scoped("InjectedOracle", injectedOracle), runtime].join("\n\n");
  await writeFile(join(directory, "adt-multi.fsx"), script);
  if (artifacts) {
    await mkdir(artifacts, { recursive: true });
    for (const [file, value] of [["AdtMulti.fs", generated], ["Oracle.fs", oracle], ["AdtMultiConsumer.fs", consumer],
      ["AdtMultiExternal.fs", external],
      ["InjectedNative.fs", injectedNative], ["InjectedOracle.fs", injectedOracle],
      ["adt-multi.fsx", script], ["selection.json", JSON.stringify([...chosen], null, 2)]]) await writeFile(join(artifacts, file), value);
    for (const file of fixtureFiles) await writeFile(join(artifacts, file), await readFile(join(directory, file)));
    await writeFile(join(artifacts, "corefn.json"), await readFile(join(directory, "output/AdtMulti/corefn.json")));
    await writeFile(join(artifacts, "AdtMultiExternal.corefn.json"), await readFile(join(directory, "output/AdtMultiExternal/corefn.json")));
  }
  const result = runFsharp(directory, "adt-multi.fsx", { optimize: true, transcript });
  // Existing generic object recursion and deliberately partial source matches
  // emit FS0040/FS0025. Native replacements must not add warnings.
  const scriptLines = script.split("\n");
  for (const warning of result.stderr.matchAll(/\((\d+),\d+\): warning (FS\d+):/g)) {
    yes(["FS0025", "FS0040"].includes(warning[2]), "only known generic fixture warning codes");
    yes(!/^\s*(?:let(?: rec)?|and) AdtMulti_\w+_adt_native(?:_apply)?\b/.test(scriptLines[Number(warning[1]) - 1]),
      "warning comes from retained generic body or wrapper");
  }
  assert.match(result.stdout, /adt-multi runtime: \d+ checks passed/, "runtime assertion suite completed");
  console.log(result.stdout.trim());
  const summary = `adt-multi converter: ${checks} checks passed`;
  transcript.push(summary);
  console.log(summary);
  if (artifacts) {
    const hashes = {};
    for (const file of ["src/Sharpurs/AdtKernel.purs", "src/Sharpurs/AdtLayout.purs", "src/Sharpurs/CodeGen.purs", "tests/adt-multi.mjs",
      "tests/support/fixtures.mjs", "tests/support/corefn.mjs", "tests/support/ast.mjs", "tests/support/fsharp.mjs",
      "tests/fixtures/adt-multi/Support.fs", "tests/fixtures/adt-multi/Runtime.fs",
      ...["Analysis", "Lower", "Emit"].map(name => `src/Sharpurs/AdtKernel/${name}.purs`),
      ...fixtureFiles.map(file => `tests/fixtures/adt-multi/${file}`)]) hashes[file] = createHash("sha256").update(await readFile(join(backend, file))).digest("hex");
    await writeFile(join(artifacts, "metadata.json"), JSON.stringify({ sourceHashes: hashes, selected: [...chosen] }, null, 2) + "\n");
  }
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
