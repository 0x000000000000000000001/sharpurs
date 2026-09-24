[<AutoOpen>]
module PureScript_Data_Semiring

open System
open System.Collections.Generic

module Data_Semiring_FFI =
    let intAdd a b = (unbox<int> a) + (unbox<int> b)
    let intMul a b = (unbox<int> a) * (unbox<int> b)
    let numAdd a b = (unbox<float> a) + (unbox<float> b)
    let numMul a b = (unbox<float> a) * (unbox<float> b)
    

let Data_Semiring_intAdd = box (fun (arg0: obj) -> box (fun (arg1: obj) -> box (Data_Semiring_FFI.``intAdd`` (unbox arg0) (unbox arg1))))
let Data_Semiring_intMul = box (fun (arg0: obj) -> box (fun (arg1: obj) -> box (Data_Semiring_FFI.``intMul`` (unbox arg0) (unbox arg1))))
let Data_Semiring_numAdd = box (fun (arg0: obj) -> box (fun (arg1: obj) -> box (Data_Semiring_FFI.``numAdd`` (unbox arg0) (unbox arg1))))
let Data_Semiring_numMul = box (fun (arg0: obj) -> box (fun (arg1: obj) -> box (Data_Semiring_FFI.``numMul`` (unbox arg0) (unbox arg1))))


let Data_Semiring_SemiringRecordusd_Dict  = (fun (x: obj) -> x)

let Data_Semiring_Semiringusd_Dict  = (fun (x: obj) -> x)

let Data_Semiring_zeroRecord  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "zeroRecord" (unbox<Map<string, obj>> (v)))))))

let Data_Semiring_zero  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "zero" (unbox<Map<string, obj>> (v)))))))

let Data_Semiring_semiringUnit  = (sharpurs_apply (box (Data_Semiring_Semiringusd_Dict)) (box ((Map.add "add" (box ((fun (v: obj) -> (fun (v1: obj) -> Data_Unit_unit)))) (Map.add "zero" (box (Data_Unit_unit)) (Map.add "mul" (box ((fun (v: obj) -> (fun (v1: obj) -> Data_Unit_unit)))) (Map.add "one" (box (Data_Unit_unit)) Map.empty)))))))

let Data_Semiring_semiringRecordNil  = (sharpurs_apply (box (Data_Semiring_SemiringRecordusd_Dict)) (box ((Map.add "addRecord" (box ((fun (v: obj) -> (fun (v1: obj) -> (fun (v2: obj) -> Map.empty))))) (Map.add "mulRecord" (box ((fun (v: obj) -> (fun (v1: obj) -> (fun (v2: obj) -> Map.empty))))) (Map.add "oneRecord" (box ((fun (v: obj) -> (fun (v1: obj) -> Map.empty)))) (Map.add "zeroRecord" (box ((fun (v: obj) -> (fun (v1: obj) -> Map.empty)))) Map.empty)))))))

let Data_Semiring_semiringProxy  = (sharpurs_apply (box (Data_Semiring_Semiringusd_Dict)) (box ((Map.add "add" (box ((fun (v: obj) -> (fun (v1: obj) -> (box Type_Proxy_Proxyusd_Ctor))))) (Map.add "mul" (box ((fun (v: obj) -> (fun (v1: obj) -> (box Type_Proxy_Proxyusd_Ctor))))) (Map.add "one" (box ((box Type_Proxy_Proxyusd_Ctor))) (Map.add "zero" (box ((box Type_Proxy_Proxyusd_Ctor))) Map.empty)))))))

let Data_Semiring_semiringNumber  = (sharpurs_apply (box (Data_Semiring_Semiringusd_Dict)) (box ((Map.add "add" (box (Data_Semiring_numAdd)) (Map.add "zero" (box ((box 0.0))) (Map.add "mul" (box (Data_Semiring_numMul)) (Map.add "one" (box ((box 1.0))) Map.empty)))))))

let Data_Semiring_semiringInt  = (sharpurs_apply (box (Data_Semiring_Semiringusd_Dict)) (box ((Map.add "add" (box (Data_Semiring_intAdd)) (Map.add "zero" (box ((box 0))) (Map.add "mul" (box (Data_Semiring_intMul)) (Map.add "one" (box ((box 1))) Map.empty)))))))

let Data_Semiring_oneRecord  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "oneRecord" (unbox<Map<string, obj>> (v)))))))

let Data_Semiring_one  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "one" (unbox<Map<string, obj>> (v)))))))

let Data_Semiring_mulRecord  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "mulRecord" (unbox<Map<string, obj>> (v)))))))

let Data_Semiring_mul  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "mul" (unbox<Map<string, obj>> (v)))))))

let Data_Semiring_addRecord  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "addRecord" (unbox<Map<string, obj>> (v)))))))

let Data_Semiring_semiringRecord  = (fun (_: obj) -> (fun (dictSemiringRecord: obj) -> (sharpurs_apply (box (Data_Semiring_Semiringusd_Dict)) (box ((Map.add "add" (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Semiring_addRecord)) (box (dictSemiringRecord))))) (box ((box Type_Proxy_Proxyusd_Ctor)))))) (Map.add "mul" (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Semiring_mulRecord)) (box (dictSemiringRecord))))) (box ((box Type_Proxy_Proxyusd_Ctor)))))) (Map.add "one" (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Semiring_oneRecord)) (box (dictSemiringRecord))))) (box ((box Type_Proxy_Proxyusd_Ctor)))))) (box ((box Type_Proxy_Proxyusd_Ctor)))))) (Map.add "zero" (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Semiring_zeroRecord)) (box (dictSemiringRecord))))) (box ((box Type_Proxy_Proxyusd_Ctor)))))) (box ((box Type_Proxy_Proxyusd_Ctor)))))) Map.empty)))))))))

let Data_Semiring_add  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "add" (unbox<Map<string, obj>> (v)))))))

let Data_Semiring_semiringFn  = (fun (dictSemiring: obj) -> (let add1 = (sharpurs_apply (box (Data_Semiring_add)) (box (dictSemiring))) in let zero1 = (sharpurs_apply (box (Data_Semiring_zero)) (box (dictSemiring))) in let mul1 = (sharpurs_apply (box (Data_Semiring_mul)) (box (dictSemiring))) in let one1 = (sharpurs_apply (box (Data_Semiring_one)) (box (dictSemiring))) in (sharpurs_apply (box (Data_Semiring_Semiringusd_Dict)) (box ((Map.add "add" (box ((fun (f: obj) -> (fun (g: obj) -> (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (add1)) (box ((sharpurs_apply (box (f)) (box (x)))))))) (box ((sharpurs_apply (box (g)) (box (x))))))))))) (Map.add "zero" (box ((fun (v: obj) -> zero1))) (Map.add "mul" (box ((fun (f: obj) -> (fun (g: obj) -> (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (mul1)) (box ((sharpurs_apply (box (f)) (box (x)))))))) (box ((sharpurs_apply (box (g)) (box (x))))))))))) (Map.add "one" (box ((fun (v: obj) -> one1))) Map.empty)))))))))

let Data_Semiring_semiringRecordCons  = (fun (dictIsSymbol: obj) -> (let reflectSymbol = (sharpurs_apply (box (Data_Symbol_reflectSymbol)) (box (dictIsSymbol))) in (fun (_: obj) -> (fun (dictSemiringRecord: obj) -> (let addRecord1 = (sharpurs_apply (box (Data_Semiring_addRecord)) (box (dictSemiringRecord))) in let mulRecord1 = (sharpurs_apply (box (Data_Semiring_mulRecord)) (box (dictSemiringRecord))) in let oneRecord1 = (sharpurs_apply (box (Data_Semiring_oneRecord)) (box (dictSemiringRecord))) in let zeroRecord1 = (sharpurs_apply (box (Data_Semiring_zeroRecord)) (box (dictSemiringRecord))) in (fun (dictSemiring: obj) -> (let add1 = (sharpurs_apply (box (Data_Semiring_add)) (box (dictSemiring))) in let mul1 = (sharpurs_apply (box (Data_Semiring_mul)) (box (dictSemiring))) in let one1 = (sharpurs_apply (box (Data_Semiring_one)) (box (dictSemiring))) in let zero1 = (sharpurs_apply (box (Data_Semiring_zero)) (box (dictSemiring))) in (sharpurs_apply (box (Data_Semiring_SemiringRecordusd_Dict)) (box ((Map.add "addRecord" (box ((fun (v: obj) -> (fun (ra: obj) -> (fun (rb: obj) -> (let tail = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (addRecord1)) (box ((box Type_Proxy_Proxyusd_Ctor)))))) (box (ra))))) (box (rb))) in let key = (sharpurs_apply (box (reflectSymbol)) (box ((box Type_Proxy_Proxyusd_Ctor)))) in let insert = (sharpurs_apply (box (Record_Unsafe_unsafeSet)) (box (key))) in let get = (sharpurs_apply (box (Record_Unsafe_unsafeGet)) (box (key))) in (sharpurs_apply (box ((sharpurs_apply (box (insert)) (box ((sharpurs_apply (box ((sharpurs_apply (box (add1)) (box ((sharpurs_apply (box (get)) (box (ra)))))))) (box ((sharpurs_apply (box (get)) (box (rb))))))))))) (box (tail))))))))) (Map.add "mulRecord" (box ((fun (v: obj) -> (fun (ra: obj) -> (fun (rb: obj) -> (let tail = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (mulRecord1)) (box ((box Type_Proxy_Proxyusd_Ctor)))))) (box (ra))))) (box (rb))) in let key = (sharpurs_apply (box (reflectSymbol)) (box ((box Type_Proxy_Proxyusd_Ctor)))) in let insert = (sharpurs_apply (box (Record_Unsafe_unsafeSet)) (box (key))) in let get = (sharpurs_apply (box (Record_Unsafe_unsafeGet)) (box (key))) in (sharpurs_apply (box ((sharpurs_apply (box (insert)) (box ((sharpurs_apply (box ((sharpurs_apply (box (mul1)) (box ((sharpurs_apply (box (get)) (box (ra)))))))) (box ((sharpurs_apply (box (get)) (box (rb))))))))))) (box (tail))))))))) (Map.add "oneRecord" (box ((fun (v: obj) -> (fun (v1: obj) -> (let tail = (sharpurs_apply (box ((sharpurs_apply (box (oneRecord1)) (box ((box Type_Proxy_Proxyusd_Ctor)))))) (box ((box Type_Proxy_Proxyusd_Ctor)))) in let key = (sharpurs_apply (box (reflectSymbol)) (box ((box Type_Proxy_Proxyusd_Ctor)))) in let insert = (sharpurs_apply (box (Record_Unsafe_unsafeSet)) (box (key))) in (sharpurs_apply (box ((sharpurs_apply (box (insert)) (box (one1))))) (box (tail)))))))) (Map.add "zeroRecord" (box ((fun (v: obj) -> (fun (v1: obj) -> (let tail = (sharpurs_apply (box ((sharpurs_apply (box (zeroRecord1)) (box ((box Type_Proxy_Proxyusd_Ctor)))))) (box ((box Type_Proxy_Proxyusd_Ctor)))) in let key = (sharpurs_apply (box (reflectSymbol)) (box ((box Type_Proxy_Proxyusd_Ctor)))) in let insert = (sharpurs_apply (box (Record_Unsafe_unsafeSet)) (box (key))) in (sharpurs_apply (box ((sharpurs_apply (box (insert)) (box (zero1))))) (box (tail)))))))) Map.empty))))))))))))))
