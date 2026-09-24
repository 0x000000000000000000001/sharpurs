[<AutoOpen>]
module PureScript_Control_Category

open System
open System.Collections.Generic

let Control_Category_Categoryusd_Dict  = (fun (x: obj) -> x)

let Control_Category_identity  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "identity" (unbox<Map<string, obj>> (v)))))))

let Control_Category_categoryFn  = (sharpurs_apply (box (Control_Category_Categoryusd_Dict)) (box ((Map.add "identity" (box ((fun (x: obj) -> x))) (Map.add "Semigroupoid0" (box ((fun (_: obj) -> Control_Semigroupoid_semigroupoidFn))) Map.empty)))))
