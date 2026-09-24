[<AutoOpen>]
module PureScript_Data_Functor_Contravariant

open System
open System.Collections.Generic

let Data_Functor_Contravariant_Contravariantusd_Dict  = (fun (x: obj) -> x)

let Data_Functor_Contravariant_contravariantConst  = (sharpurs_apply (box (Data_Functor_Contravariant_Contravariantusd_Dict)) (box ((Map.add "cmap" (box ((fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (_, x) -> (box ((sharpurs_apply (box (Data_Const_Const)) (box (x)))))))))) Map.empty))))

let Data_Functor_Contravariant_cmap  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "cmap" (unbox<Map<string, obj>> (v)))))))

let Data_Functor_Contravariant_cmapFlipped  = (fun (dictContravariant: obj) -> (let cmap1 = (sharpurs_apply (box (Data_Functor_Contravariant_cmap)) (box (dictContravariant))) in (fun (x: obj) -> (fun (f: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (cmap1)) (box (f))))) (box (x)))))))

let Data_Functor_Contravariant_coerce  = (fun (dictContravariant: obj) -> (let cmap1 = (sharpurs_apply (box (Data_Functor_Contravariant_cmap)) (box (dictContravariant))) in (fun (dictFunctor: obj) -> (let map = (sharpurs_apply (box (Data_Functor_map)) (box (dictFunctor))) in (fun (a: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (map)) (box (Data_Void_absurd))))) (box ((sharpurs_apply (box ((sharpurs_apply (box (cmap1)) (box (Data_Void_absurd))))) (box (a)))))))))))

let Data_Functor_Contravariant_imapC  = (fun (dictContravariant: obj) -> (let cmap1 = (sharpurs_apply (box (Data_Functor_Contravariant_cmap)) (box (dictContravariant))) in (fun (v: obj) -> (fun (f: obj) -> (sharpurs_apply (box (cmap1)) (box (f)))))))
