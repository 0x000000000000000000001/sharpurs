
let mutable checks = 0
let check label ok = if not ok then failwith label else checks <- checks + 1
let apply = sharpurs_apply
let call2 fn x y = apply (apply fn (box x)) (box y)
let call4 fn a b c d = apply (apply (apply (apply fn (box a)) (box b)) (box c)) (box d)
let asInt value = unbox<int> value
for a in [System.Int32.MinValue; -1; 0; 1; 9; 10; System.Int32.MaxValue] do
    for b in [System.Int32.MinValue; 0; 10; System.Int32.MaxValue] do
        check "public wrapper matches generic oracle" (call4 DirectCall_ordinal a b 20 30 = call4 DirectFallback_ordinal a b 20 30)
        check "raw direct method matches public wrapper" (DirectCall_ordinal_direct (box a) (box b) (box 20) (box 30) = call4 DirectCall_ordinal a b 20 30)
        check "captured top-level constant" (call2 DirectCall_captured a b = call2 DirectFallback_captured a b)
    check "saturated caller matches generic oracle" (apply DirectCall_saturated (box a) = apply DirectFallback_saturated (box a))
for first in [false; true] do
    for second in [false; true] do
        check "Boolean arguments and captured value" (call4 DirectCall_guarded first 11 second 22 = call4 DirectFallback_guarded first 11 second 22)
for fn in [DirectCall_ordered; DirectFallback_ordered] do
    events.Clear()
    check "ordered result" (asInt (apply fn (box 3)) = 20)
    check "all arguments evaluated once in source order" (List.ofSeq events = [1; 2; 3; 4])
let one = apply DirectCall_ordinal (box 1)
let two = apply one (box 2)
let three = apply two (box 3)
check "one captured argument reusable first" (asInt (call2 (apply one (box 2)) 3 4) = 3)
check "one captured argument reusable second" (asInt (call2 (apply one (box 0)) 5 6) = 6)
check "two captured arguments reusable first" (asInt (call2 two 7 8) = 7)
check "two captured arguments reusable second" (asInt (call2 two 9 10) = 9)
check "three captured arguments reusable first" (asInt (apply three (box 40)) = 3)
check "three captured arguments reusable second" (asInt (apply three (box 50)) = 3)
check "top-level one argument partial" (asInt (call2 (apply DirectCall_partial1 (box 2)) 3 4) = 3)
check "top-level two argument partial" (asInt (call2 DirectCall_partial2 7 8) = 7)
check "top-level three argument partial" (asInt (apply DirectCall_partial3 (box 8)) = 3)
check "function as a value" (asInt (call4 DirectCall_asValue 1 2 3 4) = 3)
events.Clear()
let orderedOne = apply DirectCall_orderedPartial (box 1)
check "partial supplied argument evaluates eagerly" (List.ofSeq events = [1])
check "ordered partial first use" (asInt (call2 (apply orderedOne (box 2)) 3 4) = 3)
check "ordered partial second use" (asInt (call2 (apply orderedOne (box 0)) 5 6) = 6)
check "partial capture does not repeat argument effects" (List.ofSeq events = [1])
let firstOfFour : obj = box (fun (a: obj) -> box (fun (_: obj) -> box (fun (_: obj) -> box (fun (_: obj) -> a))))
check "shadowed parameter remains dynamic" (asInt (apply DirectCall_shadowed firstOfFour) = 4)
check "local shadow remains local" (asInt (apply DirectCall_localShadow (box 42)) = 42)
check "imported function retains object ABI" (asInt (call2 DirectCall_imported 41 99) = 41)
for fn in [DirectCall_returned; DirectFallback_returned] do
    events.Clear()
    let returned = call2 fn true 41
    check "returned function boundary evaluates let eagerly" (List.ofSeq events = [90])
    check "returned function first use" (asInt (apply returned (box 1)) = 41)
    check "returned function reused" (asInt (apply returned (box 2)) = 41)
    check "returned function retains capture" (List.ofSeq events = [90])
let captured action = try action() |> ignore; failwith "expected failure" with ex -> ex
let rec chain (ex: System.Exception) =
    match ex with
    | :? System.Reflection.TargetInvocationException as wrapper -> 1 + chain wrapper.InnerException
    | cause when System.Object.ReferenceEquals(cause, failure) -> 0
    | cause -> failwithf "Unexpected exception: %A" cause
let publicFailure = chain (captured (fun () -> call2 DirectCall_failBody 1 2))
let fallbackFailure = chain (captured (fun () -> call2 DirectFallback_failBody 1 2))
check "public body exception chain matches generic oracle" (publicFailure = fallbackFailure && publicFailure = 2)
let directFailure = chain (captured (fun () -> apply DirectCall_bodyCall (box 1)))
let fallbackCallerFailure = chain (captured (fun () -> apply DirectFallback_bodyCall (box 1)))
check "direct body exception chain matches generic caller" (directFailure = fallbackCallerFailure && directFailure = 3)
for fn in [DirectCall_argumentCall; DirectFallback_argumentCall] do
    events.Clear()
    check "argument exception stays outside direct invocation envelope" (chain (captured (fun () -> apply fn (box 1))) = 2)
    check "argument failure stops later evaluations" (List.ofSeq events = [99])
printfn "direct-call runtime: %d checks passed" checks
