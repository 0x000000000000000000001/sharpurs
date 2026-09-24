[<AutoOpen>]
module PureScript_Data_Functor

open System
open System.Collections.Generic

module Data_Functor_FFI =
    let arrayMap = box (fun (f: obj) -> box (fun (arr: obj) ->
        let a = unbox<obj[]> arr
        let l = a.Length
        let result = Array.zeroCreate<obj> l
        for i = 0 to l - 1 do
            result.[i] <- sharpurs_apply f a.[i]
        box result
    ))
    

let Data_Functor_arrayMap = box Data_Functor_FFI.``arrayMap``


let Data_Functor_Functorusd_Dict  = (fun (x: obj) -> x)

let Data_Functor_map  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "map" (unbox<Map<string, obj>> (v)))))))

let Data_Functor_mapFlipped  = (fun (dictFunctor: obj) -> (let map1 = (sharpurs_apply (box (Data_Functor_map)) (box (dictFunctor))) in (fun (fa: obj) -> (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (map1)) (box (f))))) (box (fa)))))))

let Data_Functor_void  = (fun (dictFunctor: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Functor_map)) (box (dictFunctor))))) (box ((sharpurs_apply (box (Data_Function_const)) (box (Data_Unit_unit)))))))

let Data_Functor_voidLeft  = (fun (dictFunctor: obj) -> (let map1 = (sharpurs_apply (box (Data_Functor_map)) (box (dictFunctor))) in (fun (f: obj) -> (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (map1)) (box ((sharpurs_apply (box (Data_Function_const)) (box (x)))))))) (box (f)))))))

let Data_Functor_voidRight  = (fun (dictFunctor: obj) -> (let map1 = (sharpurs_apply (box (Data_Functor_map)) (box (dictFunctor))) in (fun (x: obj) -> (sharpurs_apply (box (map1)) (box ((sharpurs_apply (box (Data_Function_const)) (box (x)))))))))

let Data_Functor_functorProxy  = (sharpurs_apply (box (Data_Functor_Functorusd_Dict)) (box ((Map.add "map" (box ((fun (v: obj) -> (fun (v1: obj) -> (box Type_Proxy_Proxyusd_Ctor))))) Map.empty))))

let Data_Functor_functorFn  = (sharpurs_apply (box (Data_Functor_Functorusd_Dict)) (box ((Map.add "map" (box ((sharpurs_apply (box (Control_Semigroupoid_compose)) (box (Control_Semigroupoid_semigroupoidFn))))) Map.empty))))

let Data_Functor_functorArray  = (sharpurs_apply (box (Data_Functor_Functorusd_Dict)) (box ((Map.add "map" (box (Data_Functor_arrayMap)) Map.empty))))

let Data_Functor_flap  = (fun (dictFunctor: obj) -> (let map1 = (sharpurs_apply (box (Data_Functor_map)) (box (dictFunctor))) in (fun (ff: obj) -> (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (map1)) (box ((fun (f: obj) -> (sharpurs_apply (box (f)) (box (x))))))))) (box (ff)))))))
