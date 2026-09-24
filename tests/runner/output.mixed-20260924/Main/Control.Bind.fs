[<AutoOpen>]
module PureScript_Control_Bind

open System
open System.Collections.Generic

module Control_Bind_FFI =
    let arrayBind = undefined
    

let Control_Bind_arrayBind = box Control_Bind_FFI.``arrayBind``


let Control_Bind_identity  = (sharpurs_apply (box (Control_Category_identity)) (box (Control_Category_categoryFn)))

let Control_Bind_Bindusd_Dict  = (fun (x: obj) -> x)

let Control_Bind_Discardusd_Dict  = (fun (x: obj) -> x)

let Control_Bind_discard  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "discard" (unbox<Map<string, obj>> (v)))))))

let Control_Bind_bindProxy  = (sharpurs_apply (box (Control_Bind_Bindusd_Dict)) (box ((Map.add "bind" (box ((fun (v: obj) -> (fun (v1: obj) -> (box Type_Proxy_Proxyusd_Ctor))))) (Map.add "Apply0" (box ((fun (_: obj) -> Control_Apply_applyProxy))) Map.empty)))))

let Control_Bind_bindFn  = (sharpurs_apply (box (Control_Bind_Bindusd_Dict)) (box ((Map.add "bind" (box ((fun (m: obj) -> (fun (f: obj) -> (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (f)) (box ((sharpurs_apply (box (m)) (box (x)))))))) (box (x)))))))) (Map.add "Apply0" (box ((fun (_: obj) -> Control_Apply_applyFn))) Map.empty)))))

let Control_Bind_bindArray  = (sharpurs_apply (box (Control_Bind_Bindusd_Dict)) (box ((Map.add "bind" (box (Control_Bind_arrayBind)) (Map.add "Apply0" (box ((fun (_: obj) -> Control_Apply_applyArray))) Map.empty)))))

let Control_Bind_bind  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "bind" (unbox<Map<string, obj>> (v)))))))

let Control_Bind_bindFlipped  = (fun (dictBind: obj) -> (sharpurs_apply (box (Data_Function_flip)) (box ((sharpurs_apply (box (Control_Bind_bind)) (box (dictBind)))))))

let Control_Bind_composeKleisliFlipped  = (fun (dictBind: obj) -> (let bindFlipped1 = (sharpurs_apply (box (Control_Bind_bindFlipped)) (box (dictBind))) in (fun (f: obj) -> (fun (g: obj) -> (fun (a: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (bindFlipped1)) (box (f))))) (box ((sharpurs_apply (box (g)) (box (a)))))))))))

let Control_Bind_composeKleisli  = (fun (dictBind: obj) -> (let bind1 = (sharpurs_apply (box (Control_Bind_bind)) (box (dictBind))) in (fun (f: obj) -> (fun (g: obj) -> (fun (a: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (bind1)) (box ((sharpurs_apply (box (f)) (box (a)))))))) (box (g))))))))

let Control_Bind_discardProxy  = (sharpurs_apply (box (Control_Bind_Discardusd_Dict)) (box ((Map.add "discard" (box ((fun (dictBind: obj) -> (sharpurs_apply (box (Control_Bind_bind)) (box (dictBind)))))) Map.empty))))

let Control_Bind_discardUnit  = (sharpurs_apply (box (Control_Bind_Discardusd_Dict)) (box ((Map.add "discard" (box ((fun (dictBind: obj) -> (sharpurs_apply (box (Control_Bind_bind)) (box (dictBind)))))) Map.empty))))

let Control_Bind_ifM  = (fun (dictBind: obj) -> (let bind1 = (sharpurs_apply (box (Control_Bind_bind)) (box (dictBind))) in (fun (cond: obj) -> (fun (t: obj) -> (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (bind1)) (box (cond))))) (box ((fun (cond_prime: obj) -> (match ((unbox (cond_prime))) with | LitBool true () -> (box (t)) | _ -> (box (f))))))))))))

let Control_Bind_join  = (fun (dictBind: obj) -> (let bind1 = (sharpurs_apply (box (Control_Bind_bind)) (box (dictBind))) in (fun (m: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (bind1)) (box (m))))) (box (Control_Bind_identity))))))
