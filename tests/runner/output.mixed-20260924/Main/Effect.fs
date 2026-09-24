[<AutoOpen>]
module PureScript_Effect

open System
open System.Collections.Generic

module Effect_FFI =
    let pureE = box (fun (a: obj) -> box (fun _ -> a))
    let bindE = box (fun (a: obj) -> box (fun (f: obj) -> box (fun _ -> let a_res = sharpurs_apply a undefined in let f_res = sharpurs_apply f a_res in sharpurs_apply f_res undefined)))
    
    let untilE = box (fun (f: obj) -> box (fun _ ->
        let mutable condition = false
        while not condition do
            condition <- unbox<bool> (sharpurs_apply f undefined)
        undefined
    ))
    
    let whileE = box (fun (f: obj) -> box (fun (a: obj) -> box (fun _ ->
        let mutable condition = unbox<bool> (sharpurs_apply f undefined)
        while condition do
            sharpurs_apply a undefined |> ignore
            condition <- unbox<bool> (sharpurs_apply f undefined)
        undefined
    )))
    
    let forE = box (fun (lo: obj) -> box (fun (hi: obj) -> box (fun (f: obj) -> box (fun _ ->
        let l = unbox<int> lo
        let h = unbox<int> hi
        for i = l to h - 1 do
            sharpurs_apply (sharpurs_apply f (box i)) undefined |> ignore
        undefined
    ))))
    
    let foreachE = box (fun (arr: obj) -> box (fun (f: obj) -> box (fun _ ->
        let arr' = unbox<obj[]> arr
        for v in arr' do
            sharpurs_apply (sharpurs_apply f v) undefined |> ignore
        undefined
    )))
    

let Effect_pureE = box Effect_FFI.``pureE``
let Effect_bindE = box Effect_FFI.``bindE``
let Effect_untilE = box Effect_FFI.``untilE``
let Effect_whileE = box Effect_FFI.``whileE``
let Effect_forE = box Effect_FFI.``forE``
let Effect_foreachE = box Effect_FFI.``foreachE``


let rec Effect_monadEffect  = (sharpurs_apply (box (Control_Monad_Monadusd_Dict)) (box ((Map.add "Applicative0" (box ((fun (_: obj) -> Effect_applicativeEffect))) (Map.add "Bind1" (box ((fun (_: obj) -> Effect_bindEffect))) Map.empty)))))
and Effect_bindEffect  = (sharpurs_apply (box (Control_Bind_Bindusd_Dict)) (box ((Map.add "bind" (box (Effect_bindE)) (Map.add "Apply0" (box ((fun (_: obj) -> Effect_applyEffect))) Map.empty)))))
and Effect_applyEffect  = (sharpurs_apply (box (Control_Apply_Applyusd_Dict)) (box ((Map.add "apply" (box ((sharpurs_apply (box (Control_Monad_ap)) (box (Effect_monadEffect))))) (Map.add "Functor0" (box ((fun (_: obj) -> Effect_functorEffect))) Map.empty)))))
and Effect_applicativeEffect  = (sharpurs_apply (box (Control_Applicative_Applicativeusd_Dict)) (box ((Map.add "pure" (box (Effect_pureE)) (Map.add "Apply0" (box ((fun (_: obj) -> Effect_applyEffect))) Map.empty)))))
and Effect_functorEffect  = (sharpurs_apply (box (Data_Functor_Functorusd_Dict)) (box ((Map.add "map" (box ((sharpurs_apply (box (Control_Applicative_liftA1)) (box (Effect_applicativeEffect))))) Map.empty))))

let Effect_lift2  = (sharpurs_apply (box (Control_Apply_lift2)) (box (Effect_applyEffect)))

let Effect_semigroupEffect  = (fun (dictSemigroup: obj) -> (sharpurs_apply (box (Data_Semigroup_Semigroupusd_Dict)) (box ((Map.add "append" (box ((sharpurs_apply (box (Effect_lift2)) (box ((sharpurs_apply (box (Data_Semigroup_append)) (box (dictSemigroup)))))))) Map.empty)))))

let Effect_monoidEffect  = (fun (dictMonoid: obj) -> (let semigroupEffect1 = (sharpurs_apply (box (Effect_semigroupEffect)) (box ((sharpurs_apply (box ((Map.find "Semigroup0" (unbox<Map<string, obj>> (dictMonoid))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Monoid_Monoidusd_Dict)) (box ((Map.add "mempty" (box ((sharpurs_apply (box (Effect_pureE)) (box ((sharpurs_apply (box (Data_Monoid_mempty)) (box (dictMonoid)))))))) (Map.add "Semigroup0" (box ((fun (_: obj) -> semigroupEffect1))) Map.empty)))))))
