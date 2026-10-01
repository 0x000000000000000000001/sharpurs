// Shared mechanics only. Each suite owns its inputs, assertions and oracle.
import assert from 'node:assert/strict';
import { cp, readdir, writeFile } from 'node:fs/promises';
import { join } from 'node:path';
import { spawnSync } from 'node:child_process';

export function command(program, args, cwd, { transcript, timeout = 60_000, check = true } = {}) {
  const result = spawnSync(program, args, { cwd, encoding: 'utf8', timeout });
  transcript?.push(`$ ${program} ${args.join(' ')}\n${result.stdout || ''}${result.stderr || ''}`);
  if (result.error) throw result.error;
  if (check) assert.equal(result.status, 0, `${program} failed (${result.signal || result.status}):\n${result.stdout}\n${result.stderr}`);
  return result;
}

export async function packageSource(backend, name) {
  if (name === 'prelude' && process.env.PRELUDE_SRC) return process.env.PRELUDE_SRC;
  const packages = join(backend, '.spago/p');
  const found = (await readdir(packages)).find(entry => entry.startsWith(name + '-'));
  assert.ok(found, `Missing ${name} in ${packages}; run spago build first`);
  return join(packages, found, 'src');
}

export async function copyFixtures(source, directory, files, { javascript = true } = {}) {
  for (const file of files) await cp(join(source, file), join(directory, file));
  if (javascript) await writeFile(join(directory, 'package.json'), '{"type":"module"}\n');
}

export function compileFixtures(directory, sources, options) {
  return command(process.env.PURS || 'purs', ['compile', ...sources,
    '--output', join(directory, 'output'), '--codegen', 'corefn,js'], directory, options);
}

export function runFsharp(directory, script, { optimize = false, ...options } = {}) {
  return command(process.env.DOTNET || 'dotnet', ['fsi', '--nologo',
    ...(optimize ? ['--optimize+'] : []), '--exec', script], directory, options);
}
