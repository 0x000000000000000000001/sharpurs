[<AutoOpen>]
module PureScript_Foreign_Object_ST

open System
open System.Collections.Generic

module Foreign_Object_ST_FFI =
    let ``new`` = box (fun (usd___unused: obj) ->
        box (new System.Collections.Generic.Dictionary<string, obj>())
    )
    
    let peekImpl = box (fun (just: obj) -> box (fun (nothing: obj) -> box (fun (kObj: obj) -> box (fun (mObj: obj) -> box (fun (usd___unused: obj) ->
        let k = unbox<string> kObj
        let m = unbox<System.Collections.Generic.Dictionary<string, obj>> mObj
        let found, v = m.TryGetValue(k)
        if found then
            (unbox<obj -> obj> just) v
        else
            nothing
    )))))
    
    let poke = box (fun (kObj: obj) -> box (fun (vObj: obj) -> box (fun (mObj: obj) -> box (fun (usd___unused: obj) ->
        let k = unbox<string> kObj
        let m = unbox<System.Collections.Generic.Dictionary<string, obj>> mObj
        m.[k] <- vObj
        box m
    ))))
    
    let ``delete`` = box (fun (kObj: obj) -> box (fun (mObj: obj) -> box (fun (usd___unused: obj) ->
        let k = unbox<string> kObj
        let m = unbox<System.Collections.Generic.Dictionary<string, obj>> mObj
        m.Remove(k) |> ignore
        box m
    )))
    

let Foreign_Object_ST_new = box Foreign_Object_ST_FFI.``new``
let Foreign_Object_ST_peekImpl = box Foreign_Object_ST_FFI.``peekImpl``
let Foreign_Object_ST_poke = box Foreign_Object_ST_FFI.``poke``
let Foreign_Object_ST_delete = box Foreign_Object_ST_FFI.``delete``


let Foreign_Object_ST_peek  = (sharpurs_apply (box ((sharpurs_apply (box (Foreign_Object_ST_peekImpl)) (box ((fun (usd__arg1: obj) -> (box (Data_Maybe_Justusd_Ctor(usd__arg1))))))))) (box ((box Data_Maybe_Nothingusd_Ctor))))
