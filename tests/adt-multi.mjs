// Run after npm run build with the typed compiler fork selected by PURS.
import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { mkdtemp, mkdir, readFile, readdir, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { spawnSync } from "node:child_process";
import * as C from "../output/PureScript.Backend.Optimizer.CoreFn/index.js";
import * as S from "../output/PureScript.Backend.Optimizer.Syntax/index.js";
import * as Aff from "../output/Effect.Aff/index.js";
import * as Applicative from "../output/Control.Applicative/index.js";
import { Left } from "../output/Data.Either/index.js";
import { Just, Nothing } from "../output/Data.Maybe/index.js";
import { Tuple } from "../output/Data.Tuple/index.js";
import * as Map from "../output/Data.Map.Internal/index.js";
import * as Set from "../output/Data.Set/index.js";
import * as Ord from "../output/Data.Ord/index.js";
import * as App from "../output/PureScript.Backend.Optimizer.App/index.js";
import * as Builder from "../output/PureScript.Backend.Optimizer.Builder/index.js";
import * as Foreign from "../output/PureScript.Backend.Optimizer.Semantics.Foreign/index.js";
import { prepareUnary } from "../output/Sharpurs.AdtKernel/index.js";
import { translateModule, translateModuleWithConstructorWrappers, translateOptimizedModuleWithAdts } from "../output/Sharpurs.CodeGen/index.js";
import { printModule } from "../output/Sharpurs.Printer/index.js";

const backend = resolve(dirname(fileURLToPath(import.meta.url)), "..");
const artifacts = process.env.ADT_MULTI_ARTIFACTS && resolve(process.env.ADT_MULTI_ARTIFACTS);
const transcript = [];
const pure = Applicative.pure(Aff.applicativeAff);
const runAff = action => new Promise((resolve, reject) => {
  Aff.runAff(result => () => result instanceof Left ? reject(result.value0) : resolve(result.value0))(action)();
});
function command(program, args, cwd) {
  const result = spawnSync(program, args, { cwd, encoding: "utf8", timeout: 60_000 });
  transcript.push(`$ ${program} ${args.join(" ")}\n${result.stdout || ""}${result.stderr || ""}`);
  if (result.error) throw result.error;
  assert.equal(result.status, 0, `${program}: ${result.stdout}\n${result.stderr}`);
  return result;
}
function clone(value) {
  if (Array.isArray(value)) return value.map(clone);
  if (!value || typeof value !== "object") return value;
  return Object.assign(Object.create(Object.getPrototypeOf(value)), Object.fromEntries(
    Object.entries(value).map(([key, child]) => [key, clone(child)])));
}
function first(value, predicate, edit) {
  if (!value || typeof value !== "object") return false;
  if (predicate(value)) { edit(value); return true; }
  return Object.values(value).some(child => first(child, predicate, edit));
}
function expression(state, name) {
  return state.backend.bindings.flatMap(group => group.bindings).find(binding => binding.value0 === name).value1;
}
function sourceBinding(state, name) {
  return state.core.decls.flatMap(group => group instanceof C.NonRec ? [group.value0] : group.value0)
    .find(binding => binding.value1 === name);
}

const directory = await mkdtemp(join(tmpdir(), "sharpurs-adt-multi-"));
const previousCwd = process.cwd();
try {
  const packages = join(backend, ".spago/p");
  const names = await readdir(packages);
  const prelude = process.env.PRELUDE_SRC || join(packages, names.find(name => /^prelude-/.test(name)), "src");
  const partial = join(packages, names.find(name => /^partial-/.test(name)), "src");
  const fixtureFiles = ["AdtMulti.purs", "AdtMultiConsumer.purs", "AdtMultiConsumer.js", "AdtMultiExternal.purs"];
  for (const file of fixtureFiles) {
    await writeFile(join(directory, file), await readFile(join(backend, "tests/fixtures/adt-multi", file)));
  }
  const compiled = command(process.env.PURS || "purs", ["compile", join(directory, "*.purs"),
    join(prelude, "**/*.purs"), join(partial, "**/*.purs"), "--output", join(directory, "output"),
    "--codegen", "corefn,js"], directory);
  assert.doesNotMatch(compiled.stdout + compiled.stderr, /Warning \d+ of/, "fixture has no PureScript warnings");
  process.chdir(directory);
  const captured = new globalThis.Map();
  await runAff(Builder.buildModules(Aff.monadAff)({
    directives: await runAff(App.loadDirectives), rewriteLimit: 10000,
    analyzeCustom: _ => _ => Nothing.value,
    foreignSemantics: Map.filterKeys(C.ordQualified(C.ordIdent))(qualified => {
      const name = qualified.value0 instanceof Just ? qualified.value0.value0 : "";
      return !name.includes("Effect") && !name.includes("Control.Monad.ST");
    })(Foreign.coreForeignSemantics),
    traceIdents: Set.empty,
    onPrepareModule: _ => module => pure(module),
    onSkipModule: _ => _ => pure(Nothing.value),
    onCodegenModule: _ => core => module => _ => {
      if (["AdtMulti", "AdtMultiConsumer", "AdtMultiExternal"].includes(module.name)) captured.set(module.name, { core, backend: module });
      return pure(undefined);
    },
  })(await runAff(App.coreFnModulesFromOutput(join(directory, "output")))));
  assert.equal(captured.size, 3, "real fork compiler and PBO prepared producer, consumer and external dependency");
  const producer = captured.get("AdtMulti");
  const selected = prepareUnary(producer.core)(producer.backend);
  assert.ok(selected instanceof Just, "closed ADT module has native candidates");
  let checks = 0;
  const yes = (condition, label) => { assert.ok(condition, label); checks++; };
  const chosen = new globalThis.Set(selected.value0.nativeNames);
  for (const name of ["assemble", "insert", "choose", "shift", "depth", "unused", "nested", "score", "shortAndSafe", "shortOrSafe", "argumentNative"]) {
    yes(chosen.has(name), `${name}: typed closed native body selected`);
  }
  for (const name of ["returned", "partial", "generic", "booleanOnly", "readNonEmpty", "bodyFailure", "argumentFailure", "combine", "argumentOrder",
    "shortAnd", "shortOr", "readNonEmptyInline", "bodyFailureInline", "importedFailure", "importedTotal", "throughImported"]) {
    yes(!chosen.has(name), `${name}: source application boundary or unsupported signature retained`);
  }
  yes(first(expression(producer, "importedFailure"), node => node instanceof S.Fail, () => {}),
    "real optimizer inlined imported partial helper into a failure node");
  yes(!first(expression(producer, "importedFailure"), node => node instanceof S.Var && node.value0.value0 instanceof Just
    && node.value0.value0.value0 === "AdtMultiExternal", () => {}), "imported failure proof survives disappearance of its optimized reference");
  let constructors = Map.empty;
  let wrappers = Set.empty;
  const ctorNames = producer.core.dataDecls.flatMap(decl => decl.constructors.map(ctor => ctor.name));
  for (const { core } of captured.values()) for (const decl of core.dataDecls) for (const ctor of decl.constructors) {
    const name = `${core.name.replaceAll(".", "_")}_${ctor.name}`;
    constructors = Map.insert(Ord.ordString)(name)(ctor.fields.length)(constructors);
    if (core.name === "AdtMulti") wrappers = Set.insert(Ord.ordString)(name)(wrappers);
  }
  const generate = candidate => printModule(translateOptimizedModuleWithAdts(wrappers)(candidate)
    (constructors)(producer.backend)(producer.core));
  const generated = generate(selected);
  // Keep exactly the same closed layout and constructor bridges. Only native
  // function replacement is disabled, so the oracle uses real generic bodies.
  const oracleSelection = new Just({ ...selected.value0,
    nativeNames: [], bindings: Map.filterKeys(Ord.ordString)(name => ctorNames.includes(name))(selected.value0.bindings) });
  const oracle = generate(oracleSelection);
  // Fault injection stays in isolated copies of generated F#, after all real
  // source-oracle generation. Both raw implementations throw the same sentinel;
  // every caller, argument expression and invocation guard is left unchanged.
  const inject = (source, entry) => {
    let replacements = 0;
    const result = source.replace(new RegExp(`^(let ${entry} .+? = ).+$`, "m"), (_, header) => {
      replacements++; return `${header}events.Add(77); raise injectedFailure`;
    });
    yes(replacements === 1, `${entry}: exactly one raw body fault injection`);
    return result;
  };
  const injectedNative = inject(generated, "AdtMulti_score_adt_native");
  const injectedOracle = inject(oracle, "AdtMulti_score_direct");
  const consumer = printModule(translateModuleWithConstructorWrappers(wrappers)(constructors)(captured.get("AdtMultiConsumer").core));
  const external = printModule(translateModule(constructors)(captured.get("AdtMultiExternal").core));
  yes(/let AdtMulti_assemble_adt_native .*: AdtMulti_Tree/.test(generated), "four-argument native ADT result");
  yes(/let rec AdtMulti_insert_adt_native .*: AdtMulti_Tree/.test(generated), "two-argument recursive native result");
  yes(/let AdtMulti_insert_tco .*: obj/.test(generated), "generic callers retain recursive object bridge");
  yes(/AdtMulti_assemble_adt_native_apply/.test(generated), "typed cross-call exception boundary present");
  yes(/let AdtMulti_shortAndSafe_adt_native .* && /.test(generated), "source conditional lowered to native short-circuit And");
  yes(/let AdtMulti_shortOrSafe_adt_native .* \|\| /.test(generated), `source conditional lowered to native short-circuit Or: ${generated.split("\n").find(line => line.startsWith("let AdtMulti_shortOrSafe_adt_native "))}`);
  yes(!/let (?:rec )?AdtMulti_(?:assemble|insert|choose|shift)_adt_native /.test(oracle),
    "oracle has no native function replacements");
  yes(/let rec AdtMulti_insert_tco/.test(oracle) && /sharpurs_apply/.test(oracle),
    "oracle retains generated generic recursion and dispatch");
  const rejectModule = (label, edit) => {
    const state = clone(producer); edit(state);
    yes(prepareUnary(state.core)(state.backend) instanceof Nothing, label);
  };
  const rejectFunction = (name, label, edit) => {
    const state = clone(producer); edit(state);
    const candidate = prepareUnary(state.core)(state.backend);
    yes(candidate instanceof Just && !candidate.value0.nativeNames.includes(name), label);
  };
  rejectModule("polymorphic layout rejected", state => {
    state.core.dataDecls.find(decl => decl.name === "Tree").vars = ["a"];
    state.backend.dataDecls = clone(state.core.dataDecls);
  });
  rejectModule("missing constructor wrapper rejected", state => {
    state.backend.bindings = state.backend.bindings.map(group => ({ ...group,
      bindings: group.bindings.filter(binding => binding.value0 !== "Branch") }));
  });
  rejectModule("native apply helper collision rejected", state => {
    const binding = clone(sourceBinding(state, "booleanOnly")); binding.value1 = "assemble_adt_native_apply";
    state.core.decls.push(new C.NonRec(binding));
  });
  rejectModule("foreign module retains generic code", state => { state.backend.foreign = Map.singleton("unknown")(new Just(C.Int.value)); });
  rejectFunction("assemble", "missing source type rejects function", state => { sourceBinding(state, "assemble").value2.value0.type = Nothing.value; });
  rejectFunction("assemble", "missing inner lambda type rejects function", state => { sourceBinding(state, "assemble").value2.value2.value0.type = Nothing.value; });
  rejectFunction("assemble", "contradictory inner lambda type rejects function", state => { sourceBinding(state, "assemble").value2.value2.value0.type = new Just(C.Int.value); });
  rejectFunction("assemble", "missing source body type rejects function", state => {
    let body = sourceBinding(state, "assemble").value2;
    while (body instanceof C.ExprAbs) body = body.value2;
    body.value0.type = Nothing.value;
  });
  rejectFunction("assemble", "ForAll signature retains generic function", state => {
    const signature = new C.ForAll(["a"], expression(state, "assemble").value0);
    expression(state, "assemble").value0 = signature;
    sourceBinding(state, "assemble").value2.value0.type = new Just(signature);
  });
  rejectFunction("assemble", "constrained signature retains generic dictionary ABI", state => {
    const signature = new C.ConstrainedType([new Tuple(["Data", "Eq", "Eq"], [C.Int.value])], expression(state, "assemble").value0);
    expression(state, "assemble").value0 = signature;
    sourceBinding(state, "assemble").value2.value0.type = new Just(signature);
  });
  rejectFunction("assemble", "contradictory optimized signature rejected", state => { expression(state, "assemble").value0 = C.Int.value; });
  rejectFunction("assemble", "unsupported TypeApp retains function", state => {
    expression(state, "assemble").value1 = new S.TypeApp(expression(state, "assemble").value1, C.Int.value);
  });
  rejectFunction("assemble", "missing optimized annotation rejected", state => {
    const binding = state.backend.bindings.flatMap(group => group.bindings).find(binding => binding.value0 === "assemble");
    binding.value1 = binding.value1.value1;
  });
  rejectFunction("assemble", "source lambda arity contradiction rejected", state => {
    sourceBinding(state, "assemble").value2.value2 = sourceBinding(state, "assemble").value2.value2.value2;
  });
  rejectFunction("insert", "mutual source recursion retained", state => {
    const group = state.core.decls.find(group => group instanceof C.Rec && group.value0.some(binding => binding.value1 === "insert"));
    const extra = clone(group.value0[0]); extra.value1 = "otherInsert"; group.value0.push(extra);
  });
  rejectFunction("assemble", "duplicate optimized parameter levels rejected", state => {
    let changed = false;
    first(expression(state, "assemble"), node => node instanceof S.Abs, node => {
      changed = first(node.value1, inner => inner instanceof S.Abs,
        inner => { inner.value0[0].value1 = node.value0[0].value1; });
    });
    yes(changed, "real optimized nested argument lambdas found");
  });
  rejectFunction("shortAndSafe", "Boolean primitive rejects contradictory operand annotation", state => {
    yes(first(expression(state, "shortAndSafe"), node => node instanceof S.Op2 && node.value0 instanceof S.OpBooleanAnd,
      node => { node.value1 = new S.Typed(C.Int.value, node.value1); }), "real optimized Boolean And found");
  });
  const failedDependency = clone(producer);
  sourceBinding(failedDependency, "assemble").value2.value0.type = Nothing.value;
  const reduced = prepareUnary(failedDependency.core)(failedDependency.backend);
  yes(reduced instanceof Just && !reduced.value0.nativeNames.includes("insert") && reduced.value0.nativeNames.includes("depth"),
    "unsupported native dependency keeps dependent function generic without dropping independent functions");

  const main = await readFile(join(backend, "src/Main.purs"), "utf8");
  const preludeFs = main.match(/^fsPrelude = """\r?\n([\s\S]*?)^"""/m)?.[1];
  assert.ok(preludeFs, "actual runtime prelude extracted");
  const support = `
let (|LitInt|_|) (expected: int) (value: obj) = if value :? int && unbox<int> value = expected then Some() else None
let (|LitBool|_|) (expected: bool) (value: obj) = if value :? bool && unbox<bool> value = expected then Some() else None
let events = ResizeArray<int>()
let injectedFailure = System.InvalidOperationException("adt-multi injected typed failure")
let track : obj = box (fun (label: obj) -> box (fun (value: obj) -> events.Add(unbox<int> label); value))
let AdtMultiConsumer_trackColor = track
let AdtMultiConsumer_trackTree = track
let AdtMultiConsumer_trackInt = track
let Partial_Unsafe_unsafePartial : obj = box (fun (value: obj) -> sharpurs_apply value (box ()))
let Partial_Unsafe__unsafePartial = Partial_Unsafe_unsafePartial
let Data_Boolean_otherwise = box true
`;
  const scoped = (name, body) => `module ${name} =\n${body.split("\n").map(line => `    ${line}`).join("\n")}\n`;
  const runtime = `
let mutable checks = 0
let check label condition = if not condition then failwith label else checks <- checks + 1
let apply = sharpurs_apply
let call2 fn a b = apply (apply fn a) b
let call4 fn a b c d = apply (apply (apply (apply fn a) b) c) d
let node n = call4 Native.AdtMulti_assemble Native.AdtMulti_Ruby Native.AdtMulti_Tip (box n) Native.AdtMulti_Tip
let reference n = call4 Oracle.AdtMulti_assemble Oracle.AdtMulti_Ruby Oracle.AdtMulti_Tip (box n) Oracle.AdtMulti_Tip
let rec nativeShape (tree: Native.AdtMulti_Tree) =
    match tree with
    | Native.AdtMulti_Tipusd_Ctor -> "."
    | Native.AdtMulti_Branchusd_Ctor(color, left, value, right) -> sprintf "(%A,%s,%d,%s)" color (nativeShape left) value (nativeShape right)
let rec oracleShape (tree: Oracle.AdtMulti_Tree) =
    match tree with
    | Oracle.AdtMulti_Tipusd_Ctor -> "."
    | Oracle.AdtMulti_Branchusd_Ctor(color, left, value, right) -> sprintf "(%A,%s,%d,%s)" color (oracleShape left) value (oracleShape right)
let sameTree actual expected = nativeShape (unbox actual) = oracleShape (unbox expected)
let build fn empty values = List.fold (fun tree value -> call2 fn (box value) tree) empty values
let samples = [[1..40]; [40..-1..1]; [7; 2; 11; 0; 4; 9; 13; 2; 7]; [System.Int32.MinValue; 0; System.Int32.MaxValue; -1; 1]]
for values in samples do
    let tree = build Native.AdtMulti_insert Native.AdtMulti_Tip values
    let old = build Oracle.AdtMulti_insert Oracle.AdtMulti_Tip values
    check "recursive insert entire structure matches oracle" (sameTree tree old)
    check "native depth agrees" (apply Native.AdtMulti_depth tree = apply Oracle.AdtMulti_depth old)
    for value in values do
        check "duplicate insertion preserves complete tree" (sameTree (call2 Native.AdtMulti_insert (box value) tree) old)
    check "duplicate root insertion preserves root identity" (System.Object.ReferenceEquals(call2 Native.AdtMulti_insert (box values.Head) tree, tree))
    for amount in [System.Int32.MinValue; -1; 0; 1; System.Int32.MaxValue] do
        check "recursive shift, Int wrap and dependency calls agree" (sameTree (call2 Native.AdtMulti_shift (box amount) tree) (call2 Oracle.AdtMulti_shift (box amount) old))
    check "input remains immutable after transformations" (sameTree tree old)
for value in [System.Int32.MinValue; -1; 0; 1; System.Int32.MaxValue] do
    let tree = node value
    let old = reference value
    for increment in [System.Int32.MinValue; -1; 0; 1; System.Int32.MaxValue] do
        for fn, baseline in [Native.AdtMulti_readNonEmpty, Oracle.AdtMulti_readNonEmpty; Native.AdtMulti_bodyFailure, Oracle.AdtMulti_bodyFailure;
                             Native.AdtMulti_importedTotal, Oracle.AdtMulti_importedTotal] do
            check "two argument native Int result and overflow agree" (call2 fn tree (box increment) = call2 baseline old (box increment))
    for flag in [false; true] do
        check "four argument Boolean branch agrees" (sameTree (call4 Native.AdtMulti_choose (box flag) tree (box value) tree) (call4 Oracle.AdtMulti_choose (box flag) old (box value) old))
    check "unused argument keeps original ADT identity" (System.Object.ReferenceEquals(call2 Native.AdtMulti_unused tree (box 99), tree))
let child = node 7
let original = reference 7
for fn, empty, leaf, encode in [Native.AdtMultiConsumer_ordered, Native.AdtMulti_Tip, child, (fun value -> nativeShape (unbox value));
                               Oracle.AdtMultiConsumer_ordered, Oracle.AdtMulti_Tip, original, (fun value -> oracleShape (unbox value))] do
    events.Clear()
    let value = apply fn leaf
    check "consumer arguments evaluated once in order" (List.ofSeq events = [1; 2; 3; 4])
    check "ordered construction actually creates node" (encode value <> encode empty)
for fn, leaf, empty in [Native.AdtMultiConsumer_orderedPartial, child, Native.AdtMulti_Tip;
                        Oracle.AdtMultiConsumer_orderedPartial, original, Oracle.AdtMulti_Tip] do
    events.Clear()
    let partial = apply fn leaf
    check "partial supplied arguments evaluate eagerly" (List.ofSeq events = [1; 2])
    let captured = apply partial (box 9)
    apply captured leaf |> ignore
    apply captured empty |> ignore
    check "reused partial captures arguments without reevaluation" (List.ofSeq events = [1; 2])
let first = apply Native.AdtMulti_assemble Native.AdtMulti_Onyx
let second = apply first child
let third = apply second (box 9)
for right in [child; Native.AdtMulti_Tip] do
    let result = apply third right |> unbox<Native.AdtMulti_Tree>
    match result with
    | Native.AdtMulti_Branchusd_Ctor(_, left, value, actualRight) ->
        check "partial wrapper captures native child identity" (System.Object.ReferenceEquals(left, child))
        check "partial wrapper captures Int" (value = 9)
        check "partial wrapper can be reused with new argument" (System.Object.ReferenceEquals(actualRight, right))
    | _ -> failwith "expected branch"
let capturedInsert = apply Native.AdtMultiConsumer_captured (box 13)
check "function value preserves first use" (sameTree (apply capturedInsert Native.AdtMulti_Tip) (call2 Oracle.AdtMulti_insert (box 13) Oracle.AdtMulti_Tip))
check "function value preserves reuse" (sameTree (apply capturedInsert child) (call2 Oracle.AdtMulti_insert (box 13) original))
let capture action = try action() |> ignore; None with ex -> Some ex
let rec signature (ex: System.Exception) =
    match ex with
    | :? System.Reflection.TargetInvocationException as wrapper -> let depth, cause = signature wrapper.InnerException in depth + 1, cause
    | cause -> 0, cause.GetType().FullName + ": " + cause.Message
let failure action = match capture action with Some ex -> signature ex | None -> failwith "expected fixture failure"
for fn, baseline, bypass, evaluate in [Native.AdtMulti_shortAnd, Oracle.AdtMulti_shortAnd, false, true;
                                      Native.AdtMulti_shortOr, Oracle.AdtMulti_shortOr, true, false] do
    check "short-circuit skips failing right hand side" (call2 fn (box bypass) Native.AdtMulti_Tip = call2 baseline (box bypass) Oracle.AdtMulti_Tip)
    let actual = failure (fun () -> call2 fn (box evaluate) Native.AdtMulti_Tip)
    let expected = failure (fun () -> call2 baseline (box evaluate) Oracle.AdtMulti_Tip)
    if actual <> expected then printfn "short-circuit exception mismatch: native=%A oracle=%A" actual expected
    check "short-circuit evaluates necessary right hand side with preserved envelope" (actual = expected)
    check "right hand side success" (call2 fn (box evaluate) child = call2 baseline (box evaluate) original)
for fn, baseline in [Native.AdtMulti_shortAndSafe, Oracle.AdtMulti_shortAndSafe; Native.AdtMulti_shortOrSafe, Oracle.AdtMulti_shortOrSafe] do
    for flag in [false; true] do
        for tree, old in [Native.AdtMulti_Tip, Oracle.AdtMulti_Tip; child, original] do
            check "native lazy Boolean primitive matches source branches" (call2 fn (box flag) tree = call2 baseline (box flag) old)
for tree, old in [Native.AdtMulti_Tip, Oracle.AdtMulti_Tip; child, original;
                  call4 Native.AdtMulti_assemble Native.AdtMulti_Onyx child (box 11) Native.AdtMulti_Tip,
                  call4 Oracle.AdtMulti_assemble Oracle.AdtMulti_Onyx original (box 11) Oracle.AdtMulti_Tip] do
    check "nested tags short-circuit constructor accessors" (call2 Native.AdtMulti_nested tree (box 3) = call2 Oracle.AdtMulti_nested old (box 3))
for native, generic, nativeArgs, genericArgs in [
    Native.AdtMulti_readNonEmpty, Oracle.AdtMulti_readNonEmpty, [Native.AdtMulti_Tip; box 0], [Oracle.AdtMulti_Tip; box 0]
    Native.AdtMulti_bodyFailure, Oracle.AdtMulti_bodyFailure, [Native.AdtMulti_Tip; box 0], [Oracle.AdtMulti_Tip; box 0]
    Native.AdtMulti_readNonEmptyInline, Oracle.AdtMulti_readNonEmptyInline, [Native.AdtMulti_Tip; box 0], [Oracle.AdtMulti_Tip; box 0]
    Native.AdtMulti_bodyFailureInline, Oracle.AdtMulti_bodyFailureInline, [Native.AdtMulti_Tip; box 0], [Oracle.AdtMulti_Tip; box 0]
    Native.AdtMulti_importedFailure, Oracle.AdtMulti_importedFailure, [Native.AdtMulti_Tip; box 1], [Oracle.AdtMulti_Tip; box 1]
    Native.AdtMulti_throughImported, Oracle.AdtMulti_throughImported, [Native.AdtMulti_Tip; box 1], [Oracle.AdtMulti_Tip; box 1]
    Native.AdtMulti_argumentFailure, Oracle.AdtMulti_argumentFailure, [child; Native.AdtMulti_Tip], [original; Oracle.AdtMulti_Tip]
    Native.AdtMulti_argumentFailure, Oracle.AdtMulti_argumentFailure, [Native.AdtMulti_Tip; child], [Oracle.AdtMulti_Tip; original]
    Native.AdtMulti_argumentOrder, Oracle.AdtMulti_argumentOrder, [Native.AdtMulti_Tip; child], [Oracle.AdtMulti_Tip; original]
    Native.AdtMulti_argumentOrder, Oracle.AdtMulti_argumentOrder, [child; Native.AdtMulti_Tip], [original; Oracle.AdtMulti_Tip]
] do
    let invoke fn args () = List.fold apply fn args
    let actual = failure (invoke native nativeArgs)
    let expected = failure (invoke generic genericArgs)
    check "typed body or argument failure keeps complete generic exception envelope" (actual = expected)
    check "exception test reached actual pattern match failure" (snd actual |> fun message -> message.Contains("MatchFailureException"))
check "external partial helper successful branch" (call2 Native.AdtMulti_importedFailure Native.AdtMulti_Tip (box 0) = call2 Oracle.AdtMulti_importedFailure Oracle.AdtMulti_Tip (box 0))
for fn, baseline, expectedEvents in [Native.AdtMultiConsumer_firstArgumentFailure, Oracle.AdtMultiConsumer_firstArgumentFailure, [];
                                      Native.AdtMultiConsumer_lastArgumentFailure, Oracle.AdtMultiConsumer_lastArgumentFailure, [1]] do
    let left, right, oldLeft, oldRight =
        if System.Object.ReferenceEquals(fn, Native.AdtMultiConsumer_firstArgumentFailure) then Native.AdtMulti_Tip, child, Oracle.AdtMulti_Tip, original
        else child, Native.AdtMulti_Tip, original, Oracle.AdtMulti_Tip
    events.Clear()
    let actual = failure (fun () -> call2 fn left right)
    check "argument failure stops later argument effects" (List.ofSeq events = expectedEvents)
    events.Clear()
    let expected = failure (fun () -> call2 baseline oldLeft oldRight)
    check "consumer exception chain matches generic oracle" (actual = expected)
    check "consumer oracle has same event order" (List.ofSeq events = expectedEvents)
for fn, leaf, empty in [Native.AdtMulti_returned, child, Native.AdtMulti_Tip; Oracle.AdtMulti_returned, original, Oracle.AdtMulti_Tip] do
    let returned = apply fn leaf
    for increment in [0; 1; 9] do
        check "returned closure retains captured value" (unbox<int> (apply returned (box increment)) = 7 + increment)
    check "returned closure preserves eager source let failure" (capture (fun () -> apply fn empty) |> Option.isSome)
for fn, baseline, bypass, evaluate in [InjectedNative.AdtMulti_shortAndSafe, InjectedOracle.AdtMulti_shortAndSafe, false, true;
                                      InjectedNative.AdtMulti_shortOrSafe, InjectedOracle.AdtMulti_shortOrSafe, true, false] do
    events.Clear()
    check "injected native lazy RHS bypass matches generic source" (call2 fn (box bypass) InjectedNative.AdtMulti_Tip = call2 baseline (box bypass) InjectedOracle.AdtMulti_Tip)
    check "bypassed RHS was not evaluated in either version" (events.Count = 0)
    let actual = failure (fun () -> call2 fn (box evaluate) InjectedNative.AdtMulti_Tip)
    check "native RHS evaluated exactly once" (List.ofSeq events = [77])
    events.Clear()
    let expected = failure (fun () -> call2 baseline (box evaluate) InjectedOracle.AdtMulti_Tip)
    check "generic RHS evaluated exactly once" (List.ofSeq events = [77])
    check "native cross-call wraps same sentinel exactly as generic call" (actual = expected && fst actual = 2)
events.Clear()
let argumentException = failure (fun () -> call2 InjectedNative.AdtMulti_argumentNative InjectedNative.AdtMulti_Tip InjectedNative.AdtMulti_Tip)
check "failing native argument prevents outer callee evaluation" (List.ofSeq events = [77])
events.Clear()
let oracleArgumentException = failure (fun () -> call2 InjectedOracle.AdtMulti_argumentNative InjectedOracle.AdtMulti_Tip InjectedOracle.AdtMulti_Tip)
check "generic failing argument prevents outer callee evaluation" (List.ofSeq events = [77])
check "argument evaluates before outer invocation guard" (argumentException = oracleArgumentException && fst argumentException = 2)
printfn "adt-multi runtime: %d checks passed" checks
`;
  const script = [preludeFs, support, external, scoped("Native", generated + "\n\n" + consumer),
    scoped("Oracle", oracle + "\n\n" + consumer), scoped("InjectedNative", injectedNative), scoped("InjectedOracle", injectedOracle), runtime].join("\n\n");
  await writeFile(join(directory, "adt-multi.fsx"), script);
  if (artifacts) {
    await mkdir(artifacts, { recursive: true });
    for (const [file, value] of [["AdtMulti.fs", generated], ["Oracle.fs", oracle], ["AdtMultiConsumer.fs", consumer],
      ["AdtMultiExternal.fs", external],
      ["InjectedNative.fs", injectedNative], ["InjectedOracle.fs", injectedOracle],
      ["adt-multi.fsx", script], ["selection.json", JSON.stringify([...chosen], null, 2)]]) await writeFile(join(artifacts, file), value);
    for (const file of fixtureFiles) await writeFile(join(artifacts, file), await readFile(join(directory, file)));
    await writeFile(join(artifacts, "corefn.json"), await readFile(join(directory, "output/AdtMulti/corefn.json")));
    await writeFile(join(artifacts, "AdtMultiExternal.corefn.json"), await readFile(join(directory, "output/AdtMultiExternal/corefn.json")));
  }
  const result = command(process.env.DOTNET || "dotnet", ["fsi", "--nologo", "--optimize+", "--exec", "adt-multi.fsx"], directory);
  // Existing generic object recursion and deliberately partial source matches
  // emit FS0040/FS0025. Native replacements must not add warnings.
  const scriptLines = script.split("\n");
  for (const warning of result.stderr.matchAll(/\((\d+),\d+\): warning (FS\d+):/g)) {
    yes(["FS0025", "FS0040"].includes(warning[2]), "only known generic fixture warning codes");
    yes(!/^\s*(?:let(?: rec)?|and) AdtMulti_\w+_adt_native(?:_apply)?\b/.test(scriptLines[Number(warning[1]) - 1]),
      "warning comes from retained generic body or wrapper");
  }
  assert.match(result.stdout, /adt-multi runtime: \d+ checks passed/, "runtime assertion suite completed");
  console.log(result.stdout.trim());
  const summary = `adt-multi converter: ${checks} checks passed`;
  transcript.push(summary);
  console.log(summary);
  if (artifacts) {
    const hashes = {};
    for (const file of ["src/Sharpurs/AdtKernel.purs", "src/Sharpurs/AdtLayout.purs", "src/Sharpurs/CodeGen.purs", "tests/adt-multi.mjs",
      ...fixtureFiles.map(file => `tests/fixtures/adt-multi/${file}`)]) hashes[file] = createHash("sha256").update(await readFile(join(backend, file))).digest("hex");
    await writeFile(join(artifacts, "metadata.json"), JSON.stringify({ sourceHashes: hashes, selected: [...chosen] }, null, 2) + "\n");
  }
} catch (error) {
  transcript.push(error.stack || String(error));
  throw error;
} finally {
  if (artifacts) {
    await mkdir(artifacts, { recursive: true });
    await writeFile(join(artifacts, "validation.log"), transcript.join("\n") + "\n");
  }
  process.chdir(previousCwd);
  await rm(directory, { recursive: true, force: true });
}
