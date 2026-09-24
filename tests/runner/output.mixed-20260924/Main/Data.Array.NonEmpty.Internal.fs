[<AutoOpen>]
module PureScript_Data_Array_NonEmpty_Internal

open System
open System.Collections.Generic

module Data_Array_NonEmpty_Internal_FFI =
    let foldr1Impl = box (fun (f: obj) -> box (fun (xs: obj) ->
        let arr = unbox<obj[]> xs
        let mutable acc = arr.[arr.Length - 1]
        for i = arr.Length - 2 downto 0 do
            acc <- sharpurs_apply (sharpurs_apply f arr.[i]) acc
        acc
    ))
    
    let foldl1Impl = box (fun (f: obj) -> box (fun (xs: obj) ->
        let arr = unbox<obj[]> xs
        let mutable acc = arr.[0]
        for i = 1 to arr.Length - 1 do
            acc <- sharpurs_apply (sharpurs_apply f acc) arr.[i]
        acc
    ))
    
    let traverse1Impl = box (fun (apply: obj) -> box (fun (map_: obj) -> box (fun (f: obj) -> box (fun (xs: obj) ->
        let arr = unbox<obj[]> xs
        let mutable res = sharpurs_apply map_ (box (fun x -> box [| unbox x |])) |> fun m -> sharpurs_apply m (sharpurs_apply f arr.[0])
        for i = 1 to arr.Length - 1 do
            let next = sharpurs_apply f arr.[i]
            let m = sharpurs_apply map_ (box (fun x -> box (fun y -> box (Array.append (unbox<obj[]> x) [| unbox y |]))))
            let mapRes = sharpurs_apply m res
            res <- sharpurs_apply apply mapRes |> fun a -> sharpurs_apply a next
        res
    ))))
    

let Data_Array_NonEmpty_Internal_foldr1Impl = box Data_Array_NonEmpty_Internal_FFI.``foldr1Impl``
let Data_Array_NonEmpty_Internal_foldl1Impl = box Data_Array_NonEmpty_Internal_FFI.``foldl1Impl``
let Data_Array_NonEmpty_Internal_traverse1Impl = box Data_Array_NonEmpty_Internal_FFI.``traverse1Impl``


let Data_Array_NonEmpty_Internal_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Semigroup_semigroupString)))

let Data_Array_NonEmpty_Internal_NonEmptyArray  = (fun (x: obj) -> x)

let Data_Array_NonEmpty_Internal_unfoldable1NonEmptyArray  = Data_Unfoldable1_unfoldable1Array

let Data_Array_NonEmpty_Internal_traversableWithIndexNonEmptyArray  = Data_TraversableWithIndex_traversableWithIndexArray

let Data_Array_NonEmpty_Internal_traversableNonEmptyArray  = Data_Traversable_traversableArray

let Data_Array_NonEmpty_Internal_showNonEmptyArray  = (fun (dictShow: obj) -> (let show = (sharpurs_apply (box (Data_Show_show)) (box ((sharpurs_apply (box (Data_Show_showArray)) (box (dictShow)))))) in (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | xs -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Array_NonEmpty_Internal_append)) (box ((box "(NonEmptyArray ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Array_NonEmpty_Internal_append)) (box ((sharpurs_apply (box (show)) (box (xs)))))))) (box ((box ")"))))))))))))) Map.empty))))))

let Data_Array_NonEmpty_Internal_semigroupNonEmptyArray  = Data_Semigroup_semigroupArray

let Data_Array_NonEmpty_Internal_ordNonEmptyArray  = (fun (dictOrd: obj) -> (sharpurs_apply (box (Data_Ord_ordArray)) (box (dictOrd))))

let Data_Array_NonEmpty_Internal_ord1NonEmptyArray  = Data_Ord_ord1Array

let Data_Array_NonEmpty_Internal_monadNonEmptyArray  = Control_Monad_monadArray

let Data_Array_NonEmpty_Internal_functorWithIndexNonEmptyArray  = Data_FunctorWithIndex_functorWithIndexArray

let Data_Array_NonEmpty_Internal_functorNonEmptyArray  = Data_Functor_functorArray

let Data_Array_NonEmpty_Internal_foldableWithIndexNonEmptyArray  = Data_FoldableWithIndex_foldableWithIndexArray

let Data_Array_NonEmpty_Internal_foldableNonEmptyArray  = Data_Foldable_foldableArray

let rec Data_Array_NonEmpty_Internal_foldable1NonEmptyArray  = (sharpurs_apply (box (Data_Semigroup_Foldable_Foldable1usd_Dict)) (box ((Map.add "foldMap1" (box ((fun (dictSemigroup: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Semigroup_Foldable_foldMap1DefaultL)) (box (Data_Array_NonEmpty_Internal_foldable1NonEmptyArray))))) (box (Data_Array_NonEmpty_Internal_functorNonEmptyArray))))) (box (dictSemigroup)))))) (Map.add "foldr1" (box ((sharpurs_apply (box (Data_Function_Uncurried_runFn2)) (box (Data_Array_NonEmpty_Internal_foldr1Impl))))) (Map.add "foldl1" (box ((sharpurs_apply (box (Data_Function_Uncurried_runFn2)) (box (Data_Array_NonEmpty_Internal_foldl1Impl))))) (Map.add "Foldable0" (box ((fun (_: obj) -> Data_Array_NonEmpty_Internal_foldableNonEmptyArray))) Map.empty)))))))

let rec Data_Array_NonEmpty_Internal_traversable1NonEmptyArray  = (sharpurs_apply (box (Data_Semigroup_Traversable_Traversable1usd_Dict)) (box ((Map.add "traverse1" (box ((fun (dictApply: obj) -> (let apply = (sharpurs_apply (box (Control_Apply_apply)) (box (dictApply))) in let map = (sharpurs_apply (box (Data_Functor_map)) (box ((sharpurs_apply (box ((Map.find "Functor0" (unbox<Map<string, obj>> (dictApply))))) (box (Prim_undefined)))))) in (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Function_Uncurried_runFn3)) (box (Data_Array_NonEmpty_Internal_traverse1Impl))))) (box (apply))))) (box (map))))) (box (f)))))))) (Map.add "sequence1" (box ((fun (dictApply: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Semigroup_Traversable_sequence1Default)) (box (Data_Array_NonEmpty_Internal_traversable1NonEmptyArray))))) (box (dictApply)))))) (Map.add "Foldable10" (box ((fun (_: obj) -> Data_Array_NonEmpty_Internal_foldable1NonEmptyArray))) (Map.add "Traversable1" (box ((fun (_: obj) -> Data_Array_NonEmpty_Internal_traversableNonEmptyArray))) Map.empty)))))))

let Data_Array_NonEmpty_Internal_eqNonEmptyArray  = (fun (dictEq: obj) -> (sharpurs_apply (box (Data_Eq_eqArray)) (box (dictEq))))

let Data_Array_NonEmpty_Internal_eq1NonEmptyArray  = Data_Eq_eq1Array

let Data_Array_NonEmpty_Internal_bindNonEmptyArray  = Control_Bind_bindArray

let Data_Array_NonEmpty_Internal_applyNonEmptyArray  = Control_Apply_applyArray

let Data_Array_NonEmpty_Internal_applicativeNonEmptyArray  = Control_Applicative_applicativeArray

let Data_Array_NonEmpty_Internal_altNonEmptyArray  = Control_Alt_altArray
