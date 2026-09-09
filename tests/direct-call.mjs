// Run after npm run build. PURS must select the compiler with TAST annotations.
import assert from "node:assert/strict";
import { mkdtemp, mkdir, readFile, readdir, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { spawnSync } from "node:child_process";
import * as C from "../output/PureScript.Backend.Optimizer.CoreFn/index.js";
import * as Aff from "../output/Effect.Aff/index.js";
import { Left } from "../output/Data.Either/index.js";
import { Cons } from "../output/Data.List.Types/index.js";
import { Just, Nothing } from "../output/Data.Maybe/index.js";
import * as Map from "../output/Data.Map.Internal/index.js";
import * as Ord from "../output/Data.Ord/index.js";
import * as App from "../output/PureScript.Backend.Optimizer.App/index.js";
import { fromBinding, fromCall } from "../output/Sharpurs.DirectCall/index.js";
import { fromModule } from "../output/Sharpurs.AdtLayout/index.js";
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
function clone(value, rename = false) {
  if (typeof value === "string") return rename ? value.replaceAll("DirectCall", "DirectFallback") : value;
  if (Array.isArray(value)) return value.map(child => clone(child, rename));
  if (!value || typeof value !== "object") return value;
  return Object.assign(Object.create(Object.getPrototypeOf(value)), Object.fromEntries(
    Object.entries(value).map(([key, child]) => [key, clone(child, rename)])));
}
function walk(value, predicate, found = []) {
  if (!value || typeof value !== "object") return found;
  if (predicate(value)) found.push(value);
  for (const child of Object.values(value)) walk(child, predicate, found);
  return found;
}
function application(expr) {
  const args = [], nodes = [];
  while (expr instanceof C.ExprApp) { nodes.unshift(expr); args.unshift(expr.value2); expr = expr.value1; }
  return { head: expr, args, nodes };
}
function generate(core) {
  let constructors = Map.empty;
  for (const decl of core.dataDecls) for (const ctor of decl.constructors) {
    constructors = Map.insert(Ord.ordString)(`${core.name.replaceAll(".", "_")}_${ctor.name}`)(ctor.fields.length)(constructors);
  }
  return printModule(translateModule(constructors)(core));
}
const directory = await mkdtemp(join(tmpdir(), "sharpurs-direct-call-"));
try {
  const preludeRoot = join(backend, ".spago/p");
  const prelude = process.env.PRELUDE_SRC || join(preludeRoot,
    (await readdir(preludeRoot)).find(name => /^prelude-/.test(name)), "src");
  for (const file of ["DirectCall.purs", "DirectCall.js", "DirectRemote.purs"]) {
    await writeFile(join(directory, file), await readFile(join(backend, "tests/fixtures/direct-call", file)));
  }
  await writeFile(join(directory, "package.json"), '{"type":"module"}\n');
  command(process.env.PURS || "purs", ["compile", join(directory, "*.purs"), join(prelude, "**/*.purs"),
    "--output", join(directory, "output"), "--codegen", "corefn,js"], directory);
  const modules = await runAff(App.coreFnModulesFromOutput(join(directory, "output")));
  let core;
  for (let cursor = modules; cursor instanceof Cons; cursor = cursor.value1) {
    if (cursor.value0.name === "DirectCall") core = cursor.value0;
  }
  assert.ok(core, "real compiler TAST parsed by PBO");
  const binding = name => core.decls.find(group => group instanceof C.NonRec && group.value0.value1 === name);
  let checks = 0;
  const yes = (condition, label) => { assert.ok(condition, label); checks++; };
  for (const name of ["ordinal", "guarded", "captured", "imported", "failBody"]) {
    yes(fromBinding(Nothing.value)(binding(name)) instanceof Just, `${name}: supported saturated source lambdas`);
  }
  for (const name of ["unary", "generic", "stringPair", "numberPair", "higher", "shadowed", "localShadow",
    "returned", "partial1", "partial2", "partial3", "asValue", "orderedPartial"]) {
    yes(fromBinding(Nothing.value)(binding(name)) instanceof Nothing, `${name}: conservative fallback`);
  }
  const layout = fromModule(core);
  yes(layout instanceof Just, "TAST supplies closed local ADT layout");
  yes(fromBinding(layout)(binding("arrange")) instanceof Just, "closed accepted local ADT signature");
  yes(fromBinding(Nothing.value)(binding("arrange")) instanceof Nothing, "ADT needs an accepted native layout");
  const original = binding("ordinal");
  const selected = fromBinding(Nothing.value)(original).value0;
  assert.equal(selected.args.length, 4); checks++;
  yes(fromBinding(Nothing.value)(new C.Rec([original.value0])) instanceof Nothing, "recursive group retained");
  const rejectBinding = (label, edit) => {
    const copy = clone(original); edit(copy.value0);
    yes(fromBinding(Nothing.value)(copy) instanceof Nothing, label);
  };
  rejectBinding("missing declaration annotation", b => { b.value0.type = Nothing.value; });
  rejectBinding("contradictory declaration signature", b => { b.value0.type = new Just(C.Int.value); });
  rejectBinding("missing first lambda annotation", b => { b.value2.value0.type = Nothing.value; });
  rejectBinding("missing inner lambda annotation", b => { b.value2.value2.value0.type = Nothing.value; });
  rejectBinding("contradictory lambda suffix", b => { b.value2.value2.value0.type = new Just(C.Int.value); });
  rejectBinding("missing body annotation", b => {
    let e = b.value2; while (e instanceof C.ExprAbs) e = e.value2; e.value0.type = Nothing.value;
  });
  rejectBinding("contradictory body result", b => {
    let e = b.value2; while (e instanceof C.ExprAbs) e = e.value2; e.value0.type = new Just(C.Boolean.value);
  });
  rejectBinding("explicit polymorphic declaration", b => { b.value0.type = new Just(new C.ForAll(["a"], b.value0.type.value0)); });
  rejectBinding("duplicate sanitized argument names", b => {
    b.value2.value1 = "arg'"; b.value2.value2.value1 = "arg_prime";
  });
  const registry = Map.singleton(new C.Qualified(new Just("DirectCall"), "ordinal"))(selected);
  const calls = value => walk(value, e => e instanceof C.ExprApp && fromCall(registry)(e) instanceof Just);
  for (const name of ["saturated", "ordered", "captured", "argumentCall"]) {
    assert.equal(calls(binding(name)).length, 1, `${name}: exactly saturated qualified call`); checks++;
  }
  for (const name of ["partial1", "partial2", "partial3", "orderedPartial", "shadowed", "localShadow", "imported"]) {
    assert.equal(calls(binding(name)).length, 0, `${name}: no direct ordinal dispatch`); checks++;
  }
  const call = calls(binding("saturated"))[0];
  const rejectCall = (label, edit) => {
    const copy = clone(call); edit(copy, application(copy));
    yes(fromCall(registry)(copy) instanceof Nothing, label);
  };
  rejectCall("missing call result annotation", e => { e.value0.type = Nothing.value; });
  rejectCall("contradictory call result", e => { e.value0.type = new Just(C.Boolean.value); });
  rejectCall("missing argument type", (_, p) => { p.args[0].value0.type = Nothing.value; });
  rejectCall("contradictory argument type", (_, p) => { p.args[2].value0.type = new Just(C.Number.value); });
  rejectCall("missing callee signature", (_, p) => { p.head.value0.type = Nothing.value; });
  rejectCall("contradictory callee signature", (_, p) => { p.head.value0.type = new Just(C.Int.value); });
  rejectCall("missing intermediate application type", (_, p) => { p.nodes[1].value0.type = Nothing.value; });
  rejectCall("contradictory intermediate signature", (_, p) => { p.nodes[1].value0.type = new Just(C.Int.value); });
  rejectCall("unqualified local shadow", (_, p) => { p.head.value1.value0 = Nothing.value; });
  rejectCall("imported owner", (_, p) => { p.head.value1.value0 = new Just("DirectRemote"); });
  rejectCall("different function", (_, p) => { p.head.value1.value1 = "unknown"; });
  rejectCall("explicit TypeApp boundary", (_, p) => {
    p.nodes[0].value1 = new C.ExprTypeApp(p.head.value0, p.head, C.Int.value);
  });
  yes(fromCall(registry)(call.value1) instanceof Nothing, "partial application rejected");
  yes(fromCall(registry)(new C.ExprApp(call.value0, call, application(call).args[0])) instanceof Nothing,
    "over-application rejected");

  const generated = generate(core);
  const declaration = name => generated.split("\n\n").find(line => line.startsWith(`let DirectCall_${name} `));
  yes(generated.includes("let DirectCall_ordinal_direct "), "generic named function gets direct implementation");
  yes(generated.includes("let DirectCall_ordinal_direct_apply "), "direct invocation preserves exception envelope");
  yes(/DirectCall_ordinal_direct_apply/.test(declaration("saturated")), "saturated call emitted directly");
  yes(!/DirectCall_ordinal_direct_apply/.test(declaration("shadowed")), "shadowed function dispatch remains dynamic");
  yes(!/DirectCall_ordinal_direct_apply/.test(declaration("localShadow")), "local function keeps its own body");
  yes(/sharpurs_apply/.test(declaration("partial3")), "partial application retains object ABI");
  yes(!generated.includes("let DirectCall_returned_direct "), "returned function does not cross a let boundary");
  yes(!generated.includes("let DirectCall_return_direct_apply "), "source binding blocks reserved-name helper collision");
  const parameterCollision = clone(core);
  const colliding = parameterCollision.decls.find(group => group instanceof C.NonRec && group.value0.value1 === "ordinal");
  colliding.value0.value2.value1 = "DirectCall_ordinal_direct";
  yes(!generate(parameterCollision).includes("let DirectCall_ordinal_direct "),
    "argument matching helper name prevents capture");
  // The same source with unavailable declaration types is a generic-dispatch
  // oracle, including the exact exception wrapping and partial-application ABI.
  const fallbackCore = clone(core, true);
  for (const group of fallbackCore.decls) if (group instanceof C.NonRec) {
    group.value0.value0.type = Nothing.value;
    if (group.value0.value2 instanceof C.ExprAbs) group.value0.value2.value0.type = Nothing.value;
  }
  const fallback = generate(fallbackCore);
  yes(!/let DirectFallback_\w+_direct \(/.test(fallback), "fallback oracle has no direct entry points");
  const main = await readFile(join(backend, "src/Main.purs"), "utf8");
  const preludeFs = main.match(/^fsPrelude = """\r?\n([\s\S]*?)^"""/m)?.[1];
  assert.ok(preludeFs);
  const support = `
let (|LitBool|_|) (expected: bool) (value: obj) = if value :? bool && unbox<bool> value = expected then Some() else None
let events = ResizeArray<int>()
let failure = System.InvalidOperationException("direct-call fixture failure")
let DirectCall_track : obj = box (fun (label: obj) -> box (fun (value: obj) -> events.Add(unbox<int> label); value))
let DirectCall_explode : obj = box (fun (_: obj) -> events.Add(99); raise failure : obj)
let DirectFallback_track = DirectCall_track
let DirectFallback_explode = DirectCall_explode
let DirectRemote_remote : obj = box (fun (x: obj) -> box (fun (_: obj) -> x))
`;
  const runtime = `
let mutable checks = 0
let check label ok = if not ok then failwith label else checks <- checks + 1
let apply = sharpurs_apply
let call2 fn x y = apply (apply fn (box x)) (box y)
let call4 fn a b c d = apply (apply (apply (apply fn (box a)) (box b)) (box c)) (box d)
let asInt value = unbox<int> value
for a in [System.Int32.MinValue; -1; 0; 1; 9; 10; System.Int32.MaxValue] do
    for b in [System.Int32.MinValue; 0; 10; System.Int32.MaxValue] do
        check "public wrapper matches generic oracle" (call4 DirectCall_ordinal a b 20 30 = call4 DirectFallback_ordinal a b 20 30)
        check "raw direct method matches public wrapper" (DirectCall_ordinal_direct (box a) (box b) (box 20) (box 30) = call4 DirectCall_ordinal a b 20 30)
        check "captured top-level constant" (call2 DirectCall_captured a b = call2 DirectFallback_captured a b)
    check "saturated caller matches generic oracle" (apply DirectCall_saturated (box a) = apply DirectFallback_saturated (box a))
for first in [false; true] do
    for second in [false; true] do
        check "Boolean arguments and captured value" (call4 DirectCall_guarded first 11 second 22 = call4 DirectFallback_guarded first 11 second 22)
for fn in [DirectCall_ordered; DirectFallback_ordered] do
    events.Clear()
    check "ordered result" (asInt (apply fn (box 3)) = 20)
    check "all arguments evaluated once in source order" (List.ofSeq events = [1; 2; 3; 4])
let one = apply DirectCall_ordinal (box 1)
let two = apply one (box 2)
let three = apply two (box 3)
check "one captured argument reusable first" (asInt (call2 (apply one (box 2)) 3 4) = 3)
check "one captured argument reusable second" (asInt (call2 (apply one (box 0)) 5 6) = 6)
check "two captured arguments reusable first" (asInt (call2 two 7 8) = 7)
check "two captured arguments reusable second" (asInt (call2 two 9 10) = 9)
check "three captured arguments reusable first" (asInt (apply three (box 40)) = 3)
check "three captured arguments reusable second" (asInt (apply three (box 50)) = 3)
check "top-level one argument partial" (asInt (call2 (apply DirectCall_partial1 (box 2)) 3 4) = 3)
check "top-level two argument partial" (asInt (call2 DirectCall_partial2 7 8) = 7)
check "top-level three argument partial" (asInt (apply DirectCall_partial3 (box 8)) = 3)
check "function as a value" (asInt (call4 DirectCall_asValue 1 2 3 4) = 3)
events.Clear()
let orderedOne = apply DirectCall_orderedPartial (box 1)
check "partial supplied argument evaluates eagerly" (List.ofSeq events = [1])
check "ordered partial first use" (asInt (call2 (apply orderedOne (box 2)) 3 4) = 3)
check "ordered partial second use" (asInt (call2 (apply orderedOne (box 0)) 5 6) = 6)
check "partial capture does not repeat argument effects" (List.ofSeq events = [1])
let firstOfFour : obj = box (fun (a: obj) -> box (fun (_: obj) -> box (fun (_: obj) -> box (fun (_: obj) -> a))))
check "shadowed parameter remains dynamic" (asInt (apply DirectCall_shadowed firstOfFour) = 4)
check "local shadow remains local" (asInt (apply DirectCall_localShadow (box 42)) = 42)
check "imported function retains object ABI" (asInt (call2 DirectCall_imported 41 99) = 41)
for fn in [DirectCall_returned; DirectFallback_returned] do
    events.Clear()
    let returned = call2 fn true 41
    check "returned function boundary evaluates let eagerly" (List.ofSeq events = [90])
    check "returned function first use" (asInt (apply returned (box 1)) = 41)
    check "returned function reused" (asInt (apply returned (box 2)) = 41)
    check "returned function retains capture" (List.ofSeq events = [90])
let captured action = try action() |> ignore; failwith "expected failure" with ex -> ex
let rec chain (ex: System.Exception) =
    match ex with
    | :? System.Reflection.TargetInvocationException as wrapper -> 1 + chain wrapper.InnerException
    | cause when System.Object.ReferenceEquals(cause, failure) -> 0
    | cause -> failwithf "Unexpected exception: %A" cause
let publicFailure = chain (captured (fun () -> call2 DirectCall_failBody 1 2))
let fallbackFailure = chain (captured (fun () -> call2 DirectFallback_failBody 1 2))
check "public body exception chain matches generic oracle" (publicFailure = fallbackFailure && publicFailure = 2)
let directFailure = chain (captured (fun () -> apply DirectCall_bodyCall (box 1)))
let fallbackCallerFailure = chain (captured (fun () -> apply DirectFallback_bodyCall (box 1)))
check "direct body exception chain matches generic caller" (directFailure = fallbackCallerFailure && directFailure = 3)
for fn in [DirectCall_argumentCall; DirectFallback_argumentCall] do
    events.Clear()
    check "argument exception stays outside direct invocation envelope" (chain (captured (fun () -> apply fn (box 1))) = 2)
    check "argument failure stops later evaluations" (List.ofSeq events = [99])
printfn "direct-call runtime: %d checks passed" checks
`;
  const source = [preludeFs, support, generated, fallback, runtime].join("\n\n");
  await writeFile(join(directory, "direct-call.fsx"), source);
  if (process.env.DIRECT_CALL_ARTIFACTS) {
    const destination = resolve(process.env.DIRECT_CALL_ARTIFACTS);
    await mkdir(destination, { recursive: true });
    await writeFile(join(destination, "DirectCall.fs"), generated);
    await writeFile(join(destination, "DirectFallback.fs"), fallback);
    await writeFile(join(destination, "direct-call.fsx"), source);
  }
  const result = command(process.env.DOTNET || "dotnet", ["fsi", "--nologo", "--optimize+", "--exec", "direct-call.fsx"], directory);
  assert.doesNotMatch(result.stderr, /warning FS\d+/, "direct-call fixtures compile without warnings");
  console.log(result.stdout.trim());
  console.log(`direct-call converter: ${checks} checks passed`);
} finally {
  await rm(directory, { recursive: true, force: true });
}
