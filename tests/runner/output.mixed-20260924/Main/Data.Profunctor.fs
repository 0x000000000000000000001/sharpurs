[<AutoOpen>]
module PureScript_Data_Profunctor

open System
open System.Collections.Generic

let Data_Profunctor_composeFlipped  = (sharpurs_apply (box (Control_Semigroupoid_composeFlipped)) (box (Control_Semigroupoid_semigroupoidFn)))

let Data_Profunctor_identity  = (sharpurs_apply (box (Control_Category_identity)) (box (Control_Category_categoryFn)))

let Data_Profunctor_wrap  = (sharpurs_apply (box (Data_Newtype_wrap)) (box (Prim_undefined)))

let Data_Profunctor_unwrap  = (sharpurs_apply (box (Data_Newtype_unwrap)) (box (Prim_undefined)))

let Data_Profunctor_Profunctorusd_Dict  = (fun (x: obj) -> x)

let Data_Profunctor_profunctorFn  = (sharpurs_apply (box (Data_Profunctor_Profunctorusd_Dict)) (box ((Map.add "dimap" (box ((fun (a2b: obj) -> (fun (c2d: obj) -> (fun (b2c: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Profunctor_composeFlipped)) (box (a2b))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Profunctor_composeFlipped)) (box (b2c))))) (box (c2d))))))))))) Map.empty))))

let Data_Profunctor_dimap  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "dimap" (unbox<Map<string, obj>> (v)))))))

let Data_Profunctor_lcmap  = (fun (dictProfunctor: obj) -> (let dimap1 = (sharpurs_apply (box (Data_Profunctor_dimap)) (box (dictProfunctor))) in (fun (a2b: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (dimap1)) (box (a2b))))) (box (Data_Profunctor_identity))))))

let Data_Profunctor_rmap  = (fun (dictProfunctor: obj) -> (let dimap1 = (sharpurs_apply (box (Data_Profunctor_dimap)) (box (dictProfunctor))) in (fun (b2c: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (dimap1)) (box (Data_Profunctor_identity))))) (box (b2c))))))

let Data_Profunctor_unwrapIso  = (fun (dictProfunctor: obj) -> (let dimap1 = (sharpurs_apply (box (Data_Profunctor_dimap)) (box (dictProfunctor))) in (fun (_: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (dimap1)) (box (Data_Profunctor_wrap))))) (box (Data_Profunctor_unwrap))))))

let Data_Profunctor_wrapIso  = (fun (dictProfunctor: obj) -> (let dimap1 = (sharpurs_apply (box (Data_Profunctor_dimap)) (box (dictProfunctor))) in (fun (_: obj) -> (fun (v: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (dimap1)) (box (Data_Profunctor_unwrap))))) (box (Data_Profunctor_wrap)))))))

let Data_Profunctor_arr  = (fun (dictCategory: obj) -> (let identity1 = (sharpurs_apply (box (Control_Category_identity)) (box (dictCategory))) in (fun (dictProfunctor: obj) -> (let rmap1 = (sharpurs_apply (box (Data_Profunctor_rmap)) (box (dictProfunctor))) in (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (rmap1)) (box (f))))) (box (identity1))))))))
