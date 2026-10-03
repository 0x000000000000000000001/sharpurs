
let mutable checks = 0
let check label condition = if not condition then failwith label else checks <- checks + 1
let apply = sharpurs_apply
let call2 fn a b = apply (apply fn a) b
let call4 fn a b c d = apply (apply (apply (apply fn a) b) c) d
let node n = call4 Native.AdtMulti_assemble Native.AdtMulti_Ruby Native.AdtMulti_Tip (box n) Native.AdtMulti_Tip
let reference n = call4 Oracle.AdtMulti_assemble Oracle.AdtMulti_Ruby Oracle.AdtMulti_Tip (box n) Oracle.AdtMulti_Tip
let rec nativeShape (tree: Native.AdtMulti_Tree) =
    match tree with
    | Native.AdtMulti_Tipusd_Ctor -> "."
    | Native.AdtMulti_Branchusd_Ctor(color, left, value, right) -> sprintf "(%A,%s,%d,%s)" color (nativeShape left) value (nativeShape right)
let rec oracleShape (tree: Oracle.AdtMulti_Tree) =
    match tree with
    | Oracle.AdtMulti_Tipusd_Ctor -> "."
    | Oracle.AdtMulti_Branchusd_Ctor(color, left, value, right) -> sprintf "(%A,%s,%d,%s)" color (oracleShape left) value (oracleShape right)
let sameTree actual expected = nativeShape (unbox actual) = oracleShape (unbox expected)
let build fn empty values = List.fold (fun tree value -> call2 fn (box value) tree) empty values
let samples = [[1..40]; [40..-1..1]; [7; 2; 11; 0; 4; 9; 13; 2; 7]; [System.Int32.MinValue; 0; System.Int32.MaxValue; -1; 1]]
for values in samples do
    let tree = build Native.AdtMulti_insert Native.AdtMulti_Tip values
    let old = build Oracle.AdtMulti_insert Oracle.AdtMulti_Tip values
    check "recursive insert entire structure matches oracle" (sameTree tree old)
    check "native depth agrees" (apply Native.AdtMulti_depth tree = apply Oracle.AdtMulti_depth old)
    for value in values do
        check "duplicate insertion preserves complete tree" (sameTree (call2 Native.AdtMulti_insert (box value) tree) old)
    check "duplicate root insertion preserves root identity" (System.Object.ReferenceEquals(call2 Native.AdtMulti_insert (box values.Head) tree, tree))
    for amount in [System.Int32.MinValue; -1; 0; 1; System.Int32.MaxValue] do
        check "recursive shift, Int wrap and dependency calls agree" (sameTree (call2 Native.AdtMulti_shift (box amount) tree) (call2 Oracle.AdtMulti_shift (box amount) old))
    check "input remains immutable after transformations" (sameTree tree old)
for value in [System.Int32.MinValue; -1; 0; 1; System.Int32.MaxValue] do
    let tree = node value
    let old = reference value
    for increment in [System.Int32.MinValue; -1; 0; 1; System.Int32.MaxValue] do
        for fn, baseline in [Native.AdtMulti_readNonEmpty, Oracle.AdtMulti_readNonEmpty; Native.AdtMulti_bodyFailure, Oracle.AdtMulti_bodyFailure;
                             Native.AdtMulti_importedTotal, Oracle.AdtMulti_importedTotal] do
            check "two argument native Int result and overflow agree" (call2 fn tree (box increment) = call2 baseline old (box increment))
    for flag in [false; true] do
        check "four argument Boolean branch agrees" (sameTree (call4 Native.AdtMulti_choose (box flag) tree (box value) tree) (call4 Oracle.AdtMulti_choose (box flag) old (box value) old))
    check "unused argument keeps original ADT identity" (System.Object.ReferenceEquals(call2 Native.AdtMulti_unused tree (box 99), tree))
let child = node 7
let original = reference 7
for fn, empty, leaf, encode in [Native.AdtMultiConsumer_ordered, Native.AdtMulti_Tip, child, (fun value -> nativeShape (unbox value));
                               Oracle.AdtMultiConsumer_ordered, Oracle.AdtMulti_Tip, original, (fun value -> oracleShape (unbox value))] do
    events.Clear()
    let value = apply fn leaf
    check "consumer arguments evaluated once in order" (List.ofSeq events = [1; 2; 3; 4])
    check "ordered construction actually creates node" (encode value <> encode empty)
for fn, leaf, empty in [Native.AdtMultiConsumer_orderedPartial, child, Native.AdtMulti_Tip;
                        Oracle.AdtMultiConsumer_orderedPartial, original, Oracle.AdtMulti_Tip] do
    events.Clear()
    let partial = apply fn leaf
    check "partial supplied arguments evaluate eagerly" (List.ofSeq events = [1; 2])
    let captured = apply partial (box 9)
    apply captured leaf |> ignore
    apply captured empty |> ignore
    check "reused partial captures arguments without reevaluation" (List.ofSeq events = [1; 2])
let first = apply Native.AdtMulti_assemble Native.AdtMulti_Onyx
let second = apply first child
let third = apply second (box 9)
for right in [child; Native.AdtMulti_Tip] do
    let result = apply third right |> unbox<Native.AdtMulti_Tree>
    match result with
    | Native.AdtMulti_Branchusd_Ctor(_, left, value, actualRight) ->
        check "partial wrapper captures native child identity" (System.Object.ReferenceEquals(left, child))
        check "partial wrapper captures Int" (value = 9)
        check "partial wrapper can be reused with new argument" (System.Object.ReferenceEquals(actualRight, right))
    | _ -> failwith "expected branch"
let capturedInsert = apply Native.AdtMultiConsumer_captured (box 13)
check "function value preserves first use" (sameTree (apply capturedInsert Native.AdtMulti_Tip) (call2 Oracle.AdtMulti_insert (box 13) Oracle.AdtMulti_Tip))
check "function value preserves reuse" (sameTree (apply capturedInsert child) (call2 Oracle.AdtMulti_insert (box 13) original))
let capture action = try action() |> ignore; None with ex -> Some ex
let rec signature (ex: System.Exception) =
    match ex with
    | :? System.Reflection.TargetInvocationException as wrapper -> let depth, cause = signature wrapper.InnerException in depth + 1, cause
    | cause -> 0, cause.GetType().FullName + ": " + cause.Message
let failure action = match capture action with Some ex -> signature ex | None -> failwith "expected fixture failure"
for fn, baseline, bypass, evaluate in [Native.AdtMulti_shortAnd, Oracle.AdtMulti_shortAnd, false, true;
                                      Native.AdtMulti_shortOr, Oracle.AdtMulti_shortOr, true, false] do
    check "short-circuit skips failing right hand side" (call2 fn (box bypass) Native.AdtMulti_Tip = call2 baseline (box bypass) Oracle.AdtMulti_Tip)
    let actual = failure (fun () -> call2 fn (box evaluate) Native.AdtMulti_Tip)
    let expected = failure (fun () -> call2 baseline (box evaluate) Oracle.AdtMulti_Tip)
    if actual <> expected then printfn "short-circuit exception mismatch: native=%A oracle=%A" actual expected
    check "short-circuit evaluates necessary right hand side with preserved envelope" (actual = expected)
    check "right hand side success" (call2 fn (box evaluate) child = call2 baseline (box evaluate) original)
for fn, baseline in [Native.AdtMulti_shortAndSafe, Oracle.AdtMulti_shortAndSafe; Native.AdtMulti_shortOrSafe, Oracle.AdtMulti_shortOrSafe] do
    for flag in [false; true] do
        for tree, old in [Native.AdtMulti_Tip, Oracle.AdtMulti_Tip; child, original] do
            check "native lazy Boolean primitive matches source branches" (call2 fn (box flag) tree = call2 baseline (box flag) old)
for tree, old in [Native.AdtMulti_Tip, Oracle.AdtMulti_Tip; child, original;
                  call4 Native.AdtMulti_assemble Native.AdtMulti_Onyx child (box 11) Native.AdtMulti_Tip,
                  call4 Oracle.AdtMulti_assemble Oracle.AdtMulti_Onyx original (box 11) Oracle.AdtMulti_Tip] do
    check "nested tags short-circuit constructor accessors" (call2 Native.AdtMulti_nested tree (box 3) = call2 Oracle.AdtMulti_nested old (box 3))
for native, generic, nativeArgs, genericArgs in [
    Native.AdtMulti_readNonEmpty, Oracle.AdtMulti_readNonEmpty, [Native.AdtMulti_Tip; box 0], [Oracle.AdtMulti_Tip; box 0]
    Native.AdtMulti_bodyFailure, Oracle.AdtMulti_bodyFailure, [Native.AdtMulti_Tip; box 0], [Oracle.AdtMulti_Tip; box 0]
    Native.AdtMulti_readNonEmptyInline, Oracle.AdtMulti_readNonEmptyInline, [Native.AdtMulti_Tip; box 0], [Oracle.AdtMulti_Tip; box 0]
    Native.AdtMulti_bodyFailureInline, Oracle.AdtMulti_bodyFailureInline, [Native.AdtMulti_Tip; box 0], [Oracle.AdtMulti_Tip; box 0]
    Native.AdtMulti_importedFailure, Oracle.AdtMulti_importedFailure, [Native.AdtMulti_Tip; box 1], [Oracle.AdtMulti_Tip; box 1]
    Native.AdtMulti_throughImported, Oracle.AdtMulti_throughImported, [Native.AdtMulti_Tip; box 1], [Oracle.AdtMulti_Tip; box 1]
    Native.AdtMulti_argumentFailure, Oracle.AdtMulti_argumentFailure, [child; Native.AdtMulti_Tip], [original; Oracle.AdtMulti_Tip]
    Native.AdtMulti_argumentFailure, Oracle.AdtMulti_argumentFailure, [Native.AdtMulti_Tip; child], [Oracle.AdtMulti_Tip; original]
    Native.AdtMulti_argumentOrder, Oracle.AdtMulti_argumentOrder, [Native.AdtMulti_Tip; child], [Oracle.AdtMulti_Tip; original]
    Native.AdtMulti_argumentOrder, Oracle.AdtMulti_argumentOrder, [child; Native.AdtMulti_Tip], [original; Oracle.AdtMulti_Tip]
] do
    let invoke fn args () = List.fold apply fn args
    let actual = failure (invoke native nativeArgs)
    let expected = failure (invoke generic genericArgs)
    check "typed body or argument failure keeps complete generic exception envelope" (actual = expected)
    check "exception test reached actual pattern match failure" (snd actual |> fun message -> message.Contains("MatchFailureException"))
check "external partial helper successful branch" (call2 Native.AdtMulti_importedFailure Native.AdtMulti_Tip (box 0) = call2 Oracle.AdtMulti_importedFailure Oracle.AdtMulti_Tip (box 0))
for fn, baseline, expectedEvents in [Native.AdtMultiConsumer_firstArgumentFailure, Oracle.AdtMultiConsumer_firstArgumentFailure, [];
                                      Native.AdtMultiConsumer_lastArgumentFailure, Oracle.AdtMultiConsumer_lastArgumentFailure, [1]] do
    let left, right, oldLeft, oldRight =
        if System.Object.ReferenceEquals(fn, Native.AdtMultiConsumer_firstArgumentFailure) then Native.AdtMulti_Tip, child, Oracle.AdtMulti_Tip, original
        else child, Native.AdtMulti_Tip, original, Oracle.AdtMulti_Tip
    events.Clear()
    let actual = failure (fun () -> call2 fn left right)
    check "argument failure stops later argument effects" (List.ofSeq events = expectedEvents)
    events.Clear()
    let expected = failure (fun () -> call2 baseline oldLeft oldRight)
    check "consumer exception chain matches generic oracle" (actual = expected)
    check "consumer oracle has same event order" (List.ofSeq events = expectedEvents)
for fn, leaf, empty in [Native.AdtMulti_returned, child, Native.AdtMulti_Tip; Oracle.AdtMulti_returned, original, Oracle.AdtMulti_Tip] do
    let returned = apply fn leaf
    for increment in [0; 1; 9] do
        check "returned closure retains captured value" (unbox<int> (apply returned (box increment)) = 7 + increment)
    check "returned closure preserves eager source let failure" (capture (fun () -> apply fn empty) |> Option.isSome)
for fn, baseline, bypass, evaluate in [InjectedNative.AdtMulti_shortAndSafe, InjectedOracle.AdtMulti_shortAndSafe, false, true;
                                      InjectedNative.AdtMulti_shortOrSafe, InjectedOracle.AdtMulti_shortOrSafe, true, false] do
    events.Clear()
    check "injected native lazy RHS bypass matches generic source" (call2 fn (box bypass) InjectedNative.AdtMulti_Tip = call2 baseline (box bypass) InjectedOracle.AdtMulti_Tip)
    check "bypassed RHS was not evaluated in either version" (events.Count = 0)
    let actual = failure (fun () -> call2 fn (box evaluate) InjectedNative.AdtMulti_Tip)
    check "native RHS evaluated exactly once" (List.ofSeq events = [77])
    events.Clear()
    let expected = failure (fun () -> call2 baseline (box evaluate) InjectedOracle.AdtMulti_Tip)
    check "generic RHS evaluated exactly once" (List.ofSeq events = [77])
    check "native cross-call wraps same sentinel exactly as generic call" (actual = expected && fst actual = 2)
events.Clear()
let argumentException = failure (fun () -> call2 InjectedNative.AdtMulti_argumentNative InjectedNative.AdtMulti_Tip InjectedNative.AdtMulti_Tip)
check "failing native argument prevents outer callee evaluation" (List.ofSeq events = [77])
events.Clear()
let oracleArgumentException = failure (fun () -> call2 InjectedOracle.AdtMulti_argumentNative InjectedOracle.AdtMulti_Tip InjectedOracle.AdtMulti_Tip)
check "generic failing argument prevents outer callee evaluation" (List.ofSeq events = [77])
check "argument evaluates before outer invocation guard" (argumentException = oracleArgumentException && fst argumentException = 2)
printfn "adt-multi runtime: %d checks passed" checks
