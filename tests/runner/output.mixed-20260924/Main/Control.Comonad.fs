[<AutoOpen>]
module PureScript_Control_Comonad

open System
open System.Collections.Generic

let Control_Comonad_Comonadusd_Dict  = (fun (x: obj) -> x)

let Control_Comonad_extract  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "extract" (unbox<Map<string, obj>> (v)))))))
