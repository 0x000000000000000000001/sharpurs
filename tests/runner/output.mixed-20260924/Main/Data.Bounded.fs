[<AutoOpen>]
module PureScript_Data_Bounded

open System
open System.Collections.Generic

module Data_Bounded_FFI =
    let topChar = '\uffff'
    let bottomChar = '\u0000'
    let topNumber = System.Double.MaxValue
    let bottomNumber = System.Double.MinValue
    let topInt = System.Int32.MaxValue
    let bottomInt = System.Int32.MinValue
    

let Data_Bounded_topInt = box Data_Bounded_FFI.``topInt``
let Data_Bounded_bottomInt = box Data_Bounded_FFI.``bottomInt``
let Data_Bounded_topChar = box Data_Bounded_FFI.``topChar``
let Data_Bounded_bottomChar = box Data_Bounded_FFI.``bottomChar``
let Data_Bounded_topNumber = box Data_Bounded_FFI.``topNumber``
let Data_Bounded_bottomNumber = box Data_Bounded_FFI.``bottomNumber``


let Data_Bounded_ordRecord  = (sharpurs_apply (box (Data_Ord_ordRecord)) (box (Prim_undefined)))

let Data_Bounded_BoundedRecordusd_Dict  = (fun (x: obj) -> x)

let Data_Bounded_Boundedusd_Dict  = (fun (x: obj) -> x)

let Data_Bounded_topRecord  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "topRecord" (unbox<Map<string, obj>> (v)))))))

let Data_Bounded_top  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "top" (unbox<Map<string, obj>> (v)))))))

let Data_Bounded_boundedUnit  = (sharpurs_apply (box (Data_Bounded_Boundedusd_Dict)) (box ((Map.add "top" (box (Data_Unit_unit)) (Map.add "bottom" (box (Data_Unit_unit)) (Map.add "Ord0" (box ((fun (_: obj) -> Data_Ord_ordUnit))) Map.empty))))))

let Data_Bounded_boundedRecordNil  = (sharpurs_apply (box (Data_Bounded_BoundedRecordusd_Dict)) (box ((Map.add "topRecord" (box ((fun (v: obj) -> (fun (v1: obj) -> Map.empty)))) (Map.add "bottomRecord" (box ((fun (v: obj) -> (fun (v1: obj) -> Map.empty)))) (Map.add "OrdRecord0" (box ((fun (_: obj) -> Data_Ord_ordRecordNil))) Map.empty))))))

let Data_Bounded_boundedProxy  = (sharpurs_apply (box (Data_Bounded_Boundedusd_Dict)) (box ((Map.add "bottom" (box ((box Type_Proxy_Proxyusd_Ctor))) (Map.add "top" (box ((box Type_Proxy_Proxyusd_Ctor))) (Map.add "Ord0" (box ((fun (_: obj) -> Data_Ord_ordProxy))) Map.empty))))))

let Data_Bounded_boundedOrdering  = (sharpurs_apply (box (Data_Bounded_Boundedusd_Dict)) (box ((Map.add "top" (box ((box Data_Ordering_GTusd_Ctor))) (Map.add "bottom" (box ((box Data_Ordering_LTusd_Ctor))) (Map.add "Ord0" (box ((fun (_: obj) -> Data_Ord_ordOrdering))) Map.empty))))))

let Data_Bounded_boundedNumber  = (sharpurs_apply (box (Data_Bounded_Boundedusd_Dict)) (box ((Map.add "top" (box (Data_Bounded_topNumber)) (Map.add "bottom" (box (Data_Bounded_bottomNumber)) (Map.add "Ord0" (box ((fun (_: obj) -> Data_Ord_ordNumber))) Map.empty))))))

let Data_Bounded_boundedInt  = (sharpurs_apply (box (Data_Bounded_Boundedusd_Dict)) (box ((Map.add "top" (box (Data_Bounded_topInt)) (Map.add "bottom" (box (Data_Bounded_bottomInt)) (Map.add "Ord0" (box ((fun (_: obj) -> Data_Ord_ordInt))) Map.empty))))))

let Data_Bounded_boundedChar  = (sharpurs_apply (box (Data_Bounded_Boundedusd_Dict)) (box ((Map.add "top" (box (Data_Bounded_topChar)) (Map.add "bottom" (box (Data_Bounded_bottomChar)) (Map.add "Ord0" (box ((fun (_: obj) -> Data_Ord_ordChar))) Map.empty))))))

let Data_Bounded_boundedBoolean  = (sharpurs_apply (box (Data_Bounded_Boundedusd_Dict)) (box ((Map.add "top" (box ((box true))) (Map.add "bottom" (box ((box false))) (Map.add "Ord0" (box ((fun (_: obj) -> Data_Ord_ordBoolean))) Map.empty))))))

let Data_Bounded_bottomRecord  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "bottomRecord" (unbox<Map<string, obj>> (v)))))))

let Data_Bounded_boundedRecord  = (fun (_: obj) -> (fun (dictBoundedRecord: obj) -> (let ordRecord1 = (sharpurs_apply (box (Data_Bounded_ordRecord)) (box ((sharpurs_apply (box ((Map.find "OrdRecord0" (unbox<Map<string, obj>> (dictBoundedRecord))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Bounded_Boundedusd_Dict)) (box ((Map.add "top" (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Bounded_topRecord)) (box (dictBoundedRecord))))) (box ((box Type_Proxy_Proxyusd_Ctor)))))) (box ((box Type_Proxy_Proxyusd_Ctor)))))) (Map.add "bottom" (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Bounded_bottomRecord)) (box (dictBoundedRecord))))) (box ((box Type_Proxy_Proxyusd_Ctor)))))) (box ((box Type_Proxy_Proxyusd_Ctor)))))) (Map.add "Ord0" (box ((fun (_: obj) -> ordRecord1))) Map.empty)))))))))

let Data_Bounded_bottom  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "bottom" (unbox<Map<string, obj>> (v)))))))

let Data_Bounded_boundedRecordCons  = (fun (dictIsSymbol: obj) -> (let reflectSymbol = (sharpurs_apply (box (Data_Symbol_reflectSymbol)) (box (dictIsSymbol))) in (fun (dictBounded: obj) -> (let top1 = (sharpurs_apply (box (Data_Bounded_top)) (box (dictBounded))) in let bottom1 = (sharpurs_apply (box (Data_Bounded_bottom)) (box (dictBounded))) in let Ord0 = (sharpurs_apply (box ((Map.find "Ord0" (unbox<Map<string, obj>> (dictBounded))))) (box (Prim_undefined))) in (fun (_: obj) -> (fun (_: obj) -> (fun (dictBoundedRecord: obj) -> (let topRecord1 = (sharpurs_apply (box (Data_Bounded_topRecord)) (box (dictBoundedRecord))) in let bottomRecord1 = (sharpurs_apply (box (Data_Bounded_bottomRecord)) (box (dictBoundedRecord))) in let ordRecordCons = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Ord_ordRecordCons)) (box ((sharpurs_apply (box ((Map.find "OrdRecord0" (unbox<Map<string, obj>> (dictBoundedRecord))))) (box (Prim_undefined)))))))) (box (Prim_undefined))))) (box (dictIsSymbol))))) (box (Ord0))) in (sharpurs_apply (box (Data_Bounded_BoundedRecordusd_Dict)) (box ((Map.add "topRecord" (box ((fun (v: obj) -> (fun (rowProxy: obj) -> (let tail = (sharpurs_apply (box ((sharpurs_apply (box (topRecord1)) (box ((box Type_Proxy_Proxyusd_Ctor)))))) (box (rowProxy))) in let key = (sharpurs_apply (box (reflectSymbol)) (box ((box Type_Proxy_Proxyusd_Ctor)))) in let insert = (sharpurs_apply (box (Record_Unsafe_unsafeSet)) (box (key))) in (sharpurs_apply (box ((sharpurs_apply (box (insert)) (box (top1))))) (box (tail)))))))) (Map.add "bottomRecord" (box ((fun (v: obj) -> (fun (rowProxy: obj) -> (let tail = (sharpurs_apply (box ((sharpurs_apply (box (bottomRecord1)) (box ((box Type_Proxy_Proxyusd_Ctor)))))) (box (rowProxy))) in let key = (sharpurs_apply (box (reflectSymbol)) (box ((box Type_Proxy_Proxyusd_Ctor)))) in let insert = (sharpurs_apply (box (Record_Unsafe_unsafeSet)) (box (key))) in (sharpurs_apply (box ((sharpurs_apply (box (insert)) (box (bottom1))))) (box (tail)))))))) (Map.add "OrdRecord0" (box ((fun (_: obj) -> ordRecordCons))) Map.empty))))))))))))))
