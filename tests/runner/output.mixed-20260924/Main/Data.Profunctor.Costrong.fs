[<AutoOpen>]
module PureScript_Data_Profunctor_Costrong

open System
open System.Collections.Generic

let Data_Profunctor_Costrong_Costrongusd_Dict  = (fun (x: obj) -> x)

let Data_Profunctor_Costrong_unsecond  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "unsecond" (unbox<Map<string, obj>> (v)))))))

let Data_Profunctor_Costrong_unfirst  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "unfirst" (unbox<Map<string, obj>> (v)))))))
