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
import * as Maybe from '../output/Data.Maybe/index.js';

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

// Destination-passing / rotation probe. This uses the specialized class
// representation plus the real planner output, so it checks more than the
// hand-written `_reuse` probe above.
import * as Destination from '../output/Sharpurs.Native.Destination/index.js';
import * as Pattern from '../output/Sharpurs.Native.Pattern/index.js';
import * as Specialized from '../output/Sharpurs.Native.Specialized/index.js';
import * as Set from '../output/Data.Set/index.js';
import * as Tuple from '../output/Data.Tuple/index.js';
import * as Foldable from '../output/Data.Foldable/index.js';
import * as Ord from '../output/Data.Ord/index.js';

const mapFrom = (ord, entries) => Map.fromFoldable(ord)(Foldable.foldableArray)(entries);
const setFrom = (ord, entries) => Set.fromFoldable(Foldable.foldableArray)(ord)(entries);
const destModule = CoreFn.ModuleName('DestinationProbe');
const colorType = new Types.NativeAdt('Dest_Color', []);
const destTreeType = new Types.NativeAdt('Dest_Tree', []);
const constructor = (name, typeName, fields) => ({
  name,
  sourceName: new CoreFn.Qualified(new Maybe.Just(destModule), CoreFn.Ident(name)),
  typeName,
  fields,
});
const red = constructor('Dest_Red', 'Dest_Color', []);
const black = constructor('Dest_Black', 'Dest_Color', []);
const destLeaf = constructor('Dest_Leaf', 'Dest_Tree', []);
const destNode = constructor('Dest_Node', 'Dest_Tree', [colorType, destTreeType, intType, destTreeType]);
const destLayout = {
  moduleName: 'DestinationProbe',
  names: Map.empty,
  types: mapFrom(Ord.ordString, [
    new Tuple.Tuple('Dest_Color', [red, black]),
    new Tuple.Tuple('Dest_Tree', [destLeaf, destNode]),
  ]),
  constructors: mapFrom(CoreFn.ordQualified(CoreFn.ordIdent), [
    new Tuple.Tuple(red.sourceName, red),
    new Tuple.Tuple(black.sourceName, black),
    new Tuple.Tuple(destLeaf.sourceName, destLeaf),
    new Tuple.Tuple(destNode.sourceName, destNode),
  ]),
};
const destVar = name => new IR.NativeVar(name, destTreeType);
const destCtor = (name, fields) => new IR.NativeConstructor(name, fields);
const pvar = name => new Pattern.PatternVariable(name);
const pctor = (name, fields) => new Pattern.PatternConstructor(name, fields);
const targetTree = destVar('t');
const leftLeft = pctor('Dest_Node', [
  pctor('Dest_Black', []),
  pctor('Dest_Node', [
    pctor('Dest_Red', []),
    pctor('Dest_Node', [pctor('Dest_Red', []), pvar('a'), pvar('x'), pvar('b')]),
    pvar('y'),
    pvar('c'),
  ]),
  pvar('z'),
  pvar('d'),
]);
const colorValue = name => new IR.NativeVar(name, colorType);
const rebuilt = destCtor('Dest_Node', [
  colorValue('Dest_Red'),
  destCtor('Dest_Node', [colorValue('Dest_Black'), destVar('a'), new IR.NativeVar('x', intType), destVar('b')]),
  new IR.NativeVar('y', intType),
  destCtor('Dest_Node', [colorValue('Dest_Black'), destVar('c'), new IR.NativeVar('z', intType), destVar('d')]),
]);
const destCase = { patterns: [leftLeft], bindings: Map.empty, body: rebuilt };
const planned = Destination.planCase(destLayout)(setFrom(Ord.ordString, ['t']))([targetTree])(destCase);
assert.equal(planned.constructor.name, 'Just', 'destination plan must be produced');
const classes = Specialized.constructorClasses(destLayout);
const renderedPlan = Specialized.renderExpr(classes)(planned.value0.expression);
assert.equal(renderedPlan.constructor.name, 'Just', 'destination plan must render');
const constructors = ['Dest_Red', 'Dest_Black', 'Dest_Leaf', 'Dest_Node']
  .map(name => Specialized.renderConstructorWithLayout(destLayout)({ name, sourceName: new CoreFn.Qualified(new Maybe.Just(destModule), CoreFn.Ident(name)), typeName: name.startsWith('Dest_Color') ? 'Dest_Color' : (name === 'Dest_Leaf' || name === 'Dest_Node' ? 'Dest_Tree' : 'Dest_Color'), fields: name === 'Dest_Node' ? [colorType, destTreeType, intType, destTreeType] : [] }));
const destinationFsx = `module DestinationProbe

${Specialized.renderTypeDeclarations(destLayout)}
${Specialized.renderCopyFunctions(destLayout)}
${constructors.join('\n')}

let rec depth (t: Dest_Tree) : int =
    if isNull t then 0
    else
        let node = t :?> Dest_Node_Node
        1 + max (depth node.Field1) (depth node.Field3)

let balance (t: Dest_Tree) : Dest_Tree =
    let n0 = t :?> Dest_Node_Node
    let n1 = n0.Field1 :?> Dest_Node_Node
    let n2 = n1.Field1 :?> Dest_Node_Node
    let a = n2.Field1
    let x = n2.Field2
    let b = n2.Field3
    let y = n1.Field2
    let c = n1.Field3
    let z = n0.Field2
    let d = n0.Field3
    ${renderedPlan.value0}

let makeTree () =
    let leaf = Dest_Leaf
    let inner = Dest_Node Dest_Red leaf 2 leaf
    let left = Dest_Node Dest_Red inner 3 leaf
    Dest_Node Dest_Black left 4 leaf

let root = makeTree()
let rootNode = root :?> Dest_Node_Node
let originalLeft = rootNode.Field1
let result = balance root
let rotatedInPlace = obj.ReferenceEquals(result, originalLeft)
let resultDepth = depth result
let rootDepth = depth root
let original = makeTree()
let copy = sharpurs_copy_Dest_Tree original
let _ = balance copy
let copyIndependent = depth original = 3 && depth copy = 1
let alias = original
let aliasCopy = sharpurs_copy_Dest_Tree alias
let _ = balance aliasCopy
let aliasSafe = depth original = 3 && depth alias = 3
printfn "%b %d %d %b %b" rotatedInPlace resultDepth rootDepth copyIndependent aliasSafe
`;

const destinationDirectory = mkdtempSync(join(tmpdir(), 'sharpurs-native-destination-'));
try {
  const script = join(destinationDirectory, 'DestinationProbe.fs');
  writeFileSync(script, destinationFsx);
  const result = spawnSync(dotnet, ['fsi', '--nologo', '--optimize+', '--exec', script], {
    cwd: destinationDirectory,
    encoding: 'utf8',
  });
  assert.equal(result.status, 0, result.stderr || result.stdout);
  assert.match(result.stdout, /true 2 1 true true/, 'rotation, depth, deep copy and alias isolation must hold');
  console.log('native destination: rotation + alias checks passed');
} finally {
  rmSync(destinationDirectory, { recursive: true, force: true });
}
