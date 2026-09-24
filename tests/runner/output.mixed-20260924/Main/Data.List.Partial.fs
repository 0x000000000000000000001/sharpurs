[<AutoOpen>]
module PureScript_Data_List_Partial

open System
open System.Collections.Generic

let Data_List_Partial_tail  = (fun (_: obj) -> (fun (v: obj) -> (sharpurs_apply (box ((fun (_: obj) -> (match ((unbox (v))) with | Data_List_Types_Consusd_Ctor(_, xs) -> (box (xs)))))) (box (Prim_undefined)))))

let rec Data_List_Partial_last  = (fun (_: obj) -> (fun (v: obj) -> (sharpurs_apply (box ((fun (_: obj) -> (match ((unbox (v))) with | Data_List_Types_Consusd_Ctor(x, Unbox(Data_List_Types_Nilusd_Ctor)) -> (box (x)) | Data_List_Types_Consusd_Ctor(_, xs) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_Partial_last)) (box (Prim_undefined))))) (box (xs))))))))) (box (Prim_undefined)))))

let rec Data_List_Partial_init  = (fun (_: obj) -> (fun (v: obj) -> (sharpurs_apply (box ((fun (_: obj) -> (match ((unbox (v))) with | Data_List_Types_Consusd_Ctor(_, Unbox(Data_List_Types_Nilusd_Ctor)) -> (box ((box Data_List_Types_Nilusd_Ctor))) | Data_List_Types_Consusd_Ctor(x, xs) -> (box ((box (Data_List_Types_Consusd_Ctor(x, (sharpurs_apply (box ((sharpurs_apply (box (Data_List_Partial_init)) (box (Prim_undefined))))) (box (xs)))))))))))) (box (Prim_undefined)))))

let Data_List_Partial_head  = (fun (_: obj) -> (fun (v: obj) -> (sharpurs_apply (box ((fun (_: obj) -> (match ((unbox (v))) with | Data_List_Types_Consusd_Ctor(x, _) -> (box (x)))))) (box (Prim_undefined)))))
