// Structural fixture access only: no Sharpurs recognizer or eligibility rule.
// These selectors use the current output's constructors. Historical adapters
// must keep using their own runtime classes when reading/building old trees.
import assert from 'node:assert/strict';
import * as C from '../../output/PureScript.Backend.Optimizer.CoreFn/index.js';
import * as S from '../../output/PureScript.Backend.Optimizer.Syntax/index.js';
import { Just, Nothing } from '../../output/Data.Maybe/index.js';
import { Tuple } from '../../output/Data.Tuple/index.js';

// Copy enumerable AST fields, including arrays, without losing instanceof.
// The optional post-order transform belongs to the suite (e.g. oracle renaming).
// This is a tree copy, not a serializer for cycles, Maps or arbitrary JS objects.
export function clone(value, transform = value => value) {
  const copy = Array.isArray(value) ? value.map(child => clone(child, transform))
    : value && typeof value === 'object'
      ? Object.assign(Object.create(Object.getPrototypeOf(value)), Object.fromEntries(
        Object.entries(value).map(([key, child]) => [key, clone(child, transform)])))
      : value;
  return transform(copy);
}

export function expectNode(node, Type, label = Type.name) {
  assert.ok(node instanceof Type, `${label}: expected ${Type.name}, got ${node?.constructor?.name ?? String(node)}`);
  return node;
}

// Named live views: assignments update the selected node, never a detached copy.
function view(node, Type, fields, label) {
  expectNode(node, Type, label);
  const result = { node };
  for (const [name, field] of Object.entries(fields)) {
    assert.ok(Object.hasOwn(node, field), `${label ?? Type.name}: missing ${field} (${name})`);
    Object.defineProperty(result, name, {
      enumerable: true, get: () => node[field], set: value => { node[field] = value; },
    });
  }
  return result;
}

export function sourceBindings(core) {
  assert.ok(Array.isArray(core?.decls), 'source module: expected declaration groups');
  return core.decls.flatMap(group => {
    assert.ok(group instanceof C.NonRec || group instanceof C.Rec, 'source group: expected NonRec or Rec');
    const bindings = group instanceof C.NonRec ? [group.value0] : group.value0;
    assert.ok(Array.isArray(bindings), 'source recursive group: expected bindings');
    return bindings.map(node => Object.assign(view(node, C.Binding,
      { name: 'value1', expression: 'value2' }, 'source binding'), { group }));
  });
}

function unique(items, predicate, label) {
  const matches = items.filter(predicate);
  assert.equal(matches.length, 1, `${label}: expected exactly one target, found ${matches.length}`);
  return matches[0];
}

export function sourceBinding(core, name) {
  return unique(sourceBindings(core), binding => binding.name === name, `source binding ${name}`);
}

export function optimizedBinding(backend, name) {
  assert.ok(Array.isArray(backend?.bindings), 'optimized module: expected binding groups');
  const bindings = backend.bindings.flatMap(group => {
    assert.ok(Array.isArray(group.bindings), 'optimized group: expected bindings');
    return group.bindings.map(node => Object.assign(view(node, Tuple,
      { name: 'value0', expression: 'value1' }, 'optimized binding'), { group }));
  });
  return unique(bindings, binding => binding.name === name, `optimized binding ${name}`);
}

const sourceNodes = [C.Binding, C.ExprVar, C.ExprLit, C.ExprConstructor, C.ExprAccessor,
  C.ExprUpdate, C.ExprAbs, C.ExprApp, C.ExprCase, C.ExprLet, C.ExprTypeApp];
export function annotation(node) {
  assert.ok(sourceNodes.some(Type => node instanceof Type), 'source annotation: expected binding or expression');
  const ann = node.value0;
  assert.ok(ann && (ann.type instanceof Just || ann.type instanceof Nothing), 'source annotation: expected Maybe type');
  return ann;
}

export function annotatedType(node, Type) {
  const maybe = expectNode(annotation(node).type, Just, 'source type annotation');
  return expectNode(maybe.value0, Type, 'source type annotation');
}

export function sourceLambda(node, label = 'source lambda') {
  return view(node, C.ExprAbs, { parameter: 'value1', body: 'value2' }, label);
}

export function sourceLambdas(expression, count, label = 'source lambdas') {
  const lambdas = [];
  while (expression instanceof C.ExprAbs) {
    const lambda = sourceLambda(expression, label);
    lambdas.push(lambda);
    expression = lambda.body;
  }
  assert.equal(lambdas.length, count, `${label}: expected ${count} consecutive lambdas`);
  annotation(expression);
  return { lambdas, body: expression };
}

export function sourceTypeApp(node) {
  return view(node, C.ExprTypeApp, { expression: 'value1', argument: 'value2' }, 'source TypeApp');
}

export function sourceVariable(node) {
  expectNode(node, C.ExprVar, 'source variable');
  return view(node.value1, C.Qualified, { owner: 'value0', name: 'value1' }, 'source variable qualification');
}

// Stop at TypeApp/let/etc.; crossing a boundary must be explicit in the suite.
// args contains live node references; use nodes[i].argument to replace an edge.
export function sourceApplication(expression, arity, label = 'source application') {
  const nodes = [];
  while (expression instanceof C.ExprApp) {
    const app = view(expression, C.ExprApp, { fn: 'value1', argument: 'value2' }, label);
    nodes.unshift(app);
    expression = app.fn;
  }
  assert.ok(nodes.length > 0, `${label}: expected an application`);
  assert.equal(nodes.length, arity, `${label}: expected ${arity} runtime arguments before a boundary`);
  return {
    nodes,
    get head() { return nodes[0].fn; },
    set head(value) { nodes[0].fn = value; },
    get args() { return nodes.map(node => node.argument); },
  };
}

export function optimizedTyped(node) {
  return view(node, S.Typed, { type: 'value0', expression: 'value1' }, 'optimized Typed');
}

export function optimizedLambda(node) {
  const lambda = view(node, S.Abs, { body: 'value1' }, 'optimized lambda');
  assert.ok(Array.isArray(node.value0) && node.value0.length > 0, 'optimized lambda: expected parameters');
  lambda.parameters = node.value0.map(parameter => view(parameter, Tuple,
    { name: 'value0', level: 'value1' }, 'optimized lambda parameter'));
  return lambda;
}

export function dataDeclaration(core, name) {
  assert.ok(Array.isArray(core?.dataDecls), 'source module: expected data declarations');
  return unique(core.dataDecls, decl => decl.name === name, `data declaration ${name}`);
}

export function findNodes(value, predicate, found = []) {
  if (!value || typeof value !== 'object') return found;
  if (predicate(value)) found.push(value);
  for (const child of Object.values(value)) findNodes(child, predicate, found);
  return found;
}

// Intentional preorder choice, for suites that mutate the first occurrence.
// An absent target throws before the negative recognizer assertion can run.
export function firstNode(tree, Type, label, predicate = () => true) {
  function find(value) {
    if (!value || typeof value !== 'object') return undefined;
    if (value instanceof Type && predicate(value)) return value;
    for (const child of Object.values(value)) {
      const found = find(child);
      if (found !== undefined) return found;
    }
  }
  const found = find(tree);
  assert.ok(found, `${label}: no ${Type.name} target found`);
  return found;
}
