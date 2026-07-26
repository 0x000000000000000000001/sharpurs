let defer = box (fun (thunk: obj) ->
    let mutable v = null
    let mutable hasRun = false
    let mutable t = thunk
    box (fun (_: obj) ->
        if not hasRun then
            v <- sharpurs_apply t (box ())
            hasRun <- true
            t <- null
        v
    )
)

let force = box (fun (l: obj) -> sharpurs_apply l (box ()))
