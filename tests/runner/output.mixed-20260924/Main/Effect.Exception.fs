[<AutoOpen>]
module PureScript_Effect_Exception

open System
open System.Collections.Generic

module Effect_Exception_FFI =
    type NamedException(name: string, msg: string) =
        inherit System.Exception(msg)
        member this.ErrorName = name
    
    let showErrorImpl = box (fun (errObj: obj) ->
        let err = unbox<System.Exception> errObj
        let stack = if err.StackTrace <> null then err.StackTrace else err.ToString()
        box stack
    )
    
    let error = box (fun (msgObj: obj) ->
        let msg = unbox<string> msgObj
        box (System.Exception(msg))
    )
    
    let errorWithCause = box (fun (msgObj: obj) -> box (fun (causeObj: obj) ->
        let msg = unbox<string> msgObj
        let cause = unbox<System.Exception> causeObj
        box (System.Exception(msg, cause))
    ))
    
    let errorWithName = box (fun (msgObj: obj) -> box (fun (nameObj: obj) ->
        let msg = unbox<string> msgObj
        let name = unbox<string> nameObj
        box (NamedException(name, msg)) :> obj
    ))
    
    let message = box (fun (eObj: obj) ->
        let e = unbox<System.Exception> eObj
        box e.Message
    )
    
    let name = box (fun (eObj: obj) ->
        let e = unbox<System.Exception> eObj
        match e with
        | :? NamedException as ne -> box ne.ErrorName
        | _ -> box (e.GetType().Name)
    )
    
    let stackImpl = box (fun (just: obj) -> box (fun (nothing: obj) -> box (fun (eObj: obj) ->
        let e = unbox<System.Exception> eObj
        if e.StackTrace <> null then
            (unbox<obj -> obj> just) (box e.StackTrace)
        else
            nothing
    )))
    
    let throwException = box (fun (eObj: obj) -> box (fun (usd___unused: obj) ->
        let e = unbox<System.Exception> eObj
        raise e
        box () 
    ))
    
    let catchException = box (fun (cObj: obj) -> box (fun (tObj: obj) -> box (fun (usd___unused: obj) ->
        try
            let t = unbox<obj -> obj> tObj
            t (box null)
        with
        | :? System.Exception as e ->
            let c = unbox<obj -> obj> cObj
            let cApp = unbox<obj -> obj> (c (box e))
            cApp (box null)
        | e ->
            let c = unbox<obj -> obj> cObj
            let cApp = unbox<obj -> obj> (c (box (System.Exception(e.ToString()))))
            cApp (box null)
    )))
    

let Effect_Exception_showErrorImpl = box Effect_Exception_FFI.``showErrorImpl``
let Effect_Exception_error = box Effect_Exception_FFI.``error``
let Effect_Exception_errorWithCause = box Effect_Exception_FFI.``errorWithCause``
let Effect_Exception_errorWithName = box Effect_Exception_FFI.``errorWithName``
let Effect_Exception_message = box Effect_Exception_FFI.``message``
let Effect_Exception_name = box Effect_Exception_FFI.``name``
let Effect_Exception_stackImpl = box Effect_Exception_FFI.``stackImpl``
let Effect_Exception_throwException = box Effect_Exception_FFI.``throwException``
let Effect_Exception_catchException = box Effect_Exception_FFI.``catchException``


let Effect_Exception_compose  = (sharpurs_apply (box (Control_Semigroupoid_compose)) (box (Control_Semigroupoid_semigroupoidFn)))

let Effect_Exception_pure  = (sharpurs_apply (box (Control_Applicative_pure)) (box (Effect_applicativeEffect)))

let Effect_Exception_map  = (sharpurs_apply (box (Data_Functor_map)) (box (Effect_functorEffect)))

let Effect_Exception_try  = (fun (action: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Effect_Exception_catchException)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Effect_Exception_compose)) (box (Effect_Exception_pure))))) (box ((fun (usd__arg1: obj) -> (box (Data_Either_Leftusd_Ctor(usd__arg1)))))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Effect_Exception_map)) (box ((fun (usd__arg1: obj) -> (box (Data_Either_Rightusd_Ctor(usd__arg1))))))))) (box (action)))))))

let Effect_Exception_throw  = (sharpurs_apply (box ((sharpurs_apply (box (Effect_Exception_compose)) (box (Effect_Exception_throwException))))) (box (Effect_Exception_error)))

let Effect_Exception_stack  = (sharpurs_apply (box ((sharpurs_apply (box (Effect_Exception_stackImpl)) (box ((fun (usd__arg1: obj) -> (box (Data_Maybe_Justusd_Ctor(usd__arg1))))))))) (box ((box Data_Maybe_Nothingusd_Ctor))))

let Effect_Exception_showError  = (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box (Effect_Exception_showErrorImpl)) Map.empty))))
