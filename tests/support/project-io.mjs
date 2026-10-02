import assert from 'node:assert/strict';
import { chmod, mkdir, readFile, rm, stat, writeFile } from 'node:fs/promises';
import * as Aff from '../../output/Effect.Aff/index.js';
import { Left } from '../../output/Data.Either/index.js';
import { Nothing } from '../../output/Data.Maybe/index.js';
import * as Project from '../../output/Sharpurs.Project/index.js';

const runAff = action => new Promise((resolve, reject) => {
  Aff.runAff(result => () => result instanceof Left ? reject(result.value0) : resolve(result.value0))(action)();
});
const moduleAction = Project.writeModule('Example')({ fsharp: '', csharp: Nothing.value })('let value = box 42');
const finalize = () => Project.finalize({ mainModule: 'Example', modules: [] });

export async function checkProjectIO(directory) {
  const previous = process.cwd();
  const errors = [], skipped = [];
  const expectError = async (label, action, code, path) => {
    await assert.rejects(runAff(action), error => {
      assert.equal(error.code, code, label);
      assert.equal(error.path, path, label);
      assert.ok(error.message.includes(path), label + ': diagnostic names path');
      assert.equal(error.cause.code, code, label + ': original Node error retained');
      errors.push({ label, code, path, syscall: error.syscall, message: error.message });
      return true;
    }, label);
  };
  async function permission(path, mode, action) {
    const original = (await stat(path)).mode;
    await chmod(path, mode);
    try { await action(); } finally { await chmod(path, original); }
  }
  await mkdir(directory);
  process.chdir(directory);
  try {
    await expectError('missing output parent is a creation failure', Project.prepare, 'ENOENT', 'output/Main');
    await writeFile('output', 'not a directory');
    await expectError('non-directory output parent is not absent', Project.prepare, 'ENOTDIR', 'output/Main');
    await rm('output');
    await mkdir('output');
    await writeFile('output/Main', 'application file');
    await expectError('file at output directory is not an existing directory', Project.prepare, 'EEXIST', 'output/Main');
    assert.equal(await readFile('output/Main', 'utf8'), 'application file');
    await rm('output/Main');
    await runAff(Project.prepare);
    const prelude = await stat('output/Main/Sharpurs_Prelude.fs', { bigint: true });
    await runAff(Project.prepare);
    assert.equal((await stat('output/Main/Sharpurs_Prelude.fs', { bigint: true })).mtimeNs, prelude.mtimeNs);
    const emitted = await runAff(moduleAction);
    assert.equal(emitted.fsharp, 'Example.fs');
    assert.ok(emitted.csharp instanceof Nothing);
    await runAff(Project.finalize({ mainModule: 'Example', modules: [emitted] }));
    assert.doesNotMatch(await readFile('output/Main/Program.fsproj', 'utf8'), /ProjectReference/);

    await mkdir('sharp.packages.props');
    await expectError('directory reference fragment is not absent', finalize(), 'EISDIR', 'sharp.packages.props');
    await rm('sharp.packages.props', { recursive: true });
    await mkdir('output/Main/Blocked.fs');
    await expectError('output read error is not followed by a write', Project.writeModule('Blocked')({ fsharp: '', csharp: Nothing.value })(''), 'EISDIR', 'output/Main/Blocked.fs');
    await mkdir('output/Main/FFI.CSharp.csproj');
    // unlink(directory) is EPERM on macOS and EISDIR on Linux.
    await expectError('obsolete project removal failure is visible', finalize(), process.platform === 'darwin' ? 'EPERM' : 'EISDIR', 'output/Main/FFI.CSharp.csproj');
    await rm('output/Main/FFI.CSharp.csproj', { recursive: true });

    if (process.getuid?.() !== 0) {
      await writeFile('sharp.packages.props', '    <Reference Include="Application" />');
      await permission('sharp.packages.props', 0o000, () => expectError('unreadable reference fragment', finalize(), 'EACCES', 'sharp.packages.props'));
      await rm('sharp.packages.props');
      await permission('output/Main/Example.fs', 0o200, () => expectError('writeable but unreadable output is not overwritten', moduleAction, 'EACCES', 'output/Main/Example.fs'));
      const original = await readFile('output/Main/Example.fs', 'utf8');
      await permission('output/Main/Example.fs', 0o400, async () => {
        await runAff(moduleAction); // unchanged read-only output needs no write
        await expectError('changed read-only output', Project.writeModule('Example')({ fsharp: '', csharp: Nothing.value })('let value = box 43'), 'EACCES', 'output/Main/Example.fs');
      });
      assert.equal(await readFile('output/Main/Example.fs', 'utf8'), original);
      await permission('output/Main', 0o500, () => expectError('new output cannot be created', Project.writeModule('New')({ fsharp: '', csharp: Nothing.value })(''), 'EACCES', 'output/Main/New.fs'));
      await rm('output/Main', { recursive: true });
      await permission('output', 0o500, () => expectError('cannot create output directory', Project.prepare, 'EACCES', 'output/Main'));
    } else skipped.push('POSIX permission failures: root bypasses mode bits');
    return { errors, skipped };
  } finally {
    process.chdir(previous);
  }
}
