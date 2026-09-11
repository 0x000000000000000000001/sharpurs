# sharpurs

<img height="160" alt="sharpurs logo" src="https://github.com/user-attachments/assets/cfbf17c1-ada5-40ff-b804-e8f9cc75e328" />
<br />
<br />

_Experimental WIP. Typed code generation and the native FFI ecosystem are actively evolving._

An optimizing **PureScript-to-F# compiler**, written in PureScript, bringing PureScript's pure business logic to **.NET**, with its cross-platform runtime, threading facilities and library ecosystem. F# is the generated language; **C# is also supported for FFI implementations**.

`sharpurs` uses the enriched **TAST / Typed CoreFn (`tcorefn`)** produced by a [custom PureScript compiler](https://github.com/0x000000000000000000001/purescript). This retains structural types, ADT layouts, type-class declarations and type applications for native code generation. In the current toolchain, that typed payload is written to **`output/<Module>/corefn.json`**. The filename does not make it equivalent to the type-erased CoreFn emitted by the upstream compiler.

## Why F# and C# (.NET)?

F# provides functional programming, discriminated unions and pattern matching on the same runtime as C#. `sharpurs` aims to let PureScript developers keep their pure code and type system while using .NET for services, command-line tools and existing applications.

The backend generates an ordinary F# project that the .NET SDK can build and run. Native F# and C# bindings provide access to .NET APIs and NuGet libraries. Node.js runs the compiler; the generated application runs on .NET.

## Benchmarks

The [F#/C# results in altbak.pub](https://github.com/0x000000000000000000001/altbak.pub#fc) compare compiled PureScript with native F#/C# implementations on CPU and allocation workloads. The repository also records [extended workloads covering I/O, mutability and async](https://github.com/0x000000000000000000001/altbak.pub#extended-benchmark-results-io-mutability-async).

Use those documented baselines, workload definitions and execution conditions when assessing a change. Results vary substantially by algorithm and representation; an isolated timing does not establish a general advantage over V8 or another backend.

## Why a new .NET backend?

The ecosystem has evolved drastically, unlocking new architectural paradigms that make building a completely new .NET backend highly relevant today:

### 1. The optimizer & bootstrapping
While previous native compilers were often written in Haskell and parsed raw `CoreFn`, `sharpurs` is written 100% in PureScript. It integrates directly with a TAST-aware fork of [Arista's purescript-backend-optimizer](https://github.com/aristanetworks/purescript-backend-optimizer). This allows the compiler to instantly benefit from classical optimizations such as aggressive inlining, constant folding, and normalized function bodies at the AST level before .NET generation. The compiler itself runs on Node.js, ensuring it remains fully accessible.

### 2. A refined memory layout for .NET
`sharpurs` selects native representations for `.NET` where proven: integer loops, integer arithmetic, direct function and constructor calls, and typed ADT workers. Validated ADT layouts become F# discriminated unions with native fields, heavily reducing boxing and garbage collection overhead. While it preserves boxed interoperability for unsupported shapes, this selective unboxing massively improves hot-path performance.

### 3. TAST: Breaking the performance ceiling
To achieve native execution speeds, `sharpurs` consumes an enriched `tcorefn.json` (Typed CoreFn). This custom format preserves the deep structural typing information (`ann.type`), ADT layouts (`dataDecls`), type-class layouts (`classDecls`), and exact type applications (`TypeApp`) that standard `corefn` strips away. This metadata allows the compiler to generate idiomatic, structurally typed F# code end-to-end.

### 4. Zero boilerplate FFI
A unique strength of `sharpurs` is that it allows both F# and C# FFI in the same project. Companion `.fs` files can expose native F# functions, while companion `.cs` files can expose static C# methods. The backend automatically discovers these bindings and generates the necessary curried wrappers and boxing conversions needed by PureScript under the hood, making FFI development feel 100% native and type-safe.

### 5. Up-to-date with modern PureScript & .NET
`sharpurs` aims to be fully aligned with the current v0.15+ PureScript ecosystem and targets modern .NET 8 (`net8.0`). It natively leverages modern .NET SDK tooling, generating complete, ready-to-run `.fsproj` projects that integrate perfectly with the C# ecosystem and NuGet libraries.

### 6. Native Parallelism behind Aff
The [`sharpurs-aff`](https://github.com/0x000000000000000000001/sharpurs-aff) package maps PureScript's asynchronous monad (`Aff`) directly to F# `Async`, .NET tasks, and cancellation tokens. This connects PureScript asynchronous code to .NET's powerful scheduling and I/O facilities, bringing true multi-core scaling for free to CPU-bound parallel workloads.

## Prerequisites

- **Node.js and npm** to install and run the compiler.
- **Spago 0.93.x** on `PATH`; the optimizer checkout declares `^0.93.45`, and the package configurations use registry `77.10.1`.
- A **TAST-capable `purs`** from the custom compiler fork. The backend's npm development dependency uses the [`purescript-npm` wrapper](https://github.com/0x000000000000000000001/purescript-npm). Ensure application builds select this compiler too.
- The **.NET 8 SDK**, including F#, to build the generated `net8.0` projects. `dotnet` must be on `PATH`; installing only the runtime is insufficient for compilation.
- **Git** and the local compiler/FFI checkouts described below. The development scripts use Bash.

## Installation and local setup

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
source bin/pkg
for pkg in "${CORE_PACKAGES[@]}"; do
  if [ ! -d "../sharpurs-$pkg" ]; then
    git clone "https://github.com/0x000000000000000000001/sharpurs-$pkg.git" \
      "../sharpurs-$pkg" || break
  fi
done
```

With the prerequisites and local paths in place, install and build:

```bash
npm install
# After subsequent compiler changes:
npm run build
```

`npm install` runs `prepare`, which invokes `npm run build`. The build compiles the PureScript sources and bundles `Main` into `bin/sharpurs.js` for Node.js. The checked-in `bin/sharpurs` wrapper invokes that bundle with larger Node stack and heap limits.

There is currently no `bin/setup` or `flake.nix` in this repository. The clone steps above replace the old setup-script instructions.

## How to use

### Configure an application

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

### Build and run

From the application root, with the TAST compiler on `PATH`:

```bash
spago build
dotnet run --project output/Main/Program.fsproj -c Release
```

The configured backend reads the typed JSON from `output/` and writes a complete project to **`output/Main/`**, even when the selected entry module has another name:

| Generated file | Purpose |
| --- | --- |
| `Sharpurs_Prelude.fs` | Shared runtime helpers and event-loop bookkeeping. |
| `<Module>.fs` | Generated PureScript module and F# FFI wrappers, in dependency order in the project. |
| `EntryPoint.fs` | Calls the selected module's `main`. |
| `Program.fsproj` | Executable F# project targeting `net8.0`. |
| `<Module>.cs` and `FFI.CSharp.csproj` | C# sources and referenced library project, when C# FFI is present. |
| `Directory.Build.props` | Separate intermediate build directories for the F# and C# projects. |

To compile without running:

```bash
dotnet build output/Main/Program.fsproj -c Release
```

Run the generated project with `dotnet run`; the backend does not emit a standalone `main.fs` script for `dotnet fsi`.

### Compiler options

Invoke the backend from the application root after producing its typed `output/`:

```bash
../sharpurs/bin/sharpurs --main App.Main --ffi ffi
```

The equivalent backend arguments can be supplied through Spago:

```bash
spago build --backend-args "--main App.Main --ffi ffi"
```

| Option | Behavior |
| --- | --- |
| `--main <Module>` | Selects the module whose `main` is called; defaults to `Main`. It does not select modules for entrypoint-based dead-code elimination. |
| `--ffi <Directory>` | Adds a directory to the FFI search paths. A companion file beside the original `.purs` source takes precedence. |

The shared argument parser also recognizes `--bundle`, `--output`, `--rewrite-limit` and `--autoload-path`, but this backend does not use them. Input is fixed to `output/`, generated files go to `output/Main/`, and the optimizer rewrite limit is currently `10000`. There is no automatic discovery of every module exporting `main`.

## Writing FFI

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

Use normal NuGet and MSBuild project references for native dependencies. The generated project files are rewritten by each backend run, so maintain application-specific dependency configuration outside those generated files, or apply it as a repeatable post-generation step. C# FFI dependencies belong to the C# project as well when they are used there.

## Asynchronous I/O and concurrency (Aff)

`sharpurs-aff` represents an asynchronous action as an F# workflow carrying cancellation and supervision state. Fibers use `TaskCompletionSource`, `Async.StartWithContinuations` and cancellation tokens; delays use `Task.Delay`, and parallel composition uses native asynchronous tasks.

Use non-blocking .NET I/O APIs in foreign implementations and await them through the native async machinery. Starting an `Aff` does not turn blocking I/O into asynchronous I/O, and CPU parallelism depends on the operations and scheduling involved.

**Process lifetime is still an integration constraint:** the generated runtime exposes `EventLoopAdd`, `EventLoopDone` and `EventLoopWait`, and the Aff package accounts for started fibers. However, the current `EntryPoint.fs` generator only starts and joins the thread that invokes `main`; it does not call `EventLoopWait`. A host running an application that launches background fibers must arrange to wait for their completion. Automatic draining of all pending Aff work is not currently guaranteed by the generated entrypoint.

## Local development and testing

The checked-in [bin/pkg](bin/pkg) is the authoritative list of sibling packages required by [bin/test](bin/test). The runner creates its Spago configuration on first use, copies fixtures into `tests/runner/src`, compiles them, generates F#, and executes the generated project in Release mode.

From the compiler root:

```bash
# Run the vendored passing-test suite using the current backend bundle.
./bin/test

# Rebuild the backend, clean the runner caches, and run one fixture.
./bin/test TCO -c
```

The runner skips five fixtures requiring newer compiler features and stops at the first failure. Its result applies to this vendored suite and the selected toolchain; it is not a claim that every current upstream PureScript test or library is supported. The `-c` / `--clean` option clears the runner's `.spago`, `output` and `output-es` contents.

Focused regression commands are defined in [package.json](package.json):

| Commands | Coverage |
| --- | --- |
| `npm run test:runtime` | Generic function application, FFI wrappers and exception boundaries. |
| `npm run test:kernel`, `npm run test:local-kernel` | Native integer loops and locally nested kernels. |
| `npm run test:adt-kernel`, `npm run test:adt-interop` | Typed ADT generation and boxed/native boundaries. |
| `npm run test:adt-unary`, `npm run test:adt-multi` | Native recursive ADT workers and multiple arguments. |
| `npm run test:int-comparison`, `npm run test:int-arithmetic` | Integer comparison, overflow, division and modulo behavior. |
| `npm run test:direct-call` | Saturated calls and preservation of the generic fallback. |
| `npm run test:thunk-kernel` | Selected typed thunk fusion and its exclusion cases. |
| `npm run test:constructor-typeapp` | Constructor calls with explicit and inferred type applications. |

Build the compiler first for tests importing its `output/` modules. The runtime test reads source helpers directly and additionally needs `sharpurs-exceptions`. Tests that compile fixtures use the TAST `purs` and run generated F# through `dotnet fsi`; `PURS=/path/to/purs` and `DOTNET=/path/to/dotnet` select those executables in the focused scripts. The shell runner instead uses `purs` and `dotnet` through `PATH`.

## Current status and milestones

- [x] PureScript-to-F# code generation and executable .NET project generation.
- [x] F# and C# foreign implementations, including mixed modules.
- [x] TAST-aware integer, ADT, call and thunk optimizations with boxed interoperability.
- [x] A vendored PureScript passing-test runner and focused runtime/code-generation regressions.
- [x] An Aff implementation based on .NET async facilities and an alternate-library ecosystem.
- [x] Integration with the `altbak.pub` multi-runtime benchmarks.
- [ ] Broader coverage of native representations and optimizations.
- [ ] A self-contained installation workflow without local development checkouts.
- [ ] Reliable automatic waiting for pending Aff work in the generated entrypoint.
- [ ] Further FFI coverage, compiler cleanup and compatibility validation.

The project remains experimental. Contributions to the compiler, regression coverage, FFI libraries and application examples are welcome.

## Architecture

The main parts of the compilation pipeline are:

1. **Typed input:** the custom `purs` compiler emits enriched `corefn.json` files. The TAST-aware optimizer reader decodes them and sorts modules by dependencies.
2. **Optimization and selection:** the optimizer prepares `BackendModule` values. `Sharpurs.IntKernel`, `Sharpurs.AdtKernel` and `Sharpurs.ThunkKernel` select supported typed transformations while retaining access to the source AST.
3. **Code generation:** [Sharpurs.CodeGen](src/Sharpurs/CodeGen.purs) combines those selections with the general generator and produces [Sharpurs.FsAst](src/Sharpurs/FsAst.purs) values. Separate helpers recognize direct calls, instantiated constructors and integer operations.
4. **FFI and printing:** [Sharpurs.FfiSupport](src/Sharpurs/FfiSupport.js) emits foreign wrappers; [Sharpurs.Printer](src/Sharpurs/Printer.purs) prints F# declarations.
5. **Project generation:** [Main](src/Main.purs) writes the runtime, modules, entrypoint and .NET projects into `output/Main/`.

The CLI compares generated text with existing files before writing, preserving timestamps when contents are unchanged. It currently returns no cached modules from the optimizer's skip hook and does not read or write an optimization cache. The earlier `.sharpurs-cache.json` description does not describe the current implementation.

## License

MIT License. See [LICENSE](LICENSE) for details.
