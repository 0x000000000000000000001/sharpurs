[<AutoOpen>]
module PureScript_Partial_Unsafe

open System
open System.Collections.Generic

module Partial_Unsafe_FFI =
    let _unsafePartial f = (unbox<obj -> obj> f) (box null)
    

let Partial_Unsafe__unsafePartial = box (fun (arg0: obj) -> box (Partial_Unsafe_FFI.``_unsafePartial`` (unbox arg0)))


let Partial_Unsafe_crashWith  = (sharpurs_apply (box (Partial_crashWith)) (box (Prim_undefined)))

let Partial_Unsafe_unsafePartial  = Partial_Unsafe__unsafePartial

let Partial_Unsafe_unsafeCrashWith  = (fun (msg: obj) -> (sharpurs_apply (box (Partial_Unsafe_unsafePartial)) (box ((fun (_: obj) -> (sharpurs_apply (box (Partial_Unsafe_crashWith)) (box (msg))))))))
