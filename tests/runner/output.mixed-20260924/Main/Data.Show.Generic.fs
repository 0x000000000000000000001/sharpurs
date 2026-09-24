[<AutoOpen>]
module PureScript_Data_Show_Generic

open System
open System.Collections.Generic

module Data_Show_Generic_FFI =
    let intercalate = undefined
    

let Data_Show_Generic_intercalate = box Data_Show_Generic_FFI.``intercalate``


let Data_Show_Generic_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Semigroup_semigroupArray)))

let Data_Show_Generic_append1  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Semigroup_semigroupString)))

let Data_Show_Generic_GenericShowArgsusd_Dict  = (fun (x: obj) -> x)

let Data_Show_Generic_GenericShowusd_Dict  = (fun (x: obj) -> x)

let Data_Show_Generic_genericShowArgsNoArguments  = (sharpurs_apply (box (Data_Show_Generic_GenericShowArgsusd_Dict)) (box ((Map.add "genericShowArgs" (box ((fun (v: obj) -> (box [||])))) Map.empty))))

let Data_Show_Generic_genericShowArgsArgument  = (fun (dictShow: obj) -> (let show = (sharpurs_apply (box (Data_Show_show)) (box (dictShow))) in (sharpurs_apply (box (Data_Show_Generic_GenericShowArgsusd_Dict)) (box ((Map.add "genericShowArgs" (box ((fun (v: obj) -> (match ((unbox (v))) with | a -> (box ((box [|(sharpurs_apply (box (show)) (box (a)))|]))))))) Map.empty))))))

let Data_Show_Generic_genericShowArgs  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "genericShowArgs" (unbox<Map<string, obj>> (v)))))))

let Data_Show_Generic_genericShowArgsProduct  = (fun (dictGenericShowArgs: obj) -> (let genericShowArgs1 = (sharpurs_apply (box (Data_Show_Generic_genericShowArgs)) (box (dictGenericShowArgs))) in (fun (dictGenericShowArgs1: obj) -> (let genericShowArgs2 = (sharpurs_apply (box (Data_Show_Generic_genericShowArgs)) (box (dictGenericShowArgs1))) in (sharpurs_apply (box (Data_Show_Generic_GenericShowArgsusd_Dict)) (box ((Map.add "genericShowArgs" (box ((fun (v: obj) -> (match ((unbox (v))) with | Data_Generic_Rep_Productusd_Ctor(a, b) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Show_Generic_append)) (box ((sharpurs_apply (box (genericShowArgs1)) (box (a)))))))) (box ((sharpurs_apply (box (genericShowArgs2)) (box (b)))))))))))) Map.empty))))))))

let Data_Show_Generic_genericShowConstructor  = (fun (dictGenericShowArgs: obj) -> (let genericShowArgs1 = (sharpurs_apply (box (Data_Show_Generic_genericShowArgs)) (box (dictGenericShowArgs))) in (fun (dictIsSymbol: obj) -> (let reflectSymbol = (sharpurs_apply (box (Data_Symbol_reflectSymbol)) (box (dictIsSymbol))) in (sharpurs_apply (box (Data_Show_Generic_GenericShowusd_Dict)) (box ((Map.add "genericShow'" (box ((fun (v: obj) -> (match ((unbox (v))) with | a -> (box ((let ctor = (sharpurs_apply (box (reflectSymbol)) (box ((box Type_Proxy_Proxyusd_Ctor)))) in (let v1 = (sharpurs_apply (box (genericShowArgs1)) (box (a))) in (match ((unbox (v1))) with | [|  |] -> (box (ctor)) | args -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Show_Generic_append1)) (box ((box "(")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Show_Generic_append1)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Show_Generic_intercalate)) (box ((box " ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Show_Generic_append)) (box ((box [|ctor|])))))) (box (args))))))))))) (box ((box ")")))))))))))))))))) Map.empty))))))))

let Data_Show_Generic_genericShow_prime  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "genericShow'" (unbox<Map<string, obj>> (v)))))))

let rec Data_Show_Generic_genericShowNoConstructors  = (sharpurs_apply (box (Data_Show_Generic_GenericShowusd_Dict)) (box ((Map.add "genericShow'" (box ((fun (a: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Show_Generic_genericShow_prime)) (box (Data_Show_Generic_genericShowNoConstructors))))) (box (a)))))) Map.empty))))

let Data_Show_Generic_genericShowSum  = (fun (dictGenericShow: obj) -> (let genericShow_prime1 = (sharpurs_apply (box (Data_Show_Generic_genericShow_prime)) (box (dictGenericShow))) in (fun (dictGenericShow1: obj) -> (let genericShow_prime2 = (sharpurs_apply (box (Data_Show_Generic_genericShow_prime)) (box (dictGenericShow1))) in (sharpurs_apply (box (Data_Show_Generic_GenericShowusd_Dict)) (box ((Map.add "genericShow'" (box ((fun (v: obj) -> (match ((unbox (v))) with | Data_Generic_Rep_Inlusd_Ctor(a) -> (box ((sharpurs_apply (box (genericShow_prime1)) (box (a))))) | Data_Generic_Rep_Inrusd_Ctor(b) -> (box ((sharpurs_apply (box (genericShow_prime2)) (box (b))))))))) Map.empty))))))))

let Data_Show_Generic_genericShow  = (fun (dictGeneric: obj) -> (let from = (sharpurs_apply (box (Data_Generic_Rep_from)) (box (dictGeneric))) in (fun (dictGenericShow: obj) -> (let genericShow_prime1 = (sharpurs_apply (box (Data_Show_Generic_genericShow_prime)) (box (dictGenericShow))) in (fun (x: obj) -> (sharpurs_apply (box (genericShow_prime1)) (box ((sharpurs_apply (box (from)) (box (x)))))))))))
