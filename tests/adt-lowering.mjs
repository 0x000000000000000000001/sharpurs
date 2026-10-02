// Checked body lowering, with an optional self-contained historical compiler.
// Every scenario is rebuilt with each compiler's own PureScript ADT constructors.
import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { mkdir, mkdtemp, readFile, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";
import * as current from "./support/adt-lowering-api.mjs";
import { copyFixtures, runFsharp } from "./support/fixtures.mjs";

const backend = resolve(dirname(fileURLToPath(import.meta.url)), "..");
const artifacts = process.env.ADT_LOWERING_ARTIFACTS && resolve(process.env.ADT_LOWERING_ARTIFACTS);

function scenarios(api) {
  const { C, S, Maybe: { Just, Nothing }, Map, Tuple: { Tuple } } = api;
  const int = C.Int.value, bool = C.Boolean.value;
  const tree = new C.ADT("Lowering.Tree", ["Lowering", "Tree"], []);
  const qualified = name => new C.Qualified(new Just("Lowering"), name);
  const layoutResult = api.Layout.fromModule({ name: "Lowering", dataDecls: [
    { name: "Tree", vars: [], constructors: [
      { name: "Empty", fields: [] }, { name: "Node", fields: [tree, int, tree] },
    ] },
  ] });
  assert.ok(layoutResult instanceof Just, "test layout passes production admission");
  const layout = layoutResult.value0;
  const i = value => new S.Lit(new C.LitInt(value));
  const b = value => new S.Lit(new C.LitBoolean(value));
  const local = level => new S.Local(Nothing.value, level);
  const typed = (type, body) => new S.Typed(type, body);
  const call = (name, ...args) => new S.App(new S.Var(qualified(name)), args);
  const binary = (op, left, right) => new S.PrimOp(new S.Op2(op, left, right));
  const tag = (name, value) => new S.PrimOp(new S.Op1(new S.OpIsTag(qualified(name)), value));
  const ctor = (name, fields) => new S.CtorSaturated(qualified(name), 0, "Tree", name,
    fields.map((value, index) => new Tuple(`value${index}`, value)));
  const field = (value, index, label = `value${index}`) =>
    new S.Accessor(value, new S.GetCtorField(qualified("Node"), 0, "Tree", "Node", label, index));
  const add = new S.OpIntNum(S.OpAdd.value);
  const parameterTypes = [tree, tree, int, bool];
  const signature = (args, result, nativeName) => ({ args, result, nativeName, publicName: nativeName });
  let globals = Map.empty;
  for (const [name, args, result, nativeName = name] of [
    ["constant", [], int], ["helper", [int], int], ["mark", [int], int],
    ["explode", [int], int], ["explodeBool", [bool], bool], ["combine", [int, int], int],
    ["subject", parameterTypes, int, "selfWorker"],
    ["Node", [tree, int, tree], tree, "Lowering_Node_adt_native"],
  ]) globals = Map.insert(C.ordQualified(C.ordIdent))(qualified(name))(signature(args, result, nativeName))(globals);
  const context = { layout, globals, locals: Map.empty, guardedCalls: true, self: Nothing.value };
  const cases = [];
  function record(name, result, body, { admitted = true, levels = [0, 1, 2, 3], ctx = context } = {}) {
    cases.push({ name, admitted, context: ctx, target: qualified("subject"),
      signature: signature(parameterTypes, result, "selfWorker"),
      expression: typed(new C.Func(parameterTypes, result), new S.Abs(
        levels.map(level => new Tuple(Nothing.value, level)), typed(result, body))) });
  }
  const reject = (name, result, body) => record(name, result, body, { admitted: false });
  record("literalMin", int, i(-2147483648));
  record("literalMax", int, i(2147483647));
  record("literalTrue", bool, b(true));
  record("literalFalse", bool, b(false));
  record("localInt", int, local(2));
  record("localTree", tree, local(0));
  record("globalValue", int, new S.Var(qualified("constant")));
  const operations = [
    ["and", S.OpBooleanAnd.value, bool, bool], ["or", S.OpBooleanOr.value, bool, bool],
    ["add", add, int, int], ["subtract", new S.OpIntNum(S.OpSubtract.value), int, int],
    ...[["equal", S.OpEq], ["notEqual", S.OpNotEq], ["greater", S.OpGt],
      ["greaterOrEqual", S.OpGte], ["less", S.OpLt], ["lessOrEqual", S.OpLte]]
      .map(([name, op]) => [name, new S.OpIntOrd(op.value), int, bool]),
  ];
  for (const [name, operation, operand, result] of operations) {
    const left = operand === int ? local(2) : local(3);
    const right = operand === int ? i(42) : tag("Empty", local(0));
    const wrongOperand = operand === int ? bool : int;
    record(`op_${name}`, result, binary(operation, left, right));
    reject(`${name}_result_type`, result === int ? bool : int, binary(operation, left, right));
    reject(`${name}_left_type`, result, binary(operation, typed(wrongOperand, left), right));
    reject(`${name}_right_type`, result, binary(operation, left, typed(wrongOperand, right)));
  }
  record("guardedCall", int, call("helper", local(2)));
  record("wholeNativeCall", int, call("helper", local(2)), { ctx: { ...context, guardedCalls: false } });
  record("selfCall", int, call("subject", local(0), local(1), local(2), local(3)),
    { ctx: { ...context, self: new Just(qualified("subject")) } });
  record("constructorCall", tree, call("Node", local(0), local(2), local(1)));
  record("saturatedConstructor", tree, ctor("Node", [local(0), local(2), local(1)]));
  record("emptyConstructor", tree, ctor("Empty", []));
  record("leftField", tree, field(local(0), 0));
  record("intField", int, field(local(0), 1));
  record("rightField", tree, field(local(0), 2));
  record("wrongTagField", int, field(ctor("Empty", []), 1));
  record("emptyTag", bool, tag("Empty", local(0)));
  record("nodeTag", bool, tag("Node", local(0)));
  record("sharedLet", int, new S.Let(Nothing.value, 4, call("mark", local(2)), binary(add, local(4), local(4))));
  record("treeLet", tree, new S.Let(Nothing.value, 4, local(0), local(4)));
  record("orderedCall", int, call("combine", call("mark", i(1)), call("mark", i(2))));
  record("orderedBranches", int, new S.Branch([
    new S.Pair(binary(new S.OpIntOrd(S.OpEq.value), call("mark", i(1)), i(0)), call("mark", i(11))),
    new S.Pair(binary(new S.OpIntOrd(S.OpEq.value), call("mark", i(2)), i(2)), call("mark", i(22))),
  ], call("mark", i(33))));
  record("branchFallback", int, new S.Branch([new S.Pair(b(false), new S.Fail("unreached"))], i(42)));
  record("shortAnd", bool, binary(S.OpBooleanAnd.value, local(3), call("explodeBool", b(true))));
  record("shortOr", bool, binary(S.OpBooleanOr.value, local(3), call("explodeBool", b(false))));
  record("bodyFailure", int, call("explode", i(1)));
  record("unguardedFailure", int, call("explode", i(1)), { ctx: { ...context, guardedCalls: false } });
  record("argumentFailure", int, call("combine", call("explode", i(1)), call("mark", i(2))));
  record("escapedFailure", int, new S.Fail('quoted "line"\n\\🙂'));

  reject("unknown_local", int, local(99));
  reject("contradictory_nested_type", int, typed(bool, i(42)));
  reject("wrong_literal_type", int, b(true));
  reject("unsupported_literal", int, new S.Lit(new C.LitString("42")));
  reject("unsupported_result", C.String.value, new S.Fail("unsupported signature"));
  reject("unknown_global", int, new S.Var(qualified("missing")));
  reject("function_as_value", int, new S.Var(qualified("helper")));
  reject("partial_call", int, call("combine", i(1)));
  reject("over_call", int, call("helper", i(1), i(2)));
  reject("wrong_call_result", bool, call("helper", i(1)));
  reject("wrong_call_argument", int, call("helper", b(true)));
  reject("local_callee", int, new S.App(local(2), [i(1)]));
  reject("wrong_callee_annotation", int, new S.App(typed(new C.Func([bool], int), new S.Var(qualified("helper"))), [i(1)]));
  reject("unresolved_type_app", int, new S.TypeApp(i(1), int));
  reject("unsupported_primitive", int, binary(new S.OpIntNum(S.OpMultiply.value), i(2), i(3)));
  reject("missing_constructor", tree, ctor("Missing", []));
  reject("partial_constructor", tree, ctor("Node", [local(0), i(1)]));
  reject("wrong_constructor_field_type", tree, ctor("Node", [local(0), b(true), local(1)]));
  reject("wrong_constructor_field_label", tree, new S.CtorSaturated(qualified("Node"), 0, "Tree", "Node",
    [new Tuple("value1", local(0)), new Tuple("value0", i(1)), new Tuple("value2", local(1))]));
  const nodeFields = [new Tuple("value0", local(0)), new Tuple("value1", i(1)), new Tuple("value2", local(1))];
  reject("wrong_constructor_type_identity", tree, new S.CtorSaturated(qualified("Node"), 0, "Other", "Node", nodeFields));
  reject("wrong_constructor_name_identity", tree, new S.CtorSaturated(qualified("Node"), 0, "Tree", "Empty", nodeFields));
  reject("negative_field_index", int, field(local(0), -1));
  reject("large_field_index", int, field(local(0), 3));
  reject("wrong_field_label", int, field(local(0), 1, "value0"));
  reject("wrong_field_type", bool, field(local(0), 1));
  reject("wrong_projection_value", int, field(i(1), 1));
  reject("wrong_tag_result", int, tag("Empty", local(0)));
  reject("wrong_tag_value", bool, tag("Empty", i(1)));
  for (const level of [-1, 2]) reject(`invalid_let_level_${level}`, int, new S.Let(Nothing.value, level, i(1), i(2)));
  reject("let_rhs_type_read_is_not_validation", int, new S.Let(Nothing.value, 4, typed(int, b(true)), local(4)));
  reject("let_scope_does_not_leak", int, binary(add, new S.Let(Nothing.value, 4, i(1), local(4)), local(4)));
  reject("wrong_branch_condition", int, new S.Branch([new S.Pair(i(1), i(2))], i(3)));
  reject("wrong_branch_body", int, new S.Branch([new S.Pair(b(true), b(false))], i(3)));
  reject("wrong_branch_fallback", int, new S.Branch([new S.Pair(b(true), i(2))], b(false)));
  record("negative_parameter", int, i(1), { admitted: false, levels: [-1, 1, 2, 3] });
  record("duplicate_parameter", int, i(1), { admitted: false, levels: [0, 1, 2, 2] });
  record("backwards_unbound_let_level", int, new S.Let(Nothing.value, 5, i(1), i(2)),
    { admitted: false, levels: [0, 2, 4, 6] });
  assert.equal(new Set(cases.map(item => item.name)).size, cases.length, "scenario names are unique");
  return { layout, cases };
}

function lower(api) {
  const { layout, cases } = scenarios(api);
  const nativeType = type => {
    const native = api.Layout.nativeType(layout)(type);
    assert.ok(native instanceof api.Maybe.Just);
    return native.value0;
  };
  const results = [], declarations = [];
  for (const item of cases) {
    const result = api.Lower.binding(item.context)(item.target)(item.signature)(item.expression);
    const admitted = result instanceof api.Maybe.Just;
    assert.equal(admitted, item.admitted, item.name);
    const definition = admitted ? {
      parameters: result.value0.parameters.map(p => ({ level: p.level, type: nativeType(p.type) })),
      body: result.value0.body,
    } : null;
    results.push({ name: item.name, admitted, definition });
    if (definition) declarations.push(`let ${item.name} ${definition.parameters.map(p => `(sharpurs_adt_local_${p.level}: ${p.type})`).join(" ")} : ${nativeType(item.signature.result)} = ${definition.body}`);
  }
  return { results, source: `module AdtLoweringGenerated\nopen AdtLoweringSupport\n\n${declarations.join("\n\n")}\n` };
}

const actual = lower(current);
let differential = 0;
if (process.env.ADT_LOWERING_ORACLE) {
  const old = await import(pathToFileURL(resolve(process.env.ADT_LOWERING_ORACLE)));
  const expected = lower(old);
  assert.deepEqual(actual.results, expected.results, "historical admissions, rejections, parameters and complete bodies agree");
  assert.equal(actual.source, expected.source, "historical generated definitions are byte-identical");
  differential = actual.results.length;
}
const directory = await mkdtemp(join(tmpdir(), "sharpurs-adt-lowering-"));
try {
  await copyFixtures(join(backend, "tests/fixtures/adt-lowering"), directory, ["Support.fs", "Assertions.fsx"]);
  await writeFile(join(directory, "Generated.fs"), actual.source);
  if (artifacts) {
    await mkdir(artifacts, { recursive: true });
    await copyFixtures(directory, artifacts, ["Support.fs", "Assertions.fsx", "Generated.fs"]);
    await writeFile(join(artifacts, "results.json"), JSON.stringify({ differential, cases: actual.results }, null, 2) + "\n");
  }
  const transcript = [];
  const executed = runFsharp(directory, "Assertions.fsx", { optimize: true, transcript });
  // The deliberate wrong-tag projection must fail at runtime, with valid F#
  // types and an exhaustive match over the fixture's two-constructor union.
  assert.doesNotMatch(executed.stderr, /warning FS\d+/, "body fixtures compile without warnings");
  console.log(executed.stdout.trim());
  console.log(`adt-lowering: ${actual.results.length} admission checks; ${differential} historical comparisons`);
  if (artifacts) {
    await writeFile(join(artifacts, "runtime.log"), transcript.join("\n") + "\n");
    const hashes = {};
    for (const file of ["tests/adt-lowering.mjs", "tests/support/adt-lowering-api.mjs", "tests/fixtures/adt-lowering/Support.fs", "tests/fixtures/adt-lowering/Assertions.fsx"])
      hashes[file] = createHash("sha256").update(await readFile(join(backend, file))).digest("hex");
    await writeFile(join(artifacts, "sources.json"), JSON.stringify(hashes, null, 2) + "\n");
  }
} finally {
  await rm(directory, { recursive: true, force: true });
}
