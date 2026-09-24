[<AutoOpen>]
module PureScript_Control_Alt

open System
open System.Collections.Generic

let Control_Alt_Altusd_Dict  = (fun (x: obj) -> x)

let Control_Alt_altArray  = (sharpurs_apply (box (Control_Alt_Altusd_Dict)) (box ((Map.add "alt" (box ((sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Semigroup_semigroupArray))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Functor_functorArray))) Map.empty)))))

let Control_Alt_alt  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "alt" (unbox<Map<string, obj>> (v)))))))
