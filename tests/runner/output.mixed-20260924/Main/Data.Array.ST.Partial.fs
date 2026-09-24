[<AutoOpen>]
module PureScript_Data_Array_ST_Partial

open System
open System.Collections.Generic

module Data_Array_ST_Partial_FFI =
    let peekImpl = box (fun (i: obj) -> box (fun (xs: obj) ->
        let arr = unbox<System.Collections.Generic.List<obj>> xs
        let idx = unbox<int> i
        arr.[idx]
    ))
    
    let pokeImpl = box (fun (i: obj) -> box (fun (a: obj) -> box (fun (xs: obj) ->
        let arr = unbox<System.Collections.Generic.List<obj>> xs
        let idx = unbox<int> i
        arr.[idx] <- a
        box ()
    )))
    

let Data_Array_ST_Partial_peekImpl = box Data_Array_ST_Partial_FFI.``peekImpl``
let Data_Array_ST_Partial_pokeImpl = box Data_Array_ST_Partial_FFI.``pokeImpl``


let Data_Array_ST_Partial_poke  = (fun (_: obj) -> (sharpurs_apply (box (Control_Monad_ST_Uncurried_runSTFn3)) (box (Data_Array_ST_Partial_pokeImpl))))

let Data_Array_ST_Partial_peek  = (fun (_: obj) -> (sharpurs_apply (box (Control_Monad_ST_Uncurried_runSTFn2)) (box (Data_Array_ST_Partial_peekImpl))))
