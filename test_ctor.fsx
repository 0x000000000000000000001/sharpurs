type Maybe = | Just of obj | Nothing
let Data_Maybe_Just = Just
let x = Data_Maybe_Just (box 5)
printfn "%A" x
