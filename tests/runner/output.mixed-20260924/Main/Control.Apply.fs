[<AutoOpen>]
module PureScript_Control_Apply

open System
open System.Collections.Generic

module Control_Apply_FFI =
    let arrayApply = undefined
    

let Control_Apply_arrayApply = box Control_Apply_FFI.``arrayApply``


let Control_Apply_identity  = (sharpurs_apply (box (Control_Category_identity)) (box (Control_Category_categoryFn)))

let Control_Apply_Applyusd_Dict  = (fun (x: obj) -> x)

let Control_Apply_applyProxy  = (sharpurs_apply (box (Control_Apply_Applyusd_Dict)) (box ((Map.add "apply" (box ((fun (v: obj) -> (fun (v1: obj) -> (box Type_Proxy_Proxyusd_Ctor))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Functor_functorProxy))) Map.empty)))))

let Control_Apply_applyFn  = (sharpurs_apply (box (Control_Apply_Applyusd_Dict)) (box ((Map.add "apply" (box ((fun (f: obj) -> (fun (g: obj) -> (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (f)) (box (x))))) (box ((sharpurs_apply (box (g)) (box (x))))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Functor_functorFn))) Map.empty)))))

let Control_Apply_applyArray  = (sharpurs_apply (box (Control_Apply_Applyusd_Dict)) (box ((Map.add "apply" (box (Control_Apply_arrayApply)) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Functor_functorArray))) Map.empty)))))

let Control_Apply_apply  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "apply" (unbox<Map<string, obj>> (v)))))))

let Control_Apply_applyFirst  = (fun (dictApply: obj) -> (let apply1 = (sharpurs_apply (box (Control_Apply_apply)) (box (dictApply))) in let map = (sharpurs_apply (box (Data_Functor_map)) (box ((sharpurs_apply (box ((Map.find "Functor0" (unbox<Map<string, obj>> (dictApply))))) (box (Prim_undefined)))))) in (fun (a: obj) -> (fun (b: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (apply1)) (box ((sharpurs_apply (box ((sharpurs_apply (box (map)) (box (Data_Function_const))))) (box (a)))))))) (box (b)))))))

let Control_Apply_applySecond  = (fun (dictApply: obj) -> (let apply1 = (sharpurs_apply (box (Control_Apply_apply)) (box (dictApply))) in let map = (sharpurs_apply (box (Data_Functor_map)) (box ((sharpurs_apply (box ((Map.find "Functor0" (unbox<Map<string, obj>> (dictApply))))) (box (Prim_undefined)))))) in (fun (a: obj) -> (fun (b: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (apply1)) (box ((sharpurs_apply (box ((sharpurs_apply (box (map)) (box ((sharpurs_apply (box (Data_Function_const)) (box (Control_Apply_identity)))))))) (box (a)))))))) (box (b)))))))

let Control_Apply_lift2  = (fun (dictApply: obj) -> (let apply1 = (sharpurs_apply (box (Control_Apply_apply)) (box (dictApply))) in let map = (sharpurs_apply (box (Data_Functor_map)) (box ((sharpurs_apply (box ((Map.find "Functor0" (unbox<Map<string, obj>> (dictApply))))) (box (Prim_undefined)))))) in (fun (f: obj) -> (fun (a: obj) -> (fun (b: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (apply1)) (box ((sharpurs_apply (box ((sharpurs_apply (box (map)) (box (f))))) (box (a)))))))) (box (b))))))))

let Control_Apply_lift3  = (fun (dictApply: obj) -> (let apply1 = (sharpurs_apply (box (Control_Apply_apply)) (box (dictApply))) in let map = (sharpurs_apply (box (Data_Functor_map)) (box ((sharpurs_apply (box ((Map.find "Functor0" (unbox<Map<string, obj>> (dictApply))))) (box (Prim_undefined)))))) in (fun (f: obj) -> (fun (a: obj) -> (fun (b: obj) -> (fun (c: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (apply1)) (box ((sharpurs_apply (box ((sharpurs_apply (box (apply1)) (box ((sharpurs_apply (box ((sharpurs_apply (box (map)) (box (f))))) (box (a)))))))) (box (b)))))))) (box (c)))))))))

let Control_Apply_lift4  = (fun (dictApply: obj) -> (let apply1 = (sharpurs_apply (box (Control_Apply_apply)) (box (dictApply))) in let map = (sharpurs_apply (box (Data_Functor_map)) (box ((sharpurs_apply (box ((Map.find "Functor0" (unbox<Map<string, obj>> (dictApply))))) (box (Prim_undefined)))))) in (fun (f: obj) -> (fun (a: obj) -> (fun (b: obj) -> (fun (c: obj) -> (fun (d: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (apply1)) (box ((sharpurs_apply (box ((sharpurs_apply (box (apply1)) (box ((sharpurs_apply (box ((sharpurs_apply (box (apply1)) (box ((sharpurs_apply (box ((sharpurs_apply (box (map)) (box (f))))) (box (a)))))))) (box (b)))))))) (box (c)))))))) (box (d))))))))))

let Control_Apply_lift5  = (fun (dictApply: obj) -> (let apply1 = (sharpurs_apply (box (Control_Apply_apply)) (box (dictApply))) in let map = (sharpurs_apply (box (Data_Functor_map)) (box ((sharpurs_apply (box ((Map.find "Functor0" (unbox<Map<string, obj>> (dictApply))))) (box (Prim_undefined)))))) in (fun (f: obj) -> (fun (a: obj) -> (fun (b: obj) -> (fun (c: obj) -> (fun (d: obj) -> (fun (e: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (apply1)) (box ((sharpurs_apply (box ((sharpurs_apply (box (apply1)) (box ((sharpurs_apply (box ((sharpurs_apply (box (apply1)) (box ((sharpurs_apply (box ((sharpurs_apply (box (apply1)) (box ((sharpurs_apply (box ((sharpurs_apply (box (map)) (box (f))))) (box (a)))))))) (box (b)))))))) (box (c)))))))) (box (d)))))))) (box (e)))))))))))
