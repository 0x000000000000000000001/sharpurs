[<AutoOpen>]
module PureScript_Data_Functor_Invariant

open System
open System.Collections.Generic

let Data_Functor_Invariant_compose  = (sharpurs_apply (box (Control_Semigroupoid_compose)) (box (Control_Semigroupoid_semigroupoidFn)))

let Data_Functor_Invariant_Invariantusd_Dict  = (fun (x: obj) -> x)

let Data_Functor_Invariant_invariantMultiplicative  = (sharpurs_apply (box (Data_Functor_Invariant_Invariantusd_Dict)) (box ((Map.add "imap" (box ((fun (f: obj) -> (fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (f)), (unbox (v)), (unbox (v1)))) with | (f1, _, x) -> (box ((sharpurs_apply (box (Data_Monoid_Multiplicative_Multiplicative)) (box ((sharpurs_apply (box (f1)) (box (x)))))))))))))) Map.empty))))

let Data_Functor_Invariant_invariantEndo  = (sharpurs_apply (box (Data_Functor_Invariant_Invariantusd_Dict)) (box ((Map.add "imap" (box ((fun (ab: obj) -> (fun (ba: obj) -> (fun (v: obj) -> (match (((unbox (ab)), (unbox (ba)), (unbox (v)))) with | (ab1, ba1, f) -> (box ((sharpurs_apply (box (Data_Monoid_Endo_Endo)) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Functor_Invariant_compose)) (box (ab1))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_Functor_Invariant_compose)) (box (f))))) (box (ba1))))))))))))))))) Map.empty))))

let Data_Functor_Invariant_invariantDual  = (sharpurs_apply (box (Data_Functor_Invariant_Invariantusd_Dict)) (box ((Map.add "imap" (box ((fun (f: obj) -> (fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (f)), (unbox (v)), (unbox (v1)))) with | (f1, _, x) -> (box ((sharpurs_apply (box (Data_Monoid_Dual_Dual)) (box ((sharpurs_apply (box (f1)) (box (x)))))))))))))) Map.empty))))

let Data_Functor_Invariant_invariantDisj  = (sharpurs_apply (box (Data_Functor_Invariant_Invariantusd_Dict)) (box ((Map.add "imap" (box ((fun (f: obj) -> (fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (f)), (unbox (v)), (unbox (v1)))) with | (f1, _, x) -> (box ((sharpurs_apply (box (Data_Monoid_Disj_Disj)) (box ((sharpurs_apply (box (f1)) (box (x)))))))))))))) Map.empty))))

let Data_Functor_Invariant_invariantConj  = (sharpurs_apply (box (Data_Functor_Invariant_Invariantusd_Dict)) (box ((Map.add "imap" (box ((fun (f: obj) -> (fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (f)), (unbox (v)), (unbox (v1)))) with | (f1, _, x) -> (box ((sharpurs_apply (box (Data_Monoid_Conj_Conj)) (box ((sharpurs_apply (box (f1)) (box (x)))))))))))))) Map.empty))))

let Data_Functor_Invariant_invariantAdditive  = (sharpurs_apply (box (Data_Functor_Invariant_Invariantusd_Dict)) (box ((Map.add "imap" (box ((fun (f: obj) -> (fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (f)), (unbox (v)), (unbox (v1)))) with | (f1, _, x) -> (box ((sharpurs_apply (box (Data_Monoid_Additive_Additive)) (box ((sharpurs_apply (box (f1)) (box (x)))))))))))))) Map.empty))))

let Data_Functor_Invariant_imapF  = (fun (dictFunctor: obj) -> (let map = (sharpurs_apply (box (Data_Functor_map)) (box (dictFunctor))) in (fun (f: obj) -> (fun (v: obj) -> (sharpurs_apply (box (map)) (box (f)))))))

let Data_Functor_Invariant_invariantArray  = (sharpurs_apply (box (Data_Functor_Invariant_Invariantusd_Dict)) (box ((Map.add "imap" (box ((sharpurs_apply (box (Data_Functor_Invariant_imapF)) (box (Data_Functor_functorArray))))) Map.empty))))

let Data_Functor_Invariant_invariantFn  = (sharpurs_apply (box (Data_Functor_Invariant_Invariantusd_Dict)) (box ((Map.add "imap" (box ((sharpurs_apply (box (Data_Functor_Invariant_imapF)) (box (Data_Functor_functorFn))))) Map.empty))))

let Data_Functor_Invariant_imap  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "imap" (unbox<Map<string, obj>> (v)))))))

let Data_Functor_Invariant_invariantAlternate  = (fun (dictInvariant: obj) -> (let imap1 = (sharpurs_apply (box (Data_Functor_Invariant_imap)) (box (dictInvariant))) in (sharpurs_apply (box (Data_Functor_Invariant_Invariantusd_Dict)) (box ((Map.add "imap" (box ((fun (f: obj) -> (fun (g: obj) -> (fun (v: obj) -> (match (((unbox (f)), (unbox (g)), (unbox (v)))) with | (f1, g1, x) -> (box ((sharpurs_apply (box (Data_Monoid_Alternate_Alternate)) (box ((sharpurs_apply (box ((sharpurs_apply (box ((sharpurs_apply (box (imap1)) (box (f1))))) (box (g1))))) (box (x)))))))))))))) Map.empty))))))
