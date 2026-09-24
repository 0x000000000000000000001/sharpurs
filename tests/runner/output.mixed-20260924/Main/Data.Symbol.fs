[<AutoOpen>]
module PureScript_Data_Symbol

open System
open System.Collections.Generic

module Data_Symbol_FFI =
    let unsafeCoerce x = x
    

let Data_Symbol_unsafeCoerce = box (fun (arg0: obj) -> box (Data_Symbol_FFI.``unsafeCoerce`` (unbox arg0)))


let Data_Symbol_IsSymbolusd_Dict  = (fun (x: obj) -> x)

let Data_Symbol_reifySymbol  = (fun (s: obj) -> (fun (f: obj) -> (let coerce = Data_Symbol_unsafeCoerce in (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (coerce)) (box ((fun (dictIsSymbol: obj) -> (sharpurs_apply (box (f)) (box (dictIsSymbol))))))))) (box ((Map.add "reflectSymbol" (box ((fun (v: obj) -> s))) Map.empty)))))) (box ((box Type_Proxy_Proxyusd_Ctor)))))))

let Data_Symbol_reflectSymbol  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "reflectSymbol" (unbox<Map<string, obj>> (v)))))))
