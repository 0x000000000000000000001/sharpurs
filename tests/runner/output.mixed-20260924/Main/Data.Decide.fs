[<AutoOpen>]
module PureScript_Data_Decide

open System
open System.Collections.Generic

let Data_Decide_compose  = (sharpurs_apply (box (Control_Semigroupoid_compose)) (box (Control_Semigroupoid_semigroupoidFn)))

let Data_Decide_identity  = (sharpurs_apply (box (Control_Category_identity)) (box (Control_Category_categoryFn)))

let Data_Decide_Decideusd_Dict  = (fun (x: obj) -> x)

let Data_Decide_choosePredicate  = (sharpurs_apply (box (Data_Decide_Decideusd_Dict)) (box ((Map.add "choose" (box ((fun (f: obj) -> (fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (f)), (unbox (v)), (unbox (v1)))) with | (f1, g, h) -> (box ((sharpurs_apply (box (Data_Predicate_Predicate)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Decide_compose)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Either_either)) (box (g))))) (box (h)))))))) (box (f1)))))))))))))) (Map.add "Divide0" (box ((fun (_: obj) -> Data_Divide_dividePredicate))) Map.empty)))))

let Data_Decide_chooseOp  = (fun (dictSemigroup: obj) -> (let divideOp = (sharpurs_apply (box (Data_Divide_divideOp)) (box (dictSemigroup))) in (sharpurs_apply (box (Data_Decide_Decideusd_Dict)) (box ((Map.add "choose" (box ((fun (f: obj) -> (fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (f)), (unbox (v)), (unbox (v1)))) with | (f1, g, h) -> (box ((sharpurs_apply (box (Data_Op_Op)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Decide_compose)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Either_either)) (box (g))))) (box (h)))))))) (box (f1)))))))))))))) (Map.add "Divide0" (box ((fun (_: obj) -> divideOp))) Map.empty)))))))

let Data_Decide_chooseEquivalence  = (sharpurs_apply (box (Data_Decide_Decideusd_Dict)) (box ((Map.add "choose" (box ((fun (f: obj) -> (fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (f)), (unbox (v)), (unbox (v1)))) with | (f1, g, h) -> (box ((sharpurs_apply (box (Data_Equivalence_Equivalence)) (box ((fun (a: obj) -> (fun (b: obj) -> (let v2 = (sharpurs_apply (box (f1)) (box (a))) in (match ((unbox (v2))) with | Data_Either_Leftusd_Ctor(c) -> (box ((let v3 = (sharpurs_apply (box (f1)) (box (b))) in (match ((unbox (v3))) with | Data_Either_Leftusd_Ctor(d) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (g)) (box (c))))) (box (d))))) | Data_Either_Rightusd_Ctor(_) -> (box ((box false))))))) | Data_Either_Rightusd_Ctor(c) -> (box ((let v3 = (sharpurs_apply (box (f1)) (box (b))) in (match ((unbox (v3))) with | Data_Either_Leftusd_Ctor(_) -> (box ((box false))) | Data_Either_Rightusd_Ctor(d) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (h)) (box (c))))) (box (d)))))))))))))))))))))))) (Map.add "Divide0" (box ((fun (_: obj) -> Data_Divide_divideEquivalence))) Map.empty)))))

let Data_Decide_chooseComparison  = (sharpurs_apply (box (Data_Decide_Decideusd_Dict)) (box ((Map.add "choose" (box ((fun (f: obj) -> (fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (f)), (unbox (v)), (unbox (v1)))) with | (f1, g, h) -> (box ((sharpurs_apply (box (Data_Comparison_Comparison)) (box ((fun (a: obj) -> (fun (b: obj) -> (let v2 = (sharpurs_apply (box (f1)) (box (a))) in (match ((unbox (v2))) with | Data_Either_Leftusd_Ctor(c) -> (box ((let v3 = (sharpurs_apply (box (f1)) (box (b))) in (match ((unbox (v3))) with | Data_Either_Leftusd_Ctor(d) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (g)) (box (c))))) (box (d))))) | Data_Either_Rightusd_Ctor(_) -> (box ((box Data_Ordering_LTusd_Ctor))))))) | Data_Either_Rightusd_Ctor(c) -> (box ((let v3 = (sharpurs_apply (box (f1)) (box (b))) in (match ((unbox (v3))) with | Data_Either_Leftusd_Ctor(_) -> (box ((box Data_Ordering_GTusd_Ctor))) | Data_Either_Rightusd_Ctor(d) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (h)) (box (c))))) (box (d)))))))))))))))))))))))) (Map.add "Divide0" (box ((fun (_: obj) -> Data_Divide_divideComparison))) Map.empty)))))

let Data_Decide_choose  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "choose" (unbox<Map<string, obj>> (v)))))))

let Data_Decide_chosen  = (fun (dictDecide: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Decide_choose)) (box (dictDecide))))) (box (Data_Decide_identity))))
