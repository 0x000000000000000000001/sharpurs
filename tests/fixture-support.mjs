// Guard against a broken fixture silently turning a negative test green.
import assert from 'node:assert/strict';
import { test } from 'node:test';
import * as C from '../output/PureScript.Backend.Optimizer.CoreFn/index.js';
import * as S from '../output/PureScript.Backend.Optimizer.Syntax/index.js';
import { Just, Nothing } from '../output/Data.Maybe/index.js';
import { Tuple } from '../output/Data.Tuple/index.js';
import { clone, annotation, annotatedType, sourceBinding, sourceBindings, sourceLambdas,
  sourceApplication, sourceTypeApp, sourceVariable, optimizedBinding, optimizedTyped,
  optimizedLambda, dataDeclaration, firstNode, findNodes } from './support/ast.mjs';
import { fillSlots, fsharpFixture } from './support/fsharp.mjs';

const ann = () => ({ type: new Just(C.Int.value), meta: Nothing.value });
const variable = name => new C.ExprVar(ann(), new C.Qualified(new Just('Fixture'), name));
const application = (fn, arg) => new C.ExprApp(ann(), fn, arg);
const lambda = (name, body) => new C.ExprAbs(ann(), name, body);

test('copy preserves compiler constructors and isolates nested fields; renaming is suite-owned', () => {
  const original = { expr: lambda('x', variable('x')), types: [new C.ADT('Fixture.Tree', ['Fixture', 'Tree'], [])] };
  const copy = clone(original);
  assert.deepEqual(copy, original);
  assert.ok(copy.expr instanceof C.ExprAbs && copy.expr.value2 instanceof C.ExprVar);
  assert.ok(copy.types[0] instanceof C.ADT && annotation(copy.expr).type instanceof Just);
  copy.types[0].value1.push('Mutated');
  sourceVariable(copy.expr.value2).name = 'y';
  assert.equal(sourceVariable(original.expr.value2).name, 'x');
  assert.deepEqual(original.types[0].value1, ['Fixture', 'Tree']);
  const renamed = clone(original, node => typeof node === 'string' ? node.replaceAll('Fixture', 'Oracle') : node);
  assert.equal(renamed.types[0].value0, 'Oracle.Tree');
  assert.deepEqual(renamed.types[0].value1, ['Oracle', 'Tree']);
  assert.equal(sourceVariable(renamed.expr.value2).owner.value0, 'Oracle');
  assert.equal(original.types[0].value0, 'Fixture.Tree');
});

test('source/optimized binding views update the original tree and retain group ownership', () => {
  const b = new C.Binding(ann(), 'f', variable('x'));
  const rec = new C.Binding(ann(), 'g', variable('y'));
  const core = { decls: [new C.NonRec(b), new C.Rec([rec])] };
  assert.equal(sourceBindings(core).length, 2);
  const source = sourceBinding(core, 'g');
  assert.strictEqual(source.group, core.decls[1]);
  source.expression = variable('changed');
  assert.strictEqual(rec.value2, source.expression);
  source.name = 'renamed';
  assert.strictEqual(sourceBinding(core, 'renamed').node, rec);
  const typed = new S.Typed(C.Int.value, new S.Lit(new C.LitInt(1)));
  const pair = new Tuple('f', typed);
  const backend = { bindings: [{ bindings: [pair] }] };
  const optimized = optimizedBinding(backend, 'f');
  optimizedTyped(optimized.expression).type = C.Boolean.value;
  assert.strictEqual(typed.value0, C.Boolean.value);
  optimized.expression = optimizedTyped(optimized.expression).expression;
  assert.ok(pair.value1 instanceof S.Lit);
});

test('missing, duplicate and wrong-shaped targets fail before a negative assertion can run', () => {
  const b = new C.Binding(ann(), 'f', variable('x'));
  const selectors = [
    [() => sourceBinding({ decls: [new C.NonRec(b)] }, 'absent'), /source binding absent.*found 0/],
    [() => sourceBinding({ decls: [new C.NonRec(b), new C.NonRec(clone(b))] }, 'f'), /found 2/],
    [() => sourceBindings({ decls: [{ value0: b }] }), /expected NonRec or Rec/],
    [() => optimizedBinding({ bindings: [] }, 'absent'), /optimized binding absent.*found 0/],
    [() => optimizedBinding({ bindings: [{ bindings: [new Tuple('f', 1), new Tuple('f', 2)] }] }, 'f'), /found 2/],
    [() => optimizedTyped(variable('f')), /optimized Typed: expected Typed/],
    [() => dataDeclaration({ dataDecls: [] }, 'Tree'), /data declaration Tree.*found 0/],
  ];
  for (const [select, diagnostic] of selectors) {
    let reachedNegativeAssertion = false;
    assert.throws(() => { select(); reachedNegativeAssertion = true; }, diagnostic);
    assert.equal(reachedNegativeAssertion, false);
  }
});

test('application views preserve argument order, live edges and explicit TypeApp boundaries', () => {
  const head = new C.ExprTypeApp(ann(), variable('f'), C.Int.value);
  const left = variable('left'), right = variable('right');
  const inner = application(head, left), outer = application(inner, right);
  const call = sourceApplication(outer, 2);
  assert.deepEqual(call.args, [left, right]);
  assert.strictEqual(call.head, head);
  call.head = sourceTypeApp(head).expression;
  assert.strictEqual(inner.value1, head.value1);
  const replacement = variable('replacement');
  call.nodes[1].argument = replacement;
  assert.strictEqual(outer.value2, replacement);
  sourceTypeApp(head).argument = C.Number.value;
  assert.strictEqual(head.value2, C.Number.value);
  const interleaved = application(new C.ExprTypeApp(ann(), inner, C.Int.value), right);
  assert.throws(() => sourceApplication(interleaved, 2), /expected 2 runtime arguments before a boundary/);
  assert.ok(sourceApplication(interleaved, 1).head instanceof C.ExprTypeApp);
  assert.throws(() => sourceApplication(variable('f'), 1), /expected an application/);
  assert.throws(() => sourceTypeApp(variable('f')), /expected ExprTypeApp/);
  assert.throws(() => sourceVariable(head), /expected ExprVar/);
});

test('lambda views check exact source arity and optimized parameter shape', () => {
  const body = variable('body'), inner = lambda('second', body), outer = lambda('first', inner);
  const fn = sourceLambdas(outer, 2, 'fixture f');
  assert.strictEqual(fn.body, body);
  fn.lambdas[0].parameter = 'renamed';
  fn.lambdas[0].body = fn.lambdas[1].body;
  assert.equal(outer.value1, 'renamed');
  assert.strictEqual(outer.value2, body);
  assert.throws(() => sourceLambdas(outer, 2, 'fixture f'), /fixture f: expected 2 consecutive lambdas/);
  const parameter = new Tuple(new Just('x'), 7);
  const optimized = new S.Abs([parameter], new S.Local(Nothing.value, 7));
  optimizedLambda(optimized).parameters[0].level = 9;
  assert.equal(parameter.value1, 9);
  assert.throws(() => optimizedLambda(new S.Abs([], optimized)), /expected parameters/);
  assert.throws(() => optimizedLambda(new S.Abs([{}], optimized)), /expected Tuple/);
});

test('annotation selectors reject absent type evidence and unrelated runtime values', () => {
  const expr = variable('f');
  annotation(expr).type = new Just(new C.ADT('Fixture.Tree', ['Fixture', 'Tree'], []));
  annotatedType(expr, C.ADT).value1 = ['Wrong', 'Path'];
  assert.deepEqual(expr.value0.type.value0.value1, ['Wrong', 'Path']);
  assert.throws(() => annotatedType(expr, C.Func), /expected Func/);
  annotation(expr).type = Nothing.value;
  assert.throws(() => annotatedType(expr, C.ADT), /expected Just/);
  assert.throws(() => annotation({ value0: ann() }), /expected binding or expression/);
  assert.throws(() => annotation(new C.ExprVar({}, expr.value1)), /expected Maybe type/);
});

test('first-node search has explicit preorder and fails for a lost predicate target', () => {
  const first = new S.Lit(new C.LitInt(1)), second = new S.Lit(new C.LitInt(2));
  const tree = { children: [first, { child: second }] };
  assert.strictEqual(firstNode(tree, S.Lit, 'fixture literals'), first);
  assert.deepEqual(findNodes(tree, node => node instanceof S.Lit), [first, second]);
  assert.throws(() => firstNode(tree, S.Abs, 'lost lambda'), /lost lambda: no Abs target found/);
  assert.throws(() => firstNode(tree, S.Lit, 'lost literal', () => false), /lost literal: no Lit target found/);
});

test('F# insertion slots reject missing, duplicated and stale data without interpreting replacement text', async () => {
  assert.equal(fillSlots('before\n{{CASES}}\nafter', { CASES: '$& {{LITERAL}}' }), 'before\n$& {{LITERAL}}\nafter');
  assert.throws(() => fillSlots('{{CASES}}', {}), /missing slot CASES/);
  assert.throws(() => fillSlots('{{CASES}}{{CASES}}', { CASES: 'x' }), /repeated slot CASES/);
  assert.throws(() => fillSlots('static', { CASES: 'x' }), /unused slot CASES/);
  assert.throws(() => fillSlots('{{CASES}}', { CASES: 1 }), /must be text/);
  assert.match(await fsharpFixture('int-arithmetic/Runtime.fs', { CASES: '    (1, 2, 3, -1)' }), /let cases = \[\n    \(1, 2, 3, -1\)\n\]/);
});
