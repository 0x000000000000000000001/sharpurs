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
