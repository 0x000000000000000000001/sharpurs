[<AutoOpen>]
module PureScript_Data_Profunctor_Choice

open System
open System.Collections.Generic

let Data_Profunctor_Choice_identity  = (sharpurs_apply (box (Control_Category_identity)) (box (Control_Category_categoryFn)))

let Data_Profunctor_Choice_Choiceusd_Dict  = (fun (x: obj) -> x)

let Data_Profunctor_Choice_right  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "right" (unbox<Map<string, obj>> (v)))))))

let Data_Profunctor_Choice_left  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "left" (unbox<Map<string, obj>> (v)))))))

let Data_Profunctor_Choice_splitChoice  = (fun (dictSemigroupoid: obj) -> (let composeFlipped = (sharpurs_apply (box (Control_Semigroupoid_composeFlipped)) (box (dictSemigroupoid))) in (fun (dictChoice: obj) -> (let left1 = (sharpurs_apply (box (Data_Profunctor_Choice_left)) (box (dictChoice))) in let right1 = (sharpurs_apply (box (Data_Profunctor_Choice_right)) (box (dictChoice))) in (fun (l: obj) -> (fun (r: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (composeFlipped)) (box ((sharpurs_apply (box (left1)) (box (l)))))))) (box ((sharpurs_apply (box (right1)) (box (r))))))))))))

let Data_Profunctor_Choice_fanin  = (fun (dictSemigroupoid: obj) -> (let splitChoice1 = (sharpurs_apply (box (Data_Profunctor_Choice_splitChoice)) (box (dictSemigroupoid))) in (fun (dictChoice: obj) -> (let rmap = (sharpurs_apply (box (Data_Profunctor_rmap)) (box ((sharpurs_apply (box ((Map.find "Profunctor0" (unbox<Map<string, obj>> (dictChoice))))) (box (Prim_undefined)))))) in let splitChoice2 = (sharpurs_apply (box (splitChoice1)) (box (dictChoice))) in (fun (l: obj) -> (fun (r: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (rmap)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Either_either)) (box (Data_Profunctor_Choice_identity))))) (box (Data_Profunctor_Choice_identity)))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (splitChoice2)) (box (l))))) (box (r))))))))))))

let Data_Profunctor_Choice_choiceFn  = (sharpurs_apply (box (Data_Profunctor_Choice_Choiceusd_Dict)) (box ((Map.add "left" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (a2b, Data_Either_Leftusd_Ctor(a)) -> (box ((box (Data_Either_Leftusd_Ctor((sharpurs_apply (box (a2b)) (box (a)))))))) | (_, Data_Either_Rightusd_Ctor(c)) -> (box ((box (Data_Either_Rightusd_Ctor(c)))))))))) (Map.add "right" (box ((sharpurs_apply (box (Data_Functor_map)) (box (Data_Either_functorEither))))) (Map.add "Profunctor0" (box ((fun (_: obj) -> Data_Profunctor_profunctorFn))) Map.empty))))))
