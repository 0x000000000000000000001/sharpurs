import { createHash } from 'node:crypto';
import { copyFile, cp, mkdir, readFile, readdir, realpath, rm, stat, writeFile } from 'node:fs/promises';
import { basename, dirname, extname, join, relative, resolve } from 'node:path';
import { artifactDirectory, runLogged, writeJson } from './process.mjs';

const hash = bytes => createHash('sha256').update(bytes).digest('hex');
const exists = async path => { try { await stat(path); return true; } catch (error) { if (error.code === 'ENOENT') return false; throw error; } };

export async function fileManifest(directory, excluded = new Set()) {
  const entries = {};
  async function walk(path) {
    for (const item of (await readdir(path, { withFileTypes: true })).sort((a, b) => a.name.localeCompare(b.name))) {
      const file = join(path, item.name);
      if (item.isDirectory()) await walk(file);
      else if (!excluded.has(file)) {
        const metadata = await stat(file, { bigint: true });
        entries[relative(directory, file)] = { sha256: hash(await readFile(file)), size: Number(metadata.size), mtimeNs: String(metadata.mtimeNs) };
      }
    }
  }
  await walk(directory);
  return entries;
}

export function compareManifests(before, after) {
  const common = Object.keys(before).filter(name => name in after).sort();
  return {
    filesBefore: Object.keys(before).length, filesAfter: Object.keys(after).length,
    added: Object.keys(after).filter(name => !(name in before)).sort(),
    removed: Object.keys(before).filter(name => !(name in after)).sort(),
    changed: common.filter(name => before[name].sha256 !== after[name].sha256),
    rewrittenUnchanged: common.filter(name => before[name].sha256 === after[name].sha256 && before[name].mtimeNs !== after[name].mtimeNs),
  };
}

// Snapshot bytes rather than linking live dependencies. Keep native directory
// structure (including empty version directories) because FFI lookup observes it.
async function stageWorkspace(source, destination, ffiDirectory) {
  const workspace = join(destination, 'sandbox/workspace');
  const files = new Set(), directories = new Set(), origins = {};
  await mkdir(join(workspace, 'output/Main'), { recursive: true });
  async function snapshot(from, to) {
    await mkdir(dirname(to), { recursive: true });
    const bytes = await readFile(from);
    await writeFile(to, bytes);
    files.add(to);
    origins[relative(destination, to)] = { source: from, sha256: hash(bytes) };
  }
  async function nativeTree(from, to, ancestors = new Set()) {
    if (!await exists(from)) return;
    const real = await realpath(from);
    if (ancestors.has(real)) throw new Error(`Cyclic native resource directory: ${from}`);
    const next = new Set([...ancestors, real]);
    await mkdir(to, { recursive: true });
    directories.add(to);
    for (const item of await readdir(from)) {
      if (['.git', 'node_modules', 'output', 'output-es', 'bin', 'obj', '.purmeta'].includes(item)) continue;
      const path = join(from, item);
      if ((await stat(path)).isDirectory()) await nativeTree(path, join(to, item), next);
      else if (['.fs', '.cs'].includes(extname(item))) await snapshot(path, join(to, item));
    }
  }
  let count = 0;
  for (const name of (await readdir(join(source, 'output'))).sort()) {
    const from = join(source, 'output', name, 'corefn.json');
    // Some output entries are files, rather than module directories.
    if (!(await stat(join(source, 'output', name))).isDirectory() || !await exists(from)) continue;
    const bytes = await readFile(from);
    const core = JSON.parse(bytes);
    if (typeof core.modulePath !== 'string') throw new Error(`Missing modulePath in ${from}`);
    const originalPath = resolve(source, core.modulePath);
    const localDirectory = join(destination, 'sources', String(count++));
    await mkdir(localDirectory, { recursive: true });
    directories.add(localDirectory);
    core.modulePath = join(localDirectory, basename(originalPath));
    for (const extension of ['.purs', '.fs', '.cs']) {
      const original = originalPath.replace(/\.purs$/, extension);
      if (await exists(original)) await snapshot(original, core.modulePath.replace(/\.purs$/, extension));
    }
    const to = join(workspace, 'output', name, 'corefn.json');
    await mkdir(dirname(to), { recursive: true });
    await writeFile(to, JSON.stringify(core) + '\n');
    files.add(to);
    origins[relative(destination, to)] = { source: from, sha256: hash(bytes), modulePath: originalPath };
  }
  if (!count) throw new Error(`No typed CoreFn modules in ${join(source, 'output')}`);
  for (const path of ['.spago', 'spago.d', 'bak/spago.d/fs/p', 'src']) await nativeTree(join(source, path), join(workspace, path));
  await nativeTree(resolve(source, '../../bak/spago.d/fs/p'), join(destination, 'bak/spago.d/fs/p'));
  if (ffiDirectory) await nativeTree(resolve(source, ffiDirectory), join(workspace, 'ffi'));
  for (const name of await readdir(source)) {
    if (['.fs', '.cs'].includes(extname(name)) || name === 'sharp.packages.props') await snapshot(join(source, name), join(workspace, name));
  }
  await writeFile(join(workspace, 'package.json'), '{"type":"module"}\n');
  files.add(join(workspace, 'package.json'));
  await writeJson(join(destination, 'input-origins.json'), origins);
  return { workspace, files, directories, modules: count };
}

async function inputManifest(staged, destination) {
  const files = {}, directories = {};
  for (const file of [...staged.files].sort()) files[relative(destination, file)] = hash(await readFile(file));
  for (const directory of [...staged.directories].sort()) directories[relative(destination, directory)] = (await readdir(directory)).sort();
  return { files, directories };
}

export async function compareGeneration({ before, after, workspace, main = 'Main', ffiDirectory, artifacts,
  env = process.env, signal, report = console.log } = {}) {
  const destination = await artifactDirectory(artifacts, 'sharpurs-generation-');
  const result = { artifacts: destination, sourceWorkspace: resolve(workspace), main, runs: [], success: false };
  report(`Artifacts: ${destination}`);
  try {
    for (const [phase, source] of [['before', before], ['after', after]]) {
      const bytes = await readFile(resolve(source));
      await writeFile(join(destination, `compiler-${phase}.mjs`), bytes);
      result[phase + 'Compiler'] = { path: resolve(source), sha256: hash(bytes) };
    }
    const staged = await stageWorkspace(resolve(workspace), destination, ffiDirectory);
    result.modules = staged.modules;
    const inputs = await inputManifest(staged, destination);
    await writeJson(join(destination, 'inputs.json'), inputs);
    const output = join(staged.workspace, 'output/Main');
    const manifests = {};
    for (const phase of ['before', 'after-incremental', 'after-clean']) {
      if (signal?.aborted) throw new Error('Comparison cancelled');
      if (phase === 'after-clean') {
        // Preserve a possible Main/corefn.json input while removing every emitted
        // file. This detects outputs that the new compiler stopped emitting.
        for (const name of Object.keys(manifests['after-incremental'])) await rm(join(output, name));
      }
      const binary = phase === 'before' ? 'before' : 'after';
      const args = ['--expose-gc', '--stack-size=65536', '--max-old-space-size=16384',
        join(destination, `compiler-${binary}.mjs`), '--main', main, ...(ffiDirectory ? ['--ffi', 'ffi'] : [])];
      const run = { phase, ...await runLogged(process.execPath, args, {
        cwd: staged.workspace, env, signal, log: join(destination, phase + '.log'),
      }) };
      result.runs.push(run);
      if (!run.ok) throw new Error(`${phase} failed; see ${run.log}`);
      if (JSON.stringify(await inputManifest(staged, destination)) !== JSON.stringify(inputs)) throw new Error(`${phase} changed the snapshotted inputs`);
      manifests[phase] = await fileManifest(output, staged.files);
      if (!Object.keys(manifests[phase]).length) throw new Error(`${phase} emitted no files`);
      await writeJson(join(destination, phase + '.json'), manifests[phase]);
      if (phase !== 'after-incremental') await cp(output, join(destination, phase === 'before' ? 'generated-before' : 'generated-after'), { recursive: true, preserveTimestamps: true });
      report(`PASS ${phase}: ${Object.keys(manifests[phase]).length} files`);
    }
    result.incremental = compareManifests(manifests.before, manifests['after-incremental']);
    result.clean = compareManifests(manifests.before, manifests['after-clean']);
    const sameContent = diff => !diff.added.length && !diff.removed.length && !diff.changed.length;
    result.success = sameContent(result.incremental) && !result.incremental.rewrittenUnchanged.length && sameContent(result.clean);
    result.inputsUnchanged = true;
  } catch (error) { result.error = error.message; }
  await writeJson(join(destination, 'comparison.json'), result);
  report(`${result.success ? 'PASS' : 'FAIL'} generation comparison — ${join(destination, 'comparison.json')}`);
  return result;
}
