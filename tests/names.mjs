// Real TAST -> production CLI -> mixed F#/C# project. Results are checked by
// independent native values/UTF-16 keys, including a Unicode module entrypoint.
import assert from 'node:assert/strict';
import { cp, mkdir, mkdtemp, readFile, readdir, rm, writeFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { command } from './support/fixtures.mjs';

const backend = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const fixtures = join(backend, 'tests/fixtures/names');
const artifacts = process.env.NAMES_ARTIFACTS && resolve(process.env.NAMES_ARTIFACTS);
const compiler = process.env.NAMES_COMPILER || join(backend, 'bin/sharpurs.js');
const directory = await mkdtemp(join(tmpdir(), 'sharpurs-names-'));
const transcript = [];
try {
  await cp(fixtures, join(directory, 'src'), { recursive: true });
  await writeFile(join(directory, 'package.json'), '{"type":"module"}\n');
  const compiled = command(process.env.PURS || 'purs', ['compile', 'src/*.purs', '--codegen', 'corefn'], directory, { transcript });
  assert.doesNotMatch(compiled.stdout + compiled.stderr, /Warning (?:found|\d+ of)/, 'naming fixture compiles without PureScript warnings');
  const core = JSON.parse(await readFile(join(directory, 'output/Naming.Valéurs/corefn.json'), 'utf8'));
  assert.ok(core.dataDecls && core.classDecls, 'fixture is real enriched TAST');
  command(process.execPath, ['--stack-size=65536', compiler, '--main', 'Naming.Entrée'], directory, { transcript });
  const output = join(directory, 'output/Main');
  const native = await readFile(join(output, 'Naming.Native.fs'), 'utf8');
  assert.match(native, /let rec Naming_Native_read_prime_adt_native /, 'primed native worker selected');
  const values = await readFile(join(output, 'Naming.Valéurs.fs'), 'utf8');
  assert.match(values, /let Naming_Val_u00e9_urs_direct_prime_direct /, 'primed direct helper selected');
  assert.match(values, /let rec Naming_Val_u00e9_urs_recursive_prime_tco /, 'primed generic recursive worker selected');
  const result = command(process.env.DOTNET || 'dotnet', ['run', '-c', 'Release', '--nologo',
    '-p:NuGetAudit=false', '--project', 'output/Main/Program.fsproj'], directory, { transcript, timeout: 120_000 });
  const runtime = result.stdout.match(/names runtime: (\d+) checks passed/);
  assert.doesNotMatch(result.stdout + result.stderr, /warning FS\d+/, 'naming fixture compiles without F# warnings');
  assert.ok(runtime, 'compiled entrypoint completed all checks');
  assert.equal(Number(runtime[1]), 93, 'all naming, record and missing-FFI checks executed');
  console.log(runtime[0]);
} catch (error) {
  transcript.push(error.stack || String(error));
  throw error;
} finally {
  if (artifacts) {
    await mkdir(artifacts, { recursive: true });
    await cp(directory, join(artifacts, 'workspace'), { recursive: true });
    await writeFile(join(artifacts, 'validation.log'), transcript.join('\n'));
    await writeFile(join(artifacts, 'fixtures.json'), JSON.stringify((await readdir(fixtures)).sort(), null, 2) + '\n');
  }
  await rm(directory, { recursive: true, force: true });
}
