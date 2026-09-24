[<AutoOpen>]
module PureScript_Data_Generic_Rep

open System
open System.Collections.Generic

type Data_Generic_Rep_Sum =
  | Data_Generic_Rep_Inlusd_Ctor of obj
  | Data_Generic_Rep_Inrusd_Ctor of obj

type Data_Generic_Rep_Product =
  | Data_Generic_Rep_Productusd_Ctor of obj * obj

type Data_Generic_Rep_NoArguments =
  | Data_Generic_Rep_NoArgumentsusd_Ctor

let Data_Generic_Rep_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Semigroup_semigroupString)))

let Data_Generic_Rep_show  = (sharpurs_apply (box (Data_Show_show)) (box (Data_Show_showString)))

let Data_Generic_Rep_Inl  = (fun (usd__arg1: obj) -> (box (Data_Generic_Rep_Inlusd_Ctor(usd__arg1))))

let Data_Generic_Rep_Inr  = (fun (usd__arg1: obj) -> (box (Data_Generic_Rep_Inrusd_Ctor(usd__arg1))))

let Data_Generic_Rep_Product  = (fun (usd__arg1: obj) -> (fun (usd__arg2: obj) -> (box (Data_Generic_Rep_Productusd_Ctor(usd__arg1, usd__arg2)))))

let Data_Generic_Rep_NoConstructors  = (fun (x: obj) -> x)

let Data_Generic_Rep_NoArguments  = (box Data_Generic_Rep_NoArgumentsusd_Ctor)

let Data_Generic_Rep_Genericusd_Dict  = (fun (x: obj) -> x)

let Data_Generic_Rep_Constructor  = (fun (x: obj) -> x)

let Data_Generic_Rep_Argument  = (fun (x: obj) -> x)

let Data_Generic_Rep_to  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "to" (unbox<Map<string, obj>> (v)))))))

let Data_Generic_Rep_showSum  = (fun (dictShow: obj) -> (let show1 = (sharpurs_apply (box (Data_Show_show)) (box (dictShow))) in (fun (dictShow1: obj) -> (let show2 = (sharpurs_apply (box (Data_Show_show)) (box (dictShow1))) in (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | Data_Generic_Rep_Inlusd_Ctor(a) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Generic_Rep_append)) (box ((box "(Inl ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Generic_Rep_append)) (box ((sharpurs_apply (box (show1)) (box (a)))))))) (box ((box ")"))))))))) | Data_Generic_Rep_Inrusd_Ctor(b) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Generic_Rep_append)) (box ((box "(Inr ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Generic_Rep_append)) (box ((sharpurs_apply (box (show2)) (box (b)))))))) (box ((box ")"))))))))))))) Map.empty))))))))

let Data_Generic_Rep_showProduct  = (fun (dictShow: obj) -> (let show1 = (sharpurs_apply (box (Data_Show_show)) (box (dictShow))) in (fun (dictShow1: obj) -> (let show2 = (sharpurs_apply (box (Data_Show_show)) (box (dictShow1))) in (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | Data_Generic_Rep_Productusd_Ctor(a, b) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Generic_Rep_append)) (box ((box "(Product ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Generic_Rep_append)) (box ((sharpurs_apply (box (show1)) (box (a)))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Generic_Rep_append)) (box ((box " ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Generic_Rep_append)) (box ((sharpurs_apply (box (show2)) (box (b)))))))) (box ((box ")"))))))))))))))))))) Map.empty))))))))

let Data_Generic_Rep_showNoArguments  = (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (box "NoArguments")))) Map.empty))))

let Data_Generic_Rep_showConstructor  = (fun (dictIsSymbol: obj) -> (let reflectSymbol = (sharpurs_apply (box (Data_Symbol_reflectSymbol)) (box (dictIsSymbol))) in (fun (dictShow: obj) -> (let show1 = (sharpurs_apply (box (Data_Show_show)) (box (dictShow))) in (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | a -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Generic_Rep_append)) (box ((box "(Constructor @")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Generic_Rep_append)) (box ((sharpurs_apply (box (Data_Generic_Rep_show)) (box ((sharpurs_apply (box (reflectSymbol)) (box ((box Type_Proxy_Proxyusd_Ctor)))))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Generic_Rep_append)) (box ((box " ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Generic_Rep_append)) (box ((sharpurs_apply (box (show1)) (box (a)))))))) (box ((box ")"))))))))))))))))))) Map.empty))))))))

let Data_Generic_Rep_showArgument  = (fun (dictShow: obj) -> (let show1 = (sharpurs_apply (box (Data_Show_show)) (box (dictShow))) in (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | a -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Generic_Rep_append)) (box ((box "(Argument ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Generic_Rep_append)) (box ((sharpurs_apply (box (show1)) (box (a)))))))) (box ((box ")"))))))))))))) Map.empty))))))

let Data_Generic_Rep_repOf  = (fun (dictGeneric: obj) -> (fun (v: obj) -> (box Type_Proxy_Proxyusd_Ctor)))

let Data_Generic_Rep_from  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "from" (unbox<Map<string, obj>> (v)))))))
