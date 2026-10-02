module App.Entrée

let main : obj =
    box (fun (_: obj) ->
        printfn "cli runtime: selected 42"
        box ())
