let fromNumberImpl = box (fun (just: obj) -> box (fun (nothing: obj) -> box (fun (n: obj) ->
    let v = unbox<float> n
    if v = System.Math.Truncate(v) && v >= float System.Int32.MinValue && v <= float System.Int32.MaxValue then
        sharpurs_apply just (box (int v))
    else
        nothing
)))

let toNumber = box (fun (n: obj) -> box (float (unbox<int> n)))

let fromStringAsImpl = box (fun (just: obj) -> box (fun (nothing: obj) -> box (fun (radix: obj) -> box (fun (s: obj) ->
    let r = unbox<int> radix
    let str = unbox<string> s
    try
        let v = System.Convert.ToInt32(str, r)
        sharpurs_apply just (box v)
    with
    | _ -> nothing
))))

let quot = box (fun (x: obj) -> box (fun (y: obj) -> box ((unbox<int> x) / (unbox<int> y))))

let rem = box (fun (x: obj) -> box (fun (y: obj) -> box ((unbox<int> x) % (unbox<int> y))))

let pow = box (fun (x: obj) -> box (fun (y: obj) -> box (int (System.Math.Pow(float (unbox<int> x), float (unbox<int> y))))))

let toStringAs = box (fun (radix: obj) -> box (fun (i: obj) ->
    let r = unbox<int> radix
    let iv = unbox<int> i
    if r = 10 then box (string iv)
    elif r = 2 || r = 8 || r = 16 then box (System.Convert.ToString(iv, r))
    else
        if iv = 0 then box "0"
        else
            let mutable isNeg = false
            let mutable v = 0u
            if iv < 0 then
                isNeg <- true
                v <- uint32 (-int64 iv)
            else
                v <- uint32 iv
            let chars = "0123456789abcdefghijklmnopqrstuvwxyz"
            let mutable res = ""
            let ur = uint32 r
            while v > 0u do
                let rem = v % ur
                res <- string chars.[int rem] + res
                v <- v / ur
            if isNeg then box ("-" + res) else box res
))

