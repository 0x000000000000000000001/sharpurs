let mapWithIndexArray = box (fun (f: obj) -> box (fun (xs: obj) ->
    let arr = unbox<obj[]> xs
    let res = Array.mapi (fun i x -> sharpurs_apply (sharpurs_apply f (box i)) x) arr
    box res
))
