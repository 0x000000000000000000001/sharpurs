[<AutoOpen>]
module PureScript_Type_Data_Ordering

open System
open System.Collections.Generic

let Type_Data_Ordering_IsOrderingusd_Dict  = (fun (x: obj) -> x)

let Type_Data_Ordering_Invertusd_Dict  = (fun (x: obj) -> x)

let Type_Data_Ordering_Equalsusd_Dict  = (fun (x: obj) -> x)

let Type_Data_Ordering_Appendusd_Dict  = (fun (x: obj) -> x)

let Type_Data_Ordering_reflectOrdering  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "reflectOrdering" (unbox<Map<string, obj>> (v)))))))

let Type_Data_Ordering_isOrderingLT  = (sharpurs_apply (box (Type_Data_Ordering_IsOrderingusd_Dict)) (box ((Map.add "reflectOrdering" (box ((fun (v: obj) -> (box Data_Ordering_LTusd_Ctor)))) Map.empty))))

let Type_Data_Ordering_isOrderingGT  = (sharpurs_apply (box (Type_Data_Ordering_IsOrderingusd_Dict)) (box ((Map.add "reflectOrdering" (box ((fun (v: obj) -> (box Data_Ordering_GTusd_Ctor)))) Map.empty))))

let Type_Data_Ordering_isOrderingEQ  = (sharpurs_apply (box (Type_Data_Ordering_IsOrderingusd_Dict)) (box ((Map.add "reflectOrdering" (box ((fun (v: obj) -> (box Data_Ordering_EQusd_Ctor)))) Map.empty))))

let Type_Data_Ordering_reifyOrdering  = (fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (Data_Ordering_LTusd_Ctor, f) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (f)) (box (Type_Data_Ordering_isOrderingLT))))) (box ((box Type_Proxy_Proxyusd_Ctor)))))) | (Data_Ordering_EQusd_Ctor, f) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (f)) (box (Type_Data_Ordering_isOrderingEQ))))) (box ((box Type_Proxy_Proxyusd_Ctor)))))) | (Data_Ordering_GTusd_Ctor, f) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (f)) (box (Type_Data_Ordering_isOrderingGT))))) (box ((box Type_Proxy_Proxyusd_Ctor)))))))))

let Type_Data_Ordering_invertOrderingLT  = (sharpurs_apply (box (Type_Data_Ordering_Invertusd_Dict)) (box (Map.empty)))

let Type_Data_Ordering_invertOrderingGT  = (sharpurs_apply (box (Type_Data_Ordering_Invertusd_Dict)) (box (Map.empty)))

let Type_Data_Ordering_invertOrderingEQ  = (sharpurs_apply (box (Type_Data_Ordering_Invertusd_Dict)) (box (Map.empty)))

let Type_Data_Ordering_invert  = (fun (_: obj) -> (fun (v: obj) -> (box Type_Proxy_Proxyusd_Ctor)))

let Type_Data_Ordering_equalsLTLT  = (sharpurs_apply (box (Type_Data_Ordering_Equalsusd_Dict)) (box (Map.empty)))

let Type_Data_Ordering_equalsLTGT  = (sharpurs_apply (box (Type_Data_Ordering_Equalsusd_Dict)) (box (Map.empty)))

let Type_Data_Ordering_equalsLTEQ  = (sharpurs_apply (box (Type_Data_Ordering_Equalsusd_Dict)) (box (Map.empty)))

let Type_Data_Ordering_equalsGTLT  = (sharpurs_apply (box (Type_Data_Ordering_Equalsusd_Dict)) (box (Map.empty)))

let Type_Data_Ordering_equalsGTGT  = (sharpurs_apply (box (Type_Data_Ordering_Equalsusd_Dict)) (box (Map.empty)))

let Type_Data_Ordering_equalsGTEQ  = (sharpurs_apply (box (Type_Data_Ordering_Equalsusd_Dict)) (box (Map.empty)))

let Type_Data_Ordering_equalsEQLT  = (sharpurs_apply (box (Type_Data_Ordering_Equalsusd_Dict)) (box (Map.empty)))

let Type_Data_Ordering_equalsEQGT  = (sharpurs_apply (box (Type_Data_Ordering_Equalsusd_Dict)) (box (Map.empty)))

let Type_Data_Ordering_equalsEQEQ  = (sharpurs_apply (box (Type_Data_Ordering_Equalsusd_Dict)) (box (Map.empty)))

let Type_Data_Ordering_equals  = (fun (_: obj) -> (fun (v: obj) -> (fun (v1: obj) -> (box Type_Proxy_Proxyusd_Ctor))))

let Type_Data_Ordering_appendOrderingLT  = (sharpurs_apply (box (Type_Data_Ordering_Appendusd_Dict)) (box (Map.empty)))

let Type_Data_Ordering_appendOrderingGT  = (sharpurs_apply (box (Type_Data_Ordering_Appendusd_Dict)) (box (Map.empty)))

let Type_Data_Ordering_appendOrderingEQ  = (sharpurs_apply (box (Type_Data_Ordering_Appendusd_Dict)) (box (Map.empty)))

let Type_Data_Ordering_append  = (fun (_: obj) -> (fun (v: obj) -> (fun (v1: obj) -> (box Type_Proxy_Proxyusd_Ctor))))
