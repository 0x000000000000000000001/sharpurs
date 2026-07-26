let unsafePerformEffect = box (fun (fObj: obj) ->
    let f = unbox<obj -> obj> fObj
    f (box null)
)
