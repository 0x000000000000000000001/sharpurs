let unfoldrArrayImpl = box (fun (isNothing: obj) -> box (fun (fromJust: obj) -> box (fun (fst: obj) -> box (fun (snd: obj) -> box (fun (f: obj) -> box (fun (b: obj) ->
    let result = System.Collections.Generic.List<obj>()
    let rec loop value =
        let maybe = sharpurs_apply f value
        if unbox<bool> (sharpurs_apply isNothing maybe) then
            result.ToArray() |> box
        else
            let tuple = sharpurs_apply fromJust maybe
            result.Add(sharpurs_apply fst tuple)
            loop (sharpurs_apply snd tuple)
    loop b
))))))
