let unfoldr1ArrayImpl = box (fun (isNothing: obj) -> box (fun (fromJust: obj) -> box (fun (fst: obj) -> box (fun (snd: obj) -> box (fun (f: obj) -> box (fun (b: obj) ->
    let result = System.Collections.Generic.List<obj>()
    let rec loop value =
        let tuple = sharpurs_apply f value
        result.Add(sharpurs_apply fst tuple)
        let maybe = sharpurs_apply snd tuple
        if unbox<bool> (sharpurs_apply isNothing maybe) then
            result.ToArray() |> box
        else
            loop (sharpurs_apply fromJust maybe)
    loop b
))))))
