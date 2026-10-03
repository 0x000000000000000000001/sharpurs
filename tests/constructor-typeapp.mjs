// Real fork TAST -> production codegen, compared with its unchanged curried
// fallback. CONSTRUCTOR_TYPEAPP_ORACLE_OUTPUT optionally verifies that oracle
// against a complete pre-change compiled output directory.
import assert from 'node:assert/strict';
import { mkdtemp, mkdir, readFile, readdir, rm, writeFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { dirname, join, resolve } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { spawnSync } from 'node:child_process';
import { clone, annotation, sourceBinding, sourceApplication, sourceTypeApp,
  sourceVariable, findNodes, expectNode } from './support/ast.mjs';
import { fsharpFixture } from './support/fsharp.mjs';
import { helpers as preludeFs } from '../output/Sharpurs.Runtime/index.js';
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
// Constructor recognition deliberately crosses erased TypeApp boundaries.
// Keep this choice here rather than making the shared application view erase them.
function head(expr) {
  while (expr instanceof C.ExprApp || expr instanceof C.ExprTypeApp) expr = expr.value1;
  annotation(expr);
  return expr;
}
// Metadata is not evaluated by the generic backend. Removing IsConstructor
// only under TypeApp defeats the new recognizer; the old path still uses the
// same constructor registry and emits the original curried calls. No function
// body, argument, type application or exception boundary is hand-reimplemented.
function forceCurried(core) {
  const result = clone(core);
  for (const node of findNodes(result, node => node instanceof C.ExprTypeApp)) {
    const target = head(node);
    const ann = annotation(target);
    if (target instanceof C.ExprVar && ann.meta instanceof Just && ann.meta.value0 instanceof C.IsConstructor) {
      ann.meta = Nothing.value;
    }
  }
  return result;
}
async function collect(compiler, directory, builderDictionary = compiler.Aff.monadEffectAff) {
  const { C, Aff, Applicative, Either, Maybe, Map, Set, App, Builder, Foreign } = compiler;
  const pure = Applicative.pure(Aff.applicativeAff);
  const runAff = action => new Promise((resolve, reject) => Aff.runAff(result => () =>
    result instanceof Either.Left ? reject(result.value0) : resolve(result.value0))(action)());
  const captured = new globalThis.Map();
  await runAff(Builder.buildModules(builderDictionary)({
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
function configuration(compiler, captured, prepareAdts = compiler.Adt.prepareModule) {
  const { Map, Set, Ord, Maybe } = compiler;
  let constructors = Map.empty, wrappers = Set.empty;
  for (const { core } of captured.values()) for (const decl of core.dataDecls) for (const ctor of decl.constructors) {
    const name = `${core.name.replaceAll('.', '_')}_${ctor.name}`;
    constructors = Map.insert(Ord.ordString)(name)(ctor.fields.length)(constructors);
    if (core.name === 'ConstructorNative') wrappers = Set.insert(Ord.ordString)(name)(wrappers);
  }
  const producer = captured.get('ConstructorNative');
  const native = prepareAdts(producer.core)(producer.backend);
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
  const selections = tree => findNodes(tree, node => node instanceof C.ExprApp && recognize(node) instanceof Just);
  const binding = name => sourceBinding(core, name).node;
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
  const malformed = (label, edit) => {
    const expr = clone(original); edit(expr);
    assert.notDeepEqual(expr, original, `${label}: mutation changed its target`);
    yes(recognize(expr) instanceof Nothing, label);
  };
  malformed('missing constructor metadata', expr => { annotation(head(expr)).meta = Nothing.value; });
  malformed('newtype metadata is not ordinary constructor metadata', expr => { annotation(head(expr)).meta = new Just(C.IsNewtype.value); });
  malformed('constructor metadata arity disagrees', expr => {
    const meta = expectNode(annotation(head(expr)).meta, Just, 'constructor metadata').value0;
    expectNode(meta, C.IsConstructor).value1 = ['value0'];
  });
  malformed('local shadow is not a qualified constructor', expr => { sourceVariable(head(expr)).owner = Nothing.value; });
  malformed('unregistered qualified owner', expr => { sourceVariable(head(expr)).owner = new Just('OtherModule'); });
  malformed('unregistered constructor name', expr => { sourceVariable(head(expr)).name = 'OtherCtor'; });
  yes(recognize(sourceApplication(original, 2).nodes[1].fn) instanceof Nothing, 'partial constructor invocation retained');
  const interleaved=clone(original);
  const applied = sourceApplication(interleaved, 2, 'pair constructor');
  const firstApplication = applied.nodes[0];
  const typeApplication = sourceTypeApp(applied.head);
  assert.ok(typeApplication.node instanceof C.ExprTypeApp, 'real fixture contains an instantiated constructor head');
  const [firstArgument, secondArgument] = applied.args;
  // Move an erased type application between the two runtime arguments. Its
  // annotation follows the partial application; argument expressions stay put.
  firstApplication.fn = typeApplication.expression;
  typeApplication.expression = firstApplication.node;
  typeApplication.node.value0 = clone(annotation(firstApplication.node));
  applied.nodes[1].fn = typeApplication.node;
  const interleavedCall=recognize(interleaved);
  yes(interleavedCall instanceof Just, 'TypeApp between runtime applications still exposes saturation');
  yes(interleavedCall.value0.args[0]===firstArgument && interleavedCall.value0.args[1]===secondArgument,
    'interleaved TypeApp preserves both argument expressions in order');
  yes(recognize(new C.ExprApp(clone(annotation(original)), clone(original), clone(sourceApplication(original, 2).args[1]))) instanceof Nothing, 'overapplication retained');
  const withoutRegistry = Map.delete(Ord.ordString)('ConstructorTypeApp_Tuple')(config.constructors);
  yes(fromExpr(withoutRegistry)(currentMod)(original) instanceof Nothing, 'missing layout registry entry');
  const wrongRegistry = Map.insert(Ord.ordString)('ConstructorTypeApp_Tuple')(3)(config.constructors);
  yes(fromExpr(wrongRegistry)(currentMod)(original) instanceof Nothing, 'registry arity disagreement');
  const erased = clone(original, node => node instanceof C.ExprTypeApp ? sourceTypeApp(node).expression : node);
  yes(recognize(erased) instanceof Nothing, 'ordinary constructor path remains distinct');
  const explicit = clone(original);
  const target=head(explicit);
  function replaceHead(value) {
    if (value===target) return new C.ExprConstructor(clone(annotation(target)),'Tuple','Tuple',['value0','value1']);
    if (value instanceof C.ExprApp || value instanceof C.ExprTypeApp) value.value1=replaceHead(value.value1);
    return value;
  }
  replaceHead(explicit);
  yes(recognize(explicit) instanceof Just, 'explicit constructor node uses current module registry');
  const malformedExplicit=clone(explicit);
  expectNode(head(malformedExplicit), C.ExprConstructor, 'explicit constructor').value3 = ['value0'];
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
    // The historical constructor-call baseline predates Builder's MonadEffect constraint.
    const oldState=await collect(old,directory,old.Aff.monadAff), oldConfig=configuration(old,oldState,old.Adt.prepareUnary);
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
  const support = await fsharpFixture('constructor-typeapp/Support.fs');
  const scoped=(name,body)=>`module ${name} =\n${body.split('\n').map(line=>`    ${line}`).join('\n')}\n`;
  const runtime = await fsharpFixture('constructor-typeapp/Runtime.fs');
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
