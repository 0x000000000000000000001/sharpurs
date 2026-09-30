// Run after building the compiler. Compare boxed recursive code generation
// with the JavaScript backend on the same real PureScript source.
import assert from "node:assert/strict";
import { mkdtemp, readFile, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";
import { spawnSync } from "node:child_process";
import * as Aff from "../output/Effect.Aff/index.js";
import { Left } from "../output/Data.Either/index.js";
import { Cons } from "../output/Data.List.Types/index.js";
import * as Map from "../output/Data.Map/index.js";
import { ordString } from "../output/Data.Ord/index.js";
import { coreFnModulesFromOutput } from "../output/PureScript.Backend.Optimizer.App/index.js";
import { translateModule } from "../output/Sharpurs.CodeGen/index.js";
import { sanitizeName } from "../output/Sharpurs.FsAst/index.js";
import { printModule } from "../output/Sharpurs.Printer/index.js";
import { normalizeRecIndent } from "../output/Sharpurs.Printer.Layout/index.js";
import { prelude } from "../output/Sharpurs.Runtime/index.js";

const backend = resolve(dirname(fileURLToPath(import.meta.url)), "..");
const runAff = action => new Promise((resolve, reject) => {
  Aff.runAff(result => () => result instanceof Left ? reject(result.value0) : resolve(result.value0))(action)();
});
function command(program, args, cwd) {
  const result = spawnSync(program, args, { cwd, encoding: "utf8", timeout: 60_000 });
  if (result.error) throw result.error;
  assert.equal(result.status, 0, `${program}: ${result.stdout}\n${result.stderr}`);
  return result;
}

const directory = await mkdtemp(join(tmpdir(), "sharpurs-recursion-"));
try {
  for (const file of ["Recursion.purs", "Recursion.js"]) {
    await writeFile(join(directory, file), await readFile(join(backend, "tests/fixtures/recursion", file)));
  }
  await writeFile(join(directory, "package.json"), '{"type":"module"}\n');
  command(process.env.PURS || "purs", ["compile", join(directory, "Recursion.purs"),
    "--output", join(directory, "output"), "--codegen", "corefn,js"], directory);
  const modules = await runAff(coreFnModulesFromOutput(join(directory, "output")));
  let core;
  for (let cursor = modules; cursor instanceof Cons; cursor = cursor.value1) {
    if (cursor.value0.name === "Recursion.Context") core = cursor.value0;
  }
  assert.ok(core, "real compiler CoreFn parsed by PBO");
  let constructors = Map.empty;
  for (const declaration of core.dataDecls) for (const ctor of declaration.constructors) {
    const name = sanitizeName(`${core.name.replaceAll(".", "_")}_${ctor.name}`);
    constructors = Map.insert(ordString)(name)(ctor.fields.length)(constructors);
  }
  const generated = printModule(translateModule(constructors)(core));
  const oracle = await import(pathToFileURL(join(directory, "output/Recursion.Context/index.js")));
  const assertions = [];
  for (const name of ["topSaturated", "topPartial", "topValue", "topOver", "topMutual",
    "localSaturated", "localPartial", "localValue", "localOver", "localMutual", "nested", "shadowed"]) {
    for (const [count, initial] of [[0, 7], [1, -5], [19, 23]]) {
      const expected = oracle[name](count)(initial);
      assert.equal(expected, initial + count * (["nested", "shadowed"].includes(name) ? 2 : 1));
      assertions.push(`check "${name} ${count}/${initial}" ${expected} (call2 Recursion_Context_${name} ${count} (${initial}))`);
    }
  }
  for (const name of ["topSaturated", "localSaturated"]) {
    const expected = oracle[name](100_000)(9);
    assertions.push(`check "${name} deep tail call" ${expected} (call2 Recursion_Context_${name} 100000 9)`);
  }
  const partial = oracle.localPartial(8);
  assertions.push("let partial = sharpurs_apply Recursion_Context_localPartial (box 8)");
  for (const initial of [10, 30]) {
    assertions.push(`check "reuse local partial ${initial}" ${partial(initial)} (sharpurs_apply partial (box ${initial}))`);
  }
  assertions.push(`check "recursive values have no worker" ${oracle.recursiveValues} Recursion_Context_recursiveValues`);

  await writeFile(join(directory, "Sharpurs_Prelude.fs"), prelude);
  await writeFile(join(directory, "recursion.fsx"), normalizeRecIndent(`
#load "Sharpurs_Prelude.fs"
open Sharpurs_Prelude
let Recursion_Context_decrement : obj = box (fun (value: obj) -> box (unbox<int> value - 1))
let Recursion_Context_increment : obj = box (fun (value: obj) -> box (unbox<int> value + 1))
${generated}
let mutable checks = 0
let check label expected actual =
    if unbox<int> actual <> expected then failwithf "%s: expected %d, got %A" label expected actual
    checks <- checks + 1
let call2 fn first second = sharpurs_apply (sharpurs_apply fn (box first)) (box second)
${assertions.join("\n")}
printfn "recursion: %d runtime checks passed against JavaScript" checks
`));
  const result = command(process.env.DOTNET || "dotnet", ["fsi", "--nologo", "--exec", "recursion.fsx"], directory);
  process.stdout.write(result.stdout);
} finally {
  await rm(directory, { recursive: true, force: true });
}
