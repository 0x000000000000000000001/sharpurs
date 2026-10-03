
let mutable checks = 0
let check label condition = if not condition then failwith label else checks <- checks + 1
let apply = sharpurs_apply
let call fn n seed = apply (apply fn (box n)) (box seed) |> unbox<int>
let cases = [{{CASES}}]
for depth, seed, expected in cases do
    check "native route agrees with JS" (call Native.TypedThunks_run depth seed = expected)
    check "generic oracle agrees with JS" (call Oracle.TypedThunks_run depth seed = expected)
    check "literal seed result" (apply Native.TypedThunks_runLiteral (box depth) |> unbox<int> = depth + 7)
    let left = apply (apply Native.TypedThunks_escaped (box depth)) (box seed)
    check "escaping thunk stays callable" (apply Native.TypedThunks_force left |> unbox<int> = expected)
    check "escaping thunk reusable" (apply Native.TypedThunks_force left |> unbox<int> = expected)
    let partial = apply Native.TypedThunks_run (box depth)
    check "partial Int capture reused" (apply partial (box seed) |> unbox<int> = expected)
    check "partial captures independent" (apply partial (box 9) |> unbox<int> = depth + 9)
for step, depth, seed in [(2, 5, 7); (-3, 10, 29); (System.Int32.MaxValue, 3, 7)] do
    let callBy fn = apply (apply (apply fn (box step)) (box depth)) (box seed) |> unbox<int>
    check "additional captured step" (callBy Native.TypedThunks_runBy = callBy Oracle.TypedThunks_runBy)
for depth, left, right, two, captured in [{{CAPTURE_CASES}}] do
    let callTwo fn = apply (apply (apply fn (box depth)) (box left)) (box right) |> unbox<int>
    check "independent native seeds" (callTwo Native.TypedThunks_runTwo = two)
    check "independent generic seeds" (callTwo Oracle.TypedThunks_runTwo = two)
    check "capture names cannot shadow later capture values" (callTwo Native.TypedThunks_runCaptureCollision = captured)
    check "generic multiple capture oracle" (callTwo Oracle.TypedThunks_runCaptureCollision = captured)
    let partial = apply Native.TypedThunks_runCaptureCollision (box depth)
    let withLeft = apply partial (box left)
    check "reused multiple capture partial" (apply withLeft (box right) |> unbox<int> = captured)
    check "reused partial gets a fresh right capture" (apply withLeft (box -7) |> unbox<int> = depth + left - 7)
    check "reused partial gets a fresh left capture" (apply (apply partial (box 19)) (box right) |> unbox<int> = depth + 19 + right)
for depth in [0; 1; 3; 1000] do
    let seed : obj = box (fun (_: obj) -> events.Add(31); box 11)
    events.Clear()
    check "unknown callback retains result" (call Native.TypedThunks_unknownCallback depth seed = depth + 11)
    check "unknown callback executes once" (List.ofSeq events = [31])
    let partial = apply Native.TypedThunks_chain (box depth)
    let left = apply partial seed
    check "construction is delayed" (List.ofSeq events = [31])
    for _ in [1..2] do apply Native.TypedThunks_force left |> ignore
    check "no memoization added" (List.ofSeq events = [31; 31; 31])
    events.Clear()
    check "opaque nested seed result" (call Native.TypedThunks_opaqueRun depth 12 = depth + 12)
    check "opaque seed observes exactly one call" (List.ofSeq events = [12])
let signature action =
    let rec shape (error: exn) wrappers =
        match error with
        | :? System.Reflection.TargetInvocationException as invocation when not (isNull invocation.InnerException) -> shape invocation.InnerException (wrappers + 1)
        | cause -> cause.GetType().FullName, wrappers
    let caught = try action() |> ignore; None with ex -> Some (shape ex 0)
    match caught with Some value -> value | None -> failwith "Expected exception"
for depth in [0; 1; 3] do
    check "local partial exception boundary" (signature (fun () -> call Native.TypedThunks_partialRun depth 1) = signature (fun () -> call Oracle.TypedThunks_partialRun depth 1))
    check "imported partial exception boundary" (signature (fun () -> call Native.TypedThunks_importedRun depth 1) = signature (fun () -> call Oracle.TypedThunks_importedRun depth 1))
let sentinel = InvalidOperationException("delayed callback")
for depth in [0; 1; 3; 1000] do
    let mutable throws = 0
    let seed : obj = box (fun (_: obj) -> throws <- throws + 1; raise sentinel : obj)
    let chain = apply (apply Native.TypedThunks_chain (box depth)) seed
    check "throwing seed stays delayed" (throws = 0)
    for repeat in [1..2] do
        let mutable caught = None
        try apply Native.TypedThunks_force chain |> ignore with ex -> caught <- Some ex
        let rec cause (ex: exn) count =
            match ex with
            | :? System.Reflection.TargetInvocationException as e when not (isNull e.InnerException) -> cause e.InnerException (count + 1)
            | e -> e, count
        let exceptionCause, wrappers = cause caught.Value 0
        check "deferred cause identity" (Object.ReferenceEquals(exceptionCause, sentinel))
        check "original force boundaries" (wrappers = 2 * (depth + 1))
        check "repeat force calls again" (throws = repeat)
let mutable total = 0
for _ in [1..1000] do total <- total + call Native.TypedThunks_run 1000 0
check "full million-thunk workload" (total = 1000000)
printfn "thunk-kernel runtime: %d checks passed" checks
