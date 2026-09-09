// Real fork TAST -> production codegen, compared with its unchanged curried
// fallback. CONSTRUCTOR_TYPEAPP_ORACLE_OUTPUT optionally verifies that oracle
// against a complete pre-change compiled output directory.
import assert from 'node:assert/strict';
import { mkdtemp, mkdir, readFile, readdir, rm, writeFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { dirname, join, resolve } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { spawnSync } from 'node:child_process';
import * as C from '../output/PureScript.Backend.Optimizer.CoreFn/index.js';
import * as Aff from '../output/Effect.Aff/index.js';
import * as Applicative from '../output/Control.Applicative/index.js';
import * as Either from '../output/Data.Either/index.js';
import * as Maybe from '../output/Data.Maybe/index.js';
import * as Map from '../output/Data.Map.Internal/index.js';
import * as Set from '../output/Data.Set/index.js';
import * as Ord from '../output/Data.Ord/index.js';
import * as App from '../output/PureScript.Backend.Optimizer.App/index.js';
import * as Builder from '../output/PureScript.Backend.Optimizer.Builder/index.js';
import * as Foreign from '../output/PureScript.Backend.Optimizer.Semantics.Foreign/index.js';
import * as Adt from '../output/Sharpurs.AdtKernel/index.js';
import * as CodeGen from '../output/Sharpurs.CodeGen/index.js';
import * as Printer from '../output/Sharpurs.Printer/index.js';
import { fromExpr } from '../output/Sharpurs.ConstructorCall/index.js';
const { Just, Nothing } = Maybe;
const backend = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const artifacts = process.env.CONSTRUCTOR_TYPEAPP_ARTIFACTS && resolve(process.env.CONSTRUCTOR_TYPEAPP_ARTIFACTS);
const api = { C, Aff, Applicative, Either, Maybe, Map, Set, Ord, App, Builder, Foreign, Adt, CodeGen, Printer };
const moduleNames = ['ConstructorNative', 'ConstructorImported', 'ConstructorTypeApp'];
const transcript = [];
let checks = 0;
const yes = (condition, message) => { assert.ok(condition, message); checks++; };
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
function walk(value, visit) {
  if (!value || typeof value !== 'object') return;
  visit(value);
  for (const child of Object.values(value)) walk(child, visit);
}
function bindings(core) { return core.decls.flatMap(group => group instanceof C.NonRec ? [group.value0] : group.value0); }
function head(expr) {
  while (expr instanceof C.ExprApp || expr instanceof C.ExprTypeApp) expr = expr.value1;
  return expr;
}
// Metadata is not evaluated by the generic backend. Removing IsConstructor
// only under TypeApp defeats the new recognizer; the old path still uses the
// same constructor registry and emits the original curried calls. No function
// body, argument, type application or exception boundary is hand-reimplemented.
function forceCurried(core) {
  const result = clone(core);
  walk(result, node => {
    if (node instanceof C.ExprTypeApp) {
      const target = head(node);
      if (target instanceof C.ExprVar && target.value0.meta instanceof Just && target.value0.meta.value0 instanceof C.IsConstructor) {
        target.value0.meta = Nothing.value;
      }
    }
  });
  return result;
}
async function collect(compiler, directory) {
  const { C, Aff, Applicative, Either, Maybe, Map, Set, App, Builder, Foreign } = compiler;
  const pure = Applicative.pure(Aff.applicativeAff);
  const runAff = action => new Promise((resolve, reject) => Aff.runAff(result => () =>
    result instanceof Either.Left ? reject(result.value0) : resolve(result.value0))(action)());
  const captured = new globalThis.Map();
  await runAff(Builder.buildModules(Aff.monadAff)({
    directives: await runAff(App.loadDirectives), rewriteLimit: 10000,
    analyzeCustom: _ => _ => Maybe.Nothing.value,
    foreignSemantics: Map.filterKeys(C.ordQualified(C.ordIdent))(qualified => {
      const name = qualified.value0 instanceof Maybe.Just ? qualified.value0.value0 : '';
      return !name.includes('Effect') && !name.includes('Control.Monad.ST');
    })(Foreign.coreForeignSemantics),
    traceIdents: Set.empty,
    onPrepareModule: _ => module => pure(module),
    onSkipModule: _ => _ => pure(Maybe.Nothing.value),
    onCodegenModule: _ => core => optimized => _ => {
      if (moduleNames.includes(optimized.name)) captured.set(optimized.name, { core, backend: optimized });
      return pure(undefined);
    },
  })(await runAff(App.coreFnModulesFromOutput(join(directory, 'output')))));
  assert.equal(captured.size, moduleNames.length, 'real fork/PBO prepared all fixture modules');
  return captured;
}
function configuration(compiler, captured) {
  const { Map, Set, Ord, Maybe, Adt } = compiler;
  let constructors = Map.empty, wrappers = Set.empty;
  for (const { core } of captured.values()) for (const decl of core.dataDecls) for (const ctor of decl.constructors) {
    const name = `${core.name.replaceAll('.', '_')}_${ctor.name}`;
    constructors = Map.insert(Ord.ordString)(name)(ctor.fields.length)(constructors);
    if (core.name === 'ConstructorNative') wrappers = Set.insert(Ord.ordString)(name)(wrappers);
  }
  const producer = captured.get('ConstructorNative');
  const native = Adt.prepareUnary(producer.core)(producer.backend);
  assert.ok(native instanceof Maybe.Just, 'native constructor producer is admitted');
  return { constructors, wrappers, native, producer };
}
async function oldCompiler(output) {
  const modules = { C:'PureScript.Backend.Optimizer.CoreFn', Aff:'Effect.Aff', Applicative:'Control.Applicative',
    Either:'Data.Either', Maybe:'Data.Maybe', Map:'Data.Map.Internal', Set:'Data.Set', Ord:'Data.Ord',
    App:'PureScript.Backend.Optimizer.App', Builder:'PureScript.Backend.Optimizer.Builder',
    Foreign:'PureScript.Backend.Optimizer.Semantics.Foreign', Adt:'Sharpurs.AdtKernel', CodeGen:'Sharpurs.CodeGen', Printer:'Sharpurs.Printer' };
  return Object.fromEntries(await Promise.all(Object.entries(modules).map(async ([key,name]) =>
    [key, await import(pathToFileURL(join(output, name, 'index.js')))])));
}
const directory = await mkdtemp(join(tmpdir(), 'sharpurs-constructor-typeapp-'));
const previousCwd = process.cwd();
try {
  const packages = join(backend, '.spago/p');
  const prelude = process.env.PRELUDE_SRC || join(packages, (await readdir(packages)).find(name => /^prelude-/.test(name)), 'src');
  for (const file of ['ConstructorTypeApp.purs', 'ConstructorTypeApp.js', 'ConstructorNative.purs', 'ConstructorImported.purs']) {
    await writeFile(join(directory, file), await readFile(join(backend, 'tests/fixtures/constructor-typeapp', file)));
  }
  await writeFile(join(directory, 'package.json'), '{"type":"module"}\n');
  const compiled = command(process.env.PURS || 'purs', ['compile', join(directory, '*.purs'), join(prelude, '**/*.purs'),
    '--output', join(directory, 'output'), '--codegen', 'corefn,js'], directory);
  assert.doesNotMatch(compiled.stdout+compiled.stderr, /Warning \d+ of/, 'fixture has no PureScript warning');
  process.chdir(directory);
  const captured = await collect(api, directory);
  const config = configuration(api, captured);
  const core = captured.get('ConstructorTypeApp').core;
  const currentMod = new Just('ConstructorTypeApp');
  const recognize = expr => fromExpr(config.constructors)(currentMod)(expr);
  const selections = tree => { const found=[]; walk(tree,node => { if (node instanceof C.ExprApp && recognize(node) instanceof Just) found.push(node); }); return found; };
  const binding = name => bindings(core).find(value => value.value1 === name);
  for (const name of ['boxInt', 'importedBox', 'pair', 'polyPair', 'explicitPair', 'justInt', 'list', 'prepend', 'ordered', 'throwFirst', 'throwSecond', 'nativePair']) {
    yes(selections(binding(name)).length>0, `${name}: saturated instantiated constructor selected`);
  }
  for (const name of ['partialPair', 'ordinaryCall', 'capturedArgument', 'nothingInt']) {
    yes(selections(binding(name)).length===0, `${name}: partial/ordinary/nullary path retained`);
  }
  yes(selections(binding('list')).length===2, 'both nested list constructor calls selected');
  yes(recognize(selections(binding('importedBox'))[0]).value0.name==='ConstructorImported_Envelope',
    'real imported polymorphic constructor retains its qualified owner');
  const original = selections(binding('pair'))[0];
  const selected = recognize(original).value0;
  yes(selected.name==='ConstructorTypeApp_Tuple' && selected.arity===2, 'two-field constructor identity and arity');
  const malformed = (label, edit) => { const expr=clone(original); edit(expr); yes(recognize(expr) instanceof Nothing,label); };
  malformed('missing constructor metadata', expr => { head(expr).value0.meta=Nothing.value; });
  malformed('newtype metadata is not ordinary constructor metadata', expr => { head(expr).value0.meta=new Just(C.IsNewtype.value); });
  malformed('constructor metadata arity disagrees', expr => { head(expr).value0.meta.value0.value1=['value0']; });
  malformed('local shadow is not a qualified constructor', expr => { head(expr).value1.value0=Nothing.value; });
  malformed('unregistered qualified owner', expr => { head(expr).value1.value0=new Just('OtherModule'); });
  malformed('unregistered constructor name', expr => { head(expr).value1.value1='OtherCtor'; });
  yes(recognize(original.value1) instanceof Nothing, 'partial constructor invocation retained');
  const interleaved=clone(original);
  const firstApplication=interleaved.value1;
  const typeApplication=firstApplication.value1;
  assert.ok(typeApplication instanceof C.ExprTypeApp, 'real fixture contains an instantiated constructor head');
  const firstArgument=firstApplication.value2, secondArgument=interleaved.value2;
  // Move an erased type application between the two runtime arguments. Its
  // annotation follows the partial application; argument expressions stay put.
  firstApplication.value1=typeApplication.value1;
  typeApplication.value1=firstApplication;
  typeApplication.value0=clone(firstApplication.value0);
  interleaved.value1=typeApplication;
  const interleavedCall=recognize(interleaved);
  yes(interleavedCall instanceof Just, 'TypeApp between runtime applications still exposes saturation');
  yes(interleavedCall.value0.args[0]===firstArgument && interleavedCall.value0.args[1]===secondArgument,
    'interleaved TypeApp preserves both argument expressions in order');
  yes(recognize(new C.ExprApp(clone(original.value0),clone(original),clone(original.value2))) instanceof Nothing, 'overapplication retained');
  const withoutRegistry = Map.delete(Ord.ordString)('ConstructorTypeApp_Tuple')(config.constructors);
  yes(fromExpr(withoutRegistry)(currentMod)(original) instanceof Nothing, 'missing layout registry entry');
  const wrongRegistry = Map.insert(Ord.ordString)('ConstructorTypeApp_Tuple')(3)(config.constructors);
  yes(fromExpr(wrongRegistry)(currentMod)(original) instanceof Nothing, 'registry arity disagreement');
  const erased = clone(original);
  function eraseApps(value) {
    if (value instanceof C.ExprTypeApp) return eraseApps(value.value1);
    if (!value || typeof value!=='object') return value;
    for (const key of Object.keys(value)) value[key]=Array.isArray(value[key])?value[key].map(eraseApps):eraseApps(value[key]);
    return value;
  }
  yes(recognize(eraseApps(erased)) instanceof Nothing, 'ordinary constructor path remains distinct');
  const explicit = clone(original);
  const target=head(explicit);
  function replaceHead(value) {
    if (value===target) return new C.ExprConstructor(clone(target.value0),'Tuple','Tuple',['value0','value1']);
    if (value instanceof C.ExprApp || value instanceof C.ExprTypeApp) value.value1=replaceHead(value.value1);
    return value;
  }
  replaceHead(explicit);
  yes(recognize(explicit) instanceof Just, 'explicit constructor node uses current module registry');
  const malformedExplicit=clone(explicit);
  head(malformedExplicit).value3=['value0'];
  yes(recognize(malformedExplicit) instanceof Nothing, 'explicit constructor fields must agree with registry arity');
  yes(fromExpr(config.constructors)(Nothing.value)(explicit) instanceof Nothing, 'explicit node requires module context');
  const generate = source => Printer.printModule(CodeGen.translateModuleWithConstructorWrappers(config.wrappers)(config.constructors)(source));
  const generated=generate(core), oracle=generate(forceCurried(core));
  const importedFs=Printer.printModule(CodeGen.translateModule(config.constructors)(captured.get('ConstructorImported').core));
  const producerFs=Printer.printModule(CodeGen.translateOptimizedModuleWithAdts(config.wrappers)(config.native)
    (config.constructors)(config.producer.backend)(config.producer.core));
  const line = (text,name) => text.split('\n').find(line=>line.startsWith(`let ConstructorTypeApp_${name} `));
  yes(line(generated,'pair').includes('ConstructorTypeApp_Tupleusd_Ctor'), 'generated saturated DU construction');
  yes(!line(generated,'pair').includes('sharpurs_apply'), 'no generic dispatch in plain saturated pair body');
  yes(line(oracle,'pair').includes('sharpurs_apply'), 'oracle retains generated curried dispatch');
  yes(line(generated,'partialPair')===line(oracle,'partialPair'), 'partial constructor binding unchanged');
  yes(line(generated,'ordinaryCall')===line(oracle,'ordinaryCall'), 'ordinary TypeApp call unchanged');
  yes(line(generated,'nativePair').includes('ConstructorNative_Node_adt_native'), 'native factory interop retained inside polymorphic constructor');
  if (process.env.CONSTRUCTOR_TYPEAPP_ORACLE_OUTPUT) {
    const old=await oldCompiler(resolve(process.env.CONSTRUCTOR_TYPEAPP_ORACLE_OUTPUT));
    const oldState=await collect(old,directory), oldConfig=configuration(old,oldState);
    const actualOld=old.Printer.printModule(old.CodeGen.translateModuleWithConstructorWrappers(oldConfig.wrappers)(oldConfig.constructors)(oldState.get('ConstructorTypeApp').core));
    yes(actualOld===oracle, 'metadata-disabled oracle is byte-identical to actual pre-change generator');
    if (artifacts) { await mkdir(artifacts,{recursive:true}); await writeFile(join(artifacts,'actual-before.fs'),actualOld); }
  }
  const js=await import(pathToFileURL(join(directory,'output/ConstructorTypeApp/index.js')));
  for (const value of [-2147483648,-1,0,1,2147483647]) {
    yes(js.boxInt(value).value0===value,'JS Box payload');
    yes(js.importedBox(value).value0===value,'JS imported polymorphic constructor payload');
    yes(js.pair(value)('typed').value0===value && js.pair(value)('typed').value1==='typed','JS Tuple payload order');
    const list=js.list(value), next=Number(BigInt.asIntN(32,BigInt(value)+1n));
    yes(list.value0===value && list.value1.value0===next,'JS List wrapping and field order');
  }
  const main=await readFile(join(backend,'src/Main.purs'),'utf8');
  const preludeFs=main.match(/^fsPrelude = """\r?\n([\s\S]*?)^"""/m)?.[1];
  assert.ok(preludeFs);
  const support=`
let (|LitInt|_|) (expected: int) (value: obj) = if value :? int && unbox<int> value = expected then Some() else None
let (|LitBool|_|) (expected: bool) (value: obj) = if value :? bool && unbox<bool> value = expected then Some() else None
let events = ResizeArray<int>()
let sentinel = InvalidOperationException("constructor argument")
let ConstructorTypeApp_track : obj = box (fun (label: obj) -> box (fun (value: obj) -> events.Add(unbox<int> label); value))
let ConstructorTypeApp_explode : obj = box (fun (label: obj) -> events.Add(unbox<int> label); raise sentinel : obj)
`;
  const scoped=(name,body)=>`module ${name} =\n${body.split('\n').map(line=>`    ${line}`).join('\n')}\n`;
  const runtime=`
let mutable checks = 0
let check label condition = if not condition then failwith label else checks <- checks + 1
let apply = sharpurs_apply
let call2 fn a b = apply (apply fn a) b
let pairNative value = match unbox<Native.ConstructorTypeApp_Tuple> value with Native.ConstructorTypeApp_Tupleusd_Ctor (a,b) -> a,b
let pairOracle value = match unbox<Oracle.ConstructorTypeApp_Tuple> value with Oracle.ConstructorTypeApp_Tupleusd_Ctor (a,b) -> a,b
let rec listNative value =
    match unbox<Native.ConstructorTypeApp_List> value with
    | Native.ConstructorTypeApp_Nilusd_Ctor -> []
    | Native.ConstructorTypeApp_Consusd_Ctor (a,tail) -> unbox<int> a :: listNative tail
let rec listOracle value =
    match unbox<Oracle.ConstructorTypeApp_List> value with
    | Oracle.ConstructorTypeApp_Nilusd_Ctor -> []
    | Oracle.ConstructorTypeApp_Consusd_Ctor (a,tail) -> unbox<int> a :: listOracle tail
for value in [System.Int32.MinValue; -1; 0; 1; System.Int32.MaxValue] do
    match unbox<ConstructorImported_Envelope> (apply Native.ConstructorTypeApp_importedBox (box value)) with
    | ConstructorImported_Envelopeusd_Ctor payload -> check "imported polymorphic payload" (unbox<int> payload=value)
    match unbox<ConstructorImported_Envelope> (apply Oracle.ConstructorTypeApp_importedBox (box value)) with
    | ConstructorImported_Envelopeusd_Ctor payload -> check "imported polymorphic oracle payload" (unbox<int> payload=value)
    match unbox<Native.ConstructorTypeApp_Box> (apply Native.ConstructorTypeApp_boxInt (box value)) with
    | Native.ConstructorTypeApp_Boxusd_Ctor payload -> check "single payload" (unbox<int> payload=value)
    match unbox<Oracle.ConstructorTypeApp_Box> (apply Oracle.ConstructorTypeApp_boxInt (box value)) with
    | Oracle.ConstructorTypeApp_Boxusd_Ctor payload -> check "single payload oracle" (unbox<int> payload=value)
    for text in [""; "alpha"; "☃"] do
        for native,baseline in [(Native.ConstructorTypeApp_pair,Oracle.ConstructorTypeApp_pair);(Native.ConstructorTypeApp_polyPair,Oracle.ConstructorTypeApp_polyPair);(Native.ConstructorTypeApp_explicitPair,Oracle.ConstructorTypeApp_explicitPair)] do
            let a,b=pairNative (call2 native (box value) (box text))
            let c,d=pairOracle (call2 baseline (box value) (box text))
            check "two type arguments preserve payloads" (unbox<int> a=value && unbox<string> b=text && a=c && b=d)
        let partial=apply Native.ConstructorTypeApp_partialPair (box value)
        for suffix in [text; text+"!"] do
            let a,b=pairNative (apply partial (box suffix))
            check "partial constructor reused" (unbox<int> a=value && unbox<string> b=suffix)
    match unbox<Native.ConstructorTypeApp_Maybe> (apply Native.ConstructorTypeApp_justInt (box value)) with
    | Native.ConstructorTypeApp_Justusd_Ctor a -> check "Maybe payload" (unbox<int> a=value)
    | _ -> failwith "Expected Just"
    check "List constructor/order/Int32 oracle" (listNative (apply Native.ConstructorTypeApp_list (box value))=listOracle (apply Oracle.ConstructorTypeApp_list (box value)))
    let tail=apply Native.ConstructorTypeApp_list (box value)
    match unbox<Native.ConstructorTypeApp_List> (call2 Native.ConstructorTypeApp_prepend (box 91) tail) with
    | Native.ConstructorTypeApp_Consusd_Ctor (a,b) -> check "recursive payload shares source tail" (unbox<int> a=91 && Object.ReferenceEquals(b,tail))
    | _ -> failwith "Expected Cons"
    check "ordinary polymorphic function unaffected" (call2 Native.ConstructorTypeApp_ordinaryCall (box value) (box 99) |> unbox<int> = value)
match unbox<Native.ConstructorTypeApp_Maybe> Native.ConstructorTypeApp_nothingInt with
| Native.ConstructorTypeApp_Nothingusd_Ctor -> check "nullary constructor remains callable value" true
| _ -> failwith "Expected Nothing"
for fn,decode in [(Native.ConstructorTypeApp_ordered,pairNative);(Oracle.ConstructorTypeApp_ordered,pairOracle);(Native.ConstructorTypeApp_capturedArgument,pairNative);(Oracle.ConstructorTypeApp_capturedArgument,pairOracle)] do
    events.Clear()
    let partial=apply fn (box 7)
    let beforeSecond=List.ofSeq events
    let a,b=decode (apply partial (box 11))
    check "arguments observed once in order" (List.ofSeq events=[1;2] && unbox<int> a=7 && unbox<int> b=11)
    if Object.ReferenceEquals(fn,Native.ConstructorTypeApp_capturedArgument) || Object.ReferenceEquals(fn,Oracle.ConstructorTypeApp_capturedArgument) then
        check "partial captures first argument eagerly" (beforeSecond=[1])
let failure action =
    let caught = try action() |> ignore; None with error -> Some error
    let rec unwrap (error: exn) depth =
        match error with
        | :? System.Reflection.TargetInvocationException as e when not (isNull e.InnerException) -> unwrap e.InnerException (depth+1)
        | e -> e,depth
    match caught with
    | None -> failwith "Expected argument exception"
    | Some e -> unwrap e 0
for native,baseline,expectedEvents in [(Native.ConstructorTypeApp_throwFirst,Oracle.ConstructorTypeApp_throwFirst,[1]);(Native.ConstructorTypeApp_throwSecond,Oracle.ConstructorTypeApp_throwSecond,[1;2])] do
    events.Clear()
    let cause,depth=failure(fun () -> apply native (box 37))
    check "exception argument order" (List.ofSeq events=expectedEvents)
    events.Clear()
    let oldCause,oldDepth=failure(fun () -> apply baseline (box 37))
    check "original exception identity" (Object.ReferenceEquals(cause,sentinel) && Object.ReferenceEquals(oldCause,sentinel))
    check "unchanged exception wrapping depth" (depth=oldDepth && depth=2)
    check "oracle exception argument order" (List.ofSeq events=expectedEvents)
let tail=ConstructorNative_Node_adt_native 13 ConstructorNative_Leaf_adt_native
let boxedTail=box tail
let a,b=pairNative(call2 Native.ConstructorTypeApp_nativePair (box 17) boxedTail)
check "native factory payload type" (a :? ConstructorNative_Tree)
check "native value payload identity" (Object.ReferenceEquals(b,boxedTail))
match unbox<ConstructorNative_Tree> a with
| ConstructorNative_Nodeusd_Ctor (value,child) -> check "native factory field values" (value=17 && Object.ReferenceEquals(child,tail))
| _ -> failwith "Expected native Node"
printfn "constructor-typeapp runtime: %d checks passed" checks
`;
  const script=['open System',preludeFs,support,producerFs,importedFs,scoped('Native',generated),scoped('Oracle',oracle),runtime].join('\n\n');
  await writeFile(join(directory,'constructor-typeapp.fsx'),script);
  if(artifacts) {
    await mkdir(artifacts,{recursive:true});
    for(const [name,text] of [['generated.fs',generated],['oracle.fs',oracle],['producer.fs',producerFs],['imported.fs',importedFs],['constructor-typeapp.fsx',script]]) await writeFile(join(artifacts,name),text);
    for(const name of moduleNames) await writeFile(join(artifacts,`${name}.corefn.json`),await readFile(join(directory,`output/${name}/corefn.json`)));
  }
  const runtimeResult=command(process.env.DOTNET || 'dotnet',['fsi','--nologo','--optimize+','--exec','constructor-typeapp.fsx'],directory);
  assert.doesNotMatch(runtimeResult.stderr,/warning FS/,'fixture has no F# warning');
  assert.match(runtimeResult.stdout,/constructor-typeapp runtime: \d+ checks passed/);
  console.log(runtimeResult.stdout.trim());
  const summary=`constructor-typeapp converter/JS: ${checks} checks passed`;
  transcript.push(summary);
  console.log(summary);
} finally {
  process.chdir(previousCwd);
  if(artifacts){await mkdir(artifacts,{recursive:true});await writeFile(join(artifacts,'validation.log'),transcript.join('\n'));}
  await rm(directory,{recursive:true,force:true});
}
