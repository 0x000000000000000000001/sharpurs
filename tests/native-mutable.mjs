// Run after npm run build: DOTNET=/path/to/dotnet node tests/native-mutable.mjs
import assert from 'node:assert/strict';
import { spawnSync } from 'node:child_process';
import { mkdtempSync, rmSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import * as Mutable from '../output/Sharpurs.Native.Mutable/index.js';
import * as IR from '../output/Sharpurs.Native.IR/index.js';
import * as Types from '../output/Sharpurs.Native.Types/index.js';
import * as Map from '../output/Data.Map/index.js';
import * as CoreFn from '../output/PureScript.Backend.Optimizer.CoreFn/index.js';
import { Nothing } from '../output/Data.Maybe/index.js';

const dotnet = process.env.DOTNET || 'dotnet';
const treeType = new Types.NativeAdt('Probe_Tree', []);
const intType = Types.NativeInt.value;
const nodeCtor = {
  name: 'Probe_Node',
  sourceName: new CoreFn.Qualified(Nothing.value, 'Node'),
  typeName: 'Probe_Tree',
  fields: [intType, treeType],
};
const leafCtor = {
  name: 'Probe_Leaf',
  sourceName: new CoreFn.Qualified(Nothing.value, 'Leaf'),
  typeName: 'Probe_Tree',
  fields: [],
};
const layout = {
  moduleName: 'Probe',
  names: Map.empty,
  types: Map.singleton('Probe_Tree')([leafCtor, nodeCtor]),
  constructors: Map.empty,
};
const target = new IR.NativeVar('t', treeType);
const leaf = new IR.NativeVar('leaf', treeType);
const reuse = new IR.NativeReuse('Probe_Node', target, [new IR.NativeIntLit(2), leaf]);
const rendered = Mutable.renderExpr(reuse);
assert.equal(rendered.constructor.name, 'Just', 'mutable reuse must render');

const fsx = `module Probe

${Mutable.renderTypeDeclarations(layout)}

let leaf = Probe_Leaf
let f (t: Probe_Tree) = ${rendered.value0}

let fresh = f (Probe_Node 1 leaf)
let fallback = f leaf
let freshValue = match fresh with | Probe_Node_match(x, _) -> x | _ -> 0
let fallbackValue = match fallback with | Probe_Node_match(x, _) -> x | _ -> 0
let leafStillLeaf = match leaf with | Probe_Leaf_match -> true | _ -> false
printfn "%d %d %b" freshValue fallbackValue leafStillLeaf
`;

const directory = mkdtempSync(join(tmpdir(), 'sharpurs-native-mutable-'));
try {
  const script = join(directory, 'Probe.fs');
  writeFileSync(script, fsx);
  const result = spawnSync(dotnet, ['fsi', '--nologo', '--optimize+', '--exec', script], {
    cwd: directory,
    encoding: 'utf8',
  });
  assert.equal(result.status, 0, result.stderr || result.stdout);
  assert.match(result.stdout, /2 2 true/, 'fresh update, fallback copy and leaf preservation must hold');
  console.log('native mutable: 3 checks passed');
} finally {
  rmSync(directory, { recursive: true, force: true });
}
