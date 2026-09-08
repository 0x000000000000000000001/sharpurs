// Run after npm run build. PURS must select the TAST compiler fork.
import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { mkdtemp, mkdir, readdir, readFile, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { spawnSync } from "node:child_process";
import * as C from "../output/PureScript.Backend.Optimizer.CoreFn/index.js";
import * as Aff from "../output/Effect.Aff/index.js";
import * as Applicative from "../output/Control.Applicative/index.js";
import { Left } from "../output/Data.Either/index.js";
import { Just, Nothing } from "../output/Data.Maybe/index.js";
import * as Map from "../output/Data.Map.Internal/index.js";
import * as Set from "../output/Data.Set/index.js";
import * as Ord from "../output/Data.Ord/index.js";
import * as App from "../output/PureScript.Backend.Optimizer.App/index.js";
import * as Builder from "../output/PureScript.Backend.Optimizer.Builder/index.js";
import * as Foreign from "../output/PureScript.Backend.Optimizer.Semantics.Foreign/index.js";
import { prepareProducer, producerModule, translateConsumer } from "../output/Sharpurs.AdtInterop/index.js";
import { translateModule } from "../output/Sharpurs.CodeGen/index.js";
import { printModule } from "../output/Sharpurs.Printer/index.js";

const backend = resolve(dirname(fileURLToPath(import.meta.url)), "..");
const pure = Applicative.pure(Aff.applicativeAff);
const runAff = (action) => new Promise((resolve, reject) => {
  Aff.runAff((result) => () => result instanceof Left ? reject(result.value0) : resolve(result.value0))(action)();
});
function command(program, args, cwd) {
  const result = spawnSync(program, args, { cwd, encoding: "utf8", timeout: 60_000 });
  if (result.error) throw result.error;
  return result;
}
function successful(result, label) {
  if (result.status !== 0) throw new Error(`${label} failed (${result.signal || result.status}):\n${result.stdout}\n${result.stderr}`);
}
// Preserve the real compiler/optimizer ADT prototypes for targeted mutations.
function clone(value) {
  if (Array.isArray(value)) return value.map(clone);
  if (!value || typeof value !== "object") return value;
  return Object.assign(Object.create(Object.getPrototypeOf(value)), Object.fromEntries(
    Object.entries(value).map(([key, child]) => [key, clone(child)])));
}
function renameModule(value, name) {
  if (typeof value === "string") return value.replaceAll("AdtPilot", name);
  if (Array.isArray(value)) return value.map((child) => renameModule(child, name));
  if (!value || typeof value !== "object") return value;
  const renamed = Object.assign(Object.create(Object.getPrototypeOf(value)), Object.fromEntries(
    Object.entries(value).map(([key, child]) => [key, renameModule(child, name)])));
  if (renamed instanceof C.ADT) renamed.value1 = renamed.value0.split(".");
  return renamed;
}

const directory = await mkdtemp(join(tmpdir(), "sharpurs-adt-interop-"));
const previousCwd = process.cwd();
try {
  const prelude = process.env.PRELUDE_SRC || join(backend, ".spago/p",
    (await readdir(join(backend, ".spago/p"))).find((name) => /^prelude-/.test(name)) || "prelude-not-installed", "src");
  const fixtures = ["AdtPilot", "AdtConsumer"];
  for (const name of fixtures) await writeFile(join(directory, `${name}.purs`),
    await readFile(join(backend, "tests/fixtures", `${name}.purs`)));
  await writeFile(join(directory, "AdtConsumer.js"),
    ["trackColor", "trackTree", "trackInt"].map((name) => `export const ${name} = _ => value => value;`).join("\n"));
  successful(command(process.env.PURS || "purs", ["compile", ...fixtures.map((name) => join(directory, `${name}.purs`)),
    join(prelude, "**/*.purs"), "--output", join(directory, "output"), "--codegen", "corefn,js"], directory), "TAST fixture compilation");
  process.chdir(directory);
  const captured = new globalThis.Map();
  await runAff(Builder.buildModules(Aff.monadAff)({
    directives: await runAff(App.loadDirectives), rewriteLimit: 10000,
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
      if (fixtures.includes(module.name)) captured.set(module.name, { core, backend: module });
      return pure(undefined);
    },
  })(await runAff(App.coreFnModulesFromOutput(join(directory, "output")))));
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
  const main = await readFile(join(backend, "src/Main.purs"), "utf8");
  const preludeFs = main.match(/^fsPrelude = """\r?\n([\s\S]*?)^"""/m)?.[1];
  assert.ok(preludeFs, "actual runtime prelude extracted");
  const header = `let (|LitInt|_|) (expected: int) (value: obj) = if value :? int && unbox value = expected then Some() else None\n`;
  const foreign = `
let events = ResizeArray<int>()
let track : obj = box (fun (label: obj) -> box (fun (value: obj) -> events.Add(unbox<int> label); value))
let AdtConsumer_trackColor = track
let AdtConsumer_trackTree = track
let AdtConsumer_trackInt = track
`;
  const runtime = `
let mutable checks = 0
let check label ok = if not ok then failwith label else checks <- checks + 1
let apply = sharpurs_apply
let make fn color left value right = apply (apply (apply (apply fn color) (box left)) (box value)) (box right) |> unbox<AdtPilot_Tree>
let leaf value = AdtPilot_T_adt_native AdtPilot_R_adt_native AdtPilot_E_adt_native value AdtPilot_E_adt_native
let red = leaf 7
let made = make AdtConsumer_saturated AdtPilot_B red 11 red
check "saturated consumer native depth" (AdtPilot_depth_adt_native made = 2)
check "saturated consumer Int" (AdtPilot_rootValue_adt_native made = 11)
check "saturated consumer left identity" (System.Object.ReferenceEquals(AdtPilot_leftChild_adt_native made, red))
let partial = apply AdtConsumer_partial (box red)
let make11 = apply partial (box 11)
let first = apply make11 (box red) |> unbox<AdtPilot_Tree>
let second = apply make11 AdtPilot_E |> unbox<AdtPilot_Tree>
check "partial constructor reusable first" (AdtPilot_depth_adt_native first = 2)
check "partial constructor reusable second" (AdtPilot_depth_adt_native second = 2)
check "partial constructor retained identity" (System.Object.ReferenceEquals(AdtPilot_leftChild_adt_native second, red))
for value in [System.Int32.MinValue; -1; 0; 7; System.Int32.MaxValue] do
    for fn in [AdtConsumer_throughGeneric; AdtConsumer_throughPartial] do
        let tree = apply fn (box value) |> unbox<AdtPilot_Tree>
        check "constructor as polymorphic value Int" (AdtPilot_rootValue_adt_native tree = value)
        check "consumer projection native Int" (unbox<int> (apply AdtConsumer_rootValue (box tree)) = value)
        check "constructor as polymorphic value depth" (AdtPilot_depth_adt_native tree = 1)
check "consumer left projection identity" (System.Object.ReferenceEquals(apply AdtConsumer_leftChild (box made), red))
check "consumer Color projection" (unbox<AdtPilot_Color> (apply AdtConsumer_rootColor (box made)) = AdtPilot_B_adt_native)
let black value = AdtPilot_T_adt_native AdtPilot_B_adt_native AdtPilot_E_adt_native value AdtPilot_E_adt_native
let deep = AdtPilot_T_adt_native AdtPilot_B_adt_native red 11 (black 99)
check "nested color/tree/Int pattern hit" (unbox<int> (apply AdtConsumer_deepPattern (box deep)) = 99)
check "nested Int pattern miss" (unbox<int> (apply AdtConsumer_deepPattern (box (AdtPilot_T_adt_native AdtPilot_B_adt_native (leaf 8) 11 (black 99)))) = 0)
check "nested color pattern miss" (unbox<int> (apply AdtConsumer_deepPattern (box (AdtPilot_T_adt_native AdtPilot_R_adt_native red 11 (black 99)))) = 0)
check "nested tree pattern empty miss" (unbox<int> (apply AdtConsumer_deepPattern AdtPilot_E) = 0)
check "named child projection identity" (System.Object.ReferenceEquals(apply AdtConsumer_namedChild (box deep), red))
check "named child fallback" (unbox<AdtPilot_Tree> (apply AdtConsumer_namedChild AdtPilot_E) = AdtPilot_E_adt_native)
let shared = apply AdtConsumer_shared (box red) |> unbox<AdtPilot_Tree>
match shared with
| AdtPilot_Tusd_Ctor(_, left, _, right) ->
    check "shared children identity" (System.Object.ReferenceEquals(left, right))
    check "shared input identity" (System.Object.ReferenceEquals(left, red))
| _ -> failwith "shared consumer value was not a node"
check "producer native depth accepts consumer result" (AdtPilot_depth_adt_native shared = 2)
check "consumer calls producer wrapper" (unbox<int> (apply AdtConsumer_roundTrip (box red)) = 2)
check "input retains original value" (AdtPilot_rootValue_adt_native red = 7)
let boxed = apply AdtConsumer_wrap (box made)
check "local boxed ADT retains native child identity" (System.Object.ReferenceEquals(apply AdtConsumer_unwrap boxed, made))
check "local boxed ADT retains boxed Int" (unbox<int> (apply AdtConsumer_boxedValue boxed) = 42)
check "boxed outer and native inner pattern hit" (unbox<int> (apply AdtConsumer_boxedPattern boxed) = 7)
check "boxed outer and native inner pattern miss" (unbox<int> (apply AdtConsumer_boxedPattern (apply AdtConsumer_wrap (box red))) = 0)
match unbox<AdtConsumer_ConsumerBox> boxed with
| AdtConsumer_ConsumerBoxusd_Ctor(child, value) ->
    check "local boxed constructor retains object fields" ((child :? AdtPilot_Tree) && (value :? int))
events.Clear()
let ordered = apply AdtConsumer_orderedConstruction (box red) |> unbox<AdtPilot_Tree>
check "saturated arguments evaluate left to right once" (List.ofSeq events = [1; 2; 3; 4])
check "ordered construction native depth" (AdtPilot_depth_adt_native ordered = 2)
events.Clear()
let orderedPartial = apply AdtConsumer_orderedPartial (box red)
check "supplied partial arguments evaluate eagerly" (List.ofSeq events = [1; 2])
let reusedPartial = apply orderedPartial (box 17)
check "partial retains arguments without reevaluation" (List.ofSeq events = [1; 2])
let orderedFirst = apply reusedPartial (box red) |> unbox<AdtPilot_Tree>
let orderedSecond = apply reusedPartial AdtPilot_E |> unbox<AdtPilot_Tree>
check "repeated partial application keeps captured argument evaluations" (List.ofSeq events = [1; 2])
check "partial first result retains native Int" (AdtPilot_rootValue_adt_native orderedFirst = 17)
check "partial second result retains native Int" (AdtPilot_rootValue_adt_native orderedSecond = 17)
check "partial result retains left input identity" (System.Object.ReferenceEquals(AdtPilot_leftChild_adt_native orderedSecond, red))
printfn "adt-interop runtime: %d checks passed" checks
`;
  const fsx = join(directory, "adt-interop.fsx");
  const source = [preludeFs, header, producerFs, foreign, consumerFs, runtime].join("\n\n");
  await writeFile(fsx, source);
  const result = command(process.env.DOTNET || "dotnet", ["fsi", "--nologo", "--optimize+", "--exec", fsx], directory);
  if (process.env.ADT_INTEROP_ARTIFACTS) {
    const destination = resolve(process.env.ADT_INTEROP_ARTIFACTS);
    await mkdir(destination, { recursive: true });
    await writeFile(join(destination, "AdtPilot.fs"), producerFs);
    await writeFile(join(destination, "AdtConsumer.fs"), consumerFs);
    await writeFile(join(destination, "adt-interop.fsx"), source);
    await writeFile(join(destination, "fsi.log"), result.stdout + result.stderr);
    const hash = (bytes) => createHash("sha256").update(bytes).digest("hex");
    const sourceFiles = ["src/Sharpurs/AdtKernel.purs", "src/Sharpurs/AdtLayout.purs", "src/Sharpurs/AdtInterop.purs", "src/Sharpurs/CodeGen.purs",
      "tests/adt-interop.mjs", ...fixtures.map((name) => `tests/fixtures/${name}.purs`)];
    const hashes = {};
    for (const file of sourceFiles) hashes[file] = hash(await readFile(join(backend, file)));
    await writeFile(join(destination, "metadata.json"), JSON.stringify({
      sourceHashes: hashes,
      generatedHashes: { producer: hash(producerFs), consumer: hash(consumerFs), full: hash(source) },
      purs: command(process.env.PURS || "purs", ["--version"], directory).stdout.trim(),
      dotnet: command(process.env.DOTNET || "dotnet", ["--version"], directory).stdout.trim(),
      exitCode: result.status,
    }, null, 2) + "\n");
  }
  successful(result, "F# ADT interoperability");
  assert.doesNotMatch(result.stderr, /warning FS\d+/, "interop compiles without warnings");
  assert.match(result.stdout, /adt-interop runtime: 62 checks passed/, "all runtime assertions ran");
  console.log(result.stdout.trim());
  console.log(`adt-interop converter: ${converterChecks} checks passed`);
} finally {
  process.chdir(previousCwd);
  await rm(directory, { recursive: true, force: true });
}
