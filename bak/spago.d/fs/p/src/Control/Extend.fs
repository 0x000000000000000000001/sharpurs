let arrayExtend (f: obj) =
    let f' = f :?> (obj -> obj)
    fun (xs: obj) ->
        let arr = xs :?> obj[]
        let res = Array.zeroCreate arr.Length
        for i = 0 to arr.Length - 1 do
            res.[i] <- f' (arr.[i..] :> obj)
        res :> obj
