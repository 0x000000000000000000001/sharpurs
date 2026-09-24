[<AutoOpen>]
module PureScript_Data_Int_Bits

open System
open System.Collections.Generic

module Data_Int_Bits_FFI =
    let ``and`` = fun (n1: obj) -> fun (n2: obj) -> box ((n1 :?> int) &&& (n2 :?> int))
    let ``or`` = fun (n1: obj) -> fun (n2: obj) -> box ((n1 :?> int) ||| (n2 :?> int))
    let xor = fun (n1: obj) -> fun (n2: obj) -> box ((n1 :?> int) ^^^ (n2 :?> int))
    let shl = fun (n1: obj) -> fun (n2: obj) -> box ((n1 :?> int) <<< (n2 :?> int))
    let shr = fun (n1: obj) -> fun (n2: obj) -> box ((n1 :?> int) >>> (n2 :?> int))
    let zshr = fun (n1: obj) -> fun (n2: obj) -> box (int (uint32 (n1 :?> int) >>> (n2 :?> int)))
    let complement = fun (n: obj) -> box (~~~(n :?> int))
    

let Data_Int_Bits_and = box (Data_Int_Bits_FFI.``and``)
let Data_Int_Bits_complement = box (Data_Int_Bits_FFI.``complement``)
let Data_Int_Bits_or = box (Data_Int_Bits_FFI.``or``)
let Data_Int_Bits_shl = box (Data_Int_Bits_FFI.``shl``)
let Data_Int_Bits_shr = box (Data_Int_Bits_FFI.``shr``)
let Data_Int_Bits_xor = box (Data_Int_Bits_FFI.``xor``)
let Data_Int_Bits_zshr = box (Data_Int_Bits_FFI.``zshr``)



