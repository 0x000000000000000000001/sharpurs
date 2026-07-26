let x = (let a = 1 in let mutable b : obj = null in b <- box 2; b)
printfn "%A" x
