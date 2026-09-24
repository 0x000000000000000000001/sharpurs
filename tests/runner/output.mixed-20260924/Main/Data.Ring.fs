[<AutoOpen>]
module PureScript_Data_Ring

open System
open System.Collections.Generic

module Data_Ring_FFI =
    let intSub a b = (unbox<int> a) - (unbox<int> b)
    let numSub a b = (unbox<float> a) - (unbox<float> b)
    

let Data_Ring_intSub = box (fun (arg0: obj) -> box (fun (arg1: obj) -> box (Data_Ring_FFI.``intSub`` (unbox arg0) (unbox arg1))))
let Data_Ring_numSub = box (fun (arg0: obj) -> box (fun (arg1: obj) -> box (Data_Ring_FFI.``numSub`` (unbox arg0) (unbox arg1))))


let Data_Ring_semiringRecord  = (sharpurs_apply (box (Data_Semiring_semiringRecord)) (box (Prim_undefined)))

let Data_Ring_RingRecordusd_Dict  = (fun (x: obj) -> x)

let Data_Ring_Ringusd_Dict  = (fun (x: obj) -> x)

let Data_Ring_subRecord  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "subRecord" (unbox<Map<string, obj>> (v)))))))

let Data_Ring_sub  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "sub" (unbox<Map<string, obj>> (v)))))))

let Data_Ring_ringUnit  = (sharpurs_apply (box (Data_Ring_Ringusd_Dict)) (box ((Map.add "sub" (box ((fun (v: obj) -> (fun (v1: obj) -> Data_Unit_unit)))) (Map.add "Semiring0" (box ((fun (_: obj) -> Data_Semiring_semiringUnit))) Map.empty)))))

let Data_Ring_ringRecordNil  = (sharpurs_apply (box (Data_Ring_RingRecordusd_Dict)) (box ((Map.add "subRecord" (box ((fun (v: obj) -> (fun (v1: obj) -> (fun (v2: obj) -> Map.empty))))) (Map.add "SemiringRecord0" (box ((fun (_: obj) -> Data_Semiring_semiringRecordNil))) Map.empty)))))

let Data_Ring_ringRecordCons  = (fun (dictIsSymbol: obj) -> (let reflectSymbol = (sharpurs_apply (box (Data_Symbol_reflectSymbol)) (box (dictIsSymbol))) in let semiringRecordCons = (sharpurs_apply (box ((sharpurs_apply (box (Data_Semiring_semiringRecordCons)) (box (dictIsSymbol))))) (box (Prim_undefined))) in (fun (_: obj) -> (fun (dictRingRecord: obj) -> (let subRecord1 = (sharpurs_apply (box (Data_Ring_subRecord)) (box (dictRingRecord))) in let semiringRecordCons1 = (sharpurs_apply (box (semiringRecordCons)) (box ((sharpurs_apply (box ((Map.find "SemiringRecord0" (unbox<Map<string, obj>> (dictRingRecord))))) (box (Prim_undefined)))))) in (fun (dictRing: obj) -> (let sub1 = (sharpurs_apply (box (Data_Ring_sub)) (box (dictRing))) in let semiringRecordCons2 = (sharpurs_apply (box (semiringRecordCons1)) (box ((sharpurs_apply (box ((Map.find "Semiring0" (unbox<Map<string, obj>> (dictRing))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Ring_RingRecordusd_Dict)) (box ((Map.add "subRecord" (box ((fun (v: obj) -> (fun (ra: obj) -> (fun (rb: obj) -> (let tail = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (subRecord1)) (box ((box Type_Proxy_Proxyusd_Ctor)))))) (box (ra))))) (box (rb))) in let key = (sharpurs_apply (box (reflectSymbol)) (box ((box Type_Proxy_Proxyusd_Ctor)))) in let insert = (sharpurs_apply (box (Record_Unsafe_unsafeSet)) (box (key))) in let get = (sharpurs_apply (box (Record_Unsafe_unsafeGet)) (box (key))) in (sharpurs_apply (box ((sharpurs_apply (box (insert)) (box ((sharpurs_apply (box ((sharpurs_apply (box (sub1)) (box ((sharpurs_apply (box (get)) (box (ra)))))))) (box ((sharpurs_apply (box (get)) (box (rb))))))))))) (box (tail))))))))) (Map.add "SemiringRecord0" (box ((fun (_: obj) -> semiringRecordCons2))) Map.empty))))))))))))

let Data_Ring_ringRecord  = (fun (_: obj) -> (fun (dictRingRecord: obj) -> (let semiringRecord1 = (sharpurs_apply (box (Data_Ring_semiringRecord)) (box ((sharpurs_apply (box ((Map.find "SemiringRecord0" (unbox<Map<string, obj>> (dictRingRecord))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Ring_Ringusd_Dict)) (box ((Map.add "sub" (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Ring_subRecord)) (box (dictRingRecord))))) (box ((box Type_Proxy_Proxyusd_Ctor)))))) (Map.add "Semiring0" (box ((fun (_: obj) -> semiringRecord1))) Map.empty))))))))

let Data_Ring_ringProxy  = (sharpurs_apply (box (Data_Ring_Ringusd_Dict)) (box ((Map.add "sub" (box ((fun (v: obj) -> (fun (v1: obj) -> (box Type_Proxy_Proxyusd_Ctor))))) (Map.add "Semiring0" (box ((fun (_: obj) -> Data_Semiring_semiringProxy))) Map.empty)))))

let Data_Ring_ringNumber  = (sharpurs_apply (box (Data_Ring_Ringusd_Dict)) (box ((Map.add "sub" (box (Data_Ring_numSub)) (Map.add "Semiring0" (box ((fun (_: obj) -> Data_Semiring_semiringNumber))) Map.empty)))))

let Data_Ring_ringInt  = (sharpurs_apply (box (Data_Ring_Ringusd_Dict)) (box ((Map.add "sub" (box (Data_Ring_intSub)) (Map.add "Semiring0" (box ((fun (_: obj) -> Data_Semiring_semiringInt))) Map.empty)))))

let Data_Ring_ringFn  = (fun (dictRing: obj) -> (let sub1 = (sharpurs_apply (box (Data_Ring_sub)) (box (dictRing))) in let semiringFn = (sharpurs_apply (box (Data_Semiring_semiringFn)) (box ((sharpurs_apply (box ((Map.find "Semiring0" (unbox<Map<string, obj>> (dictRing))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Ring_Ringusd_Dict)) (box ((Map.add "sub" (box ((fun (f: obj) -> (fun (g: obj) -> (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (sub1)) (box ((sharpurs_apply (box (f)) (box (x)))))))) (box ((sharpurs_apply (box (g)) (box (x))))))))))) (Map.add "Semiring0" (box ((fun (_: obj) -> semiringFn))) Map.empty)))))))

let Data_Ring_negate  = (fun (dictRing: obj) -> (let sub1 = (sharpurs_apply (box (Data_Ring_sub)) (box (dictRing))) in let zero = (sharpurs_apply (box (Data_Semiring_zero)) (box ((sharpurs_apply (box ((Map.find "Semiring0" (unbox<Map<string, obj>> (dictRing))))) (box (Prim_undefined)))))) in (fun (a: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (sub1)) (box (zero))))) (box (a))))))
