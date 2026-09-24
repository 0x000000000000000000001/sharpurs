[<AutoOpen>]
module PureScript_Data_Number_Approximate

open System
open System.Collections.Generic

let Data_Number_Approximate_lessThanOrEq  = (sharpurs_apply (box (Data_Ord_lessThanOrEq)) (box (Data_Ord_ordNumber)))

let Data_Number_Approximate_sub  = (sharpurs_apply (box (Data_Ring_sub)) (box (Data_Ring_ringNumber)))

let Data_Number_Approximate_div  = (sharpurs_apply (box (Data_EuclideanRing_div)) (box (Data_EuclideanRing_euclideanRingNumber)))

let Data_Number_Approximate_mul  = (sharpurs_apply (box (Data_Semiring_mul)) (box (Data_Semiring_semiringNumber)))

let Data_Number_Approximate_add  = (sharpurs_apply (box (Data_Semiring_add)) (box (Data_Semiring_semiringNumber)))

let Data_Number_Approximate_not  = (sharpurs_apply (box (Data_HeytingAlgebra_not)) (box (Data_HeytingAlgebra_heytingAlgebraBoolean)))

let Data_Number_Approximate_Tolerance  = (fun (x: obj) -> x)

let Data_Number_Approximate_Fraction  = (fun (x: obj) -> x)

let Data_Number_Approximate_eqRelative  = (fun (v: obj) -> (fun (v1: obj) -> (fun (v2: obj) -> (match (((unbox (v)), (unbox (v1)), (unbox (v2)))) with | (frac, LitNumber 0.0 (), y) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Number_Approximate_lessThanOrEq)) (box ((sharpurs_apply (box (Data_Number_abs)) (box (y)))))))) (box (frac))))) | (frac, x, LitNumber 0.0 ()) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Number_Approximate_lessThanOrEq)) (box ((sharpurs_apply (box (Data_Number_abs)) (box (x)))))))) (box (frac))))) | (frac, x, y) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Number_Approximate_lessThanOrEq)) (box ((sharpurs_apply (box (Data_Number_abs)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Number_Approximate_sub)) (box (x))))) (box (y))))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Number_Approximate_div)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Number_Approximate_mul)) (box (frac))))) (box ((sharpurs_apply (box (Data_Number_abs)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Number_Approximate_add)) (box (x))))) (box (y)))))))))))))) (box ((box 2.0)))))))))))))

let Data_Number_Approximate_eqApproximate  = (let onePPM = (sharpurs_apply (box (Data_Number_Approximate_Fraction)) (box ((box 0.000001)))) in (sharpurs_apply (box (Data_Number_Approximate_eqRelative)) (box (onePPM))))

let Data_Number_Approximate_neqApproximate  = (fun (x: obj) -> (fun (y: obj) -> (sharpurs_apply (box (Data_Number_Approximate_not)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Number_Approximate_eqApproximate)) (box (x))))) (box (y))))))))

let Data_Number_Approximate_eqAbsolute  = (fun (v: obj) -> (fun (x: obj) -> (fun (y: obj) -> (match (((unbox (v)), (unbox (x)), (unbox (y)))) with | (tolerance, x1, y1) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Number_Approximate_lessThanOrEq)) (box ((sharpurs_apply (box (Data_Number_abs)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Number_Approximate_sub)) (box (x1))))) (box (y1))))))))))) (box (tolerance)))))))))
