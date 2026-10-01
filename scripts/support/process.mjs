import { open, mkdir, mkdtemp, writeFile } from 'node:fs/promises';
import { spawn } from 'node:child_process';
import { tmpdir, homedir } from 'node:os';
import { delimiter, dirname, join, resolve } from 'node:path';

export function toolEnvironment(root, env = process.env) {
  return { ...env, PATH: [join(root, 'node_modules/.bin'), join(homedir(), '.dotnet'), env.PATH || ''].join(delimiter) };
}

export async function artifactDirectory(requested, prefix) {
  if (!requested) return mkdtemp(join(tmpdir(), prefix));
  const path = resolve(requested);
  await mkdir(dirname(path), { recursive: true });
  await mkdir(path, { recursive: false }); // Existing evidence is never overwritten.
  return path;
}

export const writeJson = (path, value) => writeFile(path, JSON.stringify(value, null, 2) + '\n');

// A separate process per step, with unbounded log size and explicit failure,
// signal and timeout results. Cancellation reaches the subprocess group too.
export async function runLogged(program, args, { cwd, env, log, timeout = 0, signal } = {}) {
  const file = await open(log, 'w');
  await file.write(`$ ${program} ${args.join(' ')}\n`);
  const started = Date.now();
  let error, timedOut = false, timer, killTimer;
  try {
    const child = spawn(program, args, { cwd, env, detached: process.platform !== 'win32', stdio: ['ignore', file.fd, file.fd] });
    const kill = sig => {
      if (!child.pid) return;
      try {
        if (process.platform === 'win32') child.kill(sig);
        else process.kill(-child.pid, sig);
      } catch (cause) { if (cause.code !== 'ESRCH') throw cause; }
    };
    const stop = () => {
      if (killTimer) return;
      kill('SIGTERM');
      killTimer = setTimeout(() => kill('SIGKILL'), 2000);
    };
    if (timeout) timer = setTimeout(() => { timedOut = true; stop(); }, timeout);
    signal?.addEventListener('abort', stop, { once: true });
    if (signal?.aborted) stop();
    const result = await new Promise(resolve => {
      child.on('error', cause => { error = cause.message; });
      child.on('close', (code, childSignal) => resolve({ code, signal: childSignal }));
    });
    signal?.removeEventListener('abort', stop);
    if (error) await file.write(error + '\n');
    return { command: [program, ...args], ...result, error, timedOut, cancelled: !!signal?.aborted,
      ok: result.code === 0 && !error && !timedOut && !signal?.aborted, durationMs: Date.now() - started, log };
  } finally {
    clearTimeout(timer);
    clearTimeout(killTimer);
    await file.close();
  }
}
