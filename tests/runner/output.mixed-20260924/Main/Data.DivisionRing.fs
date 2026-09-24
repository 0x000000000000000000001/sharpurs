[<AutoOpen>]
module PureScript_Data_DivisionRing

open System
open System.Collections.Generic

let Data_DivisionRing_div  = (sharpurs_apply (box (Data_EuclideanRing_div)) (box (Data_EuclideanRing_euclideanRingNumber)))

let Data_DivisionRing_DivisionRingusd_Dict  = (fun (x: obj) -> x)

let Data_DivisionRing_recip  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "recip" (unbox<Map<string, obj>> (v)))))))

let Data_DivisionRing_rightDiv  = (fun (dictDivisionRing: obj) -> (let mul = (sharpurs_apply (box (Data_Semiring_mul)) (box ((sharpurs_apply (box ((Map.find "Semiring0" (unbox<Map<string, obj>> ((sharpurs_apply (box ((Map.find "Ring0" (unbox<Map<string, obj>> (dictDivisionRing))))) (box (Prim_undefined)))))))) (box (Prim_undefined)))))) in let recip1 = (sharpurs_apply (box (Data_DivisionRing_recip)) (box (dictDivisionRing))) in (fun (a: obj) -> (fun (b: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (mul)) (box (a))))) (box ((sharpurs_apply (box (recip1)) (box (b))))))))))

let Data_DivisionRing_leftDiv  = (fun (dictDivisionRing: obj) -> (let mul = (sharpurs_apply (box (Data_Semiring_mul)) (box ((sharpurs_apply (box ((Map.find "Semiring0" (unbox<Map<string, obj>> ((sharpurs_apply (box ((Map.find "Ring0" (unbox<Map<string, obj>> (dictDivisionRing))))) (box (Prim_undefined)))))))) (box (Prim_undefined)))))) in let recip1 = (sharpurs_apply (box (Data_DivisionRing_recip)) (box (dictDivisionRing))) in (fun (a: obj) -> (fun (b: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (mul)) (box ((sharpurs_apply (box (recip1)) (box (b)))))))) (box (a)))))))

let Data_DivisionRing_divisionringNumber  = (sharpurs_apply (box (Data_DivisionRing_DivisionRingusd_Dict)) (box ((Map.add "recip" (box ((fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_DivisionRing_div)) (box ((box 1.0)))))) (box (x)))))) (Map.add "Ring0" (box ((fun (_: obj) -> Data_Ring_ringNumber))) Map.empty)))))
