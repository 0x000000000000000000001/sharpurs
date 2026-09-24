[<AutoOpen>]
module PureScript_Data_Bifunctor

open System
open System.Collections.Generic

let Data_Bifunctor_identity  = (sharpurs_apply (box (Control_Category_identity)) (box (Control_Category_categoryFn)))

let Data_Bifunctor_Bifunctorusd_Dict  = (fun (x: obj) -> x)

let Data_Bifunctor_bimap  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "bimap" (unbox<Map<string, obj>> (v)))))))

let Data_Bifunctor_bivoid  = (fun (dictBifunctor: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Bifunctor_bimap)) (box (dictBifunctor))))) (box ((sharpurs_apply (box (Data_Function_const)) (box (Data_Unit_unit)))))))) (box ((sharpurs_apply (box (Data_Function_const)) (box (Data_Unit_unit)))))))

let Data_Bifunctor_lmap  = (fun (dictBifunctor: obj) -> (let bimap1 = (sharpurs_apply (box (Data_Bifunctor_bimap)) (box (dictBifunctor))) in (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (bimap1)) (box (f))))) (box (Data_Bifunctor_identity))))))

let Data_Bifunctor_rmap  = (fun (dictBifunctor: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Bifunctor_bimap)) (box (dictBifunctor))))) (box (Data_Bifunctor_identity))))

let Data_Bifunctor_bifunctorTuple  = (sharpurs_apply (box (Data_Bifunctor_Bifunctorusd_Dict)) (box ((Map.add "bimap" (box ((fun (f: obj) -> (fun (g: obj) -> (fun (v: obj) -> (match (((unbox (f)), (unbox (g)), (unbox (v)))) with | (f1, g1, Data_Tuple_Tupleusd_Ctor(x, y)) -> (box ((box (Data_Tuple_Tupleusd_Ctor((sharpurs_apply (box (f1)) (box (x))), (sharpurs_apply (box (g1)) (box (y)))))))))))))) Map.empty))))

let Data_Bifunctor_bifunctorEither  = (sharpurs_apply (box (Data_Bifunctor_Bifunctorusd_Dict)) (box ((Map.add "bimap" (box ((fun (v: obj) -> (fun (v1: obj) -> (fun (v2: obj) -> (match (((unbox (v)), (unbox (v1)), (unbox (v2)))) with | (f, _, Data_Either_Leftusd_Ctor(l)) -> (box ((box (Data_Either_Leftusd_Ctor((sharpurs_apply (box (f)) (box (l)))))))) | (_, g, Data_Either_Rightusd_Ctor(r)) -> (box ((box (Data_Either_Rightusd_Ctor((sharpurs_apply (box (g)) (box (r)))))))))))))) Map.empty))))

let Data_Bifunctor_bifunctorConst  = (sharpurs_apply (box (Data_Bifunctor_Bifunctorusd_Dict)) (box ((Map.add "bimap" (box ((fun (f: obj) -> (fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (f)), (unbox (v)), (unbox (v1)))) with | (f1, _, a) -> (box ((sharpurs_apply (box (Data_Const_Const)) (box ((sharpurs_apply (box (f1)) (box (a)))))))))))))) Map.empty))))
