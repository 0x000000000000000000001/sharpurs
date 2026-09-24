[<AutoOpen>]
module PureScript_Record_Unsafe

open System
open System.Collections.Generic

module Record_Unsafe_FFI =
    let unsafeHas = box (fun (k: obj) -> box (fun (map: obj) -> box (Map.containsKey (unbox<string> k) (unbox<Map<string, obj>> map))))
    let unsafeGet = box (fun (k: obj) -> box (fun (map: obj) -> Map.find (unbox<string> k) (unbox<Map<string, obj>> map)))
    let unsafeSet = box (fun (k: obj) -> box (fun (v: obj) -> box (fun (map: obj) -> box (Map.add (unbox<string> k) v (unbox<Map<string, obj>> map)))))
    let unsafeDelete = box (fun (k: obj) -> box (fun (map: obj) -> box (Map.remove (unbox<string> k) (unbox<Map<string, obj>> map))))
    

let Record_Unsafe_unsafeDelete = box (Record_Unsafe_FFI.``unsafeDelete``)
let Record_Unsafe_unsafeGet = box (Record_Unsafe_FFI.``unsafeGet``)
let Record_Unsafe_unsafeHas = box (Record_Unsafe_FFI.``unsafeHas``)
let Record_Unsafe_unsafeSet = box (Record_Unsafe_FFI.``unsafeSet``)



