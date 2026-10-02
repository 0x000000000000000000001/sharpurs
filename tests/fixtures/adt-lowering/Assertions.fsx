#load "Support.fs"
#load "Generated.fs"
open AdtLoweringSupport
open AdtLoweringGenerated

let mutable checks = 0
let check label passed =
    if not passed then failwith label
    checks <- checks + 1
let empty = Lowering_Emptyusd_Ctor
let child = Lowering_Nodeusd_Ctor(empty, 7, empty)
let tree = Lowering_Nodeusd_Ctor(child, 41, child)
let invoke fn = fn tree child 41 true
let same left right = System.Object.ReferenceEquals(left, right)
let captured action =
    try action() |> ignore; failwith "expected fixture failure"
    with ex -> ex
let rec chain (ex: System.Exception) =
    match ex with
    | :? System.Reflection.TargetInvocationException as wrapper -> 1 + chain wrapper.InnerException
    | cause when same cause sentinel -> 0
    | cause -> failwithf "Unexpected exception: %A" cause

check "minimum Int literal" (invoke literalMin = System.Int32.MinValue)
check "maximum Int literal" (invoke literalMax = System.Int32.MaxValue)
check "Boolean literals" (invoke literalTrue && not (invoke literalFalse))
check "local Int" (invoke localInt = 41)
check "local identity" (same (invoke localTree) tree)
check "global value" (invoke globalValue = 42)
for value in [System.Int32.MinValue; -1; 0; 41; 42; 43; System.Int32.MaxValue] do
    check "addition" (op_add tree child value true = value + 42)
    check "subtraction" (op_subtract tree child value true = value - 42)
    check "equality" (op_equal tree child value true = (value = 42))
    check "inequality" (op_notEqual tree child value true = (value <> 42))
    check "greater" (op_greater tree child value true = (value > 42))
    check "greater or equal" (op_greaterOrEqual tree child value true = (value >= 42))
    check "less" (op_less tree child value true = (value < 42))
    check "less or equal" (op_lessOrEqual tree child value true = (value <= 42))
for flag in [false; true] do
    for value, isEmpty in [empty, true; child, false] do
        check "Boolean And" (op_and value child 0 flag = (flag && isEmpty))
        check "Boolean Or" (op_or value child 0 flag = (flag || isEmpty))
check "guarded target" (invoke guardedCall = 42)
check "whole-native target" (invoke wholeNativeCall = 42)
check "self target" (invoke selfCall = 42)
for build in [constructorCall; saturatedConstructor] do
    match invoke build with
    | Lowering_Nodeusd_Ctor(left, value, right) ->
        check "constructor left sharing" (same left tree)
        check "constructor Int" (value = 41)
        check "constructor right sharing" (same right child)
    | _ -> failwith "expected Node"
check "nullary constructor" (invoke emptyConstructor = empty)
check "left projection" (same (invoke leftField) child)
check "right projection" (same (invoke rightField) child)
check "Int projection" (invoke intField = 41)
check "invalid constructor failure" ((captured (fun () -> invoke wrongTagField)).Message = "Invalid ADT constructor")
check "empty tag matches" (emptyTag empty child 0 false)
check "empty tag misses" (not (invoke emptyTag))
check "node tag matches" (invoke nodeTag)
check "node tag misses" (not (nodeTag empty child 0 false))
events.Clear()
check "let result" (invoke sharedLet = 82)
check "let evaluates once" (List.ofSeq events = [41])
check "let preserves sharing" (same (invoke treeLet) tree)
events.Clear()
check "ordered call result" (invoke orderedCall = 12)
check "arguments precede callee in source order" (List.ofSeq events = [1; 2; 77])
events.Clear()
check "ordered branches result" (invoke orderedBranches = 22)
check "branches stop after first successful condition" (List.ofSeq events = [1; 2; 22])
check "branch fallback skips failed body" (invoke branchFallback = 42)
for fn, bypass, evaluate in [shortAnd, false, true; shortOr, true, false] do
    events.Clear()
    check "short circuit result" (fn tree child 0 bypass = bypass)
    check "short circuit skips RHS" (events.Count = 0)
    check "required RHS preserves guarded cause" (chain (captured (fun () -> fn tree child 0 evaluate)) = 1)
    check "required RHS evaluates once" (List.ofSeq events = [99])
check "guarded body failure" (chain (captured (fun () -> invoke bodyFailure)) = 1)
check "whole-native failure" (chain (captured (fun () -> invoke unguardedFailure)) = 0)
events.Clear()
check "argument failure stays outside callee envelope" (chain (captured (fun () -> invoke argumentFailure)) = 1)
check "argument failure stops later arguments and callee" (List.ofSeq events = [99])
check "failure text escaping" ((captured (fun () -> invoke escapedFailure)).Message = "quoted \"line\"\n\\🙂")
printfn "adt-lowering runtime: %d checks passed" checks
