// Real runner processes with isolated tools and targeted filesystem failures.
import assert from 'node:assert/strict';
import { test } from 'node:test';
import { cp, mkdir, mkdtemp, readFile, readdir, rm, stat, writeFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { setTimeout as delay } from 'node:timers/promises';
import { fileManifest } from '../scripts/support/generation.mjs';
import { runLogged } from '../scripts/support/process.mjs';

async function fixture(t) {
  const directory = await mkdtemp(join(tmpdir(), 'sharpurs-runner-lifecycle-'));
  t.after(() => rm(directory, { recursive: true, force: true }));
  const root = join(directory, 'compiler with spaces');
  const runner = join(root, 'tests/runner');
  for (const path of ['bin', 'scripts', 'node_modules/.bin', 'tests/passing/A', 'tests/runner/src',
    'tests/runner/.purmeta', 'tests/runner/output/Main', 'tests/runner/output/Old', 'tests/runner/.spago']) {
    await mkdir(join(root, path), { recursive: true });
  }
  await mkdir(join(directory, 'temporary'));
  await cp(process.env.FIXTURE_RUNNER_ORACLE || new URL('../bin/test', import.meta.url), join(root, 'bin/test'));
  await writeFile(join(root, 'bin/pkg'), 'CORE_PACKAGES=()\n');
  await writeFile(join(root, 'bin/sharpurs.js'), '// bundle marker');
  await writeFile(join(root, 'tests/passing/A.purs'), 'module Chosen where\n');
  await writeFile(join(root, 'tests/passing/A.js'), '// chosen foreign\n');
  await writeFile(join(root, 'tests/passing/A/Support.purs'), 'module Support where\n');
  await writeFile(join(root, 'tests/passing/B.purs'), 'module Second where\n');
  await writeFile(join(runner, 'src/Main.purs'), 'module Stale where\n');
  await writeFile(join(runner, '.purmeta/old'), 'original cache');
  await writeFile(join(runner, 'output/Main/Program.fsproj'), 'stale project');
  await writeFile(join(runner, 'output/Old/corefn.json'), '{"modulePath":"src/Old.purs"}');
  await writeFile(join(runner, '.spago/marker'), 'old dependency cache');
  const trace = join(directory, 'commands.jsonl');
  for (const [path, tool] of [['node_modules/.bin/spago', 'spago'], ['bin/sharpurs', 'sharpurs'],
    ['node_modules/.bin/dotnet', 'dotnet'], ['scripts/build.mjs', 'build']]) {
    await writeFile(join(root, path), `#!/usr/bin/env node
import { appendFileSync, existsSync, mkdirSync, readFileSync, writeFileSync } from 'node:fs';
const tool = ${JSON.stringify(tool)};
appendFileSync(process.env.RUNNER_TRACE, JSON.stringify({ tool, args: process.argv.slice(2),
  source: existsSync('src/Main.purs') ? readFileSync('src/Main.purs', 'utf8') : null }) + '\\n');
if (tool === 'spago') {
  mkdirSync('.purmeta', { recursive: true });
  writeFileSync('.purmeta/old', 'mutated cache');
  writeFileSync('.purmeta/new', 'new cache entry');
}
if (tool === 'sharpurs') {
  mkdirSync('output/Main', { recursive: true });
  writeFileSync('output/Main/Program.fsproj', 'fresh project');
}
if (process.env.RUNNER_FAIL_STAGE === tool) process.exit(7);
if (process.env.RUNNER_PAUSE === tool) {
  writeFileSync(process.env.RUNNER_READY, String(process.pid));
  setInterval(() => {}, 1000);
}
`, { mode: 0o755 });
  }
  // Delegate every non-targeted operation to the real OS tool. A failing command
  // leaves its own diagnostic, so the test checks both context and original cause.
  for (const tool of ['mkdir', 'cp', 'rm', 'mv', 'mktemp']) {
    await writeFile(join(root, 'node_modules/.bin', tool), `#!/usr/bin/env node
import { mkdirSync } from 'node:fs';
import { spawnSync } from 'node:child_process';
const tool = ${JSON.stringify(tool)}, args = process.argv.slice(2), fault = process.env.RUNNER_IO_FAULT;
const has = suffix => args.some(arg => arg.endsWith(suffix));
const copyingCache = tool === 'cp' && args.includes('-pR');
const targets = {
  'create-source': tool === 'mkdir' && has('/tests/runner/src'),
  'copy-source': tool === 'cp' && has('/tests/passing/A.purs'),
  'copy-foreign': tool === 'cp' && has('/tests/passing/A.js'),
  'copy-companion': tool === 'cp' && args.includes('-r'),
  'save-cache': copyingCache && args[1].endsWith('/tests/runner/.purmeta'),
  'restore-cache': copyingCache && args[1].includes('/sharpurs-runner-cache.'),
  'clear-cache': tool === 'rm' && has('/tests/runner/.purmeta'),
  'clear-source': tool === 'rm' && has('src/Main.purs'),
  'clear-stale': tool === 'rm' && has('output/Old'),
  'clear-project': tool === 'rm' && has('output/Main/Program.fsproj'),
  'clean-build': tool === 'rm' && has('.spago/marker'),
  'publish-config': tool === 'mv' && has('/tests/runner/spago.yaml'),
  'temporary': tool === 'mktemp',
  'release-lock': tool === 'rm' && has('/tests/runner/.sharpurs-test.lock'),
};
if (targets[fault]) { console.error('injected ' + fault + ' failure'); process.exit(73); }
const executable = tool === 'mktemp' ? '/usr/bin/mktemp' : '/bin/' + tool;
const result = spawnSync(executable, args, { stdio: 'inherit' });
if (result.error) throw result.error;
if (result.status === 0 && fault === 'lock-owner' && tool === 'mkdir' && has('/tests/runner/.sharpurs-test.lock')) {
  mkdirSync(args.at(-1) + '/pid');
}
process.exit(result.status ?? 1);
`, { mode: 0o755 });
  }
  const beforeCache = await fileManifest(join(runner, '.purmeta'));
  const run = async (args = ['A', 'B'], extraEnv = {}, signal) => {
    await writeFile(trace, '');
    const result = await runLogged('bash', [join(root, 'bin/test'), ...args], {
      cwd: root, env: { ...process.env, TMPDIR: join(directory, 'temporary'), RUNNER_TRACE: trace, ...extraEnv },
      signal, timeout: 15_000, log: join(directory, 'runner.log'),
    });
    const calls = (await readFile(trace, 'utf8')).trim().split('\n').filter(Boolean).map(line => JSON.parse(line));
    return { result, calls, log: await readFile(result.log, 'utf8') };
  };
  const restored = async () => {
    assert.deepEqual(await fileManifest(join(runner, '.purmeta')), beforeCache);
    await assert.rejects(stat(join(runner, '.sharpurs-test.lock')), { code: 'ENOENT' });
    assert.deepEqual(await readdir(join(directory, 'temporary')), []);
  };
  return { directory, root, runner, run, restored, beforeCache };
}

for (const fault of ['create-source', 'lock-owner', 'temporary', 'save-cache', 'copy-source', 'copy-foreign',
  'copy-companion', 'clear-source', 'clear-stale', 'publish-config', 'clean-build']) {
  test(`runner lifecycle preparation stops after ${fault} failure`, async t => {
    const f = await fixture(t);
    const actual = await f.run(fault === 'clean-build' ? ['A', '-c'] : ['A', 'B'], { RUNNER_IO_FAULT: fault });
    assert.equal(actual.result.code, 1, actual.log);
    assert.deepEqual(actual.calls.map(call => call.tool), fault === 'clean-build' ? ['build'] : [], actual.log);
    if (fault !== 'lock-owner') assert.match(actual.log, new RegExp('injected ' + fault + ' failure'));
    assert.match(actual.log, /\[FAILED\]/);
    assert.doesNotMatch(actual.log, /Summary:.*0 failed/);
    await f.restored();
  });
}

test('runner lifecycle configuration errors stop before compilation', async t => {
  const f = await fixture(t);
  await mkdir(join(f.runner, 'spago.yaml'));
  const actual = await f.run();
  assert.equal(actual.result.code, 1, actual.log);
  assert.deepEqual(actual.calls, []);
  assert.match(actual.log, /spago\.yaml/);
  await f.restored();
});

test('runner lifecycle failed cd cannot run tools in the caller directory', async t => {
  const f = await fixture(t);
  const env = join(f.directory, 'shell-env');
  await writeFile(env, `cd() {
  if [[ "$1" == */tests/runner ]]; then echo 'injected cd failure' >&2; return 73; fi
  builtin cd "$@"
}
`);
  const actual = await f.run(['A'], { BASH_ENV: env });
  assert.equal(actual.result.code, 1, actual.log);
  assert.deepEqual(actual.calls, []);
  assert.match(actual.log, /injected cd failure/);
  await f.restored();
});

test('runner lifecycle project cleanup failure stops before backend generation', async t => {
  const f = await fixture(t);
  const actual = await f.run(['A', 'B'], { RUNNER_IO_FAULT: 'clear-project' });
  assert.equal(actual.result.code, 1, actual.log);
  assert.deepEqual(actual.calls.map(call => call.tool), ['spago']);
  assert.match(actual.log, /injected clear-project failure/);
  await f.restored();
});

for (const fault of ['clear-cache', 'restore-cache']) {
  test(`runner lifecycle restoration retains its backup after ${fault} failure`, async t => {
    const f = await fixture(t);
    const actual = await f.run(['A'], { RUNNER_IO_FAULT: fault });
    assert.equal(actual.result.code, 1, actual.log);
    assert.deepEqual(actual.calls.map(call => call.tool), ['spago', 'sharpurs', 'dotnet']);
    assert.match(actual.log, new RegExp('injected ' + fault + ' failure'));
    assert.doesNotMatch(actual.log, /Summary:.*0 failed/);
    const backup = actual.log.match(/Cache restoration failed; saved copy: (.+)/)?.[1];
    assert.ok(backup, actual.log);
    assert.deepEqual(await fileManifest(join(backup, '.purmeta')), f.beforeCache);
    await assert.rejects(stat(join(f.runner, '.sharpurs-test.lock')), { code: 'ENOENT' });
    if (fault === 'clear-cache') {
      assert.equal(await readFile(join(f.runner, '.purmeta/old'), 'utf8'), 'mutated cache');
      await assert.rejects(stat(join(f.runner, '.purmeta/.purmeta')), { code: 'ENOENT' });
    }
  });
}

test('runner lifecycle lock release failure cannot report success', async t => {
  const f = await fixture(t);
  const actual = await f.run(['A'], { RUNNER_IO_FAULT: 'release-lock' });
  assert.equal(actual.result.code, 1, actual.log);
  assert.match(actual.log, /injected release-lock failure/);
  assert.doesNotMatch(actual.log, /Summary:.*0 failed/);
  assert.deepEqual(await fileManifest(join(f.runner, '.purmeta')), f.beforeCache);
  assert.ok((await stat(join(f.runner, '.sharpurs-test.lock/pid'))).isFile());
});

test('runner lifecycle a concurrent owner prevents source preparation', async t => {
  const f = await fixture(t);
  await rm(join(f.runner, 'src'), { recursive: true });
  await mkdir(join(f.runner, '.sharpurs-test.lock'));
  await writeFile(join(f.runner, '.sharpurs-test.lock/pid'), 'another owner');
  const before = await fileManifest(f.runner);
  const actual = await f.run();
  assert.equal(actual.result.code, 1);
  assert.match(actual.log, /already in use/);
  assert.deepEqual(actual.calls, []);
  assert.deepEqual(await fileManifest(f.runner), before);
  await assert.rejects(stat(join(f.runner, 'src')), { code: 'ENOENT' });
});

test('runner lifecycle success and compiler failures restore the original cache', async t => {
  const f = await fixture(t);
  for (const stage of ['', 'spago', 'sharpurs', 'dotnet']) {
    const actual = await f.run(['A', 'B'], { RUNNER_FAIL_STAGE: stage });
    const tools = ['spago', 'sharpurs', 'dotnet'];
    assert.equal(actual.result.code, stage ? 1 : 0, actual.log);
    assert.deepEqual(actual.calls.map(call => call.tool), stage ? tools.slice(0, tools.indexOf(stage) + 1) : [...tools, ...tools]);
    assert.equal(actual.calls[0].source, 'module Chosen where\n');
    await f.restored();
  }
});

test('runner lifecycle an empty companion directory is valid', async t => {
  const f = await fixture(t);
  await rm(join(f.root, 'tests/passing/A/Support.purs'));
  const actual = await f.run(['A']);
  assert.equal(actual.result.code, 0, actual.log);
  assert.doesNotMatch(actual.log, /cannot stat|No such file/);
  assert.deepEqual(actual.calls.map(call => call.tool), ['spago', 'sharpurs', 'dotnet']);
  await f.restored();
});

test('runner lifecycle interruption restores cache after the active command exits', async t => {
  const f = await fixture(t);
  const ready = join(f.directory, 'ready.pid');
  const controller = new AbortController();
  const pending = f.run(['A', 'B'], { RUNNER_PAUSE: 'spago', RUNNER_READY: ready }, controller.signal);
  let pid;
  try {
    for (let attempt = 0; attempt < 500 && !pid; attempt++) {
      try { pid = Number(await readFile(ready, 'utf8')); } catch (error) { if (error.code !== 'ENOENT') throw error; }
      if (!pid) await delay(10);
    }
  } finally { controller.abort(); }
  const actual = await pending;
  assert.ok(pid, actual.log);
  assert.equal(actual.result.code, 143, actual.log);
  assert.equal(actual.result.cancelled, true);
  assert.deepEqual(actual.calls.map(call => call.tool), ['spago']);
  assert.throws(() => process.kill(pid, 0), { code: 'ESRCH' });
  await f.restored();
});
