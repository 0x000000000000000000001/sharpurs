// Run after npm run build. PURS must select the TAST compiler fork.
import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { mkdir, readFile, writeFile } from "node:fs/promises";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { clone, sourceBinding, sourceBindings, optimizedBinding, optimizedTyped,
  dataDeclaration, expectNode } from "./support/ast.mjs";
import { fsharpFixture } from "./support/fsharp.mjs";
import { command, compileFixtures, packageSource, runFsharp, withFixtureDirectory } from "./support/fixtures.mjs";
import { optimizeCoreFn } from "./support/corefn.mjs";
import { helpers as preludeFs } from "../output/Sharpurs.Runtime/index.js";
import * as C from "../output/PureScript.Backend.Optimizer.CoreFn/index.js";
import { Just, Nothing } from "../output/Data.Maybe/index.js";
import * as Map from "../output/Data.Map.Internal/index.js";
import * as Set from "../output/Data.Set/index.js";
import * as Ord from "../output/Data.Ord/index.js";
import { prepareModule } from "../output/Sharpurs.AdtKernel/index.js";
import * as S from "../output/PureScript.Backend.Optimizer.Syntax/index.js";
import { translateModule, translateModuleWithConstructorWrappers, translateOptimizedModuleWithAdts } from "../output/Sharpurs.CodeGen/index.js";
import { printModule } from "../output/Sharpurs.Printer/index.js";

const backend = resolve(dirname(fileURLToPath(import.meta.url)), "..");
const artifacts = process.env.ADT_INTEROP_ARTIFACTS && resolve(process.env.ADT_INTEROP_ARTIFACTS);
const transcript = [];

await withFixtureDirectory("sharpurs-adt-unary-", { artifacts, transcript }, async directory => {
  const prelude = await packageSource(backend, "prelude");
  const fixtures = ["AdtUnary", "AdtConsumer"];
  for (const name of fixtures) await writeFile(join(directory, `${name}.purs`),
    (await readFile(join(backend, "tests/fixtures", `${name}.purs`), "utf8")).replaceAll("AdtPilot", "AdtUnary"));
  await writeFile(join(directory, "AdtConsumer.js"),
    ["trackColor", "trackTree", "trackInt"].map((name) => `export const ${name} = _ => value => value;`).join("\n"));
  compileFixtures(directory, [...fixtures.map((name) => join(directory, `${name}.purs`)), join(prelude, "**/*.purs")], { transcript });
  const captured = await optimizeCoreFn(directory, fixtures);
  assert.equal(captured.size, 2, "real compiler and PBO produced both modules");
  const producer = captured.get("AdtUnary");
  const selected = prepareModule(producer.core)(producer.backend);
  assert.ok(selected instanceof Just, "mixed producer has a closed native layout");
  assert.deepEqual([...selected.value0.nativeNames].sort(), ["depth", "leftChild", "rootColor", "rootValue"],
    "only unary functions on a recursive ADT are selected");
  let constructors = Map.empty;
  let wrappers = Set.empty;
  for (const { core: source } of captured.values()) for (const decl of source.dataDecls) {
    for (const ctor of decl.constructors) {
      const name = `${source.name.replaceAll(".", "_")}_${ctor.name}`;
      constructors = Map.insert(Ord.ordString)(name)(ctor.fields.length)(constructors);
      if (source.name === "AdtUnary") wrappers = Set.insert(Ord.ordString)(name)(wrappers);
    }
  }
  let converterChecks = 2;
  const consumer = captured.get("AdtConsumer").core;
  const producerFs = printModule(translateOptimizedModuleWithAdts(wrappers)(selected)(constructors)(producer.backend)(producer.core));
  const consumerFs = printModule(translateModuleWithConstructorWrappers(wrappers)(constructors)(consumer));
  assert.match(producerFs, /let AdtUnary_depth_tco .*: obj/);
  assert.doesNotMatch(producerFs, /AdtUnary_singletonWith_adt_native|AdtUnary_isRed_adt_native/);
  converterChecks += 2;
  const reject = (label, edit) => {
    const state = clone(producer); edit(state);
    assert.notDeepEqual(state, producer, `${label}: mutation changed its target`);
    assert.ok(prepareModule(state.core)(state.backend) instanceof Nothing, label);
    converterChecks++;
  };
  reject("polymorphic layout", s => { dataDeclaration(s.core, "Tree").vars = ["a"]; s.backend.dataDecls = clone(s.core.dataDecls); });
  reject("missing constructor wrapper", s => {
    optimizedBinding(s.backend, "T");
    s.backend.bindings = s.backend.bindings.map(g => ({...g, bindings:g.bindings.filter(b => b.value0 !== "T")}));
  });
  reject("native name collides with a retained binding", s => {
    const original = sourceBindings(s.core).find(b => b.group instanceof C.NonRec);
    assert.ok(original, "fixture contains a retained nonrecursive binding");
    const b = clone(original.node); b.value1 = "depth_adt_native";
    s.core.decls.push(new C.NonRec(b));
  });
  reject("foreign declarations remain conservative", s => { s.backend.foreign = Map.singleton("unknown")(new Just(C.Int.value)); });
  const retain = (label, edit) => {
    const state = clone(producer); edit(state);
    assert.notDeepEqual(state, producer, `${label}: mutation changed its target`);
    const candidate = prepareModule(state.core)(state.backend);
    assert.ok(candidate instanceof Just && !candidate.value0.nativeNames.includes("depth") && candidate.value0.nativeNames.includes("rootValue"),label);
    converterChecks++;
  };
  const depth = s => optimizedTyped(optimizedBinding(s.backend, "depth").expression);
  retain("unsupported TypeApp retains only that function", s => { const typed = depth(s); typed.expression = new S.TypeApp(typed.expression, C.Int.value); });
  retain("contradictory signature retains only that function", s => { depth(s).type = new C.Func([C.Int.value], C.Int.value); });
  retain("mutual source recursion is not partially replaced", s => {
    const binding = sourceBinding(s.core, "depth");
    const group = expectNode(binding.group, C.Rec, "depth source recursion");
    assert.equal(group.value0.length, 1, "depth is initially self-recursive");
    const extra = clone(binding.node); extra.value1 = "otherDepth"; group.value0.push(extra);
  });
  const header = `let (|LitInt|_|) (expected: int) (value: obj) = if value :? int && unbox value = expected then Some() else None\n`;
  const foreign = await fsharpFixture("adt-interop/Foreign.fs"); // Same AdtConsumer FFI.
  const runtime = await fsharpFixture("adt-unary/Runtime.fs");
  const fsx = join(directory, "adt-unary.fsx");
  const source = [preludeFs, header, producerFs, foreign, consumerFs, runtime].join("\n\n");
  await writeFile(fsx, source);
  if (artifacts) {
    await mkdir(artifacts, { recursive: true });
    await writeFile(join(artifacts, "AdtUnary.fs"), producerFs);
    await writeFile(join(artifacts, "AdtConsumer.fs"), consumerFs);
    await writeFile(join(artifacts, "adt-unary.fsx"), source);
  }
  const result = runFsharp(directory, fsx, { optimize: true, transcript });
  if (artifacts) {
    await writeFile(join(artifacts, "fsi.log"), result.stdout + result.stderr);
    const hash = (bytes) => createHash("sha256").update(bytes).digest("hex");
    const sourceFiles = ["src/Sharpurs/AdtKernel.purs", "src/Sharpurs/AdtLayout.purs", "src/Sharpurs/CodeGen.purs",
      ...["Analysis", "Lower", "Emit"].map(name => `src/Sharpurs/AdtKernel/${name}.purs`),
      "tests/adt-unary.mjs", "tests/support/ast.mjs", "tests/support/fsharp.mjs",
      "tests/fixtures/adt-unary/Runtime.fs", "tests/fixtures/adt-interop/Foreign.fs",
      ...fixtures.map((name) => `tests/fixtures/${name}.purs`)];
    const hashes = {};
    for (const file of sourceFiles) hashes[file] = hash(await readFile(join(backend, file)));
    await writeFile(join(artifacts, "metadata.json"), JSON.stringify({
      sourceHashes: hashes,
      generatedHashes: { producer: hash(producerFs), consumer: hash(consumerFs), full: hash(source) },
      purs: command(process.env.PURS || "purs", ["--version"], directory, { transcript }).stdout.trim(),
      dotnet: command(process.env.DOTNET || "dotnet", ["--version"], directory, { transcript }).stdout.trim(),
      exitCode: result.status,
    }, null, 2) + "\n");
  }
  assert.doesNotMatch(result.stderr, /warning FS\d+/, "interop compiles without warnings");
  assert.match(result.stdout, /adt-unary runtime: 63 checks passed/, "all runtime assertions ran");
  console.log(result.stdout.trim());
  console.log(`adt-unary converter: ${converterChecks} checks passed`);
});
