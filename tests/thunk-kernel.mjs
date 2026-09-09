// Real fork TAST and PBO, checked against a generic F# oracle and generated JS.
import assert from 'node:assert/strict';
import { mkdtemp, mkdir, readFile, readdir, rm, writeFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { dirname, join, resolve } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { spawnSync } from 'node:child_process';
import * as C from '../output/PureScript.Backend.Optimizer.CoreFn/index.js';
import * as S from '../output/PureScript.Backend.Optimizer.Syntax/index.js';
import * as Aff from '../output/Effect.Aff/index.js';
import * as Applicative from '../output/Control.Applicative/index.js';
import { Left } from '../output/Data.Either/index.js';
import { Just, Nothing } from '../output/Data.Maybe/index.js';
import * as Map from '../output/Data.Map.Internal/index.js';
import * as Set from '../output/Data.Set/index.js';
import * as App from '../output/PureScript.Backend.Optimizer.App/index.js';
import * as Builder from '../output/PureScript.Backend.Optimizer.Builder/index.js';
import * as Foreign from '../output/PureScript.Backend.Optimizer.Semantics.Foreign/index.js';
import { prepareModule, fromExpr } from '../output/Sharpurs.ThunkKernel/index.js';
import { translateModule, translateOptimizedModuleWithThunks } from '../output/Sharpurs.CodeGen/index.js';
import { printModule } from '../output/Sharpurs.Printer/index.js';

const backend = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const artifacts = process.env.THUNK_KERNEL_ARTIFACTS && resolve(process.env.THUNK_KERNEL_ARTIFACTS);
const transcript = [];
const pure = Applicative.pure(Aff.applicativeAff);
const runAff = action => new Promise((resolve, reject) => {
  Aff.runAff(result => () => result instanceof Left ? reject(result.value0) : resolve(result.value0))(action)();
});
function command(program, args, cwd) {
  const result = spawnSync(program, args, { cwd, encoding: 'utf8', timeout: 60_000 });
  transcript.push(`$ ${program} ${args.join(' ')}\n${result.stdout || ''}${result.stderr || ''}`);
  if (result.error) throw result.error;
  assert.equal(result.status, 0, `${program}: ${result.stdout}\n${result.stderr}`);
  return result;
}
function clone(value) {
  if (Array.isArray(value)) return value.map(clone);
  if (!value || typeof value !== 'object') return value;
  return Object.assign(Object.create(Object.getPrototypeOf(value)), Object.fromEntries(
    Object.entries(value).map(([key, child]) => [key, clone(child)])));
}
function bindings(core) {
  return core.decls.flatMap(group => group instanceof C.NonRec ? [group.value0] : group.value0);
}
function selections(plan, tree, found = []) {
  if (!tree || typeof tree !== 'object') return found;
  if (tree instanceof C.ExprApp && fromExpr(plan)(tree) instanceof Just) found.push(tree);
  for (const child of Object.values(tree)) selections(plan, child, found);
  return found;
}
const directory = await mkdtemp(join(tmpdir(), 'sharpurs-thunk-kernel-'));
const previousCwd = process.cwd();
try {
  const packages = join(backend, '.spago/p');
  const names = await readdir(packages);
  const prelude = process.env.PRELUDE_SRC || join(packages, names.find(name => /^prelude-/.test(name)), 'src');
  const partial = join(packages, names.find(name => /^partial-/.test(name)), 'src');
  const fixtureFiles = ['TypedThunks.purs', 'ThunkExternal.purs', 'ThunkExternal.js'];
  for (const file of fixtureFiles) await writeFile(join(directory, file), await readFile(join(backend, 'tests/fixtures/thunk-kernel', file)));
  await writeFile(join(directory, 'package.json'), '{"type":"module"}\n');
  const compiled = command(process.env.PURS || 'purs', ['compile', join(directory, '*.purs'),
    join(prelude, '**/*.purs'), join(partial, '**/*.purs'), '--output', join(directory, 'output'), '--codegen', 'corefn,js'], directory);
  assert.doesNotMatch(compiled.stdout + compiled.stderr, /Warning \d+ of/, 'fixture has no PureScript warning');
  process.chdir(directory);
  const captured = new globalThis.Map();
  await runAff(Builder.buildModules(Aff.monadAff)({
    directives: await runAff(App.loadDirectives), rewriteLimit: 10000,
    analyzeCustom: _ => _ => Nothing.value,
    foreignSemantics: Map.filterKeys(C.ordQualified(C.ordIdent))(qualified => {
      const name = qualified.value0 instanceof Just ? qualified.value0.value0 : '';
      return !name.includes('Effect') && !name.includes('Control.Monad.ST');
    })(Foreign.coreForeignSemantics),
    traceIdents: Set.empty,
    onPrepareModule: _ => module => pure(module),
    onSkipModule: _ => _ => pure(Nothing.value),
    onCodegenModule: _ => core => module => _ => {
      if (['TypedThunks', 'ThunkExternal'].includes(module.name)) captured.set(module.name, { core, backend: module });
      return pure(undefined);
    },
  })(await runAff(App.coreFnModulesFromOutput(join(directory, 'output')))));
  const state = captured.get('TypedThunks');
  assert.ok(state && captured.size === 2, 'real source modules parsed and optimized');
  const selected = prepareModule(state.core)(state.backend);
  assert.ok(selected instanceof Just, 'typed thunk workers found');
  const plan = selected.value0;
  const binding = name => bindings(state.core).find(b => b.value1 === name);
  const generate = candidate => printModule(translateOptimizedModuleWithThunks(Set.empty)(Nothing.value)(candidate)(Map.empty)(state.backend)(state.core));
  const generated = generate(selected), oracle = generate(Nothing.value);
  let checks = 0;
  const yes = (condition, label) => { assert.ok(condition, label); checks++; };
  for (const name of ['run', 'runLiteral', 'runTwo', 'runBy']) {
    yes(selections(plan, binding(name)).length > 0, `${name}: proven closed seed selects native route`);
  }
  for (const name of ['escaped', 'unknown', 'unknownCallback', 'partialChain', 'partialRun', 'importedRun', 'opaqueRun', 'numberRun']) {
    yes(selections(plan, binding(name)).length === 0, `${name}: generic route retained`);
  }
  yes(generated.includes('unit -> int'), 'generated worker has native thunk type');
  yes(generated.includes('let rec TypedThunks_chain_tco'), 'original public recursive ABI remains');
  yes(oracle.includes('sharpurs_apply'), 'oracle uses actual generic generated application');
  const original = selections(plan, binding('run'))[0];
  const reject = (label, edit) => {
    const expr = clone(original); edit(expr);
    yes(fromExpr(plan)(expr) instanceof Nothing, label);
  };
  reject('missing call type', expr => { expr.value0.type = Nothing.value; });
  reject('contradictory call type', expr => { expr.value0.type = new Just(C.Number.value); });
  reject('missing argument type', expr => { expr.value2.value0.type = Nothing.value; });
  reject('function argument type does not prove closure provenance', expr => {
    const ann = clone(expr.value2.value0);
    expr.value2 = new C.ExprVar(ann, new C.Qualified(Nothing.value, 'unknownSeed'));
  });
  yes(original.value1 instanceof C.ExprTypeApp, 'real TAST v3 instantiates force at Int');
  reject('wrong helper TypeApp', expr => { expr.value1.value2 = C.Number.value; });
  reject('contradictory polymorphic helper type', expr => { expr.value1.value1.value0.type = new Just(C.Int.value); });
  reject('missing worker suffix type', expr => { expr.value2.value1.value0.type = Nothing.value; });
  reject('wrong worker suffix type', expr => { expr.value2.value1.value0.type = new Just(C.Int.value); });
  reject('partial worker is not a thunk', expr => { expr.value2 = expr.value2.value1; });
  reject('extra worker argument', expr => { expr.value2 = new C.ExprApp(expr.value2.value0, expr.value2, expr.value2.value2); });
  const rejectWorker = (label, edit) => {
    const mutated = clone(state); edit(mutated);
    const result = prepareModule(mutated.core)(mutated.backend);
    const sites = result instanceof Just ? selections(result.value0, bindings(mutated.core).find(b => b.value1 === 'run')) : [];
    yes(sites.length === 0, label);
  };
  const source = st => bindings(st.core).find(b => b.value1 === 'chain').value2;
  rejectWorker('missing source function type', st => { source(st).value0.type = Nothing.value; });
  rejectWorker('contradictory source function type', st => { source(st).value0.type = new Just(C.Int.value); });
  rejectWorker('returned Unit is not a source argument', st => { source(st).value2 = source(st).value2.value2; });
  rejectWorker('missing inner source lambda type', st => { source(st).value2.value0.type = Nothing.value; });
  rejectWorker('contradictory optimized signature', st => {
    st.backend.bindings.flatMap(g => g.bindings).find(b => b.value0 === 'chain').value1.value0 = C.Int.value;
  });
  rejectWorker('native helper name collision', st => {
    const workerName = generated.match(/let rec (?:private )?(TypedThunks_chain_thunk_native)\b/)?.[1];
    assert.ok(workerName, 'native worker declaration found');
    const extra = clone(bindings(st.core).find(b => b.value1 === 'runLiteral'));
    extra.value1 = workerName.replace(/^TypedThunks_/, '');
    st.core.decls.push(new C.NonRec(extra));
  });

  const js = await import(pathToFileURL(join(directory, 'output/TypedThunks/index.js')));
  const cases = [];
  for (const depth of [0, 1, 3, 37, 1000]) for (const seed of [-2147483648, -1, 0, 17, 2147483647]) {
    const expected = Number(BigInt.asIntN(32, BigInt(depth) + BigInt(seed)));
    yes(js.run(depth)(seed) === expected, 'JS result agrees with Int32 oracle');
    cases.push([depth, seed, expected]);
  }
  const main = await readFile(join(backend, 'src/Main.purs'), 'utf8');
  const preludeFs = main.match(/^fsPrelude = """\r?\n([\s\S]*?)^"""/m)?.[1];
  assert.ok(preludeFs);
  const support = `
open System
let (|LitInt|_|) (expected: int) (value: obj) = if value :? int && unbox<int> value = expected then Some() else None
let (|LitBool|_|) (expected: bool) (value: obj) = if value :? bool && unbox<bool> value = expected then Some() else None
let Data_Unit_unit = box ()
let events = ResizeArray<int>()
let ThunkExternal_track : obj = box (fun (value: obj) -> events.Add(unbox<int> value); value)
let Partial_Unsafe_unsafePartial : obj = box (fun (value: obj) -> sharpurs_apply value (box ()))
let Partial_Unsafe__unsafePartial = Partial_Unsafe_unsafePartial
let binary operation : obj = box (fun (x: obj) -> box (fun (y: obj) -> operation x y))
let intAdd = binary (fun x y -> box (unbox<int> x + unbox<int> y))
let intSub = binary (fun x y -> box (unbox<int> x - unbox<int> y))
let Data_Semiring_semiringInt : obj = box (Map.ofList ["add", intAdd])
let Data_Ring_ringInt : obj = box (Map.ofList ["sub", intSub])
let Data_Semiring_semiringNumber : obj = box (Map.ofList ["add", binary (fun x y -> box (unbox<float> x + unbox<float> y))])
let Data_Semiring_add : obj = box (fun (dict: obj) -> Map.find "add" (unbox<Map<string,obj>> dict))
let Data_Ring_sub : obj = box (fun (dict: obj) -> Map.find "sub" (unbox<Map<string,obj>> dict))
`;
  const external = printModule(translateModule(Map.empty)(captured.get('ThunkExternal').core));
  const scoped = (name, body) => `module ${name} =\n${body.split('\n').map(line => `    ${line}`).join('\n')}\n`;
  const runtime = `
let mutable checks = 0
let check label condition = if not condition then failwith label else checks <- checks + 1
let apply = sharpurs_apply
let call fn n seed = apply (apply fn (box n)) (box seed) |> unbox<int>
let cases = [${cases.map(row => `(${row.join(', ')})`).join('; ')}]
for depth, seed, expected in cases do
    check "native route agrees with JS" (call Native.TypedThunks_run depth seed = expected)
    check "generic oracle agrees with JS" (call Oracle.TypedThunks_run depth seed = expected)
    check "literal seed result" (apply Native.TypedThunks_runLiteral (box depth) |> unbox<int> = depth + 7)
    let left = apply (apply Native.TypedThunks_escaped (box depth)) (box seed)
    check "escaping thunk stays callable" (apply Native.TypedThunks_force left |> unbox<int> = expected)
    check "escaping thunk reusable" (apply Native.TypedThunks_force left |> unbox<int> = expected)
    let partial = apply Native.TypedThunks_run (box depth)
    check "partial Int capture reused" (apply partial (box seed) |> unbox<int> = expected)
    check "partial captures independent" (apply partial (box 9) |> unbox<int> = depth + 9)
for step, depth, seed in [(2, 5, 7); (-3, 10, 29); (System.Int32.MaxValue, 3, 7)] do
    let callBy fn = apply (apply (apply fn (box step)) (box depth)) (box seed) |> unbox<int>
    check "additional captured step" (callBy Native.TypedThunks_runBy = callBy Oracle.TypedThunks_runBy)
for depth in [0; 1; 3; 1000] do
    let seed : obj = box (fun (_: obj) -> events.Add(31); box 11)
    events.Clear()
    check "unknown callback retains result" (call Native.TypedThunks_unknownCallback depth seed = depth + 11)
    check "unknown callback executes once" (List.ofSeq events = [31])
    let partial = apply Native.TypedThunks_chain (box depth)
    let left = apply partial seed
    check "construction is delayed" (List.ofSeq events = [31])
    for _ in [1..2] do apply Native.TypedThunks_force left |> ignore
    check "no memoization added" (List.ofSeq events = [31; 31; 31])
    events.Clear()
    check "opaque nested seed result" (call Native.TypedThunks_opaqueRun depth 12 = depth + 12)
    check "opaque seed observes exactly one call" (List.ofSeq events = [12])
let signature action =
    let rec shape (error: exn) wrappers =
        match error with
        | :? System.Reflection.TargetInvocationException as invocation when not (isNull invocation.InnerException) -> shape invocation.InnerException (wrappers + 1)
        | cause -> cause.GetType().FullName, wrappers
    let caught = try action() |> ignore; None with ex -> Some (shape ex 0)
    match caught with Some value -> value | None -> failwith "Expected exception"
for depth in [0; 1; 3] do
    check "local partial exception boundary" (signature (fun () -> call Native.TypedThunks_partialRun depth 1) = signature (fun () -> call Oracle.TypedThunks_partialRun depth 1))
    check "imported partial exception boundary" (signature (fun () -> call Native.TypedThunks_importedRun depth 1) = signature (fun () -> call Oracle.TypedThunks_importedRun depth 1))
let sentinel = InvalidOperationException("delayed callback")
for depth in [0; 1; 3; 1000] do
    let mutable throws = 0
    let seed : obj = box (fun (_: obj) -> throws <- throws + 1; raise sentinel : obj)
    let chain = apply (apply Native.TypedThunks_chain (box depth)) seed
    check "throwing seed stays delayed" (throws = 0)
    for repeat in [1..2] do
        let mutable caught = None
        try apply Native.TypedThunks_force chain |> ignore with ex -> caught <- Some ex
        let rec cause (ex: exn) count =
            match ex with
            | :? System.Reflection.TargetInvocationException as e when not (isNull e.InnerException) -> cause e.InnerException (count + 1)
            | e -> e, count
        let exceptionCause, wrappers = cause caught.Value 0
        check "deferred cause identity" (Object.ReferenceEquals(exceptionCause, sentinel))
        check "original force boundaries" (wrappers = 2 * (depth + 1))
        check "repeat force calls again" (throws = repeat)
let mutable total = 0
for _ in [1..1000] do total <- total + call Native.TypedThunks_run 1000 0
check "full million-thunk workload" (total = 1000000)
printfn "thunk-kernel runtime: %d checks passed" checks
`;
  const script = [preludeFs, support, external, scoped('Native', generated), scoped('Oracle', oracle), runtime].join('\n\n');
  await writeFile(join(directory, 'thunk-kernel.fsx'), script);
  if (artifacts) {
    await mkdir(artifacts, { recursive: true });
    for (const [file, text] of [['generated.fs', generated], ['oracle.fs', oracle], ['thunk-kernel.fsx', script]]) await writeFile(join(artifacts, file), text);
    await writeFile(join(artifacts, 'corefn.json'), await readFile(join(directory, 'output/TypedThunks/corefn.json')));
  }
  const result = command(process.env.DOTNET || 'dotnet', ['fsi', '--nologo', '--optimize+', '--exec', 'thunk-kernel.fsx'], directory);
  for (const warning of result.stderr.matchAll(/\((\d+),\d+\): warning (FS\d+):/g)) {
    yes(['FS0025', 'FS0040'].includes(warning[2]), 'only known generic partial/recursive fixture warnings');
    yes(!script.split('\n')[Number(warning[1])-1].includes('_thunk_native'), 'no native worker warnings');
  }
  assert.match(result.stdout, /thunk-kernel runtime: \d+ checks passed/);
  console.log(result.stdout.trim());
  console.log(`thunk-kernel converter/JS: ${checks} checks passed`);
} catch (error) {
  transcript.push(error.stack || String(error)); throw error;
} finally {
  if (artifacts) { await mkdir(artifacts, { recursive: true }); await writeFile(join(artifacts, 'validation.log'), transcript.join('\n')); }
  process.chdir(previousCwd);
  await rm(directory, { recursive: true, force: true });
}
