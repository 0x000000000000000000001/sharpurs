[<AutoOpen>]
module PureScript_Data_Either_Inject

open System
open System.Collections.Generic

let Data_Either_Inject_compose  = (sharpurs_apply (box (Control_Semigroupoid_compose)) (box (Control_Semigroupoid_semigroupoidFn)))

let Data_Either_Inject_Injectusd_Dict  = (fun (x: obj) -> x)

let Data_Either_Inject_prj  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "prj" (unbox<Map<string, obj>> (v)))))))

let Data_Either_Inject_injectReflexive  = (sharpurs_apply (box (Data_Either_Inject_Injectusd_Dict)) (box ((Map.add "inj" (box ((sharpurs_apply (box (Control_Category_identity)) (box (Control_Category_categoryFn))))) (Map.add "prj" (box ((fun (usd__arg1: obj) -> (box (Data_Maybe_Justusd_Ctor(usd__arg1)))))) Map.empty)))))

let Data_Either_Inject_injectLeft  = (sharpurs_apply (box (Data_Either_Inject_Injectusd_Dict)) (box ((Map.add "inj" (box ((fun (usd__arg1: obj) -> (box (Data_Either_Leftusd_Ctor(usd__arg1)))))) (Map.add "prj" (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Either_either)) (box ((fun (usd__arg1: obj) -> (box (Data_Maybe_Justusd_Ctor(usd__arg1))))))))) (box ((sharpurs_apply (box (Data_Function_const)) (box ((box Data_Maybe_Nothingusd_Ctor))))))))) Map.empty)))))

let Data_Either_Inject_inj  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "inj" (unbox<Map<string, obj>> (v)))))))

let Data_Either_Inject_injectRight  = (fun (dictInject: obj) -> (sharpurs_apply (box (Data_Either_Inject_Injectusd_Dict)) (box ((Map.add "inj" (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Either_Inject_compose)) (box ((fun (usd__arg1: obj) -> (box (Data_Either_Rightusd_Ctor(usd__arg1))))))))) (box ((sharpurs_apply (box (Data_Either_Inject_inj)) (box (dictInject)))))))) (Map.add "prj" (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Either_either)) (box ((sharpurs_apply (box (Data_Function_const)) (box ((box Data_Maybe_Nothingusd_Ctor))))))))) (box ((sharpurs_apply (box (Data_Either_Inject_prj)) (box (dictInject)))))))) Map.empty))))))
