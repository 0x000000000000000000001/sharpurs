[<AutoOpen>]
module PureScript_Data_Traversable_Accum_Internal

open System
open System.Collections.Generic

let Data_Traversable_Accum_Internal_StateR  = (fun (x: obj) -> x)

let Data_Traversable_Accum_Internal_StateL  = (fun (x: obj) -> x)

let Data_Traversable_Accum_Internal_stateR  = (fun (v: obj) -> (match ((unbox (v))) with | k -> (box (k))))

let Data_Traversable_Accum_Internal_stateL  = (fun (v: obj) -> (match ((unbox (v))) with | k -> (box (k))))

let Data_Traversable_Accum_Internal_functorStateR  = (sharpurs_apply (box (Data_Functor_Functorusd_Dict)) (box ((Map.add "map" (box ((fun (f: obj) -> (fun (k: obj) -> (sharpurs_apply (box (Data_Traversable_Accum_Internal_StateR)) (box ((fun (s: obj) -> (let v = (sharpurs_apply (box ((sharpurs_apply (box (Data_Traversable_Accum_Internal_stateR)) (box (k))))) (box (s))) in (match ((unbox (v))) with | (HasProp "accum" (s1) & HasProp "value" (a)) -> (box ((Map.add "accum" (box (s1)) (Map.add "value" (box ((sharpurs_apply (box (f)) (box (a))))) Map.empty)))))))))))))) Map.empty))))

let Data_Traversable_Accum_Internal_functorStateL  = (sharpurs_apply (box (Data_Functor_Functorusd_Dict)) (box ((Map.add "map" (box ((fun (f: obj) -> (fun (k: obj) -> (sharpurs_apply (box (Data_Traversable_Accum_Internal_StateL)) (box ((fun (s: obj) -> (let v = (sharpurs_apply (box ((sharpurs_apply (box (Data_Traversable_Accum_Internal_stateL)) (box (k))))) (box (s))) in (match ((unbox (v))) with | (HasProp "accum" (s1) & HasProp "value" (a)) -> (box ((Map.add "accum" (box (s1)) (Map.add "value" (box ((sharpurs_apply (box (f)) (box (a))))) Map.empty)))))))))))))) Map.empty))))

let Data_Traversable_Accum_Internal_applyStateR  = (sharpurs_apply (box (Control_Apply_Applyusd_Dict)) (box ((Map.add "apply" (box ((fun (f: obj) -> (fun (x: obj) -> (sharpurs_apply (box (Data_Traversable_Accum_Internal_StateR)) (box ((fun (s: obj) -> (let v = (sharpurs_apply (box ((sharpurs_apply (box (Data_Traversable_Accum_Internal_stateR)) (box (x))))) (box (s))) in (match ((unbox (v))) with | (HasProp "accum" (s1) & HasProp "value" (x_prime)) -> (box ((let v1 = (sharpurs_apply (box ((sharpurs_apply (box (Data_Traversable_Accum_Internal_stateR)) (box (f))))) (box (s1))) in (match ((unbox (v1))) with | (HasProp "accum" (s2) & HasProp "value" (f_prime)) -> (box ((Map.add "accum" (box (s2)) (Map.add "value" (box ((sharpurs_apply (box (f_prime)) (box (x_prime))))) Map.empty)))))))))))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Traversable_Accum_Internal_functorStateR))) Map.empty)))))

let Data_Traversable_Accum_Internal_applyStateL  = (sharpurs_apply (box (Control_Apply_Applyusd_Dict)) (box ((Map.add "apply" (box ((fun (f: obj) -> (fun (x: obj) -> (sharpurs_apply (box (Data_Traversable_Accum_Internal_StateL)) (box ((fun (s: obj) -> (let v = (sharpurs_apply (box ((sharpurs_apply (box (Data_Traversable_Accum_Internal_stateL)) (box (f))))) (box (s))) in (match ((unbox (v))) with | (HasProp "accum" (s1) & HasProp "value" (f_prime)) -> (box ((let v1 = (sharpurs_apply (box ((sharpurs_apply (box (Data_Traversable_Accum_Internal_stateL)) (box (x))))) (box (s1))) in (match ((unbox (v1))) with | (HasProp "accum" (s2) & HasProp "value" (x_prime)) -> (box ((Map.add "accum" (box (s2)) (Map.add "value" (box ((sharpurs_apply (box (f_prime)) (box (x_prime))))) Map.empty)))))))))))))))))) (Map.add "Functor0" (box ((fun (_: obj) -> Data_Traversable_Accum_Internal_functorStateL))) Map.empty)))))

let Data_Traversable_Accum_Internal_applicativeStateR  = (sharpurs_apply (box (Control_Applicative_Applicativeusd_Dict)) (box ((Map.add "pure" (box ((fun (a: obj) -> (sharpurs_apply (box (Data_Traversable_Accum_Internal_StateR)) (box ((fun (s: obj) -> (Map.add "accum" (box (s)) (Map.add "value" (box (a)) Map.empty))))))))) (Map.add "Apply0" (box ((fun (_: obj) -> Data_Traversable_Accum_Internal_applyStateR))) Map.empty)))))

let Data_Traversable_Accum_Internal_applicativeStateL  = (sharpurs_apply (box (Control_Applicative_Applicativeusd_Dict)) (box ((Map.add "pure" (box ((fun (a: obj) -> (sharpurs_apply (box (Data_Traversable_Accum_Internal_StateL)) (box ((fun (s: obj) -> (Map.add "accum" (box (s)) (Map.add "value" (box (a)) Map.empty))))))))) (Map.add "Apply0" (box ((fun (_: obj) -> Data_Traversable_Accum_Internal_applyStateL))) Map.empty)))))
