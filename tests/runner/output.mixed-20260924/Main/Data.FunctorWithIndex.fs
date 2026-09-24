[<AutoOpen>]
module PureScript_Data_FunctorWithIndex

open System
open System.Collections.Generic

module Data_FunctorWithIndex_FFI =
    let mapWithIndexArray = box (fun (f: obj) -> box (fun (xs: obj) ->
        let arr = unbox<obj[]> xs
        let res = Array.mapi (fun i x -> sharpurs_apply (sharpurs_apply f (box i)) x) arr
        box res
    ))
    

let Data_FunctorWithIndex_mapWithIndexArray = box Data_FunctorWithIndex_FFI.``mapWithIndexArray``


let Data_FunctorWithIndex_map  = (sharpurs_apply (box (Data_Functor_map)) (box (Data_Tuple_functorTuple)))

let Data_FunctorWithIndex_bimap  = (sharpurs_apply (box (Data_Bifunctor_bimap)) (box (Data_Bifunctor_bifunctorTuple)))

let Data_FunctorWithIndex_compose  = (sharpurs_apply (box (Control_Semigroupoid_compose)) (box (Control_Semigroupoid_semigroupoidFn)))

let Data_FunctorWithIndex_map1  = (sharpurs_apply (box (Data_Functor_map)) (box (Data_Monoid_Multiplicative_functorMultiplicative)))

let Data_FunctorWithIndex_map2  = (sharpurs_apply (box (Data_Functor_map)) (box (Data_Maybe_functorMaybe)))

let Data_FunctorWithIndex_map3  = (sharpurs_apply (box (Data_Functor_map)) (box (Data_Maybe_Last_functorLast)))

let Data_FunctorWithIndex_map4  = (sharpurs_apply (box (Data_Functor_map)) (box (Data_Maybe_First_functorFirst)))

let Data_FunctorWithIndex_map5  = (sharpurs_apply (box (Data_Functor_map)) (box (Data_Either_functorEither)))

let Data_FunctorWithIndex_map6  = (sharpurs_apply (box (Data_Functor_map)) (box (Data_Monoid_Dual_functorDual)))

let Data_FunctorWithIndex_map7  = (sharpurs_apply (box (Data_Functor_map)) (box (Data_Monoid_Disj_functorDisj)))

let Data_FunctorWithIndex_bimap1  = (sharpurs_apply (box (Data_Bifunctor_bimap)) (box (Data_Bifunctor_bifunctorEither)))

let Data_FunctorWithIndex_map8  = (sharpurs_apply (box (Data_Functor_map)) (box (Data_Monoid_Conj_functorConj)))

let Data_FunctorWithIndex_map9  = (sharpurs_apply (box (Data_Functor_map)) (box (Data_Monoid_Additive_functorAdditive)))

let Data_FunctorWithIndex_FunctorWithIndexusd_Dict  = (fun (x: obj) -> x)

let Data_FunctorWithIndex_mapWithIndex  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "mapWithIndex" (unbox<Map<string, obj>> (v)))))))

let Data_FunctorWithIndex_mapDefault  = (fun (dictFunctorWithIndex: obj) -> (let mapWithIndex1 = (sharpurs_apply (box (Data_FunctorWithIndex_mapWithIndex)) (box (dictFunctorWithIndex))) in (fun (f: obj) -> (sharpurs_apply (box (mapWithIndex1)) (box ((sharpurs_apply (box (Data_Function_const)) (box (f)))))))))

let Data_FunctorWithIndex_functorWithIndexTuple  = (sharpurs_apply (box (Data_FunctorWithIndex_FunctorWithIndexusd_Dict)) (box ((Map.add "mapWithIndex" (box ((fun (f: obj) -> (sharpurs_apply (box (Data_FunctorWithIndex_map)) (box ((sharpurs_apply (box (f)) (box (Data_Unit_unit))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Tuple_functorTuple))) Map.empty)))))

let Data_FunctorWithIndex_functorWithIndexProduct  = (fun (dictFunctorWithIndex: obj) -> (let mapWithIndex1 = (sharpurs_apply (box (Data_FunctorWithIndex_mapWithIndex)) (box (dictFunctorWithIndex))) in let functorProduct = (sharpurs_apply (box (Data_Functor_Product_functorProduct)) (box ((sharpurs_apply (box ((Map.find "Functor0" (unbox<Map<string, obj>> (dictFunctorWithIndex))))) (box (Prim_undefined)))))) in (fun (dictFunctorWithIndex1: obj) -> (let mapWithIndex2 = (sharpurs_apply (box (Data_FunctorWithIndex_mapWithIndex)) (box (dictFunctorWithIndex1))) in let functorProduct1 = (sharpurs_apply (box (functorProduct)) (box ((sharpurs_apply (box ((Map.find "Functor0" (unbox<Map<string, obj>> (dictFunctorWithIndex1))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_FunctorWithIndex_FunctorWithIndexusd_Dict)) (box ((Map.add "mapWithIndex" (box ((fun (f: obj) -> (fun (v: obj) -> (match (((unbox (f)), (unbox (v)))) with | (f1, fga) -> (box ((sharpurs_apply (box (Data_Functor_Product_Product)) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_FunctorWithIndex_bimap)) (box ((sharpurs_apply (box (mapWithIndex1)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_FunctorWithIndex_compose)) (box (f1))))) (box ((fun (usd__arg1: obj) -> (box (Data_Either_Leftusd_Ctor(usd__arg1))))))))))))))) (box ((sharpurs_apply (box (mapWithIndex2)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_FunctorWithIndex_compose)) (box (f1))))) (box ((fun (usd__arg1: obj) -> (box (Data_Either_Rightusd_Ctor(usd__arg1))))))))))))))) (box (fga))))))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> functorProduct1))) Map.empty)))))))))

let Data_FunctorWithIndex_functorWithIndexMultiplicative  = (sharpurs_apply (box (Data_FunctorWithIndex_FunctorWithIndexusd_Dict)) (box ((Map.add "mapWithIndex" (box ((fun (f: obj) -> (sharpurs_apply (box (Data_FunctorWithIndex_map1)) (box ((sharpurs_apply (box (f)) (box (Data_Unit_unit))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Monoid_Multiplicative_functorMultiplicative))) Map.empty)))))

let Data_FunctorWithIndex_functorWithIndexMaybe  = (sharpurs_apply (box (Data_FunctorWithIndex_FunctorWithIndexusd_Dict)) (box ((Map.add "mapWithIndex" (box ((fun (f: obj) -> (sharpurs_apply (box (Data_FunctorWithIndex_map2)) (box ((sharpurs_apply (box (f)) (box (Data_Unit_unit))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Maybe_functorMaybe))) Map.empty)))))

let Data_FunctorWithIndex_functorWithIndexLast  = (sharpurs_apply (box (Data_FunctorWithIndex_FunctorWithIndexusd_Dict)) (box ((Map.add "mapWithIndex" (box ((fun (f: obj) -> (sharpurs_apply (box (Data_FunctorWithIndex_map3)) (box ((sharpurs_apply (box (f)) (box (Data_Unit_unit))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Maybe_Last_functorLast))) Map.empty)))))

let Data_FunctorWithIndex_functorWithIndexIdentity  = (sharpurs_apply (box (Data_FunctorWithIndex_FunctorWithIndexusd_Dict)) (box ((Map.add "mapWithIndex" (box ((fun (f: obj) -> (fun (v: obj) -> (match (((unbox (f)), (unbox (v)))) with | (f1, a) -> (box ((sharpurs_apply (box (Data_Identity_Identity)) (box ((sharpurs_apply (box ((sharpurs_apply (box (f1)) (box (Data_Unit_unit))))) (box (a))))))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Identity_functorIdentity))) Map.empty)))))

let Data_FunctorWithIndex_functorWithIndexFirst  = (sharpurs_apply (box (Data_FunctorWithIndex_FunctorWithIndexusd_Dict)) (box ((Map.add "mapWithIndex" (box ((fun (f: obj) -> (sharpurs_apply (box (Data_FunctorWithIndex_map4)) (box ((sharpurs_apply (box (f)) (box (Data_Unit_unit))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Maybe_First_functorFirst))) Map.empty)))))

let Data_FunctorWithIndex_functorWithIndexEither  = (sharpurs_apply (box (Data_FunctorWithIndex_FunctorWithIndexusd_Dict)) (box ((Map.add "mapWithIndex" (box ((fun (f: obj) -> (sharpurs_apply (box (Data_FunctorWithIndex_map5)) (box ((sharpurs_apply (box (f)) (box (Data_Unit_unit))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Either_functorEither))) Map.empty)))))

let Data_FunctorWithIndex_functorWithIndexDual  = (sharpurs_apply (box (Data_FunctorWithIndex_FunctorWithIndexusd_Dict)) (box ((Map.add "mapWithIndex" (box ((fun (f: obj) -> (sharpurs_apply (box (Data_FunctorWithIndex_map6)) (box ((sharpurs_apply (box (f)) (box (Data_Unit_unit))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Monoid_Dual_functorDual))) Map.empty)))))

let Data_FunctorWithIndex_functorWithIndexDisj  = (sharpurs_apply (box (Data_FunctorWithIndex_FunctorWithIndexusd_Dict)) (box ((Map.add "mapWithIndex" (box ((fun (f: obj) -> (sharpurs_apply (box (Data_FunctorWithIndex_map7)) (box ((sharpurs_apply (box (f)) (box (Data_Unit_unit))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Monoid_Disj_functorDisj))) Map.empty)))))

let Data_FunctorWithIndex_functorWithIndexCoproduct  = (fun (dictFunctorWithIndex: obj) -> (let mapWithIndex1 = (sharpurs_apply (box (Data_FunctorWithIndex_mapWithIndex)) (box (dictFunctorWithIndex))) in let functorCoproduct = (sharpurs_apply (box (Data_Functor_Coproduct_functorCoproduct)) (box ((sharpurs_apply (box ((Map.find "Functor0" (unbox<Map<string, obj>> (dictFunctorWithIndex))))) (box (Prim_undefined)))))) in (fun (dictFunctorWithIndex1: obj) -> (let mapWithIndex2 = (sharpurs_apply (box (Data_FunctorWithIndex_mapWithIndex)) (box (dictFunctorWithIndex1))) in let functorCoproduct1 = (sharpurs_apply (box (functorCoproduct)) (box ((sharpurs_apply (box ((Map.find "Functor0" (unbox<Map<string, obj>> (dictFunctorWithIndex1))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_FunctorWithIndex_FunctorWithIndexusd_Dict)) (box ((Map.add "mapWithIndex" (box ((fun (f: obj) -> (fun (v: obj) -> (match (((unbox (f)), (unbox (v)))) with | (f1, e) -> (box ((sharpurs_apply (box (Data_Functor_Coproduct_Coproduct)) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_FunctorWithIndex_bimap1)) (box ((sharpurs_apply (box (mapWithIndex1)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_FunctorWithIndex_compose)) (box (f1))))) (box ((fun (usd__arg1: obj) -> (box (Data_Either_Leftusd_Ctor(usd__arg1))))))))))))))) (box ((sharpurs_apply (box (mapWithIndex2)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_FunctorWithIndex_compose)) (box (f1))))) (box ((fun (usd__arg1: obj) -> (box (Data_Either_Rightusd_Ctor(usd__arg1))))))))))))))) (box (e))))))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> functorCoproduct1))) Map.empty)))))))))

let Data_FunctorWithIndex_functorWithIndexConst  = (sharpurs_apply (box (Data_FunctorWithIndex_FunctorWithIndexusd_Dict)) (box ((Map.add "mapWithIndex" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (_, x) -> (box ((sharpurs_apply (box (Data_Const_Const)) (box (x)))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Const_functorConst))) Map.empty)))))

let Data_FunctorWithIndex_functorWithIndexConj  = (sharpurs_apply (box (Data_FunctorWithIndex_FunctorWithIndexusd_Dict)) (box ((Map.add "mapWithIndex" (box ((fun (f: obj) -> (sharpurs_apply (box (Data_FunctorWithIndex_map8)) (box ((sharpurs_apply (box (f)) (box (Data_Unit_unit))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Monoid_Conj_functorConj))) Map.empty)))))

let Data_FunctorWithIndex_functorWithIndexCompose  = (fun (dictFunctorWithIndex: obj) -> (let mapWithIndex1 = (sharpurs_apply (box (Data_FunctorWithIndex_mapWithIndex)) (box (dictFunctorWithIndex))) in let functorCompose = (sharpurs_apply (box (Data_Functor_Compose_functorCompose)) (box ((sharpurs_apply (box ((Map.find "Functor0" (unbox<Map<string, obj>> (dictFunctorWithIndex))))) (box (Prim_undefined)))))) in (fun (dictFunctorWithIndex1: obj) -> (let mapWithIndex2 = (sharpurs_apply (box (Data_FunctorWithIndex_mapWithIndex)) (box (dictFunctorWithIndex1))) in let functorCompose1 = (sharpurs_apply (box (functorCompose)) (box ((sharpurs_apply (box ((Map.find "Functor0" (unbox<Map<string, obj>> (dictFunctorWithIndex1))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_FunctorWithIndex_FunctorWithIndexusd_Dict)) (box ((Map.add "mapWithIndex" (box ((fun (f: obj) -> (fun (v: obj) -> (match (((unbox (f)), (unbox (v)))) with | (f1, fga) -> (box ((sharpurs_apply (box (Data_Functor_Compose_Compose)) (box ((sharpurs_apply (box ((sharpurs_apply (box (mapWithIndex1)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_FunctorWithIndex_compose)) (box (mapWithIndex2))))) (box ((sharpurs_apply (box (Data_Tuple_curry)) (box (f1))))))))))) (box (fga))))))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> functorCompose1))) Map.empty)))))))))

let Data_FunctorWithIndex_functorWithIndexArray  = (sharpurs_apply (box (Data_FunctorWithIndex_FunctorWithIndexusd_Dict)) (box ((Map.add "mapWithIndex" (box (Data_FunctorWithIndex_mapWithIndexArray)) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Functor_functorArray))) Map.empty)))))

let Data_FunctorWithIndex_functorWithIndexApp  = (fun (dictFunctorWithIndex: obj) -> (let mapWithIndex1 = (sharpurs_apply (box (Data_FunctorWithIndex_mapWithIndex)) (box (dictFunctorWithIndex))) in let functorApp = (sharpurs_apply (box (Data_Functor_App_functorApp)) (box ((sharpurs_apply (box ((Map.find "Functor0" (unbox<Map<string, obj>> (dictFunctorWithIndex))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_FunctorWithIndex_FunctorWithIndexusd_Dict)) (box ((Map.add "mapWithIndex" (box ((fun (f: obj) -> (fun (v: obj) -> (match (((unbox (f)), (unbox (v)))) with | (f1, x) -> (box ((sharpurs_apply (box (Data_Functor_App_App)) (box ((sharpurs_apply (box ((sharpurs_apply (box (mapWithIndex1)) (box (f1))))) (box (x))))))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> functorApp))) Map.empty)))))))

let Data_FunctorWithIndex_functorWithIndexAdditive  = (sharpurs_apply (box (Data_FunctorWithIndex_FunctorWithIndexusd_Dict)) (box ((Map.add "mapWithIndex" (box ((fun (f: obj) -> (sharpurs_apply (box (Data_FunctorWithIndex_map9)) (box ((sharpurs_apply (box (f)) (box (Data_Unit_unit))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Monoid_Additive_functorAdditive))) Map.empty)))))
