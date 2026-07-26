let peekImpl = box (fun (i: obj) -> box (fun (xs: obj) ->
    let arr = unbox<System.Collections.Generic.List<obj>> xs
    let idx = unbox<int> i
    arr.[idx]
))

let pokeImpl = box (fun (i: obj) -> box (fun (a: obj) -> box (fun (xs: obj) ->
    let arr = unbox<System.Collections.Generic.List<obj>> xs
    let idx = unbox<int> i
    arr.[idx] <- a
    box ()
)))
