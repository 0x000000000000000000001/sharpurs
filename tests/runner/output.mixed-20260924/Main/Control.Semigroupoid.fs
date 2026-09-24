[<AutoOpen>]
module PureScript_Control_Semigroupoid

open System
open System.Collections.Generic

let Control_Semigroupoid_Semigroupoidusd_Dict  = (fun (x: obj) -> x)

let Control_Semigroupoid_semigroupoidFn  = (sharpurs_apply (box (Control_Semigroupoid_Semigroupoidusd_Dict)) (box ((Map.add "compose" (box ((fun (f: obj) -> (fun (g: obj) -> (fun (x: obj) -> (sharpurs_apply (box (f)) (box ((sharpurs_apply (box (g)) (box (x))))))))))) Map.empty))))

let Control_Semigroupoid_compose  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "compose" (unbox<Map<string, obj>> (v)))))))

let Control_Semigroupoid_composeFlipped  = (fun (dictSemigroupoid: obj) -> (let compose1 = (sharpurs_apply (box (Control_Semigroupoid_compose)) (box (dictSemigroupoid))) in (fun (f: obj) -> (fun (g: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (compose1)) (box (g))))) (box (f)))))))
