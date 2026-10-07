// Public native wrappers, real filesystem boundaries and real process groups.
import assert from 'node:assert/strict';
import { test } from 'node:test';
import { spawn } from 'node:child_process';
import { createHash } from 'node:crypto';
import { chmod, cp, lstat, mkdir, mkdtemp, open, readFile, readdir, readlink, realpath, rm, symlink, writeFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { basename, join, resolve } from 'node:path';
import { setTimeout as delay } from 'node:timers/promises';
import { runLogged } from '../scripts/support/process.mjs';

const artifacts = process.env.NATIVE_RUNNER_ARTIFACTS && resolve(process.env.NATIVE_RUNNER_ARTIFACTS);
const kill = (pid, signal = 'SIGKILL') => {
  try { process.kill(pid, signal); } catch (error) { if (error.code !== 'ESRCH') throw error; }
};
const alive = pid => {
  try { process.kill(pid, 0); return true; } catch (error) { if (error.code !== 'ESRCH') throw error; return false; }
};

async function snapshot(root, path = '', result = {}) {
  const full = join(root, path), info = await lstat(full, { bigint: true });
  const value = { mode: String(info.mode), mtimeNs: String(info.mtimeNs) };
  if (info.isSymbolicLink()) value.link = await readlink(full);
  else if (info.isFile()) value.sha256 = createHash('sha256').update(await readFile(full)).digest('hex');
  else if (info.isDirectory()) for (const name of (await readdir(full)).sort()) await snapshot(root, join(path, name), result);
  result[path] = value;
  return result;
}

async function fixture(t, profile = 'file', preferred = 'sharp') {
  const directory = await realpath(await mkdtemp(join(tmpdir(), 'sharpurs-native-test-')));
  const layout = join(directory, 'layout with spaces'), root = join(layout, 'sharpurs');
  const module = join(layout, 'sharpurs-alpha spaced'), dependency = join(layout, 'sharpurs-dep');
  const tools = join(directory, 'tools'), workspaces = join(directory, 'workspaces');
  const trace = join(directory, 'trace.jsonl'), ready = join(directory, 'ready.json'), log = join(directory, 'run.log');
  for (const dir of [root, module, dependency, tools, workspaces, join(directory, 'home')]) await mkdir(dir, { recursive: true });
  for (const name of ['bin/modtest', 'tools/modtest-runner.mjs', 'tools/native-test-runner.mjs', 'scripts/build.mjs', 'scripts/support/process.mjs']) {
    const target = join(root, name);
    await mkdir(resolve(target, '..'), { recursive: true });
    await cp(new URL('../' + name, import.meta.url), target);
  }
  for (const repo of [root, module, dependency, join(directory, 'purescript-backend-optimizer-sharpurs')]) {
    for (const name of ['src', 'test', 'bin', 'output/Main', '.spago', '.cache', '.purmeta']) await mkdir(join(repo, name), { recursive: true });
    for (const name of ['src/source.purs', 'test/test.purs', 'spago.yaml', 'spago.lock', 'output/Main/Program.fsproj', '.spago/sentinel', '.cache/sentinel', '.purmeta/sentinel']) {
      await writeFile(join(repo, name), name === 'spago.yaml' ? 'original profile\n' : 'original ' + name);
    }
  }
  await writeFile(join(root, 'package.json'), '{"type":"module","scripts":{"build":"node scripts/build.mjs"}}\n');
  await writeFile(join(root, 'bin/sharpurs.js'), '// installed bundle\n');
  await writeFile(join(module, 'original.yaml'), 'linked profile\n');
  if (profile !== 'file') {
    await rm(join(module, 'spago.yaml'));
    if (profile !== 'absent') await symlink(profile === 'absolute' ? join(module, 'original.yaml') : profile === 'dangling' ? 'missing.yaml' : 'original.yaml', join(module, 'spago.yaml'));
  }
  if (preferred === 'sharp') await writeFile(join(module, 'spago.sharp.yaml'), 'sharp profile\n');
  if (preferred !== 'current') await writeFile(join(module, 'spago.fs.yaml'), 'fs profile\n');
  await cp(process.env.NATIVE_RUNNER_ORACLE || new URL('../../sharpurs-arrays/bin/test', import.meta.url), join(module, 'bin/test'));
  const phase = tool => `#!/usr/bin/env node
import { appendFileSync, chmodSync, existsSync, mkdirSync, readFileSync, renameSync, writeFileSync } from 'node:fs';
import { basename, dirname, join } from 'node:path';
import { spawn } from 'node:child_process';
const args = process.argv.slice(2), cwd = process.cwd(), tool = ${JSON.stringify(tool)};
const stage = tool === 'spago' ? (basename(cwd) === 'sharpurs' ? 'compiler-' + args[0] : 'spago') : tool;
appendFileSync(process.env.NATIVE_TRACE, JSON.stringify({ stage, args, cwd, profile: readFileSync('spago.yaml', 'utf8') }) + '\\n');
console.log('stdout:' + stage); console.error('stderr:' + stage);
for (const dir of ['output/Main', '.spago', '.cache', '.purmeta']) { mkdirSync(dir, { recursive: true }); writeFileSync(join(dir, 'new'), stage); }
writeFileSync('spago.yaml', 'tool changed private profile\\n');
if (existsSync('../sharpurs-dep')) writeFileSync('../sharpurs-dep/src/source.purs', 'tool changed private dependency');
if (stage === process.env.NATIVE_FAIL) process.exit(7);
if (stage === 'sharpurs' && process.env.NATIVE_PROJECT !== 'missing') {
  for (const ext of process.env.NATIVE_PROJECT === 'both' ? ['fsproj', 'csproj'] : [process.env.NATIVE_PROJECT || 'fsproj']) writeFileSync('output/Main/Program.' + ext, 'generated');
}
if (stage === 'compiler-bundle') writeFileSync('bin/sharpurs.js', '// rebuilt private bundle');
if (stage === 'dotnet' && process.env.NATIVE_CLEANUP_FAIL) {
  mkdirSync('blocked'); writeFileSync('blocked/file', 'cannot unlink'); chmodSync('blocked', 0o555);
}
if (stage === process.env.NATIVE_PAUSE) {
  for (const signal of ['SIGINT', 'SIGTERM']) process.on(signal, () => {});
  const code = "process.on('SIGINT', () => {}); process.on('SIGTERM', () => {}); process.send('ready'); setInterval(() => {}, 1000);";
  const child = spawn(process.execPath, ['-e', code], { stdio: ['ignore', 'ignore', 'ignore', 'ipc'] });
  child.once('message', () => {
    writeFileSync(process.env.NATIVE_READY + '.tmp', JSON.stringify({ pid: process.pid, descendant: child.pid }));
    renameSync(process.env.NATIVE_READY + '.tmp', process.env.NATIVE_READY);
  });
  setInterval(() => {}, 1000);
}
`;
  await writeFile(join(tools, 'spago'), phase('spago'), { mode: 0o755 });
  await writeFile(join(tools, 'dotnet'), phase('dotnet'), { mode: 0o755 });
  await writeFile(join(root, 'bin/sharpurs'), phase('sharpurs'), { mode: 0o755 });
  const env = { ...process.env, HOME: join(directory, 'home'), PATH: tools + ':' + process.env.PATH,
    SPAGO: join(tools, 'spago'), DOTNET: join(tools, 'dotnet'), NATIVE_TRACE: trace, NATIVE_READY: ready,
    SHARPURS_NATIVE_ARTIFACTS: workspaces, TMPDIR: directory };
  let child, before;
  t.after(async () => {
    if (child?.pid && alive(child.pid)) kill(child.pid);
    try {
      const state = JSON.parse(await readFile(ready, 'utf8'));
      kill(-state.pid); kill(state.pid); kill(state.descendant);
    } catch (error) { if (error.code !== 'ENOENT') throw error; }
    for (const parent of [directory, workspaces]) for (const entry of await readdir(parent, { withFileTypes: true })) {
      if (!entry.isDirectory()) continue;
      const blocked = join(parent, entry.name, 'layout/sharpurs-alpha spaced/blocked');
      await chmod(blocked, 0o755).catch(error => { if (error.code !== 'ENOENT') throw error; });
    }
    await rm(directory, { recursive: true, force: true });
  });
  let invocation = 0;
  const collect = async (args, result) => {
    const text = await readFile(log, 'utf8');
    const calls = (await readFile(trace, 'utf8')).trim().split('\n').filter(Boolean).map(JSON.parse);
    if (artifacts) {
      const out = join(artifacts, t.name.replace(/[^a-zA-Z0-9]+/g, '-'));
      await mkdir(out, { recursive: true });
      const name = basename(directory) + '-' + ++invocation;
      await writeFile(join(out, name + '.json'), JSON.stringify({ args, result, calls }, null, 2) + '\n', { flag: 'wx' });
      await writeFile(join(out, name + '.log'), text, { flag: 'wx' });
    }
    return { result, calls, log: text, workspace: text.match(/^Native workspace: (.+)$/m)?.[1] };
  };
  const run = async (args = [], extraEnv = {}) => {
    await writeFile(trace, '');
    before = await snapshot(layout);
    const result = await runLogged(join(module, 'bin/test'), args, { cwd: directory,
      env: { ...env, ...extraEnv }, log, timeout: 15_000 });
    return collect(args, result);
  };
  const interrupt = async (stage, signal, nested = false) => {
    await writeFile(trace, '');
    before = await snapshot(layout);
    const file = await open(log, 'w');
    const command = nested ? join(root, 'bin/modtest') : join(module, 'bin/test');
    const args = nested ? ['alpha spaced'] : stage.startsWith('compiler') ? ['-c'] : [];
    let timer, timedOut = false;
    try {
      child = spawn(command, args, { cwd: directory, env: { ...env, NATIVE_PAUSE: stage }, stdio: ['ignore', file.fd, file.fd] });
      const closed = new Promise((resolve, reject) => { child.once('error', reject); child.once('close', (code, signal) => resolve({ code, signal })); });
      timer = setTimeout(() => { timedOut = true; kill(child.pid); }, 10_000);
      let state;
      for (let attempt = 0; attempt < 500 && !state; attempt++) {
        try { state = JSON.parse(await readFile(ready, 'utf8')); } catch (error) { if (error.code !== 'ENOENT') throw error; }
        if (!state) await delay(10);
      }
      assert.ok(state, 'phase and resistant descendant ready');
      kill(child.pid, signal);
      const result = { ...await closed, timedOut, state };
      for (let attempt = 0; attempt < 100 && alive(state.descendant); attempt++) await delay(10);
      return { ...await collect(args, result), state };
    } finally { clearTimeout(timer); await file.close(); }
  };
  return { directory, module, dependency, workspaces, run, interrupt,
    preserved: async () => assert.deepEqual(await snapshot(layout), before, 'all checkout bytes, mtimes, modes and links preserved') };
}

for (const profile of ['file', 'relative', 'absolute', 'dangling', 'absent']) {
  for (const failure of ['', 'spago', 'sharpurs', 'dotnet']) test(`native profile ${profile}, ${failure || 'success'} preserves checkouts`, async t => {
    const f = await fixture(t, profile);
    const actual = await f.run(['ignored-legacy-argument'], { NATIVE_FAIL: failure });
    assert.equal(actual.result.code, failure ? 1 : 0, actual.log);
    await f.preserved();
    const stages = ['spago', 'sharpurs', 'dotnet'];
    assert.deepEqual(actual.calls.map(x => x.stage), stages.slice(0, failure ? stages.indexOf(failure) + 1 : 3));
    assert.equal(actual.calls[0].profile, 'sharp profile\n');
    assert.deepEqual(actual.calls[0].args, ['build']);
    assert.ok(actual.calls.every(x => x.cwd.startsWith(actual.workspace + '/layout/')));
    if (!failure) {
      assert.deepEqual(actual.calls[1].args, ['--main', 'Test.Main']);
      assert.deepEqual(actual.calls[2].args, ['run', '-c', 'Release', '-v', 'q', '--nologo', '--project', 'output/Main/Program.fsproj']);
    }
    const report = JSON.parse(await readFile(join(actual.workspace, 'results.json'), 'utf8'));
    assert.equal(report.success, !failure);
  });
}

for (const preferred of ['fs', 'current']) test(`native selects ${preferred} profile when no sharp profile exists`, async t => {
  const f = await fixture(t, 'relative', preferred), actual = await f.run();
  assert.equal(actual.result.code, 0, actual.log);
  assert.equal(actual.calls[0].profile, preferred === 'fs' ? 'fs profile\n' : 'linked profile\n');
  await f.preserved();
});

for (const project of ['csproj', 'both', 'missing']) test(`native project ${project} requires fresh generation`, async t => {
  const f = await fixture(t), actual = await f.run([], { NATIVE_PROJECT: project });
  assert.equal(actual.result.code, project === 'missing' ? 1 : 0, actual.log);
  if (project === 'missing') assert.deepEqual(actual.calls.map(x => x.stage), ['spago', 'sharpurs']);
  else assert.equal(actual.calls.at(-1).args.at(-1), `output/Main/Program.${project === 'both' ? 'fsproj' : project}`);
  await f.preserved();
});

for (const failure of ['', 'compiler-build', 'compiler-bundle']) test(`native clean rebuild ${failure || 'success'} is private`, async t => {
  const f = await fixture(t), actual = await f.run(['--clean'], { NATIVE_FAIL: failure });
  assert.equal(actual.result.code, failure ? 1 : 0, actual.log);
  const stages = ['compiler-build', 'compiler-bundle', 'spago', 'sharpurs', 'dotnet'];
  assert.deepEqual(actual.calls.map(x => x.stage), stages.slice(0, failure ? stages.indexOf(failure) + 1 : 5));
  assert.ok(actual.calls.every(x => x.cwd.startsWith(actual.workspace + '/layout/')));
  await f.preserved();
});

test('native preparation failures never execute build tools', async t => {
  for (const kind of ['profile', 'copy']) {
    const f = await fixture(t);
    if (kind === 'profile') for (const name of ['spago.yaml', 'spago.sharp.yaml', 'spago.fs.yaml']) await rm(join(f.module, name));
    else await symlink('missing-source', join(f.dependency, 'broken'));
    const actual = await f.run();
    assert.equal(actual.result.code, 1, actual.log);
    assert.deepEqual(actual.calls, []);
    await f.preserved();
  }
});

test('native removes only its own successful temporary workspace', async t => {
  const f = await fixture(t), actual = await f.run([], { SHARPURS_NATIVE_ARTIFACTS: '' });
  assert.equal(actual.result.code, 0, actual.log);
  await assert.rejects(lstat(actual.workspace), { code: 'ENOENT' });
  await f.preserved();
});

test('native cleanup failure cannot report success', { skip: process.getuid?.() === 0 }, async t => {
  const f = await fixture(t), actual = await f.run([], { SHARPURS_NATIVE_ARTIFACTS: '', NATIVE_CLEANUP_FAIL: '1' });
  assert.equal(actual.result.code, 1, actual.log);
  assert.match(actual.log, /Native finalization/);
  assert.doesNotMatch(actual.log, /Tests passed successfully/);
  await f.preserved();
});

test('native concurrent invocations own different workspaces', async t => {
  const a = await fixture(t), b = await fixture(t);
  const [first, second] = await Promise.all([a.run(), b.run([], { SHARPURS_NATIVE_ARTIFACTS: a.workspaces })]);
  assert.equal(first.result.code, 0, first.log);
  assert.equal(second.result.code, 0, second.log);
  assert.notEqual(first.workspace, second.workspace);
  await a.preserved(); await b.preserved();
});

for (const stage of ['compiler-build', 'spago', 'sharpurs', 'dotnet']) for (const signal of ['SIGINT', 'SIGTERM']) {
  test(`native interruption ${stage} ${signal} preserves profiles and stops descendants`, async t => {
    const f = await fixture(t, 'absolute'), actual = await f.interrupt(stage, signal);
    assert.equal(actual.result.timedOut, false, actual.log);
    assert.equal(actual.result.code, signal === 'SIGINT' ? 130 : 143, actual.log);
    assert.equal(actual.calls.at(-1).stage, stage);
    assert.equal(alive(actual.state.pid), false, 'phase exited');
    assert.equal(alive(actual.state.descendant), false, 'descendant exited');
    await f.preserved();
  });
}

for (const signal of ['SIGINT', 'SIGTERM']) test(`modtest nested native cancellation ${signal} completes inner cleanup`, async t => {
  const f = await fixture(t, 'absent'), actual = await f.interrupt('spago', signal, true);
  assert.equal(actual.result.timedOut, false, actual.log);
  assert.equal(actual.result.code, signal === 'SIGINT' ? 130 : 143, actual.log);
  assert.equal(alive(actual.state.pid), false, 'phase exited');
  assert.equal(alive(actual.state.descendant), false, 'descendant exited');
  assert.match(actual.log, /Native artifacts retained:/);
  assert.equal(JSON.parse(await readFile(join(actual.workspace, 'results.json'), 'utf8')).success, false);
  await f.preserved();
});
