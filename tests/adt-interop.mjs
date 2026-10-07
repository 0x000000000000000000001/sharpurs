// Run after npm run build. PURS must select the TAST compiler fork.
import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { mkdir, readFile, writeFile } from "node:fs/promises";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { clone, optimizedBinding } from "./support/ast.mjs";
import { fsharpFixture } from "./support/fsharp.mjs";
import { command, compileFixtures, packageSource, runFsharp, withFixtureDirectory } from "./support/fixtures.mjs";
import { optimizeCoreFn } from "./support/corefn.mjs";
import { helpers as preludeFs } from "../output/Sharpurs.Runtime/index.js";
import * as C from "../output/PureScript.Backend.Optimizer.CoreFn/index.js";
import { Just, Nothing } from "../output/Data.Maybe/index.js";
import * as Map from "../output/Data.Map.Internal/index.js";
import * as Ord from "../output/Data.Ord/index.js";
import { prepareProducer, producerModule, translateConsumer } from "../output/Sharpurs.AdtInterop/index.js";
import { translateModule } from "../output/Sharpurs.CodeGen/index.js";
import { printModule } from "../output/Sharpurs.Printer/index.js";

const backend = resolve(dirname(fileURLToPath(import.meta.url)), "..");
const artifacts = process.env.ADT_INTEROP_ARTIFACTS && resolve(process.env.ADT_INTEROP_ARTIFACTS);
const transcript = [];
function renameModule(value, name) {
  return clone(value, renamed => {
    if (typeof renamed === "string") return renamed.replaceAll("AdtPilot", name);
    if (renamed instanceof C.ADT) renamed.value1 = renamed.value0.split(".");
    return renamed;
  });
}

await withFixtureDirectory("sharpurs-adt-interop-", { artifacts, transcript }, async directory => {
  const prelude = await packageSource(backend, "prelude");
  const fixtures = ["AdtPilot", "AdtConsumer"];
  for (const name of fixtures) await writeFile(join(directory, `${name}.purs`),
    await readFile(join(backend, "tests/fixtures", `${name}.purs`)));
  await writeFile(join(directory, "AdtConsumer.js"),
    ["trackColor", "trackTree", "trackInt"].map((name) => `export const ${name} = _ => value => value;`).join("\n"));
  compileFixtures(directory, [...fixtures.map((name) => join(directory, `${name}.purs`)), join(prelude, "**/*.purs")], { transcript });
  const captured = await optimizeCoreFn(directory, fixtures);
  assert.equal(captured.size, 2, "real compiler and PBO produced both modules");
  const producer = captured.get("AdtPilot");
  const accepted = prepareProducer(producer.core)(producer.backend);
  assert.ok(accepted instanceof Just, "producer uses native ADT layout");
  let constructors = Map.empty;
  for (const { core: source } of captured.values()) for (const decl of source.dataDecls) {
    for (const ctor of decl.constructors) {
      const name = `${source.name.replaceAll(".", "_")}_${ctor.name}`;
      constructors = Map.insert(Ord.ordString)(name)(ctor.fields.length)(constructors);
    }
  }
  let converterChecks = 0;
  const consumer = captured.get("AdtConsumer").core;
  const producerFs = printModule(producerModule(accepted.value0));
  const translated = translateConsumer([accepted.value0])(constructors)(consumer);
  assert.ok(translated instanceof Just, "validated producer interoperates with generic consumer");
  converterChecks++;
  const consumerFs = printModule(translated.value0);
  const legacy = translateConsumer([])(constructors)(consumer);
  assert.ok(legacy instanceof Just, "empty registry uses generic backend");
  assert.equal(printModule(legacy.value0), printModule(translateModule(constructors)(consumer)),
    "empty registry preserves existing generated code exactly");
  converterChecks += 2;
  const rejected = (label, result) => {
    assert.ok(result instanceof Nothing, label);
    converterChecks++;
  };
  rejected("missing constructor registry entry", translateConsumer([accepted.value0])(
    Map.delete(Ord.ordString)("AdtPilot_T")(constructors))(consumer));
  rejected("incorrect constructor arity", translateConsumer([accepted.value0])(
    Map.insert(Ord.ordString)("AdtPilot_T")(3)(constructors))(consumer));
  rejected("duplicate producer", translateConsumer([accepted.value0, accepted.value0])(constructors)(consumer));
  rejected("consumer cannot redeclare producer ownership", translateConsumer([accepted.value0])(constructors)(producer.core));
  const absentLayout = clone(producer);
  absentLayout.core.dataDecls = [];
  absentLayout.backend.dataDecls = [];
  rejected("producer must pass native emitter", prepareProducer(absentLayout.core)(absentLayout.backend));
  const absentWrapper = clone(producer);
  optimizedBinding(absentWrapper.backend, "T");
  absentWrapper.backend.bindings = absentWrapper.backend.bindings.map((group) => ({
    ...group, bindings: group.bindings.filter((item) => item.value0 !== "T"),
  })).filter((group) => group.bindings.length > 0);
  rejected("every producer constructor requires a public wrapper", prepareProducer(absentWrapper.core)(absentWrapper.backend));
  const renamed = renameModule(producer, "Adt.Pilot");
  const dottedProducer = prepareProducer(renamed.core)(renamed.backend);
  assert.ok(dottedProducer instanceof Just, "dotted module remains a valid native producer");
  converterChecks++;
  let dottedRegistry = constructors;
  for (const decl of renamed.core.dataDecls) for (const ctor of decl.constructors) {
    dottedRegistry = Map.insert(Ord.ordString)(`Adt_Pilot_${ctor.name}`)(ctor.fields.length)(dottedRegistry);
  }
  const colliding = clone(consumer);
  colliding.name = "Adt_Pilot";
  rejected("consumer namespace cannot collide after flattening", translateConsumer([dottedProducer.value0])(dottedRegistry)(colliding));
  const flat = renameModule(producer, "Adt_Pilot");
  const flatProducer = prepareProducer(flat.core)(flat.backend);
  assert.ok(flatProducer instanceof Just, "underscore module remains a valid native producer");
  converterChecks++;
  rejected("producer namespaces cannot collide after flattening",
    translateConsumer([dottedProducer.value0, flatProducer.value0])(dottedRegistry)(consumer));
  const header = `let (|LitInt|_|) (expected: int) (value: obj) = if value :? int && unbox value = expected then Some() else None\n`;
  const foreign = await fsharpFixture("adt-interop/Foreign.fs");
  const runtime = await fsharpFixture("adt-interop/Runtime.fs");
  const fsx = join(directory, "adt-interop.fsx");
  const source = [preludeFs, header, producerFs, foreign, consumerFs, runtime].join("\n\n");
  await writeFile(fsx, source);
  if (artifacts) {
    await mkdir(artifacts, { recursive: true });
    await writeFile(join(artifacts, "AdtPilot.fs"), producerFs);
    await writeFile(join(artifacts, "AdtConsumer.fs"), consumerFs);
    await writeFile(join(artifacts, "adt-interop.fsx"), source);
  }
  const result = runFsharp(directory, fsx, { optimize: true, transcript });
  if (artifacts) {
    await writeFile(join(artifacts, "fsi.log"), result.stdout + result.stderr);
    const hash = (bytes) => createHash("sha256").update(bytes).digest("hex");
    const sourceFiles = ["src/Sharpurs/AdtKernel.purs", "src/Sharpurs/AdtLayout.purs", "src/Sharpurs/AdtInterop.purs", "src/Sharpurs/CodeGen.purs",
      ...["Analysis", "Lower", "Emit"].map(name => `src/Sharpurs/AdtKernel/${name}.purs`),
      "tests/adt-interop.mjs", "tests/support/ast.mjs", "tests/support/fsharp.mjs",
      "tests/fixtures/adt-interop/Runtime.fs", "tests/fixtures/adt-interop/Foreign.fs",
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
  assert.match(result.stdout, /adt-interop runtime: 62 checks passed/, "all runtime assertions ran");
  console.log(result.stdout.trim());
  console.log(`adt-interop converter: ${converterChecks} checks passed`);
});
