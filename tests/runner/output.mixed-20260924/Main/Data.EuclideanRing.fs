[<AutoOpen>]
module PureScript_Data_EuclideanRing

open System
open System.Collections.Generic

module Data_EuclideanRing_FFI =
    let intDiv a b = (unbox<int> a) / (unbox<int> b)
    let intDegree a = System.Math.Abs(unbox<int> a)
    let numDiv a b = (unbox<float> a) / (unbox<float> b)
    let intMod a b =
        let x = unbox<int> a
        let y = unbox<int> b
        if y = 0 then 0
        else
            let yy = System.Math.Abs(y)
            ((x % yy) + yy) % yy
    

let Data_EuclideanRing_intDegree = box (fun (arg0: obj) -> box (Data_EuclideanRing_FFI.``intDegree`` (unbox arg0)))
let Data_EuclideanRing_intDiv = box (fun (arg0: obj) -> box (fun (arg1: obj) -> box (Data_EuclideanRing_FFI.``intDiv`` (unbox arg0) (unbox arg1))))
let Data_EuclideanRing_intMod = box (fun (arg0: obj) -> box (fun (arg1: obj) -> box (Data_EuclideanRing_FFI.``intMod`` (unbox arg0) (unbox arg1))))
let Data_EuclideanRing_numDiv = box (fun (arg0: obj) -> box (fun (arg1: obj) -> box (Data_EuclideanRing_FFI.``numDiv`` (unbox arg0) (unbox arg1))))


let Data_EuclideanRing_disj  = (sharpurs_apply (box (Data_HeytingAlgebra_disj)) (box (Data_HeytingAlgebra_heytingAlgebraBoolean)))

let Data_EuclideanRing_EuclideanRingusd_Dict  = (fun (x: obj) -> x)

let Data_EuclideanRing_mod  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "mod" (unbox<Map<string, obj>> (v)))))))

let rec Data_EuclideanRing_gcd  = (fun (dictEq: obj) -> (let eq = (sharpurs_apply (box (Data_Eq_eq)) (box (dictEq))) in (fun (dictEuclideanRing: obj) -> (let zero = (sharpurs_apply (box (Data_Semiring_zero)) (box ((sharpurs_apply (box ((Map.find "Semiring0" (unbox<Map<string, obj>> ((sharpurs_apply (box ((Map.find "Ring0" (unbox<Map<string, obj>> ((sharpurs_apply (box ((Map.find "CommutativeRing0" (unbox<Map<string, obj>> (dictEuclideanRing))))) (box (Prim_undefined)))))))) (box (Prim_undefined)))))))) (box (Prim_undefined)))))) in let mod1 = (sharpurs_apply (box (Data_EuclideanRing_mod)) (box (dictEuclideanRing))) in (fun (a: obj) -> (fun (b: obj) -> (match ((unbox ((sharpurs_apply (box ((sharpurs_apply (box (eq)) (box (b))))) (box (zero)))))) with | LitBool true () -> (box (a)) | _ -> (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_EuclideanRing_gcd)) (box (dictEq))))) (box (dictEuclideanRing))))) (box (b))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (mod1)) (box (a))))) (box (b)))))))))))))))

let Data_EuclideanRing_euclideanRingNumber  = (sharpurs_apply (box (Data_EuclideanRing_EuclideanRingusd_Dict)) (box ((Map.add "degree" (box ((fun (v: obj) -> (box 1)))) (Map.add "div" (box (Data_EuclideanRing_numDiv)) (Map.add "mod" (box ((fun (v: obj) -> (fun (v1: obj) -> (box 0.0))))) (Map.add "CommutativeRing0" (box ((fun (_: obj) -> Data_CommutativeRing_commutativeRingNumber))) Map.empty)))))))

let Data_EuclideanRing_euclideanRingInt  = (sharpurs_apply (box (Data_EuclideanRing_EuclideanRingusd_Dict)) (box ((Map.add "degree" (box (Data_EuclideanRing_intDegree)) (Map.add "div" (box (Data_EuclideanRing_intDiv)) (Map.add "mod" (box (Data_EuclideanRing_intMod)) (Map.add "CommutativeRing0" (box ((fun (_: obj) -> Data_CommutativeRing_commutativeRingInt))) Map.empty)))))))

let Data_EuclideanRing_div  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "div" (unbox<Map<string, obj>> (v)))))))

let Data_EuclideanRing_lcm  = (fun (dictEq: obj) -> (let eq = (sharpurs_apply (box (Data_Eq_eq)) (box (dictEq))) in let gcd1 = (sharpurs_apply (box (Data_EuclideanRing_gcd)) (box (dictEq))) in (fun (dictEuclideanRing: obj) -> (let Semiring0 = (sharpurs_apply (box ((Map.find "Semiring0" (unbox<Map<string, obj>> ((sharpurs_apply (box ((Map.find "Ring0" (unbox<Map<string, obj>> ((sharpurs_apply (box ((Map.find "CommutativeRing0" (unbox<Map<string, obj>> (dictEuclideanRing))))) (box (Prim_undefined)))))))) (box (Prim_undefined)))))))) (box (Prim_undefined))) in let zero = (sharpurs_apply (box (Data_Semiring_zero)) (box (Semiring0))) in let div1 = (sharpurs_apply (box (Data_EuclideanRing_div)) (box (dictEuclideanRing))) in let mul = (sharpurs_apply (box (Data_Semiring_mul)) (box (Semiring0))) in let gcd2 = (sharpurs_apply (box (gcd1)) (box (dictEuclideanRing))) in (fun (a: obj) -> (fun (b: obj) -> (match ((unbox ((sharpurs_apply (box ((sharpurs_apply (box (Data_EuclideanRing_disj)) (box ((sharpurs_apply (box ((sharpurs_apply (box (eq)) (box (a))))) (box (zero)))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (eq)) (box (b))))) (box (zero))))))))) with | LitBool true () -> (box (zero)) | _ -> (box ((sharpurs_apply (box ((sharpurs_apply (box (div1)) (box ((sharpurs_apply (box ((sharpurs_apply (box (mul)) (box (a))))) (box (b)))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (gcd2)) (box (a))))) (box (b)))))))))))))))

let Data_EuclideanRing_degree  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "degree" (unbox<Map<string, obj>> (v)))))))
