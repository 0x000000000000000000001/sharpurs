[<AutoOpen>]
module PureScript_Control_Biapply

open System
open System.Collections.Generic

let Control_Biapply_identity  = (sharpurs_apply (box (Control_Category_identity)) (box (Control_Category_categoryFn)))

let Control_Biapply_Biapplyusd_Dict  = (fun (x: obj) -> x)

let Control_Biapply_biapplyTuple  = (sharpurs_apply (box (Control_Biapply_Biapplyusd_Dict)) (box ((Map.add "biapply" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (Data_Tuple_Tupleusd_Ctor(f, g), Data_Tuple_Tupleusd_Ctor(a, b)) -> (box ((box (Data_Tuple_Tupleusd_Ctor((sharpurs_apply (box (f)) (box (a))), (sharpurs_apply (box (g)) (box (b))))))))))))) (Map.add "Bifunctor0" (box ((fun (_: obj) -> Data_Bifunctor_bifunctorTuple))) Map.empty)))))

let Control_Biapply_biapply  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "biapply" (unbox<Map<string, obj>> (v)))))))

let Control_Biapply_biapplyFirst  = (fun (dictBiapply: obj) -> (let biapply1 = (sharpurs_apply (box (Control_Biapply_biapply)) (box (dictBiapply))) in let bimap = (sharpurs_apply (box (Data_Bifunctor_bimap)) (box ((sharpurs_apply (box ((Map.find "Bifunctor0" (unbox<Map<string, obj>> (dictBiapply))))) (box (Prim_undefined)))))) in (fun (a: obj) -> (fun (b: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (biapply1)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Control_Biapply_identity)) (box ((sharpurs_apply (box ((sharpurs_apply (box (bimap)) (box ((sharpurs_apply (box (Data_Function_const)) (box (Control_Biapply_identity)))))))) (box ((sharpurs_apply (box (Data_Function_const)) (box (Control_Biapply_identity))))))))))) (box (a)))))))) (box (b)))))))

let Control_Biapply_biapplySecond  = (fun (dictBiapply: obj) -> (let biapply1 = (sharpurs_apply (box (Control_Biapply_biapply)) (box (dictBiapply))) in let bimap = (sharpurs_apply (box (Data_Bifunctor_bimap)) (box ((sharpurs_apply (box ((Map.find "Bifunctor0" (unbox<Map<string, obj>> (dictBiapply))))) (box (Prim_undefined)))))) in (fun (a: obj) -> (fun (b: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (biapply1)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Control_Biapply_identity)) (box ((sharpurs_apply (box ((sharpurs_apply (box (bimap)) (box (Data_Function_const))))) (box (Data_Function_const)))))))) (box (a)))))))) (box (b)))))))

let Control_Biapply_bilift2  = (fun (dictBiapply: obj) -> (let biapply1 = (sharpurs_apply (box (Control_Biapply_biapply)) (box (dictBiapply))) in let bimap = (sharpurs_apply (box (Data_Bifunctor_bimap)) (box ((sharpurs_apply (box ((Map.find "Bifunctor0" (unbox<Map<string, obj>> (dictBiapply))))) (box (Prim_undefined)))))) in (fun (f: obj) -> (fun (g: obj) -> (fun (a: obj) -> (fun (b: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (biapply1)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Control_Biapply_identity)) (box ((sharpurs_apply (box ((sharpurs_apply (box (bimap)) (box (f))))) (box (g)))))))) (box (a)))))))) (box (b)))))))))

let Control_Biapply_bilift3  = (fun (dictBiapply: obj) -> (let biapply1 = (sharpurs_apply (box (Control_Biapply_biapply)) (box (dictBiapply))) in let bimap = (sharpurs_apply (box (Data_Bifunctor_bimap)) (box ((sharpurs_apply (box ((Map.find "Bifunctor0" (unbox<Map<string, obj>> (dictBiapply))))) (box (Prim_undefined)))))) in (fun (f: obj) -> (fun (g: obj) -> (fun (a: obj) -> (fun (b: obj) -> (fun (c: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (biapply1)) (box ((sharpurs_apply (box ((sharpurs_apply (box (biapply1)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Control_Biapply_identity)) (box ((sharpurs_apply (box ((sharpurs_apply (box (bimap)) (box (f))))) (box (g)))))))) (box (a)))))))) (box (b)))))))) (box (c))))))))))
