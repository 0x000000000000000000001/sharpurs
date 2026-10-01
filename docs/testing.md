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

[tests/run.mjs](../tests/run.mjs) invokes Spago directly, then runs each suite in a
separate process. It puts repository-local tools first and removes npm-injected
ancestor `node_modules/.bin` paths, which can contain an unrelated legacy Spago.
`SPAGO=/absolute/path/to/spago` selects the aggregate build/assertion executable.
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

The runner stops at the first failing fixture. Its seven exclusions and reasons
remain in [bin/test](../bin/test). `./bin/test FixtureName -c` rebuilds the backend
and clears the runner's Spago/output caches. If an unhandled termination leaves a
lock, its `pid` identifies the owning process; remove the lock after that process
has ended. A cache restoration failure prints the retained backup location.

### Shared focused-test support

- [tests/support/fixtures.mjs](../tests/support/fixtures.mjs) handles fixture copying,
  package-source lookup, checked `purs compile` and `dotnet fsi` invocations.
- [tests/support/corefn.mjs](../tests/support/corefn.mjs) loads current CoreFn values
  and runs the shared optimizer callback contract. Its builder changes cwd only
  while writing fixture-local `.purmeta`, restoring it even after an error.
- Suites retain their assertions, native/generic/JavaScript oracles, fixtures and
  acceptance criteria. Six suites share these mechanics: recursion, case patterns,
  direct calls, integer arithmetic, multi-argument ADTs and thunks.
- Historical oracle adapters retain their own runtime constructors and dictionaries.
  `FFI_SUPPORT_ORACLE` accepts a self-contained older FFI JavaScript module;
  `CONSTRUCTOR_TYPEAPP_ORACLE_OUTPUT` accepts the historical output tree with
  `old.Aff.monadAff` and `old.Adt.prepareUnary`.

Extra artifacts remain suite-specific. For example:

```bash
THUNK_KERNEL_ARTIFACTS="$HOME/sharpurs-evidence/thunks-001" \
  npm test -- --skip-build --skip-assertions --suite thunk-kernel
```

Other supported variables include `FFI_SUPPORT_ARTIFACTS`, `ADT_MULTI_ARTIFACTS`,
`ADT_INTEROP_ARTIFACTS`, `CONSTRUCTOR_TYPEAPP_ARTIFACTS`, `DIRECT_CALL_ARTIFACTS`,
`INT_COMPARISON_ARTIFACTS` and `INT_ARITHMETIC_ARTIFACTS`. The unary and interop ADT
suites both use `ADT_INTEROP_ARTIFACTS`; give them separate destinations in separate
invocations. Aggregate logs survive failures; each suite defines which additional
generated files it retains and at what point.

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
