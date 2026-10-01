import { parseArgs } from 'node:util';
import { compareGeneration } from './support/generation.mjs';

async function main() {
  const { values } = parseArgs({ options: {
    before: { type: 'string' }, after: { type: 'string' }, workspace: { type: 'string' },
    main: { type: 'string', default: 'Main' }, ffi: { type: 'string' }, artifacts: { type: 'string' }, help: { type: 'boolean' },
  } });
  if (values.help) {
    console.log('Usage: npm run compare:generation -- --before BUNDLE --after BUNDLE --workspace TYPED_WORKSPACE [--main Test.Main] [--ffi DIRECTORY] [--artifacts NEW_DIRECTORY]');
    return;
  }
  for (const name of ['before', 'after', 'workspace']) if (!values[name]) throw new Error(`Missing --${name}; use --help`);
  const controller = new AbortController();
  const cancel = () => controller.abort();
  process.on('SIGINT', cancel);
  process.on('SIGTERM', cancel);
  try {
    const result = await compareGeneration({ ...values, ffiDirectory: values.ffi, signal: controller.signal });
    process.exitCode = controller.signal.aborted ? 130 : result.success ? 0 : 1;
  } finally {
    process.off('SIGINT', cancel);
    process.off('SIGTERM', cancel);
  }
}
main().catch(error => { console.error(error.message); process.exitCode = 1; });
