# Working on the compiler

## Follow a compilation

Start in [`src/Main.purs`](../src/Main.purs). Its measured phases follow the actual execution order:

1. **Load TAST + sort:** the optimizer reader loads `output/*/corefn.json` and sorts modules by dependency.
2. **Prepare:** write the runtime prelude, load optimization directives, collect constructor arities, and initialize the set of native constructor wrappers.
3. **Optimize + emit:** the optimizer's sequential builder calls `emitModule` for each module. It validates native ADT/thunk selections, registers constructor wrappers, translates declarations, loads foreign sources, and writes the module.
4. **Finalize:** write the entrypoint and .NET projects, preserving dependency order in F# compile items.

The input is the enriched typed CoreFn produced by the compiler fork. `Module Ann` is the source representation; `BackendModule` is the optimizer's representation. Code generation needs both: the source retains declarations and layout information, while optimized bindings can offer supported native implementations.

## Where changes belong

| Responsibility | Source |
| --- | --- |
| CLI arguments, phase order, optimizer callbacks, cross-module constructor state | [`Main.purs`](../src/Main.purs) |
| Foreign source lookup, F#/C# precedence, missing-implementation stubs | [`Sharpurs/Ffi.purs`](../src/Sharpurs/Ffi.purs) |
| Recognition of native declarations and wrapper text | [`Sharpurs/FfiSupport.js`](../src/Sharpurs/FfiSupport.js) |
| Generated file paths, write-if-changed behavior, MSBuild items and templates | [`Sharpurs/Project.purs`](../src/Sharpurs/Project.purs) |
| F# runtime helpers, event-loop bookkeeping, process entrypoint | [`Sharpurs/Runtime.purs`](../src/Sharpurs/Runtime.purs) |
| General expression/binding translation and native-path selection | [`Sharpurs/CodeGen.purs`](../src/Sharpurs/CodeGen.purs) |
| Translation contexts, recursive worker registration and name lookup | [`Sharpurs/CodeGen/Context.purs`](../src/Sharpurs/CodeGen/Context.purs) |
| Source patterns, newtype erasure and eligible constructor chains | [`Sharpurs/CodeGen/Pattern.purs`](../src/Sharpurs/CodeGen/Pattern.purs) |
| Case strategy selection, guards, constructor groups and fallback matches | [`Sharpurs/CodeGen/Case.purs`](../src/Sharpurs/CodeGen/Case.purs) |
| Boxed-ABI templates: closures, records, adapters, recursive groups | [`Sharpurs/CodeGen/Boxed.purs`](../src/Sharpurs/CodeGen/Boxed.purs) |
| F# AST and identifier/literal escaping | [`Sharpurs/FsAst.purs`](../src/Sharpurs/FsAst.purs), [`Sharpurs/FsAst.js`](../src/Sharpurs/FsAst.js) |
| Declaration and expression rendering | [`Sharpurs/Printer.purs`](../src/Sharpurs/Printer.purs) |
| Deferred recursive-group indentation and its marker definitions | [`Sharpurs/Printer/Layout.purs`](../src/Sharpurs/Printer/Layout.purs), [`Layout.js`](../src/Sharpurs/Printer/Layout.js) |
| Typed integer, ADT, direct-call, constructor and thunk subsets | The corresponding `Sharpurs/*Kernel`, `AdtLayout`, `DirectCall`, `ConstructorCall` and integer-operation modules |

`Ffi.loadModule` returns wrapper text and optional C# source. `Project.writeModule` owns the writes. This keeps foreign-source selection independent of output paths. Runtime templates are pure strings, also imported by the focused tests through the compiled `Sharpurs.Runtime` module.

`CodeGen` inspects the source AST and chooses implementations; `CodeGen.Boxed` accepts already translated `FsExpr` values and escaped target identifiers. It owns the F# fragments for the object ABI. Literal expressions and compound patterns are represented structurally in `FsAst` and rendered by `Printer`. `FsIdent` denotes an identifier; `FsRawExpr` explicitly marks an already-rendered expression supplied by a template or native kernel.

`CodeGen.Context.ModuleEnv` holds module-wide constructor information and native selections. Expression translation receives a `Context` combining that environment, the current module prefix and the visible recursive workers. Entering a recursive group extends this context; leaving it restores the enclosing context. Variable references, applications and local bindings have separate translation helpers in `CodeGen`, sharing name resolution through `Context.qualifiedName`.

For `ExprCase`, `CodeGen` supplies `Case.translate` with the pattern environment and an expression-translation callback. `Pattern` needs only constructor arities and the current module; it translates ordinary binders and recognizes unary chains with shallow leaves. `Case` selects the matching strategy before translating branch bodies. Its nested path uses named records for branches, constructor groups, catch-alls and match targets, including the distinction between an unboxed scrutinee and the boxed field captured by a variable.

## Conventions that affect correctness

- **The general ABI is boxed.** Values cross the generic path as F# `obj`; functions are curried, records use `Map<string, obj>`, and effects defer work in a thunk. Typed workers need the appropriate public wrappers at these boundaries.
- **Constructor arities and native wrappers are different facts.** All source constructor arities are collected up front. A constructor enters the native-wrapper set only after its producer's complete native layout and wrappers have been validated. Consumers can then choose native factories without losing the boxed public ABI.
- **Recursive scope is explicit.** Each recursive function records a positive arity, its escaped worker name and a `TopLevel` or `Local` scope. Top-level workers have a public curried alias; local workers need an adapter for partial calls and function-valued references. Saturated calls target the worker directly; extra arguments apply to its result. Zero-argument recursive values have no worker. This registry is distinct from constructor arities and relies on CoreFn's lexical binder renaming.
- **Deep unary patterns use nested matches.** F# compiles deeply nested active patterns very slowly. The nested path requires one scrutinee, unconditional alternatives, eligible patterns and a chain depth of at least four. Shallow non-chain leaves retain their ordinary patterns; their inspected constructor depth is limited to two. Newtype layers do not add depth. Guards, multiple scrutinees and unsupported shapes use the ordinary matcher.
- **Dependency order is observable.** The sequential builder makes validated producers available to consumers; the F# project lists their files in that same order. The current skip hook always returns `Nothing`.
- **F# owns wrappers when both native files exist.** The C# source is still copied and compiled for explicit use by F# helpers. If neither implementation exists, a curried failing stub delays the error until invocation at the known arity; unknown/non-function types retain the thunk fallback.
- **Direct-call rendering has two conventions.** `Printer.printExpr` unboxes `FsDirectApp` arguments and boxes the result at the declaration boundary. `Printer.printExprInline`, used by the boxed templates, passes arguments directly. Both share one renderer, with the convention propagated through applications, constructors, matches and guards. Choosing the wrong entrypoint can change F# type inference and boxing behavior.
- **Local recursive groups need a final layout pass.** `CodeGen.Boxed` emits markers from `Printer.Layout` for groups whose indentation depends on their enclosing expression. `Project.writeModule` calls `normalizeRecIndent` after assembling the whole source file. Marker definitions and decoding share one source; column accounting uses UTF-16 code units.
- **Function-call exceptions have a boundary.** `sharpurs_apply` preserves `TargetInvocationException` wrapping for both its fast path and reflection path. Direct-call wrappers must preserve the corresponding behavior.
- **Project writes are incremental.** Identical text keeps its timestamp. Generation does not delete stale files, and all `.cs` files remaining in `output/Main/` enter the C# project. `sharp.packages.props` supplies raw item elements for the F# project only.

## Checks for a change

Rebuild from the compiler root with the repository's tools first on `PATH`:

```bash
export PATH="$PWD/node_modules/.bin:$PATH"
spago build
spago bundle --module Main --platform node --outfile bin/sharpurs.js --bundle-type app
spago test
```

Use the focused commands listed in the [README](../README.md#development-and-testing) for the affected representation or runtime boundary. These scripts compile and execute F# and require `dotnet` on `PATH` (or `DOTNET` pointing to it). Their runtime helpers come from the rebuilt compiler output, so rebuild before running them.

`bin/test` exercises the complete CLI on vendored PureScript fixtures. Its `tests/runner` directory is shared: run fixture selections sequentially. The focused scripts use their own temporary directories.

For orchestration or template refactors, compare generated sources and project files against the previous compiler on the same typed input and FFI sources. This checks module order, wrapper selection, filenames and project references together. Content comparisons also distinguish a formatting-only change from a change to emitted code. Existing benchmark baselines require their own separate-process performance comparisons if optimization or runtime behavior changes.
