[<AutoOpen>]
module PureScript_Record_Builder

open System
open System.Collections.Generic

module Record_Builder_FFI =
    let copyRecord = box (fun (recObj: obj) ->
        let recDict = unbox<System.Collections.Generic.Dictionary<string, obj>> recObj
        let copy = System.Collections.Generic.Dictionary<string, obj>()
        for kvp in recDict do
            copy.[kvp.Key] <- kvp.Value
        box copy
    )
    
    let unsafeInsert = box (fun (lObj: obj) -> box (fun (aObj: obj) -> box (fun (recObj: obj) ->
        let l = unbox<string> lObj
        let recDict = unbox<System.Collections.Generic.Dictionary<string, obj>> recObj
        recDict.[l] <- aObj
        box recDict
    )))
    
    let unsafeModify = box (fun (lObj: obj) -> box (fun (fObj: obj) -> box (fun (recObj: obj) ->
        let l = unbox<string> lObj
        let f = unbox<obj -> obj> fObj
        let recDict = unbox<System.Collections.Generic.Dictionary<string, obj>> recObj
        recDict.[l] <- f recDict.[l]
        box recDict
    )))
    
    let unsafeDelete = box (fun (lObj: obj) -> box (fun (recObj: obj) ->
        let l = unbox<string> lObj
        let recDict = unbox<System.Collections.Generic.Dictionary<string, obj>> recObj
        recDict.Remove(l) |> ignore
        box recDict
    ))
    
    let unsafeRename = box (fun (l1Obj: obj) -> box (fun (l2Obj: obj) -> box (fun (recObj: obj) ->
        let l1 = unbox<string> l1Obj
        let l2 = unbox<string> l2Obj
        let recDict = unbox<System.Collections.Generic.Dictionary<string, obj>> recObj
        recDict.[l2] <- recDict.[l1]
        recDict.Remove(l1) |> ignore
        box recDict
    )))
    

let Record_Builder_copyRecord = box Record_Builder_FFI.``copyRecord``
let Record_Builder_unsafeInsert = box Record_Builder_FFI.``unsafeInsert``
let Record_Builder_unsafeModify = box Record_Builder_FFI.``unsafeModify``
let Record_Builder_unsafeDelete = box Record_Builder_FFI.``unsafeDelete``
let Record_Builder_unsafeRename = box Record_Builder_FFI.``unsafeRename``


let Record_Builder_Builder  = (fun (x: obj) -> x)

let Record_Builder_union  = (fun (_: obj) -> (fun (r1: obj) -> (sharpurs_apply (box (Record_Builder_Builder)) (box ((fun (r2: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Function_Uncurried_runFn2)) (box (Record_Unsafe_Union_unsafeUnionFn))))) (box (r1))))) (box (r2)))))))))

let Record_Builder_semigroupoidBuilder  = Control_Semigroupoid_semigroupoidFn

let Record_Builder_rename  = (fun (dictIsSymbol: obj) -> (let reflectSymbol = (sharpurs_apply (box (Data_Symbol_reflectSymbol)) (box (dictIsSymbol))) in (fun (dictIsSymbol1: obj) -> (let reflectSymbol1 = (sharpurs_apply (box (Data_Symbol_reflectSymbol)) (box (dictIsSymbol1))) in (fun (_: obj) -> (fun (_: obj) -> (fun (_: obj) -> (fun (_: obj) -> (fun (l1: obj) -> (fun (l2: obj) -> (sharpurs_apply (box (Record_Builder_Builder)) (box ((fun (r1: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Record_Builder_unsafeRename)) (box ((sharpurs_apply (box (reflectSymbol)) (box (l1)))))))) (box ((sharpurs_apply (box (reflectSymbol1)) (box (l2)))))))) (box (r1)))))))))))))))))

let Record_Builder_nub  = (fun (_: obj) -> (sharpurs_apply (box (Record_Builder_Builder)) (box (Unsafe_Coerce_unsafeCoerce))))

let Record_Builder_modify  = (fun (_: obj) -> (fun (_: obj) -> (fun (dictIsSymbol: obj) -> (let reflectSymbol = (sharpurs_apply (box (Data_Symbol_reflectSymbol)) (box (dictIsSymbol))) in (fun (l: obj) -> (fun (f: obj) -> (sharpurs_apply (box (Record_Builder_Builder)) (box ((fun (r1: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Record_Builder_unsafeModify)) (box ((sharpurs_apply (box (reflectSymbol)) (box (l)))))))) (box (f))))) (box (r1)))))))))))))

let Record_Builder_merge  = (fun (_: obj) -> (fun (_: obj) -> (fun (r1: obj) -> (sharpurs_apply (box (Record_Builder_Builder)) (box ((fun (r2: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Function_Uncurried_runFn2)) (box (Record_Unsafe_Union_unsafeUnionFn))))) (box (r1))))) (box (r2))))))))))

let Record_Builder_insert  = (fun (_: obj) -> (fun (_: obj) -> (fun (dictIsSymbol: obj) -> (let reflectSymbol = (sharpurs_apply (box (Data_Symbol_reflectSymbol)) (box (dictIsSymbol))) in (fun (l: obj) -> (fun (a: obj) -> (sharpurs_apply (box (Record_Builder_Builder)) (box ((fun (r1: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Record_Builder_unsafeInsert)) (box ((sharpurs_apply (box (reflectSymbol)) (box (l)))))))) (box (a))))) (box (r1)))))))))))))

let Record_Builder_disjointUnion  = (fun (_: obj) -> (fun (_: obj) -> (fun (r1: obj) -> (sharpurs_apply (box (Record_Builder_Builder)) (box ((fun (r2: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Function_Uncurried_runFn2)) (box (Record_Unsafe_Union_unsafeUnionFn))))) (box (r1))))) (box (r2))))))))))

let Record_Builder_delete  = (fun (dictIsSymbol: obj) -> (let reflectSymbol = (sharpurs_apply (box (Data_Symbol_reflectSymbol)) (box (dictIsSymbol))) in (fun (_: obj) -> (fun (_: obj) -> (fun (l: obj) -> (sharpurs_apply (box (Record_Builder_Builder)) (box ((fun (r2: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Record_Builder_unsafeDelete)) (box ((sharpurs_apply (box (reflectSymbol)) (box (l)))))))) (box (r2))))))))))))

let Record_Builder_categoryBuilder  = Control_Category_categoryFn

let Record_Builder_build  = (fun (v: obj) -> (fun (r1: obj) -> (match (((unbox (v)), (unbox (r1)))) with | (b, r11) -> (box ((sharpurs_apply (box (b)) (box ((sharpurs_apply (box (Record_Builder_copyRecord)) (box (r11)))))))))))

let Record_Builder_buildFromScratch  = (sharpurs_apply (box ((sharpurs_apply (box (Data_Function_flip)) (box (Record_Builder_build))))) (box (Map.empty)))

let Record_Builder_flip  = (fun (f: obj) -> (fun (b: obj) -> (sharpurs_apply (box (Record_Builder_Builder)) (box ((fun (a: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Record_Builder_build)) (box ((sharpurs_apply (box (f)) (box (a)))))))) (box (b)))))))))
