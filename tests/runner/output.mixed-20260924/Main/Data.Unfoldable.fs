[<AutoOpen>]
module PureScript_Data_Unfoldable

open System
open System.Collections.Generic

module Data_Unfoldable_FFI =
    let unfoldrArrayImpl = box (fun (isNothing: obj) -> box (fun (fromJust: obj) -> box (fun (fst: obj) -> box (fun (snd: obj) -> box (fun (f: obj) -> box (fun (b: obj) ->
        let result = System.Collections.Generic.List<obj>()
        let rec loop value =
            let maybe = sharpurs_apply f value
            if unbox<bool> (sharpurs_apply isNothing maybe) then
                result.ToArray() |> box
            else
                let tuple = sharpurs_apply fromJust maybe
                result.Add(sharpurs_apply fst tuple)
                loop (sharpurs_apply snd tuple)
        loop b
    ))))))
    

let Data_Unfoldable_unfoldrArrayImpl = box Data_Unfoldable_FFI.``unfoldrArrayImpl``


let Data_Unfoldable_map  = (sharpurs_apply (box (Data_Functor_map)) (box (Data_Maybe_functorMaybe)))

let Data_Unfoldable_fromJust  = (sharpurs_apply (box (Data_Maybe_fromJust)) (box (Prim_undefined)))

let Data_Unfoldable_lessThanOrEq  = (sharpurs_apply (box (Data_Ord_lessThanOrEq)) (box (Data_Ord_ordInt)))

let Data_Unfoldable_sub  = (sharpurs_apply (box (Data_Ring_sub)) (box (Data_Ring_ringInt)))

let Data_Unfoldable_Unfoldableusd_Dict  = (fun (x: obj) -> x)

let Data_Unfoldable_unfoldr  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "unfoldr" (unbox<Map<string, obj>> (v)))))))

let Data_Unfoldable_unfoldableMaybe  = (sharpurs_apply (box (Data_Unfoldable_Unfoldableusd_Dict)) (box ((Map.add "unfoldr" (box ((fun (f: obj) -> (fun (b: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Unfoldable_map)) (box (Data_Tuple_fst))))) (box ((sharpurs_apply (box (f)) (box (b)))))))))) (Map.add "Unfoldable10" (box ((fun (_: obj) -> Data_Unfoldable1_unfoldable1Maybe))) Map.empty)))))

let Data_Unfoldable_unfoldableArray  = (sharpurs_apply (box (Data_Unfoldable_Unfoldableusd_Dict)) (box ((Map.add "unfoldr" (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Unfoldable_unfoldrArrayImpl)) (box (Data_Maybe_isNothing))))) (box ((sharpurs_apply (box (Partial_Unsafe_unsafePartial)) (box ((fun (_: obj) -> Data_Unfoldable_fromJust))))))))) (box (Data_Tuple_fst))))) (box (Data_Tuple_snd))))) (Map.add "Unfoldable10" (box ((fun (_: obj) -> Data_Unfoldable1_unfoldable1Array))) Map.empty)))))

let Data_Unfoldable_replicate  = (fun (dictUnfoldable: obj) -> (let unfoldr1 = (sharpurs_apply (box (Data_Unfoldable_unfoldr)) (box (dictUnfoldable))) in (fun (n: obj) -> (fun (v: obj) -> (let step = (fun (i: obj) -> (match ((unbox ((sharpurs_apply (box ((sharpurs_apply (box (Data_Unfoldable_lessThanOrEq)) (box (i))))) (box ((box 0))))))) with | LitBool true () -> (box ((box Data_Maybe_Nothingusd_Ctor))) | _ -> (box ((box (Data_Maybe_Justusd_Ctor((box (Data_Tuple_Tupleusd_Ctor(v, (sharpurs_apply (box ((sharpurs_apply (box (Data_Unfoldable_sub)) (box (i))))) (box ((box 1)))))))))))))) in (sharpurs_apply (box ((sharpurs_apply (box (unfoldr1)) (box (step))))) (box (n))))))))

let Data_Unfoldable_replicateA  = (fun (dictApplicative: obj) -> (fun (dictUnfoldable: obj) -> (let replicate1 = (sharpurs_apply (box (Data_Unfoldable_replicate)) (box (dictUnfoldable))) in (fun (dictTraversable: obj) -> (let sequence = (sharpurs_apply (box ((sharpurs_apply (box (Data_Traversable_sequence)) (box (dictTraversable))))) (box (dictApplicative))) in (fun (n: obj) -> (fun (m: obj) -> (sharpurs_apply (box (sequence)) (box ((sharpurs_apply (box ((sharpurs_apply (box (replicate1)) (box (n))))) (box (m)))))))))))))

let Data_Unfoldable_none  = (fun (dictUnfoldable: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Unfoldable_unfoldr)) (box (dictUnfoldable))))) (box ((sharpurs_apply (box (Data_Function_const)) (box ((box Data_Maybe_Nothingusd_Ctor))))))))) (box (Data_Unit_unit))))

let Data_Unfoldable_fromMaybe  = (fun (dictUnfoldable: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Unfoldable_unfoldr)) (box (dictUnfoldable))))) (box ((fun (b: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Unfoldable_map)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Function_flip)) (box ((fun (usd__arg1: obj) -> (fun (usd__arg2: obj) -> (box (Data_Tuple_Tupleusd_Ctor(usd__arg1, usd__arg2)))))))))) (box ((box Data_Maybe_Nothingusd_Ctor))))))))) (box (b))))))))
