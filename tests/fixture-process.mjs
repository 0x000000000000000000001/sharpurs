// Observable process/workspace failures: diagnostics survive and cwd never leaks.
import assert from 'node:assert/strict';
import { access, mkdir, mkdtemp, readFile, realpath, rm, writeFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { join, relative } from 'node:path';
import { test } from 'node:test';
import { command, compileFixtures, withCwd, withFixtureDirectory } from './support/fixtures.mjs';
import { optimizeCoreFn } from './support/corefn.mjs';

async function scratch(t) {
  const directory = await realpath(await mkdtemp(join(tmpdir(), 'sharpurs-fixture-process-')));
  const previous = process.cwd();
  t.after(async () => { process.chdir(previous); await rm(directory, { recursive: true, force: true }); });
  return directory;
}

test('checked commands preserve spaced arguments, cwd and both output streams', async t => {
  const directory = await scratch(t), transcript = [];
  const result = command(process.execPath, ['-e',
    'console.log(JSON.stringify({cwd:process.cwd(),arg:process.argv[1]}));console.error("stderr marker")', 'a b=é'], directory, { transcript });
  assert.deepEqual(JSON.parse(result.stdout), { cwd: directory, arg: 'a b=é' });
  assert.match(transcript.join('\n'), /stderr marker/);
  assert.match(transcript.join('\n'), /a b=é/);
});

test('nonzero commands and missing executables stop their dependent action', async t => {
  const directory = await scratch(t), transcript = [];
  let reached = false;
  assert.throws(() => {
    command(process.execPath, ['-e', 'console.log("out marker");console.error("err marker");process.exit(7)'], directory, { transcript });
    reached = true;
  }, /failed \(7\):[\s\S]*out marker[\s\S]*err marker/);
  assert.equal(reached, false);
  assert.throws(() => command(join(directory, 'absent tool'), [], directory, { transcript }), { code: 'ENOENT' });
  assert.equal(transcript.length, 2);
  assert.match(transcript[1], /absent tool/);
});

test('a caller timeout terminates the command and preserves its partial output', async t => {
  const directory = await scratch(t), transcript = [];
  assert.throws(() => command(process.execPath, ['-e', 'console.log("started");setInterval(()=>{},1000)'], directory,
    { transcript, timeout: 500 }), { code: 'ETIMEDOUT' });
  assert.match(transcript.join('\n'), /started/);
});

test('cwd scopes restore after an asynchronous rejection', async t => {
  const directory = await scratch(t), previous = process.cwd(), failure = new Error('scope failure');
  await assert.rejects(withCwd(directory, async () => {
    assert.equal(process.cwd(), directory);
    await Promise.resolve();
    throw failure;
  }), error => error === failure);
  assert.equal(process.cwd(), previous);
});

test('successful fixture work returns its value, publishes its log and removes the temporary', async t => {
  const root = await scratch(t), previous = process.cwd();
  let workspace;
  const result = await withFixtureDirectory('sharpurs-process-success-',
    { artifacts: relative(previous, join(root, 'saved artifacts')), transcript: ['success marker'] }, async directory => {
      workspace = directory;
      process.chdir(directory);
      return 42;
    });
  assert.equal(result, 42);
  assert.equal(process.cwd(), previous);
  assert.match(await readFile(join(root, 'saved artifacts/validation.log'), 'utf8'), /success marker/);
  await assert.rejects(access(workspace), { code: 'ENOENT' });
});

test('real compile failure retains sources and complete diagnostics before cleanup', async t => {
  const root = await scratch(t), previous = process.cwd(), artifacts = join(root, 'failed compile'), transcript = [];
  let workspace, reached = false;
  await assert.rejects(withFixtureDirectory('sharpurs-process-compile-', { artifacts, transcript }, async directory => {
    workspace = directory;
    await writeFile(join(directory, 'Broken.purs'), 'module Broken where\nvalue =\n');
    compileFixtures(directory, [join(directory, 'Broken.purs')], { transcript });
    reached = true;
  }), /failed/);
  assert.equal(reached, false);
  assert.equal(process.cwd(), previous);
  const log = await readFile(join(artifacts, 'validation.log'), 'utf8');
  assert.match(log, /compile/);
  assert.match(log, /Broken.purs/);
  assert.match(log, /Error|error/);
  assert.equal(await readFile(join(artifacts, 'workspace/Broken.purs'), 'utf8'), 'module Broken where\nvalue =\n');
  await assert.rejects(access(workspace), { code: 'ENOENT' });
});

test('real Builder callbacks and missing callbacks restore cwd and keep fixture-local metadata', async t => {
  const directory = await scratch(t), previous = process.cwd();
  await writeFile(join(directory, 'Present.purs'), 'module Present where\nvalue :: Int\nvalue = 42\n');
  compileFixtures(directory, [join(directory, 'Present.purs')]);
  const captured = await optimizeCoreFn(relative(previous, directory), ['Present']);
  assert.equal(captured.get('Present').core.name, 'Present');
  assert.equal(captured.get('Present').backend.name, 'Present');
  assert.equal(process.cwd(), previous);
  await access(join(directory, '.purmeta'));
  await assert.rejects(optimizeCoreFn(directory, ['Missing']), /Builder did not reach Missing/);
  assert.equal(process.cwd(), previous);
});

test('a Builder reader failure restores cwd before it propagates', async t => {
  const directory = await scratch(t), previous = process.cwd();
  await mkdir(join(directory, 'output/Invalid'), { recursive: true });
  await writeFile(join(directory, 'output/Invalid/corefn.json'), '{broken json');
  await assert.rejects(optimizeCoreFn(directory, ['Invalid']));
  assert.equal(process.cwd(), previous);
});

test('a failed assembled program is retained without an explicit artifact destination', async t => {
  await scratch(t);
  const previous = process.cwd(), failure = new Error('runtime failure');
  let workspace;
  await assert.rejects(withFixtureDirectory('sharpurs-process-retained-', { transcript: ['runtime stdout'] }, async directory => {
    workspace = directory;
    t.after(() => rm(directory, { recursive: true, force: true }));
    await writeFile(join(directory, 'program.fsx'), 'failwith "fixture"\n');
    process.chdir(directory);
    throw failure;
  }), error => error === failure);
  assert.equal(process.cwd(), previous);
  assert.equal(await readFile(join(workspace, 'program.fsx'), 'utf8'), 'failwith "fixture"\n');
  assert.match(await readFile(join(workspace, 'validation.log'), 'utf8'), /runtime stdout[\s\S]*runtime failure/);
});

for (const suiteFails of [true, false]) test(`artifact publication failure retains the workspace (${suiteFails ? 'original failure wins' : 'success becomes failure'})`, async t => {
  const root = await scratch(t), previous = process.cwd(), artifacts = join(root, 'not a directory');
  await writeFile(artifacts, 'original file');
  const failure = new Error('original suite failure');
  let workspace;
  await assert.rejects(withFixtureDirectory('sharpurs-process-publish-', { artifacts }, async directory => {
    workspace = directory;
    t.after(() => rm(directory, { recursive: true, force: true }));
    await writeFile(join(directory, 'program.fsx'), 'printfn "retained"\n');
    process.chdir(directory);
    if (suiteFails) throw failure;
  }), error => suiteFails ? error === failure : error.code === 'EEXIST');
  assert.equal(process.cwd(), previous);
  await access(join(workspace, 'program.fsx'));
  await access(join(workspace, 'validation.log'));
  assert.equal(await readFile(artifacts, 'utf8'), 'original file');
});
