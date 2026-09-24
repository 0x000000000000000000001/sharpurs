[<AutoOpen>]
module PureScript_Data_String_Unsafe

open System
open System.Collections.Generic

module Data_String_Unsafe_FFI =
    let charAt = box (fun (i: obj) -> box (fun (s: obj) ->
        let idx = unbox<int> i
        let str = unbox<string> s
        if idx >= 0 && idx < str.Length then box str.[idx]
        else failwith "Data.String.Unsafe.charAt: Invalid index."
    ))
    
    let char = box (fun (s: obj) ->
        let str = unbox<string> s
        if str.Length = 1 then box str.[0]
        else failwith "Data.String.Unsafe.char: Expected string of length 1."
    )
    

let Data_String_Unsafe_char = box (Data_String_Unsafe_FFI.``char``)
let Data_String_Unsafe_charAt = box (Data_String_Unsafe_FFI.``charAt``)



