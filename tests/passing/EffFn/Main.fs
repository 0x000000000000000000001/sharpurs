let add3 = box (fun (a: obj) -> box (fun (b: obj) -> box (fun (c: obj) ->
    box (unbox<string> a + unbox<string> b + unbox<string> c)
)))
