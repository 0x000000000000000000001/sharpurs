// Test the actual process boundary, including the production shell wrapper and
// real Spago invocation. No mock argv splitting or generated-source patching.
import assert from 'node:assert/strict';
import { cp, mkdir, mkdtemp, readFile, rm, writeFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { spawnSync } from 'node:child_process';
import { fileManifest, compareManifests } from '../scripts/support/generation.mjs';
import { command } from './support/fixtures.mjs';

const backend = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const fixtures = join(backend, 'tests/fixtures/cli');
const compiler = join(backend, 'bin/sharpurs.js');
const wrapper = join(backend, 'bin/sharpurs');
const oracle = process.env.CLI_ORACLE && resolve(process.env.CLI_ORACLE);
const artifacts = process.env.CLI_ARTIFACTS && resolve(process.env.CLI_ARTIFACTS);
const directory = await mkdtemp(join(tmpdir(), 'sharpurs-cli-'));
const quiet = join(directory, 'no project');
const workspace = join(directory, 'application with spaces');
const output = join(workspace, 'output/Main');
const ffi = 'native = sources é';
const transcript = [], diagnostics = [], runtimes = [], differentials = [];

function invoke(args, cwd, { binary = compiler, shell = false } = {}) {
  const executable = shell ? wrapper : process.execPath;
  const argv = shell ? args : ['--stack-size=65536', binary, ...args];
  const result = spawnSync(executable, argv, { cwd, encoding: 'utf8', timeout: 60_000 });
  transcript.push(`$ ${JSON.stringify([executable, ...argv])}\n${result.stdout || ''}${result.stderr || ''}`);
  assert.ifError(result.error);
  assert.equal(result.signal, null);
  return result;
}
function identical(before, after) {
  const diff = compareManifests(before, after);
  assert.deepEqual(diff.added, []);
  assert.deepEqual(diff.removed, []);
  assert.deepEqual(diff.changed, []);
  assert.deepEqual(diff.rewrittenUnchanged, []);
}
async function runtime(label, expected) {
  const result = command(process.env.DOTNET || 'dotnet', ['run', '-c', 'Release', '--nologo',
    '-p:NuGetAudit=false', '--project', 'output/Main/Program.fsproj'], workspace, { transcript, timeout: 120_000 });
  assert.ok(result.stdout.split(/\r?\n/).includes(`cli runtime: ${expected}`), label);
  assert.doesNotMatch(result.stdout + result.stderr, /warning (?:FS|CS)\d+/);
  runtimes.push({ label, expected });
}
async function generated() {
  // .NET owns bin/obj; compare every file directly emitted by the backend.
  return Object.fromEntries(Object.entries(await fileManifest(output)).filter(([name]) => !name.includes('/')));
}
try {
  await mkdir(quiet);
  await writeFile(join(quiet, 'application.txt'), 'preserve this input\n');
  const initial = await fileManifest(quiet);
  const errors = [
    { args: ['--main'], message: 'Missing value for --main.' },
    { args: ['--ffi'], message: 'Missing value for --ffi.' },
    { args: ['--main', '--help'], message: 'Missing value for --main.' },
    { args: ['--ffi', '--main', 'Main'], message: 'Missing value for --ffi.' },
    { args: ['--main', ''], message: 'Empty value for --main.' },
    { args: ['--ffi', ''], message: 'Empty value for --ffi.' },
    { args: ['--main='], message: 'Empty value for --main.' },
    { args: ['--ffi='], message: 'Empty value for --ffi.' },
    { args: ['--main', 'Main', '--main=Other'], message: 'Option --main may only be specified once.' },
    { args: ['--ffi=one', '--ffi', 'two'], message: 'Option --ffi may only be specified once.' },
    { args: ['--unknown'], message: 'Unknown option: "--unknown"' },
    { args: ['--unknown=value'], message: 'Unknown option: "--unknown"' },
    { args: ['--MAIN', 'Main'], message: 'Unknown option: "--MAIN"' },
    { args: ['Main'], message: 'Unexpected positional argument: "Main"' },
    { args: ['--', 'Main'], message: 'Unexpected positional argument: "Main"' },
    { args: ['--main Main --ffi native'], message: 'Unknown option: "--main Main --ffi native"' },
    { args: ['--help=1'], message: 'Option --help does not take a value.' },
    { args: ['-h=yes'], message: 'Option -h does not take a value.' },
    { args: ['--help', '--unknown'], message: 'Unknown option: "--unknown"' },
    ...['--bundle', '--output', '--rewrite-limit', '--autoload-path'].flatMap(flag => [
      { args: [flag], message: `Option ${flag} is not supported by Sharpurs; it was previously ignored.` },
      { args: [flag + '=ignored'], message: `Option ${flag} is not supported by Sharpurs; it was previously ignored.` },
    ]),
  ];
  for (const { args, message } of errors) {
    const result = invoke(args, quiet);
    assert.equal(result.status, 2, JSON.stringify(args));
    assert.equal(result.stdout, '');
    assert.equal(result.stderr, `sharpurs: ${message}\nRun sharpurs --help for usage.\n`);
    identical(initial, await fileManifest(quiet));
    diagnostics.push({ args, code: result.status, stderr: result.stderr });
  }
  let help;
  for (const args of [['--help'], ['-h'], ['--main', 'App.Entrée', '--ffi', ffi, '--help'], ['-h', '--help', '--']]) {
    const result = invoke(args, quiet, { shell: true });
    assert.equal(result.status, 0);
    assert.equal(result.stderr, '');
    assert.match(result.stdout, /^Usage: sharpurs/m);
    for (const flag of ['--main', '--ffi', '--help']) assert.ok(result.stdout.includes(flag));
    assert.ok(result.stdout.includes('default: Main'));
    help ??= result.stdout;
    assert.equal(result.stdout, help);
    identical(initial, await fileManifest(quiet));
    diagnostics.push({ args, code: result.status, help: true });
  }
  const noInput = invoke([], quiet);
  assert.equal(noInput.status, 1, 'generation errors are distinct from usage errors');
  assert.match(noInput.stderr, /ENOENT/);
  identical(initial, await fileManifest(quiet));

  await mkdir(join(workspace, 'src'), { recursive: true });
  for (const name of ['Main.purs', 'Main.fs', 'Entry.purs']) await cp(join(fixtures, name), join(workspace, 'src', name));
  for (const name of [ffi, 'plain-ffi', '-native']) {
    await mkdir(join(workspace, name));
    await cp(join(fixtures, 'Selected.fs'), join(workspace, name, 'App.Entrée.fs'));
  }
  await mkdir(join(workspace, 'native'));
  await cp(join(fixtures, 'Truncated.fs'), join(workspace, 'native/App.Entrée.fs'));
  command(process.env.PURS || 'purs', ['compile', 'src/*.purs', '--codegen', 'corefn'], workspace, { transcript });
  assert.equal(invoke([], workspace).status, 0);
  await runtime('default entrypoint', 'Main');
  assert.equal(invoke(['--'], workspace).status, 0);
  await runtime('empty positional terminator', 'Main');

  const ordinaryArgs = ['--main', 'App.Entrée', '--ffi', 'plain-ffi'];
  assert.equal(invoke(ordinaryArgs, workspace).status, 0);
  if (oracle) {
    const current = await generated();
    assert.equal(invoke(ordinaryArgs, workspace, { binary: oracle }).status, 0);
    identical(current, await generated());
    differentials.push({ scenario: 'existing separate --main/--ffi arguments', identical: true });
  }
  await runtime('ordinary explicit main and ffi', 'selected 42');

  const spacedArgs = ['--ffi', ffi, '--main', 'App.Entrée'];
  if (oracle) {
    assert.equal(invoke(spacedArgs, workspace, { binary: oracle }).status, 0);
    assert.match(await readFile(join(output, 'App.Entrée.fs'), 'utf8'), /CLI selected the truncated FFI path/);
    differentials.push({ scenario: 'H04 whitespace regression', truncatedFfiSelected: true });
  }
  assert.equal(invoke(spacedArgs, workspace, { shell: true }).status, 0);
  assert.doesNotMatch(await readFile(join(output, 'App.Entrée.fs'), 'utf8'), /truncated FFI path/);
  await runtime('shell wrapper preserves spaces, equals sign and Unicode', 'selected 42');
  const separated = await generated();
  assert.equal(invoke(['--main=App.Entrée', '--ffi=' + ffi], workspace).status, 0);
  identical(separated, await generated());
  await runtime('equals syntax preserves the complete value', 'selected 42');
  assert.equal(invoke(['--main=App.Entrée', '--ffi=-native'], workspace).status, 0);
  await runtime('leading-dash directory through equals syntax', 'selected 42');

  // Real Spago 1.x passes backend.args entries verbatim; CLI --backend-args
  // overrides that array. JSON string quoting is also valid YAML quoting.
  const config = `package:\n  name: cli-fixture\n  dependencies: []\nworkspace:\n  packageSet:\n    registry: 77.10.1\n  backend:\n    cmd: ${JSON.stringify(wrapper)}\n    args: ${JSON.stringify(spacedArgs)}\n`;
  await writeFile(join(workspace, 'spago.yaml'), config);
  command(process.env.SPAGO || 'spago', ['build', '--offline'], workspace, { transcript, timeout: 120_000 });
  await runtime('Spago backend.args array', 'selected 42');
  const spagoGenerated = await generated();
  assert.equal(invoke(spacedArgs, workspace, { shell: true }).status, 0);
  identical(spagoGenerated, await generated());
  command(process.env.SPAGO || 'spago', ['build', '--offline', '--backend-args=--main', '--backend-args=App.Entrée',
    '--backend-args=--ffi', '--backend-args=' + ffi], workspace, { transcript, timeout: 120_000 });
  identical(spagoGenerated, await generated());
  await runtime('Spago repeated --backend-args', 'selected 42');
  console.log(`cli: ${errors.length} usage errors, 4 help invocations, generation exit code, ${runtimes.length} runtime phases, ${differentials.length} historical comparisons`);
} catch (error) {
  transcript.push(error.stack || String(error));
  throw error;
} finally {
  if (artifacts) {
    await mkdir(artifacts, { recursive: true });
    await writeFile(join(artifacts, 'results.json'), JSON.stringify({ diagnostics, runtimes, differentials }, null, 2) + '\n');
    await writeFile(join(artifacts, 'validation.log'), transcript.join('\n'));
    await cp(directory, join(artifacts, 'workspace'), { recursive: true });
  }
  await rm(directory, { recursive: true, force: true });
}
