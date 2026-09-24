[<AutoOpen>]
module PureScript_Foreign_Object_ST_Unsafe

open System
open System.Collections.Generic

module Foreign_Object_ST_Unsafe_FFI =
    open System.Collections.Generic
    
    let unsafeIndex = fun (m: obj) -> fun (k: obj) ->
        box (fun () ->
            let dict = unbox<Dictionary<string, obj>> m
            let key = unbox<string> k
            box dict.[key]
        )
    
    let unsafeFreeze = fun (m: obj) ->
        box (fun () ->
            let dict = unbox<Dictionary<string, obj>> m
            let res = Dictionary<string, obj>()
            for kvp in dict do
                res.[kvp.Key] <- kvp.Value
            box (res |> Seq.map (fun kv -> kv.Key, kv.Value) |> Map.ofSeq)
        )
    

let Foreign_Object_ST_Unsafe_unsafeFreeze = box (Foreign_Object_ST_Unsafe_FFI.``unsafeFreeze``)



