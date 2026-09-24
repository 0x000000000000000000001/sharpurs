[<AutoOpen>]
module PureScript_Foreign_Object_Unsafe

open System
open System.Collections.Generic

module Foreign_Object_Unsafe_FFI =
    let unsafeIndex = fun (m: obj) -> fun (k: obj) ->
        let dict = unbox<Map<string, obj>> m
        let key = unbox<string> k
        box dict.[key]
    

let Foreign_Object_Unsafe_unsafeIndex = box (Foreign_Object_Unsafe_FFI.``unsafeIndex``)



