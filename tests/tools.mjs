// Exercise orchestration with actual child processes and isolated file trees.
// Compiler semantics are covered by the focused suites, not reimplemented here.
import assert from 'node:assert/strict';
import { test } from 'node:test';
import { cp, mkdir, mkdtemp, readFile, realpath, rm, stat, symlink, writeFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { delimiter, dirname, join } from 'node:path';
import { setTimeout as delay } from 'node:timers/promises';
import { runChecks } from './run.mjs';
import { runLogged, toolEnvironment } from '../scripts/support/process.mjs';
import { compareGeneration, fileManifest } from '../scripts/support/generation.mjs';

const quiet = () => {};
async function sandbox(t) {
  const directory = await mkdtemp(join(tmpdir(), 'sharpurs-tools-'));
  t.after(() => rm(directory, { recursive: true, force: true }));
  return directory;
}

test('child failures keep their exit code and complete logs', async t => {
  const directory = await sandbox(t);
  const log = join(directory, 'child.log');
  const result = await runLogged(process.execPath, ['-e', 'process.stdout.write("x".repeat(2**21)); process.stderr.write("failure"); process.exit(7)'], { log });
  assert.equal(result.ok, false);
  assert.equal(result.code, 7);
  assert.ok((await stat(log)).size > 2 ** 21);
  assert.match(await readFile(log, 'utf8'), /failure$/);
});

test('missing executables are recorded as failures', async t => {
  const directory = await sandbox(t);
  const result = await runLogged(join(directory, 'missing'), [], { log: join(directory, 'missing.log') });
  assert.equal(result.ok, false);
  assert.match(result.error, /ENOENT/);
});

test('tool lookup prefers local tools then PATH over ancestor npm bins', async t => {
  const directory = await sandbox(t);
  const root = join(directory, 'compiler');
  for (const path of ['compiler/node_modules/.bin', 'node_modules/.bin', 'tools']) await mkdir(join(directory, path), { recursive: true });
  const script = value => '#!/bin/sh\necho ' + value + '\n';
  await writeFile(join(directory, 'node_modules/.bin/spago'), script('legacy'), { mode: 0o755 });
  await writeFile(join(directory, 'tools/spago'), script('selected'), { mode: 0o755 });
  await symlink(join(directory, 'node_modules/.bin'), join(directory, 'legacy-bin-alias'));
  const env = toolEnvironment(root, { ...process.env, PATH: [join(directory, 'legacy-bin-alias'), join(directory, 'node_modules/.bin'), join(directory, 'tools'), '/usr/bin', '/bin'].join(delimiter) });
  const log = join(directory, 'lookup.log');
  assert.equal((await runLogged('spago', [], { env, log })).ok, true);
  assert.match(await readFile(log, 'utf8'), /selected/);
  await writeFile(join(root, 'node_modules/.bin/spago'), script('local'), { mode: 0o755 });
  assert.equal((await runLogged('spago', [], { env, log })).ok, true);
  assert.match(await readFile(log, 'utf8'), /local/);
});

async function buildFixture(t) {
  const directory = await sandbox(t);
  const root = join(directory, 'compiler with spaces');
  for (const path of ['scripts/support', 'node_modules/.bin', 'tools', 'bin']) await mkdir(join(root, path), { recursive: true });
  for (const path of ['node_modules/.bin', 'tools']) await mkdir(join(directory, path), { recursive: true });
  for (const path of ['scripts/build.mjs', 'scripts/support/process.mjs']) await cp(new URL('../' + path, import.meta.url), join(root, path));
  const { scripts } = JSON.parse(await readFile(new URL('../package.json', import.meta.url), 'utf8'));
  await writeFile(join(root, 'package.json'), JSON.stringify({ type: 'module', scripts: { build: scripts.build, prepare: scripts.prepare } }));
  const trace = join(directory, 'commands.jsonl');
  const installTool = async (path, name) => writeFile(path, `#!/usr/bin/env node
import { appendFileSync } from 'node:fs';
const args = process.argv.slice(2);
appendFileSync(process.env.BUILD_TRACE, JSON.stringify({ tool: ${JSON.stringify(name)}, args, cwd: process.cwd() }) + '\\n');
if (${JSON.stringify(name)} === 'legacy') process.exit(81);
if (args[0] === process.env.FAIL_STAGE) { console.error('stage failed: ' + args[0]); process.exit(Number(process.env.FAIL_CODE)); }
`, { mode: 0o755 });
  await installTool(join(directory, 'node_modules/.bin/spago'), 'legacy');
  await installTool(join(directory, 'tools/spago'), 'selected');
  const env = { ...process.env, BUILD_TRACE: trace,
    PATH: [join(directory, 'node_modules/.bin'), join(directory, 'tools'), dirname(process.execPath), process.env.PATH].join(delimiter) };
  delete env.SPAGO;
  const invoke = async (program, args, extraEnv = {}) => {
    await writeFile(trace, '');
    const result = await runLogged(program, args, { cwd: root, env: { ...env, ...extraEnv }, log: join(directory, 'build.log') });
    const calls = (await readFile(trace, 'utf8')).trim().split('\n').filter(Boolean).map(line => JSON.parse(line));
    return { result, calls };
  };
  return { directory, root, installTool, invoke };
}

const bundleArgs = ['bundle', '--module', 'Main', '--platform', 'node', '--outfile', 'bin/sharpurs.js', '--bundle-type', 'app'];

test('npm build and nested prepare select PATH, local and explicit Spago over ancestor bins', async t => {
  const { directory, root, installTool, invoke } = await buildFixture(t);
  const cwd = await realpath(root);
  for (const command of ['build', 'prepare']) {
    const { result, calls } = await invoke('npm', ['run', command]);
    assert.equal(result.code, 0, await readFile(result.log, 'utf8'));
    assert.deepEqual(calls, [['build'], bundleArgs].map(args => ({ tool: 'selected', args, cwd })));
  }
  await installTool(join(root, 'node_modules/.bin/spago'), 'local');
  const local = await invoke('npm', ['run', 'build']);
  assert.equal(local.result.code, 0);
  assert.deepEqual(local.calls.map(call => call.tool), ['local', 'local']);
  const explicit = join(directory, 'tools/custom spago');
  await installTool(explicit, 'explicit');
  const selected = await invoke('npm', ['run', 'prepare'], { SPAGO: explicit });
  assert.equal(selected.result.code, 0);
  assert.deepEqual(selected.calls.map(call => call.tool), ['explicit', 'explicit']);
});

test('npm build propagates compile/bundle failures and stops before dependent steps', async t => {
  const { root, invoke } = await buildFixture(t);
  for (const [stage, code, commands] of [['build', 7, [['build']]], ['bundle', 9, [['build'], bundleArgs]]]) {
    const { result, calls } = await invoke('npm', ['run', 'build'], { FAIL_STAGE: stage, FAIL_CODE: String(code) });
    assert.equal(result.code, code);
    assert.deepEqual(calls.map(call => call.args), commands);
    assert.match(await readFile(result.log, 'utf8'), new RegExp('stage failed: ' + stage));
  }
  const missing = await invoke('npm', ['run', 'build'], { SPAGO: join(root, 'missing spago') });
  assert.equal(missing.result.code, 1);
  assert.deepEqual(missing.calls, []);
  assert.match(await readFile(missing.result.log, 'utf8'), /ENOENT/);
});

test('module clean builds use the public build entrypoint and selected toolchain', async t => {
  const { directory, root, invoke } = await buildFixture(t);
  const cwd = await realpath(root);
  await cp(new URL('../tools/modtest-runner.mjs', import.meta.url), join(root, 'tools/modtest-runner.mjs'));
  await mkdir(join(directory, 'sharpurs-example/bin'), { recursive: true });
  await writeFile(join(directory, 'sharpurs-example/bin/test'), '#!/bin/sh\nexit 0\n', { mode: 0o755 });
  const { result, calls } = await invoke(process.execPath, ['tools/modtest-runner.mjs', '--all', '-c']);
  assert.equal(result.code, 0, await readFile(result.log, 'utf8'));
  assert.deepEqual(calls, [['build'], bundleArgs].map(args => ({ tool: 'selected', args, cwd })));
  assert.match(await readFile(result.log, 'utf8'), /Summary: 1 modules passed/);
});

test('CLI clean builds reach fixture compilation only after the public build succeeds', async t => {
  const { root, installTool, invoke } = await buildFixture(t);
  await installTool(join(root, 'node_modules/.bin/spago'), 'local');
  await cp(new URL('../bin/test', import.meta.url), join(root, 'bin/test'));
  await writeFile(join(root, 'bin/pkg'), 'CORE_PACKAGES=()\n');
  await writeFile(join(root, 'bin/sharpurs.js'), '// bundle marker');
  await writeFile(join(root, 'bin/sharpurs'), '#!/bin/sh\necho fixture-generation-reached\nexit 23\n', { mode: 0o755 });
  await mkdir(join(root, 'tests/passing'), { recursive: true });
  await writeFile(join(root, 'tests/passing/Fixture.purs'), 'module Main where\n');
  const failed = await invoke('bash', ['bin/test', 'Fixture', '-c'], { FAIL_STAGE: 'build', FAIL_CODE: '7' });
  assert.equal(failed.result.code, 1);
  assert.deepEqual(failed.calls.map(call => call.args), [['build']]);
  const built = await invoke('bash', ['bin/test', 'Fixture', '-c']);
  assert.equal(built.result.code, 1);
  assert.deepEqual(built.calls.map(call => call.args), [['build'], bundleArgs, ['build', '-q']]);
  assert.match(await readFile(built.result.log, 'utf8'), /fixture-generation-reached/);
});

test('timeouts and cancellation cannot report success', async t => {
  const directory = await sandbox(t);
  const args = ['-e', 'setInterval(() => {}, 1000)'];
  const timed = await runLogged(process.execPath, args, { timeout: 100, log: join(directory, 'timeout.log') });
  assert.equal(timed.ok, false);
  assert.equal(timed.timedOut, true);
  const controller = new AbortController();
  const pending = runLogged(process.execPath, args, { signal: controller.signal, log: join(directory, 'cancel.log') });
  const timer = setTimeout(() => controller.abort(), 100);
  const cancelled = await pending;
  clearTimeout(timer);
  assert.equal(cancelled.ok, false);
  assert.equal(cancelled.cancelled, true);
});

test('cancellation also stops descendants that ignore SIGTERM', async t => {
  const directory = await sandbox(t);
  const pidFile = join(directory, 'descendant.pid');
  let pid;
  t.after(() => { if (pid) { try { process.kill(pid, 'SIGKILL'); } catch (error) { if (error.code !== 'ESRCH') throw error; } } });
  const descendant = `process.on('SIGTERM', () => {}); require('fs').writeFileSync(${JSON.stringify(pidFile)}, String(process.pid)); setInterval(() => {}, 1000);`;
  const parent = `require('child_process').spawn(process.execPath, ['-e', ${JSON.stringify(descendant)}], { stdio: 'ignore' });`;
  const controller = new AbortController();
  const pending = runLogged(process.execPath, ['-e', parent], { signal: controller.signal, timeout: 5000, log: join(directory, 'descendant.log') });
  for (let attempt = 0; attempt < 200 && !pid; attempt++) {
    try { pid = Number(await readFile(pidFile, 'utf8')); } catch (error) { if (error.code !== 'ENOENT') throw error; }
    if (!pid) await delay(10);
  }
  controller.abort();
  const result = await pending;
  assert.ok(pid, 'descendant started');
  assert.equal(result.cancelled, true);
  const alive = () => { try { process.kill(pid, 0); return true; } catch (error) { if (error.code !== 'ESRCH') throw error; return false; } };
  for (let attempt = 0; attempt < 200 && alive(); attempt++) await delay(10);
  assert.equal(alive(), false);
});

test('check runner continues independent suites and writes a failing summary', async t => {
  const directory = await sandbox(t);
  await mkdir(join(directory, 'tests'));
  await writeFile(join(directory, 'package.json'), JSON.stringify({ scripts: {
    'test:first': 'node tests/first.mjs', 'test:second': 'node tests/second.mjs',
  } }));
  await writeFile(join(directory, 'tests/first.mjs'), 'console.log("first failure"); process.exit(3)');
  await writeFile(join(directory, 'tests/second.mjs'), 'console.log("second completed")');
  const artifacts = join(directory, 'reports/run');
  const result = await runChecks({ directory, artifacts, build: false, assertions: false, report: quiet });
  assert.equal(result.success, false);
  assert.deepEqual(result.results.map(step => [step.name, step.code]), [['test:first', 3], ['test:second', 0]]);
  assert.equal(JSON.parse(await readFile(join(artifacts, 'results.json'), 'utf8')).success, false);
  await assert.rejects(runChecks({ directory, suites: ['misspelled'], report: quiet }), /Unknown suite/);
  await assert.rejects(runChecks({ directory, artifacts, report: quiet }), /EEXIST/);
});

test('failed prerequisites stop dependent checks', async t => {
  const directory = await sandbox(t);
  await writeFile(join(directory, 'package.json'), JSON.stringify({ scripts: { 'test:one': 'node unused.mjs' } }));
  await writeFile(join(directory, 'build'), 'process.exit(4)');
  const result = await runChecks({ directory, artifacts: join(directory, 'artifacts'), env: { ...process.env, SPAGO: process.execPath }, report: quiet });
  assert.equal(result.success, false);
  assert.equal(result.results.length, 1);
  assert.equal(result.results[0].name, 'build');
  assert.equal(result.results[0].code, 4);
  assert.equal(result.requested.length, 4);
});

test('CLI runner rejects stale success, restores caches and serializes invocations', async t => {
  const directory = await sandbox(t);
  for (const path of ['bin', 'node_modules/.bin', 'tests/passing', 'tests/runner/output/Main', 'tests/runner/.purmeta']) await mkdir(join(directory, path), { recursive: true });
  await cp(new URL('../bin/test', import.meta.url), join(directory, 'bin/test'));
  await writeFile(join(directory, 'bin/pkg'), 'CORE_PACKAGES=()\n');
  await writeFile(join(directory, 'bin/sharpurs.js'), '// bundle marker');
  await writeFile(join(directory, 'bin/sharpurs'), '#!/bin/bash\necho "generation failed"\nexit 7\n', { mode: 0o755 });
  await writeFile(join(directory, 'node_modules/.bin/spago'), '#!/bin/bash\nmkdir -p .purmeta\necho mutated > .purmeta/old\necho generated > .purmeta/new\n', { mode: 0o755 });
  await writeFile(join(directory, 'node_modules/.bin/dotnet'), '#!/bin/bash\necho stale-project-ran > unexpected\n', { mode: 0o755 });
  await writeFile(join(directory, 'tests/passing/Fixture.purs'), 'module Main where\n');
  await writeFile(join(directory, 'tests/runner/output/Main/Program.fsproj'), 'stale project');
  await writeFile(join(directory, 'tests/runner/.purmeta/old'), 'original');
  const run = args => runLogged('bash', [join(directory, 'bin/test'), ...args], { log: join(directory, 'cli.log') });
  assert.equal((await run(['Fixture'])).ok, false);
  assert.match(await readFile(join(directory, 'cli.log'), 'utf8'), /sharpurs generation failed/);
  assert.equal(await readFile(join(directory, 'tests/runner/.purmeta/old'), 'utf8'), 'original');
  await assert.rejects(stat(join(directory, 'tests/runner/.purmeta/new')), { code: 'ENOENT' });
  await assert.rejects(stat(join(directory, 'tests/runner/unexpected')), { code: 'ENOENT' });
  await writeFile(join(directory, 'bin/sharpurs'), '#!/bin/bash\nexit 0\n');
  await writeFile(join(directory, 'tests/runner/output/Main/Program.fsproj'), 'stale project');
  assert.equal((await run(['Fixture'])).ok, false);
  assert.match(await readFile(join(directory, 'cli.log'), 'utf8'), /did not generate Main project/);
  await assert.rejects(stat(join(directory, 'tests/runner/unexpected')), { code: 'ENOENT' });
  assert.equal((await run(['not-a-fixture'])).ok, false);
  assert.match(await readFile(join(directory, 'cli.log'), 'utf8'), /not found/);
  await mkdir(join(directory, 'tests/runner/.sharpurs-test.lock'));
  assert.equal((await run(['Fixture'])).ok, false);
  assert.match(await readFile(join(directory, 'cli.log'), 'utf8'), /already in use/);
});

async function fixtureSelection(t, names = ['A', 'B', 'C', 'TCOMutRec']) {
  const directory = await sandbox(t);
  for (const path of ['bin', 'node_modules/.bin', 'tests/passing']) await mkdir(join(directory, path), { recursive: true });
  await cp(process.env.FIXTURE_RUNNER_ORACLE || new URL('../bin/test', import.meta.url), join(directory, 'bin/test'));
  await writeFile(join(directory, 'bin/pkg'), 'CORE_PACKAGES=()\n');
  await writeFile(join(directory, 'bin/sharpurs.js'), '// bundle marker');
  await writeFile(join(directory, 'bin/sharpurs'), '#!/bin/sh\nmkdir -p output/Main\necho project > output/Main/Program.fsproj\n', { mode: 0o755 });
  await writeFile(join(directory, 'node_modules/.bin/spago'), `#!/usr/bin/env node
import { appendFileSync, readFileSync } from 'node:fs';
appendFileSync(process.env.FIXTURE_TRACE, JSON.stringify(readFileSync('src/Main.purs', 'utf8')) + '\\n');
`, { mode: 0o755 });
  await writeFile(join(directory, 'node_modules/.bin/dotnet'), '#!/bin/sh\nexit 0\n', { mode: 0o755 });
  for (const name of names) await writeFile(join(directory, 'tests/passing', name + '.purs'), `module ${name} where\n`);
  const trace = join(directory, 'compiled.jsonl');
  const run = async (args, cwd = directory) => {
    await writeFile(trace, '');
    const result = await runLogged('bash', [join(directory, 'bin/test'), ...args], {
      cwd, env: { ...process.env, FIXTURE_TRACE: trace }, log: join(directory, 'selection.log'),
    });
    const sources = (await readFile(trace, 'utf8')).trim().split('\n').filter(Boolean).map(line => JSON.parse(line));
    return { result, sources, log: await readFile(result.log, 'utf8') };
  };
  return { directory, run };
}

test('CLI fixture bounds reject invalid ranges before touching the shared runner', async t => {
  const { directory, run } = await fixtureSelection(t);
  const runner = join(directory, 'tests/runner');
  await mkdir(join(runner, '.purmeta'), { recursive: true });
  await writeFile(join(runner, '.purmeta/old'), 'preserve cache');
  await writeFile(join(runner, 'spago.yaml'), 'preserve configuration');
  await mkdir(join(directory, 'other'));
  await writeFile(join(directory, 'other/A.purs'), 'module Other where\n');
  const before = await fileManifest(runner);
  for (const [args, diagnostic] of [
    [['--skip-before=Missing'], /--skip-before.*not in the selected fixtures/],
    [['--until=Missing'], /--until.*not in the selected fixtures/],
    [['--skip-before='], /--skip-before needs a fixture name/],
    [['skip_before='], /--skip-before needs a fixture name/],
    [['--until='], /--until needs a fixture name/],
    [['until='], /--until needs a fixture name/],
    [['--skip-before=C', '--until=A'], /--until precedes --skip-before/],
    [['A', '--skip-before=B'], /not in the selected fixtures/],
    [['A', '--until=B'], /not in the selected fixtures/],
    [['A', 'other/A.purs', '--skip-before=A'], /ambiguous/],
    [['A', 'other/A.purs', '--until=A.purs'], /ambiguous/],
    [['TCOMutRec'], /no runnable fixtures/],
    [['--skip-before=TCOMutRec', '--until=TCOMutRec'], /no runnable fixtures/],
    [['--skip-before=Missing', '-c'], /not in the selected fixtures/],
  ]) {
    const actual = await run(args);
    assert.equal(actual.result.code, 1, JSON.stringify(args));
    assert.match(actual.log, diagnostic);
    assert.deepEqual(actual.sources, []);
    assert.deepEqual(await fileManifest(runner), before, 'invalid selection preserves cache bytes and mtimes');
    await assert.rejects(stat(join(runner, 'src')), { code: 'ENOENT' });
    await assert.rejects(stat(join(runner, '.sharpurs-test.lock')), { code: 'ENOENT' });
  }
});

test('CLI fixture ranges preserve order and inclusive excluded boundaries', async t => {
  const { run } = await fixtureSelection(t);
  for (const [args, expected, excluded] of [
    [[], ['A', 'B', 'C'], true],
    [['--skip-before=B', '--until=C.purs'], ['B', 'C'], false],
    [['skip_before=B.purs', 'until=B'], ['B'], false],
    [['C', 'A', 'B', '--skip-before=C', '--until=A'], ['C', 'A'], false],
    [['A', 'TCOMutRec', 'B', '--until=TCOMutRec'], ['A'], true],
    [['A', 'TCOMutRec', 'B', '--skip-before=TCOMutRec.purs'], ['B'], true],
  ]) {
    const actual = await run(args);
    assert.equal(actual.result.code, 0, actual.log);
    assert.deepEqual(actual.sources, expected.map(name => `module ${name} where\n`));
    assert.equal(actual.log.includes('=> Skipping TCOMutRec.purs (blacklisted)'), excluded);
    assert.equal((actual.log.match(/\[OK\]/g) || []).length, expected.length);
  }
});

test('CLI caller-relative fixture paths retain spaces and Unicode after entering the runner', async t => {
  const { directory, run } = await fixtureSelection(t);
  const caller = join(directory, 'caller');
  await mkdir(join(caller, 'fixtures with spaces'), { recursive: true });
  const name = 'fixtures with spaces/Entrée.purs';
  await writeFile(join(caller, name), 'module Chosen where\n');
  const actual = await run([name, '--skip-before=Entrée', '--until=Entrée.purs'], caller);
  assert.equal(actual.result.code, 0, actual.log);
  assert.deepEqual(actual.sources, ['module Chosen where\n']);
});

test('CLI empty fixture inventories fail without creating a runner', async t => {
  const { directory, run } = await fixtureSelection(t, []);
  const actual = await run([]);
  assert.equal(actual.result.code, 1);
  assert.match(actual.log, /No fixture found/);
  assert.deepEqual(actual.sources, []);
  await assert.rejects(stat(join(directory, 'tests/runner')), { code: 'ENOENT' });
});

async function comparisonFixture(t) {
  const directory = await sandbox(t);
  const workspace = join(directory, 'prepared/workspace with spaces');
  for (const path of ['output/Main', 'src', '.spago/p/native/v1/src', '.spago/p/native/v2']) await mkdir(join(workspace, path), { recursive: true });
  await writeFile(join(workspace, 'output/Main/corefn.json'), JSON.stringify({ modulePath: 'src/Main.purs' }));
  await writeFile(join(workspace, 'src/Main.purs'), 'module Main where');
  await writeFile(join(workspace, 'src/Main.fs'), 'native source');
  await writeFile(join(workspace, '.spago/p/native/v1/src/Native.fs'), 'dependency');
  await writeFile(join(workspace, 'sharp.packages.props'), '<PackageReference Include="Example" Version="1" />');
  await writeFile(join(workspace, 'output/Main/user.fs'), 'preserve existing application output');
  const compiler = async (name, { changed = false, removed = false, rewrite = false, mutate = false, fail = false } = {}) => {
    const file = join(directory, name + '.mjs');
    await writeFile(file, `import fs from 'node:fs';
const core = JSON.parse(fs.readFileSync('output/Main/corefn.json', 'utf8'));
const source = fs.readFileSync(core.modulePath.replace(/\\.purs$/, '.fs'), 'utf8');
const files = { 'Module.fs': source + ${JSON.stringify(changed ? ' changed' : '')}, 'Program.fsproj': 'project', ...(${removed} ? {} : { 'Legacy.cs': 'native' }) };
for (const [name, text] of Object.entries(files)) {
 const path = 'output/Main/' + name;
 if (${rewrite} || !fs.existsSync(path) || fs.readFileSync(path, 'utf8') !== text) fs.writeFileSync(path, text);
}
if (${mutate}) fs.writeFileSync('output/Main/corefn.json', '{}');
if (${fail}) process.exit(9);
`);
    return file;
  };
  return { directory, workspace, compiler };
}

test('generation comparison snapshots inputs and retains original application output', async t => {
  const { directory, workspace, compiler } = await comparisonFixture(t);
  const before = await compiler('before');
  const result = await compareGeneration({ before, after: before, workspace, artifacts: join(directory, 'comparison'), report: quiet });
  assert.equal(result.success, true, result.error);
  assert.equal(result.runs.length, 3);
  assert.equal(result.clean.filesAfter, 3);
  assert.deepEqual(result.incremental.rewrittenUnchanged, []);
  assert.equal(await readFile(join(workspace, 'output/Main/user.fs'), 'utf8'), 'preserve existing application output');
  assert.equal(JSON.parse(await readFile(join(workspace, 'output/Main/corefn.json'), 'utf8')).modulePath, 'src/Main.purs');
  assert.ok((await stat(join(result.artifacts, 'sandbox/workspace/.spago/p/native/v2'))).isDirectory());
  const inputs = JSON.parse(await readFile(join(result.artifacts, 'inputs.json'), 'utf8'));
  assert.ok(inputs.files['sandbox/workspace/sharp.packages.props']);
  await assert.rejects(compareGeneration({ before, after: before, workspace, artifacts: result.artifacts, report: quiet }), /EEXIST/);
});

for (const [name, options, inspect] of [
  ['changed output', { changed: true }, result => assert.deepEqual(result.clean.changed, ['Module.fs'])],
  ['removed output hidden by an incremental run', { removed: true }, result => {
    assert.deepEqual(result.incremental.removed, []);
    assert.deepEqual(result.clean.removed, ['Legacy.cs']);
  }],
  ['rewritten identical output', { rewrite: true }, result => assert.equal(result.incremental.rewrittenUnchanged.length, 3)],
  ['input mutation', { mutate: true }, result => assert.match(result.error, /changed the snapshotted inputs/)],
  ['failed compiler process', { fail: true }, result => assert.equal(result.runs.at(-1).code, 9)],
]) test(`generation comparison rejects ${name}`, async t => {
  const { directory, workspace, compiler } = await comparisonFixture(t);
  const result = await compareGeneration({ before: await compiler('before'), after: await compiler('after', options),
    workspace, artifacts: join(directory, 'comparison'), report: quiet });
  assert.equal(result.success, false);
  inspect(result);
  assert.equal(JSON.parse(await readFile(join(result.artifacts, 'comparison.json'), 'utf8')).success, false);
});
