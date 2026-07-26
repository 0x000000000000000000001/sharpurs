let map_ = box (fun (f: obj) -> box (fun (a: obj) -> box (fun (usd___unused: obj) ->
    let res = sharpurs_apply a (box ())
    sharpurs_apply f res
)))

let bind_ = box (fun (a: obj) -> box (fun (f: obj) -> box (fun (usd___unused: obj) ->
    let res = sharpurs_apply a (box ())
    let fRes = sharpurs_apply f res
    sharpurs_apply fRes (box ())
)))

let run = box (fun (f: obj) -> sharpurs_apply f (box ()))

let ``new`` = box (fun (val_: obj) -> box (fun (usd___unused: obj) -> box (ref val_)))

let read = box (fun (r: obj) -> box (fun (usd___unused: obj) -> (unbox<obj ref> r).Value))

let modifyImpl = box (fun (f: obj) -> box (fun (r: obj) -> box (fun (usd___unused: obj) ->
    let rRef = unbox<obj ref> r
    let t = sharpurs_apply f rRef.Value
    let state = Map.find "state" (unbox<Map<string, obj>> t)
    let value = Map.find "value" (unbox<Map<string, obj>> t)
    rRef.Value <- state
    value
)))

let write = box (fun (a: obj) -> box (fun (r: obj) -> box (fun (usd___unused: obj) ->
    let rRef = unbox<obj ref> r
    rRef.Value <- a
    a
)))

let pure_ = box (fun (a: obj) -> box (fun (usd___unused: obj) -> a))

let ``while`` = box (fun (f: obj) -> box (fun (a: obj) -> box (fun (usd___unused: obj) ->
    let rec loop () =
        if unbox<bool> (sharpurs_apply f (box ())) then
            sharpurs_apply a (box ()) |> ignore
            loop ()
        else ()
    loop ()
    box ()
)))

let ``for`` = box (fun (lo: obj) -> box (fun (hi: obj) -> box (fun (f: obj) -> box (fun (usd___unused: obj) ->
    let low = unbox<int> lo
    let high = unbox<int> hi
    for i = low to high - 1 do
        sharpurs_apply f (box i) |> fun a -> sharpurs_apply a (box ()) |> ignore
    box ()
))))

let foreach = box (fun (arr: obj) -> box (fun (f: obj) -> box (fun (usd___unused: obj) ->
    let a = unbox<obj[]> arr
    for i = 0 to a.Length - 1 do
        sharpurs_apply f a.[i] |> fun act -> sharpurs_apply act (box ()) |> ignore
    box ()
)))
