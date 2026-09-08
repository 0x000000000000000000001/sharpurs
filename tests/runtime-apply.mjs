// Run with: DOTNET=/path/to/dotnet node tests/runtime-apply.mjs
import { mkdtemp, readFile, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { spawnSync } from "node:child_process";
import { appendFfiWrappersImpl } from "../src/Sharpurs/FfiSupport.js";

const read = (path) => readFile(new URL(path, import.meta.url), "utf8");
const main = await read("../src/Main.purs");
const prelude = main.match(/^fsPrelude = """\r?\n([\s\S]*?)^"""/m)?.[1];
if (!prelude) throw new Error("Cannot extract fsPrelude from src/Main.purs");

const exceptionFfi = appendFfiWrappersImpl("Effect.Exception")([
  "throwException", "catchException",
])(await read("../../sharpurs-exceptions/src/Effect/Exception.fs"));
const fixtureFfi = appendFfiWrappersImpl("RuntimeFixture")([
  "increment", "sum", "returnFunction",
])(`let increment (value: int) = value + 1
let sum (left: int) (right: int) = left + right
let returnFunction = fun (left: obj) -> fun (right: obj) ->
    box (unbox<int> left + unbox<int> right)
`);
const assertions = await read("./runtime-apply.fsx");
const directory = await mkdtemp(join(tmpdir(), "sharpurs-runtime-apply-"));
try {
  const script = join(directory, "runtime-apply.fsx");
  // Match the generated prelude's suppression for the existing Exception.fs type test.
  await writeFile(script, ['#nowarn "67"', prelude, exceptionFfi, fixtureFfi, assertions].join("\n"));
  const result = spawnSync(process.env.DOTNET || "dotnet", [
    "fsi", "--nologo", "--exec", script,
  ], { stdio: "inherit", timeout: 30_000 });
  if (result.error) throw result.error;
  if (result.status !== 0) throw new Error(`Runtime assertions failed (${result.signal || result.status})`);
} finally {
  await rm(directory, { recursive: true, force: true });
}
