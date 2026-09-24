[<AutoOpen>]
module PureScript_Data_Profunctor_Closed

open System
open System.Collections.Generic

let Data_Profunctor_Closed_Closedusd_Dict  = (fun (x: obj) -> x)

let Data_Profunctor_Closed_closedFunction  = (sharpurs_apply (box (Data_Profunctor_Closed_Closedusd_Dict)) (box ((Map.add "closed" (box ((sharpurs_apply (box (Control_Semigroupoid_compose)) (box (Control_Semigroupoid_semigroupoidFn))))) (Map.add "Profunctor0" (box ((fun (_: obj) -> Data_Profunctor_profunctorFn))) Map.empty)))))

let Data_Profunctor_Closed_closed  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "closed" (unbox<Map<string, obj>> (v)))))))
