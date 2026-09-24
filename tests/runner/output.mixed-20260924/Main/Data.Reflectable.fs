[<AutoOpen>]
module PureScript_Data_Reflectable

open System
open System.Collections.Generic

module Data_Reflectable_FFI =
    let unsafeCoerce x = x
    

let Data_Reflectable_unsafeCoerce = box (fun (arg0: obj) -> box (Data_Reflectable_FFI.``unsafeCoerce`` (unbox arg0)))


let Data_Reflectable_Reifiableusd_Dict  = (fun (x: obj) -> x)

let Data_Reflectable_Reflectableusd_Dict  = (fun (x: obj) -> x)

let Data_Reflectable_reifiableString  = (sharpurs_apply (box (Data_Reflectable_Reifiableusd_Dict)) (box (Map.empty)))

let Data_Reflectable_reifiableOrdering  = (sharpurs_apply (box (Data_Reflectable_Reifiableusd_Dict)) (box (Map.empty)))

let Data_Reflectable_reifiableInt  = (sharpurs_apply (box (Data_Reflectable_Reifiableusd_Dict)) (box (Map.empty)))

let Data_Reflectable_reifiableBoolean  = (sharpurs_apply (box (Data_Reflectable_Reifiableusd_Dict)) (box (Map.empty)))

let Data_Reflectable_reifyType  = (fun (_: obj) -> (fun (s: obj) -> (fun (f: obj) -> (let coerce = Data_Reflectable_unsafeCoerce in (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (coerce)) (box ((fun (dictReflectable: obj) -> (sharpurs_apply (box (f)) (box (dictReflectable))))))))) (box ((Map.add "reflectType" (box ((fun (v: obj) -> s))) Map.empty)))))) (box ((box Type_Proxy_Proxyusd_Ctor))))))))

let Data_Reflectable_reflectType  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "reflectType" (unbox<Map<string, obj>> (v)))))))
