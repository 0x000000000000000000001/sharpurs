[<AutoOpen>]
module PureScript_Partial

open System
open System.Collections.Generic

module Partial_FFI =
    let _crashWith msg = failwith (unbox<string> msg)
    

let Partial__crashWith = box (fun (arg0: obj) -> box (Partial_FFI.``_crashWith`` (unbox arg0)))


let Partial_crashWith  = (fun (_: obj) -> Partial__crashWith)

let Partial_crashWith1  = (sharpurs_apply (box (Partial_crashWith)) (box (Prim_undefined)))

let Partial_crash  = (fun (_: obj) -> (sharpurs_apply (box (Partial_crashWith1)) (box ((box "Partial.crash: partial function")))))
