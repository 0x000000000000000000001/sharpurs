// Real TAST -> CLI -> MSBuild, across native-file/module removal and a clean
// regeneration. Invalid leftovers make accidental directory discovery fail.
import assert from 'node:assert/strict';
import { cp, mkdir, mkdtemp, readFile, readdir, rm, writeFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { spawnSync } from 'node:child_process';
import { fileManifest, compareManifests } from '../scripts/support/generation.mjs';
import { command } from './support/fixtures.mjs';
import { checkProjectIO } from './support/project-io.mjs';

const backend = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const fixtures = join(backend, 'tests/fixtures/project');
const compiler = process.env.PROJECT_COMPILER || join(backend, 'bin/sharpurs.js');
const artifacts = process.env.PROJECT_ARTIFACTS && resolve(process.env.PROJECT_ARTIFACTS);
const directory = await mkdtemp(join(tmpdir(), 'sharpurs-project-'));
const output = join(directory, 'output/Main');
const transcript = [], phases = [];
const cliArgs = ['--stack-size=65536', compiler];
let io;
const includes = text => [...text.matchAll(/<Compile Include="([^"]+)"/g)].map(match => match[1]);
const text = name => readFile(join(output, name), 'utf8');
const manifest = async files => Object.fromEntries(Object.entries(await fileManifest(output)).filter(([name]) => files.includes(name)));
const equalFiles = (before, after, timestamps = true) => {
  const diff = compareManifests(before, after, { timestamps });
  assert.deepEqual(diff.added, [], 'no unexpected outputs');
  assert.deepEqual(diff.removed, [], 'no missing outputs');
  assert.deepEqual(diff.changed, [], 'complete contents preserved');
  if (timestamps) assert.deepEqual(diff.rewrittenUnchanged, [], 'unchanged outputs keep exact timestamps');
};
async function generate(label, csharp, { removed = false, incremental = false } = {}) {
  const inputs = await manifest(['corefn.json', 'Application.notes', 'Leftover.cs', 'Leftover.fs']);
  const expected = ['Sharpurs_Prelude.fs', 'Project.Zebra.fs', 'Project.Alpha.fs', 'Main.fs', 'EntryPoint.fs',
    'Program.fsproj', 'Directory.Build.props', ...csharp, ...(removed ? [] : ['Project.Removed.fs']),
    ...(csharp.length ? ['FFI.CSharp.csproj'] : [])];
  const before = incremental && await manifest(expected);
  command(process.execPath, cliArgs, directory, { transcript });
  equalFiles(inputs, await manifest(Object.keys(inputs)));
  const fs = includes(await text('Program.fsproj'));
  assert.deepEqual([...fs].sort(), expected.filter(name => name.endsWith('.fs')).sort());
  assert.equal(fs[0], 'Sharpurs_Prelude.fs');
  assert.equal(fs.at(-1), 'EntryPoint.fs');
  assert.ok(fs.indexOf('Project.Zebra.fs') < fs.indexOf('Project.Alpha.fs'), 'dependency precedes its alphabetically earlier consumer');
  assert.ok(fs.indexOf('Project.Alpha.fs') < fs.indexOf('Main.fs'), 'entry module follows its dependency');
  const project = await text('Program.fsproj');
  assert.ok(project.includes(await readFile(join(directory, 'sharp.packages.props'), 'utf8')), 'explicit application references preserved verbatim');
  if (csharp.length) {
    assert.deepEqual(includes(await text('FFI.CSharp.csproj')), csharp, 'only current native sources, in stable order');
    assert.match(project, /<ProjectReference Include="FFI.CSharp.csproj"/);
  } else {
    assert.doesNotMatch(project, /FFI.CSharp.csproj/);
    assert.ok(!(await readdir(output)).includes('FFI.CSharp.csproj'), 'obsolete generator-owned C# project removed');
  }
  const after = await manifest(expected);
  if (before) equalFiles(before, after);
  const result = command(process.env.DOTNET || 'dotnet', ['run', '-c', 'Release', '--nologo', '--project', 'output/Main/Program.fsproj'], directory, { transcript });
  assert.match(result.stdout, /^project runtime: 42$/m);
  assert.doesNotMatch(result.stdout + result.stderr, /warning (?:FS|CS)\d+/);
  phases.push({ label, fsharp: fs, csharp, files: after, inputsPreserved: true, runtime: 42 });
  if (artifacts) {
    await mkdir(join(artifacts, label), { recursive: true });
    for (const name of expected) await cp(join(output, name), join(artifacts, label, name));
  }
  return after;
}
try {
  await mkdir(join(directory, 'src'));
  for (const name of ['Main.purs', 'Main.fs', 'Zebra.purs', 'Zebra.cs', 'Alpha.purs', 'Alpha.cs', 'Removed.purs', 'Removed.cs']) {
    await cp(join(fixtures, name), join(directory, 'src', name));
  }
  await cp(join(fixtures, 'application'), join(directory, 'application'), { recursive: true });
  await cp(join(fixtures, 'sharp.packages.props'), join(directory, 'sharp.packages.props'));
  await writeFile(join(directory, 'package.json'), '{"type":"module"}\n');
  command(process.env.PURS || 'purs', ['compile', 'src/*.purs', '--codegen', 'corefn'], directory, { transcript });
  await writeFile(join(output, 'Leftover.cs'), 'invalid stale C#; must never be compiled\n');
  await writeFile(join(output, 'Leftover.fs'), 'invalid stale F#; must never be compiled\n');
  await writeFile(join(output, 'Application.notes'), 'application-owned input\n');
  const mixed = ['Project.Alpha.cs', 'Project.Removed.cs', 'Project.Zebra.cs'];
  await generate('mixed', mixed);
  await generate('mixed-incremental', mixed, { incremental: true });

  // Remove a module from the TAST graph, then replace one C# FFI with F#.
  // Its previously emitted sources deliberately remain in output/Main.
  const stale = await manifest(['Project.Removed.fs', 'Project.Removed.cs', 'Project.Zebra.cs']);
  for (const name of ['Removed.purs', 'Removed.cs', 'Zebra.cs']) await rm(join(directory, 'src', name));
  await rm(join(directory, 'output/Project.Removed'), { recursive: true });
  await cp(join(fixtures, 'fsharp/Zebra.fs'), join(directory, 'src/Zebra.fs'));
  await generate('removed-module-and-ffi', ['Project.Alpha.cs'], { removed: true });
  equalFiles(stale, await manifest(Object.keys(stale)));
  await generate('removed-incremental', ['Project.Alpha.cs'], { removed: true, incremental: true });

  await rm(join(directory, 'src/Alpha.cs'));
  await cp(join(fixtures, 'fsharp/Alpha.fs'), join(directory, 'src/Alpha.fs'));
  await generate('fsharp-only', [], { removed: true });
  const current = await generate('fsharp-incremental', [], { removed: true, incremental: true });
  // The test owns these specific generated files. Main/corefn.json and the
  // application inputs stay in place throughout this clean regeneration.
  for (const name of [...Object.keys(current), ...Object.keys(stale), 'Project.Alpha.cs']) await rm(join(output, name));
  const clean = await generate('fsharp-clean', [], { removed: true });
  equalFiles(current, clean, false);

  // Optional references are absent only on ENOENT. A real CLI process must fail
  // on an unreadable fragment and name it, including Node's path-less EISDIR.
  await rm(join(directory, 'sharp.packages.props'));
  await mkdir(join(directory, 'sharp.packages.props'));
  const failed = spawnSync(process.execPath, cliArgs, { cwd: directory, encoding: 'utf8', timeout: 60_000 });
  transcript.push(`CLI invalid reference fragment:\n${failed.stdout}${failed.stderr}`);
  assert.ifError(failed.error);
  assert.notEqual(failed.status, 0);
  assert.match(failed.stderr, /EISDIR/);
  assert.match(failed.stderr, /sharp\.packages\.props/);

  io = await checkProjectIO(join(directory, 'io'));
  console.log(`project: ${phases.length} CLI/build/runtime phases, clean equality, input/timestamp preservation, ${io.errors.length} characterized I/O failures`);
} catch (error) {
  transcript.push(error.stack || String(error));
  throw error;
} finally {
  if (artifacts) {
    await mkdir(artifacts, { recursive: true });
    await writeFile(join(artifacts, 'results.json'), JSON.stringify({ phases, io }, null, 2) + '\n');
    await writeFile(join(artifacts, 'validation.log'), transcript.join('\n'));
  }
  await rm(directory, { recursive: true, force: true });
}
