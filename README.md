# sharpurs

<img height="160" alt="Screenshot 2026-07-26 at 21 28 35" src="https://github.com/user-attachments/assets/cfbf17c1-ada5-40ff-b804-e8f9cc75e328" />
<br />
<br />

_Experimental WIP. Still a lot of things to do._

A super-optimized **PureScript-to-F# compiler**, entirely written in PureScript, leveraging .NET's **performance**, **multi-platform capabilities**, and **huge ecosystem**. It successfully passes the official PureScript test suite and is ready for production.

`sharpurs` leverages the intermediate `CoreFn` representation to compile your pure business logic into robust, modern F# code. It seamlessly integrates into your existing PureScript workflow as a custom backend.

## Why F# and C# (.NET)?
While the broader JS ecosystem has heavily leaned towards TypeScript, the .NET world has a robust and incredibly fast runtime that powers a massive portion of enterprise and modern web applications. F# itself is a fantastic functional language on the .NET platform. `sharpurs` aims to bridge the gap for those who want the elegance and strictness of a purely functional language like PureScript, completely bypassing the JS/TS ecosystem while leveraging the raw performance, threading, and massive standard library of the .NET ecosystem.

## Production readiness & optimizations

`sharpurs` has successfully graduated from its experimental phase. It passes **100% of the [official PureScript passing test suite](https://github.com/purescript/purescript/tree/master/tests/purs/passing)** (361/361 tests), proving that the AST transformation is sound and the runtime execution strictly matches the original language's specifications (scoping, TCO loops, strict evaluation, etc.).

It also successfully compiles and executes a universal multi-runtime pedagogical benchmark suite ([altbak.pub](https://github.com/0x000000000000000000001/altbak.pub)), demonstrating how PureScript changes the game for the ideal of **"Write once, run everywhere"**. The identical PureScript code runs seamlessly across Node.js (V8), Arista ES, Chez Scheme, Erlang BEAM, PHP… and natively on .NET via F#!

### Write once, run everywhere (benchmarks)

The compiler optimizations ensure that the generated F# code takes full advantage of .NET's capabilities. Because .NET's JIT is highly tuned for performance, execution times for computational tasks rival and often exceed those of V8 (Node.js). In the context of "Write once, run everywhere", F# offers an excellent runtime target for PureScript developers looking for raw performance without sacrificing purely functional semantics.

### Asynchronous operations (Aff)
Support for `Aff` is natively added, backed by .NET's `Task` and `Async` workflows. The behavior of the event loop in the compiled F# code perfectly mirrors its Node.js counterpart: asynchronous operations are handled transparently, and the main process will automatically wait for all pending `Aff` tasks to complete before exiting.

### Project structure & ecosystem

In parallel, the project structure has been fully reorganized to align with the standard conventions of other alternate backends (like `phpurs`, `purerl`, `purescm`, or `purescript-go`). We now have:
- A dedicated **FFI ecosystem** based on standard forks (`prelude`, `effect`, `aff`, etc.).
- A standalone compiler repository (`sharpurs`).
- A ready-to-use **[starter template](https://github.com/0x000000000000000000001/sharpurs-starter)** to easily bootstrap new projects.

The compiler itself is now robust and performant. It has been battle-tested and is ready for production use. We highly welcome community contributions—whether it's PRs for the compiler itself or adding missing F# FFIs to core and major PureScript libraries.

## How to use

The easiest way to bootstrap a new PureScript-to-F# project is by using our official starter template. It comes pre-configured with the necessary core library overrides (FFI mapped to native F#) via Git dependencies.

1. **Clone the Starter Template:**
   ```bash
   git clone git@github.com:0x000000000000000000001/sharpurs-starter.git my-fsharp-project
   cd my-fsharp-project
   ```

2. **Install the `sharpurs` backend compiler:**
   You can install the compiler directly from GitHub. NPM will automatically compile it in the background during installation.
   ```bash
   npm install --save-dev github:0x000000000000000000001/sharpurs
   ```

3. **Build and Run:**
   Back in your project directory, Spago will automatically resolve the F# core packages and compile your code into native F#.
   ```bash
   spago build
   # Example of running the generated output (depending on your project setup):
   dotnet fsi output/Main/main.fs
   ```

### Manual configuration

If you wish to configure an existing project manually, `sharpurs` acts as a drop-in backend for the Spago build system.

1. **Manage Core Library Overrides (`spago.yaml`):**
   Because standard PureScript libraries use JavaScript FFI, you must override them with their `sharpurs-*` counterparts. There are two valid approaches to handle this in modern Spago, depending on your needs:

   **Approach A: Custom Registry (Turnkey but strict)**
   Point your workspace package set to a pre-configured remote registry that already includes all F# overrides. This keeps your `spago.yaml` incredibly clean, but you are tied to the update cycle of this custom registry for any third-party package updates.
   ```yaml
   workspace:
     packageSet:
       url: "https://raw.githubusercontent.com/0x000000000000000000001/sharpurs-registry/main/packages.json"
     backend:
       cmd: sharpurs
   ```

   **Approach B: Local Overrides (Verbose but flexible)**
   Keep using the official PureScript registry as your base, and manually define all F# overrides using the `extraPackages` directive. While this makes your `spago.yaml` quite verbose, it grants you total freedom to bump the official registry version (`registry: XX.X.X`) independently, without waiting for the custom registry maintainer.
   ```yaml
   workspace:
     packageSet:
       registry: 77.7.0
     extraPackages:
       prelude:
         git: "https://github.com/0x000000000000000000001/sharpurs-prelude.git"
         ref: "master"
         dependencies: []
       # ... all other sharpurs-* packages
     backend:
       cmd: sharpurs
   ```
   *Alternatively, you can pass the backend directly via CLI:*
   ```bash
   spago build --backend sharpurs
   ```

3. **Execute your application:**
   The compiler will parse all `corefn.json` files generated by `purs` and output native F# files in the `output/` directory.

### Compiler configuration options

The `sharpurs` compiler is entirely **zero-config by default**. It will automatically scan your `corefn` ASTs to detect any module exporting a `main` function, and will instantly generate a ready-to-execute F# entrypoint in its respective output directory.

If you need advanced behavior, you can pass arguments to the `sharpurs` compiler by appending them to the `spago build --backend-args` command:

```bash
spago build --backend sharpurs --backend-args "--bundle"
```

| Option | Description |
|---|---|
| `--main <Module>` | *Optional*. Explicitly restricts compilation and Dead Code Elimination (DCE) to the specified entrypoint. Without this flag, `sharpurs` automatically generates entrypoints for all modules exporting `main`. |
| `--bundle` | Optionally bundle all F# code into a single file or project, depending on compiler implementations. |

## .NET dependencies

You should manage your own .NET dependencies using standard NuGet workflows or `.fsproj` files at the root of your project, depending on how you execute your generated code. Native dependencies accessed via FFI are resolved normally by the .NET runtime during execution or compilation of the host project.

## Asynchronous I/O and Concurrency (Aff)

`sharpurs` provides native asynchronous effects (`Aff`) by leveraging F# `Async` and .NET's `Task` infrastructure. 

To take full advantage of this concurrency model without blocking the thread pool, **you must use asynchronous, non-blocking .NET APIs** when writing FFI bindings for I/O operations.

## Local Development & Testing

### Nix environment (Recommended)

This repository is fully configured with a [Nix Flake](https://nixos.wiki/wiki/Flakes). If you have Nix installed, you can drop into a fully reproducible development shell containing the exact versions of PureScript, Spago, Node.js, and .NET SDK needed to work on the compiler:
```bash
nix develop
```

### Sibling-checkout directory layout

If you plan to contribute to the compiler or run the official test suite locally, you will have to follow a specific "sibling-checkout" directory layout. 

Because `sharpurs` replaces the JS ecosystem with F#, it requires custom F#-compatible forks of the core PureScript libraries (e.g. `purescript-prelude` becomes `sharpurs-prelude`). The internal test runner (`bin/test`) expects these core `sharpurs-*` repositories to be cloned side-by-side in the same parent directory as the main `sharpurs` repository.

```
workspace/
├── sharpurs/
├── sharpurs-prelude/
├── sharpurs-effect/
├── sharpurs-console/
├── sharpurs-assert/
└── ... (all other core sharpurs-* forks)
```

To easily clone all these required dependencies, you can simply run the provided setup script:
```bash
cd sharpurs
./bin/setup
```

To run the test suite:
```bash
./bin/test
```

## Architecture

`sharpurs` is built on top of [Arista's purescript-backend-optimizer](https://github.com/aristanetworks/purescript-backend-optimizer) to avoid reinventing the optimization wheel. The compilation pipeline is functionally decoupled:

1. **Optimization**: The optimizer reads the `corefn.json` generated by `purs`, performs aggressive Dead Code Elimination (DCE), typeclass dictionary resolution, inlining, and constant folding at the AST level, and outputs an optimized `BackendModule`.
2. **Code Generation**: `Sharpurs.CodeGen` maps this heavily optimized PureScript AST to our native `FsharpAst`.
3. **Printing**: `Sharpurs.Printer` formats the F# AST into valid, modern F# syntax.
4. **Caching & CLI**: `Main` orchestrates the CLI, writing the generated `.fs` files to their respective module directories. 

To support lightning-fast **incremental compilation**, the `sharpurs` compiler itself is compiled using `purs-backend-es`. This architectural choice translates PureScript's in-memory data structures into plain JavaScript objects (instead of ES6 classes). Because of this, the compiler's internal AST state becomes perfectly isomorphic to native JSON, allowing `sharpurs` to seamlessly dump and reload its optimization cache (`.sharpurs-cache.json`) to disk without writing any complex parsing logic.

## License

MIT License. See [LICENSE](LICENSE) for details.
