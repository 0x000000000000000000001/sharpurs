let foldr1Impl = box (fun (f: obj) -> box (fun (xs: obj) ->
    let arr = unbox<obj[]> xs
    let mutable acc = arr.[arr.Length - 1]
    for i = arr.Length - 2 downto 0 do
        acc <- sharpurs_apply (sharpurs_apply f arr.[i]) acc
    acc
))

let foldl1Impl = box (fun (f: obj) -> box (fun (xs: obj) ->
    let arr = unbox<obj[]> xs
    let mutable acc = arr.[0]
    for i = 1 to arr.Length - 1 do
        acc <- sharpurs_apply (sharpurs_apply f acc) arr.[i]
    acc
))

let traverse1Impl = box (fun (apply: obj) -> box (fun (map_: obj) -> box (fun (f: obj) -> box (fun (xs: obj) ->
    let arr = unbox<obj[]> xs
    let mutable res = sharpurs_apply map_ (box (fun x -> box [| unbox x |])) |> fun m -> sharpurs_apply m (sharpurs_apply f arr.[0])
    for i = 1 to arr.Length - 1 do
        let next = sharpurs_apply f arr.[i]
        let m = sharpurs_apply map_ (box (fun x -> box (fun y -> box (Array.append (unbox<obj[]> x) [| unbox y |]))))
        let mapRes = sharpurs_apply m res
        res <- sharpurs_apply apply mapRes |> fun a -> sharpurs_apply a next
    res
))))
