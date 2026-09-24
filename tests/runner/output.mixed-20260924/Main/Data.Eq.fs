[<AutoOpen>]
module PureScript_Data_Eq

open System
open System.Collections.Generic

module Data_Eq_FFI =
    let eqIntImpl (a: obj) (b: obj) = unbox<int> a = unbox<int> b
    let eqNumberImpl (a: obj) (b: obj) = unbox<float> a = unbox<float> b
    let eqStringImpl (a: obj) (b: obj) = unbox<string> a = unbox<string> b
    let eqCharImpl (a: obj) (b: obj) = unbox<char> a = unbox<char> b
    let eqBooleanImpl (a: obj) (b: obj) = unbox<bool> a = unbox<bool> b
    let eqArrayImpl (f: obj) (xs: obj) (ys: obj) =
        let xs' = unbox<obj[]> xs
        let ys' = unbox<obj[]> ys
        if xs'.Length <> ys'.Length then box false
        else
            let mutable eq = true
            for i = 0 to xs'.Length - 1 do
                if not (unbox<bool> (sharpurs_apply (sharpurs_apply f xs'.[i]) ys'.[i])) then
                    eq <- false
            box eq
    

let Data_Eq_eqBooleanImpl = box (fun (arg0: obj) -> box (Data_Eq_FFI.``eqBooleanImpl`` (unbox arg0)))
let Data_Eq_eqIntImpl = box (fun (arg0: obj) -> box (Data_Eq_FFI.``eqIntImpl`` (unbox arg0)))
let Data_Eq_eqNumberImpl = box (fun (arg0: obj) -> box (Data_Eq_FFI.``eqNumberImpl`` (unbox arg0)))
let Data_Eq_eqCharImpl = box (fun (arg0: obj) -> box (Data_Eq_FFI.``eqCharImpl`` (unbox arg0)))
let Data_Eq_eqStringImpl = box (fun (arg0: obj) -> box (Data_Eq_FFI.``eqStringImpl`` (unbox arg0)))
let Data_Eq_eqArrayImpl = box (fun (arg0: obj) -> box (Data_Eq_FFI.``eqArrayImpl`` (unbox arg0)))


let Data_Eq_conj  = (sharpurs_apply (box (Data_HeytingAlgebra_conj)) (box (Data_HeytingAlgebra_heytingAlgebraBoolean)))

let Data_Eq_EqRecordusd_Dict  = (fun (x: obj) -> x)

let Data_Eq_Equsd_Dict  = (fun (x: obj) -> x)

let Data_Eq_Eq1usd_Dict  = (fun (x: obj) -> x)

let Data_Eq_eqVoid  = (sharpurs_apply (box (Data_Eq_Equsd_Dict)) (box ((Map.add "eq" (box ((fun (v: obj) -> (fun (v1: obj) -> (box true))))) Map.empty))))

let Data_Eq_eqUnit  = (sharpurs_apply (box (Data_Eq_Equsd_Dict)) (box ((Map.add "eq" (box ((fun (v: obj) -> (fun (v1: obj) -> (box true))))) Map.empty))))

let Data_Eq_eqString  = (sharpurs_apply (box (Data_Eq_Equsd_Dict)) (box ((Map.add "eq" (box (Data_Eq_eqStringImpl)) Map.empty))))

let Data_Eq_eqRowNil  = (sharpurs_apply (box (Data_Eq_EqRecordusd_Dict)) (box ((Map.add "eqRecord" (box ((fun (v: obj) -> (fun (v1: obj) -> (fun (v2: obj) -> (box true)))))) Map.empty))))

let Data_Eq_eqRecord  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "eqRecord" (unbox<Map<string, obj>> (v)))))))

let Data_Eq_eqRec  = (fun (_: obj) -> (fun (dictEqRecord: obj) -> (sharpurs_apply (box (Data_Eq_Equsd_Dict)) (box ((Map.add "eq" (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Eq_eqRecord)) (box (dictEqRecord))))) (box ((box Type_Proxy_Proxyusd_Ctor)))))) Map.empty))))))

let Data_Eq_eqProxy  = (sharpurs_apply (box (Data_Eq_Equsd_Dict)) (box ((Map.add "eq" (box ((fun (v: obj) -> (fun (v1: obj) -> (box true))))) Map.empty))))

let Data_Eq_eqNumber  = (sharpurs_apply (box (Data_Eq_Equsd_Dict)) (box ((Map.add "eq" (box (Data_Eq_eqNumberImpl)) Map.empty))))

let Data_Eq_eqInt  = (sharpurs_apply (box (Data_Eq_Equsd_Dict)) (box ((Map.add "eq" (box (Data_Eq_eqIntImpl)) Map.empty))))

let Data_Eq_eqChar  = (sharpurs_apply (box (Data_Eq_Equsd_Dict)) (box ((Map.add "eq" (box (Data_Eq_eqCharImpl)) Map.empty))))

let Data_Eq_eqBoolean  = (sharpurs_apply (box (Data_Eq_Equsd_Dict)) (box ((Map.add "eq" (box (Data_Eq_eqBooleanImpl)) Map.empty))))

let Data_Eq_eq1  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "eq1" (unbox<Map<string, obj>> (v)))))))

let Data_Eq_eq  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "eq" (unbox<Map<string, obj>> (v)))))))

let Data_Eq_eq2  = (sharpurs_apply (box (Data_Eq_eq)) (box (Data_Eq_eqBoolean)))

let Data_Eq_eqArray  = (fun (dictEq: obj) -> (sharpurs_apply (box (Data_Eq_Equsd_Dict)) (box ((Map.add "eq" (box ((sharpurs_apply (box (Data_Eq_eqArrayImpl)) (box ((sharpurs_apply (box (Data_Eq_eq)) (box (dictEq)))))))) Map.empty)))))

let Data_Eq_eq1Array  = (sharpurs_apply (box (Data_Eq_Eq1usd_Dict)) (box ((Map.add "eq1" (box ((fun (dictEq: obj) -> (sharpurs_apply (box (Data_Eq_eq)) (box ((sharpurs_apply (box (Data_Eq_eqArray)) (box (dictEq))))))))) Map.empty))))

let Data_Eq_eqRowCons  = (fun (dictEqRecord: obj) -> (let eqRecord1 = (sharpurs_apply (box (Data_Eq_eqRecord)) (box (dictEqRecord))) in (fun (_: obj) -> (fun (dictIsSymbol: obj) -> (let reflectSymbol = (sharpurs_apply (box (Data_Symbol_reflectSymbol)) (box (dictIsSymbol))) in (fun (dictEq: obj) -> (let eq3 = (sharpurs_apply (box (Data_Eq_eq)) (box (dictEq))) in (sharpurs_apply (box (Data_Eq_EqRecordusd_Dict)) (box ((Map.add "eqRecord" (box ((fun (v: obj) -> (fun (ra: obj) -> (fun (rb: obj) -> (let tail = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (eqRecord1)) (box ((box Type_Proxy_Proxyusd_Ctor)))))) (box (ra))))) (box (rb))) in let key = (sharpurs_apply (box (reflectSymbol)) (box ((box Type_Proxy_Proxyusd_Ctor)))) in let get = (sharpurs_apply (box (Record_Unsafe_unsafeGet)) (box (key))) in (sharpurs_apply (box ((sharpurs_apply (box (Data_Eq_conj)) (box ((sharpurs_apply (box ((sharpurs_apply (box (eq3)) (box ((sharpurs_apply (box (get)) (box (ra)))))))) (box ((sharpurs_apply (box (get)) (box (rb))))))))))) (box (tail))))))))) Map.empty)))))))))))

let Data_Eq_notEq  = (fun (dictEq: obj) -> (let eq3 = (sharpurs_apply (box (Data_Eq_eq)) (box (dictEq))) in (fun (x: obj) -> (fun (y: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Eq_eq2)) (box ((sharpurs_apply (box ((sharpurs_apply (box (eq3)) (box (x))))) (box (y)))))))) (box ((box false))))))))

let Data_Eq_notEq1  = (fun (dictEq1: obj) -> (let eq11 = (sharpurs_apply (box (Data_Eq_eq1)) (box (dictEq1))) in (fun (dictEq: obj) -> (let eq12 = (sharpurs_apply (box (eq11)) (box (dictEq))) in (fun (x: obj) -> (fun (y: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Eq_eq2)) (box ((sharpurs_apply (box ((sharpurs_apply (box (eq12)) (box (x))))) (box (y)))))))) (box ((box false))))))))))
