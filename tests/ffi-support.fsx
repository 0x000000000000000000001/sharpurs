// Compiled as Entry.fs by the production Project module after the native FFIs.
open System
open System.Reflection

let mutable checks = 0
let check label condition =
    if not condition then failwith label
    checks <- checks + 1
let apply = sharpurs_apply
let expectInt label expected actual = check label (unbox<int> actual = expected)
let captured action =
    match (try action() |> ignore; None with ex -> Some ex) with
    | Some ex -> ex
    | None -> failwith "Expected an exception"
let expectCause label cause action =
    let error = captured action
    check label (match error with
        | :? TargetInvocationException as ex -> Object.ReferenceEquals(ex.InnerException, cause)
        | _ -> false)

expectInt "F# value" 41 Fixture_Fsharp_value
expectInt "F# escaped identifier" 17 Fixture_Fsharp_match
expectInt "function-valued binding" 42 (apply Fixture_Fsharp_functionValue (box 41))
expectInt "Unit is one F# argument" 42 (apply Fixture_Fsharp_unitValue (box ()))
expectInt "tuple is one F# argument" 13 (apply Fixture_Fsharp_tupled (box (20, 7)))
expectInt "recursive F# declaration" 0 (apply Fixture_Fsharp_countdown (box 37))

let fsPartial = apply Fixture_Fsharp_add (box 10)
check "F# partial defers native call" (Fixture_Fsharp_FFI.calls = 0)
expectInt "F# saturated result" 42 (apply fsPartial (box 32))
expectInt "F# partial can be reused" 15 (apply fsPartial (box 5))
check "F# calls happen only on saturation" (Fixture_Fsharp_FFI.calls = 2)
expectInt "F# argument order" 13 (apply (apply (apply Fixture_Fsharp_difference (box 20)) (box 3)) (box 4))

let fsEffect = apply Fixture_Fsharp_effect (box 23)
check "F# effect construction is delayed" (Fixture_Fsharp_FFI.calls = 2)
expectInt "F# effect result" 23 (apply fsEffect (box ()))
expectInt "F# effect can run again" 23 (apply fsEffect (box ()))
check "F# effect executes on every force" (Fixture_Fsharp_FFI.calls = 4)
let fsFail = apply Fixture_Fsharp_fail (box 1)
expectCause "F# final argument exception boundary" Fixture_Fsharp_FFI.failure (fun () -> apply fsFail (box 2))
let fsDelayedFailure = apply Fixture_Fsharp_delayedFailure (box 0)
expectCause "F# deferred exception boundary" Fixture_Fsharp_FFI.failure (fun () -> apply fsDelayedFailure (box ()))

expectInt "C# nullary method value" 7 Fixture_Csharp_getValue
check "C# nullary method runs at initialization" (Fixture.Csharp.FFI.Reads = 1)
expectInt "C# boxed value can be reused" 7 Fixture_Csharp_getValue
check "C# value read does not call again" (Fixture.Csharp.FFI.Reads = 1)
let csPartial = apply Fixture_Csharp_add (box 10)
check "C# partial defers native call" (Fixture.Csharp.FFI.Calls = 0)
expectInt "C# method casing and saturated result" 42 (apply csPartial (box 32))
expectInt "C# partial can be reused" 15 (apply csPartial (box 5))
check "C# calls happen only on saturation" (Fixture.Csharp.FFI.Calls = 2)
expectInt "C# argument order" 13 (apply (apply (apply Fixture_Csharp_difference (box 20)) (box 3)) (box 4))
expectInt "C# nullable return type" 29 (apply Fixture_Csharp_nullable (box 29))
check "C# array return type" (unbox<int[]> (apply (apply Fixture_Csharp_pair (box 3)) (box 9)) = [|3; 9|])
let offset = apply Fixture_Csharp_offset (box 10)
expectInt "C# returned delegate" 42 (apply offset (box 32))
expectInt "C# returned delegate retains its capture" 15 (apply offset (box 5))

let csEffect = apply Fixture_Csharp_effect (box 23)
check "C# effect construction is delayed" (Fixture.Csharp.FFI.Calls = 2)
expectInt "C# effect result" 23 (apply csEffect (box ()))
expectInt "C# effect can run again" 23 (apply csEffect (box ()))
check "C# effect executes on every force" (Fixture.Csharp.FFI.Calls = 4)
let csFail = apply Fixture_Csharp_fail (box 1)
expectCause "C# final argument exception boundary" Fixture.Csharp.FFI.Failure (fun () -> apply csFail (box 2))
let csDelayedFailure = apply Fixture_Csharp_delayedFailure (box 0)
expectCause "C# deferred exception boundary" Fixture.Csharp.FFI.Failure (fun () -> apply csDelayedFailure (box ()))

expectInt "F# owns wrappers while C# helpers remain usable" 42 Fixture_Both_chosen
expectInt "configured FFI directory" 73 Fixture_Search_value

for name, stub in ["binary", Fixture_Missing_binary; "quantified", Fixture_Missing_quantified;
                   "constrained", Fixture_Missing_constrained; "applied", Fixture_Missing_applied] do
    let partial = apply stub (box 1)
    check "missing function keeps its curried ABI" (partial :? (obj -> obj))
    for right in [2; 3] do
        let error = captured (fun () -> apply partial (box right))
        check "missing function fails only at saturation" (match error with
            | :? TargetInvocationException as ex -> ex.InnerException.Message = "FFI not implemented: Fixture.Missing." + name
            | _ -> false)
for name, stub in ["unknown", Fixture_Missing_unknown; "value", Fixture_Missing_value] do
    let error = captured (fun () -> apply stub (box ()))
    check "untyped or value import retains failing thunk" (match error with
        | :? TargetInvocationException as ex -> ex.InnerException.Message = "FFI not implemented: Fixture.Missing." + name
        | _ -> false)

printfn "ffi-support runtime: %d checks passed" checks
let Entry_main : obj = box (fun (_: obj) -> box ())
