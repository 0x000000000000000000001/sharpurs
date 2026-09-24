[<AutoOpen>]
module PureScript_Data_Profunctor_Strong

open System
open System.Collections.Generic

let Data_Profunctor_Strong_Strongusd_Dict  = (fun (x: obj) -> x)

let Data_Profunctor_Strong_strongFn  = (sharpurs_apply (box (Data_Profunctor_Strong_Strongusd_Dict)) (box ((Map.add "first" (box ((fun (a2b: obj) -> (fun (v: obj) -> (match (((unbox (a2b)), (unbox (v)))) with | (a2b1, Data_Tuple_Tupleusd_Ctor(a, c)) -> (box ((box (Data_Tuple_Tupleusd_Ctor((sharpurs_apply (box (a2b1)) (box (a))), c)))))))))) (Map.add "second" (box ((sharpurs_apply (box (Data_Functor_map)) (box (Data_Tuple_functorTuple))))) (Map.add "Profunctor0" (box ((fun (_: obj) -> Data_Profunctor_profunctorFn))) Map.empty))))))

let Data_Profunctor_Strong_second  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "second" (unbox<Map<string, obj>> (v)))))))

let Data_Profunctor_Strong_first  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "first" (unbox<Map<string, obj>> (v)))))))

let Data_Profunctor_Strong_splitStrong  = (fun (dictSemigroupoid: obj) -> (let composeFlipped = (sharpurs_apply (box (Control_Semigroupoid_composeFlipped)) (box (dictSemigroupoid))) in (fun (dictStrong: obj) -> (let first1 = (sharpurs_apply (box (Data_Profunctor_Strong_first)) (box (dictStrong))) in let second1 = (sharpurs_apply (box (Data_Profunctor_Strong_second)) (box (dictStrong))) in (fun (l: obj) -> (fun (r: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (composeFlipped)) (box ((sharpurs_apply (box (first1)) (box (l)))))))) (box ((sharpurs_apply (box (second1)) (box (r))))))))))))

let Data_Profunctor_Strong_fanout  = (fun (dictSemigroupoid: obj) -> (let splitStrong1 = (sharpurs_apply (box (Data_Profunctor_Strong_splitStrong)) (box (dictSemigroupoid))) in (fun (dictStrong: obj) -> (let lcmap = (sharpurs_apply (box (Data_Profunctor_lcmap)) (box ((sharpurs_apply (box ((Map.find "Profunctor0" (unbox<Map<string, obj>> (dictStrong))))) (box (Prim_undefined)))))) in let splitStrong2 = (sharpurs_apply (box (splitStrong1)) (box (dictStrong))) in (fun (l: obj) -> (fun (r: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (lcmap)) (box ((fun (a: obj) -> (box (Data_Tuple_Tupleusd_Ctor(a, a))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (splitStrong2)) (box (l))))) (box (r))))))))))))
