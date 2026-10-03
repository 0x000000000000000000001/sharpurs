// Real fork TAST and PBO, checked against a generic F# oracle and generated JS.
import assert from 'node:assert/strict';
import { mkdtemp, mkdir, readFile, rm, writeFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { dirname, join, resolve } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { compileFixtures, copyFixtures, packageSource, runFsharp } from './support/fixtures.mjs';
import { optimizeCoreFn } from './support/corefn.mjs';
import { clone, annotation, sourceBinding, sourceLambda, sourceLambdas, sourceApplication,
  sourceTypeApp, optimizedBinding, optimizedTyped, findNodes } from './support/ast.mjs';
import { fsharpFixture } from './support/fsharp.mjs';
import { createHash } from 'node:crypto';
import { helpers as preludeFs } from '../output/Sharpurs.Runtime/index.js';
import * as C from '../output/PureScript.Backend.Optimizer.CoreFn/index.js';
import * as S from '../output/PureScript.Backend.Optimizer.Syntax/index.js';
import { Just, Nothing } from '../output/Data.Maybe/index.js';
import * as Map from '../output/Data.Map.Internal/index.js';
import * as Set from '../output/Data.Set/index.js';
import { prepareModule, fromExpr } from '../output/Sharpurs.ThunkKernel/index.js';
import { translateModule, translateOptimizedModuleWithThunks } from '../output/Sharpurs.CodeGen/index.js';
import { printModule } from '../output/Sharpurs.Printer/index.js';

const backend = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const artifacts = process.env.THUNK_KERNEL_ARTIFACTS && resolve(process.env.THUNK_KERNEL_ARTIFACTS);
const transcript = [];
const selections = (plan, tree) => findNodes(tree, node => node instanceof C.ExprApp && fromExpr(plan)(node) instanceof Just);
function forceCall(expr) {
  const call = sourceApplication(expr, 1, 'force call');
  const seed = call.args[0];
  const worker = sourceApplication(seed, 2, 'chain seed');
  return { call, seed, worker, helper: sourceTypeApp(call.head), suffix: worker.nodes[0].node };
}
const directory = await mkdtemp(join(tmpdir(), 'sharpurs-thunk-kernel-'));
try {
  const prelude = await packageSource(backend, 'prelude');
  const partial = await packageSource(backend, 'partial');
  const fixtureFiles = ['TypedThunks.purs', 'ThunkExternal.purs', 'ThunkExternal.js'];
  await copyFixtures(join(backend, 'tests/fixtures/thunk-kernel'), directory, fixtureFiles);
  const compiled = compileFixtures(directory, [join(directory, '*.purs'), join(prelude, '**/*.purs'), join(partial, '**/*.purs')], { transcript });
  assert.doesNotMatch(compiled.stdout + compiled.stderr, /Warning \d+ of/, 'fixture has no PureScript warning');
  const captured = await optimizeCoreFn(directory, ['TypedThunks', 'ThunkExternal']);
  const state = captured.get('TypedThunks');
  assert.ok(state && captured.size === 2, 'real source modules parsed and optimized');
  const selected = prepareModule(state.core)(state.backend);
  assert.ok(selected instanceof Just, 'typed thunk workers found');
  const plan = selected.value0;
  const binding = name => sourceBinding(state.core, name).node;
  const generate = candidate => printModule(translateOptimizedModuleWithThunks(Set.empty)(Nothing.value)(candidate)(Map.empty)(state.backend)(state.core));
  const generated = generate(selected), oracle = generate(Nothing.value);
  let checks = 0;
  const yes = (condition, label) => { assert.ok(condition, label); checks++; };
  for (const name of ['run', 'runLiteral', 'runTwo', 'runBy', 'runCaptureCollision']) {
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
    const expr = clone(original); edit(expr, forceCall(expr));
    assert.notDeepEqual(expr, original, `${label}: mutation changed its target`);
    yes(fromExpr(plan)(expr) instanceof Nothing, label);
  };
  reject('missing call type', expr => { annotation(expr).type = Nothing.value; });
  reject('contradictory call type', expr => { annotation(expr).type = new Just(C.Number.value); });
  reject('missing argument type', (_, p) => { annotation(p.seed).type = Nothing.value; });
  reject('function argument type does not prove closure provenance', (_, p) => {
    const ann = clone(annotation(p.seed));
    p.call.nodes[0].argument = new C.ExprVar(ann, new C.Qualified(Nothing.value, 'unknownSeed'));
  });
  yes(forceCall(original).call.head instanceof C.ExprTypeApp, 'real TAST v3 instantiates force at Int');
  reject('wrong helper TypeApp', (_, p) => { p.helper.argument = C.Number.value; });
  reject('contradictory polymorphic helper type', (_, p) => { annotation(p.helper.expression).type = new Just(C.Int.value); });
  reject('missing worker suffix type', (_, p) => { annotation(p.suffix).type = Nothing.value; });
  reject('wrong worker suffix type', (_, p) => { annotation(p.suffix).type = new Just(C.Int.value); });
  reject('partial worker is not a thunk', (_, p) => { p.call.nodes[0].argument = p.suffix; });
  reject('extra worker argument', (_, p) => { p.call.nodes[0].argument = new C.ExprApp(annotation(p.seed), p.seed, p.worker.args[1]); });
  const rejectWorker = (label, edit) => {
    const mutated = clone(state); edit(mutated);
    assert.notDeepEqual(mutated, state, `${label}: mutation changed its target`);
    const result = prepareModule(mutated.core)(mutated.backend);
    const sites = result instanceof Just ? selections(result.value0, sourceBinding(mutated.core, 'run').node) : [];
    yes(sites.length === 0, label);
  };
  const source = st => sourceLambdas(sourceBinding(st.core, 'chain').expression, 2, 'chain');
  rejectWorker('missing source function type', st => { annotation(source(st).lambdas[0].node).type = Nothing.value; });
  rejectWorker('contradictory source function type', st => { annotation(source(st).lambdas[0].node).type = new Just(C.Int.value); });
  rejectWorker('returned Unit is not a source argument', st => { const fn = source(st); fn.lambdas[0].body = fn.lambdas[1].body; });
  rejectWorker('missing inner source lambda type', st => { annotation(source(st).lambdas[1].node).type = Nothing.value; });
  rejectWorker('contradictory optimized signature', st => {
    optimizedTyped(optimizedBinding(st.backend, 'chain').expression).type = C.Int.value;
  });
  rejectWorker('native helper name collision', st => {
    const workerName = generated.match(/let rec (?:private )?(TypedThunks_chain_thunk_native)\b/)?.[1];
    assert.ok(workerName, 'native worker declaration found');
    const extra = clone(sourceBinding(st.core, 'runLiteral').node);
    extra.value1 = workerName.replace(/^TypedThunks_/, '');
    st.core.decls.push(new C.NonRec(extra));
  });
  rejectWorker('native worker cannot shadow a source-local binder', st => {
    sourceLambda(sourceBinding(st.core, 'run').expression).parameter = 'TypedThunks_chain_thunk_native';
  });
  const forceSource = st => sourceLambda(sourceBinding(st.core, 'force').expression);
  rejectWorker('helper needs source type evidence', st => { annotation(forceSource(st).node).type = Nothing.value; });
  rejectWorker('helper checks nested optimized annotations', st => {
    const typed = optimizedTyped(optimizedBinding(st.backend, 'force').expression);
    typed.expression = new S.Typed(C.Int.value, typed.expression);
  });
  rejectWorker('optimized helper shape cannot erase an opaque source dependency', st => {
    const fn = forceSource(st);
    fn.body = new C.ExprVar(clone(annotation(fn.body)), new C.Qualified(new Just('ThunkExternal'), 'track'));
  });

  const js = await import(pathToFileURL(join(directory, 'output/TypedThunks/index.js')));
  const cases = [];
  for (const depth of [0, 1, 3, 37, 1000]) for (const seed of [-2147483648, -1, 0, 17, 2147483647]) {
    const expected = Number(BigInt.asIntN(32, BigInt(depth) + BigInt(seed)));
    yes(js.run(depth)(seed) === expected, 'JS result agrees with Int32 oracle');
    cases.push([depth, seed, expected]);
  }
  const captureCases = [[0, 7, 23], [3, -5, 11], [37, 2147483647, 5]].map(([depth, left, right]) => {
    const wrap = value => Number(BigInt.asIntN(32, value));
    const two = wrap(2n * BigInt(depth) + BigInt(left) + BigInt(right));
    const captured = wrap(BigInt(depth) + BigInt(left) + BigInt(right));
    yes(js.runTwo(depth)(left)(right) === two, 'JS independent seeds agree with Int32 oracle');
    yes(js.runCaptureCollision(depth)(left)(right) === captured, 'JS multiple captures agree with Int32 oracle');
    return [depth, left, right, two, captured];
  });
  const support = await fsharpFixture('thunk-kernel/Support.fs');
  const external = printModule(translateModule(Map.empty)(captured.get('ThunkExternal').core));
  const scoped = (name, body) => `module ${name} =\n${body.split('\n').map(line => `    ${line}`).join('\n')}\n`;
  const runtime = await fsharpFixture('thunk-kernel/Runtime.fs', {
    CASES: cases.map(row => `(${row.join(', ')})`).join('; '),
    CAPTURE_CASES: captureCases.map(row => `(${row.join(', ')})`).join('; '),
  });
  const script = [preludeFs, support, external, scoped('Native', generated), scoped('Oracle', oracle), runtime].join('\n\n');
  await writeFile(join(directory, 'thunk-kernel.fsx'), script);
  if (artifacts) {
    await mkdir(artifacts, { recursive: true });
    for (const [file, text] of [['generated.fs', generated], ['oracle.fs', oracle], ['thunk-kernel.fsx', script]]) await writeFile(join(artifacts, file), text);
    await writeFile(join(artifacts, 'corefn.json'), await readFile(join(directory, 'output/TypedThunks/corefn.json')));
  }
  const result = runFsharp(directory, 'thunk-kernel.fsx', { optimize: true, transcript });
  for (const warning of result.stderr.matchAll(/\((\d+),\d+\): warning (FS\d+):/g)) {
    yes(['FS0025', 'FS0040'].includes(warning[2]), 'only known generic partial/recursive fixture warnings');
    yes(!script.split('\n')[Number(warning[1])-1].includes('_thunk_native'), 'no native worker warnings');
  }
  assert.match(result.stdout, /thunk-kernel runtime: \d+ checks passed/);
  if (artifacts) {
    const sourceHashes = {};
    for (const file of ['src/Sharpurs/ThunkKernel.purs', 'tests/thunk-kernel.mjs',
      'tests/support/fixtures.mjs', 'tests/support/corefn.mjs', 'tests/support/ast.mjs', 'tests/support/fsharp.mjs',
      'tests/fixtures/thunk-kernel/Support.fs', 'tests/fixtures/thunk-kernel/Runtime.fs',
      ...['Analysis', 'Helpers', 'Lower', 'Call', 'Emit'].map(name => `src/Sharpurs/ThunkKernel/${name}.purs`),
      ...fixtureFiles.map(name => `tests/fixtures/thunk-kernel/${name}`)]) {
      sourceHashes[file] = createHash('sha256').update(await readFile(join(backend, file))).digest('hex');
    }
    await writeFile(join(artifacts, 'metadata.json'), JSON.stringify({ sourceHashes, selected: plan.nativeNames, converterChecks: checks }, null, 2) + '\n');
  }
  console.log(result.stdout.trim());
  console.log(`thunk-kernel converter/JS: ${checks} checks passed`);
} catch (error) {
  transcript.push(error.stack || String(error)); throw error;
} finally {
  if (artifacts) { await mkdir(artifacts, { recursive: true }); await writeFile(join(artifacts, 'validation.log'), transcript.join('\n')); }
  await rm(directory, { recursive: true, force: true });
}
