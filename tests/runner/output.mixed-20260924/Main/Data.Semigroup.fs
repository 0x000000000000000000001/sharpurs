[<AutoOpen>]
module PureScript_Data_Semigroup

open System
open System.Collections.Generic

module Data_Semigroup_FFI =
    let concatString a b = (unbox<string> a) + (unbox<string> b)
    let concatArray a b = undefined
    

let Data_Semigroup_concatString = box (fun (arg0: obj) -> box (fun (arg1: obj) -> box (Data_Semigroup_FFI.``concatString`` (unbox arg0) (unbox arg1))))
let Data_Semigroup_concatArray = box (fun (arg0: obj) -> box (fun (arg1: obj) -> box (Data_Semigroup_FFI.``concatArray`` (unbox arg0) (unbox arg1))))


let Data_Semigroup_SemigroupRecordusd_Dict  = (fun (x: obj) -> x)

let Data_Semigroup_Semigroupusd_Dict  = (fun (x: obj) -> x)

let Data_Semigroup_semigroupVoid  = (sharpurs_apply (box (Data_Semigroup_Semigroupusd_Dict)) (box ((Map.add "append" (box ((fun (v: obj) -> Data_Void_absurd))) Map.empty))))

let Data_Semigroup_semigroupUnit  = (sharpurs_apply (box (Data_Semigroup_Semigroupusd_Dict)) (box ((Map.add "append" (box ((fun (v: obj) -> (fun (v1: obj) -> Data_Unit_unit)))) Map.empty))))

let Data_Semigroup_semigroupString  = (sharpurs_apply (box (Data_Semigroup_Semigroupusd_Dict)) (box ((Map.add "append" (box (Data_Semigroup_concatString)) Map.empty))))

let Data_Semigroup_semigroupRecordNil  = (sharpurs_apply (box (Data_Semigroup_SemigroupRecordusd_Dict)) (box ((Map.add "appendRecord" (box ((fun (v: obj) -> (fun (v1: obj) -> (fun (v2: obj) -> Map.empty))))) Map.empty))))

let Data_Semigroup_semigroupProxy  = (sharpurs_apply (box (Data_Semigroup_Semigroupusd_Dict)) (box ((Map.add "append" (box ((fun (v: obj) -> (fun (v1: obj) -> (box Type_Proxy_Proxyusd_Ctor))))) Map.empty))))

let Data_Semigroup_semigroupArray  = (sharpurs_apply (box (Data_Semigroup_Semigroupusd_Dict)) (box ((Map.add "append" (box (Data_Semigroup_concatArray)) Map.empty))))

let Data_Semigroup_appendRecord  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "appendRecord" (unbox<Map<string, obj>> (v)))))))

let Data_Semigroup_semigroupRecord  = (fun (_: obj) -> (fun (dictSemigroupRecord: obj) -> (sharpurs_apply (box (Data_Semigroup_Semigroupusd_Dict)) (box ((Map.add "append" (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Semigroup_appendRecord)) (box (dictSemigroupRecord))))) (box ((box Type_Proxy_Proxyusd_Ctor)))))) Map.empty))))))

let Data_Semigroup_append  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "append" (unbox<Map<string, obj>> (v)))))))

let Data_Semigroup_semigroupFn  = (fun (dictSemigroup: obj) -> (let append1 = (sharpurs_apply (box (Data_Semigroup_append)) (box (dictSemigroup))) in (sharpurs_apply (box (Data_Semigroup_Semigroupusd_Dict)) (box ((Map.add "append" (box ((fun (f: obj) -> (fun (g: obj) -> (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (append1)) (box ((sharpurs_apply (box (f)) (box (x)))))))) (box ((sharpurs_apply (box (g)) (box (x))))))))))) Map.empty))))))

let Data_Semigroup_semigroupRecordCons  = (fun (dictIsSymbol: obj) -> (let reflectSymbol = (sharpurs_apply (box (Data_Symbol_reflectSymbol)) (box (dictIsSymbol))) in (fun (_: obj) -> (fun (dictSemigroupRecord: obj) -> (let appendRecord1 = (sharpurs_apply (box (Data_Semigroup_appendRecord)) (box (dictSemigroupRecord))) in (fun (dictSemigroup: obj) -> (let append1 = (sharpurs_apply (box (Data_Semigroup_append)) (box (dictSemigroup))) in (sharpurs_apply (box (Data_Semigroup_SemigroupRecordusd_Dict)) (box ((Map.add "appendRecord" (box ((fun (v: obj) -> (fun (ra: obj) -> (fun (rb: obj) -> (let tail = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (appendRecord1)) (box ((box Type_Proxy_Proxyusd_Ctor)))))) (box (ra))))) (box (rb))) in let key = (sharpurs_apply (box (reflectSymbol)) (box ((box Type_Proxy_Proxyusd_Ctor)))) in let insert = (sharpurs_apply (box (Record_Unsafe_unsafeSet)) (box (key))) in let get = (sharpurs_apply (box (Record_Unsafe_unsafeGet)) (box (key))) in (sharpurs_apply (box ((sharpurs_apply (box (insert)) (box ((sharpurs_apply (box ((sharpurs_apply (box (append1)) (box ((sharpurs_apply (box (get)) (box (ra)))))))) (box ((sharpurs_apply (box (get)) (box (rb))))))))))) (box (tail))))))))) Map.empty)))))))))))
