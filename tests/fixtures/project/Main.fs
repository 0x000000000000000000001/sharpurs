module Main

let verify (value: int) : obj =
    box (fun (_: obj) ->
        if value <> 42 then failwithf "project result: expected 42, got %d" value
        if Application.value <> value then failwith "explicit application reference"
        printfn "project runtime: 42"
        box ())
