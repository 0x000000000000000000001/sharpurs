[<AutoOpen>]
module PureScript_Data_Distributive

open System
open System.Collections.Generic

let Data_Distributive_compose  = (sharpurs_apply (box (Control_Semigroupoid_compose)) (box (Control_Semigroupoid_semigroupoidFn)))

let Data_Distributive_unwrap  = (sharpurs_apply (box (Data_Newtype_unwrap)) (box (Prim_undefined)))

let Data_Distributive_identity  = (sharpurs_apply (box (Control_Category_identity)) (box (Control_Category_categoryFn)))

let Data_Distributive_Distributiveusd_Dict  = (fun (x: obj) -> x)

let Data_Distributive_distributiveIdentity  = (sharpurs_apply (box (Data_Distributive_Distributiveusd_Dict)) (box ((Map.add "distribute" (box ((fun (dictFunctor: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Distributive_compose)) (box (Data_Identity_Identity))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Functor_map)) (box (dictFunctor))))) (box (Data_Distributive_unwrap))))))))) (Map.add "collect" (box ((fun (dictFunctor: obj) -> (let map = (sharpurs_apply (box (Data_Functor_map)) (box (dictFunctor))) in (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Distributive_compose)) (box (Data_Identity_Identity))))) (box ((sharpurs_apply (box (map)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Distributive_compose)) (box (Data_Distributive_unwrap))))) (box (f)))))))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Identity_functorIdentity))) Map.empty))))))

let Data_Distributive_distribute  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "distribute" (unbox<Map<string, obj>> (v)))))))

let rec Data_Distributive_distributiveFunction  = (sharpurs_apply (box (Data_Distributive_Distributiveusd_Dict)) (box ((Map.add "distribute" (box ((fun (dictFunctor: obj) -> (let map = (sharpurs_apply (box (Data_Functor_map)) (box (dictFunctor))) in (fun (a: obj) -> (fun (e: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (map)) (box ((fun (v: obj) -> (sharpurs_apply (box (v)) (box (e))))))))) (box (a))))))))) (Map.add "collect" (box ((fun (dictFunctor: obj) -> (let map = (sharpurs_apply (box (Data_Functor_map)) (box (dictFunctor))) in (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Distributive_compose)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Distributive_distribute)) (box (Data_Distributive_distributiveFunction))))) (box (dictFunctor)))))))) (box ((sharpurs_apply (box (map)) (box (f))))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Functor_functorFn))) Map.empty))))))

let Data_Distributive_cotraverse  = (fun (dictDistributive: obj) -> (let map = (sharpurs_apply (box (Data_Functor_map)) (box ((sharpurs_apply (box ((Map.find "Functor0" (unbox<Map<string, obj>> (dictDistributive))))) (box (Prim_undefined)))))) in let distribute1 = (sharpurs_apply (box (Data_Distributive_distribute)) (box (dictDistributive))) in (fun (dictFunctor: obj) -> (let distribute2 = (sharpurs_apply (box (distribute1)) (box (dictFunctor))) in (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Distributive_compose)) (box ((sharpurs_apply (box (map)) (box (f)))))))) (box (distribute2))))))))

let Data_Distributive_collectDefault  = (fun (dictDistributive: obj) -> (let distribute1 = (sharpurs_apply (box (Data_Distributive_distribute)) (box (dictDistributive))) in (fun (dictFunctor: obj) -> (let distribute2 = (sharpurs_apply (box (distribute1)) (box (dictFunctor))) in let map = (sharpurs_apply (box (Data_Functor_map)) (box (dictFunctor))) in (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Distributive_compose)) (box (distribute2))))) (box ((sharpurs_apply (box (map)) (box (f)))))))))))

let rec Data_Distributive_distributiveTuple  = (fun (dictTypeEquals: obj) -> (let from = (sharpurs_apply (box (Type_Equality_from)) (box (dictTypeEquals))) in (sharpurs_apply (box (Data_Distributive_Distributiveusd_Dict)) (box ((Map.add "collect" (box ((fun (dictFunctor: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Distributive_collectDefault)) (box ((sharpurs_apply (box (Data_Distributive_distributiveTuple)) (box (dictTypeEquals)))))))) (box (dictFunctor)))))) (Map.add "distribute" (box ((fun (dictFunctor: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Distributive_compose)) (box ((fun (usd__arg1: obj) -> (box (Data_Tuple_Tupleusd_Ctor((sharpurs_apply (box (from)) (box (Data_Unit_unit))), usd__arg1))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Functor_map)) (box (dictFunctor))))) (box (Data_Tuple_snd))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Tuple_functorTuple))) Map.empty))))))))

let Data_Distributive_collect  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "collect" (unbox<Map<string, obj>> (v)))))))

let Data_Distributive_distributeDefault  = (fun (dictDistributive: obj) -> (let collect1 = (sharpurs_apply (box (Data_Distributive_collect)) (box (dictDistributive))) in (fun (dictFunctor: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (collect1)) (box (dictFunctor))))) (box (Data_Distributive_identity))))))
