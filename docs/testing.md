# Replaying compiler checks

Run these commands from the **compiler repository root**, the directory containing
`package.json`, `spago.yaml` and `bin/sharpurs`.

## Checkouts and tools

Use the [checkout layout and clone commands](../README.md#build-the-backend) in the
README. The build needs `../../purescript-backend-optimizer-sharpurs` on its
`edge-sharpurs` branch and the sibling overrides in [spago.yaml](../spago.yaml).
CLI fixtures need every `sharpurs-*` sibling listed in [bin/pkg](../bin/pkg);
the runtime suite also reads `sharpurs-exceptions`. Install the backend's npm
dependencies and allow Spago to populate `.spago/p` before running individual suites.

The validated toolchain is Node.js **24.8.0**, Spago **1.0.3**, the TAST fork of
PureScript **0.15.16** and .NET SDK **8.0.423**. Select the tools before starting:

```bash
export PATH="$PWD/node_modules/.bin:$HOME/.dotnet:$PATH"
node --version
spago --version
purs --version
dotnet --version
```

`purs` must emit the [typed payload](../README.md#compile-and-run-an-application),
including `dataDecls`, `classDecls` and expression types in `corefn.json`.
The compiler's version string alone does not establish this.

## One command, one report

```bash
# Build, bundle, PureScript assertions and every test:* suite in package.json.
npm test

# Add CLI fixtures. One bin/test invocation executes the selection sequentially.
npm test -- --fixture PartialFunction --fixture NewtypeEff

# Reuse the current build and focus on two suites.
npm test -- --skip-build --skip-assertions --suite adt-multi --suite thunk-kernel

# Full vendored CLI replay, including the seven documented exclusions.
npm test -- --all-fixtures
```

[tests/run.mjs](../tests/run.mjs) uses the build steps exported by
[scripts/build.mjs](../scripts/build.mjs), then runs each suite in a separate
process. The same entrypoint serves `npm run build`, `prepare`, `bin/test -c` and
`bin/modtest -c`. Tool selection puts repository-local tools first and removes
npm-injected ancestor `node_modules/.bin` paths, including symlink aliases, which
can contain an unrelated legacy Spago. `SPAGO=/absolute/path/to/spago` selects
the compiler build/assertion executable.
Focused suites accept `PURS` and `DOTNET`; the shell CLI runner uses `PATH`.
`PRELUDE_SRC` overrides the focused fixtures' prelude source directory when needed.

Each run prints its artifact directory and one **PASS/FAIL** line per step, then
counts passed, failed and unrun steps. It returns **0 only when every requested
step passed**, **1 on failure**, and **130 on cancellation**. Failed build/bundle
steps stop dependent checks; a failed independent suite allows the remaining
suites to report their own results. Cancellation terminates the active subprocess
group and records the incomplete run.

Logs and `results.json` remain in the printed temporary directory. To choose a
durable location, use a **new** directory:

```bash
npm test -- --artifacts "$HOME/sharpurs-evidence/check-001"
```

Existing artifact directories are rejected rather than overwritten. The report
records the Node version, commands, exit codes/signals, elapsed times and log
paths. Open the failed step's log directly; the runner does not truncate output.
Individual commands such as `npm run test:thunk-kernel` remain available after a build.

### Shared CLI workspace

`bin/test` owns `tests/runner` for the duration of its invocation. Its lock at
`tests/runner/.sharpurs-test.lock` prevents concurrent fixture selections. The
runner snapshots and restores the pre-existing `.purmeta` files, including
untracked entries, on success, failure and handled interruption. Compilation,
backend generation and .NET execution each need to succeed; a stale generated
project cannot turn a generation failure into a passing fixture.

The whole `tests/runner/` directory is generated and ignored by Git, including
optimizer caches, logs and build outputs. Maintained inputs live in
`tests/passing/` and `tests/fixtures/`; the runner creates its workspace from them.

The runner stops at the first failing fixture. Its seven exclusions and reasons
remain in [bin/test](../bin/test). `./bin/test FixtureName -c` rebuilds the backend
and clears the runner's Spago/output caches. If an unhandled termination leaves a
lock, its `pid` identifies the owning process; remove the lock after that process
has ended. A cache restoration failure prints the retained backup location.

After diagnosing an infrastructure failure, `./bin/test --skip-before=FixtureName`
resumes inclusively at that fixture. Retain the failed attempt and verify the
combined successful names against the complete expected inventory, including the
exclusions. Bash's glob order follows its locale; compare inventory membership
independently of a language's default string sort, and retain actual execution
order. A resumed qualification must identify both attempts and their exit codes.

### Shared focused-test support

- [tests/support/fixtures.mjs](../tests/support/fixtures.mjs) handles fixture copying,
  package-source lookup, checked `purs compile` and `dotnet fsi` invocations.
- [tests/support/corefn.mjs](../tests/support/corefn.mjs) loads current CoreFn values
  and runs the shared optimizer callback contract. Its builder changes cwd only
  while writing fixture-local `.purmeta`, restoring it even after an error.
- [tests/support/ast.mjs](../tests/support/ast.mjs) copies prototype-bearing trees
  and provides checked, named access to source/optimized fixture nodes.
- [tests/support/fsharp.mjs](../tests/support/fsharp.mjs) loads suite-owned F#
  fragments from `tests/fixtures/` and validates explicit insertion slots.
- Suites retain their assertions, native/generic/JavaScript oracles, fixtures and
  acceptance criteria. Recursion, case patterns, direct calls, integer arithmetic,
  multi-argument ADTs and thunks share the real-TAST mechanics. ADT lowering also
  uses the copying and F# process helpers for its small typed scenarios.
- Historical oracle adapters retain their own runtime constructors and dictionaries.
  `FFI_SUPPORT_ORACLE` accepts a self-contained older FFI JavaScript module;
  `CONSTRUCTOR_TYPEAPP_ORACLE_OUTPUT` accepts the historical output tree with
  `old.Aff.monadAff` and `old.Adt.prepareUnary`.

Extra artifacts remain suite-specific. For example:

```bash
THUNK_KERNEL_ARTIFACTS="$HOME/sharpurs-evidence/thunks-001" \
  npm test -- --skip-build --skip-assertions --suite thunk-kernel
```

Other supported variables include `CLI_ARTIFACTS`, `PROJECT_ARTIFACTS`, `NAMES_ARTIFACTS`, `FFI_SUPPORT_ARTIFACTS`, `ADT_KERNEL_ARTIFACTS`, `ADT_MULTI_ARTIFACTS`,
`ADT_INTEROP_ARTIFACTS`, `ADT_LOWERING_ARTIFACTS`, `CONSTRUCTOR_TYPEAPP_ARTIFACTS`, `DIRECT_CALL_ARTIFACTS`,
`INT_COMPARISON_ARTIFACTS` and `INT_ARITHMETIC_ARTIFACTS`. The unary and interop ADT
suites both use `ADT_INTEROP_ARTIFACTS`; give them separate destinations in separate
invocations. Aggregate logs survive failures; each suite defines which additional
generated files it retains and at what point.

### Contributing a structural mutation

Copy the real parsed fixture with `clone` before editing it. The helper preserves
PureScript constructor prototypes and recursively copies enumerable fields and
arrays. `structuredClone` and JSON round-trips lose the `instanceof` evidence used
by the compiler. `clone` copies trees, not arbitrary cyclic JavaScript graphs;
its optional post-order transform keeps oracle renaming decisions in the suite.

Select the intended node and state the contract being violated. For example,
the ADT multi suite removes evidence from the second **source** lambda:

```js
const state = clone(producer);
const binding = sourceBinding(state.core, "assemble");
const fn = sourceLambdas(binding.expression, 4, "assemble");
annotation(fn.lambdas[1].node).type = Nothing.value;
assert.notDeepEqual(state, producer, "missing inner lambda type: target changed");
const candidate = prepareModule(state.core)(state.backend);
assert.ok(candidate instanceof Just && !candidate.value0.nativeNames.includes("assemble"),
  "missing source evidence keeps this function generic");
```

Use the source and optimized selectors for their respective representations:

| Selector | Checked shape and returned access |
| --- | --- |
| `sourceBinding(core, name)` / `sourceBindings(core)` | `NonRec`/`Rec` groups and `Binding` nodes; named `name`, `expression`, raw `node` and owning `group` |
| `optimizedBinding(backend, name)` | Optimized groups and binding tuples; named `expression`, `name`, raw `node` and `group` |
| `sourceLambdas(expr, count, label)` | Exact consecutive source-lambda count; ordered `lambdas` and the remaining `body` |
| `sourceApplication(expr, arity, label)` | Exact runtime application count before a boundary; `head`, ordered `args`, and live application `nodes` |
| `sourceTypeApp` / `sourceVariable` | Explicit type-application argument/expression or variable owner/name |
| `annotation` / `annotatedType` | Source annotation with a `Maybe` type; the latter additionally requires `Just` and the requested type constructor |
| `optimizedTyped` / `optimizedLambda` | Optimized annotation and lambda/parameter tuples, including mutable parameter `level` |
| `firstNode(tree, Type, label, predicate)` | First preorder match, with a diagnostic assertion if none exists |

Named views are **live**: setting `binding.expression`, `call.head`,
`call.nodes[i].argument` or `lambda.parameter` updates the original selected tree.
Use `.node` when passing the underlying constructor to production functions.
`call.args` is an ordered array of node references; replace an argument edge via
`call.nodes[i].argument`, not by assigning to that temporary array.

Binding/data-declaration lookup requires exactly one match. Application/lambda
selectors require the expected arity and do not erase `TypeApp` or cross a `let`.
The constructor-TypeApp suite explicitly owns its different boundary traversal.
A lost target must throw before the negative recognizer assertion is reached;
never treat a missing fixture node as a successful rejection.

The support module imports only runtime constructors and structural utilities.
Each suite keeps its production recognizer calls, mutation decisions, rejection
labels, generic/JavaScript oracles and historical compiler adapters. A historical
compiler must construct/read its trees with **its own** runtime constructors and
dictionaries. Do not apply current-constructor selectors to an older output tree.

`npm run test:fixture-support` checks prototype-preserving isolation, live edges,
absent/ambiguous/wrong-shaped targets, annotation evidence, traversal order and
explicit application boundaries. The concrete examples are
[`direct-call.mjs`](../tests/direct-call.mjs),
[`int-arithmetic.mjs`](../tests/int-arithmetic.mjs) and
[`adt-multi.mjs`](../tests/adt-multi.mjs).

### Contributing an F# fixture

Keep static F# support and assertions in the suite's fixture directory, usually
`Support.fs` and `Runtime.fs`. The JavaScript suite explicitly assembles the
runtime prelude, support, generated modules, generic oracle and runtime assertions
in their required order. These fragments are test programs, not application FFI.
The unary and interoperability suites share only their identical `AdtConsumer`
foreign support; their runtime assertions remain separate.

For calculated data or generated assertions, use named slots, for example:

```fsharp
let cases = [
{{CASES}}
]
```

```js
const runtime = await fsharpFixture("int-arithmetic/Runtime.fs", { CASES: fsCases });
const script = [preludeFs, support, generated, fallback, runtime].join("\n\n");
```

Every supplied slot must occur exactly once; missing, repeated and unused slots
fail before F# execution. Replacement text is inserted literally. JavaScript
continues to calculate oracle data and generated assertions; static fixture files
keep the suite-specific runtime contracts visible. Save the **assembled** program
before invoking FSI so that a failure can be replayed directly. The aggregate log
records the process output; the primary mutation suites also retain their
`validation.log` on selector/compilation/runtime failures when their artifact
variable is set.

When refactoring these tests, compare the complete assembled F# files and the
mutated recognizer inputs with a frozen baseline, as well as the suite results.
Matching check counts alone would not detect an accidentally changed oracle call.
The [H06 report](validation/h06-2026-10-03.md) records the exact mutation/program
comparison and the deliberately missing-target failure replay.

### CLI contract and Spago transport

`npm run test:cli` exercises actual compiler processes. Usage errors must exit 2,
print one diagnostic/usage hint to stderr, and leave an input-only workspace
untouched. The cases cover missing/empty values, duplicate valued options,
unknown/case-mismatched options, unexpected positional arguments, packed argument
strings, invalid help values and the four formerly ignored PBO options. Four help
invocations through `bin/sharpurs` must exit 0 with identical stdout and no writes;
a missing TAST directory separately checks pipeline exit code 1.

The dependency-free fixtures under [`tests/fixtures/cli/`](../tests/fixtures/cli/)
have default `Main` and Unicode `App.Entrée` entrypoints. The selected native
implementation exists in a directory containing spaces, `=` and Unicode; a
different implementation in the truncated directory detects accidental splitting.
Eight .NET runtime phases exercise defaults, the end marker, ordinary arguments,
the shell wrapper, equals syntax, a leading-dash directory, Spago's YAML argument
array and repeated `--backend-args`. The last two invoke real Spago in offline mode
with package set 77.10.1 already cached by the backend build.

```bash
CLI_ORACLE=/absolute/path/to/compiler-before.mjs \
CLI_ARTIFACTS="$HOME/sharpurs-evidence/cli-001" \
  npm test -- --suite cli --suite project --suite names --suite tools \
  --fixture PartialFunction
```

The optional `CLI_ORACLE` checks byte/mtime equality with the historical compiler
for existing separate `--main`/`--ffi` arguments, then characterizes its truncated
spaced-path selection. `CLI_ARTIFACTS` retains diagnostics, runtime phase results,
historical outcomes, workspace and commands on success or failure. Whole-compiler
generation comparisons complement this focused test on frozen b8x/native inputs.

### Names, strings and real CLI regression

`npm run test:names` compiles the dependency-free fixtures under
[`tests/fixtures/names/`](../tests/fixtures/names/) to TAST, invokes the built CLI
with the Unicode entrypoint `Naming.Entrée`, then compiles/runs its F#/C# project.
The native checker uses independent .NET strings, including distinct lone UTF-16
surrogates, to check record creation, update, access and patterns. It also checks
ordinary/native constructors, direct/recursive worker names, F#/C# foreign names
and reuse of a partially applied missing FFI.

`NAMES_ARTIFACTS=/new/path` retains the complete workspace and command log on
success or failure. `NAMES_COMPILER=/absolute/path/to/self-contained.mjs` selects
another compiler bundle, allowing a regression to be demonstrated on the prior
compiler with the same fixtures and expected runtime values. This suite needs
the TAST compiler and .NET SDK, but no native-library sibling checkouts.

The [H01 report](validation/h01-2026-10-02.md) records the before/after regression,
the qualified generation comparison and the scope of application validation.

### Project lifecycle and I/O

`npm run test:project` compiles the dependency-free fixtures in
[`tests/fixtures/project/`](../tests/fixtures/project/) to real TAST and runs seven
CLI/MSBuild/runtime phases: mixed F#/C#, its incremental replay, removal of a
module and a C# FFI, another incremental replay, F# only, its incremental replay
and clean regeneration. Each executable must return the same independently
checked value, **42**, and consume an explicit application `ProjectReference`.
The fixture includes a dependency order different from alphabetical order.

Invalid stale `.fs`/`.cs` sources remain in the output directory. The suite checks
the complete compile-item lists, stable C# ordering, disappearance of the obsolete
C# project, identical current files after clean regeneration, exact incremental
mtimes and preservation of `Main/corefn.json` and application-owned files.
Named fixture files replace the native implementations; no generated source is
rewritten to make a build pass.

The CLI must also fail on a directory used as `sharp.packages.props` and name the
path in stderr. [`tests/support/project-io.mjs`](../tests/support/project-io.mjs)
calls production project operations against real missing paths, files in place
of directories, unreadable files, read-only files/directories and an unremovable
obsolete project. It checks error codes, paths and original causes, as well as
successful absent-fragment and unchanged-read-only-output behavior. POSIX
permission cases are explicitly reported as skipped when running as root;
other cases still run. The H04 validation ran all eleven failure cases as a
non-root user, with no skips.

```bash
PROJECT_ARTIFACTS="$HOME/sharpurs-evidence/project-001" \
  npm test -- --suite project --suite ffi-support --suite names --suite tools
```

`PROJECT_ARTIFACTS` retains each completed phase's generated files, content/mtime
manifest, I/O failure report and command log. `PROJECT_COMPILER` can select a
self-contained historical CLI bundle: the H03 bundle fails the first phase's
compile-item assertion because it includes the invalid leftover C# file.
See the [H04 report](validation/h04-2026-10-02.md) for the exact qualification.

### Call conventions

`npm run test:printer` constructs calls with `FsAst.directCall` and
`FsAst.boxedNativeCall`, then reuses each AST in declarations, lambdas, constructors,
native factories, guards, branches, boxed branches, generic applications and
records. Native, object and mixed nested calls must retain their adapters in every
placement. The suite compiles and executes **67 F# checks**, including named values,
reusable partials, a returned function, evaluation order and the existing pattern/
layout checks. `printExprInline` is now an alias of `printExpr`.

For a source-call change, also run `direct-call`, `recursion`, `case-patterns`,
`selection`, the constructor suite and the affected kernel suites. Their real TAST
fixtures check eligibility, partial/over-application and exception boundaries.
The [H02 report](validation/h02-2026-10-02.md) records their replay and strict
before/after generation comparisons on frozen b8x and native inputs.

### ADT lowering differential replay

`npm run test:adt-lowering` calls the production `Lower.binding` with **108 typed
scenarios**: the ten admitted primitives, calls, constructors, fields, tags,
lexical bindings, branches and failures, plus contradictory/unsupported inputs.
It compiles/runs **109 F# checks** of values, sharing, branch/argument order,
short-circuiting, string escaping and exception envelopes. Static F# support and
assertions live in [`tests/fixtures/adt-lowering/`](../tests/fixtures/adt-lowering/).
The real-TAST ADT kernel/interop/unary/multi and constructor suites complement
these deliberately small lowering inputs.

Before editing the compiler, build it and snapshot the complete lowering API:

```bash
EVIDENCE=$(mktemp -d "${TMPDIR:-/tmp}/sharpurs-adt-lowering.XXXXXX")
export PATH="$PWD/node_modules/.bin:$HOME/.dotnet:$PATH"
spago build
./node_modules/.bin/esbuild tests/support/adt-lowering-api.mjs \
  --bundle --platform=node --format=esm --outfile="$EVIDENCE/adt-lowering-before.mjs"

# After editing, rebuild and compare against that immutable bundle.
ADT_LOWERING_ORACLE="$EVIDENCE/adt-lowering-before.mjs" \
ADT_LOWERING_ARTIFACTS="$EVIDENCE/adt-lowering-after" \
  npm test -- --suite adt-lowering --suite adt-kernel --suite adt-multi
```

The test rebuilds each scenario with each compiler's own PureScript constructors.
It compares admission/rejection, parameter levels/native types and every emitted
body byte-for-byte; it also compares the complete generated definitions. This
avoids mixing old/new `instanceof` identities or importing current dependencies
into the historical lowering implementation. With no oracle configured, the
same admission and F# behavior assertions still run.

`ADT_LOWERING_ARTIFACTS` retains the generated F#, static support/assertions,
per-scenario results, source hashes and runtime log. The
[H03 report](validation/h03-2026-10-02.md) records the historical differential and
the independent whole-compiler generation comparison.

## Compare complete generations before and after a change

Save **self-contained compiler bundles**. Copying one module from `output/` can
still import current dependencies and is not a whole-compiler baseline.

```bash
# Before editing the compiler; keep EVIDENCE for the remaining commands.
EVIDENCE=$(mktemp -d "${TMPDIR:-/tmp}/sharpurs-refactor.XXXXXX")
export PATH="$PWD/node_modules/.bin:$HOME/.dotnet:$PATH"
spago build
spago bundle --module Main --platform node --outfile bin/sharpurs.js --bundle-type app
cp bin/sharpurs.js "$EVIDENCE/compiler-before.mjs"

# After the change, rebuild and snapshot the other compiler.
spago build
spago bundle --module Main --platform node --outfile bin/sharpurs.js --bundle-type app
cp bin/sharpurs.js "$EVIDENCE/compiler-after.mjs"

npm run compare:generation -- \
  --before "$EVIDENCE/compiler-before.mjs" \
  --after "$EVIDENCE/compiler-after.mjs" \
  --workspace /absolute/path/to/typed-application-workspace \
  --main Test.Main \
  --artifacts "$EVIDENCE/generation"
```

Prepare the application input once with its TAST toolchain. `--workspace` must
contain `output/*/corefn.json` and the sources/native dependency paths referenced
by those files. Preserve the application's native overrides and optional
`sharp.packages.props` item fragment. For b8x, use its prepared Sharpurs workspace
and select **`Test.Main`**. The comparison snapshots the inputs into its own
directory, keeping the application's working output intact. `--main` defaults to
`Main`; `--ffi /path/to/native-overrides` snapshots an explicit FFI search directory.

[scripts/compare-generation.mjs](../scripts/compare-generation.mjs) performs three
sequential, separate Node invocations on the **same snapshotted inputs**:

1. **Before:** generate with the old bundle in a fresh output directory.
2. **After incremental:** generate with the new bundle over those files. File lists
   and SHA-256 content must match; unchanged files must keep their nanosecond mtimes.
3. **After clean:** remove emitted files and regenerate with the new bundle. File
   lists and content must still match. This catches stale files that masked a
   removed output during the incremental run, including obsolete C# project items.

The snapshot copies native companion files, `.spago`/`spago.d` native directory
trees, both maintained `bak/spago.d/fs/p` override roots, local native files and
`sharp.packages.props`. It rewrites only `modulePath` in each input JSON to point
to the copied source, using paths relative to the sandbox. Native directories are
preserved even when empty because the resolver observes their layout. File hashes
and native directory listings are checked again after each generation.

Find the evidence here:

| Artifact | Purpose |
| --- | --- |
| `comparison.json` | Overall success, bundle hashes, commands and changed/added/removed/rewritten file lists. Exit code is 0 only on an identical comparison. |
| `before.log`, `after-incremental.log`, `after-clean.log` | Complete generation output and errors for each subprocess. |
| `before.json`, `after-incremental.json`, `after-clean.json` | Per-file SHA-256, byte size and original `mtimeNs`, including `.fsproj`, `.csproj` and `.props`. Input CoreFn is excluded from output comparisons. |
| `generated-before/`, `generated-after-incremental/`, `generated-after/` | All three generated trees for examining the actual differing text. |
| `inputs.json`, `input-origins.json` | Snapshot hashes/directory listings and original input locations/hashes. |
| `compiler-before.mjs`, `compiler-after.mjs`, `sandbox/`, `sources/`, `bak/` | Retained bundles and isolated inputs; `bak/` exists when that override root is present. |

Use `diff -u` on a file listed in `comparison.json` to inspect a mismatch. The clean
pass records mtimes for evidence but compares content only. These checks establish
generation equivalence; the application build/runtime tests and benchmark results
are additional integration checks. Performance comparisons use separate processes
with identical workloads and no concurrent build.

`npm run test:tools` exercises orchestration failures, cancellation, CLI locking
and cache restoration, and comparison failures (content, removed outputs, mtimes,
input mutation and process errors) using isolated child processes.

Its build regressions invoke real `npm run build` and nested `prepare` processes
in isolated projects with a deliberately failing ancestor Spago. They check
caller-selected, local and explicit tools (including paths with spaces), compile
and bundle failure propagation, missing executables, and delegation from both
clean-build runners. The PATH checks also cover an ancestor-bin symlink alias.

## Full integration replay

### Pin the run before building

Create a fresh evidence directory and record the compiler/dependency revisions,
working-tree status, source hashes, lockfiles, executable paths/versions and the
self-contained current compiler bundle. Include uncommitted maintained inputs.
Record application target links and the bytes/mtimes of the caches and working
outputs being preserved. Use separate copies for library scripts with cross-sibling
cleanup, and isolated output directories for application compilation.

The cycle-2 inventory is **23 focused suites**, **49 PureScript assertions**,
**359 active CLI fixtures**, **seven exclusions** and **19 native test modules**.
The complete aggregate therefore has **27 steps**: build, bundle, PureScript
assertions, the 23 suites, and one sequential CLI-fixture selection. Capture the
actual inventories from `package.json`, `tests/passing/`, `bin/test` and
`bin/modtest --list`; compare the executed names and exclusions, not just totals.
Relative to M11, the five additional suites are `fixture-support`, `cli`,
`project`, `names` and `adt-lowering`; the vendored CLI/native inventories are the
same. A future added or removed case should appear explicitly in the run report.

### Compiler and native libraries

```bash
# From the compiler root, with the toolchain selected as above.
npm test -- --all-fixtures --artifacts /absolute/path/to/new-check-report
./bin/modtest --list
./bin/modtest --all
```

`bin/modtest` selects sibling `sharpurs-*` directories containing an executable
`bin/test` and runs their scripts sequentially. Use an isolated copy of the
checkout layout for this replay: library scripts clear both their own and their
siblings' generated outputs and dependency caches. Include all local dependency
overrides, the compiler wrapper/bundle, `scripts/build.mjs`,
`scripts/support/process.mjs` and `tools/modtest-runner.mjs` in that copy.
The selected module list, library source hashes and final exit code identify the
tested scope. `--skip-before NAME` resumes inclusively after a diagnosed failure.
After the aggregate, compare the runner's `.purmeta` existence, file inventory,
contents and nanosecond mtimes with the pre-run snapshot. An initially absent
cache must remain absent. Recheck the original library inputs after `modtest`.

### Application integration: b8x `Test.Main`

Prepare an isolated application workspace with freshly compiled typed input,
the native dependency/override paths and `sharp.packages.props`. Use the
application's Sharpurs dependency profile to resolve the source files, then send
`purs compile --codegen corefn --output /path/to/isolated/output` to the isolated
destination. The application's active target links remain an independent profile.

From that prepared workspace:

```bash
/absolute/path/to/sharpurs/bin/sharpurs --main Test.Main
dotnet build output/Main/Program.fsproj -c Release --nologo -p:NuGetAudit=false
dotnet output/Main/bin/Release/net8.0/Program.dll
```

The PostgreSQL integration uses Npgsql **8.0.7** through the application's package
fragment. Its FFI reads `POSTGRES_HOST`, `POSTGRES_HOST_STORE`,
`POSTGRES_HOST_STORE_LOCK`, `POSTGRES_HOST_EDGE`, `POSTGRES_PORT`,
`POSTGRES_DB_STORE`, `POSTGRES_DB_EDGE`, `POSTGRES_USER` and `POSTGRES_PASSWORD`.
Set these for the test services; the host replay uses the published PostgreSQL
port and the `store`/`edge` root databases. RabbitMQ configuration uses
`RABBITMQ_HOST`, `RABBITMQ_PORT`, `RABBITMQ_USER` and `RABBITMQ_PASS`.

Require both a zero process exit code and the expected **286/286** summary in
**ten successive fresh processes**, recording the same Release assembly hash and
a separate complete log for each execution. Finish this application series before
replaying the benchmark entrypoint.
The fixtures own creation, connection teardown and deletion of their temporary
databases. Check their database selector before and after execution:

```bash
docker exec "$POSTGRES_CONTAINER" sh -lc \
  'exec psql -U "$POSTGRES_USER" -d postgres -Atc "$1"' sh \
  "SELECT datname FROM pg_database WHERE datname LIKE 'store_test_%' OR datname LIKE 'edge_test_%' ORDER BY datname"
```

The qualified baseline has an empty result before the series and after **every**
run, including a failed process. Retain the observed names if cleanup fails so they
can be attributed to the failing execution. Recheck source/typed-input hashes,
original output/cache contents and mtimes, and application target links afterward.

### Benchmark results

The reference CPU workloads live in `altbak.pub-sharpurs` and run through its real
`App.main`. Compile that entrypoint's source dependencies with the TAST toolchain,
generate with `--main App`, build the .NET project, and execute its DLL in **three
successive fresh processes**. Keep the existing workload sources, warm-ups, result consumption and
measurement driver intact.

Compare all **14 output values, names and their order** with the archived reference
at `scratch/sharpurs-thunk-native-20260909/expected-output.log`. Also check that the
displayed total agrees with the sum of the individual scores within their printed
rounding precision. Source, typed-input and assembly hashes identify the executed
program. A result-validation replay establishes output preservation; a timing
comparison additionally needs identical workloads, separate processes and all
builds completed before the measurement series. Record the protocol before
execution: entrypoint, immutable source/input/assembly identities, reference
outputs, repetition count and total-rounding check. For a timing comparison,
also define the before/after versions, process order and aggregation in advance.

### Close with checked evidence

Retain commands, exit codes, complete logs, compiler and application diagnostics,
source/input/output manifests, generation comparisons and assembly hashes. Link a
human-readable report to a machine-readable summary and a durable archive with a
SHA-256 checksum; verify every archive member against its recorded bytes. A final
source/cache check ties the results to the actual validated inputs. Record failed
attempts and their diagnosed retries alongside the successful evidence.

The [H07 cycle-2 report](validation/h07-2026-10-03.md) records the complete
qualification, including the diagnosed disk-exhaustion failure, inclusive CLI
recovery, requested cleanup and attributed external application change. The
[M11 replay report](validation/m11-2026-10-02.md) remains the first-cycle reference.
