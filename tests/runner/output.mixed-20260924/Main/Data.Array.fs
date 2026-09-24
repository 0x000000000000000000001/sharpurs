[<AutoOpen>]
module PureScript_Data_Array

open System
open System.Collections.Generic

module Data_Array_FFI =
    open System
    open System.Collections.Generic
    open System.Linq
    
    let rangeImpl =
        fun (startVal: obj) -> fun (endVal: obj) ->
            let start = startVal :?> int
            let endV = endVal :?> int
            let step = if start > endV then -1 else 1
            let size = (endV - start) * step + 1
            let result = Array.zeroCreate size
            let mutable i = start
            let mutable n = 0
            while i <> endV do
                result.[n] <- box i
                n <- n + 1
                i <- i + step
            result.[n] <- box i
            result :> obj
    
    let replicateImpl =
        fun (countVal: obj) -> fun (value: obj) ->
            let count = countVal :?> int
            if count < 1 then Array.empty<obj> :> obj
            else
                let result = Array.zeroCreate count
                for i = 0 to count - 1 do
                    result.[i] <- value
                result :> obj
    
    let length =
        fun (xs: obj) ->
            let arr = xs :?> obj[]
            arr.Length :> obj
    
    let unconsImpl =
        fun (empty: obj) -> fun (next: obj) -> fun (xs: obj) ->
            let arr = xs :?> obj[]
            if arr.Length = 0 then
                sharpurs_apply empty null
            else
                let head = arr.[0]
                let tail = Array.zeroCreate (arr.Length - 1)
                Array.Copy(arr, 1, tail, 0, arr.Length - 1)
                sharpurs_apply (sharpurs_apply next head) (tail :> obj)
    
    let indexImpl =
        fun (just: obj) -> fun (nothing: obj) -> fun (xs: obj) -> fun (iVal: obj) ->
            let arr = xs :?> obj[]
            let i = iVal :?> int
            if i < 0 || i >= arr.Length then nothing
            else
                sharpurs_apply just arr.[i]
    
    let _updateAt =
        fun (just: obj) -> fun (nothing: obj) -> fun (iVal: obj) -> fun (a: obj) -> fun (xs: obj) ->
            let arr = xs :?> obj[]
            let i = iVal :?> int
            if i < 0 || i >= arr.Length then nothing
            else
                let l1 = Array.zeroCreate arr.Length
                Array.Copy(arr, 0, l1, 0, arr.Length)
                l1.[i] <- a
                sharpurs_apply just (l1 :> obj)
    
    let _insertAt =
        fun (just: obj) -> fun (nothing: obj) -> fun (iVal: obj) -> fun (a: obj) -> fun (xs: obj) ->
            let arr = xs :?> obj[]
            let i = iVal :?> int
            if i < 0 || i > arr.Length then nothing
            else
                let l1 = Array.zeroCreate (arr.Length + 1)
                Array.Copy(arr, 0, l1, 0, i)
                l1.[i] <- a
                Array.Copy(arr, i, l1, i + 1, arr.Length - i)
                sharpurs_apply just (l1 :> obj)
    
    let _deleteAt =
        fun (just: obj) -> fun (nothing: obj) -> fun (iVal: obj) -> fun (xs: obj) ->
            let arr = xs :?> obj[]
            let i = iVal :?> int
            if i < 0 || i >= arr.Length then nothing
            else
                let l1 = Array.zeroCreate (arr.Length - 1)
                Array.Copy(arr, 0, l1, 0, i)
                Array.Copy(arr, i + 1, l1, i, arr.Length - i - 1)
                sharpurs_apply just (l1 :> obj)
    
    let reverse =
        fun (xs: obj) ->
            let arr = xs :?> obj[]
            let l1 = Array.zeroCreate arr.Length
            for i = 0 to arr.Length - 1 do
                l1.[i] <- arr.[arr.Length - 1 - i]
            l1 :> obj
    
    let concat =
        fun (xss: obj) ->
            let arrs = xss :?> obj[]
            let mutable totalLength = 0
            for xs in arrs do
                totalLength <- totalLength + (xs :?> obj[]).Length
            let result = Array.zeroCreate totalLength
            let mutable current = 0
            for xs in arrs do
                let xsArr = xs :?> obj[]
                Array.Copy(xsArr, 0, result, current, xsArr.Length)
                current <- current + xsArr.Length
            result :> obj
    
    let filterImpl =
        fun (f: obj) -> fun (xs: obj) ->
            let arr = xs :?> obj[]
            let res = ResizeArray<obj>()
            for x in arr do
                if unbox<bool> (sharpurs_apply f x) then res.Add(x)
            res.ToArray() :> obj
    
    let sliceImpl =
        fun (sVal: obj) -> fun (eVal: obj) -> fun (lVal: obj) ->
            let mutable s = sVal :?> int
            let mutable e = eVal :?> int
            let l = lVal :?> obj[]
            if s < 0 then s <- l.Length + s
            if e < 0 then e <- l.Length + e
            if s < 0 then s <- 0
            if e > l.Length then e <- l.Length
            if s > e then s <- e
            
            let res = Array.zeroCreate (e - s)
            Array.Copy(l, s, res, 0, e - s)
            res :> obj
    
    let zipWithImpl =
        fun (f: obj) -> fun (xs: obj) -> fun (ys: obj) ->
            let arrX = xs :?> obj[]
            let arrY = ys :?> obj[]
            let length = Math.Min(arrX.Length, arrY.Length)
            let result = Array.zeroCreate length
            for i = 0 to length - 1 do
                let step1 = sharpurs_apply f arrX.[i]
                result.[i] <- sharpurs_apply step1 arrY.[i]
            result :> obj
    
    let unsafeIndexImpl =
        fun (xs: obj) -> fun (n: obj) ->
            let arr = xs :?> obj[]
            let i = n :?> int
            arr.[i]
    
    let sortByImpl =
        fun (compare: obj) -> fun (fromOrdering: obj) -> fun (xs: obj) ->
            let arr = xs :?> obj[]
            if arr.Length < 2 then arr :> obj
            else
                let comparer =
                    { new IComparer<obj> with
                        member _.Compare(a: obj, b: obj) =
                            let step1 = sharpurs_apply compare a
                            let ord = sharpurs_apply step1 b
                            unbox<int> (sharpurs_apply fromOrdering ord) }
                let sorted = arr.OrderBy((fun x -> x), comparer).ToArray()
                sorted :> obj
    
    let scanrImpl =
        fun (f: obj) -> fun (b: obj) -> fun (xs: obj) ->
            let arr = xs :?> obj[]
            let outArr = Array.zeroCreate arr.Length
            let mutable acc = b
            for i = arr.Length - 1 downto 0 do
                let step1 = sharpurs_apply f arr.[i]
                acc <- sharpurs_apply step1 acc
                outArr.[i] <- acc
            outArr :> obj
    
    let scanlImpl =
        fun (f: obj) -> fun (b: obj) -> fun (xs: obj) ->
            let arr = xs :?> obj[]
            let outArr = Array.zeroCreate arr.Length
            let mutable acc = b
            for i = 0 to arr.Length - 1 do
                let step1 = sharpurs_apply f acc
                acc <- sharpurs_apply step1 arr.[i]
                outArr.[i] <- acc
            outArr :> obj
    
    let partitionImpl =
        fun (f: obj) -> fun (xs: obj) ->
            let arr = xs :?> obj[]
            let yes = ResizeArray<obj>()
            let no = ResizeArray<obj>()
            for x in arr do
                if unbox<bool> (sharpurs_apply f x) then yes.Add(x)
                else no.Add(x)
            let res = Map.empty<string, obj>
            let res = res.Add("yes", yes.ToArray() :> obj)
            let res = res.Add("no", no.ToArray() :> obj)
            box res
    
    type private ConsList =
        | Cons of obj * ConsList
        | EmptyList
    
    let fromFoldableImpl =
        fun (foldr: obj) -> fun (xsVal: obj) ->
            let cons = fun (head: obj) -> box (fun (tail: obj) -> box (Cons(head, unbox<ConsList> tail)))
            let listObj = Sharpurs_Prelude.sharpurs_apply (Sharpurs_Prelude.sharpurs_apply (Sharpurs_Prelude.sharpurs_apply foldr (box cons)) (box EmptyList)) xsVal
            let list = unbox<ConsList> listObj
            
            let rec countElements (l: ConsList) acc =
                match l with
                | EmptyList -> acc
                | Cons(_, tail) -> countElements tail (acc + 1)
            
            let size = countElements list 0
            let result = Array.zeroCreate size
            
            let rec fillArray (l: ConsList) i =
                match l with
                | EmptyList -> ()
                | Cons(head, tail) -> 
                    result.[i] <- head
                    fillArray tail (i + 1)
                    
            fillArray list 0
            result :> obj
    
    let findMapImpl =
        fun (nothing: obj) -> fun (isJust: obj) -> fun (f: obj) -> fun (xs: obj) ->
            let arr = xs :?> obj[]
            let mutable result = nothing
            let mutable i = 0
            while i < arr.Length && obj.ReferenceEquals(result, nothing) do
                let res = sharpurs_apply f arr.[i]
                if unbox<bool> (sharpurs_apply isJust res) then result <- res
                i <- i + 1
            result
    
    let findLastIndexImpl =
        fun (just: obj) -> fun (nothing: obj) -> fun (f: obj) -> fun (xs: obj) ->
            let arr = xs :?> obj[]
            let mutable result = nothing
            let mutable i = arr.Length - 1
            while i >= 0 && obj.ReferenceEquals(result, nothing) do
                if unbox<bool> (sharpurs_apply f arr.[i]) then
                    result <- sharpurs_apply just (box i)
                i <- i - 1
            result
    
    let findIndexImpl =
        fun (just: obj) -> fun (nothing: obj) -> fun (f: obj) -> fun (xs: obj) ->
            let arr = xs :?> obj[]
            let mutable result = nothing
            let mutable i = 0
            while i < arr.Length && obj.ReferenceEquals(result, nothing) do
                if unbox<bool> (sharpurs_apply f arr.[i]) then
                    result <- sharpurs_apply just (box i)
                i <- i + 1
            result
    
    let anyImpl =
        fun (p: obj) -> fun (xs: obj) ->
            let arr = xs :?> obj[]
            let mutable result = false
            let mutable i = 0
            while i < arr.Length && not result do
                if unbox<bool> (sharpurs_apply p arr.[i]) then result <- true
                i <- i + 1
            box result
    
    let allImpl =
        fun (p: obj) -> fun (xs: obj) ->
            let arr = xs :?> obj[]
            let mutable result = true
            let mutable i = 0
            while i < arr.Length && result do
                if not (unbox<bool> (sharpurs_apply p arr.[i])) then result <- false
                i <- i + 1
            box result
    

let Data_Array__deleteAt = box (Data_Array_FFI.``_deleteAt``)
let Data_Array__insertAt = box (Data_Array_FFI.``_insertAt``)
let Data_Array__updateAt = box (Data_Array_FFI.``_updateAt``)
let Data_Array_allImpl = box (Data_Array_FFI.``allImpl``)
let Data_Array_anyImpl = box (Data_Array_FFI.``anyImpl``)
let Data_Array_concat = box (Data_Array_FFI.``concat``)
let Data_Array_filterImpl = box (Data_Array_FFI.``filterImpl``)
let Data_Array_findIndexImpl = box (Data_Array_FFI.``findIndexImpl``)
let Data_Array_findLastIndexImpl = box (Data_Array_FFI.``findLastIndexImpl``)
let Data_Array_findMapImpl = box (Data_Array_FFI.``findMapImpl``)
let Data_Array_fromFoldableImpl = box (Data_Array_FFI.``fromFoldableImpl``)
let Data_Array_indexImpl = box (Data_Array_FFI.``indexImpl``)
let Data_Array_length = box (Data_Array_FFI.``length``)
let Data_Array_partitionImpl = box (Data_Array_FFI.``partitionImpl``)
let Data_Array_rangeImpl = box (Data_Array_FFI.``rangeImpl``)
let Data_Array_replicateImpl = box (Data_Array_FFI.``replicateImpl``)
let Data_Array_reverse = box (Data_Array_FFI.``reverse``)
let Data_Array_scanlImpl = box (Data_Array_FFI.``scanlImpl``)
let Data_Array_scanrImpl = box (Data_Array_FFI.``scanrImpl``)
let Data_Array_sliceImpl = box (Data_Array_FFI.``sliceImpl``)
let Data_Array_sortByImpl = box (Data_Array_FFI.``sortByImpl``)
let Data_Array_unconsImpl = box (Data_Array_FFI.``unconsImpl``)
let Data_Array_unsafeIndexImpl = box (Data_Array_FFI.``unsafeIndexImpl``)
let Data_Array_zipWithImpl = box (Data_Array_FFI.``zipWithImpl``)


let Data_Array_intercalate1  = (sharpurs_apply (box ((box Data_Foldable_intercalate))) (box ((box Data_Foldable_foldableArray))))

let Data_Array_zero  = (sharpurs_apply (box ((box Data_Semiring_zero))) (box ((box Data_Semiring_semiringInt))))

let Data_Array_one  = (sharpurs_apply (box ((box Data_Semiring_one))) (box ((box Data_Semiring_semiringInt))))

let Data_Array_void  = (sharpurs_apply (box ((box Data_Functor_void))) (box ((box Control_Monad_ST_Internal_functorST))))

let Data_Array_pure  = (sharpurs_apply (box ((box Control_Applicative_pure))) (box ((box Control_Monad_ST_Internal_applicativeST))))

let Data_Array_fromJust  = (sharpurs_apply (box ((box Data_Maybe_fromJust))) (box ((box Prim_undefined))))

let Data_Array_void1  = (sharpurs_apply (box ((box Data_Functor_void))) (box ((box Control_Monad_ST_Internal_functorST))))

let Data_Array_foldMap1  = (sharpurs_apply (box ((box Data_Foldable_foldMap))) (box ((box Data_Foldable_foldableArray))))

let Data_Array_fold1  = (sharpurs_apply (box ((box Data_Foldable_fold))) (box ((box Data_Foldable_foldableArray))))

let Data_Array_not  = (sharpurs_apply (box ((box Data_HeytingAlgebra_not))) (box ((box Data_HeytingAlgebra_heytingAlgebraBoolean))))

let Data_Array_void2  = (sharpurs_apply (box ((box Data_Functor_void))) (box ((box Control_Monad_ST_Internal_functorST))))

let Data_Array_zipWith  = (sharpurs_apply (box ((box Data_Function_Uncurried_runFn3))) (box ((box Data_Array_zipWithImpl))))

let Data_Array_zipWithA  = (box (fun (dictApplicative: obj) -> (box (fun (f: obj) -> (box (fun (xs: obj) -> (box (fun (ys: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Traversable_sequence))) (box ((box Data_Traversable_traversableArray)))))) (box ((box dictApplicative)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_zipWith))) (box ((box f)))))) (box ((box xs)))))) (box ((box ys)))))))))))))))

let Data_Array_zip  = (sharpurs_apply (box ((box Data_Array_zipWith))) (box ((box Data_Tuple_Tuple))))

let Data_Array_updateAtIndices  = (box (fun (dictFoldable: obj) -> (box (fun (us: obj) -> (box (fun (xs: obj) -> (sharpurs_apply (box ((box Control_Monad_ST_Internal_run))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_ST_withArray))) (box ((box (fun (res: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Foldable_traverse_))) (box ((box Control_Monad_ST_Internal_applicativeST)))))) (box ((box dictFoldable)))))) (box ((box (fun (v: obj) -> (match ((unbox ((box v)))) with | i -> ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_ST_poke))) (box ((box i)))))) (box ((box a)))))) (box ((box res))))))))))))) (box ((box us))))))))))) (box ((box xs)))))))))))))

let Data_Array_updateAt  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_Uncurried_runFn5))) (box ((box Data_Array__updateAt)))))) (box ((box Data_Maybe_Just)))))) (box ((box Data_Maybe_Nothing))))

let Data_Array_unsafeIndex  = (box (fun (usd__unused: obj) -> (sharpurs_apply (box ((box Data_Function_Uncurried_runFn2))) (box ((box Data_Array_unsafeIndexImpl))))))

let Data_Array_uncons  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_Uncurried_runFn3))) (box ((box Data_Array_unconsImpl)))))) (box ((sharpurs_apply (box ((box Data_Function_const))) (box ((box Data_Maybe_Nothing))))))))) (box ((box (fun (x: obj) -> (box (fun (xs: obj) -> (sharpurs_apply (box ((box Data_Maybe_Just))) (box ((box ((Map.add "head" (box ((box x))) (Map.add "tail" (box ((box xs))) Map.empty))))))))))))))

let Data_Array_toUnfoldable  = (box (fun (dictUnfoldable: obj) -> (box (fun (xs: obj) -> (let len = (sharpurs_apply (box ((box Data_Array_length))) (box ((box xs)))) in let f = (box (fun (i: obj) -> (match ((unbox ((box i)))) with | i1 when (unbox (box ((unbox<int> (box ((box i1)))) < (unbox<int> (box ((box len))))))) -> ((sharpurs_apply (box ((box Data_Maybe_Just))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Tuple_Tuple))) (box ((sharpurs_apply (box ((box Partial_Unsafe_unsafePartial))) (box ((box (fun (usd__unused: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_unsafeIndex))) (box ((box Prim_undefined)))))) (box ((box xs)))))) (box ((box i1)))))))))))))) (box ((box ((unbox<int> (box ((box i1)))) + (unbox<int> (box ((box 1))))))))))))) | i1 when (unbox (box Data_Boolean_otherwise)) -> ((box Data_Maybe_Nothing))))) in (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Unfoldable_unfoldr))) (box ((box dictUnfoldable)))))) (box ((box f)))))) (box ((box 0)))))))))

let Data_Array_tail  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_Uncurried_runFn3))) (box ((box Data_Array_unconsImpl)))))) (box ((sharpurs_apply (box ((box Data_Function_const))) (box ((box Data_Maybe_Nothing))))))))) (box ((box (fun (v: obj) -> (box (fun (xs: obj) -> (sharpurs_apply (box ((box Data_Maybe_Just))) (box ((box xs)))))))))))

let Data_Array_sortBy  = (box (fun (comp: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_Uncurried_runFn3))) (box ((box Data_Array_sortByImpl)))))) (box ((box comp)))))) (box ((box (fun (v: obj) -> (match ((unbox ((box v)))) with | _ -> ((box 1)) | _ -> ((box 0)) | _ -> ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Ring_negate))) (box ((box Data_Ring_ringInt)))))) (box ((box 1)))))))))))))

let Data_Array_sortWith  = (box (fun (dictOrd: obj) -> (box (fun (f: obj) -> (sharpurs_apply (box ((box Data_Array_sortBy))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Ord_comparing))) (box ((box dictOrd)))))) (box ((box f)))))))))))

let Data_Array_sort  = (box (fun (dictOrd: obj) -> (let compare = (sharpurs_apply (box ((box Data_Ord_compare))) (box ((box dictOrd)))) in (box (fun (xs: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_sortBy))) (box ((box compare)))))) (box ((box xs)))))))))

let Data_Array_snoc  = (box (fun (xs: obj) -> (box (fun (x: obj) -> (sharpurs_apply (box ((box Control_Monad_ST_Internal_run))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_ST_withArray))) (box ((sharpurs_apply (box ((box Data_Array_ST_push))) (box ((box x))))))))) (box ((box xs)))))))))))

let Data_Array_slice  = (sharpurs_apply (box ((box Data_Function_Uncurried_runFn3))) (box ((box Data_Array_sliceImpl))))

let Data_Array_splitAt  = (box (fun (v: obj) -> (box (fun (v1: obj) -> (match (((unbox ((box v))), (unbox ((box v1))))) with | (i, xs) when (unbox (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Ord_lessThanOrEq))) (box ((box Data_Ord_ordInt)))))) (box ((box i)))))) (box ((box 0))))) -> ((box ((Map.add "before" (box ((box [||]))) (Map.add "after" (box ((box xs))) Map.empty))))) | (i, xs) -> ((box ((Map.add "before" (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_slice))) (box ((box 0)))))) (box ((box i)))))) (box ((box xs)))))) (Map.add "after" (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_slice))) (box ((box i)))))) (box ((sharpurs_apply (box ((box Data_Array_length))) (box ((box xs))))))))) (box ((box xs)))))) Map.empty))))))))))

let Data_Array_take  = (box (fun (n: obj) -> (box (fun (xs: obj) -> (match ((unbox ((box ((unbox<int> (box ((box n)))) < (unbox<int> (box ((box 1))))))))) with | LitBool true () -> ((box [||])) | _ -> ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_slice))) (box ((box 0)))))) (box ((box n)))))) (box ((box xs))))))))))

let Data_Array_singleton  = (box (fun (a: obj) -> (box [|(box a)|])))

let Data_Array_scanr  = (sharpurs_apply (box ((box Data_Function_Uncurried_runFn3))) (box ((box Data_Array_scanrImpl))))

let Data_Array_scanl  = (sharpurs_apply (box ((box Data_Function_Uncurried_runFn3))) (box ((box Data_Array_scanlImpl))))

let Data_Array_replicate  = (sharpurs_apply (box ((box Data_Function_Uncurried_runFn2))) (box ((box Data_Array_replicateImpl))))

let Data_Array_range  = (sharpurs_apply (box ((box Data_Function_Uncurried_runFn2))) (box ((box Data_Array_rangeImpl))))

let Data_Array_partition  = (sharpurs_apply (box ((box Data_Function_Uncurried_runFn2))) (box ((box Data_Array_partitionImpl))))

let Data_Array_null  = (box (fun (xs: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Eq_eq))) (box ((box Data_Eq_eqInt)))))) (box ((sharpurs_apply (box ((box Data_Array_length))) (box ((box xs))))))))) (box ((box 0))))))

let Data_Array_modifyAtIndices  = (box (fun (dictFoldable: obj) -> (box (fun (is: obj) -> (box (fun (f: obj) -> (box (fun (xs: obj) -> (sharpurs_apply (box ((box Control_Monad_ST_Internal_run))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_ST_withArray))) (box ((box (fun (res: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Foldable_traverse_))) (box ((box Control_Monad_ST_Internal_applicativeST)))))) (box ((box dictFoldable)))))) (box ((box (fun (i: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_ST_modify))) (box ((box i)))))) (box ((box f)))))) (box ((box res))))))))))) (box ((box is))))))))))) (box ((box xs)))))))))))))))

let Data_Array_mapWithIndex  = (sharpurs_apply (box ((box Data_FunctorWithIndex_mapWithIndex))) (box ((box Data_FunctorWithIndex_functorWithIndexArray))))

let Data_Array_intersperse  = (box (fun (a: obj) -> (box (fun (arr: obj) -> (let v = (sharpurs_apply (box ((box Data_Array_length))) (box ((box arr)))) in (match ((unbox ((box v)))) with | len when (unbox (box ((unbox<int> (box ((box len)))) < (unbox<int> (box ((box 2))))))) -> ((box arr)) | len when (unbox (box Data_Boolean_otherwise)) -> ((sharpurs_apply (box ((box Data_Array_ST_run))) (box ((let unsafeGetElem = (box (fun (idx: obj) -> (sharpurs_apply (box ((box Partial_Unsafe_unsafePartial))) (box ((box (fun (usd__unused: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_unsafeIndex))) (box ((box Prim_undefined)))))) (box ((box arr)))))) (box ((box idx))))))))))) in (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_bind))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((box Data_Array_ST_new)))))) (box ((box (fun (out: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_bind))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_ST_push))) (box ((sharpurs_apply (box ((box unsafeGetElem))) (box ((box 0))))))))) (box ((box out))))))))) (box ((box (fun (usd__unused: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_discard))) (box ((box Control_Bind_discardUnit)))))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Monad_ST_Internal_for))) (box ((box 1)))))) (box ((box len)))))) (box ((box (fun (idx: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_bind))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_ST_push))) (box ((box a)))))) (box ((box out))))))))) (box ((box (fun (usd__unused: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Functor_void))) (box ((box Control_Monad_ST_Internal_functorST)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_ST_push))) (box ((sharpurs_apply (box ((box unsafeGetElem))) (box ((box idx))))))))) (box ((box out)))))))))))))))))))))) (box ((box (fun (usd__unused: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Control_Applicative_pure))) (box ((box Control_Monad_ST_Internal_applicativeST)))))) (box ((box out))))))))))))))))))))))))))))))

let Data_Array_intercalate  = (box (fun (dictMonoid: obj) -> (sharpurs_apply (box ((box Data_Array_intercalate1))) (box ((box dictMonoid))))))

let Data_Array_insertAt  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_Uncurried_runFn5))) (box ((box Data_Array__insertAt)))))) (box ((box Data_Maybe_Just)))))) (box ((box Data_Maybe_Nothing))))

let Data_Array_init  = (box (fun (xs: obj) -> (match ((unbox ((box xs)))) with | xs1 when (unbox (sharpurs_apply (box ((box Data_Array_null))) (box ((box xs1))))) -> ((box Data_Maybe_Nothing)) | xs1 when (unbox (box Data_Boolean_otherwise)) -> ((sharpurs_apply (box ((box Data_Maybe_Just))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_slice))) (box ((box Data_Array_zero)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Ring_sub))) (box ((box Data_Ring_ringInt)))))) (box ((sharpurs_apply (box ((box Data_Array_length))) (box ((box xs1))))))))) (box ((box Data_Array_one))))))))) (box ((box xs1)))))))))))

let Data_Array_index  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_Uncurried_runFn4))) (box ((box Data_Array_indexImpl)))))) (box ((box Data_Maybe_Just)))))) (box ((box Data_Maybe_Nothing))))

let Data_Array_last  = (box (fun (xs: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_index))) (box ((box xs)))))) (box ((box ((unbox<int> (box ((sharpurs_apply (box ((box Data_Array_length))) (box ((box xs))))))) - (unbox<int> (box ((box 1)))))))))))

let Data_Array_unsnoc  = (box (fun (xs: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Apply_apply))) (box ((box Data_Maybe_applyMaybe)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Functor_map))) (box ((box Data_Maybe_functorMaybe)))))) (box ((box (fun (v: obj) -> (box (fun (v1: obj) -> (box ((Map.add "init" (box ((box v))) (Map.add "last" (box ((box v1))) Map.empty))))))))))))) (box ((sharpurs_apply (box ((box Data_Array_init))) (box ((box xs)))))))))))) (box ((sharpurs_apply (box ((box Data_Array_last))) (box ((box xs)))))))))

let Data_Array_modifyAt  = (box (fun (i: obj) -> (box (fun (f: obj) -> (box (fun (xs: obj) -> (let go = (box (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_updateAt))) (box ((box i)))))) (box ((sharpurs_apply (box ((box f))) (box ((box x))))))))) (box ((box xs)))))) in (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Maybe_maybe))) (box ((box Data_Maybe_Nothing)))))) (box ((box go)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_index))) (box ((box xs)))))) (box ((box i))))))))))))))

let Data_Array_unzip  = (box (fun (xs: obj) -> (sharpurs_apply (box ((box Control_Monad_ST_Internal_run))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_bind))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((box Data_Array_ST_new)))))) (box ((box (fun (fsts: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_bind))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((box Data_Array_ST_new)))))) (box ((box (fun (snds: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_bind))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((sharpurs_apply (box ((box Data_Array_ST_Iterator_iterator))) (box ((box (fun (v: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_index))) (box ((box xs)))))) (box ((box v)))))))))))))) (box ((box (fun (iter: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_discard))) (box ((box Control_Bind_discardUnit)))))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_ST_Iterator_iterate))) (box ((box iter)))))) (box ((box (fun (v: obj) -> (match ((unbox ((box v)))) with | fst -> ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_discard))) (box ((box Control_Bind_discardUnit)))))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_void)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_ST_push))) (box ((box fst)))))) (box ((box fsts)))))))))))) (box ((box (fun (usd__unused: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_void)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_ST_push))) (box ((box snd)))))) (box ((box snds)))))))))))))))))))))))) (box ((box (fun (usd__unused: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_bind))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((sharpurs_apply (box ((box Data_Array_ST_unsafeFreeze))) (box ((box fsts))))))))) (box ((box (fun (fsts_prime: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_bind))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((sharpurs_apply (box ((box Data_Array_ST_unsafeFreeze))) (box ((box snds))))))))) (box ((box (fun (snds_prime: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_pure)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Tuple_Tuple))) (box ((box fsts_prime)))))) (box ((box snds_prime))))))))))))))))))))))))))))))))))))))))))

let Data_Array_head  = (box (fun (xs: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_index))) (box ((box xs)))))) (box ((box 0))))))

let Data_Array_nubBy  = (box (fun (comp: obj) -> (box (fun (xs: obj) -> (let indexedAndSorted = (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_sortBy))) (box ((box (fun (x: obj) -> (box (fun (y: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box comp))) (box ((sharpurs_apply (box ((box Data_Tuple_snd))) (box ((box x))))))))) (box ((sharpurs_apply (box ((box Data_Tuple_snd))) (box ((box y)))))))))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_mapWithIndex))) (box ((box Data_Tuple_Tuple)))))) (box ((box xs))))))) in (let v = (sharpurs_apply (box ((box Data_Array_head))) (box ((box indexedAndSorted)))) in (match ((unbox ((box v)))) with | _ -> ((box [||])) | x -> ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Functor_map))) (box ((box Data_Functor_functorArray)))))) (box ((box Data_Tuple_snd))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_sortWith))) (box ((box Data_Ord_ordInt)))))) (box ((box Data_Tuple_fst))))))))) (box ((sharpurs_apply (box ((box Control_Monad_ST_Internal_run))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_bind))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_ST_unsafeThaw)))))) (box ((sharpurs_apply (box ((box Data_Array_singleton))) (box ((box x)))))))))))) (box ((box (fun (result: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_discard))) (box ((box Control_Bind_discardUnit)))))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Monad_ST_Internal_foreach))) (box ((box indexedAndSorted)))))) (box ((box (fun (v1: obj) -> (match ((unbox ((box v1)))) with | (_ as pair) -> ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_bind))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Functor_map))) (box ((box Control_Monad_ST_Internal_functorST)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_Tuple_snd)))))) (box ((sharpurs_apply (box ((box Partial_Unsafe_unsafePartial))) (box ((box (fun (usd__unused: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_Array_fromJust)))))) (box ((box Data_Array_last))))))))))))))))) (box ((sharpurs_apply (box ((box Data_Array_ST_unsafeFreeze))) (box ((box result)))))))))))) (box ((box (fun (lst: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Applicative_when))) (box ((box Control_Monad_ST_Internal_applicativeST)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Eq_notEq))) (box ((box Data_Ordering_eqOrdering)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box comp))) (box ((box lst)))))) (box ((box x_prime))))))))) (box ((box Data_Ordering_EQ)))))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_void1)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_ST_push))) (box ((box pair)))))) (box ((box result))))))))))))))))))))))))))) (box ((box (fun (usd__unused: obj) -> (sharpurs_apply (box ((box Data_Array_ST_unsafeFreeze))) (box ((box result)))))))))))))))))))))))))))))))

let Data_Array_nub  = (box (fun (dictOrd: obj) -> (sharpurs_apply (box ((box Data_Array_nubBy))) (box ((sharpurs_apply (box ((box Data_Ord_compare))) (box ((box dictOrd)))))))))

let Data_Array_groupBy  = (box (fun (op: obj) -> (box (fun (xs: obj) -> (sharpurs_apply (box ((box Control_Monad_ST_Internal_run))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_bind))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((box Data_Array_ST_new)))))) (box ((box (fun (result: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_bind))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((sharpurs_apply (box ((box Data_Array_ST_Iterator_iterator))) (box ((box (fun (v: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_index))) (box ((box xs)))))) (box ((box v)))))))))))))) (box ((box (fun (iter: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_discard))) (box ((box Control_Bind_discardUnit)))))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_ST_Iterator_iterate))) (box ((box iter)))))) (box ((box (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Functor_void))) (box ((box Control_Monad_ST_Internal_functorST)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_bind))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((box Data_Array_ST_new)))))) (box ((box (fun (sub: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_bind))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_ST_push))) (box ((box x)))))) (box ((box sub))))))))) (box ((box (fun (usd__unused: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_discard))) (box ((box Control_Bind_discardUnit)))))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_ST_Iterator_pushWhile))) (box ((sharpurs_apply (box ((box op))) (box ((box x))))))))) (box ((box iter)))))) (box ((box sub))))))))) (box ((box (fun (usd__unused: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_bind))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((sharpurs_apply (box ((box Data_Array_ST_unsafeFreeze))) (box ((box sub))))))))) (box ((box (fun (grp: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_ST_push))) (box ((sharpurs_apply (box ((box Data_Array_NonEmpty_Internal_NonEmptyArray))) (box ((box grp))))))))) (box ((box result))))))))))))))))))))))))))))))))))))) (box ((box (fun (usd__unused: obj) -> (sharpurs_apply (box ((box Data_Array_ST_unsafeFreeze))) (box ((box result))))))))))))))))))))))))))

let Data_Array_groupAllBy  = (box (fun (cmp: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((sharpurs_apply (box ((box Data_Array_groupBy))) (box ((box (fun (x: obj) -> (box (fun (y: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Eq_eq))) (box ((box Data_Ordering_eqOrdering)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box cmp))) (box ((box x)))))) (box ((box y))))))))) (box ((box Data_Ordering_EQ)))))))))))))))) (box ((sharpurs_apply (box ((box Data_Array_sortBy))) (box ((box cmp)))))))))

let Data_Array_groupAll  = (box (fun (dictOrd: obj) -> (sharpurs_apply (box ((box Data_Array_groupAllBy))) (box ((sharpurs_apply (box ((box Data_Ord_compare))) (box ((box dictOrd)))))))))

let Data_Array_group  = (box (fun (dictEq: obj) -> (let eq = (sharpurs_apply (box ((box Data_Eq_eq))) (box ((box dictEq)))) in (box (fun (xs: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_groupBy))) (box ((box eq)))))) (box ((box xs)))))))))

let Data_Array_fromFoldable  = (box (fun (dictFoldable: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_Uncurried_runFn2))) (box ((box Data_Array_fromFoldableImpl)))))) (box ((sharpurs_apply (box ((box Data_Foldable_foldr))) (box ((box dictFoldable)))))))))

let Data_Array_foldr  = (sharpurs_apply (box ((box Data_Foldable_foldr))) (box ((box Data_Foldable_foldableArray))))

let Data_Array_foldl  = (sharpurs_apply (box ((box Data_Foldable_foldl))) (box ((box Data_Foldable_foldableArray))))

let Data_Array_transpose  = (box (fun (xs: obj) -> (let buildNext = (box (fun (idx: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_applyFlipped))) (box ((box xs)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_flip))) (box ((box Data_Array_foldl)))))) (box ((box Data_Maybe_Nothing)))))) (box ((box (fun (acc: obj) -> (box (fun (nextArr: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Maybe_maybe))) (box ((box acc)))))) (box ((box (fun (el: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Maybe_Just)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Maybe_maybe))) (box ((box [|(box el)|])))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_flip))) (box ((box Data_Array_snoc)))))) (box ((box el))))))))) (box ((box acc))))))))))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_index))) (box ((box nextArr)))))) (box ((box idx))))))))))))))))))) in 
                                                                                                                                                                                                        
                                                                                                                                                                                                        let rec go_tco (idx: obj) (allArrays: obj) : obj = ((let v = (sharpurs_apply (box ((box buildNext))) (box ((box idx)))) in (match ((unbox ((box v)))) with | _ -> ((box allArrays)) | next -> ((go_tco ((box ((unbox<int> (box ((box idx)))) + (unbox<int> (box ((box 1))))))) ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_snoc))) (box ((box allArrays)))))) (box ((box next)))))))))) 
                                                                                                                                                                                                        and go = box ((fun (idx: obj) -> (fun (allArrays: obj) -> go_tco idx allArrays))) 
                                                                                                                                                                                                        in
                                                                                                                                                                                                        (go_tco ((box 0)) ((box [||]))))))

let Data_Array_foldRecM  = (box (fun (dictMonadRec: obj) -> (let Monad0 = (sharpurs_apply (box ((Map.find "Monad0" (unbox<Map<string, obj>> ((box dictMonadRec)))))) (box ((box Prim_undefined)))) in let Applicative0 = (sharpurs_apply (box ((Map.find "Applicative0" (unbox<Map<string, obj>> ((box Monad0)))))) (box ((box Prim_undefined)))) in let Bind1 = (sharpurs_apply (box ((Map.find "Bind1" (unbox<Map<string, obj>> ((box Monad0)))))) (box ((box Prim_undefined)))) in (box (fun (f: obj) -> (box (fun (b: obj) -> (box (fun (array: obj) -> (let go = (box (fun (res: obj) -> (box (fun (i: obj) -> (match (((unbox ((box res))), (unbox ((box i))))) with | (res1, i1) when (unbox (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Ord_greaterThanOrEq))) (box ((box Data_Ord_ordInt)))))) (box ((box i1)))))) (box ((sharpurs_apply (box ((box Data_Array_length))) (box ((box array)))))))) -> ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Applicative_pure))) (box ((box Applicative0)))))) (box ((sharpurs_apply (box ((box Control_Monad_Rec_Class_Done))) (box ((box res1)))))))) | (res1, i1) when (unbox (box Data_Boolean_otherwise)) -> ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_bind))) (box ((box Bind1)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box f))) (box ((box res1)))))) (box ((sharpurs_apply (box ((box Partial_Unsafe_unsafePartial))) (box ((box (fun (usd__unused: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_unsafeIndex))) (box ((box Prim_undefined)))))) (box ((box array)))))) (box ((box i1))))))))))))))))) (box ((box (fun (res_prime: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Control_Applicative_pure))) (box ((box Applicative0)))))) (box ((sharpurs_apply (box ((box Control_Monad_Rec_Class_Loop))) (box ((box ((Map.add "a" (box ((box res_prime))) (Map.add "b" (box ((box ((unbox<int> (box ((box i1)))) + (unbox<int> (box ((box 1)))))))) Map.empty))))))))))))))))))))) in (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Monad_Rec_Class_tailRecM2))) (box ((box dictMonadRec)))))) (box ((box go)))))) (box ((box b)))))) (box ((box 0))))))))))))))

let Data_Array_foldMap  = (box (fun (dictMonoid: obj) -> (sharpurs_apply (box ((box Data_Array_foldMap1))) (box ((box dictMonoid))))))

let rec Data_Array_foldM_tco (dictMonad: obj) : obj = ((let Applicative0 = (sharpurs_apply (box ((Map.find "Applicative0" (unbox<Map<string, obj>> ((box dictMonad)))))) (box ((box Prim_undefined)))) in let Bind1 = (sharpurs_apply (box ((Map.find "Bind1" (unbox<Map<string, obj>> ((box dictMonad)))))) (box ((box Prim_undefined)))) in (box (fun (f: obj) -> (box (fun (b: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_Uncurried_runFn3))) (box ((box Data_Array_unconsImpl)))))) (box ((box (fun (v: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Control_Applicative_pure))) (box ((box Applicative0)))))) (box ((box b))))))))))) (box ((box (fun (a: obj) -> (box (fun (as_var: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_bind))) (box ((box Bind1)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box f))) (box ((box b)))))) (box ((box a))))))))) (box ((box (fun (b_prime: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_foldM))) (box ((box dictMonad)))))) (box ((box f)))))) (box ((box b_prime)))))) (box ((box as_var))))))))))))))))))))))
and Data_Array_foldM = box (fun (dictMonad: obj) -> Data_Array_foldM_tco dictMonad)


let Data_Array_fold  = (box (fun (dictMonoid: obj) -> (sharpurs_apply (box ((box Data_Array_fold1))) (box ((box dictMonoid))))))

let Data_Array_findMap  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_Uncurried_runFn4))) (box ((box Data_Array_findMapImpl)))))) (box ((box Data_Maybe_Nothing)))))) (box ((box Data_Maybe_isJust))))

let Data_Array_findLastIndex  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_Uncurried_runFn4))) (box ((box Data_Array_findLastIndexImpl)))))) (box ((box Data_Maybe_Just)))))) (box ((box Data_Maybe_Nothing))))

let Data_Array_insertBy  = (box (fun (cmp: obj) -> (box (fun (x: obj) -> (box (fun (ys: obj) -> (let i = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Maybe_maybe))) (box ((box 0)))))) (box ((box (fun (v: obj) -> (box ((unbox<int> (box ((box v)))) + (unbox<int> (box ((box 1))))))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_findLastIndex))) (box ((box (fun (y: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Eq_eq))) (box ((box Data_Ordering_eqOrdering)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box cmp))) (box ((box x)))))) (box ((box y))))))))) (box ((box Data_Ordering_GT))))))))))) (box ((box ys))))))) in (sharpurs_apply (box ((box Partial_Unsafe_unsafePartial))) (box ((box (fun (usd__unused: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Maybe_fromJust))) (box ((box Prim_undefined)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_insertAt))) (box ((box i)))))) (box ((box x)))))) (box ((box ys)))))))))))))))))))

let Data_Array_insert  = (box (fun (dictOrd: obj) -> (sharpurs_apply (box ((box Data_Array_insertBy))) (box ((sharpurs_apply (box ((box Data_Ord_compare))) (box ((box dictOrd)))))))))

let Data_Array_findIndex  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_Uncurried_runFn4))) (box ((box Data_Array_findIndexImpl)))))) (box ((box Data_Maybe_Just)))))) (box ((box Data_Maybe_Nothing))))

let Data_Array_span  = (box (fun (p: obj) -> (box (fun (arr: obj) -> (let v = (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_findIndex))) (box ((box (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_HeytingAlgebra_not))) (box ((box Data_HeytingAlgebra_heytingAlgebraBoolean)))))) (box ((sharpurs_apply (box ((box p))) (box ((box x)))))))))))))) (box ((box arr)))) in (match ((unbox ((box v)))) with | LitInt 0 () -> ((box ((Map.add "init" (box ((box [||]))) (Map.add "rest" (box ((box arr))) Map.empty))))) | i -> ((box ((Map.add "init" (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_slice))) (box ((box 0)))))) (box ((box i)))))) (box ((box arr)))))) (Map.add "rest" (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_slice))) (box ((box i)))))) (box ((sharpurs_apply (box ((box Data_Array_length))) (box ((box arr))))))))) (box ((box arr)))))) Map.empty))))) | _ -> ((box ((Map.add "init" (box ((box arr))) (Map.add "rest" (box ((box [||]))) Map.empty)))))))))))

let Data_Array_takeWhile  = (box (fun (p: obj) -> (box (fun (xs: obj) -> (Map.find "init" (unbox<Map<string, obj>> ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_span))) (box ((box p)))))) (box ((box xs)))))))))))

let Data_Array_find  = (box (fun (f: obj) -> (box (fun (xs: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Functor_map))) (box ((box Data_Maybe_functorMaybe)))))) (box ((sharpurs_apply (box ((box Partial_Unsafe_unsafePartial))) (box ((box (fun (usd__unused: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_unsafeIndex))) (box ((box Prim_undefined)))))) (box ((box xs)))))))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_findIndex))) (box ((box f)))))) (box ((box xs)))))))))))

let Data_Array_filter  = (sharpurs_apply (box ((box Data_Function_Uncurried_runFn2))) (box ((box Data_Array_filterImpl))))

let Data_Array_intersectBy  = (box (fun (eq: obj) -> (box (fun (xs: obj) -> (box (fun (ys: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_filter))) (box ((box (fun (x: obj) -> (sharpurs_apply (box ((box Data_Maybe_isJust))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_findIndex))) (box ((sharpurs_apply (box ((box eq))) (box ((box x))))))))) (box ((box ys)))))))))))))) (box ((box xs))))))))))

let Data_Array_intersect  = (box (fun (dictEq: obj) -> (sharpurs_apply (box ((box Data_Array_intersectBy))) (box ((sharpurs_apply (box ((box Data_Eq_eq))) (box ((box dictEq)))))))))

let Data_Array_elemLastIndex  = (box (fun (dictEq: obj) -> (box (fun (x: obj) -> (sharpurs_apply (box ((box Data_Array_findLastIndex))) (box ((box (fun (v: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Eq_eq))) (box ((box dictEq)))))) (box ((box v)))))) (box ((box x)))))))))))))

let Data_Array_elemIndex  = (box (fun (dictEq: obj) -> (box (fun (x: obj) -> (sharpurs_apply (box ((box Data_Array_findIndex))) (box ((box (fun (v: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Eq_eq))) (box ((box dictEq)))))) (box ((box v)))))) (box ((box x)))))))))))))

let Data_Array_notElem  = (box (fun (dictEq: obj) -> (box (fun (a: obj) -> (box (fun (arr: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Maybe_isNothing)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_elemIndex))) (box ((box dictEq)))))) (box ((box a)))))) (box ((box arr)))))))))))))

let Data_Array_elem  = (box (fun (dictEq: obj) -> (box (fun (a: obj) -> (box (fun (arr: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Maybe_isJust)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_elemIndex))) (box ((box dictEq)))))) (box ((box a)))))) (box ((box arr)))))))))))))

let Data_Array_dropWhile  = (box (fun (p: obj) -> (box (fun (xs: obj) -> (Map.find "rest" (unbox<Map<string, obj>> ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_span))) (box ((box p)))))) (box ((box xs)))))))))))

let Data_Array_dropEnd  = (box (fun (n: obj) -> (box (fun (xs: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_take))) (box ((box ((unbox<int> (box ((sharpurs_apply (box ((box Data_Array_length))) (box ((box xs))))))) - (unbox<int> (box ((box n))))))))))) (box ((box xs))))))))

let Data_Array_drop  = (box (fun (n: obj) -> (box (fun (xs: obj) -> (match ((unbox ((box ((unbox<int> (box ((box n)))) < (unbox<int> (box ((box 1))))))))) with | LitBool true () -> ((box xs)) | _ -> ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_slice))) (box ((box n)))))) (box ((sharpurs_apply (box ((box Data_Array_length))) (box ((box xs))))))))) (box ((box xs))))))))))

let Data_Array_takeEnd  = (box (fun (n: obj) -> (box (fun (xs: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_drop))) (box ((box ((unbox<int> (box ((sharpurs_apply (box ((box Data_Array_length))) (box ((box xs))))))) - (unbox<int> (box ((box n))))))))))) (box ((box xs))))))))

let Data_Array_deleteAt  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_Uncurried_runFn4))) (box ((box Data_Array__deleteAt)))))) (box ((box Data_Maybe_Just)))))) (box ((box Data_Maybe_Nothing))))

let Data_Array_deleteBy  = (box (fun (v: obj) -> (box (fun (v1: obj) -> (box (fun (v2: obj) -> (match (((unbox ((box v))), (unbox ((box v1))), (unbox ((box v2))))) with | (_, _, [|  |]) -> ((box [||])) | (eq, x, ys) -> ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Maybe_maybe))) (box ((box ys)))))) (box ((box (fun (i: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Partial_Unsafe_unsafePartial)))))) (box ((box (fun (usd__unused: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Maybe_fromJust))) (box ((box Prim_undefined)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_deleteAt))) (box ((box i)))))) (box ((box ys))))))))))))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_findIndex))) (box ((sharpurs_apply (box ((box eq))) (box ((box x))))))))) (box ((box ys)))))))))))))))

let Data_Array_delete  = (box (fun (dictEq: obj) -> (sharpurs_apply (box ((box Data_Array_deleteBy))) (box ((sharpurs_apply (box ((box Data_Eq_eq))) (box ((box dictEq)))))))))

let Data_Array_difference  = (box (fun (dictEq: obj) -> (sharpurs_apply (box ((box Data_Array_foldr))) (box ((sharpurs_apply (box ((box Data_Array_delete))) (box ((box dictEq)))))))))

let Data_Array_cons  = (box (fun (x: obj) -> (box (fun (xs: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semigroup_append))) (box ((box Data_Semigroup_semigroupArray)))))) (box ((box [|(box x)|])))))) (box ((box xs))))))))

let rec Data_Array_some_tco (dictAlternative: obj) : obj = ((let Apply0 = (sharpurs_apply (box ((Map.find "Apply0" (unbox<Map<string, obj>> ((sharpurs_apply (box ((Map.find "Applicative0" (unbox<Map<string, obj>> ((box dictAlternative)))))) (box ((box Prim_undefined))))))))) (box ((box Prim_undefined)))) in let Functor0 = (sharpurs_apply (box ((Map.find "Functor0" (unbox<Map<string, obj>> ((sharpurs_apply (box ((Map.find "Alt0" (unbox<Map<string, obj>> ((sharpurs_apply (box ((Map.find "Plus1" (unbox<Map<string, obj>> ((box dictAlternative)))))) (box ((box Prim_undefined))))))))) (box ((box Prim_undefined))))))))) (box ((box Prim_undefined)))) in (box (fun (dictLazy: obj) -> (box (fun (v: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Apply_apply))) (box ((box Apply0)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Functor_map))) (box ((box Functor0)))))) (box ((box Data_Array_cons)))))) (box ((box v))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Lazy_defer))) (box ((box dictLazy)))))) (box ((box (fun (v1: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_many))) (box ((box dictAlternative)))))) (box ((box dictLazy)))))) (box ((box v))))))))))))))))))
and Data_Array_some = box (fun (dictAlternative: obj) -> Data_Array_some_tco dictAlternative)
 and Data_Array_many_tco (dictAlternative: obj) : obj = ((let Alt0 = (sharpurs_apply (box ((Map.find "Alt0" (unbox<Map<string, obj>> ((sharpurs_apply (box ((Map.find "Plus1" (unbox<Map<string, obj>> ((box dictAlternative)))))) (box ((box Prim_undefined))))))))) (box ((box Prim_undefined)))) in let Applicative0 = (sharpurs_apply (box ((Map.find "Applicative0" (unbox<Map<string, obj>> ((box dictAlternative)))))) (box ((box Prim_undefined)))) in (box (fun (dictLazy: obj) -> (box (fun (v: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Alt_alt))) (box ((box Alt0)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_some))) (box ((box dictAlternative)))))) (box ((box dictLazy)))))) (box ((box v))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Applicative_pure))) (box ((box Applicative0)))))) (box ((box [||])))))))))))))
and Data_Array_many = box (fun (dictAlternative: obj) -> Data_Array_many_tco dictAlternative)


let Data_Array_concatMap  = (sharpurs_apply (box ((box Data_Function_flip))) (box ((sharpurs_apply (box ((box Control_Bind_bind))) (box ((box Control_Bind_bindArray)))))))

let Data_Array_mapMaybe  = (box (fun (f: obj) -> (sharpurs_apply (box ((box Data_Array_concatMap))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Maybe_maybe))) (box ((box [||])))))) (box ((box Data_Array_singleton))))))))) (box ((box f)))))))))

let Data_Array_filterA  = (box (fun (dictApplicative: obj) -> (let Functor0 = (sharpurs_apply (box ((Map.find "Functor0" (unbox<Map<string, obj>> ((sharpurs_apply (box ((Map.find "Apply0" (unbox<Map<string, obj>> ((box dictApplicative)))))) (box ((box Prim_undefined))))))))) (box ((box Prim_undefined)))) in (box (fun (p: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_composeFlipped))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Traversable_traverse))) (box ((box Data_Traversable_traversableArray)))))) (box ((box dictApplicative)))))) (box ((box (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Functor_map))) (box ((box Functor0)))))) (box ((sharpurs_apply (box ((box Data_Tuple_Tuple))) (box ((box x))))))))) (box ((sharpurs_apply (box ((box p))) (box ((box x))))))))))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Functor_map))) (box ((box Functor0)))))) (box ((sharpurs_apply (box ((box Data_Array_mapMaybe))) (box ((box (fun (v: obj) -> (match ((unbox ((box v)))) with | x -> ((match ((unbox ((box b)))) with | LitBool true () -> ((sharpurs_apply (box ((box Data_Maybe_Just))) (box ((box x))))) | _ -> ((box Data_Maybe_Nothing)))))))))))))))))))))

let Data_Array_catMaybes  = (sharpurs_apply (box ((box Data_Array_mapMaybe))) (box ((sharpurs_apply (box ((box Control_Category_identity))) (box ((box Control_Category_categoryFn)))))))

let Data_Array_any  = (sharpurs_apply (box ((box Data_Function_Uncurried_runFn2))) (box ((box Data_Array_anyImpl))))

let Data_Array_nubByEq  = (box (fun (eq: obj) -> (box (fun (xs: obj) -> (sharpurs_apply (box ((box Control_Monad_ST_Internal_run))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_bind))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((box Data_Array_ST_new)))))) (box ((box (fun (arr: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_discard))) (box ((box Control_Bind_discardUnit)))))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Monad_ST_Internal_foreach))) (box ((box xs)))))) (box ((box (fun (x: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Bind_bind))) (box ((box Control_Monad_ST_Internal_bindST)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Functor_map))) (box ((box Control_Monad_ST_Internal_functorST)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Semigroupoid_compose))) (box ((box Control_Semigroupoid_semigroupoidFn)))))) (box ((box Data_Array_not)))))) (box ((sharpurs_apply (box ((box Data_Array_any))) (box ((box (fun (v: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box eq))) (box ((box v)))))) (box ((box x))))))))))))))))) (box ((sharpurs_apply (box ((box Data_Array_ST_unsafeFreeze))) (box ((box arr)))))))))))) (box ((box (fun (e: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Control_Applicative_when))) (box ((box Control_Monad_ST_Internal_applicativeST)))))) (box ((box e))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Function_apply))) (box ((box Data_Array_void2)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_ST_push))) (box ((box x)))))) (box ((box arr))))))))))))))))))))))))) (box ((box (fun (usd__unused: obj) -> (sharpurs_apply (box ((box Data_Array_ST_unsafeFreeze))) (box ((box arr)))))))))))))))))))))

let Data_Array_nubEq  = (box (fun (dictEq: obj) -> (sharpurs_apply (box ((box Data_Array_nubByEq))) (box ((sharpurs_apply (box ((box Data_Eq_eq))) (box ((box dictEq)))))))))

let Data_Array_unionBy  = (box (fun (eq: obj) -> (box (fun (xs: obj) -> (box (fun (ys: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Semigroup_append))) (box ((box Data_Semigroup_semigroupArray)))))) (box ((box xs)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_foldl))) (box ((sharpurs_apply (box ((box Data_Function_flip))) (box ((sharpurs_apply (box ((box Data_Array_deleteBy))) (box ((box eq)))))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_nubByEq))) (box ((box eq)))))) (box ((box ys))))))))) (box ((box xs)))))))))))))

let Data_Array_union  = (box (fun (dictEq: obj) -> (sharpurs_apply (box ((box Data_Array_unionBy))) (box ((sharpurs_apply (box ((box Data_Eq_eq))) (box ((box dictEq)))))))))

let Data_Array_alterAt  = (box (fun (i: obj) -> (box (fun (f: obj) -> (box (fun (xs: obj) -> (let go = (box (fun (x: obj) -> (let v = (sharpurs_apply (box ((box f))) (box ((box x)))) in (match ((unbox ((box v)))) with | _ -> ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_deleteAt))) (box ((box i)))))) (box ((box xs))))) | x_prime -> ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_updateAt))) (box ((box i)))))) (box ((box x_prime)))))) (box ((box xs))))))))) in (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Maybe_maybe))) (box ((box Data_Maybe_Nothing)))))) (box ((box go)))))) (box ((sharpurs_apply (box ((sharpurs_apply (box ((box Data_Array_index))) (box ((box xs)))))) (box ((box i))))))))))))))

let Data_Array_all  = (sharpurs_apply (box ((box Data_Function_Uncurried_runFn2))) (box ((box Data_Array_allImpl))))
