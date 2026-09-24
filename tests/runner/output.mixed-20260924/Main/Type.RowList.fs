[<AutoOpen>]
module PureScript_Type_RowList

open System
open System.Collections.Generic

let Type_RowList_RowListSetusd_Dict  = (fun (x: obj) -> x)

let Type_RowList_RowListRemoveusd_Dict  = (fun (x: obj) -> x)

let Type_RowList_RowListNubusd_Dict  = (fun (x: obj) -> x)

let Type_RowList_RowListAppendusd_Dict  = (fun (x: obj) -> x)

let Type_RowList_ListToRowusd_Dict  = (fun (x: obj) -> x)

let Type_RowList_rowListSetImpl  = (fun (dictTypeEquals: obj) -> (fun (dictTypeEquals1: obj) -> (fun (_: obj) -> (sharpurs_apply (box (Type_RowList_RowListSetusd_Dict)) (box (Map.empty))))))

let Type_RowList_rowListRemoveNil  = (sharpurs_apply (box (Type_RowList_RowListRemoveusd_Dict)) (box (Map.empty)))

let Type_RowList_rowListRemoveCons  = (fun (_: obj) -> (fun (_: obj) -> (fun (_: obj) -> (sharpurs_apply (box (Type_RowList_RowListRemoveusd_Dict)) (box (Map.empty))))))

let Type_RowList_rowListNubNil  = (sharpurs_apply (box (Type_RowList_RowListNubusd_Dict)) (box (Map.empty)))

let Type_RowList_rowListNubCons  = (fun (dictTypeEquals: obj) -> (fun (dictTypeEquals1: obj) -> (fun (dictTypeEquals2: obj) -> (fun (_: obj) -> (fun (_: obj) -> (sharpurs_apply (box (Type_RowList_RowListNubusd_Dict)) (box (Map.empty))))))))

let Type_RowList_rowListAppendNil  = (fun (dictTypeEquals: obj) -> (sharpurs_apply (box (Type_RowList_RowListAppendusd_Dict)) (box (Map.empty))))

let Type_RowList_rowListAppendCons  = (fun (_: obj) -> (fun (dictTypeEquals: obj) -> (sharpurs_apply (box (Type_RowList_RowListAppendusd_Dict)) (box (Map.empty)))))

let Type_RowList_listToRowNil  = (sharpurs_apply (box (Type_RowList_ListToRowusd_Dict)) (box (Map.empty)))

let Type_RowList_listToRowCons  = (fun (_: obj) -> (fun (_: obj) -> (sharpurs_apply (box (Type_RowList_ListToRowusd_Dict)) (box (Map.empty)))))
