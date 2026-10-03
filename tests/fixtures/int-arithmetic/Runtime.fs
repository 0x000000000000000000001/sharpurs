
let mutable checks = 0
let check label condition = if not condition then failwith label else checks <- checks + 1
let apply = sharpurs_apply
let call fn x y = apply (apply fn (box x)) (box y) |> unbox<int>
let cases = [
{{CASES}}
]
for x, y, sum, difference in cases do
    let before = fallbackCalls
    check "native sum matches JS" (call IntArithmetic_addInt x y = sum)
    check "native difference matches JS" (call IntArithmetic_subInt x y = difference)
    check "native Int operations bypass dictionaries" (fallbackCalls = before)
    check "generic oracle sum" (call ArithmeticFallback_addInt x y = sum)
    check "generic oracle difference" (call ArithmeticFallback_subInt x y = difference)
    check "oracle uses actual dictionary calls" (fallbackCalls = before + 2)
    check "visible TypeApp add fallback" (call IntArithmetic_visibleAdd x y = sum)
    check "visible TypeApp sub fallback" (call IntArithmetic_visibleSub x y = difference)
    check "generic Int add" (call (apply IntArithmetic_genericAdd Data_Semiring_semiringInt) x y = sum)
    check "generic Int sub" (call (apply IntArithmetic_genericSub Data_Ring_ringInt) x y = difference)
    check "annotated add alias" (call IntArithmetic_annotatedAdd x y = sum)
    check "annotated sub alias" (call IntArithmetic_annotatedSub x y = difference)
    check "custom Int semiring retains supplied operation" (call (apply IntArithmetic_genericAdd customSemiring) x y = (y - x))
    check "custom Int ring retains supplied operation" (call (apply IntArithmetic_genericSub customRing) x y = sum)
for y in [System.Int32.MinValue; -1; 0; 1; System.Int32.MaxValue] do
    check "partial add reusable" (unbox<int> (apply IntArithmetic_partialAdd (box y)) = 5 + y)
    check "partial sub reusable" (unbox<int> (apply IntArithmetic_partialSub (box y)) = 5 - y)
let capturedAdd = apply IntArithmetic_capturedAdd (box System.Int32.MaxValue)
let capturedSub = apply IntArithmetic_capturedSub (box System.Int32.MinValue)
for y in [-1; 0; 1; 7] do
    check "captured addition reused" (unbox<int> (apply capturedAdd (box y)) = System.Int32.MaxValue + y)
    check "captured subtraction reused" (unbox<int> (apply capturedSub (box y)) = System.Int32.MinValue - y)
let floatCall fn x y = apply (apply fn (box x)) (box y) |> unbox<float>
check "Number addition fallback" (floatCall IntArithmetic_numberAdd 1.25 1.5 = 2.75)
check "Number subtraction fallback" (floatCall IntArithmetic_numberSub 1.25 1.5 = -0.25)
for fn, expected in [IntArithmetic_orderedAdd, 10; ArithmeticFallback_orderedAdd, 10;
                     IntArithmetic_orderedSub, -4; ArithmeticFallback_orderedSub, -4] do
    events.Clear()
    check "ordered arithmetic result" (call fn 3 7 = expected)
    check "operands evaluate exactly once left to right" (List.ofSeq events = [1; 2])
for fn, reference in [IntArithmetic_delayedAdd, ArithmeticFallback_delayedAdd;
                      IntArithmetic_delayedSub, ArithmeticFallback_delayedSub] do
    reads <- 0
    currentValue <- 10
    let delayed = apply fn (box 7)
    let oracle = apply reference (box 7)
    check "constructing closures does not force reads" (reads = 0)
    for value in [11; -1; System.Int32.MinValue; System.Int32.MaxValue] do
        currentValue <- value
        let before = reads
        let actual = apply delayed Data_Unit_unit
        check "each force reads once" (reads = before + 1)
        check "reused closure sees latest state" (actual = apply oracle Data_Unit_unit)
        check "oracle force reads once" (reads = before + 2)
let captured action = try action() |> ignore; failwith "expected failure" with ex -> ex
let rec chain (ex: System.Exception) =
    match ex with
    | :? System.Reflection.TargetInvocationException as wrapper -> 1 + chain wrapper.InnerException
    | cause when System.Object.ReferenceEquals(cause, failure) -> 0
    | cause -> failwithf "Unexpected exception: %A" cause
for fn, reference, expected in [IntArithmetic_firstFailureAdd, ArithmeticFallback_firstFailureAdd, [1; 99];
                                IntArithmetic_secondFailureAdd, ArithmeticFallback_secondFailureAdd, [1; 2; 99];
                                IntArithmetic_firstFailureSub, ArithmeticFallback_firstFailureSub, [1; 99];
                                IntArithmetic_secondFailureSub, ArithmeticFallback_secondFailureSub, [1; 2; 99]] do
    events.Clear()
    let actualChain = chain (captured (fun () -> apply fn (box 3)))
    check "argument exception stops further evaluation" (List.ofSeq events = expected)
    events.Clear()
    let oracleChain = chain (captured (fun () -> apply reference (box 3)))
    check "argument exception wrapper chain matches generic oracle" (actualChain = oracleChain && actualChain = 2)
    check "generic oracle has same exception order" (List.ofSeq events = expected)
printfn "int-arithmetic runtime: %d checks passed" checks
