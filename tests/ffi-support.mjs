// Production FFI adapters, resolver and project writer -> compiled F#/C#.
// FFI_SUPPORT_ORACLE optionally compares complete wrapper text with an older
// FfiSupport.js snapshot. FFI_SUPPORT_ARTIFACTS retains the project and report.
import assert from 'node:assert/strict';
import { createHash } from 'node:crypto';
import { cp, mkdir, readFile, writeFile } from 'node:fs/promises';
import { dirname, join, resolve } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { command, withFixtureDirectory } from './support/fixtures.mjs';
import { runAff } from './support/corefn.mjs';
import { Just, Nothing } from '../output/Data.Maybe/index.js';
import * as Map from '../output/Data.Map.Internal/index.js';
import * as C from '../output/PureScript.Backend.Optimizer.CoreFn/index.js';
import { appendFfiWrappers, appendCsFfiWrappers } from '../output/Sharpurs.FfiSupport/index.js';
import { loadModule } from '../output/Sharpurs.Ffi/index.js';
import * as Project from '../output/Sharpurs.Project/index.js';

const backend = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const fixtures = join(backend, 'tests/fixtures/ffi-support');
const artifacts = process.env.FFI_SUPPORT_ARTIFACTS && resolve(process.env.FFI_SUPPORT_ARTIFACTS);
const oracle = process.env.FFI_SUPPORT_ORACLE && await import(pathToFileURL(resolve(process.env.FFI_SUPPORT_ORACLE)));
const transcript = [];
let checks = 0, differentialChecks = 0;
const equal = (actual, expected, label) => { assert.deepEqual(actual, expected, label); checks++; };
const yes = (condition, label) => { assert.ok(condition, label); checks++; };
function wrappers(language, moduleName, required, content) {
  const result = (language === 'fs' ? appendFfiWrappers : appendCsFfiWrappers)(moduleName)(required)(content);
  if (oracle) {
    const old = language === 'fs' ? oracle.appendFfiWrappersImpl : oracle.appendCsFfiWrappersImpl;
    assert.equal(result, old(moduleName)(required)(content), `${language}: pre-change wrapper text is identical`);
    differentialChecks++;
  }
  return result;
}

// Exact public wrappers characterize value/function distinctions and the
// restricted declaration forms. Executable fixtures below verify their ABI.
const shapeCases = [
  ['fs', 'let work = 7', 'let Example_Native_work = box (Example_Native_FFI.``work``)'],
  ['fs', 'let work : int -> int = fun x -> x', 'let Example_Native_work = box (Example_Native_FFI.``work``)'],
  ['fs', 'let work (x: int) = x', 'let Example_Native_work = box (fun (arg0: obj) -> box (Example_Native_FFI.``work`` (unbox arg0)))'],
  ['fs', 'let rec ``work`` x y : int = x + y', 'let Example_Native_work = box (fun (arg0: obj) -> box (fun (arg1: obj) -> box (Example_Native_FFI.``work`` (unbox arg0) (unbox arg1))))'],
  ['fs', 'let work () = 7', 'let Example_Native_work = box (fun (arg0: obj) -> box (Example_Native_FFI.``work`` (unbox arg0)))'],
  ['fs', 'let work (x: int, y: int) = x + y', 'let Example_Native_work = box (fun (arg0: obj) -> box (Example_Native_FFI.``work`` (unbox arg0)))'],
  ['fs', 'let work\n    (x: int)\n    (y: int) = x + y', 'let Example_Native_work = box (fun (arg0: obj) -> box (fun (arg1: obj) -> box (Example_Native_FFI.``work`` (unbox arg0) (unbox arg1))))'],
  ['fs', 'let workExtra x y = x + y\nlet work = 7', 'let Example_Native_work = box (Example_Native_FFI.``work``)'],
  ['fs', 'let Work x y = x + y', 'let Example_Native_work = box (Example_Native_FFI.``work``)'],
  ['fs', 'let private work x y = x + y', 'let Example_Native_work = box (Example_Native_FFI.``work``)'],
  ['cs', 'public static int Work() => 7;', 'let Example_Native_work = box (Example.Native.FFI.Work())'],
  ['cs', 'public static int Work(int x, int y) => x + y;', 'let Example_Native_work = box (fun (arg0: obj) -> box (fun (arg1: obj) -> box (Example.Native.FFI.Work(unbox arg0, unbox arg1))))'],
  ['cs', 'public static int? WORK(int x) => x;', 'let Example_Native_work = box (fun (arg0: obj) -> box (Example.Native.FFI.WORK(unbox arg0)))'],
  ['cs', 'public static int[] work(int x) => new[] { x };', 'let Example_Native_work = box (fun (arg0: obj) -> box (Example.Native.FFI.work(unbox arg0)))'],
  ['cs', 'public static object WorkExtra(int x) => x;', 'let Example_Native_work = box (Example.Native.FFI.work())'],
  ['cs', 'public static int Work(\nint x,\nint y) => x + y;', 'let Example_Native_work = box (Example.Native.FFI.work())'],
];

for (const [language, source, expected] of shapeCases) {
  const output = wrappers(language, 'Example.Native', ['work'], source);
  equal(output.split('\n').find(line => line.startsWith('let Example_Native_work = ')), expected, `${language}: declaration form`);
}
equal(wrappers('cs', 'Example.Native', [], ''), '\n', 'empty C# export list');
equal(wrappers('fs', 'Example.Native', [], 'module Example.Native\n\nlet hidden = 3\n'),
  'module Example_Native_FFI =\n    let hidden = 3\n    \n\n\n', 'F# module nesting, indentation and trailing newline');

// Several arities, whitespace forms and module qualifiers exercise the optional
// differential oracle beyond the hand-written declaration fixtures.
if (oracle) for (const moduleName of ['Single', 'Example.Native', 'Under_score.Native']) {
  for (let arity = 0; arity <= 6; arity++) {
    const args = Array.from({ length: arity }, (_, i) => `p${i}`);
    for (const newline of ['\n', '\r\n']) {
      const fs = `module ${moduleName}${newline}let work ${args.map(arg => `(${arg}: int)`).join(' ')} = 0${newline}`;
      const cs = `public static int Work(${args.map(arg => `int ${arg}`).join(', ')}) => 0;${newline}`;
      wrappers('fs', moduleName, ['work', 'missing', 'work'], fs);
      wrappers('cs', moduleName, ['work', 'missing', 'work'], cs);
    }
  }
}

await withFixtureDirectory('sharpurs-ffi-support-', { artifacts, transcript }, async directory => {
  const sourceDirectory = join(directory, 'native');
  const overrideDirectory = join(directory, 'overrides/src/Fixture');
  await mkdir(sourceDirectory, { recursive: true });
  await mkdir(overrideDirectory, { recursive: true });
  await mkdir(join(directory, 'output'));
  const fixtureFiles = ['Fsharp.fs', 'Csharp.cs', 'Both.fs', 'Both.cs'];
  for (const file of fixtureFiles) await cp(join(fixtures, file), join(sourceDirectory, file));
  await writeFile(join(overrideDirectory, 'Fsharp.fs'), 'let value = -1000\n');
  await writeFile(join(overrideDirectory, 'Search.fs'), 'let value = 73\n');
  process.chdir(directory);

  const fn = new C.Func([C.Int.value, C.Int.value], C.Int.value);
  const missing = {
    binary: new Just(fn),
    quantified: new Just(new C.ForAll(['a'], fn)),
    constrained: new Just(new C.ConstrainedType([], fn)),
    applied: new Just(new C.TypeApp(fn, [C.Int.value])),
    unknown: Nothing.value,
    value: new Just(C.Int.value),
  };
  const names = {
    Fsharp: ['value', 'match', 'functionValue', 'unitValue', 'tupled', 'add', 'difference', 'countdown', 'effect', 'fail', 'delayedFailure'],
    Csharp: ['getValue', 'add', 'difference', 'nullable', 'pair', 'offset', 'effect', 'fail', 'delayedFailure'],
    Both: ['chosen'], Search: ['value'], Missing: Object.keys(missing), Empty: [],
  };
  const emitted = new globalThis.Map();
  const modules = [];
  await runAff(Project.prepare);
  for (const [shortName, required] of Object.entries(names)) {
    let foreign = Map.empty;
    for (const name of required) foreign = Map.insert(C.ordIdent)(name)(shortName === 'Missing' ? missing[name] : Nothing.value)(foreign);
    const source = { name: `Fixture.${shortName}`, path: join(sourceDirectory, `${shortName}.purs`), foreign };
    const ffi = await runAff(loadModule(new Just('overrides'))(source));
    emitted.set(shortName, ffi);
    const sorted = [...required].sort();
    if (['Fsharp', 'Both'].includes(shortName)) {
      const fs = await readFile(join(sourceDirectory, `${shortName}.fs`), 'utf8');
      equal(ffi.fsharp, wrappers('fs', source.name, sorted, fs) + '\n\n', 'source-relative F# owns the public wrappers');
    }
    if (shortName === 'Csharp') {
      const cs = await readFile(join(sourceDirectory, 'Csharp.cs'), 'utf8');
      equal(ffi.fsharp, wrappers('cs', source.name, sorted, cs) + '\n\n', 'C# adapter used when F# is absent');
    }
    if (['Csharp', 'Both'].includes(shortName)) {
      yes(ffi.csharp instanceof Just, 'C# implementation remains available');
      equal(ffi.csharp.value0, await readFile(join(sourceDirectory, `${shortName}.cs`), 'utf8'), 'C# source is preserved verbatim');
    } else yes(ffi.csharp instanceof Nothing, 'no C# source invented');
    modules.push(await runAff(Project.writeModule(source.name)(ffi)('')));
  }
  equal(emitted.get('Empty').fsharp, '', 'empty module needs no stubs');
  yes(emitted.get('Search').fsharp.includes('let value = 73'), 'configured directory supplies the native file');
  equal(await readFile(join(directory, 'output/Main/Fixture.Both.cs'), 'utf8'), await readFile(join(fixtures, 'Both.cs'), 'utf8'), 'Project writes C# even when F# owns the wrappers');

  modules.push(await runAff(Project.writeModule('Entry')({ fsharp: '', csharp: Nothing.value })(await readFile(join(backend, 'tests/ffi-support.fsx'), 'utf8'))));
  await runAff(Project.finalize({ mainModule: 'Entry', modules }));
  const result = command(process.env.DOTNET || 'dotnet', ['run', '-c', 'Release', '--nologo', '--project', 'output/Main/Program.fsproj'], directory,
    { transcript, timeout: 120_000 });
  assert.doesNotMatch(result.stdout + result.stderr, /warning FS\d+/, 'native FFI fixtures compile without F# warnings');
  const runtime = result.stdout.match(/ffi-support runtime: (\d+) checks passed/);
  assert.ok(runtime, 'compiled F#/C# runtime checks completed');
  if (artifacts) {
    await mkdir(artifacts, { recursive: true });
    await cp(join(directory, 'output/Main'), join(artifacts, 'generated'), { recursive: true });
    const sourceHashes = {};
    for (const file of ['src/Sharpurs/FfiSupport.js', 'src/Sharpurs/FfiSupport.purs', 'src/Sharpurs/Ffi.purs', 'tests/ffi-support.mjs', 'tests/ffi-support.fsx',
      ...fixtureFiles.map(file => `tests/fixtures/ffi-support/${file}`)]) {
      sourceHashes[file] = createHash('sha256').update(await readFile(join(backend, file))).digest('hex');
    }
    await writeFile(join(artifacts, 'metadata.json'), JSON.stringify({ sourceHashes, converterChecks: checks, runtimeChecks: Number(runtime[1]), differentialChecks }, null, 2) + '\n');
  }
  console.log(runtime[0]);
  console.log(`ffi-support converter/resolver: ${checks} checks passed`);
  if (oracle) console.log(`ffi-support differential: ${differentialChecks} complete wrapper outputs identical`);
});
