// The runner prepends the actual runtime and generated FFI wrappers.
open System
open System.Reflection

let mutable checks = 0
let check label condition =
    if not condition then failwith label
    checks <- checks + 1

let apply = sharpurs_apply
let expectInt label expected actual = check label (unbox<int> actual = expected)
let captured action =
    match (try action () |> ignore; None with ex -> Some ex) with
    | Some ex -> ex
    | None -> failwith "Expected an exception"

let expectChain label depth (cause: Exception) action =
    let rec inspect remaining (ex: Exception) =
        if remaining = 0 then Object.ReferenceEquals(ex, cause)
        else
            match ex with
            | :? TargetInvocationException as wrapper ->
                wrapper.Message = (TargetInvocationException(cause)).Message
                && inspect (remaining - 1) wrapper.InnerException
            | _ -> false
    check label (inspect depth (captured action))

let direct = box (fun (value: obj) -> box (unbox<int> value + 1))
expectInt "obj -> obj closure" 42 (apply direct (box 41))
let typed = box (fun (value: int) -> value + 1)
check "typed closure requires fallback" (not (typed :? (obj -> obj)))
expectInt "typed closure fallback" 42 (apply typed (box 41))

let boxedCurried = box (fun (left: obj) ->
    box (fun (right: obj) -> box (unbox<int> left + unbox<int> right)))
let partial = apply boxedCurried (box 10)
check "boxed function return" (partial :? (obj -> obj))
expectInt "partial application" 42 (apply partial (box 32))
expectInt "partial application keeps capture" 15 (apply partial (box 5))
let unboxedCurried = box (fun (left: obj) -> fun (right: obj) ->
    box (unbox<int> left + unbox<int> right))
check "unboxed function return requires fallback" (not (unboxedCurried :? (obj -> obj)))
expectInt "unboxed function return" 42 (apply (apply unboxedCurried (box 10)) (box 32))

let delegateValue = box (Func<int, int>(fun value -> value + 1))
expectInt ".NET delegate fallback" 42 (apply delegateValue (box 41))
expectInt "generated typed FFI wrapper" 42 (apply RuntimeFixture_increment (box 41))
expectInt "generated curried FFI wrapper" 42 (apply (apply RuntimeFixture_sum (box 10)) (box 32))
expectInt "FFI unboxed function return" 42 (apply (apply RuntimeFixture_returnFunction (box 10)) (box 32))

let failure = InvalidOperationException("target failure")
let throwing = box (fun (_: obj) -> raise failure : obj)
expectChain "direct exception wrapper" 1 failure (fun () -> apply throwing null)
let typedThrowing = box (fun (_: int) -> raise failure : int)
expectChain "fallback exception wrapper" 1 failure (fun () -> apply typedThrowing (box 0))
let nested = box (fun (_: obj) -> apply throwing null)
expectChain "nested exception wrappers" 2 failure (fun () -> apply nested null)

let nullError = captured (fun () -> apply null null)
check "null guard remains outside invocation wrapping"
    (nullError.GetType() = typeof<Exception>
     && nullError.Message = "sharpurs_apply: func is null!")

let mutable effects = 0
let effect = box (fun (_: obj) -> effects <- effects + 1; box 42)
expectInt "effect result" 42 (apply effect null)
check "effect runs once" (effects = 1)

let recover = box (fun (ex: obj) -> box (fun (_: obj) -> ex))
let caught handler action = apply (apply Effect_Exception_catchException handler) action
let failingEffect = apply Effect_Exception_throwException (box failure)
check "catchException sees original effect failure"
    (Object.ReferenceEquals(apply (caught recover failingEffect) null, failure))
let wrappedEffect = box (fun (_: obj) -> apply failingEffect null)
let recoveredWrapper = apply (caught recover wrappedEffect) null
check "catchException preserves invocation wrapper"
    (match recoveredWrapper with
     | :? TargetInvocationException as ex -> Object.ReferenceEquals(ex.InnerException, failure)
     | _ -> false)
let handlerFailure = ArgumentException("handler failure")
let throwingHandler = box (fun (_: obj) -> box (fun (_: obj) -> raise handlerFailure : obj))
expectChain "handler failure escapes catchException" 1 handlerFailure
    (fun () -> apply (caught throwingHandler failingEffect) null)

printfn "runtime-apply: %d checks passed" checks
