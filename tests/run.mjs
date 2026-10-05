import { readFile } from 'node:fs/promises';
import { dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { parseArgs } from 'node:util';
import { artifactDirectory, runLogged, toolEnvironment, writeJson } from '../scripts/support/process.mjs';
import { buildSteps } from '../scripts/build.mjs';

const root = resolve(dirname(fileURLToPath(import.meta.url)), '..');

export async function runChecks({ directory = root, artifacts, suites = [], fixtures = [], allFixtures = false,
  build = true, assertions = true, env = process.env, signal, report = console.log } = {}) {
  const scripts = JSON.parse(await readFile(join(directory, 'package.json'), 'utf8')).scripts;
  const available = Object.keys(scripts).filter(name => name.startsWith('test:'));
  const selected = [...new Set(suites.length ? suites.map(name => name.startsWith('test:') ? name : `test:${name}`) : available)];
  for (const name of selected) if (!available.includes(name)) throw new Error(`Unknown suite ${name}; choose ${available.join(', ')}`);
  const tasks = selected.map(name => {
    const args = scripts[name].split(/\s+/);
    if (args.shift() !== 'node') throw new Error(`${name} must be a direct node command`);
    return { name, program: process.execPath, args };
  });
  if (allFixtures && fixtures.length) throw new Error('Choose named fixtures or --all-fixtures');
  if (assertions) tasks.unshift({ name: 'purescript', program: env.SPAGO || 'spago', args: ['test'] });
  if (build) tasks.unshift(...buildSteps(env).map(task => ({ ...task, prerequisite: true })));
  if (allFixtures || fixtures.length) tasks.push({ name: 'cli-fixtures', program: join(directory, 'bin/test'), args: fixtures });
  const destination = await artifactDirectory(artifacts, 'sharpurs-check-');
  const results = [];
  const summary = { artifacts: destination, node: { version: process.version, executable: process.execPath },
    requested: tasks.map(task => task.name), results, success: false };
  report(`Artifacts: ${destination}`);
  try {
    for (const task of tasks) {
      if (signal?.aborted) break;
      const result = { name: task.name, ...await runLogged(task.program, task.args, {
        cwd: directory, env: toolEnvironment(directory, env), signal, log: join(destination, task.name.replaceAll(':', '-') + '.log'),
      }) };
      results.push(result);
      report(`${result.ok ? 'PASS' : 'FAIL'} ${task.name} — ${result.log}`);
      await writeJson(join(destination, 'results.json'), summary);
      if (!result.ok && task.prerequisite) break;
    }
    summary.success = results.length === tasks.length && results.every(result => result.ok);
  } catch (error) { summary.error = error.message; }
  await writeJson(join(destination, 'results.json'), summary);
  if (summary.error) report(`FAIL ${summary.error}`);
  report(`Summary: ${results.filter(result => result.ok).length} passed, ${results.filter(result => !result.ok).length} failed, ${tasks.length - results.length} not run.`);
  return summary;
}

async function main() {
  const { values } = parseArgs({ options: {
    suite: { type: 'string', multiple: true }, fixture: { type: 'string', multiple: true },
    'all-fixtures': { type: 'boolean' }, 'skip-build': { type: 'boolean' }, 'skip-assertions': { type: 'boolean' },
    artifacts: { type: 'string' }, help: { type: 'boolean' },
  } });
  if (values.help) {
    console.log('Usage: npm test -- [--suite NAME ...] [--fixture NAME ... | --all-fixtures] [--skip-build] [--skip-assertions] [--artifacts NEW_DIRECTORY]');
    return;
  }
  const controller = new AbortController();
  const cancel = () => controller.abort();
  process.on('SIGINT', cancel);
  process.on('SIGTERM', cancel);
  try {
    const result = await runChecks({ suites: values.suite, fixtures: values.fixture, allFixtures: values['all-fixtures'],
      build: !values['skip-build'], assertions: !values['skip-assertions'], artifacts: values.artifacts, signal: controller.signal });
    process.exitCode = controller.signal.aborted ? 130 : result.success ? 0 : 1;
  } finally {
    process.off('SIGINT', cancel);
    process.off('SIGTERM', cancel);
  }
}
if (process.argv[1] && resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  main().catch(error => { console.error(error.message); process.exitCode = 1; });
}
