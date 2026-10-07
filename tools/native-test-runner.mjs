// Each invocation owns a fresh copy of the sibling layout. Never switch a
// checkout's profile or clear its (or another checkout's) generated files.
import { copyFile, cp, mkdir, mkdtemp, readdir, realpath, rm, stat } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { basename, dirname, join, relative, resolve, sep } from 'node:path';
import { fileURLToPath } from 'node:url';
import { buildSteps } from '../scripts/build.mjs';
import { runProcess, toolEnvironment, writeJson } from '../scripts/support/process.mjs';

const compiler = fileURLToPath(new URL('../', import.meta.url));
const excluded = new Set(['.git', 'node_modules', '.spago', '.cache', '.purmeta',
  'output', 'output-es', 'output.bak', '.sharpurs-cache.json']);
const controller = new AbortController();
const interrupt = () => controller.abort('SIGINT');
const terminate = () => controller.abort('SIGTERM');
process.on('SIGINT', interrupt);
process.on('SIGTERM', terminate);

function checkInterrupted() {
  if (controller.signal.aborted) throw new Error(`Interrupted by ${controller.signal.reason}`);
}

async function isFile(path) {
  try { return (await stat(path)).isFile(); } catch (error) {
    if (error.code === 'ENOENT') return false;
    throw error;
  }
}

async function copyInputs(source, target, skipProfile = false) {
  await cp(source, target, { recursive: true, dereference: true, filter: path => {
    checkInterrupted();
    const parts = relative(source, path).split(sep);
    return !parts.some(part => excluded.has(part)) &&
      !(parts[0] === 'tests' && parts[1] === 'runner') &&
      !(skipProfile && parts.length === 1 && parts[0] === 'spago.yaml');
  } });
}

async function prepare(source, workspace, clean) {
  const parent = dirname(source), layout = join(workspace, 'layout');
  const target = join(layout, basename(source)), backend = join(layout, 'sharpurs');
  let profile;
  for (const name of ['spago.sharp.yaml', 'spago.fs.yaml', 'spago.yaml']) {
    if (await isFile(join(source, name))) { profile = name; break; }
  }
  if (!profile) throw new Error(`No Spago profile in ${source}`);
  await mkdir(layout);
  // Retain sibling names and local overrides, including untracked maintained
  // inputs. Materialize links so generated writes cannot follow them home.
  for (const entry of (await readdir(parent, { withFileTypes: true })).sort((a, b) => a.name.localeCompare(b.name))) {
    if (!entry.name.startsWith('sharpurs-') || !(await stat(join(parent, entry.name))).isDirectory()) continue;
    await copyInputs(join(parent, entry.name), join(layout, entry.name), entry.name === basename(source));
  }
  await copyFile(join(source, profile), join(target, 'spago.yaml'));
  await mkdir(join(backend, 'bin'), { recursive: true });
  await copyFile(join(compiler, 'bin/sharpurs'), join(backend, 'bin/sharpurs'));
  if (clean) {
    // The compiler's checked-in layout has one non-sibling local dependency.
    // Rebuild privately too; tool executables are read from the installed checkout.
    for (const name of ['src', 'test', 'spago.yaml', 'spago.lock', 'package.json']) {
      await copyInputs(join(compiler, name), join(backend, name));
    }
    await copyInputs(resolve(compiler, '../../purescript-backend-optimizer-sharpurs'),
      join(workspace, 'purescript-backend-optimizer-sharpurs'));
  } else {
    await copyFile(join(compiler, 'bin/sharpurs.js'), join(backend, 'bin/sharpurs.js'));
  }
  // The compiler's fallback native overrides are part of the frozen layout.
  try { await copyInputs(join(compiler, 'bak'), join(backend, 'bak')); } catch (error) {
    if (error.code !== 'ENOENT' || error.path !== join(compiler, 'bak')) throw error;
  }
  checkInterrupted();
  return { target, backend, profile };
}

let workspace, success = false;
const report = { source: null, clean: false, steps: [], success: false };
try {
  if (!process.argv[2]) throw new Error('Usage: native-test-runner.mjs MODULE_DIRECTORY [-c|--clean]');
  const source = await realpath(resolve(process.argv[2]));
  if (!basename(source).startsWith('sharpurs-')) throw new Error(`Expected a sharpurs-* module: ${source}`);
  report.source = source;
  // Older wrappers ignored other arguments; retain that contract.
  report.clean = process.argv.slice(3).some(arg => arg === '-c' || arg === '--clean');
  const base = resolve(process.env.SHARPURS_NATIVE_ARTIFACTS || tmpdir());
  await mkdir(base, { recursive: true });
  workspace = await realpath(await mkdtemp(join(base, basename(source) + '-')));
  console.log(`Native workspace: ${workspace}`);
  const { target, backend, profile } = await prepare(source, workspace, report.clean);
  Object.assign(report, { workspace, target, backend, profile });
  const env = toolEnvironment(compiler);
  async function run(name, program, args, cwd) {
    checkInterrupted();
    console.log(`=== ${name} ===`);
    const result = await runProcess(program, args, { cwd, env, signal: controller.signal });
    report.steps.push({ name, cwd, ...result });
    checkInterrupted();
    if (!result.ok) throw new Error(`${name}: ${result.error || result.signal || 'exit ' + result.code}`);
  }
  if (report.clean) {
    for (const step of buildSteps(env)) await run('compiler-' + step.name, step.program, step.args, backend);
  }
  await run('Building project with spago', env.SPAGO || 'spago', ['build'], target);
  await run('Generating code with sharpurs', '../sharpurs/bin/sharpurs', ['--main', 'Test.Main'], target);
  let project;
  for (const extension of ['fsproj', 'csproj']) {
    const path = `output/Main/Program.${extension}`;
    if (await isFile(join(target, path))) { project = path; break; }
  }
  if (!project) throw new Error('No output/Main/Program.fsproj or Program.csproj found.');
  await run('Compiling and running .NET test binary', env.DOTNET || 'dotnet',
    ['run', '-c', 'Release', '-v', 'q', '--nologo', '--project', project], target);
  success = true;
} catch (error) {
  report.error = error.message;
  console.error(`[FAILED] ${error.message}`);
  process.exitCode = controller.signal.aborted ? (controller.signal.reason === 'SIGINT' ? 130 : 143) : 1;
} finally {
  if (workspace) {
    try {
      report.success = success;
      await writeJson(join(workspace, 'results.json'), report);
      if (success && !process.env.SHARPURS_NATIVE_ARTIFACTS) await rm(workspace, { recursive: true });
      else console.log(`Native artifacts retained: ${workspace}`);
    } catch (error) {
      success = false;
      process.exitCode = 1;
      report.success = false;
      report.error = `Native finalization: ${error.message}`;
      try { await writeJson(join(workspace, 'results.json'), report); } catch { /* The I/O diagnostic below remains authoritative. */ }
      console.error(`[FAILED] Native finalization: ${error.message}\nWorkspace: ${workspace}`);
    }
  }
  process.off('SIGINT', interrupt);
  process.off('SIGTERM', terminate);
}
if (controller.signal.aborted) process.exitCode = controller.signal.reason === 'SIGINT' ? 130 : 143;
else if (success) console.log('\n✅ Tests passed successfully!');
