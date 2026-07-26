let foldrArray = box (fun (f: obj) -> box (fun (init: obj) -> box (fun (xs: obj) ->
    let arr = unbox<obj[]> xs
    box (Array.foldBack (fun a b -> unbox<obj> (sharpurs_apply (sharpurs_apply f a) b)) arr init)
)))

let foldlArray = box (fun (f: obj) -> box (fun (init: obj) -> box (fun (xs: obj) ->
    let arr = unbox<obj[]> xs
    box (Array.fold (fun b a -> unbox<obj> (sharpurs_apply (sharpurs_apply f b) a)) init arr)
)))
