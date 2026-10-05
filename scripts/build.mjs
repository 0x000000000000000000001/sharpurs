import { spawnSync } from 'node:child_process';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { toolEnvironment } from './support/process.mjs';

const root = resolve(dirname(fileURLToPath(import.meta.url)), '..');

export function buildSteps(env = process.env) {
  const program = env.SPAGO || 'spago';
  return [
    { name: 'build', program, args: ['build'] },
    { name: 'bundle', program, args: ['bundle', '--module', 'Main', '--platform', 'node', '--outfile', 'bin/sharpurs.js', '--bundle-type', 'app'] },
  ];
}

function main() {
  const env = toolEnvironment(root);
  for (const { program, args } of buildSteps(env)) {
    const result = spawnSync(program, args, { cwd: root, env, stdio: 'inherit' });
    if (result.error) {
      console.error(result.error.message);
      process.exitCode = 1;
      return;
    }
    if (result.signal) {
      process.kill(process.pid, result.signal);
      return;
    }
    if (result.status !== 0) {
      process.exitCode = result.status ?? 1;
      return;
    }
  }
}

if (process.argv[1] && resolve(process.argv[1]) === fileURLToPath(import.meta.url)) main();
