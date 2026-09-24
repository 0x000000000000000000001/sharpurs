[<AutoOpen>]
module PureScript_Record

open System
open System.Collections.Generic

let Record_conj  = (sharpurs_apply (box (Data_HeytingAlgebra_conj)) (box (Data_HeytingAlgebra_heytingAlgebraBoolean)))

let Record_EqualFieldsusd_Dict  = (fun (x: obj) -> x)

let Record_union  = (fun (_: obj) -> (fun (l: obj) -> (fun (r: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Function_Uncurried_runFn2)) (box (Record_Unsafe_Union_unsafeUnionFn))))) (box (l))))) (box (r))))))

let Record_set  = (fun (dictIsSymbol: obj) -> (let reflectSymbol = (sharpurs_apply (box (Data_Symbol_reflectSymbol)) (box (dictIsSymbol))) in (fun (_: obj) -> (fun (_: obj) -> (fun (l: obj) -> (fun (b: obj) -> (fun (r: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Record_Unsafe_unsafeSet)) (box ((sharpurs_apply (box (reflectSymbol)) (box (l)))))))) (box (b))))) (box (r))))))))))

let Record_nub  = (fun (_: obj) -> Unsafe_Coerce_unsafeCoerce)

let Record_merge  = (fun (_: obj) -> (fun (_: obj) -> (fun (l: obj) -> (fun (r: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Function_Uncurried_runFn2)) (box (Record_Unsafe_Union_unsafeUnionFn))))) (box (l))))) (box (r)))))))

let Record_insert  = (fun (dictIsSymbol: obj) -> (let reflectSymbol = (sharpurs_apply (box (Data_Symbol_reflectSymbol)) (box (dictIsSymbol))) in (fun (_: obj) -> (fun (_: obj) -> (fun (l: obj) -> (fun (a: obj) -> (fun (r: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Record_Unsafe_unsafeSet)) (box ((sharpurs_apply (box (reflectSymbol)) (box (l)))))))) (box (a))))) (box (r))))))))))

let Record_get  = (fun (dictIsSymbol: obj) -> (let reflectSymbol = (sharpurs_apply (box (Data_Symbol_reflectSymbol)) (box (dictIsSymbol))) in (fun (_: obj) -> (fun (l: obj) -> (fun (r: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Record_Unsafe_unsafeGet)) (box ((sharpurs_apply (box (reflectSymbol)) (box (l)))))))) (box (r))))))))

let Record_modify  = (fun (dictIsSymbol: obj) -> (let set1 = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Record_set)) (box (dictIsSymbol))))) (box (Prim_undefined))))) (box (Prim_undefined))) in let get1 = (sharpurs_apply (box ((sharpurs_apply (box (Record_get)) (box (dictIsSymbol))))) (box (Prim_undefined))) in (fun (_: obj) -> (fun (_: obj) -> (fun (l: obj) -> (fun (f: obj) -> (fun (r: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (set1)) (box (l))))) (box ((sharpurs_apply (box (f)) (box ((sharpurs_apply (box ((sharpurs_apply (box (get1)) (box (l))))) (box (r))))))))))) (box (r))))))))))

let Record_equalFieldsNil  = (sharpurs_apply (box (Record_EqualFieldsusd_Dict)) (box ((Map.add "equalFields" (box ((fun (v: obj) -> (fun (v1: obj) -> (fun (v2: obj) -> (box true)))))) Map.empty))))

let Record_equalFields  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "equalFields" (unbox<Map<string, obj>> (v)))))))

let Record_equalFieldsCons  = (fun (dictIsSymbol: obj) -> (let get1 = (sharpurs_apply (box ((sharpurs_apply (box (Record_get)) (box (dictIsSymbol))))) (box (Prim_undefined))) in (fun (dictEq: obj) -> (let eq = (sharpurs_apply (box (Data_Eq_eq)) (box (dictEq))) in (fun (_: obj) -> (fun (dictEqualFields: obj) -> (let equalFields1 = (sharpurs_apply (box (Record_equalFields)) (box (dictEqualFields))) in (sharpurs_apply (box (Record_EqualFieldsusd_Dict)) (box ((Map.add "equalFields" (box ((fun (v: obj) -> (fun (a: obj) -> (fun (b: obj) -> (let get_prime = (sharpurs_apply (box (get1)) (box ((box Type_Proxy_Proxyusd_Ctor)))) in let equalRest = (sharpurs_apply (box (equalFields1)) (box ((box Type_Proxy_Proxyusd_Ctor)))) in (sharpurs_apply (box ((sharpurs_apply (box (Record_conj)) (box ((sharpurs_apply (box ((sharpurs_apply (box (eq)) (box ((sharpurs_apply (box (get_prime)) (box (a)))))))) (box ((sharpurs_apply (box (get_prime)) (box (b))))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (equalRest)) (box (a))))) (box (b)))))))))))) Map.empty)))))))))))

let Record_equal  = (fun (_: obj) -> (fun (dictEqualFields: obj) -> (let equalFields1 = (sharpurs_apply (box (Record_equalFields)) (box (dictEqualFields))) in (fun (a: obj) -> (fun (b: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (equalFields1)) (box ((box Type_Proxy_Proxyusd_Ctor)))))) (box (a))))) (box (b))))))))

let Record_disjointUnion  = (fun (_: obj) -> (fun (_: obj) -> (fun (l: obj) -> (fun (r: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Function_Uncurried_runFn2)) (box (Record_Unsafe_Union_unsafeUnionFn))))) (box (l))))) (box (r)))))))

let Record_delete  = (fun (dictIsSymbol: obj) -> (let reflectSymbol = (sharpurs_apply (box (Data_Symbol_reflectSymbol)) (box (dictIsSymbol))) in (fun (_: obj) -> (fun (_: obj) -> (fun (l: obj) -> (fun (r: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Record_Unsafe_unsafeDelete)) (box ((sharpurs_apply (box (reflectSymbol)) (box (l)))))))) (box (r)))))))))

let Record_rename  = (fun (dictIsSymbol: obj) -> (let get1 = (sharpurs_apply (box ((sharpurs_apply (box (Record_get)) (box (dictIsSymbol))))) (box (Prim_undefined))) in let delete1 = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Record_delete)) (box (dictIsSymbol))))) (box (Prim_undefined))))) (box (Prim_undefined))) in (fun (dictIsSymbol1: obj) -> (let insert1 = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Record_insert)) (box (dictIsSymbol1))))) (box (Prim_undefined))))) (box (Prim_undefined))) in (fun (_: obj) -> (fun (_: obj) -> (fun (_: obj) -> (fun (_: obj) -> (fun (prev: obj) -> (fun (next: obj) -> (fun (record: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (insert1)) (box (next))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (get1)) (box (prev))))) (box (record)))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (delete1)) (box (prev))))) (box (record)))))))))))))))))
