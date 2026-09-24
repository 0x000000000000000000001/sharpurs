[<AutoOpen>]
module PureScript_Control_Extend

open System
open System.Collections.Generic

module Control_Extend_FFI =
    let arrayExtend = box (fun (arg1: obj) -> box (fun (arg2: obj) -> failwith "Not implemented: arrayExtend"))
    

let Control_Extend_arrayExtend = box Control_Extend_FFI.``arrayExtend``


let Control_Extend_identity  = (sharpurs_apply (box (Control_Category_identity)) (box (Control_Category_categoryFn)))

let Control_Extend_Extendusd_Dict  = (fun (x: obj) -> x)

let Control_Extend_extendFn  = (fun (dictSemigroup: obj) -> (let append = (sharpurs_apply (box (Data_Semigroup_append)) (box (dictSemigroup))) in (sharpurs_apply (box (Control_Extend_Extendusd_Dict)) (box ((Map.add "extend" (box ((fun (f: obj) -> (fun (g: obj) -> (fun (w: obj) -> (sharpurs_apply (box (f)) (box ((fun (w_prime: obj) -> (sharpurs_apply (box (g)) (box ((sharpurs_apply (box ((sharpurs_apply (box (append)) (box (w))))) (box (w_prime))))))))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Functor_functorFn))) Map.empty)))))))

let Control_Extend_extendArray  = (sharpurs_apply (box (Control_Extend_Extendusd_Dict)) (box ((Map.add "extend" (box (Control_Extend_arrayExtend)) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Functor_functorArray))) Map.empty)))))

let Control_Extend_extend  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "extend" (unbox<Map<string, obj>> (v)))))))

let Control_Extend_extendFlipped  = (fun (dictExtend: obj) -> (let extend1 = (sharpurs_apply (box (Control_Extend_extend)) (box (dictExtend))) in (fun (w: obj) -> (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (extend1)) (box (f))))) (box (w)))))))

let Control_Extend_duplicate  = (fun (dictExtend: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Control_Extend_extend)) (box (dictExtend))))) (box (Control_Extend_identity))))

let Control_Extend_composeCoKleisliFlipped  = (fun (dictExtend: obj) -> (let extend1 = (sharpurs_apply (box (Control_Extend_extend)) (box (dictExtend))) in (fun (f: obj) -> (fun (g: obj) -> (fun (w: obj) -> (sharpurs_apply (box (f)) (box ((sharpurs_apply (box ((sharpurs_apply (box (extend1)) (box (g))))) (box (w)))))))))))

let Control_Extend_composeCoKleisli  = (fun (dictExtend: obj) -> (let extend1 = (sharpurs_apply (box (Control_Extend_extend)) (box (dictExtend))) in (fun (f: obj) -> (fun (g: obj) -> (fun (w: obj) -> (sharpurs_apply (box (g)) (box ((sharpurs_apply (box ((sharpurs_apply (box (extend1)) (box (f))))) (box (w)))))))))))
