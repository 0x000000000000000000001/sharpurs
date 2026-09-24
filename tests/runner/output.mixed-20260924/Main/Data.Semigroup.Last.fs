[<AutoOpen>]
module PureScript_Data_Semigroup_Last

open System
open System.Collections.Generic

let Data_Semigroup_Last_append  = (sharpurs_apply (box (Data_Semigroup_append)) (box (Data_Semigroup_semigroupString)))

let Data_Semigroup_Last_Last  = (fun (x: obj) -> x)

let Data_Semigroup_Last_showLast  = (fun (dictShow: obj) -> (let show = (sharpurs_apply (box (Data_Show_show)) (box (dictShow))) in (sharpurs_apply (box (Data_Show_Showusd_Dict)) (box ((Map.add "show" (box ((fun (v: obj) -> (match ((unbox (v))) with | a -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Semigroup_Last_append)) (box ((box "(Last ")))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Semigroup_Last_append)) (box ((sharpurs_apply (box (show)) (box (a)))))))) (box ((box ")"))))))))))))) Map.empty))))))

let Data_Semigroup_Last_semigroupLast  = (sharpurs_apply (box (Data_Semigroup_Semigroupusd_Dict)) (box ((Map.add "append" (box ((fun (v: obj) -> (fun (x: obj) -> x)))) Map.empty))))

let Data_Semigroup_Last_ordLast  = (fun (dictOrd: obj) -> dictOrd)

let Data_Semigroup_Last_functorLast  = (sharpurs_apply (box (Data_Functor_Functorusd_Dict)) (box ((Map.add "map" (box ((fun (f: obj) -> (fun (m: obj) -> (match ((unbox (m))) with | v -> (box ((sharpurs_apply (box (Data_Semigroup_Last_Last)) (box ((sharpurs_apply (box (f)) (box (v))))))))))))) Map.empty))))

let Data_Semigroup_Last_eqLast  = (fun (dictEq: obj) -> dictEq)

let Data_Semigroup_Last_eq1Last  = (sharpurs_apply (box (Data_Eq_Eq1usd_Dict)) (box ((Map.add "eq1" (box ((fun (dictEq: obj) -> (sharpurs_apply (box (Data_Eq_eq)) (box ((sharpurs_apply (box (Data_Semigroup_Last_eqLast)) (box (dictEq))))))))) Map.empty))))

let Data_Semigroup_Last_ord1Last  = (sharpurs_apply (box (Data_Ord_Ord1usd_Dict)) (box ((Map.add "compare1" (box ((fun (dictOrd: obj) -> (sharpurs_apply (box (Data_Ord_compare)) (box ((sharpurs_apply (box (Data_Semigroup_Last_ordLast)) (box (dictOrd))))))))) (Map.add "Eq10" (box ((fun (_: obj) -> Data_Semigroup_Last_eq1Last))) Map.empty)))))

let Data_Semigroup_Last_boundedLast  = (fun (dictBounded: obj) -> dictBounded)

let Data_Semigroup_Last_applyLast  = (sharpurs_apply (box (Control_Apply_Applyusd_Dict)) (box ((Map.add "apply" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (f, x) -> (box ((sharpurs_apply (box (Data_Semigroup_Last_Last)) (box ((sharpurs_apply (box (f)) (box (x))))))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Semigroup_Last_functorLast))) Map.empty)))))

let Data_Semigroup_Last_bindLast  = (sharpurs_apply (box (Control_Bind_Bindusd_Dict)) (box ((Map.add "bind" (box ((fun (v: obj) -> (fun (f: obj) -> (match (((unbox (v)), (unbox (f)))) with | (x, f1) -> (box ((sharpurs_apply (box (f1)) (box (x)))))))))) (Map.add "Apply0" (box ((fun (_: obj) -> Data_Semigroup_Last_applyLast))) Map.empty)))))

let Data_Semigroup_Last_applicativeLast  = (sharpurs_apply (box (Control_Applicative_Applicativeusd_Dict)) (box ((Map.add "pure" (box (Data_Semigroup_Last_Last)) (Map.add "Apply0" (box ((fun (_: obj) -> Data_Semigroup_Last_applyLast))) Map.empty)))))

let Data_Semigroup_Last_monadLast  = (sharpurs_apply (box (Control_Monad_Monadusd_Dict)) (box ((Map.add "Applicative0" (box ((fun (_: obj) -> Data_Semigroup_Last_applicativeLast))) (Map.add "Bind1" (box ((fun (_: obj) -> Data_Semigroup_Last_bindLast))) Map.empty)))))
