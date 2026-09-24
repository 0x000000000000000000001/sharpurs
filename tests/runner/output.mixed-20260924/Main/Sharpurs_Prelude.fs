[<AutoOpen>]
module Sharpurs_Prelude

#nowarn "25"
#nowarn "46"
#nowarn "66"
#nowarn "67"
#nowarn "3370"

open System
open System.Collections.Generic

let (|LitBool|_|) (expected: bool) (value: obj) = if value :? bool && unbox value = expected then Some() else None
let (|LitInt|_|) (expected: int) (value: obj) = if value :? int && unbox value = expected then Some() else None
let (|LitNumber|_|) (expected: float) (value: obj) = if value :? float && unbox value = expected then Some() else None
let (|LitString|_|) (expected: string) (value: obj) = if value :? string && unbox value = expected then Some() else None
let (|LitChar|_|) (expected: char) (value: obj) = if value :? char && unbox value = expected then Some() else None
let (|HasProp|_|) (key: string) (value: obj) = if value :? Map<string, obj> then Map.tryFind key (unbox<Map<string, obj>> value) else None

module SharpursRuntime =
    let eventLoopWg = new System.Threading.CountdownEvent(1)
    let mutable loopHasTasks = false
    
    let EventLoopAdd (n: int) =
        loopHasTasks <- true
        eventLoopWg.AddCount(n)
        
    let EventLoopDone () =
        if eventLoopWg.CurrentCount > 0 then
            eventLoopWg.Signal() |> ignore
        
    let EventLoopWait () =
        if eventLoopWg.CurrentCount > 0 then
            eventLoopWg.Signal() |> ignore
        if loopHasTasks then
            eventLoopWg.Wait()


let objMap = Map.empty<string, obj>
let unbox<'a> (x: obj) : 'a = unbox x
let (|Unbox|) (x: obj) = unbox x

let undefined = Unchecked.defaultof<obj>
let Prim_undefined = undefined
let intMod a b = unbox<int> a % unbox<int> b
let semiringInt = 0

// PureScript's Euclidean Int modulo, including zero and Int32.MinValue / -1.
let sharpurs_int_mod (left: int) (right: int) : int =
    if right = 0 || right = -1 then 0
    else
        let remainder = left % right
        if remainder < 0 then
            if right > 0 then remainder + right else remainder - right
        else remainder

let sharpurs_apply (func: obj) (arg: obj) : obj =
    if isNull func then failwith "sharpurs_apply: func is null!"
    match func with
    | :? (obj -> obj) as invoke ->
        try invoke arg
        // Keep the exception boundary of MethodInfo.Invoke for callers and FFI.
        with ex -> raise (System.Reflection.TargetInvocationException(ex))
    | _ ->
        let method = func.GetType().GetMethods() |> Array.find (fun m -> m.Name = "Invoke" && m.GetParameters().Length = 1)
        method.Invoke(func, [| arg |])
