[<AutoOpen>]
module PureScript_Data_Unfoldable1

open System
open System.Collections.Generic

module Data_Unfoldable1_FFI =
    let unfoldr1ArrayImpl = box (fun (isNothing: obj) -> box (fun (fromJust: obj) -> box (fun (fst: obj) -> box (fun (snd: obj) -> box (fun (f: obj) -> box (fun (b: obj) ->
        let result = System.Collections.Generic.List<obj>()
        let rec loop value =
            let tuple = sharpurs_apply f value
            result.Add(sharpurs_apply fst tuple)
            let maybe = sharpurs_apply snd tuple
            if unbox<bool> (sharpurs_apply isNothing maybe) then
                result.ToArray() |> box
            else
                loop (sharpurs_apply fromJust maybe)
        loop b
    ))))))
    

let Data_Unfoldable1_unfoldr1ArrayImpl = box Data_Unfoldable1_FFI.``unfoldr1ArrayImpl``


let Data_Unfoldable1_fromJust  = (sharpurs_apply (box (Data_Maybe_fromJust)) (box (Prim_undefined)))

let Data_Unfoldable1_lessThanOrEq  = (sharpurs_apply (box (Data_Ord_lessThanOrEq)) (box (Data_Ord_ordInt)))

let Data_Unfoldable1_sub  = (sharpurs_apply (box (Data_Ring_sub)) (box (Data_Ring_ringInt)))

let Data_Unfoldable1_add  = (sharpurs_apply (box (Data_Semiring_add)) (box (Data_Semiring_semiringInt)))

let Data_Unfoldable1_eq  = (sharpurs_apply (box (Data_Eq_eq)) (box (Data_Eq_eqInt)))

let Data_Unfoldable1_greaterThanOrEq  = (sharpurs_apply (box (Data_Ord_greaterThanOrEq)) (box (Data_Ord_ordInt)))

let Data_Unfoldable1_negate  = (sharpurs_apply (box (Data_Ring_negate)) (box (Data_Ring_ringInt)))

let Data_Unfoldable1_greaterThan  = (sharpurs_apply (box (Data_Ord_greaterThan)) (box (Data_Ord_ordInt)))

let Data_Unfoldable1_Unfoldable1usd_Dict  = (fun (x: obj) -> x)

let Data_Unfoldable1_unfoldr1  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "unfoldr1" (unbox<Map<string, obj>> (v)))))))

let Data_Unfoldable1_unfoldable1Maybe  = (sharpurs_apply (box (Data_Unfoldable1_Unfoldable1usd_Dict)) (box ((Map.add "unfoldr1" (box ((fun (f: obj) -> (fun (b: obj) -> (box (Data_Maybe_Justusd_Ctor((sharpurs_apply (box (Data_Tuple_fst)) (box ((sharpurs_apply (box (f)) (box (b))))))))))))) Map.empty))))

let Data_Unfoldable1_unfoldable1Array  = (sharpurs_apply (box (Data_Unfoldable1_Unfoldable1usd_Dict)) (box ((Map.add "unfoldr1" (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Unfoldable1_unfoldr1ArrayImpl)) (box (Data_Maybe_isNothing))))) (box ((sharpurs_apply (box (Partial_Unsafe_unsafePartial)) (box ((fun (_: obj) -> Data_Unfoldable1_fromJust))))))))) (box (Data_Tuple_fst))))) (box (Data_Tuple_snd))))) Map.empty))))

let Data_Unfoldable1_replicate1  = (fun (dictUnfoldable1: obj) -> (let unfoldr11 = (sharpurs_apply (box (Data_Unfoldable1_unfoldr1)) (box (dictUnfoldable1))) in (fun (n: obj) -> (fun (v: obj) -> (let step = (fun (i: obj) -> (match ((unbox (i))) with | i1 when (unbox (sharpurs_apply (box ((sharpurs_apply (box (Data_Unfoldable1_lessThanOrEq)) (box (i1))))) (box ((box 0))))) -> (box ((box (Data_Tuple_Tupleusd_Ctor(v, (box Data_Maybe_Nothingusd_Ctor)))))) | i1 when (unbox Data_Boolean_otherwise) -> (box ((box (Data_Tuple_Tupleusd_Ctor(v, (box (Data_Maybe_Justusd_Ctor((sharpurs_apply (box ((sharpurs_apply (box (Data_Unfoldable1_sub)) (box (i1))))) (box ((box 1)))))))))))))) in (sharpurs_apply (box ((sharpurs_apply (box (unfoldr11)) (box (step))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Unfoldable1_sub)) (box (n))))) (box ((box 1))))))))))))

let Data_Unfoldable1_replicate1A  = (fun (dictApply: obj) -> (fun (dictUnfoldable1: obj) -> (let replicate11 = (sharpurs_apply (box (Data_Unfoldable1_replicate1)) (box (dictUnfoldable1))) in (fun (dictTraversable1: obj) -> (let sequence1 = (sharpurs_apply (box ((sharpurs_apply (box (Data_Semigroup_Traversable_sequence1)) (box (dictTraversable1))))) (box (dictApply))) in (fun (n: obj) -> (fun (m: obj) -> (sharpurs_apply (box (sequence1)) (box ((sharpurs_apply (box ((sharpurs_apply (box (replicate11)) (box (n))))) (box (m)))))))))))))

let Data_Unfoldable1_singleton  = (fun (dictUnfoldable1: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Unfoldable1_replicate1)) (box (dictUnfoldable1))))) (box ((box 1)))))

let Data_Unfoldable1_range  = (fun (dictUnfoldable1: obj) -> (let unfoldr11 = (sharpurs_apply (box (Data_Unfoldable1_unfoldr1)) (box (dictUnfoldable1))) in (fun (start: obj) -> (fun (end_: obj) -> (let go = (fun (delta: obj) -> (fun (i: obj) -> (let i_prime = (sharpurs_apply (box ((sharpurs_apply (box (Data_Unfoldable1_add)) (box (i))))) (box (delta))) in (box (Data_Tuple_Tupleusd_Ctor(i, (match ((unbox ((sharpurs_apply (box ((sharpurs_apply (box (Data_Unfoldable1_eq)) (box (i))))) (box (end_)))))) with | LitBool true () -> (box ((box Data_Maybe_Nothingusd_Ctor))) | _ -> (box ((box (Data_Maybe_Justusd_Ctor(i_prime)))))))))))) in (let delta = (match ((unbox ((sharpurs_apply (box ((sharpurs_apply (box (Data_Unfoldable1_greaterThanOrEq)) (box (end_))))) (box (start)))))) with | LitBool true () -> (box ((box 1))) | _ -> (box ((sharpurs_apply (box (Data_Unfoldable1_negate)) (box ((box 1))))))) in (sharpurs_apply (box ((sharpurs_apply (box (unfoldr11)) (box ((sharpurs_apply (box (go)) (box (delta)))))))) (box (start)))))))))

let Data_Unfoldable1_iterateN  = (fun (dictUnfoldable1: obj) -> (let unfoldr11 = (sharpurs_apply (box (Data_Unfoldable1_unfoldr1)) (box (dictUnfoldable1))) in (fun (n: obj) -> (fun (f: obj) -> (fun (s: obj) -> (let go = (fun (v: obj) -> (match ((unbox (v))) with | Data_Tuple_Tupleusd_Ctor(x, n_prime) -> (box ((box (Data_Tuple_Tupleusd_Ctor(x, (match ((unbox ((sharpurs_apply (box ((sharpurs_apply (box (Data_Unfoldable1_greaterThan)) (box (n_prime))))) (box ((box 0))))))) with | LitBool true () -> (box ((box (Data_Maybe_Justusd_Ctor((box (Data_Tuple_Tupleusd_Ctor((sharpurs_apply (box (f)) (box (x))), (sharpurs_apply (box ((sharpurs_apply (box (Data_Unfoldable1_sub)) (box (n_prime))))) (box ((box 1)))))))))))) | _ -> (box ((box Data_Maybe_Nothingusd_Ctor))))))))))) in (sharpurs_apply (box ((sharpurs_apply (box (unfoldr11)) (box (go))))) (box ((box (Data_Tuple_Tupleusd_Ctor(s, (sharpurs_apply (box ((sharpurs_apply (box (Data_Unfoldable1_sub)) (box (n))))) (box ((box 1))))))))))))))))
