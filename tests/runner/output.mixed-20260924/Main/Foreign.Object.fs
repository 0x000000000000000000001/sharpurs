[<AutoOpen>]
module PureScript_Foreign_Object

open System
open System.Collections.Generic

module Foreign_Object_FFI =
    open System
    open System.Collections.Generic
    
    let _copyST = fun (m: obj) ->
        box (fun () ->
            let dict = unbox<Map<string, obj>> m
            let res = Dictionary<string, obj>()
            for kvp in dict do
                res.[kvp.Key] <- kvp.Value
            box res
        )
    
    let empty = box (Map.empty<string, obj>)
    
    let runST = fun (f: obj) ->
        let dict = unbox<Dictionary<string, obj>> (sharpurs_apply f (box ()))
        let res = Dictionary<string, obj>()
        for kvp in dict do
            res.[kvp.Key] <- kvp.Value
        box (res |> Seq.map (fun kv -> kv.Key, kv.Value) |> Map.ofSeq)
    
    let _fmapObject = fun (m: obj) -> fun (f: obj) ->
        let dict = unbox<Map<string, obj>> m
        let res = Dictionary<string, obj>()
        for kvp in dict do
            res.[kvp.Key] <- sharpurs_apply f (box kvp.Value)
        box (res |> Seq.map (fun kv -> kv.Key, kv.Value) |> Map.ofSeq)
    
    let _mapWithKey = fun (m: obj) -> fun (f: obj) ->
        let dict = unbox<Map<string, obj>> m
        let res = Dictionary<string, obj>()
        for kvp in dict do
            res.[kvp.Key] <- sharpurs_apply (sharpurs_apply f (box kvp.Key)) (box kvp.Value)
        box (res |> Seq.map (fun kv -> kv.Key, kv.Value) |> Map.ofSeq)
    
    let _foldM = fun (bind: obj) -> fun (f: obj) -> fun (mz: obj) -> fun (m: obj) ->
        let dict = unbox<Map<string, obj>> m
        let mutable acc = mz
        let g = fun (k: string) -> fun (z: obj) ->
            sharpurs_apply (sharpurs_apply (sharpurs_apply f z) (box k)) (box dict.[k])
        for kvp in dict do
            acc <- sharpurs_apply (sharpurs_apply bind acc) (box (g kvp.Key))
        acc
    
    let _foldSCObject = fun (m: obj) -> fun (z: obj) -> fun (f: obj) -> fun (fromMaybe: obj) ->
        let dict = unbox<Map<string, obj>> m
        let mutable acc = z
        let mutable continue_loop = true
        for kvp in dict do
            if continue_loop then
                let maybeR = sharpurs_apply (sharpurs_apply (sharpurs_apply f acc) (box kvp.Key)) (box kvp.Value)
                let r = sharpurs_apply (sharpurs_apply fromMaybe undefined) maybeR
                if isNull r then
                    continue_loop <- false
                else
                    acc <- r
        acc
    
    let all = fun (f: obj) -> fun (m: obj) ->
        let dict = unbox<Map<string, obj>> m
        let mutable res = true
        for kvp in dict do
            if res then
                res <- unbox<bool> (sharpurs_apply (sharpurs_apply f (box kvp.Key)) (box kvp.Value))
        box res
    
    let size = fun (m: obj) ->
        let dict = unbox<Map<string, obj>> m
        box dict.Count
    
    let _lookup = fun (no: obj) -> fun (yes: obj) -> fun (k: obj) -> fun (m: obj) ->
        let dict = unbox<Map<string, obj>> m
        let key = unbox<string> k
        match Map.tryFind key dict with
        | Some v -> sharpurs_apply yes (box v)
        | None -> no
    
    let _lookupST = fun (no: obj) -> fun (yes: obj) -> fun (k: obj) -> fun (m: obj) ->
        box (fun _ ->
            let dict = unbox<Dictionary<string, obj>> m
            let key = unbox<string> k
            let mutable v = box null
            if dict.TryGetValue(key, &v) then
                sharpurs_apply yes v
            else
                no
        )
    
    let toArrayWithKey = fun (f: obj) -> fun (m: obj) ->
        let dict = unbox<Map<string, obj>> m
        let res = Array.zeroCreate dict.Count
        let mutable i = 0
        for kvp in dict do
            res.[i] <- sharpurs_apply (sharpurs_apply f (box kvp.Key)) (box kvp.Value)
            i <- i + 1
        box res
    
    let keys = fun (m: obj) ->
        let dict = unbox<Map<string, obj>> m
        let res = Array.zeroCreate dict.Count
        let mutable i = 0
        for kvp in dict do
            res.[i] <- box kvp.Key
            i <- i + 1
        box res
    

let Foreign_Object__copyST = box (Foreign_Object_FFI.``_copyST``)
let Foreign_Object__fmapObject = box (Foreign_Object_FFI.``_fmapObject``)
let Foreign_Object__foldM = box (Foreign_Object_FFI.``_foldM``)
let Foreign_Object__foldSCObject = box (Foreign_Object_FFI.``_foldSCObject``)
let Foreign_Object__lookup = box (Foreign_Object_FFI.``_lookup``)
let Foreign_Object__lookupST = box (Foreign_Object_FFI.``_lookupST``)
let Foreign_Object__mapWithKey = box (Foreign_Object_FFI.``_mapWithKey``)
let Foreign_Object_all = box (Foreign_Object_FFI.``all``)
let Foreign_Object_empty = box (Foreign_Object_FFI.``empty``)
let Foreign_Object_keys = box (Foreign_Object_FFI.``keys``)
let Foreign_Object_runST = box (Foreign_Object_FFI.``runST``)
let Foreign_Object_size = box (Foreign_Object_FFI.``size``)
let Foreign_Object_toArrayWithKey = box (Foreign_Object_FFI.``toArrayWithKey``)


let Foreign_Object_showTuple  = (sharpurs_apply (box ((box Data_Tuple_showTuple))) (box ((box Data_Show_showString))))

let Foreign_Object_void  = (sharpurs_apply (box ((box Data_Functor_void))) (box ((box Control_Monad_ST_Internal_functorST))))

let Foreign_Object_identity  = (sharpurs_apply (box ((box Control_Category_identity))) (box ((box Control_Category_categoryFn))))

let Foreign_Object_ordTuple  = (sharpurs_apply (box ((box Data_Tuple_ordTuple))) (box ((box Data_Ord_ordString))))

let Foreign_Object_values  = (sharpurs_apply (box ((box Foreign_Object_toArrayWithKey))) (box ((box (fun (v: obj) -> (box (fun (v1: obj) -> (box v1))))))))

let Foreign_Object_toUnfoldable  = (box (fun (dictUnfoldable: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((sharpurs_apply (box ((box Data_Array_toUnfoldable))) (box ((box dictUnfoldable))))))))) (box ((sharpurs_apply (box ((box Foreign_Object_toArrayWithKey))) (box ((box Data_Tuple_Tuple)))))))))

let Foreign_Object_toAscUnfoldable  = (box (fun (dictUnfoldable: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((sharpurs_apply (box ((box Data_Array_toUnfoldable))) (box ((box dictUnfoldable))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_sortWith))) (box ((box Data_Ord_ordString)))))) (box ((box Data_Tuple_fst))))))))) (box ((sharpurs_apply (box ((box Foreign_Object_toArrayWithKey))) (box ((box Data_Tuple_Tuple))))))))))))

let Foreign_Object_toAscArray  = (sharpurs_apply (box ((box Foreign_Object_toAscUnfoldable))) (box ((box Data_Unfoldable_unfoldableArray))))

let Foreign_Object_toArray  = (sharpurs_apply (box ((box Foreign_Object_toArrayWithKey))) (box ((box Data_Tuple_Tuple))))

let Foreign_Object_thawST  = (box Foreign_Object__copyST)

let Foreign_Object_singleton  = (box (fun (k: obj) -> (box (fun (v: obj) -> (sharpurs_apply (box ((box Foreign_Object_runST))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_bindFlipped))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Foreign_Object_ST_poke))) (box ((box k)))))) (box ((box v))))))))) (box ((box Foreign_Object_ST_new)))))))))))

let Foreign_Object_showObject  = (box (fun (dictShow: obj) -> (let showArray = (sharpurs_apply (box ((box Data_Show_showArray))) (box ((sharpurs_apply (box ((box Foreign_Object_showTuple))) (box ((box dictShow))))))) in (sharpurs_apply (box ((box Data_Show_Showusd_Dict))) (box ((box ((Map.add "show" (box ((box (fun (m: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semigroup_append))) (box ((box Data_Semigroup_semigroupString)))))) (box ((box "(fromFoldable ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semigroup_append))) (box ((box Data_Semigroup_semigroupString)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Show_show))) (box ((box showArray)))))) (box ((sharpurs_apply (box ((box Foreign_Object_toArray))) (box ((box m)))))))))))) (box ((box ")"))))))))))) Map.empty)))))))))

let Foreign_Object_mutate  = (box (fun (f: obj) -> (box (fun (m: obj) -> (sharpurs_apply (box ((box Foreign_Object_runST))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_bind))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((sharpurs_apply (box ((box Foreign_Object_thawST))) (box ((box m))))))))) (box ((box (fun (s: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_bind))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((sharpurs_apply (box ((box f))) (box ((box s))))))))) (box ((box (fun (usd__unused: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Control_Applicative_pure))) (box ((box Control_Monad_ST_Internal_applicativeST)))))) (box ((box s)))))))))))))))))))))

let Foreign_Object_member  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_Uncurried_runFn4))) (box ((box Foreign_Object__lookup)))))) (box ((box false)))))) (box ((sharpurs_apply (box ((box Data_Function_const))) (box ((box true)))))))

let Foreign_Object_mapWithKey  = (box (fun (f: obj) -> (box (fun (m: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_Uncurried_runFn2))) (box ((box Foreign_Object__mapWithKey)))))) (box ((box m)))))) (box ((box f))))))))

let Foreign_Object_lookup  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_Uncurried_runFn4))) (box ((box Foreign_Object__lookup)))))) (box ((box Data_Maybe_Nothing)))))) (box ((box Data_Maybe_Just))))

let Foreign_Object_isSubmap  = (box (fun (dictEq: obj) -> (box (fun (m1: obj) -> (box (fun (m2: obj) -> (let f = (box (fun (k: obj) -> (box (fun (v: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_Uncurried_runFn4))) (box ((box Foreign_Object__lookup)))))) (box ((box false)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Eq_eq))) (box ((box dictEq)))))) (box ((box v))))))))) (box ((box k)))))) (box ((box m2)))))))) in (sharpurs_apply (box ((sharpurs_apply (box ((box Foreign_Object_all))) (box ((box f)))))) (box ((box m1)))))))))))

let Foreign_Object_isEmpty  = (sharpurs_apply (box ((box Foreign_Object_all))) (box ((box (fun (v: obj) -> (box (fun (v1: obj) -> (box false))))))))

let Foreign_Object_insert  = (box (fun (k: obj) -> (box (fun (v: obj) -> (sharpurs_apply (box ((box Foreign_Object_mutate))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Foreign_Object_ST_poke))) (box ((box k)))))) (box ((box v)))))))))))

let Foreign_Object_functorObject  = (sharpurs_apply (box ((box Data_Functor_Functorusd_Dict))) (box ((box ((Map.add "map" (box ((box (fun (f: obj) -> (box (fun (m: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_Uncurried_runFn2))) (box ((box Foreign_Object__fmapObject)))))) (box ((box m)))))) (box ((box f)))))))))) Map.empty))))))

let Foreign_Object_functorWithIndexObject  = (sharpurs_apply (box ((box Data_FunctorWithIndex_FunctorWithIndexusd_Dict))) (box ((box ((Map.add "mapWithIndex" (box ((box Foreign_Object_mapWithKey))) (Map.add "Functor0" (box ((box (fun (usd__unused: obj) -> (box Foreign_Object_functorObject))))) Map.empty)))))))

let Foreign_Object_fromHomogeneous  = (box (fun (usd__unused: obj) -> (box Unsafe_Coerce_unsafeCoerce)))

let Foreign_Object_fromFoldableWithIndex  = (box (fun (dictFoldableWithIndex: obj) -> (box (fun (l: obj) -> (sharpurs_apply (box ((box Foreign_Object_runST))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_bind))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((box Foreign_Object_ST_new)))))) (box ((box (fun (s: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_discard))) (box ((box Control_Bind_discardUnit)))))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_FoldableWithIndex_forWithIndex_))) (box ((box Control_Monad_ST_Internal_applicativeST)))))) (box ((box dictFoldableWithIndex)))))) (box ((box l)))))) (box ((box (fun (k: obj) -> (box (fun (v: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Foreign_Object_ST_poke))) (box ((box k)))))) (box ((box v)))))) (box ((box s)))))))))))))))) (box ((box (fun (usd__unused: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Control_Applicative_pure))) (box ((box Control_Monad_ST_Internal_applicativeST)))))) (box ((box s)))))))))))))))))))))

let Foreign_Object_fromFoldableWith  = (box (fun (dictFoldable: obj) -> (box (fun (f: obj) -> (box (fun (l: obj) -> (sharpurs_apply (box ((box Foreign_Object_runST))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_bind))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((box Foreign_Object_ST_new)))))) (box ((box (fun (s: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_discard))) (box ((box Control_Bind_discardUnit)))))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Foldable_for_))) (box ((box Control_Monad_ST_Internal_applicativeST)))))) (box ((box dictFoldable)))))) (box ((box l)))))) (box ((box (fun (v: obj) -> (match ((unbox ((box v)))) with | k -> ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_bind))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_Uncurried_runFn4))) (box ((box Foreign_Object__lookupST)))))) (box ((box v1)))))) (box ((sharpurs_apply (box ((box f))) (box ((box v1))))))))) (box ((box k)))))) (box ((box s))))))))) (box ((box (fun (v_prime: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Foreign_Object_ST_poke))) (box ((box k)))))) (box ((box v_prime)))))) (box ((box s))))))))))))))))))))) (box ((box (fun (usd__unused: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Control_Applicative_pure))) (box ((box Control_Monad_ST_Internal_applicativeST)))))) (box ((box s)))))))))))))))))))))))

let Foreign_Object_fromFoldable  = (box (fun (dictFoldable: obj) -> (box (fun (l: obj) -> (sharpurs_apply (box ((box Foreign_Object_runST))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_bind))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((box Foreign_Object_ST_new)))))) (box ((box (fun (s: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_discard))) (box ((box Control_Bind_discardUnit)))))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Monad_ST_Internal_foreach))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_fromFoldable))) (box ((box dictFoldable)))))) (box ((box l))))))))) (box ((box (fun (v: obj) -> (match ((unbox ((box v)))) with | k -> ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Foreign_Object_void)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Foreign_Object_ST_poke))) (box ((box k)))))) (box ((box v1)))))) (box ((box s))))))))))))))))))) (box ((box (fun (usd__unused: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Control_Applicative_pure))) (box ((box Control_Monad_ST_Internal_applicativeST)))))) (box ((box s)))))))))))))))))))))

let Foreign_Object_freezeST  = (box Foreign_Object__copyST)

let Foreign_Object_foldMaybe  = (box (fun (f: obj) -> (box (fun (z: obj) -> (box (fun (m: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_Uncurried_runFn4))) (box ((box Foreign_Object__foldSCObject)))))) (box ((box m)))))) (box ((box z)))))) (box ((box f)))))) (box ((box Data_Maybe_fromMaybe))))))))))

let Foreign_Object_foldM  = (box (fun (dictMonad: obj) -> (let bind_var = (sharpurs_apply (box ((box Control_Bind_bind))) (box ((sharpurs_apply (box ((Map.find "Bind1" (unbox<Map<string, obj>> ((box dictMonad)))))) (box ((box Prim_undefined))))))) in let Applicative0 = (sharpurs_apply (box ((Map.find "Applicative0" (unbox<Map<string, obj>> ((box dictMonad)))))) (box ((box Prim_undefined)))) in (box (fun (f: obj) -> (box (fun (z: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Foreign_Object__foldM))) (box ((box bind_var)))))) (box ((box f)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Applicative_pure))) (box ((box Applicative0)))))) (box ((box z))))))))))))))

let Foreign_Object_union  = (box (fun (m: obj) -> (sharpurs_apply (box ((box Foreign_Object_mutate))) (box ((box (fun (s: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Foreign_Object_foldM))) (box ((box Control_Monad_ST_Internal_monadST)))))) (box ((box (fun (s_prime: obj) -> (box (fun (k: obj) -> (box (fun (v: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Foreign_Object_ST_poke))) (box ((box k)))))) (box ((box v)))))) (box ((box s_prime))))))))))))))) (box ((box s)))))) (box ((box m)))))))))))

let Foreign_Object_unions  = (box (fun (dictFoldable: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Foldable_foldl))) (box ((box dictFoldable)))))) (box ((box Foreign_Object_union)))))) (box ((box Foreign_Object_empty))))))

let Foreign_Object_unionWith  = (box (fun (f: obj) -> (box (fun (m1: obj) -> (box (fun (m2: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Foreign_Object_mutate))) (box ((box (fun (s1: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Foreign_Object_foldM))) (box ((box Control_Monad_ST_Internal_monadST)))))) (box ((box (fun (s2: obj) -> (box (fun (k: obj) -> (box (fun (v1: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Foreign_Object_ST_poke))) (box ((box k)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_Uncurried_runFn4))) (box ((box Foreign_Object__lookup)))))) (box ((box v1)))))) (box ((box (fun (v2: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box f))) (box ((box v1)))))) (box ((box v2))))))))))) (box ((box k)))))) (box ((box m2))))))))) (box ((box s2))))))))))))))) (box ((box s1)))))) (box ((box m1))))))))))) (box ((box m2))))))))))

let Foreign_Object_semigroupObject  = (box (fun (dictSemigroup: obj) -> (sharpurs_apply (box ((box Data_Semigroup_Semigroupusd_Dict))) (box ((box ((Map.add "append" (box ((sharpurs_apply (box ((box Foreign_Object_unionWith))) (box ((sharpurs_apply (box ((box Data_Semigroup_append))) (box ((box dictSemigroup))))))))) Map.empty))))))))

let Foreign_Object_monoidObject  = (box (fun (dictSemigroup: obj) -> (let semigroupObject1 = (sharpurs_apply (box ((box Foreign_Object_semigroupObject))) (box ((box dictSemigroup)))) in (sharpurs_apply (box ((box Data_Monoid_Monoidusd_Dict))) (box ((box ((Map.add "mempty" (box ((box Foreign_Object_empty))) (Map.add "Semigroup0" (box ((box (fun (usd__unused: obj) -> (box semigroupObject1))))) Map.empty))))))))))

let Foreign_Object_fold  = (sharpurs_apply (box ((box Foreign_Object__foldM))) (box ((box Data_Function_applyFlipped))))

let Foreign_Object_foldMap  = (box (fun (dictMonoid: obj) -> (let Semigroup0 = (sharpurs_apply (box ((Map.find "Semigroup0" (unbox<Map<string, obj>> ((box dictMonoid)))))) (box ((box Prim_undefined)))) in let mempty = (sharpurs_apply (box ((box Data_Monoid_mempty))) (box ((box dictMonoid)))) in (box (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Foreign_Object_fold))) (box ((box (fun (acc: obj) -> (box (fun (k: obj) -> (box (fun (v: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semigroup_append))) (box ((box Semigroup0)))))) (box ((box acc)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box f))) (box ((box k)))))) (box ((box v)))))))))))))))))) (box ((box mempty)))))))))

let Foreign_Object_foldableObject  = (sharpurs_apply (box ((box Data_Foldable_Foldableusd_Dict))) (box ((box ((Map.add "foldl" (box ((box (fun (f: obj) -> (sharpurs_apply (box ((box Foreign_Object_fold))) (box ((box (fun (z: obj) -> (box (fun (v: obj) -> (sharpurs_apply (box ((box f))) (box ((box z))))))))))))))) (Map.add "foldr" (box ((box (fun (f: obj) -> (box (fun (z: obj) -> (box (fun (m: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Foldable_foldr))) (box ((box Data_Foldable_foldableArray)))))) (box ((box f)))))) (box ((box z)))))) (box ((sharpurs_apply (box ((box Foreign_Object_values))) (box ((box m))))))))))))))) (Map.add "foldMap" (box ((box (fun (dictMonoid: obj) -> (box (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Foreign_Object_foldMap))) (box ((box dictMonoid)))))) (box ((sharpurs_apply (box ((box Data_Function_const))) (box ((box f))))))))))))) Map.empty))))))))

let Foreign_Object_foldableWithIndexObject  = (sharpurs_apply (box ((box Data_FoldableWithIndex_FoldableWithIndexusd_Dict))) (box ((box ((Map.add "foldlWithIndex" (box ((box (fun (f: obj) -> (sharpurs_apply (box ((box Foreign_Object_fold))) (box ((sharpurs_apply (box ((box Data_Function_flip))) (box ((box f))))))))))) (Map.add "foldrWithIndex" (box ((box (fun (f: obj) -> (box (fun (z: obj) -> (box (fun (m: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Foldable_foldr))) (box ((box Data_Foldable_foldableArray)))))) (box ((sharpurs_apply (box ((box Data_Tuple_uncurry))) (box ((box f))))))))) (box ((box z)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Foreign_Object_toArrayWithKey))) (box ((box Data_Tuple_Tuple)))))) (box ((box m))))))))))))))) (Map.add "foldMapWithIndex" (box ((box (fun (dictMonoid: obj) -> (sharpurs_apply (box ((box Foreign_Object_foldMap))) (box ((box dictMonoid)))))))) (Map.add "Foldable0" (box ((box (fun (usd__unused: obj) -> (box Foreign_Object_foldableObject))))) Map.empty)))))))))

let rec Foreign_Object_traversableWithIndexObject : obj = ((sharpurs_apply (box ((box Data_TraversableWithIndex_TraversableWithIndexusd_Dict))) (box ((box ((Map.add "traverseWithIndex" (box ((box (fun (dictApplicative: obj) -> (let Apply0 = (sharpurs_apply (box ((Map.find "Apply0" (unbox<Map<string, obj>> ((box dictApplicative)))))) (box ((box Prim_undefined)))) in let Functor0 = (sharpurs_apply (box ((Map.find "Functor0" (unbox<Map<string, obj>> ((sharpurs_apply (box ((Map.find "Apply0" (unbox<Map<string, obj>> ((box dictApplicative)))))) (box ((box Prim_undefined))))))))) (box ((box Prim_undefined)))) in (box (fun (f: obj) -> (box (fun (ms: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Foreign_Object_fold))) (box ((box (fun (acc: obj) -> (box (fun (k: obj) -> (box (fun (v: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Apply_apply))) (box ((box Apply0)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Functor_map))) (box ((box Functor0)))))) (box ((sharpurs_apply (box ((box Data_Function_flip))) (box ((sharpurs_apply (box ((box Foreign_Object_insert))) (box ((box k)))))))))))) (box ((box acc))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box f))) (box ((box k)))))) (box ((box v)))))))))))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Applicative_pure))) (box ((box dictApplicative)))))) (box ((box Foreign_Object_empty))))))))) (box ((box ms))))))))))))) (Map.add "FunctorWithIndex0" (box ((box (fun (usd__unused: obj) -> (box Foreign_Object_functorWithIndexObject))))) (Map.add "FoldableWithIndex1" (box ((box (fun (usd__unused: obj) -> (box Foreign_Object_foldableWithIndexObject))))) (Map.add "Traversable2" (box ((box (fun (usd__unused: obj) -> (box Foreign_Object_traversableObject))))) Map.empty))))))))))
 and Foreign_Object_traversableObject : obj = ((sharpurs_apply (box ((box Data_Traversable_Traversableusd_Dict))) (box ((box ((Map.add "traverse" (box ((box (fun (dictApplicative: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_TraversableWithIndex_traverseWithIndex))) (box ((box Foreign_Object_traversableWithIndexObject)))))) (box ((box dictApplicative))))))))) (box ((box Data_Function_const)))))))) (Map.add "sequence" (box ((box (fun (dictApplicative: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Traversable_traverse))) (box ((box Foreign_Object_traversableObject)))))) (box ((box dictApplicative)))))) (box ((box Foreign_Object_identity)))))))) (Map.add "Functor0" (box ((box (fun (usd__unused: obj) -> (box Foreign_Object_functorObject))))) (Map.add "Foldable1" (box ((box (fun (usd__unused: obj) -> (box Foreign_Object_foldableObject))))) Map.empty))))))))))


let Foreign_Object_filterWithKey  = (box (fun (predicate: obj) -> (box (fun (m: obj) -> (let go = (let step = (box (fun (acc: obj) -> (box (fun (k: obj) -> (box (fun (v: obj) -> (match ((unbox ((sharpurs_apply (box ((sharpurs_apply (box ((box predicate))) (box ((box k)))))) (box ((box v))))))) with | LitBool true () -> ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Foreign_Object_ST_poke))) (box ((box k)))))) (box ((box v)))))) (box ((box acc))))) | _ -> ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Applicative_pure))) (box ((box Control_Monad_ST_Internal_applicativeST)))))) (box ((box acc)))))))))))) in (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_bind))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((box Foreign_Object_ST_new)))))) (box ((box (fun (m_prime: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Foreign_Object_foldM))) (box ((box Control_Monad_ST_Internal_monadST)))))) (box ((box step)))))) (box ((box m_prime)))))) (box ((box m)))))))))) in (sharpurs_apply (box ((box Foreign_Object_runST))) (box ((box go)))))))))

let Foreign_Object_filterKeys  = (box (fun (predicate: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Foreign_Object_filterWithKey)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_Function_const)))))) (box ((box predicate)))))))))

let Foreign_Object_filter  = (box (fun (predicate: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Foreign_Object_filterWithKey)))))) (box ((sharpurs_apply (box ((box Data_Function_const))) (box ((box predicate)))))))))

let Foreign_Object_eqObject  = (box (fun (dictEq: obj) -> (sharpurs_apply (box ((box Data_Eq_Equsd_Dict))) (box ((box ((Map.add "eq" (box ((box (fun (m1: obj) -> (box (fun (m2: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_HeytingAlgebra_conj))) (box ((box Data_HeytingAlgebra_heytingAlgebraBoolean)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Foreign_Object_isSubmap))) (box ((box dictEq)))))) (box ((box m1)))))) (box ((box m2))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Foreign_Object_isSubmap))) (box ((box dictEq)))))) (box ((box m2)))))) (box ((box m1))))))))))))) Map.empty))))))))

let Foreign_Object_ordObject  = (box (fun (dictOrd: obj) -> (let ordArray = (sharpurs_apply (box ((box Data_Ord_ordArray))) (box ((sharpurs_apply (box ((box Foreign_Object_ordTuple))) (box ((box dictOrd))))))) in let eqObject1 = (sharpurs_apply (box ((box Foreign_Object_eqObject))) (box ((sharpurs_apply (box ((Map.find "Eq0" (unbox<Map<string, obj>> ((box dictOrd)))))) (box ((box Prim_undefined))))))) in (sharpurs_apply (box ((box Data_Ord_Ordusd_Dict))) (box ((box ((Map.add "compare" (box ((box (fun (m1: obj) -> (box (fun (m2: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Ord_compare))) (box ((box ordArray)))))) (box ((sharpurs_apply (box ((box Foreign_Object_toAscArray))) (box ((box m1))))))))) (box ((sharpurs_apply (box ((box Foreign_Object_toAscArray))) (box ((box m2))))))))))))) (Map.add "Eq0" (box ((box (fun (usd__unused: obj) -> (box eqObject1))))) Map.empty))))))))))

let Foreign_Object_eq1Object  = (sharpurs_apply (box ((box Data_Eq_Eq1usd_Dict))) (box ((box ((Map.add "eq1" (box ((box (fun (dictEq: obj) -> (sharpurs_apply (box ((box Data_Eq_eq))) (box ((sharpurs_apply (box ((box Foreign_Object_eqObject))) (box ((box dictEq))))))))))) Map.empty))))))

let Foreign_Object_delete  = (box (fun (k: obj) -> (sharpurs_apply (box ((box Foreign_Object_mutate))) (box ((sharpurs_apply (box ((box Foreign_Object_ST_delete))) (box ((box k)))))))))

let Foreign_Object_pop  = (box (fun (k: obj) -> (box (fun (m: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Functor_mapFlipped))) (box ((box Data_Maybe_functorMaybe)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Foreign_Object_lookup))) (box ((box k)))))) (box ((box m))))))))) (box ((box (fun (a: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Tuple_Tuple))) (box ((box a)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Foreign_Object_delete))) (box ((box k)))))) (box ((box m))))))))))))))))

let Foreign_Object_alter  = (box (fun (f: obj) -> (box (fun (k: obj) -> (box (fun (m: obj) -> (let v = (sharpurs_apply (box ((box f))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Foreign_Object_lookup))) (box ((box k)))))) (box ((box m))))))) in (match ((unbox ((box v)))) with | _ -> ((sharpurs_apply (box ((sharpurs_apply (box ((box Foreign_Object_delete))) (box ((box k)))))) (box ((box m))))) | v1 -> ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Foreign_Object_insert))) (box ((box k)))))) (box ((box v1)))))) (box ((box m)))))))))))))

let Foreign_Object_update  = (box (fun (f: obj) -> (box (fun (k: obj) -> (box (fun (m: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Foreign_Object_alter))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Maybe_maybe))) (box ((box Data_Maybe_Nothing)))))) (box ((box f))))))))) (box ((box k)))))) (box ((box m))))))))))
