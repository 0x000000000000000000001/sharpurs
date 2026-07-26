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
