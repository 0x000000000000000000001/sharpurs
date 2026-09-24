[<AutoOpen>]
module PureScript_Data_Ord

open System
open System.Collections.Generic

module Data_Ord_FFI =
    let ordIntImpl lt eq gt x y = let x' = unbox<int> x in let y' = unbox<int> y in if x' < y' then lt else if x' = y' then eq else gt
    let ordNumberImpl lt eq gt x y = let x' = unbox<float> x in let y' = unbox<float> y in if x' < y' then lt else if x' = y' then eq else gt
    let ordStringImpl lt eq gt x y = let x' = unbox<string> x in let y' = unbox<string> y in if x' < y' then lt else if x' = y' then eq else gt
    let ordCharImpl lt eq gt x y = let x' = unbox<char> x in let y' = unbox<char> y in if x' < y' then lt else if x' = y' then eq else gt
    let ordBooleanImpl lt eq gt x y = let x' = unbox<bool> x in let y' = unbox<bool> y in if x' < y' then lt else if x' = y' then eq else gt
    let ordArrayImpl = undefined
    

let Data_Ord_ordBooleanImpl = box (fun (arg0: obj) -> box (fun (arg1: obj) -> box (fun (arg2: obj) -> box (fun (arg3: obj) -> box (fun (arg4: obj) -> box (Data_Ord_FFI.``ordBooleanImpl`` (unbox arg0) (unbox arg1) (unbox arg2) (unbox arg3) (unbox arg4)))))))
let Data_Ord_ordIntImpl = box (fun (arg0: obj) -> box (fun (arg1: obj) -> box (fun (arg2: obj) -> box (fun (arg3: obj) -> box (fun (arg4: obj) -> box (Data_Ord_FFI.``ordIntImpl`` (unbox arg0) (unbox arg1) (unbox arg2) (unbox arg3) (unbox arg4)))))))
let Data_Ord_ordNumberImpl = box (fun (arg0: obj) -> box (fun (arg1: obj) -> box (fun (arg2: obj) -> box (fun (arg3: obj) -> box (fun (arg4: obj) -> box (Data_Ord_FFI.``ordNumberImpl`` (unbox arg0) (unbox arg1) (unbox arg2) (unbox arg3) (unbox arg4)))))))
let Data_Ord_ordStringImpl = box (fun (arg0: obj) -> box (fun (arg1: obj) -> box (fun (arg2: obj) -> box (fun (arg3: obj) -> box (fun (arg4: obj) -> box (Data_Ord_FFI.``ordStringImpl`` (unbox arg0) (unbox arg1) (unbox arg2) (unbox arg3) (unbox arg4)))))))
let Data_Ord_ordCharImpl = box (fun (arg0: obj) -> box (fun (arg1: obj) -> box (fun (arg2: obj) -> box (fun (arg3: obj) -> box (fun (arg4: obj) -> box (Data_Ord_FFI.``ordCharImpl`` (unbox arg0) (unbox arg1) (unbox arg2) (unbox arg3) (unbox arg4)))))))
let Data_Ord_ordArrayImpl = box Data_Ord_FFI.``ordArrayImpl``


let Data_Ord_eqRec  = (sharpurs_apply (box (Data_Eq_eqRec)) (box (Prim_undefined)))

let Data_Ord_negate  = (sharpurs_apply (box (Data_Ring_negate)) (box (Data_Ring_ringInt)))

let Data_Ord_notEq  = (sharpurs_apply (box (Data_Eq_notEq)) (box (Data_Ordering_eqOrdering)))

let Data_Ord_OrdRecordusd_Dict  = (fun (x: obj) -> x)

let Data_Ord_Ordusd_Dict  = (fun (x: obj) -> x)

let Data_Ord_Ord1usd_Dict  = (fun (x: obj) -> x)

let Data_Ord_ordVoid  = (sharpurs_apply (box (Data_Ord_Ordusd_Dict)) (box ((Map.add "compare" (box ((fun (v: obj) -> (fun (v1: obj) -> (box Data_Ordering_EQusd_Ctor))))) (Map.add "Eq0" (box ((fun (_: obj) -> Data_Eq_eqVoid))) Map.empty)))))

let Data_Ord_ordUnit  = (sharpurs_apply (box (Data_Ord_Ordusd_Dict)) (box ((Map.add "compare" (box ((fun (v: obj) -> (fun (v1: obj) -> (box Data_Ordering_EQusd_Ctor))))) (Map.add "Eq0" (box ((fun (_: obj) -> Data_Eq_eqUnit))) Map.empty)))))

let Data_Ord_ordString  = (sharpurs_apply (box (Data_Ord_Ordusd_Dict)) (box ((Map.add "compare" (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Ord_ordStringImpl)) (box ((box Data_Ordering_LTusd_Ctor)))))) (box ((box Data_Ordering_EQusd_Ctor)))))) (box ((box Data_Ordering_GTusd_Ctor)))))) (Map.add "Eq0" (box ((fun (_: obj) -> Data_Eq_eqString))) Map.empty)))))

let Data_Ord_ordRecordNil  = (sharpurs_apply (box (Data_Ord_OrdRecordusd_Dict)) (box ((Map.add "compareRecord" (box ((fun (v: obj) -> (fun (v1: obj) -> (fun (v2: obj) -> (box Data_Ordering_EQusd_Ctor)))))) (Map.add "EqRecord0" (box ((fun (_: obj) -> Data_Eq_eqRowNil))) Map.empty)))))

let Data_Ord_ordProxy  = (sharpurs_apply (box (Data_Ord_Ordusd_Dict)) (box ((Map.add "compare" (box ((fun (v: obj) -> (fun (v1: obj) -> (box Data_Ordering_EQusd_Ctor))))) (Map.add "Eq0" (box ((fun (_: obj) -> Data_Eq_eqProxy))) Map.empty)))))

let Data_Ord_ordOrdering  = (sharpurs_apply (box (Data_Ord_Ordusd_Dict)) (box ((Map.add "compare" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (Data_Ordering_LTusd_Ctor, Data_Ordering_LTusd_Ctor) -> (box ((box Data_Ordering_EQusd_Ctor))) | (Data_Ordering_EQusd_Ctor, Data_Ordering_EQusd_Ctor) -> (box ((box Data_Ordering_EQusd_Ctor))) | (Data_Ordering_GTusd_Ctor, Data_Ordering_GTusd_Ctor) -> (box ((box Data_Ordering_EQusd_Ctor))) | (Data_Ordering_LTusd_Ctor, _) -> (box ((box Data_Ordering_LTusd_Ctor))) | (Data_Ordering_EQusd_Ctor, Data_Ordering_LTusd_Ctor) -> (box ((box Data_Ordering_GTusd_Ctor))) | (Data_Ordering_EQusd_Ctor, Data_Ordering_GTusd_Ctor) -> (box ((box Data_Ordering_LTusd_Ctor))) | (Data_Ordering_GTusd_Ctor, _) -> (box ((box Data_Ordering_GTusd_Ctor)))))))) (Map.add "Eq0" (box ((fun (_: obj) -> Data_Ordering_eqOrdering))) Map.empty)))))

let Data_Ord_ordNumber  = (sharpurs_apply (box (Data_Ord_Ordusd_Dict)) (box ((Map.add "compare" (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Ord_ordNumberImpl)) (box ((box Data_Ordering_LTusd_Ctor)))))) (box ((box Data_Ordering_EQusd_Ctor)))))) (box ((box Data_Ordering_GTusd_Ctor)))))) (Map.add "Eq0" (box ((fun (_: obj) -> Data_Eq_eqNumber))) Map.empty)))))

let Data_Ord_ordInt  = (sharpurs_apply (box (Data_Ord_Ordusd_Dict)) (box ((Map.add "compare" (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Ord_ordIntImpl)) (box ((box Data_Ordering_LTusd_Ctor)))))) (box ((box Data_Ordering_EQusd_Ctor)))))) (box ((box Data_Ordering_GTusd_Ctor)))))) (Map.add "Eq0" (box ((fun (_: obj) -> Data_Eq_eqInt))) Map.empty)))))

let Data_Ord_ordChar  = (sharpurs_apply (box (Data_Ord_Ordusd_Dict)) (box ((Map.add "compare" (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Ord_ordCharImpl)) (box ((box Data_Ordering_LTusd_Ctor)))))) (box ((box Data_Ordering_EQusd_Ctor)))))) (box ((box Data_Ordering_GTusd_Ctor)))))) (Map.add "Eq0" (box ((fun (_: obj) -> Data_Eq_eqChar))) Map.empty)))))

let Data_Ord_ordBoolean  = (sharpurs_apply (box (Data_Ord_Ordusd_Dict)) (box ((Map.add "compare" (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Ord_ordBooleanImpl)) (box ((box Data_Ordering_LTusd_Ctor)))))) (box ((box Data_Ordering_EQusd_Ctor)))))) (box ((box Data_Ordering_GTusd_Ctor)))))) (Map.add "Eq0" (box ((fun (_: obj) -> Data_Eq_eqBoolean))) Map.empty)))))

let Data_Ord_compareRecord  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "compareRecord" (unbox<Map<string, obj>> (v)))))))

let Data_Ord_ordRecord  = (fun (_: obj) -> (fun (dictOrdRecord: obj) -> (let eqRec1 = (sharpurs_apply (box (Data_Ord_eqRec)) (box ((sharpurs_apply (box ((Map.find "EqRecord0" (unbox<Map<string, obj>> (dictOrdRecord))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Ord_Ordusd_Dict)) (box ((Map.add "compare" (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Ord_compareRecord)) (box (dictOrdRecord))))) (box ((box Type_Proxy_Proxyusd_Ctor)))))) (Map.add "Eq0" (box ((fun (_: obj) -> eqRec1))) Map.empty))))))))

let Data_Ord_compare1  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "compare1" (unbox<Map<string, obj>> (v)))))))

let Data_Ord_compare  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "compare" (unbox<Map<string, obj>> (v)))))))

let Data_Ord_compare2  = (sharpurs_apply (box (Data_Ord_compare)) (box (Data_Ord_ordInt)))

let Data_Ord_comparing  = (fun (dictOrd: obj) -> (let compare3 = (sharpurs_apply (box (Data_Ord_compare)) (box (dictOrd))) in (fun (f: obj) -> (fun (x: obj) -> (fun (y: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (compare3)) (box ((sharpurs_apply (box (f)) (box (x)))))))) (box ((sharpurs_apply (box (f)) (box (y)))))))))))

let Data_Ord_greaterThan  = (fun (dictOrd: obj) -> (let compare3 = (sharpurs_apply (box (Data_Ord_compare)) (box (dictOrd))) in (fun (a1: obj) -> (fun (a2: obj) -> (let v = (sharpurs_apply (box ((sharpurs_apply (box (compare3)) (box (a1))))) (box (a2))) in (match ((unbox (v))) with | Data_Ordering_GTusd_Ctor -> (box ((box true))) | _ -> (box ((box false)))))))))

let Data_Ord_greaterThanOrEq  = (fun (dictOrd: obj) -> (let compare3 = (sharpurs_apply (box (Data_Ord_compare)) (box (dictOrd))) in (fun (a1: obj) -> (fun (a2: obj) -> (let v = (sharpurs_apply (box ((sharpurs_apply (box (compare3)) (box (a1))))) (box (a2))) in (match ((unbox (v))) with | Data_Ordering_LTusd_Ctor -> (box ((box false))) | _ -> (box ((box true)))))))))

let Data_Ord_lessThan  = (fun (dictOrd: obj) -> (let compare3 = (sharpurs_apply (box (Data_Ord_compare)) (box (dictOrd))) in (fun (a1: obj) -> (fun (a2: obj) -> (let v = (sharpurs_apply (box ((sharpurs_apply (box (compare3)) (box (a1))))) (box (a2))) in (match ((unbox (v))) with | Data_Ordering_LTusd_Ctor -> (box ((box true))) | _ -> (box ((box false)))))))))

let Data_Ord_signum  = (fun (dictOrd: obj) -> (let lessThan1 = (sharpurs_apply (box (Data_Ord_lessThan)) (box (dictOrd))) in let greaterThan1 = (sharpurs_apply (box (Data_Ord_greaterThan)) (box (dictOrd))) in (fun (dictRing: obj) -> (let Semiring0 = (sharpurs_apply (box ((Map.find "Semiring0" (unbox<Map<string, obj>> (dictRing))))) (box (Prim_undefined))) in let zero = (sharpurs_apply (box (Data_Semiring_zero)) (box (Semiring0))) in let negate1 = (sharpurs_apply (box (Data_Ring_negate)) (box (dictRing))) in let one = (sharpurs_apply (box (Data_Semiring_one)) (box (Semiring0))) in (fun (x: obj) -> (match ((unbox ((sharpurs_apply (box ((sharpurs_apply (box (lessThan1)) (box (x))))) (box (zero)))))) with | LitBool true () -> (box ((sharpurs_apply (box (negate1)) (box (one))))) | _ -> (box ((match ((unbox ((sharpurs_apply (box ((sharpurs_apply (box (greaterThan1)) (box (x))))) (box (zero)))))) with | LitBool true () -> (box (one)) | _ -> (box (x)))))))))))

let Data_Ord_lessThanOrEq  = (fun (dictOrd: obj) -> (let compare3 = (sharpurs_apply (box (Data_Ord_compare)) (box (dictOrd))) in (fun (a1: obj) -> (fun (a2: obj) -> (let v = (sharpurs_apply (box ((sharpurs_apply (box (compare3)) (box (a1))))) (box (a2))) in (match ((unbox (v))) with | Data_Ordering_GTusd_Ctor -> (box ((box false))) | _ -> (box ((box true)))))))))

let Data_Ord_max  = (fun (dictOrd: obj) -> (let compare3 = (sharpurs_apply (box (Data_Ord_compare)) (box (dictOrd))) in (fun (x: obj) -> (fun (y: obj) -> (let v = (sharpurs_apply (box ((sharpurs_apply (box (compare3)) (box (x))))) (box (y))) in (match ((unbox (v))) with | Data_Ordering_LTusd_Ctor -> (box (y)) | Data_Ordering_EQusd_Ctor -> (box (x)) | Data_Ordering_GTusd_Ctor -> (box (x))))))))

let Data_Ord_min  = (fun (dictOrd: obj) -> (let compare3 = (sharpurs_apply (box (Data_Ord_compare)) (box (dictOrd))) in (fun (x: obj) -> (fun (y: obj) -> (let v = (sharpurs_apply (box ((sharpurs_apply (box (compare3)) (box (x))))) (box (y))) in (match ((unbox (v))) with | Data_Ordering_LTusd_Ctor -> (box (x)) | Data_Ordering_EQusd_Ctor -> (box (x)) | Data_Ordering_GTusd_Ctor -> (box (y))))))))

let Data_Ord_ordArray  = (fun (dictOrd: obj) -> (let compare3 = (sharpurs_apply (box (Data_Ord_compare)) (box (dictOrd))) in let eqArray = (sharpurs_apply (box (Data_Eq_eqArray)) (box ((sharpurs_apply (box ((Map.find "Eq0" (unbox<Map<string, obj>> (dictOrd))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Ord_Ordusd_Dict)) (box ((Map.add "compare" (box ((let toDelta = (fun (x: obj) -> (fun (y: obj) -> (let v = (sharpurs_apply (box ((sharpurs_apply (box (compare3)) (box (x))))) (box (y))) in (match ((unbox (v))) with | Data_Ordering_EQusd_Ctor -> (box ((box 0))) | Data_Ordering_LTusd_Ctor -> (box ((box 1))) | Data_Ordering_GTusd_Ctor -> (box ((sharpurs_apply (box (Data_Ord_negate)) (box ((box 1)))))))))) in (fun (xs: obj) -> (fun (ys: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Ord_compare2)) (box ((box 0)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Ord_ordArrayImpl)) (box (toDelta))))) (box (xs))))) (box (ys))))))))))) (Map.add "Eq0" (box ((fun (_: obj) -> eqArray))) Map.empty)))))))

let Data_Ord_ord1Array  = (sharpurs_apply (box (Data_Ord_Ord1usd_Dict)) (box ((Map.add "compare1" (box ((fun (dictOrd: obj) -> (sharpurs_apply (box (Data_Ord_compare)) (box ((sharpurs_apply (box (Data_Ord_ordArray)) (box (dictOrd))))))))) (Map.add "Eq10" (box ((fun (_: obj) -> Data_Eq_eq1Array))) Map.empty)))))

let Data_Ord_ordRecordCons  = (fun (dictOrdRecord: obj) -> (let compareRecord1 = (sharpurs_apply (box (Data_Ord_compareRecord)) (box (dictOrdRecord))) in let eqRowCons = (sharpurs_apply (box ((sharpurs_apply (box (Data_Eq_eqRowCons)) (box ((sharpurs_apply (box ((Map.find "EqRecord0" (unbox<Map<string, obj>> (dictOrdRecord))))) (box (Prim_undefined)))))))) (box (Prim_undefined))) in (fun (_: obj) -> (fun (dictIsSymbol: obj) -> (let reflectSymbol = (sharpurs_apply (box (Data_Symbol_reflectSymbol)) (box (dictIsSymbol))) in let eqRowCons1 = (sharpurs_apply (box (eqRowCons)) (box (dictIsSymbol))) in (fun (dictOrd: obj) -> (let compare3 = (sharpurs_apply (box (Data_Ord_compare)) (box (dictOrd))) in let eqRowCons2 = (sharpurs_apply (box (eqRowCons1)) (box ((sharpurs_apply (box ((Map.find "Eq0" (unbox<Map<string, obj>> (dictOrd))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Ord_OrdRecordusd_Dict)) (box ((Map.add "compareRecord" (box ((fun (v: obj) -> (fun (ra: obj) -> (fun (rb: obj) -> (let unsafeGet_prime = Record_Unsafe_unsafeGet in let key = (sharpurs_apply (box (reflectSymbol)) (box ((box Type_Proxy_Proxyusd_Ctor)))) in let left = (sharpurs_apply (box ((sharpurs_apply (box (compare3)) (box ((sharpurs_apply (box ((sharpurs_apply (box (unsafeGet_prime)) (box (key))))) (box (ra)))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (unsafeGet_prime)) (box (key))))) (box (rb)))))) in (match ((unbox ((sharpurs_apply (box ((sharpurs_apply (box (Data_Ord_notEq)) (box (left))))) (box ((box Data_Ordering_EQusd_Ctor))))))) with | LitBool true () -> (box (left)) | _ -> (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (compareRecord1)) (box ((box Type_Proxy_Proxyusd_Ctor)))))) (box (ra))))) (box (rb)))))))))))) (Map.add "EqRecord0" (box ((fun (_: obj) -> eqRowCons2))) Map.empty))))))))))))

let Data_Ord_clamp  = (fun (dictOrd: obj) -> (let min1 = (sharpurs_apply (box (Data_Ord_min)) (box (dictOrd))) in let max1 = (sharpurs_apply (box (Data_Ord_max)) (box (dictOrd))) in (fun (low: obj) -> (fun (hi: obj) -> (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (min1)) (box (hi))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (max1)) (box (low))))) (box (x)))))))))))

let Data_Ord_between  = (fun (dictOrd: obj) -> (let lessThan1 = (sharpurs_apply (box (Data_Ord_lessThan)) (box (dictOrd))) in let greaterThan1 = (sharpurs_apply (box (Data_Ord_greaterThan)) (box (dictOrd))) in (fun (low: obj) -> (fun (hi: obj) -> (fun (x: obj) -> (match (((unbox (low)), (unbox (hi)), (unbox (x)))) with | (low1, hi1, x1) when (unbox (sharpurs_apply (box ((sharpurs_apply (box (lessThan1)) (box (x1))))) (box (low1)))) -> (box ((box false))) | (low1, hi1, x1) when (unbox (sharpurs_apply (box ((sharpurs_apply (box (greaterThan1)) (box (x1))))) (box (hi1)))) -> (box ((box false))) | (low1, hi1, x1) when (unbox (box true)) -> (box ((box true)))))))))

let Data_Ord_abs  = (fun (dictOrd: obj) -> (let greaterThanOrEq1 = (sharpurs_apply (box (Data_Ord_greaterThanOrEq)) (box (dictOrd))) in (fun (dictRing: obj) -> (let zero = (sharpurs_apply (box (Data_Semiring_zero)) (box ((sharpurs_apply (box ((Map.find "Semiring0" (unbox<Map<string, obj>> (dictRing))))) (box (Prim_undefined)))))) in let negate1 = (sharpurs_apply (box (Data_Ring_negate)) (box (dictRing))) in (fun (x: obj) -> (match ((unbox ((sharpurs_apply (box ((sharpurs_apply (box (greaterThanOrEq1)) (box (x))))) (box (zero)))))) with | LitBool true () -> (box (x)) | _ -> (box ((sharpurs_apply (box (negate1)) (box (x)))))))))))
