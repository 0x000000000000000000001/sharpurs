[<AutoOpen>]
module PureScript_Data_Unit

open System
open System.Collections.Generic

module Data_Unit_FFI =
    let unit = undefined
    

let Data_Unit_unit = box (Data_Unit_FFI.``unit``)



