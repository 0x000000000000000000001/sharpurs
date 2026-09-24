[<AutoOpen>]
module PureScript_Control_Lazy

open System
open System.Collections.Generic

let Control_Lazy_Lazyusd_Dict  = (fun (x: obj) -> x)

let Control_Lazy_lazyUnit  = (sharpurs_apply (box (Control_Lazy_Lazyusd_Dict)) (box ((Map.add "defer" (box ((fun (v: obj) -> Data_Unit_unit))) Map.empty))))

let Control_Lazy_lazyFn  = (sharpurs_apply (box (Control_Lazy_Lazyusd_Dict)) (box ((Map.add "defer" (box ((fun (f: obj) -> (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (f)) (box (Data_Unit_unit))))) (box (x))))))) Map.empty))))

let Control_Lazy_defer  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "defer" (unbox<Map<string, obj>> (v)))))))

let Control_Lazy_fix  = (fun (dictLazy: obj) -> (let defer1 = (sharpurs_apply (box (Control_Lazy_defer)) (box (dictLazy))) in (fun (f: obj) -> (let mutable go : obj = null in go <- (sharpurs_apply (box (defer1)) (box ((fun (v: obj) -> (sharpurs_apply (box (f)) (box (go))))))); go))))
