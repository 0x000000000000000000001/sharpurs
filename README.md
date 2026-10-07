# 🔷 sharpurs

<img height="160" alt="sharpurs logo" src="https://github.com/user-attachments/assets/cfbf17c1-ada5-40ff-b804-e8f9cc75e328" />

_Experimental WIP. Typed code generation and the native FFI ecosystem are actively evolving._

An optimizing **PureScript-to-F# compiler**, written in PureScript with JavaScript FFI helpers, bringing PureScript's pure business logic to **.NET**, with its cross-platform runtime, threading facilities and library ecosystem. F# is the generated language; **C# is also supported for FFI implementations**. Node.js runs the compiler; the generated application runs on .NET.

`sharpurs` uses the enriched **TAST / Typed CoreFn (`tcorefn`)** produced by a [custom PureScript compiler](https://github.com/0x000000000000000000001/purescript). This retains structural types, ADT layouts, type-class declarations and type applications for native code generation. In the current toolchain, that typed payload is written to **`output/<Module>/corefn.json`**. The filename does not make it equivalent to the type-erased CoreFn emitted by the upstream compiler.

## Features

F# provides functional programming, discriminated unions and pattern matching on the same runtime as C#. `sharpurs` aims to let PureScript developers keep their pure code and type system while using .NET for services, command-line tools and existing applications.

- **Optimization before F# generation.** The compiler is implemented in PureScript with JavaScript FFI helpers. It uses the [`edge-sharpurs` fork of purescript-backend-optimizer](https://github.com/0x000000000000000000001/purescript-backend-optimizer/tree/edge-sharpurs), derived from [Arista's optimizer](https://github.com/aristanetworks/purescript-backend-optimizer), for transformations such as inlining and constant folding.
- **Selected native representations.** Supported integer loops, arithmetic, direct calls, constructor calls and ADT workers use typed F# code. Validated ADT layouts become discriminated unions with native fields. General code and interoperability still use boxed `obj` values, curried functions and dynamic records; native representation is selected per supported shape.
- **F# and C# foreign implementations.** Companion `.fs` and `.cs` files can coexist in one project. The backend emits curried wrappers and boxing conversions for simple exported declarations. Foreign code remains responsible for matching PureScript types and effect semantics.
- **An ordinary .NET project.** Generated applications target `net8.0` with F# language version `7.0`. The .NET SDK builds the F# executable and any accompanying C# library project.
- **Native asynchronous primitives.** [`sharpurs-aff`](https://github.com/0x000000000000000000001/sharpurs-aff) uses F# `Async`, .NET tasks and cancellation tokens. Its implementation and process-lifetime constraints are described [below](#asynchronous-io-and-concurrency-aff).

## Benchmarks

The [F#/C# results in altbak.pub](https://github.com/0x000000000000000000001/altbak.pub#fc) compare compiled PureScript with native F#/C# implementations on CPU and allocation workloads. The repository also records [extended workloads covering I/O, mutability and async](https://github.com/0x000000000000000000001/altbak.pub#extended-benchmark-results-io-mutability-async); its current extended results table covers JavaScript and Go, not sharpurs.

Use those documented baselines, workload definitions and execution conditions when assessing a change. Results vary substantially by algorithm and representation; an isolated timing does not establish a general advantage over V8 or another backend.

## Getting started

### Prerequisites

- **Node.js and npm** to install and run the compiler.
- **Spago** on `PATH`. The optimizer checkout declares `^0.93.45`, and the package configurations use registry `77.10.1`. The console example below has also been checked with Spago `1.0.3`.
- A **TAST-capable `purs`** from the custom compiler fork for application builds. The backend's npm development dependency uses the [`purescript-npm` wrapper](https://github.com/0x000000000000000000001/purescript-npm), but its package name or version alone does not establish that the installed binary emits the required typed payload. Check the generated JSON as described below.
- The **.NET 8 SDK**, including F#, to build the generated `net8.0` projects. `dotnet` must be on `PATH`; installing only the runtime is insufficient for compilation.
- **Git** and the local compiler/FFI checkouts described below. The development scripts use Bash.

### Build the backend

The current [spago.yaml](spago.yaml) references a local optimizer checkout and sibling library forks. Its npm `prepare` hook runs the compiler build, so a bare `npm install --save-dev github:0x000000000000000000001/sharpurs` is not a self-contained installation while those paths are required.

Use this layout for the checked-in configuration:

```text
workspace/
├── purescript-backend-optimizer-sharpurs/
└── sharpurs/
    ├── sharpurs/                 # this compiler repository
    ├── sharpurs-prelude/
    ├── sharpurs-effect/
    ├── sharpurs-console/
    ├── sharpurs-st/
    ├── sharpurs-unsafe-coerce/
    ├── sharpurs-assert/
    ├── ...                      # other forks listed in bin/pkg
    └── my-app/
```

From an empty workspace directory, clone the optimizer branch and backend:

```bash
git clone --branch edge-sharpurs \
  https://github.com/0x000000000000000000001/purescript-backend-optimizer.git \
  purescript-backend-optimizer-sharpurs
mkdir sharpurs
cd sharpurs
git clone https://github.com/0x000000000000000000001/sharpurs.git
cd sharpurs
```

Clone the test runner's core package list from the compiler root. Run this snippet in **Bash**, since `bin/pkg` defines a Bash array:

```bash
bash <<'BASH'
set -e
source bin/pkg
for pkg in "${CORE_PACKAGES[@]}"; do
  if [ ! -d "../sharpurs-$pkg" ]; then
    git clone "https://github.com/0x000000000000000000001/sharpurs-$pkg.git" \
      "../sharpurs-$pkg"
  fi
done
BASH
```

With the prerequisites and local paths in place, install and build:

```bash
npm install
# After subsequent compiler changes:
npm run build
```

`npm install` runs `prepare`, which invokes `npm run build`. The build compiles the PureScript sources and bundles `Main` into `bin/sharpurs.js` for Node.js. The checked-in `bin/sharpurs` wrapper invokes that bundle with larger Node stack and heap limits.

`scripts/build.mjs` owns those two build commands, also used by the aggregate
checks and the CLI/native-library clean-build entrypoints. It selects the
repository-local tools before the caller's `PATH` and removes npm-injected
ancestor `node_modules/.bin` directories, including symlink aliases. This keeps an
ancestor's legacy Spago from replacing the toolchain for `spago.yaml`.
`SPAGO=/absolute/path/to/spago npm run build` explicitly selects the build tool;
the executable path is passed as one argument, including spaces.

Each backend invocation reports monotonic elapsed times to stderr, in milliseconds, for `load TAST + sort`, `prepare`, `optimize + emit`, `finalize`, and `backend total`. The total includes these phases; it excludes the earlier `purs` compilation and target-language compilation or execution. Each phase waits for its asynchronous callbacks and file writes to finish. Failed phases and the total are marked `(failed)`, and the original error is rethrown.

### Compile and run an application

Create `my-app` beside the compiler and library forks in the layout above. A minimal `spago.yaml` for a console application is:

```yaml
package:
  name: my-app
  dependencies:
    - prelude
    - effect
    - console
workspace:
  packageSet:
    registry: 77.10.1
  extraPackages:
    prelude:
      path: "../sharpurs-prelude"
    effect:
      path: "../sharpurs-effect"
    console:
      path: "../sharpurs-console"
  backend:
    cmd: ../sharpurs/bin/sharpurs
```

Add `src/Main.purs`:

```purescript
module Main where

import Prelude
import Effect (Effect)
import Effect.Console (log)

main :: Effect Unit
main = log "Hello from PureScript on .NET!"
```

Use `sharpurs-*` overrides for every dependency whose foreign implementation needs to run on .NET, including transitive dependencies. Packages containing only PureScript can generally remain on the official registry. The three overrides above cover this example, not a complete application ecosystem.

For a Git-based application configuration, an override can instead use:

```yaml
prelude:
  git: "https://github.com/0x000000000000000000001/sharpurs-prelude.git"
  ref: "main"
  dependencies: []
```

Pin compatible revisions when maintaining an application. The [`sharpurs-hello-world`](https://github.com/0x000000000000000000001/sharpurs-hello-world) checkout contains a broader local override configuration and examples mixing F# and C# FFI.

From the application root, with the TAST compiler on `PATH`:

```bash
# Replace this directory with the one containing your TAST-capable purs binary.
export PATH="/path/to/tast-compiler/bin:$PATH"
spago build
dotnet run --project output/Main/Program.fsproj -c Release
```

Spago requests the `corefn` code-generation target when a backend is configured. Check that `output/Main/corefn.json` contains the fork's `dataDecls` and `classDecls` fields, and that expression annotations retain type information, directly or through the `typeTable` used by newer fork revisions. A version string alone is insufficient: a compiler can emit ordinary CoreFn successfully while omitting those fields. After changing compiler binaries, clear the application's generated `output/` and rebuild so cached JSON cannot hide a mismatch.

The configured backend reads the typed JSON from `output/` and writes a complete project to **`output/Main/`**, even when the selected entry module has another name:

| Generated file | Purpose |
| --- | --- |
| `Sharpurs_Prelude.fs` | Shared runtime helpers and event-loop bookkeeping. |
| `<Module>.fs` | Generated PureScript module and F# FFI wrappers, in dependency order in the project. |
| `EntryPoint.fs` | Calls the selected module's `main`. |
| `Program.fsproj` | Executable F# project targeting `net8.0`. |
| `<Module>.cs` and `FFI.CSharp.csproj` | C# sources and referenced library project, when C# FFI is present. |
| `Directory.Build.props` | Separate intermediate build directories for the F# and C# projects. |

Project compile items come from the **current invocation's emitted sources**.
F# files retain dependency order; C# files have stable filename order. An old
`.fs` or `.cs` left in `output/Main/` is not added implicitly. When no C# FFI is
emitted, the backend removes its obsolete `FFI.CSharp.csproj`. Identical generated
text keeps its timestamp. `output/Main/corefn.json` remains an input and is
preserved alongside the project files.

To compile without running:

```bash
dotnet build output/Main/Program.fsproj -c Release
```

Run the generated project with `dotnet run`; the backend does not emit a standalone `main.fs` script for `dotnet fsi`.

### Compiler options

Invoke the backend from the application root after producing its typed `output/`:

```bash
../sharpurs/bin/sharpurs --main App.Main --ffi "ffi with spaces"
../sharpurs/bin/sharpurs --help
```

The backend receives each argument intact. To supply the same arguments through
Spago, use an array in the workspace configuration:

```yaml
workspace:
  backend:
    cmd: ../sharpurs/bin/sharpurs
    args: ["--main", "App.Main", "--ffi", "ffi with spaces"]
```

Or override the array from the Spago command line, once per argument:

```bash
spago build --backend-args=--main --backend-args=App.Main \
  --backend-args=--ffi --backend-args="ffi with spaces"
```

| Option | Behavior |
| --- | --- |
| `--main <Module>` | Selects the module whose `main` is called; defaults to `Main`. It does not select modules for entrypoint-based dead-code elimination. |
| `--ffi <Directory>` | Adds a directory to the FFI search paths. A companion file beside the original `.purs` source takes precedence. |
| `--help`, `-h` | Prints usage to stdout and exits 0 before reading TAST or writing generated files. |

`--main` and `--ffi` also accept `--name=value`. Values must be nonempty and each
valued option may appear only once. A value beginning with `-` needs the equals
form, for example `--ffi=-native`. Spaces, Unicode and additional `=` characters
in a value are preserved. There are no positional arguments; a final `--` is
accepted as an empty end-of-options marker.

Invalid arguments print a diagnostic to stderr and exit **2 before generation**.
Every argument is checked, including when `--help` is present. Compilation errors
exit **1**. The formerly ignored PBO options `--bundle`, `--output`, `--rewrite-limit`
and `--autoload-path` are now explicitly rejected, as are unknown options. Input
remains fixed to `output/`, generated files go to `output/Main/`, and the optimizer
rewrite limit is `10000`; keep Spago's default output directory. There is no
automatic discovery of every module exporting `main`.

**Migration:** replace a single `--backend-args "--main App.Main --ffi ffi"` string
with the argument array or repeated options above. Sharpurs no longer re-splits
arguments on spaces; doing so would corrupt quoted directory names. See the
[CLI contract](docs/compiler.md#command-line-boundary) for the implementation boundary.

## Foreign function interface

Place the implementation beside the corresponding `.purs` file. For example, `src/Hello.purs`:

```purescript
module Hello where

foreign import greeting :: String -> String
```

An F# implementation in `src/Hello.fs` can use a typed parameter:

```fsharp
module Hello

let greeting (name: string) = "Hello " + name + " from F#!"
```

Alternatively, `src/Hello.cs` can provide the same import:

```csharp
namespace Hello;

public static class FFI
{
    public static string greeting(string name)
    {
        return "Hello " + name + " from C#!";
    }
}
```

For C# bindings, the namespace matches the PureScript module name and the class is named `FFI`. The wrapper generator looks for public static methods, matching their names case-insensitively. For F#, it discovers top-level `let` definitions. Both generators infer arity from source text and insert curried `obj` wrappers, `unbox` calls and result boxing. They are lightweight source recognizers, not full F#/C# parsers; keep exported declarations simple, and use a small wrapper for complex native signatures.

An `Effect` implementation must defer its work until the effect is invoked. For example, the F# shape of `String -> Effect Unit` is:

```fsharp
let logMessage (message: string) =
    box (fun (_: obj) -> printfn "%s" message; box null)
```

The native wrapper does not infer PureScript effect semantics for you. Use the implementations in `sharpurs-effect`, `sharpurs-console` and `sharpurs-aff` as references for effects and callbacks.

If both companion files exist, `.fs` supplies the PureScript-facing wrappers and `.cs` is still compiled, so the F# code can call C# helpers explicitly. The backend also searches local and Spago package directories for foreign implementations.

### .NET dependencies

For additional F# project dependencies, place a `sharp.packages.props` file beside the application's `spago.yaml`:

```xml
<PackageReference Include="Npgsql" Version="8.0.7" />
```

The backend inserts this optional fragment verbatim into the generated `Program.fsproj`'s `ItemGroup`. It contains item elements, not an outer `<Project>` or `<ItemGroup>`. An absent file is optional; a read/access error fails generation with the affected path. Generated projects are updated only when their contents change.

For application-owned F# or C# code, keep a separate .NET project and add an
explicit reference in the same fragment:

```xml
<ProjectReference Include="../../native/Application.csproj" />
```

Relative paths are resolved from `output/Main/Program.fsproj`. This replaces the
old implicit inclusion of arbitrary `.cs` files placed in `output/Main/`. The
fragment is not added to `FFI.CSharp.csproj`; dependencies used directly by
generated C# FFI still need their own repeatable project-configuration step.

## Development and testing

The checked-in [bin/pkg](bin/pkg) is the authoritative list of sibling packages required by [bin/test](bin/test). The runner creates its Spago configuration on first use, copies fixtures into `tests/runner/src`, compiles them, generates F#, and executes the generated project in Release mode.

From the compiler root:

```bash
# Build, bundle, PureScript assertions and every focused suite, with retained logs.
npm test

# Include a sequential selection of complete CLI fixtures.
npm test -- --fixture PartialFunction --fixture NewtypeEff

# Run the vendored passing-test suite using the current backend bundle.
./bin/test

# Rebuild the backend, clean the runner caches, and run one fixture.
./bin/test TCO -c

# List native-library tests, then run one in its own private sibling layout.
./bin/modtest --list
./bin/modtest partial

# Preview an inclusive native resume selection without building or running it.
./bin/modtest --skip-before=partial --list
```

`npm test` prints a per-step PASS/FAIL summary and the location of its logs and `results.json`; it returns nonzero if any requested check fails. Use `--suite NAME` to select focused suites, `--skip-build` to reuse the current build, and `--artifacts NEW_DIRECTORY` to retain the report at a chosen location. `npm test -- --all-fixtures` includes the full CLI replay. See [Replaying compiler checks](docs/testing.md) for prerequisites, options, failure locations and the before/after generation comparison command.

The runner skips seven fixtures: five require newer compiler features, `4179` relies on JavaScript-specific behavior, and `TCOMutRec` expects a stack overflow that this backend's tail-call optimization avoids. The reasons live beside the blacklist in `bin/test`. The runner stops at the first failure, locks its shared workspace and restores the pre-existing `.purmeta` cache on exit. Its result applies to this vendored suite and the selected toolchain; it is not a claim that every current upstream PureScript test or library is supported. The `-c` / `--clean` option clears the runner's `.spago`, `output` and `output-es` contents.

Each native-library `bin/test` delegates to the compiler's shared runner. It copies
the sibling sources into a fresh private layout and selects `spago.sharp.yaml`,
then `spago.fs.yaml`, then `spago.yaml`. Original profiles, caches and outputs keep
their bytes and timestamps. A library's `bin/test -c` rebuilds the compiler inside
that private layout; compiler-root `bin/modtest -c` rebuilds the installed compiler
once before the selected modules. Failure workspaces are retained and printed;
`SHARPURS_NATIVE_ARTIFACTS` also retains successful runs. See the
[native replay and resume contract](docs/testing.md#compiler-and-native-libraries)
and the [workspace ownership table](docs/testing.md#workspace-ownership).

Focused regression commands are defined in [package.json](package.json):

| Commands | Coverage |
| --- | --- |
| `npm run test:tools` | Aggregate/modtest/native-runner failures and cancellation, CLI locking/cache restoration, private profiles/workspaces and generation manifest comparisons. |
| `npm run test:fixture-support` | Structural selectors and F# slots, deterministic package-source resolution, checked commands, cwd restoration and failed-workspace retention. |
| `npm run test:cli` | Usage diagnostics/codes, write-free help, exact spaced FFI paths, shell-wrapper and real Spago invocation, plus optional historical comparison. |
| `npm run test:runtime` | Generic function application, FFI wrappers and exception boundaries. |
| `npm run test:ffi-support` | F#/C# declaration forms, values/functions, partials, effects, native-file precedence and missing implementations in a generated .NET project. |
| `npm run test:project` | Real CLI/MSBuild lifecycle: output inventory/order, stale-source exclusion, module/FFI removal, explicit references, incremental/clean generation and filesystem errors. |
| `npm run test:names` | Real CLI/TAST integration: Unicode modules, primed foreigns/workers, reserved native names and exact UTF-16 record keys across creation/update/access/patterns. |
| `npm run test:printer` | Direct-call rendering conventions, structured patterns and nested recursive layout. |
| `npm run test:recursion` | Boxed recursive workers, partial/value uses, mixed arities and nested scopes against the JavaScript backend. |
| `npm run test:case-patterns` | Ordinary and deep nested matches, bound fields, newtypes, guards and scrutinee evaluation order. |
| `npm run test:kernel`, `npm run test:local-kernel` | Native integer loops and locally nested kernels. |
| `npm run test:selection` | Implementation priorities, whole-group fallback, direct-helper registration and name collisions. |
| `npm run test:adt-kernel`, `npm run test:adt-interop` | Typed ADT generation and boxed/native boundaries. |
| `npm run test:adt-lowering` | Typed body admission/rejection, F# evaluation/sharing/exception behavior and optional historical lowering comparison. |
| `npm run test:adt-unary`, `npm run test:adt-multi` | Native recursive ADT workers and multiple arguments. |
| `npm run test:int-comparison`, `npm run test:int-arithmetic` | Integer comparison, overflow, division and modulo behavior. |
| `npm run test:direct-call` | Saturated calls and preservation of the generic fallback. |
| `npm run test:thunk-kernel` | Typed thunk selection, independent/multiple captures, reused partials, delay and exception boundaries. |
| `npm run test:constructor-typeapp` | Constructor calls with explicit and inferred type applications. |

Build the compiler first: the focused tests import its `output/` modules, including the runtime source exported by `Sharpurs.Runtime`. The runtime test additionally needs `sharpurs-exceptions`. Suites with PureScript fixtures use the TAST `purs` and run generated F# through `dotnet fsi`; `test:ffi-support` and `test:names` compile and run generated F#/C# projects. `PURS=/path/to/purs` and `DOTNET=/path/to/dotnet` select those executables in the focused scripts. The shell runner instead uses `purs` and `dotnet` through `PATH`.

The [cycle-2 integration report](docs/validation/h07-2026-10-03.md) records the
compiler/tool revisions, fixture and library checks, b8x `Test.Main` executions,
database cleanup and reference benchmark outputs. The
[cycle-3 evidence index](docs/testing.md#cycle-3-validation-evidence) links the
subsequent runner/support/template qualifications and their archives. The
[full replay procedure](docs/testing.md#full-integration-replay) connects these
checks to their commands and required application configuration.

## Architecture

The main parts of the compilation pipeline are:

1. **Typed input:** the custom `purs` compiler emits enriched `corefn.json` files. The TAST-aware optimizer reader decodes them and sorts modules by dependencies.
2. **Optimization and selection:** the optimizer prepares `BackendModule` values. `Sharpurs.IntKernel`, `Sharpurs.AdtKernel` and `Sharpurs.ThunkKernel` select supported typed transformations while retaining access to the source AST. The ADT kernel's `prepareModule` returns an `AdtModule`: `Analysis` owns evidence and named primitives, `Lower` validates/translates bodies, and `Emit` owns their F# syntax and ABI templates.
3. **Code generation:** [Sharpurs.CodeGen.Selection](src/Sharpurs/CodeGen/Selection.purs) registers candidates and chooses binding/expression routes with explicit priorities and collision checks. [Sharpurs.CodeGen](src/Sharpurs/CodeGen.purs) translates these plans into [Sharpurs.FsAst](src/Sharpurs/FsAst.purs) values. [Sharpurs.CodeGen.Boxed](src/Sharpurs/CodeGen/Boxed.purs) owns the generic object-ABI source templates. Separate helpers recognize direct calls, instantiated constructors and integer operations.
4. **FFI and printing:** [Sharpurs.Ffi](src/Sharpurs/Ffi.purs) resolves foreign sources and chooses wrappers or missing-implementation stubs. [Sharpurs.FfiSupport](src/Sharpurs/FfiSupport.js) recognizes native declarations, plans their call shapes and renders boxed wrappers. [Sharpurs.Printer](src/Sharpurs/Printer.purs) prints F# declarations.
5. **Project generation:** [Sharpurs.Project](src/Sharpurs/Project.purs) writes the sources and ordered .NET projects into `output/Main/`. [Sharpurs.Runtime](src/Sharpurs/Runtime.purs) owns the shared F# prelude and entrypoint templates.

[Main](src/Main.purs) coordinates these phases and tracks the validated native constructor wrappers available to subsequent modules. See the [compiler maintenance guide](docs/compiler.md) for responsibilities, representation conventions and relevant checks.

The thunk kernel separates helper recognition (`Helpers`), signature/capture evidence (`Analysis`), optimized worker lowering (`Lower`), source-call proofs (`Call`) and F# templates (`Emit`). A selected worker is callable natively only when the source site also proves its seeds; opaque callbacks retain the public boxed path.

The CLI compares generated text with existing files before writing, preserving
timestamps when contents are unchanged. Its optimizer skip hook returns no cached
modules: every module must still register its native constructors and emitted
files. PBO can write `.purmeta/` metadata in the invoking workspace; the
[test runners](docs/testing.md#workspace-ownership) own its isolation or restoration.

## Current status and limitations

The compiler and native libraries remain experimental. The main areas still being developed are:

- Broader coverage of native representations and optimizations.
- A self-contained installation workflow without local development checkouts.
- Aff compatibility and process-lifetime accounting for pending asynchronous work.
- Further FFI coverage, compiler cleanup and compatibility validation.

### Asynchronous I/O and concurrency (Aff)

`sharpurs-aff` represents an asynchronous action as an F# workflow carrying cancellation and supervision state. Fibers use `TaskCompletionSource`, `Async.StartWithContinuations` and cancellation tokens; delays use `Task.Delay`, and parallel composition uses native asynchronous tasks. The package remains incomplete: for example, `_onCompleteFiber` is currently a no-op stub. Treat it as experimental rather than a fully compatible replacement for the JavaScript Aff runtime.

Use non-blocking .NET I/O APIs in foreign implementations and await them through the native async machinery. Starting an `Aff` does not turn blocking I/O into asynchronous I/O, and CPU parallelism depends on the operations and scheduling involved.

**Process lifetime follows the runtime's bookkeeping:** the generated entrypoint joins the thread invoking `main`, then calls `EventLoopWait`. The Aff package accounts for started fibers through `EventLoopAdd` and `EventLoopDone`. Native background work must participate in that accounting to keep the process alive; permanently registered work can also prevent it from exiting.

### Generated output

Old generated `.fs` and `.cs` sources may remain on disk, but projects include
only the current invocation's emitted files. The obsolete C# project is removed
when the last C# FFI disappears. After removing or renaming PureScript modules,
clear stale upstream TAST and rebuild the application's `output/` so removed
modules cannot remain compiler inputs. Keep maintained FFI and application-owned
projects outside that generated tree; use explicit
[project references](#net-dependencies). The
[output ownership contract](docs/compiler.md#project-outputs-and-filesystem-errors)
distinguishes upstream inputs, generated files and maintained application items.

Contributions to the compiler, regression coverage, FFI libraries and application examples are welcome.

## License

MIT License. See [LICENSE](LICENSE) for details.
