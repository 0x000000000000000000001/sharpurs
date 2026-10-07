// Fixture dependencies come from the built workspace's resolved lockfile, never
// from the order (or apparent newest version) of the installed cache entries.
import { readFile, readdir, stat } from 'node:fs/promises';
import { join, resolve } from 'node:path';

const version = /^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?(?:\+[0-9A-Za-z.-]+)?$/;
const own = (object, key) => object && Object.hasOwn(object, key) ? object[key] : undefined;
const recovery = 'Run spago build in that workspace or provide an explicit source directory.';

async function unresolved(backend, name, reason) {
  const packages = join(backend, '.spago/p');
  let entries;
  try { entries = await readdir(packages, { withFileTypes: true }); } catch (error) {
    if (error.code !== 'ENOENT') throw new Error(`Cannot inspect ${packages}: ${error.message}`, { cause: error });
    entries = [];
  }
  const candidates = entries.filter(entry => (entry.isDirectory() || entry.isSymbolicLink()) &&
    entry.name.startsWith(name + '-') && version.test(entry.name.slice(name.length + 1)))
    .map(entry => entry.name).sort();
  const cached = candidates.length > 1 ? ` Ambiguous cached versions: ${candidates.join(', ')}.`
    : candidates.length ? ` Cached ${candidates[0]} does not establish the configured version.` : '';
  throw new Error(`Cannot resolve fixture package ${name}: ${reason}.${cached} ${recovery}`);
}

async function sourceDirectory(path, name, origin) {
  let info;
  try { info = await stat(path); } catch (error) {
    throw new Error(`Cannot access source for ${name} (${origin}): ${path}: ${error.message}. ${recovery}`, { cause: error });
  }
  if (!info.isDirectory()) throw new Error(`Expected source directory for ${name} (${origin}): ${path}`);
  return path;
}

export async function packageSource(backend, name, { source } = {}) {
  if (typeof name !== 'string' || !/^[a-z][a-z0-9]*(?:-[a-z0-9]+)*$/.test(name)) {
    throw new Error(`Invalid fixture package name: ${name}`);
  }
  const explicit = source ?? (name === 'prelude' ? process.env.PRELUDE_SRC || undefined : undefined);
  if (explicit !== undefined) {
    if (typeof explicit !== 'string' || !explicit) throw new Error(`Empty or invalid explicit source directory for ${name}`);
    // Caller-relative overrides must be absolute before a suite changes cwd.
    return sourceDirectory(resolve(explicit), name, source === undefined ? 'PRELUDE_SRC' : 'explicit source');
  }
  backend = resolve(backend);
  const lockfile = join(backend, 'spago.lock');
  let text;
  try { text = await readFile(lockfile, 'utf8'); } catch (error) {
    if (error.code === 'ENOENT') return unresolved(backend, name, `missing ${lockfile}`);
    throw new Error(`Cannot read ${lockfile}: ${error.message}`, { cause: error });
  }
  let lock;
  try { lock = JSON.parse(text); } catch (error) {
    throw new Error(`Invalid JSON in ${lockfile}: ${error.message}. ${recovery}`, { cause: error });
  }
  const workspace = own(lock?.workspace?.packages, name);
  const dependency = own(lock?.packages, name);
  if (workspace !== undefined && dependency !== undefined) {
    throw new Error(`Ambiguous fixture package ${name} in ${lockfile}: both workspace and dependency entries. ${recovery}`);
  }
  const entry = workspace === undefined ? dependency : { type: 'local', path: workspace?.path };
  if (entry === undefined) return unresolved(backend, name, `no resolved entry in ${lockfile}`);
  let directory;
  if (entry?.type === 'registry') {
    if (typeof entry.version !== 'string' || !version.test(entry.version)) {
      throw new Error(`Invalid registry version for ${name} in ${lockfile}`);
    }
    directory = join(backend, '.spago/p', `${name}-${entry.version}`, 'src');
  } else if (entry?.type === 'local') {
    if (typeof entry.path !== 'string' || !entry.path) throw new Error(`Missing local path for ${name} in ${lockfile}`);
    directory = resolve(backend, entry.path, 'src');
  } else {
    throw new Error(`Unsupported dependency type ${JSON.stringify(entry?.type)} for ${name} in ${lockfile}; provide an explicit source directory.`);
  }
  return sourceDirectory(directory, name, `${lockfile}, ${entry.type}${entry.version ? ' ' + entry.version : ''}`);
}
