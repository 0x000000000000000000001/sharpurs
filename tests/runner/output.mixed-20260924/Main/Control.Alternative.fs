[<AutoOpen>]
module PureScript_Control_Alternative

open System
open System.Collections.Generic

let Control_Alternative_Alternativeusd_Dict  = (fun (x: obj) -> x)

let Control_Alternative_guard  = (fun (dictAlternative: obj) -> (let pure_ = (sharpurs_apply (box (Control_Applicative_pure)) (box ((sharpurs_apply (box ((Map.find "Applicative0" (unbox<Map<string, obj>> (dictAlternative))))) (box (Prim_undefined)))))) in let empty = (sharpurs_apply (box (Control_Plus_empty)) (box ((sharpurs_apply (box ((Map.find "Plus1" (unbox<Map<string, obj>> (dictAlternative))))) (box (Prim_undefined)))))) in (fun (v: obj) -> (match ((unbox (v))) with | LitBool true () -> (box ((sharpurs_apply (box (pure_)) (box (Data_Unit_unit))))) | LitBool false () -> (box (empty))))))

let Control_Alternative_alternativeArray  = (sharpurs_apply (box (Control_Alternative_Alternativeusd_Dict)) (box ((Map.add "Applicative0" (box ((fun (_: obj) -> Control_Applicative_applicativeArray))) (Map.add "Plus1" (box ((fun (_: obj) -> Control_Plus_plusArray))) Map.empty)))))
