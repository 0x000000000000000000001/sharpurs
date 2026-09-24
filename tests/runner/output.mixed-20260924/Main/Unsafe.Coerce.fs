[<AutoOpen>]
module PureScript_Unsafe_Coerce

open System
open System.Collections.Generic

module Unsafe_Coerce_FFI =
    let unsafeCoerce = box (fun (x: obj) -> x)
    

let Unsafe_Coerce_unsafeCoerce = box (Unsafe_Coerce_FFI.``unsafeCoerce``)



