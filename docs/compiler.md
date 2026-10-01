# Working on the compiler

## Follow a compilation

Start in [`src/Main.purs`](../src/Main.purs). Its measured phases follow the actual execution order:

1. **Load TAST + sort:** the optimizer reader loads `output/*/corefn.json` and sorts modules by dependency.
2. **Prepare:** write the runtime prelude, load optimization directives, collect constructor arities, and initialize the set of native constructor wrappers.
3. **Optimize + emit:** the optimizer's sequential builder calls `emitModule` for each module. It validates native ADT/thunk selections and registers constructor wrappers. `CodeGen.Selection` collects the remaining candidates and plans binding/expression routes; `CodeGen` translates the selected plans. The callback then loads foreign sources and writes the module.
4. **Finalize:** write the entrypoint and .NET projects, preserving dependency order in F# compile items.

The input is the enriched typed CoreFn produced by the compiler fork. `Module Ann` is the source representation; `BackendModule` is the optimizer's representation. Code generation needs both: the source retains declarations and layout information, while optimized bindings can offer supported native implementations.

## Where changes belong

| Responsibility | Source |
| --- | --- |
| CLI arguments, phase order, optimizer callbacks, cross-module constructor state | [`Main.purs`](../src/Main.purs) |
| Foreign source lookup, F#/C# precedence, missing-implementation stubs | [`Sharpurs/Ffi.purs`](../src/Sharpurs/Ffi.purs) |
| Native declaration recognition, call shapes and boxed-wrapper templates | [`Sharpurs/FfiSupport.js`](../src/Sharpurs/FfiSupport.js), [`FfiSupport.purs`](../src/Sharpurs/FfiSupport.purs) |
| Generated file paths, write-if-changed behavior, MSBuild items and templates | [`Sharpurs/Project.purs`](../src/Sharpurs/Project.purs) |
| F# runtime helpers, event-loop bookkeeping, process entrypoint | [`Sharpurs/Runtime.purs`](../src/Sharpurs/Runtime.purs) |
| General expression/binding translation and emission of selected plans | [`Sharpurs/CodeGen.purs`](../src/Sharpurs/CodeGen.purs) |
| Candidate registration, direct-helper collisions and implementation priority | [`Sharpurs/CodeGen/Selection.purs`](../src/Sharpurs/CodeGen/Selection.purs) |
| Translation contexts, recursive worker registration and lookup | [`Sharpurs/CodeGen/Context.purs`](../src/Sharpurs/CodeGen/Context.purs) |
| Source TAST annotations, lambda/application spines and expression traversal | [`Sharpurs/Analysis/Source.purs`](../src/Sharpurs/Analysis/Source.purs) |
| Qualified F# value names | [`Sharpurs/Names.purs`](../src/Sharpurs/Names.purs) |
| Source patterns, newtype erasure and eligible constructor chains | [`Sharpurs/CodeGen/Pattern.purs`](../src/Sharpurs/CodeGen/Pattern.purs) |
| Case strategy selection, guards, constructor groups and fallback matches | [`Sharpurs/CodeGen/Case.purs`](../src/Sharpurs/CodeGen/Case.purs) |
| Boxed-ABI templates: closures, records, adapters, recursive groups | [`Sharpurs/CodeGen/Boxed.purs`](../src/Sharpurs/CodeGen/Boxed.purs) |
| F# AST and identifier/literal escaping | [`Sharpurs/FsAst.purs`](../src/Sharpurs/FsAst.purs), [`Sharpurs/FsAst.js`](../src/Sharpurs/FsAst.js) |
| Declaration and expression rendering | [`Sharpurs/Printer.purs`](../src/Sharpurs/Printer.purs) |
| Deferred recursive-group indentation and its marker definitions | [`Sharpurs/Printer/Layout.purs`](../src/Sharpurs/Printer/Layout.purs), [`Layout.js`](../src/Sharpurs/Printer/Layout.js) |
| Closed ADT layouts, constructor metadata and native type names | [`Sharpurs/AdtLayout.purs`](../src/Sharpurs/AdtLayout.purs) |
| ADT module admission, binding selection/order and generated-name collisions | [`Sharpurs/AdtKernel.purs`](../src/Sharpurs/AdtKernel.purs) |
| ADT signatures, source dependencies and type/constructor evidence | [`Sharpurs/AdtKernel/Analysis.purs`](../src/Sharpurs/AdtKernel/Analysis.purs) |
| Optimized ADT bodies, nested type checks and native call targets | [`Sharpurs/AdtKernel/Lower.purs`](../src/Sharpurs/AdtKernel/Lower.purs) |
| ADT definition headers, public wrappers, guarded calls and recursive bridges | [`Sharpurs/AdtKernel/Emit.purs`](../src/Sharpurs/AdtKernel/Emit.purs) |
| Whole-native ADT producers and cross-module consumer checks | [`Sharpurs/AdtInterop.purs`](../src/Sharpurs/AdtInterop.purs) |
| Thunk worker selection, recursion-group agreement and name collisions | [`Sharpurs/ThunkKernel.purs`](../src/Sharpurs/ThunkKernel.purs) |
| Thunk signatures, parameter evidence, capture plans and reserved names | [`Sharpurs/ThunkKernel/Analysis.purs`](../src/Sharpurs/ThunkKernel/Analysis.purs) |
| Identity/force helper implementations, aliases and typed references | [`Sharpurs/ThunkKernel/Helpers.purs`](../src/Sharpurs/ThunkKernel/Helpers.purs) |
| Optimized thunk worker bodies and lexical-level checks | [`Sharpurs/ThunkKernel/Lower.purs`](../src/Sharpurs/ThunkKernel/Lower.purs) |
| Source force-call eligibility, closed seeds and capture-value resolution | [`Sharpurs/ThunkKernel/Call.purs`](../src/Sharpurs/ThunkKernel/Call.purs) |
| Native thunk definitions, closures, captures and boxed force results | [`Sharpurs/ThunkKernel/Emit.purs`](../src/Sharpurs/ThunkKernel/Emit.purs) |
| Typed integer, direct-call and constructor subsets | `IntKernel`, `Optimized`, `DirectCall`, `ConstructorCall` and integer-operation modules |

`Ffi.loadModule` returns wrapper text and optional C# source. `Project.writeModule` owns the writes. This keeps foreign-source selection independent of output paths. Runtime templates are pure strings, also imported by the focused tests through the compiled `Sharpurs.Runtime` module.

`CodeGen.Selection` chooses implementations and `CodeGen` translates their plans and the remaining source AST. `CodeGen.Boxed` accepts already translated `FsExpr` values and escaped target identifiers. It owns the F# fragments for the object ABI. Literal expressions and compound patterns are represented structurally in `FsAst` and rendered by `Printer`. `FsIdent` denotes an identifier; `FsRawExpr` explicitly marks an already-rendered expression supplied by a template or native kernel.

`CodeGen.Context.ModuleEnv` holds module-wide constructor information and native selections. Expression translation receives a `Context` combining that environment, the current module prefix and the visible recursive workers. Entering a recursive group extends this context; leaving it restores the enclosing context. Variable references, applications and local bindings have separate translation helpers in `CodeGen`, sharing name qualification through `Names.qualified`.

For `ExprCase`, `CodeGen` supplies `Case.translate` with the pattern environment and an expression-translation callback. `Pattern` needs only constructor arities and the current module; it translates ordinary binders and recognizes unary chains with shallow leaves. `Case` selects the matching strategy before translating branch bodies. Its nested path uses named records for branches, constructor groups, catch-alls and match targets, including the distinction between an unboxed scrutinee and the boxed field captured by a variable.

## Selecting implementations

Start in `CodeGen.Selection` when changing precedence or deciding whether a recognized candidate may replace source code:

1. `fromBackend` collects `BindingCandidates`: native Int kernels and complete optimized expressions containing local kernels. The ADT/thunk recognizers supply their validated selections through `ModuleEnv`.
2. `prepareModule` reserves direct-call entries and returns a `ModulePlan` containing the selected environment and one `BindingPlan` per source group. Its registration checks use the same `replacement` decision as `forBinding`, so a suppressed direct helper cannot remain callable.
3. `CodeGen.translateBindingPlan` emits the chosen implementation. Each recognizer remains responsible for type/layout/body eligibility; this policy handles precedence and source group boundaries.

Binding routes are tried in this order:

| Priority | Route | `NonRec` | Singleton `Rec` | Mutual `Rec` |
| --- | --- | --- | --- | --- |
| 1 | Validated native ADT declaration | Eligible | Eligible | Whole-group fallback |
| 2 | Native Int kernel | Eligible | Eligible | Whole-group fallback |
| 3 | Complete optimized expression | Eligible | Fallback | Whole-group fallback |
| 4 | Registered direct function | Eligible | Fallback | Whole-group fallback |
| 5 | Generic source translation | Fallback | Fallback | Original group retained |

A recognized member never splits a mutual source group: its generic peers can depend on the group's `_tco` entry points. An optimization map entry without a corresponding source binding emits nothing.

Direct registration considers the escaped names of **all** source bindings and foreign declarations. Either generated suffix, `_direct` or `_direct_apply`, colliding with those names or with the candidate's own parameters keeps the generic function. Keys retain the source module qualifier, so local shadows and imported homonyms cannot use the module's helper. ADT/thunk-specific collision and dependency validation remains in those recognizers.

Expression selection has its own entry point, `forExpression`, with this priority: **native thunk → registered direct invocation → Int comparison → Int arithmetic → instantiated constructor → generic expression**. `ExpressionPlan` keeps source operands until `CodeGen` translates the chosen route. An unsupported recognized integer operator falls through to constructor/generic handling, preserving the existing fallback boundary. Native thunk declarations supplement the original public bindings; they are not another whole-binding replacement tier.

`CodeGen` explicitly exports the module and standalone-binding entry points used by the CLI, interop and tests. `translateModuleUsing env { kernels, expressions } source` accepts named candidate maps and applies the complete module policy; the convenience entry points delegate to it. Standalone binding translation does not invent a module-wide direct-call registry.

`npm run test:selection` checks competing candidate sets through production registration and emission, including singleton/mutual groups, helper suppression, escaped-name/foreign/parameter collisions and qualified call-site lookup. Recognizer suites additionally execute the emitted implementations.

## Inside the ADT kernel

`AdtKernel.prepareModule` returns an `AdtModule` for selective native generation. Both names cover functions with multiple arguments. Its preparation sequence is:

1. **Admit the layout and constructors.** `AdtLayout.fromModule` validates a closed representation using source `dataDecls`: native Int/Boolean fields and local monomorphic ADTs. `AdtKernel` checks source/optimized module agreement, excludes foreign/class declarations, and validates every constructor implementation and public wrapper.
2. **Preserve source dependencies.** `Analysis.unsupportedBindings` finds noncanonical imports and propagates that exclusion through local dependants. This evidence survives optimizer inlining, including inlined partial helpers whose invocation boundary is observable. Constructor signatures become callable when their binding is visited in source order.
3. **Select functions in source order.** Source and optimized recursion groups must agree and each selected function must be a singleton. Its complete source lambda spine must match the optimized signature, with at least one argument whose ADT has a constructor containing that same ADT. Calls can use previously admitted definitions and the current recursive function. A rejected function does not enter that registry.
4. **Lower a supported body.** `Lower.binding` collects parameters and translates the optimized expression, checking nested annotations, distinct/nonnegative lexical levels, exact application saturation, constructor metadata and primitive operand types. `Analysis.typeOf` supplies an expected type for a let RHS; lowering still validates the complete RHS. Unsupported forms, including unresolved type applications, reject the binding.
5. **Emit and register.** `Emit` receives a lowered definition with named `parameters` and `body` fields. It produces the native header and object-ABI boundaries. Module admission also checks generated names and requires at least one selected function. `AdtModule.bindings` contains constructors and selected functions; `nativeNames` lists functions only.

The emitted boundaries have distinct roles:

| Entry | Purpose |
| --- | --- |
| `name_adt_native` | Typed constructor/function definition. Values and ADT fields pass directly, preserving sharing. |
| Public `name` | Curried object wrapper; partially applied closures retain their arguments. The final invocation unboxes arguments and boxes the result. |
| `name_adt_native_apply` | Guarded call between native functions in a mixed module, preserving one `TargetInvocationException` boundary. Arguments evaluate before entry; constructors and self-recursion use the unguarded definition. |
| `name_tco` | Object-argument bridge for generic callers of a selected recursive function. |

`AdtKernel.fromModule` is the separate all-or-nothing path used by `AdtInterop.prepareProducer`. It requires every optimized binding to have a supported native signature and body, supports whole mutually recursive function groups, and emits unguarded internal calls plus public wrappers. `AdtInterop` then checks producer completeness, module/name collisions and constructor arities before exposing factories to a boxed consumer. The CLI's `prepareModule` path supplies its constructor registry through `Main` and can retain generic source bindings alongside native functions.

The ADT kernel, interop, unary and multi-argument suites cover layout/type rejection, public and partial applications, sharing, short-circuiting, source dependencies and exception boundaries. The constructor-type-application suite covers factories consumed through instantiated constructors.

## Inside the thunk kernel

Thunk selection has two proof boundaries: admitting a private worker and admitting a source call to that worker. `ThunkKernel.prepareModule` returns a `ThunkModule` containing declarations, worker names and helper/worker registries. `ThunkKernel.fromExpr` delegates source-call selection to `Call` using those registries. The production path is:

1. **Recognize helpers in source order.** `Helpers.select` checks nonrecursive singleton bindings for identity/delay and force implementations. Source and optimized signatures must each have the helper shape. The optimized body must return its callback unchanged or apply it exactly once to `Data.Unit.unit`; nested annotations are checked. Source references may use lexical locals, unit and previously recognized helpers. An alias requires the same variable reference in both representations. The erased newtype identity has one explicit exception for its unannotated source body.
2. **Prove worker signatures and dependencies.** Source/optimized recursion groups must agree; workers are singletons, with optional self-recursion. `Analysis.worker` uses the actual source lambda spine as arity, requires the remaining flattened signature to return `Unit -> Int`, and requires at least one thunk parameter. Native parameter types are Int, Boolean, Unit and `Unit -> Int`. Source references are limited to lexical locals, self, recognized helpers, unit and the supported canonical integer operations. Source evidence prevents optimizer inlining from hiding an opaque or partial helper.
3. **Lower optimized bodies.** `Lower.binding` receives a `Worker`, collects a named `Parameters` result and returns an `Emit.Definition` with `parameters` and `body`. It validates nested annotations, distinct/nonnegative parameter levels, lexical scope, exact saturation and each supported operation. Local callbacks, self-calls and recognized helpers are the only callable targets. A returned Unit lambda stays delayed; failures, foreign calls and unsupported callback shapes reject the worker.
4. **Emit private workers.** `Emit.worker` renders `let private` or `let rec private` declarations. Selection protects generated names against public/foreign names and source-local binders. `CodeGen` adds these declarations before the ordinary source bindings; the thunk kernel does not replace their public curried ABI.
5. **Prove the source force call.** `Call.fromExpr` requires a recognized force helper returning Int. Its thunk must come from a supported lambda, a recognized identity helper, or an exactly saturated selected worker. Every application suffix is type-checked. Source worker arguments are restricted further to Int expressions and proven thunks; a callback variable with type `Unit -> Int` is not provenance evidence. Helper references alone may use explicit `TypeApp Int` with a matching one-variable polymorphic signature.

`Analysis.planCaptures` returns a named `CapturePlan`: Int captures in first-reference order, fresh native names and the extended local-name map. It reserves source reference names, local binders and existing native locals. `Call` resolves capture values in the original environment before translating the body with the new aliases; this prevents an early capture from shadowing a later one. `Emit.capturedThunk` evaluates those Int captures when constructing the closure, then delays its body. The accepted source Int subset consists of literals, lexical Int values and canonical addition/subtraction.

The boundaries that preserve behavior are:

| Boundary | Contract |
| --- | --- |
| Public value and partial application | The boxed curried ABI remains available, including escaping thunks and reused partial closures. Native declarations are supplementary. |
| Closure construction | Captures are resolved once per constructed closure; separate seeds and reused partial applications receive their own values. |
| Force | The body executes on every force. No memoization or eager evaluation of an opaque callback is introduced. |
| Exception behavior | Native routing requires both worker and seed proofs. Unknown callbacks, partial helpers and foreign effects retain the generic application path and its invocation wrappers. |

`Analysis.strip` and optimized `Lower.typeOf` inspect shapes/types only; validation still occurs when lowering those nodes. Capture traversal similarly does not authorize unsupported body syntax. These distinctions keep the source TAST's provenance evidence separate from the optimized IR's implementation evidence.

`npm run test:thunk-kernel` compares generated F# with the generic compiler path and JavaScript. It covers independent seeds, multiple captures with generated-name collisions, reusable partials, delayed/opaque/throwing callbacks, source/optimized helper contradictions and the million-thunk result. Set `THUNK_KERNEL_ARTIFACTS` to retain generated code, typed input, source hashes and execution logs.

## Inside the FFI adapters

`Ffi.loadModule` resolves implementations and returns a `ModuleFfi` containing F# wrapper text and optional C# source. It asks the optimizer's `findFfiFile` for `.fs` and `.cs` independently: a file beside the source `.purs` takes precedence, followed by dependency/override directories, the configured FFI directory and the local directory. `FfiSupport.purs` exposes two pure text adapters; `Project` owns the generated files and project references.

The adapter pipeline in `FfiSupport.js` has three explicit stages:

1. **Recognize a declaration.** `recognizeFsharp` and `recognizeCsharp` return a `Declaration` with `nativeName` and `parameterText`, or no match. F# source first passes through `prepareFsharpSource`, which removes a simple `module Name` header and nests the indented implementation under `Module_Name_FFI`.
2. **Determine the call shape.** `fsharpArity`/`csharpArity` interpret the parameter text. `fsharpCall`/`csharpCall` produce a named `CallShape` with the public export name, native target, arity and `curried`/`method` convention. Public names flatten module dots to underscores and retain the requested foreign name.
3. **Render the wrapper.** `renderWrapper` uses only that call shape. Each argument introduces a boxed `obj -> obj` closure. The final application unboxes the arguments and boxes the native result. F# applications use separate curried arguments; C# methods use one comma-separated argument list. Requested export order and generated whitespace are preserved.

The text recognizers retain these contracts:

| Source form | Recognition and call shape |
| --- | --- |
| F# `let name ... =` | Case-sensitive, line-start match, with optional `rec` and double-backtick name. A name boundary prevents matching a longer identifier. The first matching declaration supplies the parameter text. |
| F# parameters | Parenthesized groups count as one argument, including a tuple or typed parameter; `()` is also one argument. Remaining whitespace-separated parameters are counted until a type annotation. Multiline parameter text is supported. |
| F# value or function-valued binding | `let value = ...` and `let value : ... = ...` have arity zero. The wrapper boxes the named value directly, including a function stored in a value binding. |
| C# `public static ... Name(...)` | Case-insensitive method-name match; the captured native casing is used in the call. The return-type pattern supports a single token with identifier, array/generic delimiters and optional nullable suffix. The implementation is expected under `Module.Name.FFI`. |
| C# parameters | A single-line parameter list is counted by commas. Empty parentheses mean arity zero, and the wrapper invokes the method during initialization. |

These are restricted textual forms: F# modifiers such as `private`/`inline`, C# multiline parameter lists, nested comma-containing parameter types and other richer syntax have no full-language parsing contract. When a required declaration is not recognized, the adapter retains its zero-arity behavior: F# references the requested value; C# invokes the requested method name. This is distinct from the missing-file policy.

File selection and missing implementations belong to `Ffi.purs`:

| Files found | Result |
| --- | --- |
| F# only | Nest the F# source and emit its public wrappers. |
| C# only | Emit C# method wrappers and return the original C# source for `Project`. |
| Both | F# owns the public wrappers. C# is still copied and compiled, allowing explicit calls from F# helpers. |
| Neither, with required foreigns | Emit failing placeholders. Function types determine their curried arity after peeling `ForAll`, constraints and type applications; unknown/non-function types retain a one-argument failing thunk. |
| Neither, without foreigns | Emit no FFI text. |

Partial application constructs closures until the final native argument arrives. A returned effect thunk remains delayed and reusable; native code determines its body. `sharpurs_apply` supplies the generic invocation exception boundary. Missing function placeholders also wait for saturation, so creating an unused partial does not fail at module initialization.

`npm run test:ffi-support` runs declaration checks, calls the production `Ffi.loadModule` and `Project` entry points, and compiles the generated mixed F#/C# project. It checks values, casing, tuple/Unit arguments, reusable partials, returned functions/delegates, effects, exception causes, file precedence and typed missing stubs. `FFI_SUPPORT_ARTIFACTS` retains the generated project, source hashes and log. `FFI_SUPPORT_ORACLE=/path/to/FfiSupport-before.mjs` additionally compares complete emitted text across declaration fixtures, arities, line endings and module names.

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

## Shared analyses and recognition contracts

`Analysis.Source` provides structural views of the source TAST. Its consumers still own native eligibility: accepted types, canonical dictionaries, exact saturation, captures, dependencies, collisions and exception boundaries. Missing or contradictory annotations provide no type evidence; each consumer retains its existing checks, including the explicit newtype-identity exception in the thunk helper recognizer.

The shared operations follow these contracts:

| Analysis | Shared behavior and consumers |
| --- | --- |
| `Source.annotation`, `hasType` | Read the source annotation and compare types structurally. Used by direct calls, integer operators and the ADT/thunk kernels. No inference or substitution is performed. |
| `Source.hasPolymorphicType` | Check exactly one quantified variable against the caller's complete signature. Integer arithmetic, comparison and thunk helper calls retain their own identities and check any explicit `TypeApp Int` against both signatures. |
| `Source.lambdas` | Collect consecutive `ExprAbs` nodes, their identifiers and annotations. `let`, `case` and `TypeApp` end the spine. Shared by direct-call selection, generic recursive emission and thunk worker arity detection. |
| `Source.applications`, `flattenApp` | Traverse only `ExprApp`, in argument order. The annotated view retains the result annotation of each application for direct-call suffix checks; the value-only view serves generic calls and thunk calls. `TypeApp` remains in the head. |
| `Source.bindings` | Flatten source binding groups while retaining recursive/singleton status. ADT and thunk selection continue to distinguish a recursive singleton from a mutual group. |
| `Source.children`, `references` | Traverse all expression children, including guards, let right-hand sides, literals, record updates and `TypeApp`. Variable references retain their order and duplicates. ADT dependency checks and thunk capture/reference checks share this traversal. |
| `Names.inModule`, `qualified` | Flatten the owner, then escape the complete value name. An explicit qualifier wins over the current module; an unqualified lexical reference can retain no owner. Constructor registries, native producers and consumers use the same value-name convention. |

Some similar-looking walks have different contracts:

- **Constructor saturation:** `ConstructorCall.applications` deliberately traverses both `App` and `TypeApp`. Eligibility still requires a registered constructor, matching field metadata and exact saturation; an unqualified local variable is not constructor evidence.
- **Source and optimized annotations:** `Analysis.Source` handles `Expr Ann`. Optimized kernels inspect `NeutralExpr` / `Typed` and validate their nested annotations during lowering. Reading an outer type does not authorize discarding contradictory inner types or unresolved type applications.
- **Function boundaries:** ADT source validation consumes the full parameter list and rejects extra lambdas. Thunk validation stops at the worker's arity and retains a returned function as its body; its `arrow` helper normalizes the function-valued suffix. Integer kernels additionally enforce native Int arguments, valid lexical levels and tail position. These validators remain specific to their kernels.
- **Whole-tree traversal versus call recognition:** dependency/capture analysis descends through a `TypeApp` to inspect its contents; that traversal does not grant permission to recognize a call across the same node.

The direct-call, integer-operation, constructor and ADT/thunk suites exercise these contracts through the production recognizers. They compile real TAST fixtures and also check targeted mutations: missing or contradictory annotations, polymorphic signatures, unresolved type applications, partial/over-applied calls and disagreement between source lambdas and optimized arity. Their F# runtime checks cover argument order, reusable partial applications and exception boundaries.

## Checks for a change

Rebuild from the compiler root with the repository's tools first on `PATH`:

```bash
export PATH="$PWD/node_modules/.bin:$PATH"
spago build
spago bundle --module Main --platform node --outfile bin/sharpurs.js --bundle-type app
spago test
```

Use the focused commands listed in the [README](../README.md#development-and-testing) for the affected representation or runtime boundary. Runtime suites compile and execute F# and require `dotnet` on `PATH` (or `DOTNET` pointing to it); `test:selection` checks the generated declarations directly in Node.js. All use the rebuilt compiler output, so rebuild before running them.

`bin/test` exercises the complete CLI on vendored PureScript fixtures. Its `tests/runner` directory is shared: run fixture selections sequentially. The focused scripts use their own temporary directories.

For orchestration or template refactors, compare generated sources and project files against the previous compiler on the same typed input and FFI sources. This checks module order, wrapper selection, filenames and project references together. Content comparisons also distinguish a formatting-only change from a change to emitted code. Existing benchmark baselines require their own separate-process performance comparisons if optimization or runtime behavior changes.
