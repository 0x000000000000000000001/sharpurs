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
| Recognition of native declarations and wrapper text | [`Sharpurs/FfiSupport.js`](../src/Sharpurs/FfiSupport.js) |
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
| Typed integer, direct-call, constructor and thunk subsets | `IntKernel`, `Optimized`, `DirectCall`, `ConstructorCall`, integer-operation modules and `ThunkKernel` |

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
