[<AutoOpen>]
module PureScript_Data_Profunctor_Split

open System
open System.Collections.Generic

type Data_Profunctor_Split_SplitF =
  | Data_Profunctor_Split_SplitFusd_Ctor of obj * obj * obj

let Data_Profunctor_Split_compose  = (sharpurs_apply (box (Control_Semigroupoid_compose)) (box (Control_Semigroupoid_semigroupoidFn)))

let Data_Profunctor_Split_identity  = (sharpurs_apply (box (Control_Category_identity)) (box (Control_Category_categoryFn)))

let Data_Profunctor_Split_SplitF  = (fun (usd__arg1: obj) -> (fun (usd__arg2: obj) -> (fun (usd__arg3: obj) -> (box (Data_Profunctor_Split_SplitFusd_Ctor(usd__arg1, usd__arg2, usd__arg3))))))

let Data_Profunctor_Split_Split  = (fun (x: obj) -> x)

let Data_Profunctor_Split_unSplit  = (fun (f: obj) -> (fun (v: obj) -> (match (((unbox (f)), (unbox (v)))) with | (f1, e) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Exists_runExists)) (box ((fun (v1: obj) -> (match ((unbox (v1))) with | Data_Profunctor_Split_SplitFusd_Ctor(g, h, fx) -> (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (f1)) (box (g))))) (box (h))))) (box (fx)))))))))))) (box (e))))))))

let Data_Profunctor_Split_split  = (fun (f: obj) -> (fun (g: obj) -> (fun (fx: obj) -> (sharpurs_apply (box (Data_Profunctor_Split_Split)) (box ((sharpurs_apply (box (Data_Exists_mkExists)) (box ((box (Data_Profunctor_Split_SplitFusd_Ctor(f, g, fx))))))))))))

let Data_Profunctor_Split_profunctorSplit  = (sharpurs_apply (box (Data_Profunctor_Profunctorusd_Dict)) (box ((Map.add "dimap" (box ((fun (f: obj) -> (fun (g: obj) -> (sharpurs_apply (box (Data_Profunctor_Split_unSplit)) (box ((fun (h: obj) -> (fun (i: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Profunctor_Split_split)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Profunctor_Split_compose)) (box (h))))) (box (f)))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Profunctor_Split_compose)) (box (g))))) (box (i))))))))))))))) Map.empty))))

let Data_Profunctor_Split_lowerSplit  = (fun (dictInvariant: obj) -> (sharpurs_apply (box (Data_Profunctor_Split_unSplit)) (box ((sharpurs_apply (box (Data_Function_flip)) (box ((sharpurs_apply (box (Data_Functor_Invariant_imap)) (box (dictInvariant))))))))))

let Data_Profunctor_Split_liftSplit  = (sharpurs_apply (box ((sharpurs_apply (box (Data_Profunctor_Split_split)) (box (Data_Profunctor_Split_identity))))) (box (Data_Profunctor_Split_identity)))

let Data_Profunctor_Split_hoistSplit  = (fun (nat: obj) -> (sharpurs_apply (box (Data_Profunctor_Split_unSplit)) (box ((fun (f: obj) -> (fun (g: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Profunctor_Split_compose)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Profunctor_Split_split)) (box (f))))) (box (g)))))))) (box (nat)))))))))

let Data_Profunctor_Split_functorSplit  = (sharpurs_apply (box (Data_Functor_Functorusd_Dict)) (box ((Map.add "map" (box ((fun (f: obj) -> (sharpurs_apply (box (Data_Profunctor_Split_unSplit)) (box ((fun (g: obj) -> (fun (h: obj) -> (fun (fx: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Profunctor_Split_split)) (box (g))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Profunctor_Split_compose)) (box (f))))) (box (h)))))))) (box (fx)))))))))))) Map.empty))))
