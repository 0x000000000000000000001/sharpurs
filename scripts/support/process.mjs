import { open, mkdir, mkdtemp, writeFile } from 'node:fs/promises';
import { realpathSync } from 'node:fs';
import { spawn } from 'node:child_process';
import { tmpdir, homedir } from 'node:os';
import { delimiter, dirname, join, resolve } from 'node:path';

function toolPath(path) {
  const absolute = resolve(path);
  try { return realpathSync(absolute); } catch (error) {
    if (error.code === 'ENOENT' || error.code === 'ENOTDIR') return absolute;
    throw error;
  }
}

export function toolEnvironment(root, env = process.env) {
  // npm injects ancestor package bins as well. An unrelated legacy Spago there
  // must not shadow the toolchain chosen on PATH for this spago.yaml project.
  const ancestors = new Set();
  for (let parent = dirname(root); ; parent = dirname(parent)) {
    ancestors.add(toolPath(join(parent, 'node_modules/.bin')));
    if (parent === dirname(parent)) break;
  }
  const path = (env.PATH || '').split(delimiter).filter(entry => !ancestors.has(toolPath(entry)));
  return { ...env, PATH: [join(root, 'node_modules/.bin'), join(homedir(), '.dotnet'), ...path].join(delimiter) };
}

export async function artifactDirectory(requested, prefix) {
  if (!requested) return mkdtemp(join(tmpdir(), prefix));
  const path = resolve(requested);
  await mkdir(dirname(path), { recursive: true });
  await mkdir(path, { recursive: false }); // Existing evidence is never overwritten.
  return path;
}

export const writeJson = (path, value) => writeFile(path, JSON.stringify(value, null, 2) + '\n');

// Own one process group and return the same result for inherited and logged I/O.
// Abort reasons SIGINT/SIGTERM preserve a caller's signal; other aborts use TERM.
export async function runProcess(program, args, { cwd, env, stdio = 'inherit', timeout = 0, signal, killGraceMs = 2000 } = {}) {
  const started = Date.now();
  const summary = (result, error, timedOut) => ({ command: [program, ...args], ...result, error, timedOut,
    cancelled: !!signal?.aborted, ok: result.code === 0 && !error && !timedOut && !signal?.aborted,
    durationMs: Date.now() - started });
  if (signal?.aborted) return summary({ code: null, signal: null }, undefined, false);
  let error, timedOut = false, timer, killTimer;
  let abort;
  try {
    const child = spawn(program, args, { cwd, env, detached: process.platform !== 'win32', stdio });
    const kill = sig => {
      if (!child.pid) return;
      try {
        if (process.platform === 'win32') child.kill(sig);
        else process.kill(-child.pid, sig);
      } catch (cause) { if (cause.code !== 'ESRCH') throw cause; }
    };
    const stop = (sig = 'SIGTERM') => {
      if (killTimer) return;
      kill(sig);
      killTimer = setTimeout(() => kill('SIGKILL'), killGraceMs);
    };
    abort = () => stop(signal.reason === 'SIGINT' ? 'SIGINT' : 'SIGTERM');
    if (timeout) timer = setTimeout(() => { timedOut = true; stop(); }, timeout);
    signal?.addEventListener('abort', abort, { once: true });
    if (signal?.aborted) abort();
    const result = await new Promise(resolve => {
      child.on('error', cause => { error = cause.message; });
      child.on('close', (code, childSignal) => resolve({ code, signal: childSignal }));
    });
    // The direct child may exit before its descendants. Finish cancellation
    // before returning to the caller and cancelling the escalation timer.
    if (timedOut || signal?.aborted) kill('SIGKILL');
    return summary(result, error, timedOut);
  } finally {
    if (abort) signal?.removeEventListener('abort', abort);
    clearTimeout(timer);
    clearTimeout(killTimer);
  }
}

// File descriptors retain complete output without a pipe/buffer size limit.
export async function runLogged(program, args, { log, ...options } = {}) {
  const file = await open(log, 'w');
  try {
    await file.write(`$ ${program} ${args.join(' ')}\n`);
    const result = await runProcess(program, args, { ...options, stdio: ['ignore', file.fd, file.fd] });
    if (result.error) await file.write(result.error + '\n');
    return { ...result, log };
  } finally {
    await file.close();
  }
}
