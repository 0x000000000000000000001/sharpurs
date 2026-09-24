let unsafeGet = box (fun (key: obj) -> box (fun (record: obj) ->
    box (Map.find (unbox<string> key) (unbox<Map<string, obj>> record))
))

let unsafeSet = box (fun (key: obj) -> box (fun (value: obj) -> box (fun (record: obj) ->
    box (Map.add (unbox<string> key) value (unbox<Map<string, obj>> record))
)))
