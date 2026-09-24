[<AutoOpen>]
module PureScript_Type_Data_Symbol

open System
open System.Collections.Generic

let Type_Data_Symbol_Equalsusd_Dict  = (fun (x: obj) -> x)

let Type_Data_Symbol_uncons  = (fun (_: obj) -> (fun (v: obj) -> (Map.add "head" (box ((box Type_Proxy_Proxyusd_Ctor))) (Map.add "tail" (box ((box Type_Proxy_Proxyusd_Ctor))) Map.empty))))

let Type_Data_Symbol_equalsSymbol  = (fun (_: obj) -> (fun (_: obj) -> (sharpurs_apply (box (Type_Data_Symbol_Equalsusd_Dict)) (box (Map.empty)))))

let Type_Data_Symbol_equals  = (fun (_: obj) -> (fun (v: obj) -> (fun (v1: obj) -> (box Type_Proxy_Proxyusd_Ctor))))

let Type_Data_Symbol_compare  = (fun (_: obj) -> (fun (v: obj) -> (fun (v1: obj) -> (box Type_Proxy_Proxyusd_Ctor))))

let Type_Data_Symbol_append  = (fun (_: obj) -> (fun (v: obj) -> (fun (v1: obj) -> (box Type_Proxy_Proxyusd_Ctor))))
