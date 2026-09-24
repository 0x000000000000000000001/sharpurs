module Sharpurs_EntryPoint

open System.Threading

[<EntryPoint>]
let main argv =
    let thread = Thread(ThreadStart(fun () ->
        (unbox<obj -> obj> Main_main) null |> ignore
    ), 1024 * 1024 * 1024)
    thread.Start()
    thread.Join()
    0
