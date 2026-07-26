let assertImpl = box (fun (messageObj: obj) -> box (fun (successObj: obj) -> box (fun (usd___unused: obj) ->
    let message = unbox<string> messageObj
    let success = unbox<bool> successObj
    if not success then
        failwith message
    box null
)))

let checkThrows = box (fun (fnObj: obj) -> box (fun (usd___unused: obj) ->
    let fn = unbox<obj -> obj> fnObj
    try
        fn (box null) |> ignore
        box false
    with
    | _ -> box true
))
