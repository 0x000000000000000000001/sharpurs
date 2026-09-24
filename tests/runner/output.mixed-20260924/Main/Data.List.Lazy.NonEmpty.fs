[<AutoOpen>]
module PureScript_Data_List_Lazy_NonEmpty

open System
open System.Collections.Generic

let Data_List_Lazy_NonEmpty_compose  = (sharpurs_apply (box (Control_Semigroupoid_compose)) (box (Control_Semigroupoid_semigroupoidFn)))

let Data_List_Lazy_NonEmpty_map  = (sharpurs_apply (box (Data_Functor_map)) (box (Data_Maybe_functorMaybe)))

let Data_List_Lazy_NonEmpty_add  = (sharpurs_apply (box (Data_Semiring_add)) (box (Data_Semiring_semiringInt)))

let Data_List_Lazy_NonEmpty_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_List_Lazy_Types_semigroupList)))

let Data_List_Lazy_NonEmpty_uncons  = (fun (v: obj) -> (match ((unbox (v))) with | nel -> (box ((let v1 = (sharpurs_apply (box (Data_Lazy_force)) (box (nel))) in (match ((unbox (v1))) with | Data_NonEmpty_NonEmptyusd_Ctor(x, xs) -> (box ((Map.add "head" (box (x)) (Map.add "tail" (box (xs)) Map.empty))))))))))

let Data_List_Lazy_NonEmpty_toList  = (fun (v: obj) -> (match ((unbox (v))) with | nel -> (box ((let v1 = (sharpurs_apply (box (Data_Lazy_force)) (box (nel))) in (match ((unbox (v1))) with | Data_NonEmpty_NonEmptyusd_Ctor(x, xs) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_Lazy_Types_cons)) (box (x))))) (box (xs)))))))))))

let Data_List_Lazy_NonEmpty_toUnfoldable  = (fun (dictUnfoldable: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_List_Lazy_NonEmpty_compose)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Unfoldable_unfoldr)) (box (dictUnfoldable))))) (box ((fun (xs: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_List_Lazy_NonEmpty_map)) (box ((fun (rec_: obj) -> (box (Data_Tuple_Tupleusd_Ctor((Map.find "head" (unbox<Map<string, obj>> (rec_))), (Map.find "tail" (unbox<Map<string, obj>> (rec_)))))))))))) (box ((sharpurs_apply (box (Data_List_Lazy_uncons)) (box (xs))))))))))))))) (box (Data_List_Lazy_NonEmpty_toList))))

let Data_List_Lazy_NonEmpty_tail  = (fun (v: obj) -> (match ((unbox (v))) with | nel -> (box ((let v1 = (sharpurs_apply (box (Data_Lazy_force)) (box (nel))) in (match ((unbox (v1))) with | Data_NonEmpty_NonEmptyusd_Ctor(_, xs) -> (box (xs))))))))

let Data_List_Lazy_NonEmpty_singleton  = (sharpurs_apply (box (Control_Applicative_pure)) (box (Data_List_Lazy_Types_applicativeNonEmptyList)))

let Data_List_Lazy_NonEmpty_repeat  = (fun (x: obj) -> (sharpurs_apply (box (Data_List_Lazy_Types_NonEmptyList)) (box ((sharpurs_apply (box (Data_Lazy_defer)) (box ((fun (v: obj) -> (box (Data_NonEmpty_NonEmptyusd_Ctor(x, (sharpurs_apply (box (Data_List_Lazy_repeat)) (box (x))))))))))))))

let Data_List_Lazy_NonEmpty_length  = (fun (v: obj) -> (match ((unbox (v))) with | nel -> (box ((let v1 = (sharpurs_apply (box (Data_Lazy_force)) (box (nel))) in (match ((unbox (v1))) with | Data_NonEmpty_NonEmptyusd_Ctor(_, xs) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_Lazy_NonEmpty_add)) (box ((box 1)))))) (box ((sharpurs_apply (box (Data_List_Lazy_length)) (box (xs))))))))))))))

let Data_List_Lazy_NonEmpty_last  = (fun (v: obj) -> (match ((unbox (v))) with | nel -> (box ((let v1 = (sharpurs_apply (box (Data_Lazy_force)) (box (nel))) in (match ((unbox (v1))) with | Data_NonEmpty_NonEmptyusd_Ctor(x, xs) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Maybe_fromMaybe)) (box (x))))) (box ((sharpurs_apply (box (Data_List_Lazy_last)) (box (xs))))))))))))))

let Data_List_Lazy_NonEmpty_iterate  = (fun (f: obj) -> (fun (x: obj) -> (sharpurs_apply (box (Data_List_Lazy_Types_NonEmptyList)) (box ((sharpurs_apply (box (Data_Lazy_defer)) (box ((fun (v: obj) -> (box (Data_NonEmpty_NonEmptyusd_Ctor(x, (sharpurs_apply (box ((sharpurs_apply (box (Data_List_Lazy_iterate)) (box (f))))) (box ((sharpurs_apply (box (f)) (box (x))))))))))))))))))

let Data_List_Lazy_NonEmpty_init  = (fun (v: obj) -> (match ((unbox (v))) with | nel -> (box ((let v1 = (sharpurs_apply (box (Data_Lazy_force)) (box (nel))) in (match ((unbox (v1))) with | Data_NonEmpty_NonEmptyusd_Ctor(x, xs) -> (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Maybe_maybe)) (box (Data_List_Lazy_Types_nil))))) (box ((fun (v2: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_List_Lazy_Types_cons)) (box (x))))) (box (v2))))))))) (box ((sharpurs_apply (box (Data_List_Lazy_init)) (box (xs))))))))))))))

let Data_List_Lazy_NonEmpty_head  = (fun (v: obj) -> (match ((unbox (v))) with | nel -> (box ((let v1 = (sharpurs_apply (box (Data_Lazy_force)) (box (nel))) in (match ((unbox (v1))) with | Data_NonEmpty_NonEmptyusd_Ctor(x, _) -> (box (x))))))))

let Data_List_Lazy_NonEmpty_fromList  = (fun (l: obj) -> (let v = (sharpurs_apply (box (Data_List_Lazy_Types_step)) (box (l))) in (match ((unbox (v))) with | Data_List_Lazy_Types_Nilusd_Ctor -> (box ((box Data_Maybe_Nothingusd_Ctor))) | Data_List_Lazy_Types_Consusd_Ctor(x, xs) -> (box ((box (Data_Maybe_Justusd_Ctor((sharpurs_apply (box (Data_List_Lazy_Types_NonEmptyList)) (box ((sharpurs_apply (box (Data_Lazy_defer)) (box ((fun (v1: obj) -> (box (Data_NonEmpty_NonEmptyusd_Ctor(x, xs))))))))))))))))))

let Data_List_Lazy_NonEmpty_fromFoldable  = (fun (dictFoldable: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_List_Lazy_NonEmpty_compose)) (box (Data_List_Lazy_NonEmpty_fromList))))) (box ((sharpurs_apply (box (Data_List_Lazy_fromFoldable)) (box (dictFoldable)))))))

let Data_List_Lazy_NonEmpty_cons  = (fun (y: obj) -> (fun (v: obj) -> (match (((unbox (y)), (unbox (v)))) with | (y1, nel) -> (box ((sharpurs_apply (box (Data_List_Lazy_Types_NonEmptyList)) (box ((sharpurs_apply (box (Data_Lazy_defer)) (box ((fun (v1: obj) -> (let v2 = (sharpurs_apply (box (Data_Lazy_force)) (box (nel))) in (match ((unbox (v2))) with | Data_NonEmpty_NonEmptyusd_Ctor(x, xs) -> (box ((box (Data_NonEmpty_NonEmptyusd_Ctor(y1, (sharpurs_apply (box ((sharpurs_apply (box (Data_List_Lazy_Types_cons)) (box (x))))) (box (xs))))))))))))))))))))))

let Data_List_Lazy_NonEmpty_concatMap  = (sharpurs_apply (box (Data_Function_flip)) (box ((sharpurs_apply (box (Control_Bind_bind)) (box (Data_List_Lazy_Types_bindNonEmptyList))))))

let Data_List_Lazy_NonEmpty_appendFoldable  = (fun (dictFoldable: obj) -> (let fromFoldable1 = (sharpurs_apply (box (Data_List_Lazy_fromFoldable)) (box (dictFoldable))) in (fun (nel: obj) -> (fun (ys: obj) -> (sharpurs_apply (box (Data_List_Lazy_Types_NonEmptyList)) (box ((sharpurs_apply (box (Data_Lazy_defer)) (box ((fun (v: obj) -> (box (Data_NonEmpty_NonEmptyusd_Ctor((sharpurs_apply (box (Data_List_Lazy_NonEmpty_head)) (box (nel))), (sharpurs_apply (box ((sharpurs_apply (box (Data_List_Lazy_NonEmpty_append)) (box ((sharpurs_apply (box (Data_List_Lazy_NonEmpty_tail)) (box (nel)))))))) (box ((sharpurs_apply (box (fromFoldable1)) (box (ys))))))))))))))))))))
