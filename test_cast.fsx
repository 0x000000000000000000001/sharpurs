type Ordering = | LT | GT | EQ

let v: obj = box GT

let res = 
    match unbox v with
    | GT -> true
    | _ -> false

printfn "Match Result: %A" res
