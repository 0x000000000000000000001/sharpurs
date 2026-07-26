let charAt = box (fun (i: obj) -> box (fun (s: obj) ->
    let idx = unbox<int> i
    let str = unbox<string> s
    if idx >= 0 && idx < str.Length then box str.[idx]
    else failwith "Data.String.Unsafe.charAt: Invalid index."
))

let char = box (fun (s: obj) ->
    let str = unbox<string> s
    if str.Length = 1 then box str.[0]
    else failwith "Data.String.Unsafe.char: Expected string of length 1."
)
