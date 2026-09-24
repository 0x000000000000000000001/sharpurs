[<AutoOpen>]
module PureScript_Control_Plus

open System
open System.Collections.Generic

let Control_Plus_Plususd_Dict  = (fun (x: obj) -> x)

let Control_Plus_plusArray  = (sharpurs_apply (box (Control_Plus_Plususd_Dict)) (box ((Map.add "empty" (box ((box [||]))) (Map.add "Alt0" (box ((fun (_: obj) -> Control_Alt_altArray))) Map.empty)))))

let Control_Plus_empty  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "empty" (unbox<Map<string, obj>> (v)))))))
