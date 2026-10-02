module Main

let main : obj =
    box (fun (_: obj) ->
        printfn "cli runtime: Main"
        box ())
