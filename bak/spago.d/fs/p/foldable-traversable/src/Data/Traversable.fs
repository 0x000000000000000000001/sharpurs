let traverseArrayImpl = box (fun (apply: obj) -> box (fun (map_: obj) -> box (fun (pure_: obj) -> box (fun (f: obj) -> box (fun (xs: obj) ->
    let arr = unbox<obj[]> xs
    if arr.Length = 0 then
        sharpurs_apply pure_ (box [||])
    else
        let rec go i =
            if i = arr.Length - 1 then
                sharpurs_apply map_ (box (fun x -> box [| unbox x |])) |> fun m -> sharpurs_apply m (sharpurs_apply f arr.[i])
            else
                let rest = go (i + 1)
                let m = sharpurs_apply map_ (box (fun x -> box (fun y -> box (Array.append [| unbox x |] (unbox<obj[]> y)))))
                let applyFirst = sharpurs_apply m (sharpurs_apply f arr.[i])
                sharpurs_apply apply applyFirst |> fun a -> sharpurs_apply a rest
        go 0
)))))
