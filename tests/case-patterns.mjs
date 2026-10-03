// Run after building. Exercise ordinary and nested case lowering on real
// PureScript input, with JavaScript results as the oracle and F# evaluation logs.
import assert from "node:assert/strict";
import { mkdtemp, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";
import { compileFixtures, copyFixtures, runFsharp } from "./support/fixtures.mjs";
import { readCoreFn } from "./support/corefn.mjs";
import { fsharpFixture } from "./support/fsharp.mjs";
import * as Map from "../output/Data.Map/index.js";
import { ordString } from "../output/Data.Ord/index.js";
import { translateModule } from "../output/Sharpurs.CodeGen/index.js";
import { sanitizeName } from "../output/Sharpurs.FsAst/index.js";
import { printModule } from "../output/Sharpurs.Printer/index.js";
import { normalizeRecIndent } from "../output/Sharpurs.Printer.Layout/index.js";
import { prelude } from "../output/Sharpurs.Runtime/index.js";

const backend = resolve(dirname(fileURLToPath(import.meta.url)), "..");

const directory = await mkdtemp(join(tmpdir(), "sharpurs-case-patterns-"));
try {
  await copyFixtures(join(backend, "tests/fixtures/case-patterns"), directory, ["CasePatterns.purs", "CasePatterns.js"]);
  compileFixtures(directory, [join(directory, "CasePatterns.purs")]);
  const core = (await readCoreFn(directory)).get("Case.Patterns");
  assert.ok(core, "real compiler CoreFn parsed by PBO");
  let constructors = Map.empty;
  for (const declaration of core.dataDecls) for (const ctor of declaration.constructors) {
    const name = sanitizeName(`${core.name.replaceAll(".", "_")}_${ctor.name}`);
    constructors = Map.insert(ordString)(name)(ctor.fields.length)(constructors);
  }
  const generated = printModule(translateModule(constructors)(core));
  const oracle = await import(pathToFileURL(join(directory, "output/Case.Patterns/index.js")));
  const int = value => ({ js: value, fs: `(box ${value})` });
  const node = (name, ...args) => ({
    js: args.length ? args.reduce((fn, arg) => fn(arg.js), oracle[name].create) : oracle[name].value,
    fs: `(box (Case_Patterns_${name}usd_Ctor${args.length ? `(${args.map(arg => arg.fs).join(", ")})` : ""}))`,
  });
  const end = node("End");
  const a = value => node("A", value);
  const b = value => node("B", value);
  const target = a(a(a(end)));
  const values = [end, a(end), a(a(end)), target, b(a(a(end))), a(a(b(end))), a(a(a(b(end)))),
    a(a(node("Leaf", int(0)))), a(a(node("Leaf", int(7)))), node("Pair", end, node("Leaf", int(23))),
    node("Pair", a(end), node("Leaf", int(24))), a(a(a(node("Leaf", int(25)))))];
  const assertions = [];
  for (const name of ["deep", "shallow", "bound", "named", "wrapped"]) {
    for (const [index, value] of values.entries()) {
      const expected = oracle[name](value.js); // Wrapped is erased by both backends.
      assertions.push("events.Clear()",
        `check "${name} input ${index}" ${expected} (sharpurs_apply Case_Patterns_${name} ${value.fs})`);
      if (name === "deep") assertions.push(`checkEvents "deep scrutinee evaluated once, input ${index}" [1]`);
    }
  }
  for (const enabled of [false, true]) for (const value of [target, end]) {
    const expected = oracle.guarded(enabled)(value.js);
    const events = value === target ? (enabled ? "1; 2" : "1; 2; 3") : "1";
    assertions.push("events.Clear()",
      `check "guarded ${enabled}" ${expected} (call2 Case_Patterns_guarded (box ${enabled}) ${value.fs})`,
      `checkEvents "guards run in source order and stop at success" [${events}]`);
  }
  for (const left of [target, end]) for (const right of [b(node("Leaf", int(81))), end]) {
    oracle.track.events.length = 0;
    const expected = oracle.multiple(left.js)(right.js);
    // The frontend may reorder pure scrutinees before producing CoreFn. Check
    // its JavaScript order, as well as exactly-once evaluation of both inputs.
    const order = oracle.track.events;
    assert.deepEqual([...order].sort(), [1, 2]);
    assertions.push("events.Clear()",
      `check "multiple scrutinees" ${expected} (call2 Case_Patterns_multiple ${left.fs} ${right.fs})`,
      `checkEvents "scrutinee order matches JavaScript" [${order.join("; ")}]`);
  }

  await writeFile(join(directory, "Sharpurs_Prelude.fs"), prelude);
  const runtime = await fsharpFixture("case-patterns/Runtime.fs", { GENERATED: generated, ASSERTIONS: assertions.join("\n") });
  await writeFile(join(directory, "case-patterns.fsx"), normalizeRecIndent(runtime));
  const result = runFsharp(directory, "case-patterns.fsx");
  process.stdout.write(result.stdout);
} finally {
  await rm(directory, { recursive: true, force: true });
}
