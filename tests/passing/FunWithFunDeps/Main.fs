let fnil = box ([] : obj list)

let fcons = box (fun (hd: obj) -> box (fun (tl: obj) ->
    box (hd :: unbox<obj list> tl)
))

let fappendImpl = box (fun (left: obj) -> box (fun (right: obj) ->
    box (unbox<obj list> left @ unbox<obj list> right)
))

// The outer vector holds boxed vectors as plain elements.
let fflattenImpl = box (fun (v: obj) ->
    box (List.collect (fun item -> unbox<obj list> item) (unbox<obj list> v))
)

let ftoArray = box (fun (v: obj) ->
    box (List.toArray (unbox<obj list> v))
)
