[<AutoOpen>]
module PureScript_Data_Profunctor_Cochoice

open System
open System.Collections.Generic

let Data_Profunctor_Cochoice_Cochoiceusd_Dict  = (fun (x: obj) -> x)

let Data_Profunctor_Cochoice_unright  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "unright" (unbox<Map<string, obj>> (v)))))))

let Data_Profunctor_Cochoice_unleft  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "unleft" (unbox<Map<string, obj>> (v)))))))
