// Resolve actual filesystem layouts, including misleading/stale cache entries.
import assert from 'node:assert/strict';
import { test } from 'node:test';
import { mkdir, mkdtemp, readFile, realpath, rm, symlink, writeFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { join, resolve } from 'node:path';
import { pathToFileURL } from 'node:url';

const { packageSource } = await import(process.env.PACKAGE_SOURCE_ORACLE
  ? pathToFileURL(resolve(process.env.PACKAGE_SOURCE_ORACLE)) : './support/fixtures.mjs');
const registry = version => ({ type: 'registry', version, integrity: 'fixture', dependencies: [] });

async function fixture(t) {
  const directory = await realpath(await mkdtemp(join(tmpdir(), 'sharpurs-package-sources-')));
  const backend = join(directory, 'backend with spaces');
  await mkdir(backend);
  const previous = process.env.PRELUDE_SRC, cwd = process.cwd();
  delete process.env.PRELUDE_SRC;
  t.after(async () => {
    process.chdir(cwd);
    if (previous === undefined) delete process.env.PRELUDE_SRC;
    else process.env.PRELUDE_SRC = previous;
    await rm(directory, { recursive: true, force: true });
  });
  const sources = async path => {
    await mkdir(path, { recursive: true });
    await writeFile(join(path, 'Fixture.purs'), 'module Fixture where\n');
    return path;
  };
  const cache = (name, version) => sources(join(backend, '.spago/p', name + '-' + version, 'src'));
  const lock = (packages, workspace = { packages: {} }) => writeFile(join(backend, 'spago.lock'), JSON.stringify({ workspace, packages }) + '\n');
  return { directory, backend, sources, cache, lock };
}

test('one registry package uses its resolved lock version', async t => {
  const f = await fixture(t), source = await f.cache('prelude', '6.0.2');
  await f.lock({ prelude: registry('6.0.2') });
  assert.equal(await packageSource(f.backend, 'prelude'), source);
});

test('several cached versions select the pinned version, not the first or newest', async t => {
  const f = await fixture(t);
  await f.cache('prelude', '0.0.1');
  const source = await f.cache('prelude', '6.0.2');
  await f.cache('prelude', '99.0.0');
  await f.lock({ prelude: registry('6.0.2') });
  assert.equal(await packageSource(f.backend, 'prelude'), source);
});

test('cache creation order and lock updates cannot select a stale version', async t => {
  for (const order of [['6.0.2', '6.0.3'], ['6.0.3', '6.0.2']]) {
    const f = await fixture(t);
    for (const version of order) await f.cache('prelude', version);
    for (const version of ['6.0.2', '6.0.3']) {
      await f.lock({ prelude: registry(version) });
      assert.equal(await packageSource(f.backend, 'prelude'), join(f.backend, '.spago/p', 'prelude-' + version, 'src'));
    }
  }
});

test('a longer package prefix cannot stand in for the requested package', async t => {
  const f = await fixture(t);
  await f.cache('prelude-extra', '6.0.2');
  await f.lock({ prelude: registry('6.0.2') });
  await assert.rejects(packageSource(f.backend, 'prelude'), /source for prelude.*spago\.lock.*6\.0\.2/);
});

test('hyphenated package names still select exactly the configured package', async t => {
  const f = await fixture(t);
  await f.cache('foldable-traversable-extra', '6.0.0');
  const source = await f.cache('foldable-traversable', '6.0.0');
  await f.lock({ 'foldable-traversable': registry('6.0.0') });
  assert.equal(await packageSource(f.backend, 'foldable-traversable'), source);
});

test('a missing pinned version cannot fall back to an installed older version', async t => {
  const f = await fixture(t);
  await f.cache('prelude', '6.0.1');
  await f.lock({ prelude: registry('6.0.2') });
  await assert.rejects(packageSource(f.backend, 'prelude'), /source for prelude.*registry 6\.0\.2.*prelude-6\.0\.2/);
});

for (const shape of ['missing cache', 'missing src', 'src file', 'dangling src']) test(`missing package source: ${shape}`, async t => {
  const f = await fixture(t);
  await f.lock({ prelude: registry('6.0.2') });
  const parent = join(f.backend, '.spago/p/prelude-6.0.2');
  if (shape !== 'missing cache') await mkdir(parent, { recursive: true });
  if (shape === 'src file') await writeFile(join(parent, 'src'), 'not a source directory');
  if (shape === 'dangling src') await symlink('missing-directory', join(parent, 'src'));
  await assert.rejects(packageSource(f.backend, 'prelude'), error => {
    assert.match(error.message, /source.*prelude.*spago\.lock/);
    assert.ok(error.message.includes(join(parent, 'src')), error.message);
    return true;
  });
});

test('local dependencies use the locked path relative to the workspace', async t => {
  const f = await fixture(t), source = await f.sources(join(f.directory, 'local prelude/src'));
  await f.cache('prelude', '6.0.2');
  await f.lock({ prelude: { type: 'local', path: '../local prelude' } });
  assert.equal(await packageSource(f.backend, 'prelude'), source);
});

test('workspace packages resolve their own source directory', async t => {
  const f = await fixture(t), source = await f.sources(join(f.backend, 'packages/prelude/src'));
  await f.cache('prelude', '6.0.2');
  await f.lock({}, { packages: { prelude: { path: 'packages/prelude', core: { dependencies: [] } } } });
  assert.equal(await packageSource(f.backend, 'prelude'), source);
});

test('conflicting workspace and dependency entries diagnose ambiguity', async t => {
  const f = await fixture(t);
  await f.sources(join(f.backend, 'local/src'));
  await f.cache('prelude', '6.0.2');
  await f.lock({ prelude: registry('6.0.2') }, { packages: { prelude: { path: 'local' } } });
  await assert.rejects(packageSource(f.backend, 'prelude'), /Ambiguous.*prelude.*spago\.lock.*workspace and dependency/);
});

test('resolved overrides take precedence over the base package-set version', async t => {
  const f = await fixture(t), source = await f.cache('prelude', '6.0.2');
  await f.cache('prelude', '6.0.1');
  await f.lock({ prelude: registry('6.0.2') }, { package_set: { content: { prelude: '6.0.1' } }, extra_packages: { prelude: '6.0.2' } });
  assert.equal(await packageSource(f.backend, 'prelude'), source);
});

for (const versions of [[], ['6.0.2'], ['6.0.3', '6.0.2']]) test(`missing lockfile with ${versions.length} cached versions never guesses configuration`, async t => {
  const f = await fixture(t);
  for (const version of versions) await f.cache('prelude', version);
  await f.cache('prelude-extra', '6.0.2');
  await assert.rejects(packageSource(f.backend, 'prelude'), error => {
    assert.match(error.message, /Cannot resolve fixture package prelude: missing .*spago\.lock/);
    assert.match(error.message, /Run spago build/);
    if (versions.length === 2) assert.match(error.message, /Ambiguous cached versions: prelude-6\.0\.2, prelude-6\.0\.3/);
    assert.doesNotMatch(error.message, /prelude-extra/);
    return true;
  });
});

test('package-set membership alone is not a resolved fixture dependency', async t => {
  const f = await fixture(t);
  await f.cache('prelude', '6.0.2');
  await f.lock({}, { package_set: { content: { prelude: '6.0.2' } } });
  await assert.rejects(packageSource(f.backend, 'prelude'), /no resolved entry in .*spago\.lock/);
});

test('malformed lockfiles do not fall back to cache contents', async t => {
  const f = await fixture(t);
  await f.cache('prelude', '6.0.2');
  await writeFile(join(f.backend, 'spago.lock'), '{ invalid JSON');
  await assert.rejects(packageSource(f.backend, 'prelude'), /Invalid JSON in .*spago\.lock/);
});

test('unresolved versions and missing local paths cannot synthesize a source path', async t => {
  const f = await fixture(t);
  await f.cache('prelude', '6.0.2');
  for (const entry of [registry('^6.0.2'), registry(null), { type: 'local' }, { type: 'local', path: '' }]) {
    await f.lock({ prelude: entry });
    await assert.rejects(packageSource(f.backend, 'prelude'), /(?:Invalid registry version|Missing local path).*prelude.*spago\.lock/);
  }
});

test('unsupported lock source kinds require an explicit source instead of a registry fallback', async t => {
  const f = await fixture(t), source = await f.sources(join(f.directory, 'git checkout/src'));
  await f.cache('prelude', '6.0.2');
  await f.lock({ prelude: { type: 'git', url: 'https://example.invalid/prelude', rev: 'pinned' } });
  await assert.rejects(packageSource(f.backend, 'prelude'), /Unsupported dependency type "git".*prelude.*explicit source/);
  assert.equal(await packageSource(f.backend, 'prelude', { source }), source);
});

test('PRELUDE_SRC accepts spaces and bypasses absent metadata', async t => {
  const f = await fixture(t), source = await f.sources(join(f.directory, 'override sources/src'));
  process.env.PRELUDE_SRC = source;
  assert.equal(await packageSource(f.backend, 'prelude'), source);
});

test('caller-relative PRELUDE_SRC stays valid after changing cwd', async t => {
  const f = await fixture(t), source = await f.sources(join(f.directory, 'override sources/src'));
  process.chdir(f.directory);
  process.env.PRELUDE_SRC = 'override sources/src';
  const selected = await packageSource(f.backend, 'prelude');
  process.chdir(f.backend);
  assert.equal(selected, source);
  assert.match(await readFile(join(selected, 'Fixture.purs'), 'utf8'), /module Fixture/);
});

test('explicit source options override PRELUDE_SRC and work for other packages', async t => {
  const f = await fixture(t), source = await f.sources(join(f.directory, 'selected source/src'));
  process.env.PRELUDE_SRC = await f.sources(join(f.directory, 'different source/src'));
  assert.equal(await packageSource(f.backend, 'prelude', { source }), source);
  assert.equal(await packageSource(f.backend, 'partial', { source }), source);
});

test('PRELUDE_SRC does not override Partial resolution', async t => {
  const f = await fixture(t), source = await f.cache('partial', '4.0.0');
  process.env.PRELUDE_SRC = await f.sources(join(f.directory, 'override/src'));
  await f.lock({ partial: registry('4.0.0') });
  assert.equal(await packageSource(f.backend, 'partial'), source);
});

test('a missing explicit source fails before compilation even with a cached package', async t => {
  const f = await fixture(t);
  await f.cache('prelude', '6.0.2');
  await f.lock({ prelude: registry('6.0.2') });
  process.env.PRELUDE_SRC = join(f.directory, 'absent source/src');
  await assert.rejects(packageSource(f.backend, 'prelude'), /source for prelude \(PRELUDE_SRC\).*absent source/);
});

test('an empty PRELUDE_SRC is unset but an empty explicit source is invalid', async t => {
  const f = await fixture(t), source = await f.cache('prelude', '6.0.2');
  await f.lock({ prelude: registry('6.0.2') });
  process.env.PRELUDE_SRC = '';
  assert.equal(await packageSource(f.backend, 'prelude'), source);
  await assert.rejects(packageSource(f.backend, 'prelude', { source: '' }), /Empty or invalid explicit source/);
});
