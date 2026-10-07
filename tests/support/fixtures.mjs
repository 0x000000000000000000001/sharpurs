// Shared mechanics only. Each suite owns its inputs, assertions and oracle.
import assert from 'node:assert/strict';
import { cp, mkdir, mkdtemp, rm, writeFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { join, resolve } from 'node:path';
import { spawnSync } from 'node:child_process';
export { packageSource } from './packages.mjs';

export function command(program, args, cwd, { transcript, timeout = 60_000 } = {}) {
  const result = spawnSync(program, args, { cwd, encoding: 'utf8', timeout });
  transcript?.push(`$ ${program} ${args.join(' ')}\n${result.stdout || ''}${result.stderr || ''}`);
  if (result.error) throw result.error;
  assert.equal(result.status, 0, `${program} failed (${result.signal || result.status}):\n${result.stdout}\n${result.stderr}`);
  return result;
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

// Cwd is process-global: callers must keep these operations sequential.
export async function withCwd(directory, action) {
  const previous = process.cwd();
  process.chdir(directory);
  try {
    return await action();
  } finally {
    process.chdir(previous);
  }
}

// Suites own their programs and optional success artifacts. On failure keep the
// entire workspace, including any assembled program, inputs and command log.
export async function withFixtureDirectory(prefix, { artifacts, transcript = [] }, action) {
  const previous = process.cwd();
  const destination = artifacts && resolve(artifacts);
  const directory = await mkdtemp(join(tmpdir(), prefix));
  let failed = false;
  try {
    return await action(directory);
  } catch (error) {
    failed = true;
    transcript.push(error.stack || String(error));
    throw error;
  } finally {
    try {
      process.chdir(previous);
      await writeFile(join(directory, 'validation.log'), transcript.join('\n') + '\n');
      if (destination) {
        await mkdir(destination, { recursive: true });
        if (failed) await cp(directory, join(destination, 'workspace'), { recursive: true });
        await cp(join(directory, 'validation.log'), join(destination, 'validation.log'));
      }
      if (failed) console.error(`Fixture failure artifacts: ${destination || directory}`);
      if (!failed || destination) await rm(directory, { recursive: true, force: true });
    } catch (error) {
      console.error(`Fixture workspace retained: ${directory}\n${error.stack || error}`);
      // A publication/cleanup error must not replace the original suite failure.
      if (!failed) throw error;
    }
  }
}
