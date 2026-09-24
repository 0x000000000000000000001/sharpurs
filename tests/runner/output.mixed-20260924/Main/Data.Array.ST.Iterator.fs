[<AutoOpen>]
module PureScript_Data_Array_ST_Iterator

open System
open System.Collections.Generic

type Data_Array_ST_Iterator_Iterator =
  | Data_Array_ST_Iterator_Iteratorusd_Ctor of obj * obj

let Data_Array_ST_Iterator_bind  = (sharpurs_apply (box (Control_Bind_bind)) (box (Control_Monad_ST_Internal_bindST)))

let Data_Array_ST_Iterator_pure  = (sharpurs_apply (box (Control_Applicative_pure)) (box (Control_Monad_ST_Internal_applicativeST)))

let Data_Array_ST_Iterator_add  = (sharpurs_apply (box (Data_Semiring_add)) (box (Data_Semiring_semiringInt)))

let Data_Array_ST_Iterator_map  = (sharpurs_apply (box (Data_Functor_map)) (box (Control_Monad_ST_Internal_functorST)))

let Data_Array_ST_Iterator_not  = (sharpurs_apply (box (Data_HeytingAlgebra_not)) (box (Data_HeytingAlgebra_heytingAlgebraBoolean)))

let Data_Array_ST_Iterator_void  = (sharpurs_apply (box (Data_Functor_void)) (box (Control_Monad_ST_Internal_functorST)))

let Data_Array_ST_Iterator_Iterator  = (fun (usd__arg1: obj) -> (fun (usd__arg2: obj) -> (box (Data_Array_ST_Iterator_Iteratorusd_Ctor(usd__arg1, usd__arg2)))))

let Data_Array_ST_Iterator_peek  = (fun (v: obj) -> (match ((unbox (v))) with | Data_Array_ST_Iterator_Iteratorusd_Ctor(f, currentIndex) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Array_ST_Iterator_bind)) (box ((sharpurs_apply (box (Control_Monad_ST_Internal_read)) (box (currentIndex)))))))) (box ((fun (i: obj) -> (sharpurs_apply (box (Data_Array_ST_Iterator_pure)) (box ((sharpurs_apply (box (f)) (box (i))))))))))))))

let Data_Array_ST_Iterator_next  = (fun (v: obj) -> (match ((unbox (v))) with | Data_Array_ST_Iterator_Iteratorusd_Ctor(f, currentIndex) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Array_ST_Iterator_bind)) (box ((sharpurs_apply (box (Control_Monad_ST_Internal_read)) (box (currentIndex)))))))) (box ((fun (i: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Array_ST_Iterator_bind)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Control_Monad_ST_Internal_modify)) (box ((fun (v1: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Array_ST_Iterator_add)) (box (v1))))) (box ((box 1)))))))))) (box (currentIndex)))))))) (box ((fun (_: obj) -> (sharpurs_apply (box (Data_Array_ST_Iterator_pure)) (box ((sharpurs_apply (box (f)) (box (i))))))))))))))))))

let Data_Array_ST_Iterator_pushWhile  = (fun (p: obj) -> (fun (iter: obj) -> (fun (array: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Array_ST_Iterator_bind)) (box ((sharpurs_apply (box (Control_Monad_ST_Internal_new)) (box ((box false))))))))) (box ((fun (break: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Control_Monad_ST_Internal_while)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Array_ST_Iterator_map)) (box (Data_Array_ST_Iterator_not))))) (box ((sharpurs_apply (box (Control_Monad_ST_Internal_read)) (box (break))))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Array_ST_Iterator_bind)) (box ((sharpurs_apply (box (Data_Array_ST_Iterator_peek)) (box (iter)))))))) (box ((fun (mx: obj) -> (match ((unbox (mx))) with | Data_Maybe_Justusd_Ctor(x) when (unbox (sharpurs_apply (box (p)) (box (x)))) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Array_ST_Iterator_bind)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Array_ST_push)) (box (x))))) (box (array)))))))) (box ((fun (_: obj) -> (sharpurs_apply (box (Data_Array_ST_Iterator_void)) (box ((sharpurs_apply (box (Data_Array_ST_Iterator_next)) (box (iter)))))))))))) | _ -> (box ((sharpurs_apply (box (Data_Array_ST_Iterator_void)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Control_Monad_ST_Internal_write)) (box ((box true)))))) (box (break)))))))))))))))))))))))

let Data_Array_ST_Iterator_pushAll  = (sharpurs_apply (box (Data_Array_ST_Iterator_pushWhile)) (box ((sharpurs_apply (box (Data_Function_const)) (box ((box true)))))))

let Data_Array_ST_Iterator_iterator  = (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Array_ST_Iterator_map)) (box ((fun (usd__arg1: obj) -> (box (Data_Array_ST_Iterator_Iteratorusd_Ctor(f, usd__arg1))))))))) (box ((sharpurs_apply (box (Control_Monad_ST_Internal_new)) (box ((box 0))))))))

let Data_Array_ST_Iterator_iterate  = (fun (iter: obj) -> (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Data_Array_ST_Iterator_bind)) (box ((sharpurs_apply (box (Control_Monad_ST_Internal_new)) (box ((box false))))))))) (box ((fun (break: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (Control_Monad_ST_Internal_while)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Array_ST_Iterator_map)) (box (Data_Array_ST_Iterator_not))))) (box ((sharpurs_apply (box (Control_Monad_ST_Internal_read)) (box (break))))))))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Array_ST_Iterator_bind)) (box ((sharpurs_apply (box (Data_Array_ST_Iterator_next)) (box (iter)))))))) (box ((fun (mx: obj) -> (match ((unbox (mx))) with | Data_Maybe_Justusd_Ctor(x) -> (box ((sharpurs_apply (box (f)) (box (x))))) | Data_Maybe_Nothingusd_Ctor -> (box ((sharpurs_apply (box (Data_Array_ST_Iterator_void)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Control_Monad_ST_Internal_write)) (box ((box true)))))) (box (break))))))))))))))))))))))

let Data_Array_ST_Iterator_exhausted  = (sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (Control_Semigroupoid_compose)) (box (Control_Semigroupoid_semigroupoidFn))))) (box ((sharpurs_apply (box (Data_Array_ST_Iterator_map)) (box (Data_Maybe_isNothing)))))))) (box (Data_Array_ST_Iterator_peek)))
