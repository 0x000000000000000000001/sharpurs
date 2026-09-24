[<AutoOpen>]
module PureScript_Data_List_ZipList

open System
open System.Collections.Generic

let Data_List_ZipList_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Semigroup_semigroupString)))

let Data_List_ZipList_append1  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_List_Lazy_Types_semigroupList)))

let Data_List_ZipList_ZipList  = (fun (x: obj) -> x)

let Data_List_ZipList_traversableZipList  = Data_List_Lazy_Types_traversableList

let Data_List_ZipList_showZipList  = (fun (dictShow: obj) -> (let show = (sharpurs_apply (box (Data_Show_show)) (box ((sharpurs_apply (box (Data_List_Lazy_Types_showList)) (box (dictShow)))))) in (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | xs -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_ZipList_append)) (box ((box "(ZipList ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_ZipList_append)) (box ((sharpurs_apply (box (show)) (box (xs)))))))) (box ((box ")"))))))))))))) Map.empty))))))

let Data_List_ZipList_semigroupZipList  = Data_List_Lazy_Types_semigroupList

let Data_List_ZipList_ordZipList  = (fun (dictOrd: obj) -> (sharpurs_apply (box (Data_List_Lazy_Types_ordList)) (box (dictOrd))))

let Data_List_ZipList_newtypeZipList  = (sharpurs_apply (box (Data_Newtype_Newtypeusd_Dict)) (box ((Map.add "Coercible0" (box ((fun (_: obj) -> Prim_undefined))) Map.empty))))

let Data_List_ZipList_monoidZipList  = Data_List_Lazy_Types_monoidList

let Data_List_ZipList_functorZipList  = Data_List_Lazy_Types_functorList

let Data_List_ZipList_foldableZipList  = Data_List_Lazy_Types_foldableList

let Data_List_ZipList_eqZipList  = (fun (dictEq: obj) -> (sharpurs_apply (box (Data_List_Lazy_Types_eqList)) (box (dictEq))))

let Data_List_ZipList_applyZipList  = (sharpurs_apply (box (Control_Apply_Applyusd_Dict)) (box ((Map.add "apply" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (fs, xs) -> (box ((sharpurs_apply (box (Data_List_ZipList_ZipList)) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_Lazy_zipWith)) (box (Data_Function_apply))))) (box (fs))))) (box (xs))))))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_List_ZipList_functorZipList))) Map.empty)))))

let Data_List_ZipList_zipListIsNotBind  = (fun (_: obj) -> (sharpurs_apply (box (Control_Bind_Bindusd_Dict)) (box ((Map.add "bind" (box ((sharpurs_apply (box (Partial_Unsafe_unsafeCrashWith)) (box ((box "bind: unreachable")))))) (Map.add "Apply0" (box ((fun (_: obj) -> Data_List_ZipList_applyZipList))) Map.empty))))))

let Data_List_ZipList_applicativeZipList  = (sharpurs_apply (box (Control_Applicative_Applicativeusd_Dict)) (box ((Map.add "pure" (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Control_Semigroupoid_compose)) (box (Control_Semigroupoid_semigroupoidFn))))) (box (Data_List_ZipList_ZipList))))) (box (Data_List_Lazy_repeat))))) (Map.add "Apply0" (box ((fun (_: obj) -> Data_List_ZipList_applyZipList))) Map.empty)))))

let Data_List_ZipList_altZipList  = (sharpurs_apply (box (Control_Alt_Altusd_Dict)) (box ((Map.add "alt" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (xs, ys) -> (box ((sharpurs_apply (box (Data_List_ZipList_ZipList)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_ZipList_append1)) (box (xs))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_Lazy_drop)) (box ((sharpurs_apply (box (Data_List_Lazy_length)) (box (xs)))))))) (box (ys)))))))))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_List_ZipList_functorZipList))) Map.empty)))))

let Data_List_ZipList_plusZipList  = (sharpurs_apply (box (Control_Plus_Plususd_Dict)) (box ((Map.add "empty" (box ((sharpurs_apply (box (Data_Monoid_mempty)) (box (Data_List_ZipList_monoidZipList))))) (Map.add "Alt0" (box ((fun (_: obj) -> Data_List_ZipList_altZipList))) Map.empty)))))

let Data_List_ZipList_alternativeZipList  = (sharpurs_apply (box (Control_Alternative_Alternativeusd_Dict)) (box ((Map.add "Applicative0" (box ((fun (_: obj) -> Data_List_ZipList_applicativeZipList))) (Map.add "Plus1" (box ((fun (_: obj) -> Data_List_ZipList_plusZipList))) Map.empty)))))
