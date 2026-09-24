[<AutoOpen>]
module PureScript_Effect_Ref

open System
open System.Collections.Generic

module Effect_Ref_FFI =
    let _new s = ref s
    
    let newWithSelf f =
        let r = ref (box null)
        let s = (unbox (f (box r)))
        r := s
        box r
    
    let read r = !(unbox<obj ref> r)
    
    let write s r =
        let rRef = unbox<obj ref> r
        rRef := s
    
    let modifyImpl f r =
        let rRef = unbox<obj ref> r
        let result = unbox<Map<string, obj>> ((unbox<obj -> obj> f) (!rRef))
        rRef := Map.find "state" result
        Map.find "value" result
    

let Effect_Ref__new = box (fun (arg0: obj) -> box (Effect_Ref_FFI.``_new`` (unbox arg0)))
let Effect_Ref_newWithSelf = box (fun (arg0: obj) -> box (Effect_Ref_FFI.``newWithSelf`` (unbox arg0)))
let Effect_Ref_read = box (fun (arg0: obj) -> box (Effect_Ref_FFI.``read`` (unbox arg0)))
let Effect_Ref_modifyImpl = box (fun (arg0: obj) -> box (fun (arg1: obj) -> box (Effect_Ref_FFI.``modifyImpl`` (unbox arg0) (unbox arg1))))
let Effect_Ref_write = box (fun (arg0: obj) -> box (fun (arg1: obj) -> box (Effect_Ref_FFI.``write`` (unbox arg0) (unbox arg1))))


let Effect_Ref_void  = (sharpurs_apply (box (Data_Functor_void)) (box (Effect_functorEffect)))

let Effect_Ref_new  = Effect_Ref__new

let Effect_Ref_modify_prime  = Effect_Ref_modifyImpl

let Effect_Ref_modify  = (fun (f: obj) -> (sharpurs_apply (box (Effect_Ref_modify_prime)) (box ((fun (s: obj) -> (let s_prime = (sharpurs_apply (box (f)) (box (s))) in (Map.add "state" (box (s_prime)) (Map.add "value" (box (s_prime)) Map.empty))))))))

let Effect_Ref_modify_  = (fun (f: obj) -> (fun (s: obj) -> (sharpurs_apply (box (Effect_Ref_void)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Effect_Ref_modify)) (box (f))))) (box (s))))))))
