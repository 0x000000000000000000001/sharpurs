open System
open System.IO
let lines = File.ReadAllLines("output/Main/Program.fsx")
let mutable out = []
for i in 0 .. lines.Length - 1 do
    let line = lines.[i]
    if line.StartsWith("let ") && not (line.Contains("=")) then
        // Multiline let ?
        out <- line :: out
    elif line.StartsWith("let ") && line.Contains("=") && not (line.StartsWith("let (|")) && not (line.StartsWith("let objMap")) && not (line.StartsWith("let unbox")) && not (line.StartsWith("let undefined")) && not (line.StartsWith("let Prim_")) && not (line.StartsWith("let intMod")) && not (line.StartsWith("let semiringInt")) && not (line.StartsWith("let sharpurs_apply")) then
        let name = line.Split(' ').[1]
        out <- sprintf "printfn \"Init: %s\"" name :: line :: out
    else
        out <- line :: out

File.WriteAllLines("output/Main/Program_Trace.fsx", List.rev out |> List.toArray)
