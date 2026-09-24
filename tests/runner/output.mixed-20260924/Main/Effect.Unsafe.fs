[<AutoOpen>]
module PureScript_Effect_Unsafe

open System
open System.Collections.Generic

module Effect_Unsafe_FFI =
    let unsafePerformEffect = 
        fun (fVal: obj) ->
            let f = fVal :?> (obj -> obj)
            f null
    

let Effect_Unsafe_unsafePerformEffect = box (Effect_Unsafe_FFI.``unsafePerformEffect``)



