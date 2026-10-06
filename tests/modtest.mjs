// Exercise the public module runner with real processes in an isolated layout.
import assert from 'node:assert/strict';
import { test } from 'node:test';
import { spawn } from 'node:child_process';
import { cp, mkdir, mkdtemp, open, readFile, realpath, rm, writeFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { join, resolve } from 'node:path';
import { setTimeout as delay } from 'node:timers/promises';
import { runLogged } from '../scripts/support/process.mjs';

const artifacts = process.env.MODTEST_ARTIFACTS && resolve(process.env.MODTEST_ARTIFACTS);
const bundleArgs = ['bundle', '--module', 'Main', '--platform', 'node', '--outfile', 'bin/sharpurs.js', '--bundle-type', 'app'];
const kill = (pid, signal = 'SIGKILL') => {
  try { process.kill(pid, signal); } catch (error) { if (error.code !== 'ESRCH') throw error; }
};
const alive = pid => {
  try { process.kill(pid, 0); return true; } catch (error) { if (error.code !== 'ESRCH') throw error; return false; }
};

async function fixture(t) {
  const directory = await realpath(await mkdtemp(join(tmpdir(), 'sharpurs-modtest-')));
  const root = join(directory, 'compiler with spaces');
  for (const path of ['bin', 'tools', 'scripts/support', 'node_modules/.bin']) await mkdir(join(root, path), { recursive: true });
  for (const name of ['bin/modtest', 'scripts/build.mjs', 'scripts/support/process.mjs']) {
    await cp(new URL('../' + name, import.meta.url), join(root, name));
  }
  await cp(process.env.MODTEST_RUNNER_ORACLE || new URL('../tools/modtest-runner.mjs', import.meta.url), join(root, 'tools/modtest-runner.mjs'));
  const trace = join(directory, 'commands.jsonl'), ready = join(directory, 'ready.json');
  const env = { ...process.env, MODTEST_TRACE: trace, MODTEST_READY: ready };
  delete env.SPAGO;
  let child;
  t.after(async () => {
    if (child?.pid && alive(child.pid)) kill(child.pid);
    try {
      const state = JSON.parse(await readFile(ready, 'utf8'));
      kill(-state.pid);
      kill(state.pid);
      kill(state.descendant);
    } catch (error) { if (error.code !== 'ENOENT') throw error; }
    await rm(directory, { recursive: true, force: true });
  });
  const phaseScript = stage => `#!/usr/bin/env node
import { appendFileSync, renameSync, writeFileSync } from 'node:fs';
import { spawn } from 'node:child_process';
const args = process.argv.slice(2), stage = ${stage};
appendFileSync(process.env.MODTEST_TRACE, JSON.stringify({ stage, args, cwd: process.cwd() }) + '\\n');
console.log('stdout:' + stage);
console.error('stderr:' + stage);
if (stage === process.env.MODTEST_FAIL) process.exit(7);
if (stage === process.env.MODTEST_SIGNAL) process.kill(process.pid, 'SIGTERM');
if (stage === process.env.MODTEST_PAUSE) {
  for (const signal of ['SIGINT', 'SIGTERM']) process.on(signal, () => {
    console.log('received:' + signal);
    if (process.env.MODTEST_STOP_MODE !== 'stubborn') process.exit(0);
  });
  const resistant = "process.on('SIGINT', () => {}); process.on('SIGTERM', () => {}); process.send('ready'); setInterval(() => {}, 1000);";
  const descendant = spawn(process.execPath, ['-e', resistant], { stdio: ['ignore', 'ignore', 'ignore', 'ipc'] });
  descendant.once('message', () => {
    const ready = process.env.MODTEST_READY;
    writeFileSync(ready + '.tmp', JSON.stringify({ pid: process.pid, descendant: descendant.pid }));
    renameSync(ready + '.tmp', ready);
  });
  setInterval(() => {}, 1000);
}
`;
  await writeFile(join(root, 'node_modules/.bin/spago'), phaseScript("'spago:' + args[0]"), { mode: 0o755 });
  for (const name of ['alpha', 'beta', 'space name']) {
    const module = join(directory, 'sharpurs-' + name);
    await mkdir(join(module, 'bin'), { recursive: true });
    await writeFile(join(module, 'bin/test'), phaseScript(JSON.stringify('sharpurs-' + name)), { mode: 0o755 });
  }
  await mkdir(join(directory, 'sharpurs-unrunnable'));
  const log = join(directory, 'modtest.log');
  let invocation = 0;
  const collect = async (args, result) => {
    const calls = (await readFile(trace, 'utf8')).trim().split('\n').filter(Boolean).map(line => JSON.parse(line));
    const text = await readFile(log, 'utf8');
    if (artifacts) {
      const path = join(artifacts, t.name.replace(/[^a-zA-Z0-9]+/g, '-'));
      await mkdir(path, { recursive: true });
      const name = String(++invocation);
      await writeFile(join(path, name + '.json'), JSON.stringify({ args, result, calls }, null, 2) + '\n', { flag: 'wx' });
      await writeFile(join(path, name + '.log'), text, { flag: 'wx' });
    }
    return { result, calls, log: text };
  };
  const run = async (args, extraEnv = {}) => {
    await writeFile(trace, '');
    const result = await runLogged(join(root, 'bin/modtest'), args, { cwd: directory,
      env: { ...env, ...extraEnv }, log, timeout: 10_000 });
    return collect(args, result);
  };
  const interrupt = async (args, signal, extraEnv) => {
    await writeFile(trace, '');
    const file = await open(log, 'w');
    let timer, timedOut = false;
    try {
      child = spawn(join(root, 'bin/modtest'), args, { cwd: directory,
        env: { ...env, ...extraEnv }, stdio: ['ignore', file.fd, file.fd] });
      const closed = new Promise((resolve, reject) => {
        child.once('error', reject);
        child.once('close', (code, signal) => resolve({ code, signal }));
      });
      timer = setTimeout(() => { timedOut = true; kill(child.pid); }, 8_000);
      let state;
      for (let attempt = 0; attempt < 500 && !state; attempt++) {
        try { state = JSON.parse(await readFile(ready, 'utf8')); } catch (error) { if (error.code !== 'ENOENT') throw error; }
        if (!state) await delay(10);
      }
      assert.ok(state, 'active phase and resistant descendant started');
      // Only signal the runner. Its own process owner must stop the descendants.
      kill(child.pid, signal);
      const result = { ...await closed, timedOut, activePhase: state };
      for (let attempt = 0; attempt < 100 && alive(state.descendant); attempt++) await delay(10);
      return { ...await collect(args, result), state };
    } finally {
      clearTimeout(timer);
      await file.close();
    }
  };
  const expected = stages => stages.map(stage => ({ stage, args: stage === 'spago:build' ? ['build'] : stage === 'spago:bundle' ? bundleArgs : [],
    cwd: stage.startsWith('spago:') ? root : join(directory, stage) }));
  return { directory, root, run, interrupt, expected };
}

test('modtest preserves selection, order, aliases and exact child arguments', async t => {
  const f = await fixture(t);
  for (const [args, names] of [
    [[], ['alpha', 'beta', 'space name']],
    [['--all'], ['alpha', 'beta', 'space name']],
    [['beta', 'alpha', 'beta'], ['beta', 'alpha']],
    [['--skip-before', 'beta'], ['beta', 'space name']],
    [['--skip-before=sharpurs-beta'], ['beta', 'space name']],
    [['space name', 'beta', 'alpha', 'skip_before=beta'], ['beta', 'alpha']],
    [['--', 'space name', 'alpha'], ['space name', 'alpha']],
    [['beta', 'alpha', '-c'], ['beta', 'alpha']],
  ]) {
    const actual = await f.run(args);
    assert.equal(actual.result.code, 0, actual.log);
    const stages = [...(args.includes('-c') ? ['spago:build', 'spago:bundle'] : []), ...names.map(name => 'sharpurs-' + name)];
    assert.deepEqual(actual.calls, f.expected(stages));
    for (const stage of stages) {
      assert.ok(actual.log.includes('stdout:' + stage));
      assert.ok(actual.log.includes('stderr:' + stage));
    }
    assert.ok(actual.log.includes(`Summary: ${names.length} modules passed.`));
  }
});

test('modtest help and listing never build or execute module scripts', async t => {
  const f = await fixture(t);
  for (const args of [['--help'], ['-h', '-c'], ['--list', '-c'], ['beta', 'alpha', '--list', '-c']]) {
    const actual = await f.run(args);
    assert.equal(actual.result.code, 0, actual.log);
    assert.deepEqual(actual.calls, []);
    if (args.includes('--list')) assert.equal(actual.log.split('\n').filter(line => line.startsWith('sharpurs-')).join('\n'),
      (args[0] === 'beta' ? ['sharpurs-beta', 'sharpurs-alpha'] : ['sharpurs-alpha', 'sharpurs-beta', 'sharpurs-space name']).join('\n'));
    else assert.match(actual.log, /Usage: .*bin\/modtest/);
  }
});

test('modtest usage errors stop before builds and keep exit 2', async t => {
  const f = await fixture(t);
  for (const args of [['--unknown'], ['--all', 'alpha'], ['--skip-before='], ['--skip-before', '--list'], ['--skip-before=missing'], ['missing']]) {
    const actual = await f.run([...args, '-c']);
    assert.equal(actual.result.code, 2, actual.log);
    assert.deepEqual(actual.calls, []);
    assert.match(actual.log, /\[FAILED\]/);
  }
});

test('modtest stops dependent commands after build or first-module failure', async t => {
  const f = await fixture(t);
  for (const stage of ['spago:build', 'spago:bundle', 'sharpurs-beta']) {
    const actual = await f.run(['beta', 'alpha', '-c'], { MODTEST_FAIL: stage });
    assert.equal(actual.result.code, 1, actual.log);
    const order = ['spago:build', 'spago:bundle', 'sharpurs-beta'];
    assert.deepEqual(actual.calls, f.expected(order.slice(0, order.indexOf(stage) + 1)));
    assert.match(actual.log, /failed \(exit (1|7)\)/);
    assert.doesNotMatch(actual.log, /Summary:.*modules passed/);
  }
});

test('modtest reports a missing command and does not run later modules', async t => {
  const f = await fixture(t);
  await writeFile(join(f.directory, 'sharpurs-alpha/bin/test'), '#!/nonexistent/sharpurs-test-interpreter\n');
  const actual = await f.run(['alpha', 'beta']);
  assert.equal(actual.result.code, 1, actual.log);
  assert.deepEqual(actual.calls, []);
  assert.match(actual.log, /sharpurs-alpha:.*ENOENT/);
});

test('modtest records a child signal as failure and stops later modules', async t => {
  const f = await fixture(t);
  const actual = await f.run(['alpha', 'beta'], { MODTEST_SIGNAL: 'sharpurs-alpha' });
  assert.equal(actual.result.code, 1, actual.log);
  assert.deepEqual(actual.calls, f.expected(['sharpurs-alpha']));
  assert.match(actual.log, /sharpurs-alpha failed \(SIGTERM\)/);
  assert.doesNotMatch(actual.log, /Summary:.*modules passed/);
});

test('modtest reports each successful build and module explicitly', async t => {
  const f = await fixture(t);
  const actual = await f.run(['beta', 'alpha', '-c']);
  assert.equal(actual.result.code, 0, actual.log);
  assert.deepEqual([...actual.log.matchAll(/\[PASS\] (.+)/g)].map(match => match[1]), ['build-sharpurs', 'sharpurs-beta', 'sharpurs-alpha']);
});

for (const stage of ['spago:build', 'sharpurs-alpha']) for (const signal of ['SIGINT', 'SIGTERM']) for (const mode of ['exit', 'stubborn']) {
  test(`modtest interruption ${stage} ${signal} ${mode} removes resistant descendants`, async t => {
    const f = await fixture(t);
    const args = stage.startsWith('spago:') ? ['alpha', 'beta', '-c'] : ['alpha', 'beta'];
    const actual = await f.interrupt(args, signal, { MODTEST_PAUSE: stage, MODTEST_STOP_MODE: mode });
    assert.equal(actual.result.timedOut, false, actual.log);
    assert.equal(actual.result.code, signal === 'SIGINT' ? 130 : 143, actual.log);
    assert.deepEqual(actual.calls, f.expected([stage]));
    assert.ok(actual.log.includes('received:' + signal), actual.log);
    assert.ok(actual.log.includes('Interrupted by ' + signal), actual.log);
    assert.equal(alive(actual.state.pid), false, 'active phase exited');
    assert.equal(alive(actual.state.descendant), false, 'resistant descendant exited');
    assert.doesNotMatch(actual.log, /Summary:.*modules passed/);
  });
}
