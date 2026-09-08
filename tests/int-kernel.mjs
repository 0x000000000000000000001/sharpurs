// Run after npm run build: DOTNET=/path/to/dotnet node tests/int-kernel.mjs
import { mkdtemp, readFile, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { spawnSync } from "node:child_process";
import * as K from "../output/Sharpurs.IntKernel/index.js";
import { printKernel } from "../output/Sharpurs.IntKernel.CodeGen/index.js";
import { printDecl } from "../output/Sharpurs.Printer/index.js";
import { Qualified } from "../output/PureScript.Backend.Optimizer.CoreFn/index.js";
import { Just, Nothing } from "../output/Data.Maybe/index.js";
import { intAdd } from "../output/Data.Semiring/foreign.js";
import { intSub } from "../output/Data.Ring/foreign.js";
import { intMod } from "../output/Data.EuclideanRing/foreign.js";

const read = (path) => readFile(new URL(path, import.meta.url), "utf8");
const main = await read("../src/Main.purs");
const prelude = main.match(/^fsPrelude = """\r?\n([\s\S]*?)^"""/m)?.[1];
if (!prelude) throw new Error("Cannot extract fsPrelude from src/Main.purs");

const lit = (value) => new K.IntLiteral(value);
const local = (level) => new K.IntLocal(level);
const binary = (operator, left, right) => new K.IntBinary(operator, left, right);
const equal = (left, right) => new K.IntEqual(left, right);
const branch = (condition, yes, no) => new K.IntIf(condition, yes, no);
const recur = (...args) => new K.IntTailCall(args);
const add = (left, right) => binary(K.IntAdd.value, left, right);
const sub = (left, right) => binary(K.IntSubtract.value, left, right);
const mod = (left, right) => binary(K.IntModulo.value, left, right);
const param = (level, name = "value") => ({
  level, name: name === null ? Nothing.value : new Just(name),
});
const emit = (name, args, body) => printDecl(printKernel(`KernelFixture_${name}`)({
  name: new Qualified(new Just("KernelFixture"), name), args, body,
}));

// Exercise the emitter directly; the PureScript suite tests the TAST converter.
// Nonconsecutive levels, repeated names and an anonymous binder must remain distinct.
const fixtures = [
  emit("add", [param(3), param(8)], add(local(3), local(8))),
  emit("subtract", [param(3), param(8)], sub(local(3), local(8))),
  emit("modulo", [param(3), param(8)], mod(local(3), local(8))),
  emit("minimum", [param(0, null)], lit(-2147483648)),
  emit("maximum", [param(0, null)], lit(2147483647)),
  emit("anonymous", [param(11, null)], local(11)),
  emit("chooseThen", [param(0)], branch(equal(local(0), lit(0)), lit(41), recur(local(0)))),
  emit("chooseElse", [param(0)], branch(equal(local(0), lit(0)), recur(local(0)), lit(42))),
  emit("nested", [param(0), param(1)],
    add(branch(equal(local(0), lit(0)), lit(10), lit(20)), local(1))),
  emit("deepTailRec", [param(0, "n"), param(1, "acc")],
    branch(equal(local(0), lit(0)), local(1),
      recur(sub(local(0), lit(1)), add(local(1), mod(local(0), lit(3)))))),
  emit("swap", [param(4, "n"), param(8, "value"), param(13, "value")],
    branch(equal(local(4), lit(0)), local(8),
      recur(sub(local(4), lit(1)), local(13), local(8)))),
];

// Expected answers come from the installed PureScript primitive implementation,
// especially Euclidean modulo (including divisor zero and minBound / -1).
const boundaries = [-2147483648, -2147483647, -1073741824, -5, -3, -2, -1,
  0, 1, 2, 3, 5, 1073741824, 2147483646, 2147483647];
const fsInt = (value) => `(${value})`;
const rows = boundaries.flatMap((left) => boundaries.map((right) =>
  [left, right, intAdd(left)(right), intSub(left)(right), intMod(left)(right)]
    .map(fsInt).join(", ")));
const reference = `let arithmeticCases : (int * int * int * int * int) array = [|\n${
  rows.map((row) => `    (${row})`).join("\n")
}\n|]`;
const assertions = await read("./int-kernel.fsx");
const directory = await mkdtemp(join(tmpdir(), "sharpurs-int-kernel-"));
try {
  const script = join(directory, "int-kernel.fsx");
  await writeFile(script, [prelude, ...fixtures, reference, assertions].join("\n\n"));
  const result = spawnSync(process.env.DOTNET || "dotnet", [
    "fsi", "--nologo", "--optimize+", "--exec", script,
  ], { stdio: "inherit", timeout: 30_000 });
  if (result.error) throw result.error;
  if (result.status !== 0) throw new Error(`IntKernel runtime assertions failed (${result.signal || result.status})`);
} finally {
  await rm(directory, { recursive: true, force: true });
}
