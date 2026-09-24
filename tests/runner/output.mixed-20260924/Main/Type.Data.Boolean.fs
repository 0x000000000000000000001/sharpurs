[<AutoOpen>]
module PureScript_Type_Data_Boolean

open System
open System.Collections.Generic

let Type_Data_Boolean_Orusd_Dict  = (fun (x: obj) -> x)

let Type_Data_Boolean_Notusd_Dict  = (fun (x: obj) -> x)

let Type_Data_Boolean_IsBooleanusd_Dict  = (fun (x: obj) -> x)

let Type_Data_Boolean_Ifusd_Dict  = (fun (x: obj) -> x)

let Type_Data_Boolean_Andusd_Dict  = (fun (x: obj) -> x)

let Type_Data_Boolean_reflectBoolean  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "reflectBoolean" (unbox<Map<string, obj>> (v)))))))

let Type_Data_Boolean_orTrue  = (sharpurs_apply (box (Type_Data_Boolean_Orusd_Dict)) (box (Map.empty)))

let Type_Data_Boolean_orFalse  = (sharpurs_apply (box (Type_Data_Boolean_Orusd_Dict)) (box (Map.empty)))

let Type_Data_Boolean_or  = (fun (_: obj) -> (fun (v: obj) -> (fun (v1: obj) -> (box Type_Proxy_Proxyusd_Ctor))))

let Type_Data_Boolean_notTrue  = (sharpurs_apply (box (Type_Data_Boolean_Notusd_Dict)) (box (Map.empty)))

let Type_Data_Boolean_notFalse  = (sharpurs_apply (box (Type_Data_Boolean_Notusd_Dict)) (box (Map.empty)))

let Type_Data_Boolean_not  = (fun (_: obj) -> (fun (v: obj) -> (box Type_Proxy_Proxyusd_Ctor)))

let Type_Data_Boolean_isBooleanTrue  = (sharpurs_apply (box (Type_Data_Boolean_IsBooleanusd_Dict)) (box ((Map.add "reflectBoolean" (box ((fun (v: obj) -> (box true)))) Map.empty))))

let Type_Data_Boolean_isBooleanFalse  = (sharpurs_apply (box (Type_Data_Boolean_IsBooleanusd_Dict)) (box ((Map.add "reflectBoolean" (box ((fun (v: obj) -> (box false)))) Map.empty))))

let Type_Data_Boolean_reifyBoolean  = (fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (LitBool true (), f) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (f)) (box (Type_Data_Boolean_isBooleanTrue))))) (box ((box Type_Proxy_Proxyusd_Ctor)))))) | (LitBool false (), f) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (f)) (box (Type_Data_Boolean_isBooleanFalse))))) (box ((box Type_Proxy_Proxyusd_Ctor)))))))))

let Type_Data_Boolean_if_  = (fun (_: obj) -> (fun (v: obj) -> (fun (v1: obj) -> (fun (v2: obj) -> (box Type_Proxy_Proxyusd_Ctor)))))

let Type_Data_Boolean_ifTrue  = (sharpurs_apply (box (Type_Data_Boolean_Ifusd_Dict)) (box (Map.empty)))

let Type_Data_Boolean_ifFalse  = (sharpurs_apply (box (Type_Data_Boolean_Ifusd_Dict)) (box (Map.empty)))

let Type_Data_Boolean_andTrue  = (sharpurs_apply (box (Type_Data_Boolean_Andusd_Dict)) (box (Map.empty)))

let Type_Data_Boolean_andFalse  = (sharpurs_apply (box (Type_Data_Boolean_Andusd_Dict)) (box (Map.empty)))

let Type_Data_Boolean_and  = (fun (_: obj) -> (fun (v: obj) -> (fun (v1: obj) -> (box Type_Proxy_Proxyusd_Ctor))))
