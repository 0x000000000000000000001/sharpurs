// Compile a real TAST fixture, check conservative selection, then execute F#.
import assert from "node:assert/strict";
import { mkdtemp, mkdir, readFile, readdir, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";
import { spawnSync } from "node:child_process";
import * as C from "../output/PureScript.Backend.Optimizer.CoreFn/index.js";
import * as Aff from "../output/Effect.Aff/index.js";
import { Left } from "../output/Data.Either/index.js";
import { Cons } from "../output/Data.List.Types/index.js";
import { Just, Nothing } from "../output/Data.Maybe/index.js";
import * as Map from "../output/Data.Map.Internal/index.js";
import * as App from "../output/PureScript.Backend.Optimizer.App/index.js";
import { fromExpr } from "../output/Sharpurs.IntComparison/index.js";
import { translateModule } from "../output/Sharpurs.CodeGen/index.js";
import { printModule } from "../output/Sharpurs.Printer/index.js";

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
function clone(value) {
  if (Array.isArray(value)) return value.map(clone);
  if (!value || typeof value !== "object") return value;
  return Object.assign(Object.create(Object.getPrototypeOf(value)), Object.fromEntries(
    Object.entries(value).map(([key, child]) => [key, clone(child)])));
}
function selections(value, found = []) {
  if (!value || typeof value !== "object") return found;
  if (value instanceof C.ExprApp && fromExpr(value) instanceof Just) found.push(value);
  for (const child of Object.values(value)) selections(child, found);
  return found;
}
const directory = await mkdtemp(join(tmpdir(), "sharpurs-int-comparison-"));
try {
  const preludeRoot = join(backend, ".spago/p");
  const prelude = process.env.PRELUDE_SRC || join(preludeRoot,
    (await readdir(preludeRoot)).find(name => /^prelude-/.test(name)), "src");
  await writeFile(join(directory, "IntCompare.purs"), await readFile(join(backend, "tests/fixtures/IntCompare.purs")));
  await writeFile(join(directory, "IntCompare.js"), "export const track = _ => value => value;\n");
  await writeFile(join(directory, "package.json"), '{"type":"module"}\n');
  command(process.env.PURS || "purs", ["compile", join(directory, "IntCompare.purs"),
    join(prelude, "**/*.purs"), "--output", join(directory, "output"), "--codegen", "corefn,js"], directory);
  const modules = await runAff(App.coreFnModulesFromOutput(join(directory, "output")));
  let core;
  for (let cursor = modules; cursor instanceof Cons; cursor = cursor.value1) {
    if (cursor.value0.name === "IntCompare") core = cursor.value0;
  }
  assert.ok(core, "fixture parsed by PBO's TAST reader");
  const bindings = core.decls.flatMap(group => group instanceof C.NonRec ? [group.value0] : group.value0);
  const binding = name => bindings.find(b => b.value1 === name);
  let checks = 0;
  for (const name of ["less", "greater", "ordered"]) {
    assert.equal(selections(binding(name)).length, 1, `${name}: one native comparison`); checks++;
  }
  for (const name of ["annotated", "partial", "genericLess", "stringLess", "numberLess", "custom"]) {
    assert.equal(selections(binding(name)).length, 0, `${name}: generic semantics retained`); checks++;
  }
  const original = selections(binding("less"))[0];
  const parts = e => ({ right: e.value2, left: e.value1.value2,
    dictionary: e.value1.value1.value2, head: e.value1.value1.value1 });
  const reject = (label, edit) => {
    const expr = clone(original); edit(expr, parts(expr));
    assert.ok(fromExpr(expr) instanceof Nothing, label); checks++;
  };
  reject("missing result type", e => { e.value0.type = Nothing.value; });
  reject("wrong result type", e => { e.value0.type = new Just(C.Int.value); });
  reject("missing operand type", (_, p) => { p.left.value0.type = Nothing.value; });
  reject("wrong operand type", (_, p) => { p.right.value0.type = new Just(C.Number.value); });
  reject("contradictory partial signature", e => { e.value1.value0.type = new Just(C.Int.value); });
  reject("contradictory dictionary application", e => { e.value1.value1.value0.type = new Just(C.Int.value); });
  reject("local dictionary", (_, p) => { p.dictionary.value1.value0 = Nothing.value; });
  reject("custom Int dictionary", (_, p) => { p.dictionary.value1.value1 = "customOrdInt"; });
  reject("wrong dictionary owner", (_, p) => { p.dictionary.value1.value0 = new Just("Other.Ord"); });
  reject("missing dictionary type", (_, p) => { p.dictionary.value0.type = Nothing.value; });
  reject("inconsistent dictionary path", (_, p) => { p.dictionary.value0.type.value0.value1 = ["Other", "Ord"]; });
  reject("local function shadows lessThan", (_, p) => { p.head.value1.value1.value0 = Nothing.value; });
  reject("wrong function owner", (_, p) => { p.head.value1.value1.value0 = new Just("Other.Ord"); });
  reject("wrong TypeApp argument", (_, p) => { p.head.value2 = C.String.value; });
  reject("contradictory instantiated signature", (_, p) => { p.head.value0.type = new Just(C.Int.value); });
  reject("missing polymorphic signature", (_, p) => { p.head.value1.value0.type = Nothing.value; });
  reject("extra TypeApp", (e, p) => { e.value1.value1.value1 = new C.ExprTypeApp(p.head.value0, p.head, C.Int.value); });
  assert.ok(fromExpr(original.value1) instanceof Nothing, "partial application rejected"); checks++;
  assert.ok(fromExpr(new C.ExprApp(original.value0, original, parts(original).right)) instanceof Nothing,
    "over-application rejected"); checks++;
  const generated = printModule(translateModule(Map.empty)(core));
  const byName = name => generated.split("\n\n").find(line => line.startsWith(`let IntCompare_${name} `));
  for (const name of ["less", "greater"]) {
    assert.doesNotMatch(byName(name), /sharpurs_apply/); checks++;
  }
  assert.match(byName("genericLess"), /Data_Ord_lessThan/); checks++;
  assert.match(byName("custom"), /IntCompare_ordReverse/); checks++;
  const js = await import(pathToFileURL(join(directory, "output/IntCompare/index.js")));
  const values = [-2147483648, -2147483647, -1, 0, 1, 2147483646, 2147483647];
  const cases = values.flatMap(x => values.map(y => [x, y, js.less(x)(y), js.greater(x)(y), js.custom(x)(y)]));
  const fsCases = cases.map(row => `    (${row.join(", ")})`).join(";\n");
  const main = await readFile(join(backend, "src/Main.purs"), "utf8");
  const preludeFs = main.match(/^fsPrelude = """\r?\n([\s\S]*?)^"""/m)?.[1];
  assert.ok(preludeFs);
  // Instrumented object-ABI dependencies: fallback calls must still dispatch
  // through their supplied dictionary, including the reversed custom order.
  const support = `
let mutable fallbackCalls = 0
let events = ResizeArray<int>()
let IntCompare_track : obj = box (fun (label: obj) -> box (fun (value: obj) -> events.Add(unbox<int> label); value))
let Data_Ord_Ordusd_Dict : obj = box (fun (value: obj) -> value)
let Data_Eq_Equsd_Dict = Data_Ord_Ordusd_Dict
let comparison : obj = box (fun (x: obj) -> box (fun (y: obj) -> box (Unchecked.compare x y)))
let equality : obj = box (fun (x: obj) -> box (fun (y: obj) -> box (x = y)))
let Data_Eq_eqInt : obj = box (Map.ofList ["eq", equality])
let Data_Eq_eq : obj = box (fun (dict: obj) -> Map.find "eq" (unbox<Map<string,obj>> dict))
let Data_Ord_ordInt : obj = box (Map.ofList ["compare", comparison; "Eq0", box (fun (_: obj) -> Data_Eq_eqInt)])
let Data_Ord_ordString = Data_Ord_ordInt
let Data_Ord_ordNumber = Data_Ord_ordInt
let Data_Ord_compare : obj = box (fun (dict: obj) -> Map.find "compare" (unbox<Map<string,obj>> dict))
let relation predicate : obj = box (fun (dict: obj) -> box (fun (x: obj) -> box (fun (y: obj) ->
    fallbackCalls <- fallbackCalls + 1
    box (predicate (unbox<int> (sharpurs_apply (sharpurs_apply (sharpurs_apply Data_Ord_compare dict) x) y))))))
let Data_Ord_lessThan = relation (fun ordering -> ordering < 0)
let Data_Ord_greaterThan = relation (fun ordering -> ordering > 0)
`;
  const runtime = `
let mutable checks = 0
let check label condition = if not condition then failwith label else checks <- checks + 1
let call fn x y = sharpurs_apply (sharpurs_apply fn (box x)) (box y) |> unbox<bool>
let cases = [
${fsCases}
]
for x, y, less, greater, custom in cases do
    let before = fallbackCalls
    check "less matches JS" (call IntCompare_less x y = less)
    check "greater matches JS" (call IntCompare_greater x y = greater)
    check "native calls bypass dictionary" (fallbackCalls = before)
    check "custom order matches JS" (call IntCompare_custom x y = custom)
    check "custom order uses its dictionary" (fallbackCalls = before + 1)
    check "annotated alias matches JS" (call IntCompare_annotated x y = less)
    check "generic Int dispatch" (call (sharpurs_apply IntCompare_genericLess Data_Ord_ordInt) x y = less)
    check "partial comparator" (unbox<bool> (sharpurs_apply IntCompare_partial (box y)) = (5 < y))
check "String fallback" (call IntCompare_stringLess "a" "b")
check "Number fallback" (call IntCompare_numberLess 1.25 1.5)
events.Clear()
check "ordered comparison result" (call IntCompare_ordered 3 7)
check "left-to-right once" (List.ofSeq events = [1; 2])
printfn "int-comparison runtime: %d checks passed" checks
`;
  const source = [preludeFs, support, generated, runtime].join("\n\n");
  await writeFile(join(directory, "comparison.fsx"), source);
  if (process.env.INT_COMPARISON_ARTIFACTS) {
    const target = resolve(process.env.INT_COMPARISON_ARTIFACTS);
    await mkdir(target, { recursive: true });
    await writeFile(join(target, "IntCompare.fs"), generated);
    await writeFile(join(target, "comparison.fsx"), source);
  }
  const result = command(process.env.DOTNET || "dotnet", ["fsi", "--nologo", "--optimize+", "--exec", "comparison.fsx"], directory);
  assert.doesNotMatch(result.stderr, /warning FS\d+/);
  console.log(result.stdout.trim());
  console.log(`int-comparison converter: ${checks} checks passed`);
} finally {
  await rm(directory, { recursive: true, force: true });
}
