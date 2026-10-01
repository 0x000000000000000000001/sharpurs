// Run after building the compiler. Compare boxed recursive code generation
// with the JavaScript backend on the same real PureScript source.
import assert from "node:assert/strict";
import { mkdtemp, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";
import { compileFixtures, copyFixtures, runFsharp } from "./support/fixtures.mjs";
import { readCoreFn } from "./support/corefn.mjs";
import * as Map from "../output/Data.Map/index.js";
import { ordString } from "../output/Data.Ord/index.js";
import { translateModule } from "../output/Sharpurs.CodeGen/index.js";
import { sanitizeName } from "../output/Sharpurs.FsAst/index.js";
import { printModule } from "../output/Sharpurs.Printer/index.js";
import { normalizeRecIndent } from "../output/Sharpurs.Printer.Layout/index.js";
import { prelude } from "../output/Sharpurs.Runtime/index.js";

const backend = resolve(dirname(fileURLToPath(import.meta.url)), "..");

const directory = await mkdtemp(join(tmpdir(), "sharpurs-recursion-"));
try {
  await copyFixtures(join(backend, "tests/fixtures/recursion"), directory, ["Recursion.purs", "Recursion.js"]);
  compileFixtures(directory, [join(directory, "Recursion.purs")]);
  const core = (await readCoreFn(directory)).get("Recursion.Context");
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
  const result = runFsharp(directory, "recursion.fsx");
  process.stdout.write(result.stdout);
} finally {
  await rm(directory, { recursive: true, force: true });
}
