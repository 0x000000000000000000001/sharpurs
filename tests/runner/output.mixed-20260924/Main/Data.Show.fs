[<AutoOpen>]
module PureScript_Data_Show

open System
open System.Collections.Generic

module Data_Show_FFI =
    let showIntImpl (x: obj) : obj = box (string (unbox<int> x))
    let showNumberImpl (x: obj) : obj = box (string (unbox<float> x))
    let showStringImpl (x: obj) : obj = box ("\"" + unbox<string> x + "\"")
    let showCharImpl c = string (unbox<char> c)
    let showArrayImpl a = undefined
    

let Data_Show_showIntImpl = box (fun (arg0: obj) -> box (Data_Show_FFI.``showIntImpl`` (unbox arg0)))
let Data_Show_showNumberImpl = box (fun (arg0: obj) -> box (Data_Show_FFI.``showNumberImpl`` (unbox arg0)))
let Data_Show_showCharImpl = box (fun (arg0: obj) -> box (Data_Show_FFI.``showCharImpl`` (unbox arg0)))
let Data_Show_showStringImpl = box (fun (arg0: obj) -> box (Data_Show_FFI.``showStringImpl`` (unbox arg0)))
let Data_Show_showArrayImpl = box (fun (arg0: obj) -> box (Data_Show_FFI.``showArrayImpl`` (unbox arg0)))


let Data_Show_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Semigroup_semigroupString)))

let Data_Show_ShowRecordFieldsusd_Dict  = (fun (x: obj) -> x)

let Data_Show_Showusd_Dict  = (fun (x: obj) -> x)

let Data_Show_showVoid  = (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box (Data_Void_absurd)) Map.empty))))

let Data_Show_showUnit  = (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (box "unit")))) Map.empty))))

let Data_Show_showString  = (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box (Data_Show_showStringImpl)) Map.empty))))

let Data_Show_showRecordFieldsNil  = (sharpurs_apply (box (Data_Show_ShowRecordFieldsusd_Dict)) (box ((Map.add "showRecordFields" (box ((fun (v: obj) -> (fun (v1: obj) -> (box ""))))) Map.empty))))

let Data_Show_showRecordFields  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "showRecordFields" (unbox<Map<string, obj>> (v)))))))

let Data_Show_showRecord  = (fun (_: obj) -> (fun (_: obj) -> (fun (dictShowRecordFields: obj) -> (let showRecordFields1 = (sharpurs_apply (box (Data_Show_showRecordFields)) (box (dictShowRecordFields))) in (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (record: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Show_append)) (box ((box "{")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Show_append)) (box ((sharpurs_apply (box ((sharpurs_apply (box (showRecordFields1)) (box ((box Type_Proxy_Proxyusd_Ctor)))))) (box (record)))))))) (box ((box "}")))))))))) Map.empty))))))))

let Data_Show_showProxy  = (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (box "Proxy")))) Map.empty))))

let Data_Show_showNumber  = (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box (Data_Show_showNumberImpl)) Map.empty))))

let Data_Show_showInt  = (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box (Data_Show_showIntImpl)) Map.empty))))

let Data_Show_showChar  = (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box (Data_Show_showCharImpl)) Map.empty))))

let Data_Show_showBoolean  = (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | LitBool true () -> (box ((box "true"))) | LitBool false () -> (box ((box "false"))))))) Map.empty))))

let Data_Show_show  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "show" (unbox<Map<string, obj>> (v)))))))

let Data_Show_showArray  = (fun (dictShow: obj) -> (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((sharpurs_apply (box (Data_Show_showArrayImpl)) (box ((sharpurs_apply (box (Data_Show_show)) (box (dictShow)))))))) Map.empty)))))

let Data_Show_showRecordFieldsCons  = (fun (dictIsSymbol: obj) -> (let reflectSymbol = (sharpurs_apply (box (Data_Symbol_reflectSymbol)) (box (dictIsSymbol))) in (fun (dictShowRecordFields: obj) -> (let showRecordFields1 = (sharpurs_apply (box (Data_Show_showRecordFields)) (box (dictShowRecordFields))) in (fun (dictShow: obj) -> (let show1 = (sharpurs_apply (box (Data_Show_show)) (box (dictShow))) in (sharpurs_apply (box (Data_Show_ShowRecordFieldsusd_Dict)) (box ((Map.add "showRecordFields" (box ((fun (v: obj) -> (fun (record: obj) -> (let tail = (sharpurs_apply (box ((sharpurs_apply (box (showRecordFields1)) (box ((box Type_Proxy_Proxyusd_Ctor)))))) (box (record))) in let key = (sharpurs_apply (box (reflectSymbol)) (box ((box Type_Proxy_Proxyusd_Ctor)))) in let focus = (sharpurs_apply (box ((sharpurs_apply (box (Record_Unsafe_unsafeGet)) (box (key))))) (box (record))) in (sharpurs_apply (box ((sharpurs_apply (box (Data_Show_append)) (box ((box " ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Show_append)) (box (key))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Show_append)) (box ((box ": ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Show_append)) (box ((sharpurs_apply (box (show1)) (box (focus)))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Show_append)) (box ((box ",")))))) (box (tail)))))))))))))))))))) Map.empty))))))))))

let Data_Show_showRecordFieldsConsNil  = (fun (dictIsSymbol: obj) -> (let reflectSymbol = (sharpurs_apply (box (Data_Symbol_reflectSymbol)) (box (dictIsSymbol))) in (fun (dictShow: obj) -> (let show1 = (sharpurs_apply (box (Data_Show_show)) (box (dictShow))) in (sharpurs_apply (box (Data_Show_ShowRecordFieldsusd_Dict)) (box ((Map.add "showRecordFields" (box ((fun (v: obj) -> (fun (record: obj) -> (let key = (sharpurs_apply (box (reflectSymbol)) (box ((box Type_Proxy_Proxyusd_Ctor)))) in let focus = (sharpurs_apply (box ((sharpurs_apply (box (Record_Unsafe_unsafeGet)) (box (key))))) (box (record))) in (sharpurs_apply (box ((sharpurs_apply (box (Data_Show_append)) (box ((box " ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Show_append)) (box (key))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Show_append)) (box ((box ": ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Show_append)) (box ((sharpurs_apply (box (show1)) (box (focus)))))))) (box ((box " ")))))))))))))))))) Map.empty))))))))
