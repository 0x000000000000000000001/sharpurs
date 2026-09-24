[<AutoOpen>]
module PureScript_Control_Monad_ST_Internal

open System
open System.Collections.Generic

module Control_Monad_ST_Internal_FFI =
    let map_ = box (fun (f: obj) -> box (fun (a: obj) -> box (fun (usd___unused: obj) ->
        let res = sharpurs_apply a (box ())
        sharpurs_apply f res
    )))
    
    let bind_ = box (fun (a: obj) -> box (fun (f: obj) -> box (fun (usd___unused: obj) ->
        let res = sharpurs_apply a (box ())
        let fRes = sharpurs_apply f res
        sharpurs_apply fRes (box ())
    )))
    
    let run = box (fun (f: obj) -> sharpurs_apply f (box ()))
    
    let ``new`` = box (fun (val_: obj) -> box (fun (usd___unused: obj) -> box (ref val_)))
    
    let read = box (fun (r: obj) -> box (fun (usd___unused: obj) -> (unbox<obj ref> r).Value))
    
    let modifyImpl = box (fun (f: obj) -> box (fun (r: obj) -> box (fun (usd___unused: obj) ->
        let rRef = unbox<obj ref> r
        let t = sharpurs_apply f rRef.Value
        let state = Map.find "state" (unbox<Map<string, obj>> t)
        let value = Map.find "value" (unbox<Map<string, obj>> t)
        rRef.Value <- state
        value
    )))
    
    let write = box (fun (a: obj) -> box (fun (r: obj) -> box (fun (usd___unused: obj) ->
        let rRef = unbox<obj ref> r
        rRef.Value <- a
        a
    )))
    
    let pure_ = box (fun (a: obj) -> box (fun (usd___unused: obj) -> a))
    
    let ``while`` = box (fun (f: obj) -> box (fun (a: obj) -> box (fun (usd___unused: obj) ->
        let rec loop () =
            if unbox<bool> (sharpurs_apply f (box ())) then
                sharpurs_apply a (box ()) |> ignore
                loop ()
            else ()
        loop ()
        box ()
    )))
    
    let ``for`` = box (fun (lo: obj) -> box (fun (hi: obj) -> box (fun (f: obj) -> box (fun (usd___unused: obj) ->
        let low = unbox<int> lo
        let high = unbox<int> hi
        for i = low to high - 1 do
            sharpurs_apply f (box i) |> fun a -> sharpurs_apply a (box ()) |> ignore
        box ()
    ))))
    
    let foreach = box (fun (arr: obj) -> box (fun (f: obj) -> box (fun (usd___unused: obj) ->
        let a = unbox<obj[]> arr
        for i = 0 to a.Length - 1 do
            sharpurs_apply f a.[i] |> fun act -> sharpurs_apply act (box ()) |> ignore
        box ()
    )))
    

let Control_Monad_ST_Internal_map_ = box Control_Monad_ST_Internal_FFI.``map_``
let Control_Monad_ST_Internal_pure_ = box Control_Monad_ST_Internal_FFI.``pure_``
let Control_Monad_ST_Internal_bind_ = box Control_Monad_ST_Internal_FFI.``bind_``
let Control_Monad_ST_Internal_run = box Control_Monad_ST_Internal_FFI.``run``
let Control_Monad_ST_Internal_while = box Control_Monad_ST_Internal_FFI.``while``
let Control_Monad_ST_Internal_for = box Control_Monad_ST_Internal_FFI.``for``
let Control_Monad_ST_Internal_foreach = box Control_Monad_ST_Internal_FFI.``foreach``
let Control_Monad_ST_Internal_new = box Control_Monad_ST_Internal_FFI.``new``
let Control_Monad_ST_Internal_read = box Control_Monad_ST_Internal_FFI.``read``
let Control_Monad_ST_Internal_modifyImpl = box Control_Monad_ST_Internal_FFI.``modifyImpl``
let Control_Monad_ST_Internal_write = box Control_Monad_ST_Internal_FFI.``write``


let Control_Monad_ST_Internal_modify_prime  = Control_Monad_ST_Internal_modifyImpl

let Control_Monad_ST_Internal_modify  = (fun (f: obj) -> (sharpurs_apply (box (Control_Monad_ST_Internal_modify_prime)) (box ((fun (s: obj) -> (let s_prime = (sharpurs_apply (box (f)) (box (s))) in (Map.add "state" (box (s_prime)) (Map.add "value" (box (s_prime)) Map.empty))))))))

let Control_Monad_ST_Internal_functorST  = (sharpurs_apply (box (Data_Functor_Functorusd_Dict)) (box ((Map.add "map" (box (Control_Monad_ST_Internal_map_)) Map.empty))))

let Control_Monad_ST_Internal_map  = (sharpurs_apply (box (Data_Functor_map)) (box (Control_Monad_ST_Internal_functorST)))

let Control_Monad_ST_Internal_void  = (sharpurs_apply (box (Data_Functor_void)) (box (Control_Monad_ST_Internal_functorST)))

let rec Control_Monad_ST_Internal_monadST  = (sharpurs_apply (box (Control_Monad_Monadusd_Dict)) (box ((Map.add "Applicative0" (box ((fun (_: obj) -> Control_Monad_ST_Internal_applicativeST))) (Map.add "Bind1" (box ((fun (_: obj) -> Control_Monad_ST_Internal_bindST))) Map.empty)))))
and Control_Monad_ST_Internal_bindST  = (sharpurs_apply (box (Control_Bind_Bindusd_Dict)) (box ((Map.add "bind" (box (Control_Monad_ST_Internal_bind_)) (Map.add "Apply0" (box ((fun (_: obj) -> Control_Monad_ST_Internal_applyST))) Map.empty)))))
and Control_Monad_ST_Internal_applyST  = (sharpurs_apply (box (Control_Apply_Applyusd_Dict)) (box ((Map.add "apply" (box ((sharpurs_apply (box (Control_Monad_ap)) (box (Control_Monad_ST_Internal_monadST))))) (Map.add "Functor0" (box ((fun (_: obj) -> Control_Monad_ST_Internal_functorST))) Map.empty)))))
and Control_Monad_ST_Internal_applicativeST  = (sharpurs_apply (box (Control_Applicative_Applicativeusd_Dict)) (box ((Map.add "pure" (box (Control_Monad_ST_Internal_pure_)) (Map.add "Apply0" (box ((fun (_: obj) -> Control_Monad_ST_Internal_applyST))) Map.empty)))))

let Control_Monad_ST_Internal_lift2  = (sharpurs_apply (box (Control_Apply_lift2)) (box (Control_Monad_ST_Internal_applyST)))

let Control_Monad_ST_Internal_bind  = (sharpurs_apply (box (Control_Bind_bind)) (box (Control_Monad_ST_Internal_bindST)))

let Control_Monad_ST_Internal_bindFlipped  = (sharpurs_apply (box (Control_Bind_bindFlipped)) (box (Control_Monad_ST_Internal_bindST)))

let Control_Monad_ST_Internal_discard  = (sharpurs_apply (box ((sharpurs_apply (box (Control_Bind_discard)) (box (Control_Bind_discardUnit))))) (box (Control_Monad_ST_Internal_bindST)))

let Control_Monad_ST_Internal_pure  = (sharpurs_apply (box (Control_Applicative_pure)) (box (Control_Monad_ST_Internal_applicativeST)))

let Control_Monad_ST_Internal_semigroupST  = (fun (dictSemigroup: obj) -> (sharpurs_apply (box (Data_Semigroup_Semigroupusd_Dict)) (box ((Map.add "append" (box ((sharpurs_apply (box (Control_Monad_ST_Internal_lift2)) (box ((sharpurs_apply (box (Data_Semigroup_append)) (box (dictSemigroup)))))))) Map.empty)))))

let Control_Monad_ST_Internal_monadRecST  = (sharpurs_apply (box (Control_Monad_Rec_Class_MonadRecusd_Dict)) (box ((Map.add "tailRecM" (box ((fun (f: obj) -> (fun (a: obj) -> (let isLooping = (fun (v: obj) -> (match ((unbox (v))) with | Control_Monad_Rec_Class_Loopusd_Ctor(_) -> (box ((box true))) | _ -> (box ((box false))))) in let fromDone = (sharpurs_apply (box (Partial_Unsafe_unsafePartial)) (box ((fun (_: obj) -> (fun (v: obj) -> (sharpurs_apply (box ((fun (_: obj) -> (match ((unbox (v))) with | Control_Monad_Rec_Class_Doneusd_Ctor(b) -> (box (b)))))) (box (Prim_undefined)))))))) in (sharpurs_apply (box ((sharpurs_apply (box (Control_Monad_ST_Internal_bind)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Control_Monad_ST_Internal_bindFlipped)) (box (Control_Monad_ST_Internal_new))))) (box ((sharpurs_apply (box (f)) (box (a))))))))))) (box ((fun (r: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Control_Monad_ST_Internal_discard)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Control_Monad_ST_Internal_while)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Control_Monad_ST_Internal_map)) (box (isLooping))))) (box ((sharpurs_apply (box (Control_Monad_ST_Internal_read)) (box (r))))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Control_Monad_ST_Internal_bind)) (box ((sharpurs_apply (box (Control_Monad_ST_Internal_read)) (box (r)))))))) (box ((fun (v: obj) -> (match ((unbox (v))) with | Control_Monad_Rec_Class_Loopusd_Ctor(a_prime) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Control_Monad_ST_Internal_bind)) (box ((sharpurs_apply (box (f)) (box (a_prime)))))))) (box ((fun (e: obj) -> (sharpurs_apply (box (Control_Monad_ST_Internal_void)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Control_Monad_ST_Internal_write)) (box (e))))) (box (r)))))))))))) | Control_Monad_Rec_Class_Doneusd_Ctor(_) -> (box ((sharpurs_apply (box (Control_Monad_ST_Internal_pure)) (box (Data_Unit_unit)))))))))))))))))) (box ((fun (_: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Control_Monad_ST_Internal_map)) (box (fromDone))))) (box ((sharpurs_apply (box (Control_Monad_ST_Internal_read)) (box (r))))))))))))))))))) (Map.add "Monad0" (box ((fun (_: obj) -> Control_Monad_ST_Internal_monadST))) Map.empty)))))

let Control_Monad_ST_Internal_monoidST  = (fun (dictMonoid: obj) -> (let semigroupST1 = (sharpurs_apply (box (Control_Monad_ST_Internal_semigroupST)) (box ((sharpurs_apply (box ((Map.find "Semigroup0" (unbox<Map<string, obj>> (dictMonoid))))) (box (Prim_undefined)))))) in (sharpurs_apply (box (Data_Monoid_Monoidusd_Dict)) (box ((Map.add "mempty" (box ((sharpurs_apply (box (Control_Monad_ST_Internal_pure)) (box ((sharpurs_apply (box (Data_Monoid_mempty)) (box (dictMonoid)))))))) (Map.add "Semigroup0" (box ((fun (_: obj) -> semigroupST1))) Map.empty)))))))
