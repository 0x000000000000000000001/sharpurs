[<AutoOpen>]
module PureScript_Data_Array_ST

open System
open System.Collections.Generic

module Data_Array_ST_FFI =
    let ``new`` = box (fun () -> box (System.Collections.Generic.List<obj>()))
    let peekImpl = box (fun (just: obj) -> box (fun (nothing: obj) -> box (fun (i: obj) -> box (fun (xs: obj) ->
        let arr = unbox<System.Collections.Generic.List<obj>> xs
        let idx = unbox<int> i
        if idx >= 0 && idx < arr.Count then sharpurs_apply just arr.[idx] else nothing
    ))))
    let pokeImpl = box (fun (i: obj) -> box (fun (a: obj) -> box (fun (xs: obj) ->
        let arr = unbox<System.Collections.Generic.List<obj>> xs
        let idx = unbox<int> i
        let ret = idx >= 0 && idx < arr.Count
        if ret then arr.[idx] <- a
        box ret
    )))
    let lengthImpl = box (fun (xs: obj) -> box (unbox<System.Collections.Generic.List<obj>> xs).Count)
    let popImpl = box (fun (just: obj) -> box (fun (nothing: obj) -> box (fun (xs: obj) ->
        let arr = unbox<System.Collections.Generic.List<obj>> xs
        if arr.Count > 0 then
            let v = arr.[arr.Count - 1]
            arr.RemoveAt(arr.Count - 1)
            sharpurs_apply just v
        else nothing
    )))
    let pushAllImpl = box (fun (as_: obj) -> box (fun (xs: obj) ->
        let arr = unbox<System.Collections.Generic.List<obj>> xs
        let src = unbox<obj[]> as_
        arr.AddRange(src)
        box arr.Count
    ))
    let shiftImpl = box (fun (just: obj) -> box (fun (nothing: obj) -> box (fun (xs: obj) ->
        let arr = unbox<System.Collections.Generic.List<obj>> xs
        if arr.Count > 0 then
            let v = arr.[0]
            arr.RemoveAt(0)
            sharpurs_apply just v
        else nothing
    )))
    let unshiftAllImpl = box (fun (as_: obj) -> box (fun (xs: obj) ->
        let arr = unbox<System.Collections.Generic.List<obj>> xs
        let src = unbox<obj[]> as_
        arr.InsertRange(0, src)
        box arr.Count
    ))
    let spliceImpl = box (fun (i: obj) -> box (fun (howMany: obj) -> box (fun (bs: obj) -> box (fun (xs: obj) ->
        let arr = unbox<System.Collections.Generic.List<obj>> xs
        let idx = unbox<int> i
        let count = unbox<int> howMany
        let src = unbox<obj[]> bs
        let ret = arr.GetRange(idx, count).ToArray()
        arr.RemoveRange(idx, count)
        arr.InsertRange(idx, src)
        box ret
    ))))
    let unsafeFreezeImpl = box (fun (xs: obj) -> box ((unbox<System.Collections.Generic.List<obj>> xs).ToArray()))
    let unsafeThawImpl = box (fun (xs: obj) -> box (System.Collections.Generic.List<obj>(unbox<obj[]> xs)))
    let freezeImpl = box (fun (xs: obj) -> box ((unbox<System.Collections.Generic.List<obj>> xs).ToArray()))
    let thawImpl = box (fun (xs: obj) -> box (System.Collections.Generic.List<obj>(unbox<obj[]> xs)))
    let cloneImpl = box (fun (xs: obj) -> box (System.Collections.Generic.List<obj>(unbox<System.Collections.Generic.List<obj>> xs)))
    
    let sortByImpl = box (fun (compare: obj) -> box (fun (fromOrdering: obj) -> box (fun (xs: obj) ->
        let arr = unbox<System.Collections.Generic.List<obj>> xs
        arr.Sort(System.Collections.Generic.Comparer<obj>.Create(fun x y -> 
            let ord = sharpurs_apply (sharpurs_apply compare x) y
            unbox<int> (sharpurs_apply fromOrdering ord)
        ))
        xs
    )))
    
    let pushImpl = box (fun (a: obj) -> box (fun (xs: obj) ->
        let arr = unbox<System.Collections.Generic.List<obj>> xs
        arr.Add(a)
        box arr.Count
    ))
    let unshiftImpl = box (fun (a: obj) -> box (fun (xs: obj) ->
        let arr = unbox<System.Collections.Generic.List<obj>> xs
        arr.Insert(0, a)
        box arr.Count
    ))
    
    let toAssocArrayImpl = box (fun (xs: obj) ->
        let arr = unbox<System.Collections.Generic.List<obj>> xs
        let res = Array.zeroCreate<obj> arr.Count
        for i = 0 to arr.Count - 1 do
            let m = Map.empty |> Map.add "value" arr.[i] |> Map.add "index" (box i)
            res.[i] <- box m
        box res
    )
    

let Data_Array_ST_unsafeFreezeImpl = box Data_Array_ST_FFI.``unsafeFreezeImpl``
let Data_Array_ST_unsafeThawImpl = box Data_Array_ST_FFI.``unsafeThawImpl``
let Data_Array_ST_new = box Data_Array_ST_FFI.``new``
let Data_Array_ST_thawImpl = box Data_Array_ST_FFI.``thawImpl``
let Data_Array_ST_cloneImpl = box Data_Array_ST_FFI.``cloneImpl``
let Data_Array_ST_shiftImpl = box Data_Array_ST_FFI.``shiftImpl``
let Data_Array_ST_sortByImpl = box Data_Array_ST_FFI.``sortByImpl``
let Data_Array_ST_freezeImpl = box Data_Array_ST_FFI.``freezeImpl``
let Data_Array_ST_peekImpl = box Data_Array_ST_FFI.``peekImpl``
let Data_Array_ST_pokeImpl = box Data_Array_ST_FFI.``pokeImpl``
let Data_Array_ST_lengthImpl = box Data_Array_ST_FFI.``lengthImpl``
let Data_Array_ST_popImpl = box Data_Array_ST_FFI.``popImpl``
let Data_Array_ST_pushImpl = box Data_Array_ST_FFI.``pushImpl``
let Data_Array_ST_pushAllImpl = box Data_Array_ST_FFI.``pushAllImpl``
let Data_Array_ST_unshiftAllImpl = box Data_Array_ST_FFI.``unshiftAllImpl``
let Data_Array_ST_spliceImpl = box Data_Array_ST_FFI.``spliceImpl``
let Data_Array_ST_toAssocArrayImpl = box Data_Array_ST_FFI.``toAssocArrayImpl``


let Data_Array_ST_bind  = (sharpurs_apply (box (Control_Bind_bind)) (box (Control_Monad_ST_Internal_bindST)))

let Data_Array_ST_negate  = (sharpurs_apply (box (Data_Ring_negate)) (box (Data_Ring_ringInt)))

let Data_Array_ST_pure  = (sharpurs_apply (box (Control_Applicative_pure)) (box (Control_Monad_ST_Internal_applicativeST)))

let Data_Array_ST_unshiftAll  = (sharpurs_apply (box (Control_Monad_ST_Uncurried_runSTFn2)) (box (Data_Array_ST_unshiftAllImpl)))

let Data_Array_ST_unshift  = (fun (a: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Control_Monad_ST_Uncurried_runSTFn2)) (box (Data_Array_ST_unshiftAllImpl))))) (box ((box [|a|])))))

let Data_Array_ST_unsafeThaw  = (sharpurs_apply (box (Control_Monad_ST_Uncurried_runSTFn1)) (box (Data_Array_ST_unsafeThawImpl)))

let Data_Array_ST_unsafeFreeze  = (sharpurs_apply (box (Control_Monad_ST_Uncurried_runSTFn1)) (box (Data_Array_ST_unsafeFreezeImpl)))

let Data_Array_ST_toAssocArray  = (sharpurs_apply (box (Control_Monad_ST_Uncurried_runSTFn1)) (box (Data_Array_ST_toAssocArrayImpl)))

let Data_Array_ST_thaw  = (sharpurs_apply (box (Control_Monad_ST_Uncurried_runSTFn1)) (box (Data_Array_ST_thawImpl)))

let Data_Array_ST_withArray  = (fun (f: obj) -> (fun (xs: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Array_ST_bind)) (box ((sharpurs_apply (box (Data_Array_ST_thaw)) (box (xs)))))))) (box ((fun (result: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Array_ST_bind)) (box ((sharpurs_apply (box (f)) (box (result)))))))) (box ((fun (_: obj) -> (sharpurs_apply (box (Data_Array_ST_unsafeFreeze)) (box (result)))))))))))))

let Data_Array_ST_splice  = (sharpurs_apply (box (Control_Monad_ST_Uncurried_runSTFn4)) (box (Data_Array_ST_spliceImpl)))

let Data_Array_ST_sortBy  = (fun (comp: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Control_Monad_ST_Uncurried_runSTFn3)) (box (Data_Array_ST_sortByImpl))))) (box (comp))))) (box ((fun (v: obj) -> (match ((unbox (v))) with | Data_Ordering_GTusd_Ctor -> (box ((box 1))) | Data_Ordering_EQusd_Ctor -> (box ((box 0))) | Data_Ordering_LTusd_Ctor -> (box ((sharpurs_apply (box (Data_Array_ST_negate)) (box ((box 1))))))))))))

let Data_Array_ST_sortWith  = (fun (dictOrd: obj) -> (let comparing = (sharpurs_apply (box (Data_Ord_comparing)) (box (dictOrd))) in (fun (f: obj) -> (sharpurs_apply (box (Data_Array_ST_sortBy)) (box ((sharpurs_apply (box (comparing)) (box (f)))))))))

let Data_Array_ST_sort  = (fun (dictOrd: obj) -> (sharpurs_apply (box (Data_Array_ST_sortBy)) (box ((sharpurs_apply (box (Data_Ord_compare)) (box (dictOrd)))))))

let Data_Array_ST_shift  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Control_Monad_ST_Uncurried_runSTFn3)) (box (Data_Array_ST_shiftImpl))))) (box ((fun (usd__arg1: obj) -> (box (Data_Maybe_Justusd_Ctor(usd__arg1))))))))) (box ((box Data_Maybe_Nothingusd_Ctor))))

let Data_Array_ST_run  = (fun (st: obj) -> (sharpurs_apply (box (Control_Monad_ST_Internal_run)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Array_ST_bind)) (box (st))))) (box (Data_Array_ST_unsafeFreeze)))))))

let Data_Array_ST_pushAll  = (sharpurs_apply (box (Control_Monad_ST_Uncurried_runSTFn2)) (box (Data_Array_ST_pushAllImpl)))

let Data_Array_ST_push  = (sharpurs_apply (box (Control_Monad_ST_Uncurried_runSTFn2)) (box (Data_Array_ST_pushImpl)))

let Data_Array_ST_pop  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Control_Monad_ST_Uncurried_runSTFn3)) (box (Data_Array_ST_popImpl))))) (box ((fun (usd__arg1: obj) -> (box (Data_Maybe_Justusd_Ctor(usd__arg1))))))))) (box ((box Data_Maybe_Nothingusd_Ctor))))

let Data_Array_ST_poke  = (sharpurs_apply (box (Control_Monad_ST_Uncurried_runSTFn3)) (box (Data_Array_ST_pokeImpl)))

let Data_Array_ST_peek  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Control_Monad_ST_Uncurried_runSTFn4)) (box (Data_Array_ST_peekImpl))))) (box ((fun (usd__arg1: obj) -> (box (Data_Maybe_Justusd_Ctor(usd__arg1))))))))) (box ((box Data_Maybe_Nothingusd_Ctor))))

let Data_Array_ST_modify  = (fun (i: obj) -> (fun (f: obj) -> (fun (xs: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Array_ST_bind)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Array_ST_peek)) (box (i))))) (box (xs)))))))) (box ((fun (entry: obj) -> (match ((unbox (entry))) with | Data_Maybe_Justusd_Ctor(x) -> (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Array_ST_poke)) (box (i))))) (box ((sharpurs_apply (box (f)) (box (x)))))))) (box (xs))))) | Data_Maybe_Nothingusd_Ctor -> (box ((sharpurs_apply (box (Data_Array_ST_pure)) (box ((box false))))))))))))))

let Data_Array_ST_length  = (sharpurs_apply (box (Control_Monad_ST_Uncurried_runSTFn1)) (box (Data_Array_ST_lengthImpl)))

let Data_Array_ST_freeze  = (sharpurs_apply (box (Control_Monad_ST_Uncurried_runSTFn1)) (box (Data_Array_ST_freezeImpl)))

let Data_Array_ST_clone  = (sharpurs_apply (box (Control_Monad_ST_Uncurried_runSTFn1)) (box (Data_Array_ST_cloneImpl)))
