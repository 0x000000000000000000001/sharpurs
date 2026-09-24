[<AutoOpen>]
module PureScript_Control_Applicative

open System
open System.Collections.Generic

let Control_Applicative_Applicativeusd_Dict  = (fun (x: obj) -> x)

let Control_Applicative_pure  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "pure" (unbox<Map<string, obj>> (v)))))))

let Control_Applicative_unless  = (fun (dictApplicative: obj) -> (let pure1 = (sharpurs_apply (box (Control_Applicative_pure)) (box (dictApplicative))) in (fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (LitBool false (), m) -> (box (m)) | (LitBool true (), _) -> (box ((sharpurs_apply (box (pure1)) (box (Data_Unit_unit))))))))))

let Control_Applicative_when  = (fun (dictApplicative: obj) -> (let pure1 = (sharpurs_apply (box (Control_Applicative_pure)) (box (dictApplicative))) in (fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (LitBool true (), m) -> (box (m)) | (LitBool false (), _) -> (box ((sharpurs_apply (box (pure1)) (box (Data_Unit_unit))))))))))

let Control_Applicative_liftA1  = (fun (dictApplicative: obj) -> (let apply = (sharpurs_apply (box (Control_Apply_apply)) (box ((sharpurs_apply (box ((Map.find "Apply0" (unbox<Map<string, obj>> (dictApplicative))))) (box (Prim_undefined)))))) in let pure1 = (sharpurs_apply (box (Control_Applicative_pure)) (box (dictApplicative))) in (fun (f: obj) -> (fun (a: obj) -> (sharpurs_apply (box ((sharpurs_apply (box (apply)) (box ((sharpurs_apply (box (pure1)) (box (f)))))))) (box (a)))))))

let Control_Applicative_applicativeProxy  = (sharpurs_apply (box (Control_Applicative_Applicativeusd_Dict)) (box ((Map.add "pure" (box ((fun (v: obj) -> (box Type_Proxy_Proxyusd_Ctor)))) (Map.add "Apply0" (box ((fun (_: obj) -> Control_Apply_applyProxy))) Map.empty)))))

let Control_Applicative_applicativeFn  = (sharpurs_apply (box (Control_Applicative_Applicativeusd_Dict)) (box ((Map.add "pure" (box ((fun (x: obj) -> (fun (v: obj) -> x)))) (Map.add "Apply0" (box ((fun (_: obj) -> Control_Apply_applyFn))) Map.empty)))))

let Control_Applicative_applicativeArray  = (sharpurs_apply (box (Control_Applicative_Applicativeusd_Dict)) (box ((Map.add "pure" (box ((fun (x: obj) -> (box [|x|])))) (Map.add "Apply0" (box ((fun (_: obj) -> Control_Apply_applyArray))) Map.empty)))))
