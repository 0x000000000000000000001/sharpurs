
let mutable checks = 0
let check label condition = if not condition then failwith label else checks <- checks + 1
let apply = sharpurs_apply
let call2 fn a b = apply (apply fn a) b
let pairNative value = match unbox<Native.ConstructorTypeApp_Tuple> value with Native.ConstructorTypeApp_Tupleusd_Ctor (a,b) -> a,b
let pairOracle value = match unbox<Oracle.ConstructorTypeApp_Tuple> value with Oracle.ConstructorTypeApp_Tupleusd_Ctor (a,b) -> a,b
let rec listNative value =
    match unbox<Native.ConstructorTypeApp_List> value with
    | Native.ConstructorTypeApp_Nilusd_Ctor -> []
    | Native.ConstructorTypeApp_Consusd_Ctor (a,tail) -> unbox<int> a :: listNative tail
let rec listOracle value =
    match unbox<Oracle.ConstructorTypeApp_List> value with
    | Oracle.ConstructorTypeApp_Nilusd_Ctor -> []
    | Oracle.ConstructorTypeApp_Consusd_Ctor (a,tail) -> unbox<int> a :: listOracle tail
for value in [System.Int32.MinValue; -1; 0; 1; System.Int32.MaxValue] do
    match unbox<ConstructorImported_Envelope> (apply Native.ConstructorTypeApp_importedBox (box value)) with
    | ConstructorImported_Envelopeusd_Ctor payload -> check "imported polymorphic payload" (unbox<int> payload=value)
    match unbox<ConstructorImported_Envelope> (apply Oracle.ConstructorTypeApp_importedBox (box value)) with
    | ConstructorImported_Envelopeusd_Ctor payload -> check "imported polymorphic oracle payload" (unbox<int> payload=value)
    match unbox<Native.ConstructorTypeApp_Box> (apply Native.ConstructorTypeApp_boxInt (box value)) with
    | Native.ConstructorTypeApp_Boxusd_Ctor payload -> check "single payload" (unbox<int> payload=value)
    match unbox<Oracle.ConstructorTypeApp_Box> (apply Oracle.ConstructorTypeApp_boxInt (box value)) with
    | Oracle.ConstructorTypeApp_Boxusd_Ctor payload -> check "single payload oracle" (unbox<int> payload=value)
    for text in [""; "alpha"; "☃"] do
        for native,baseline in [(Native.ConstructorTypeApp_pair,Oracle.ConstructorTypeApp_pair);(Native.ConstructorTypeApp_polyPair,Oracle.ConstructorTypeApp_polyPair);(Native.ConstructorTypeApp_explicitPair,Oracle.ConstructorTypeApp_explicitPair)] do
            let a,b=pairNative (call2 native (box value) (box text))
            let c,d=pairOracle (call2 baseline (box value) (box text))
            check "two type arguments preserve payloads" (unbox<int> a=value && unbox<string> b=text && a=c && b=d)
        let partial=apply Native.ConstructorTypeApp_partialPair (box value)
        for suffix in [text; text+"!"] do
            let a,b=pairNative (apply partial (box suffix))
            check "partial constructor reused" (unbox<int> a=value && unbox<string> b=suffix)
    match unbox<Native.ConstructorTypeApp_Maybe> (apply Native.ConstructorTypeApp_justInt (box value)) with
    | Native.ConstructorTypeApp_Justusd_Ctor a -> check "Maybe payload" (unbox<int> a=value)
    | _ -> failwith "Expected Just"
    check "List constructor/order/Int32 oracle" (listNative (apply Native.ConstructorTypeApp_list (box value))=listOracle (apply Oracle.ConstructorTypeApp_list (box value)))
    let tail=apply Native.ConstructorTypeApp_list (box value)
    match unbox<Native.ConstructorTypeApp_List> (call2 Native.ConstructorTypeApp_prepend (box 91) tail) with
    | Native.ConstructorTypeApp_Consusd_Ctor (a,b) -> check "recursive payload shares source tail" (unbox<int> a=91 && Object.ReferenceEquals(b,tail))
    | _ -> failwith "Expected Cons"
    check "ordinary polymorphic function unaffected" (call2 Native.ConstructorTypeApp_ordinaryCall (box value) (box 99) |> unbox<int> = value)
match unbox<Native.ConstructorTypeApp_Maybe> Native.ConstructorTypeApp_nothingInt with
| Native.ConstructorTypeApp_Nothingusd_Ctor -> check "nullary constructor remains callable value" true
| _ -> failwith "Expected Nothing"
for fn,decode in [(Native.ConstructorTypeApp_ordered,pairNative);(Oracle.ConstructorTypeApp_ordered,pairOracle);(Native.ConstructorTypeApp_capturedArgument,pairNative);(Oracle.ConstructorTypeApp_capturedArgument,pairOracle)] do
    events.Clear()
    let partial=apply fn (box 7)
    let beforeSecond=List.ofSeq events
    let a,b=decode (apply partial (box 11))
    check "arguments observed once in order" (List.ofSeq events=[1;2] && unbox<int> a=7 && unbox<int> b=11)
    if Object.ReferenceEquals(fn,Native.ConstructorTypeApp_capturedArgument) || Object.ReferenceEquals(fn,Oracle.ConstructorTypeApp_capturedArgument) then
        check "partial captures first argument eagerly" (beforeSecond=[1])
let failure action =
    let caught = try action() |> ignore; None with error -> Some error
    let rec unwrap (error: exn) depth =
        match error with
        | :? System.Reflection.TargetInvocationException as e when not (isNull e.InnerException) -> unwrap e.InnerException (depth+1)
        | e -> e,depth
    match caught with
    | None -> failwith "Expected argument exception"
    | Some e -> unwrap e 0
for native,baseline,expectedEvents in [(Native.ConstructorTypeApp_throwFirst,Oracle.ConstructorTypeApp_throwFirst,[1]);(Native.ConstructorTypeApp_throwSecond,Oracle.ConstructorTypeApp_throwSecond,[1;2])] do
    events.Clear()
    let cause,depth=failure(fun () -> apply native (box 37))
    check "exception argument order" (List.ofSeq events=expectedEvents)
    events.Clear()
    let oldCause,oldDepth=failure(fun () -> apply baseline (box 37))
    check "original exception identity" (Object.ReferenceEquals(cause,sentinel) && Object.ReferenceEquals(oldCause,sentinel))
    check "unchanged exception wrapping depth" (depth=oldDepth && depth=2)
    check "oracle exception argument order" (List.ofSeq events=expectedEvents)
let tail=ConstructorNative_Node_adt_native 13 ConstructorNative_Leaf_adt_native
let boxedTail=box tail
let a,b=pairNative(call2 Native.ConstructorTypeApp_nativePair (box 17) boxedTail)
check "native factory payload type" (a :? ConstructorNative_Tree)
check "native value payload identity" (Object.ReferenceEquals(b,boxedTail))
match unbox<ConstructorNative_Tree> a with
| ConstructorNative_Nodeusd_Ctor (value,child) -> check "native factory field values" (value=17 && Object.ReferenceEquals(child,tail))
| _ -> failwith "Expected native Node"
printfn "constructor-typeapp runtime: %d checks passed" checks
