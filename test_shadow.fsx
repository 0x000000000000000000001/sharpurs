[<RequireQualifiedAccess>]
type P = | P of obj * obj
let P = fun x y -> box (P.P(x,y))
let test = P.P(box 1, box 2)
printfn "%A" test
