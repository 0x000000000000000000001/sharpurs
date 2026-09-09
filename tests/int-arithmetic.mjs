// Real fork-Purs TAST -> PBO -> F#, checked against generated JS and a generic
// F# oracle. Run after the backend build; no compiler rebuild occurs here.
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
import { fromExpr } from "../output/Sharpurs.IntArithmetic/index.js";
import { translateModule } from "../output/Sharpurs.CodeGen/index.js";
import { printModule } from "../output/Sharpurs.Printer/index.js";

const backend = resolve(dirname(fileURLToPath(import.meta.url)), "..");
const artifacts = process.env.INT_ARITHMETIC_ARTIFACTS && resolve(process.env.INT_ARITHMETIC_ARTIFACTS);
const transcript = [];
const runAff = action => new Promise((resolve, reject) => {
  Aff.runAff(result => () => result instanceof Left ? reject(result.value0) : resolve(result.value0))(action)();
});
function command(program, args, cwd) {
  const result = spawnSync(program, args, { cwd, encoding: "utf8", timeout: 60_000 });
  transcript.push(`$ ${program} ${args.join(" ")}\n${result.stdout || ""}${result.stderr || ""}`);
  if (result.error) throw result.error;
  assert.equal(result.status, 0, `${program}: ${result.stdout}\n${result.stderr}`);
  return result;
}
function clone(value, rename = false) {
  if (typeof value === "string") return rename ? value.replaceAll("IntArithmetic", "ArithmeticFallback") : value;
  if (Array.isArray(value)) return value.map(child => clone(child, rename));
  if (!value || typeof value !== "object") return value;
  return Object.assign(Object.create(Object.getPrototypeOf(value)), Object.fromEntries(
    Object.entries(value).map(([key, child]) => [key, clone(child, rename)])));
}
function selections(value, found = []) {
  if (!value || typeof value !== "object") return found;
  if (value instanceof C.ExprApp && fromExpr(value) instanceof Just) found.push(value);
  for (const child of Object.values(value)) selections(child, found);
  return found;
}
function parts(expr) {
  return { right: expr.value2, left: expr.value1.value2,
    dictionary: expr.value1.value1.value2, head: expr.value1.value1.value1 };
}
const generate = core => printModule(translateModule(Map.empty)(core));
const directory = await mkdtemp(join(tmpdir(), "sharpurs-int-arithmetic-"));
try {
  const preludeRoot = join(backend, ".spago/p");
  const prelude = process.env.PRELUDE_SRC || join(preludeRoot,
    (await readdir(preludeRoot)).find(name => /^prelude-/.test(name)), "src");
  for (const file of ["IntArithmetic.purs", "IntArithmetic.js"]) {
    await writeFile(join(directory, file), await readFile(join(backend, "tests/fixtures/int-arithmetic", file)));
  }
  await writeFile(join(directory, "package.json"), '{"type":"module"}\n');
  command(process.env.PURS || "purs", ["compile", join(directory, "IntArithmetic.purs"),
    join(prelude, "**/*.purs"), "--output", join(directory, "output"), "--codegen", "corefn,js"], directory);
  const modules = await runAff(App.coreFnModulesFromOutput(join(directory, "output")));
  let core;
  for (let cursor = modules; cursor instanceof Cons; cursor = cursor.value1) {
    if (cursor.value0.name === "IntArithmetic") core = cursor.value0;
  }
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
  const bindings = core.decls.flatMap(group => group instanceof C.NonRec ? [group.value0] : group.value0);
  const binding = name => bindings.find(b => b.value1 === name);
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
    const applied = binding(name).value2.value2.value2.value1.value1;
    yes(applied instanceof C.ExprTypeApp && applied.value2 === C.Int.value,
      `${name}: visible TypeApp Int preserved by the real fork`);
    yes(applied.value1.value1.value0.type instanceof Nothing,
      `${name}: unavailable canonical head annotation explains fallback`);
  }
  for (const name of ["addInt", "subInt"]) {
    const original = selections(binding(name))[0];
    const reject = (label, edit) => {
      const expr = clone(original); edit(expr, parts(expr));
      yes(fromExpr(expr) instanceof Nothing, `${name}: ${label}`);
    };
    reject("missing result type", e => { e.value0.type = Nothing.value; });
    reject("wrong result type", e => { e.value0.type = new Just(C.Boolean.value); });
    reject("missing operand type", (_, p) => { p.left.value0.type = Nothing.value; });
    reject("wrong operand type", (_, p) => { p.right.value0.type = new Just(C.Number.value); });
    reject("contradictory partial signature", e => { e.value1.value0.type = new Just(C.Int.value); });
    reject("missing dictionary application type", e => { e.value1.value1.value0.type = Nothing.value; });
    reject("contradictory dictionary application", e => { e.value1.value1.value0.type = new Just(C.Int.value); });
    reject("local dictionary", (_, p) => { p.dictionary.value1.value0 = Nothing.value; });
    reject("custom Int dictionary", (_, p) => { p.dictionary.value1.value1 = "customInt"; });
    reject("wrong dictionary owner", (_, p) => { p.dictionary.value1.value0 = new Just("Other.Numeric"); });
    reject("missing dictionary type", (_, p) => { p.dictionary.value0.type = Nothing.value; });
    reject("inconsistent dictionary path", (_, p) => { p.dictionary.value0.type.value0.value1 = ["Other", "Numeric"]; });
    reject("local function shadow", (_, p) => { p.head.value1.value1.value0 = Nothing.value; });
    reject("wrong function owner", (_, p) => { p.head.value1.value1.value0 = new Just("Other.Numeric"); });
    reject("unsupported numeric operation", (_, p) => { p.head.value1.value1.value1 = "mul"; });
    reject("wrong TypeApp argument", (_, p) => { p.head.value2 = C.Number.value; });
    reject("contradictory instantiated signature", (_, p) => { p.head.value0.type = new Just(C.Int.value); });
    reject("missing polymorphic signature", (_, p) => { p.head.value1.value0.type = Nothing.value; });
    reject("extra TypeApp", (e, p) => { e.value1.value1.value1 = new C.ExprTypeApp(p.head.value0, p.head, C.Int.value); });
    yes(fromExpr(original.value1) instanceof Nothing, `${name}: partial application rejected`);
    yes(fromExpr(new C.ExprApp(original.value0, original, parts(original).right)) instanceof Nothing,
      `${name}: over-application rejected`);
    const concrete = clone(original);
    const instantiatedHead = parts(concrete).head;
    concrete.value1.value1.value1 = new C.ExprVar(instantiatedHead.value0, instantiatedHead.value1.value1);
    yes(fromExpr(concrete) instanceof Just, `${name}: concrete canonical signature without TypeApp`);
    const polymorphic = clone(original);
    polymorphic.value1.value1.value1 = parts(polymorphic).head.value1;
    yes(fromExpr(polymorphic) instanceof Just, `${name}: exact ForAll signature without TypeApp`);
  }
  const add = clone(selections(binding("addInt"))[0]);
  add.value1.value1.value2 = clone(parts(selections(binding("subInt"))[0]).dictionary);
  yes(fromExpr(add) instanceof Nothing, "add must not accept canonical Ring dictionary");
  const sub = clone(selections(binding("subInt"))[0]);
  sub.value1.value1.value2 = clone(parts(selections(binding("addInt"))[0]).dictionary);
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
  const fallbackCore = clone(core, true);
  for (const expr of selections(fallbackCore)) parts(expr).head.value0.type = Nothing.value;
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
  const main = await readFile(join(backend, "src/Main.purs"), "utf8");
  const preludeFs = main.match(/^fsPrelude = """\r?\n([\s\S]*?)^"""/m)?.[1];
  assert.ok(preludeFs);
  const support = `
let mutable fallbackCalls = 0
let events = ResizeArray<int>()
let failure = System.InvalidOperationException("int-arithmetic fixture failure")
let mutable currentValue = 0
let mutable reads = 0
let IntArithmetic_track : obj = box (fun (label: obj) -> box (fun (value: obj) -> events.Add(unbox<int> label); value))
let IntArithmetic_explode : obj = box (fun (_: obj) -> events.Add(99); raise failure : obj)
let IntArithmetic_readCurrent : obj = box (fun (_: obj) -> reads <- reads + 1; box currentValue)
let ArithmeticFallback_track = IntArithmetic_track
let ArithmeticFallback_explode = IntArithmetic_explode
let ArithmeticFallback_readCurrent = IntArithmetic_readCurrent
let Data_Unit_unit = box ()
let binary operation : obj = box (fun (x: obj) -> box (fun (y: obj) ->
    fallbackCalls <- fallbackCalls + 1
    operation x y))
let intAdd = binary (fun x y -> box (unbox<int> x + unbox<int> y))
let intSub = binary (fun x y -> box (unbox<int> x - unbox<int> y))
let numberAdd = binary (fun x y -> box (unbox<float> x + unbox<float> y))
let numberSub = binary (fun x y -> box (unbox<float> x - unbox<float> y))
let Data_Semiring_semiringInt : obj = box (Map.ofList ["add", intAdd])
let Data_Ring_ringInt : obj = box (Map.ofList ["sub", intSub; "Semiring0", box (fun (_: obj) -> Data_Semiring_semiringInt)])
let Data_Semiring_semiringNumber : obj = box (Map.ofList ["add", numberAdd])
let Data_Ring_ringNumber : obj = box (Map.ofList ["sub", numberSub; "Semiring0", box (fun (_: obj) -> Data_Semiring_semiringNumber)])
let Data_Semiring_add : obj = box (fun (dict: obj) -> Map.find "add" (unbox<Map<string,obj>> dict))
let Data_Ring_sub : obj = box (fun (dict: obj) -> Map.find "sub" (unbox<Map<string,obj>> dict))
// Deliberately noncanonical dictionaries exercise object-ABI dispatch rather
// than deriving semantics from the types of their Int operands.
let customSemiring : obj = box (Map.ofList ["add", binary (fun x y -> box (unbox<int> y - unbox<int> x))])
let customRing : obj = box (Map.ofList ["sub", intAdd])
`;
  const runtime = `
let mutable checks = 0
let check label condition = if not condition then failwith label else checks <- checks + 1
let apply = sharpurs_apply
let call fn x y = apply (apply fn (box x)) (box y) |> unbox<int>
let cases = [
${fsCases}
]
for x, y, sum, difference in cases do
    let before = fallbackCalls
    check "native sum matches JS" (call IntArithmetic_addInt x y = sum)
    check "native difference matches JS" (call IntArithmetic_subInt x y = difference)
    check "native Int operations bypass dictionaries" (fallbackCalls = before)
    check "generic oracle sum" (call ArithmeticFallback_addInt x y = sum)
    check "generic oracle difference" (call ArithmeticFallback_subInt x y = difference)
    check "oracle uses actual dictionary calls" (fallbackCalls = before + 2)
    check "visible TypeApp add fallback" (call IntArithmetic_visibleAdd x y = sum)
    check "visible TypeApp sub fallback" (call IntArithmetic_visibleSub x y = difference)
    check "generic Int add" (call (apply IntArithmetic_genericAdd Data_Semiring_semiringInt) x y = sum)
    check "generic Int sub" (call (apply IntArithmetic_genericSub Data_Ring_ringInt) x y = difference)
    check "annotated add alias" (call IntArithmetic_annotatedAdd x y = sum)
    check "annotated sub alias" (call IntArithmetic_annotatedSub x y = difference)
    check "custom Int semiring retains supplied operation" (call (apply IntArithmetic_genericAdd customSemiring) x y = (y - x))
    check "custom Int ring retains supplied operation" (call (apply IntArithmetic_genericSub customRing) x y = sum)
for y in [System.Int32.MinValue; -1; 0; 1; System.Int32.MaxValue] do
    check "partial add reusable" (unbox<int> (apply IntArithmetic_partialAdd (box y)) = 5 + y)
    check "partial sub reusable" (unbox<int> (apply IntArithmetic_partialSub (box y)) = 5 - y)
let capturedAdd = apply IntArithmetic_capturedAdd (box System.Int32.MaxValue)
let capturedSub = apply IntArithmetic_capturedSub (box System.Int32.MinValue)
for y in [-1; 0; 1; 7] do
    check "captured addition reused" (unbox<int> (apply capturedAdd (box y)) = System.Int32.MaxValue + y)
    check "captured subtraction reused" (unbox<int> (apply capturedSub (box y)) = System.Int32.MinValue - y)
let floatCall fn x y = apply (apply fn (box x)) (box y) |> unbox<float>
check "Number addition fallback" (floatCall IntArithmetic_numberAdd 1.25 1.5 = 2.75)
check "Number subtraction fallback" (floatCall IntArithmetic_numberSub 1.25 1.5 = -0.25)
for fn, expected in [IntArithmetic_orderedAdd, 10; ArithmeticFallback_orderedAdd, 10;
                     IntArithmetic_orderedSub, -4; ArithmeticFallback_orderedSub, -4] do
    events.Clear()
    check "ordered arithmetic result" (call fn 3 7 = expected)
    check "operands evaluate exactly once left to right" (List.ofSeq events = [1; 2])
for fn, reference in [IntArithmetic_delayedAdd, ArithmeticFallback_delayedAdd;
                      IntArithmetic_delayedSub, ArithmeticFallback_delayedSub] do
    reads <- 0
    currentValue <- 10
    let delayed = apply fn (box 7)
    let oracle = apply reference (box 7)
    check "constructing closures does not force reads" (reads = 0)
    for value in [11; -1; System.Int32.MinValue; System.Int32.MaxValue] do
        currentValue <- value
        let before = reads
        let actual = apply delayed Data_Unit_unit
        check "each force reads once" (reads = before + 1)
        check "reused closure sees latest state" (actual = apply oracle Data_Unit_unit)
        check "oracle force reads once" (reads = before + 2)
let captured action = try action() |> ignore; failwith "expected failure" with ex -> ex
let rec chain (ex: System.Exception) =
    match ex with
    | :? System.Reflection.TargetInvocationException as wrapper -> 1 + chain wrapper.InnerException
    | cause when System.Object.ReferenceEquals(cause, failure) -> 0
    | cause -> failwithf "Unexpected exception: %A" cause
for fn, reference, expected in [IntArithmetic_firstFailureAdd, ArithmeticFallback_firstFailureAdd, [1; 99];
                                IntArithmetic_secondFailureAdd, ArithmeticFallback_secondFailureAdd, [1; 2; 99];
                                IntArithmetic_firstFailureSub, ArithmeticFallback_firstFailureSub, [1; 99];
                                IntArithmetic_secondFailureSub, ArithmeticFallback_secondFailureSub, [1; 2; 99]] do
    events.Clear()
    let actualChain = chain (captured (fun () -> apply fn (box 3)))
    check "argument exception stops further evaluation" (List.ofSeq events = expected)
    events.Clear()
    let oracleChain = chain (captured (fun () -> apply reference (box 3)))
    check "argument exception wrapper chain matches generic oracle" (actualChain = oracleChain && actualChain = 2)
    check "generic oracle has same exception order" (List.ofSeq events = expected)
printfn "int-arithmetic runtime: %d checks passed" checks
`;
  const source = [preludeFs, support, generated, fallback, runtime].join("\n\n");
  await writeFile(join(directory, "arithmetic.fsx"), source);
  if (artifacts) {
    await mkdir(artifacts, { recursive: true });
    for (const [name, contents] of [["IntArithmetic.fs", generated], ["ArithmeticFallback.fs", fallback],
      ["arithmetic.fsx", source], ["oracle-cases.json", JSON.stringify(cases, null, 2) + "\n"]]) {
      await writeFile(join(artifacts, name), contents);
    }
  }
  const result = command(process.env.DOTNET || "dotnet", ["fsi", "--nologo", "--optimize+", "--exec", "arithmetic.fsx"], directory);
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
