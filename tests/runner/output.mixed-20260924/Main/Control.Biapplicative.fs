[<AutoOpen>]
module PureScript_Control_Biapplicative

open System
open System.Collections.Generic

let Control_Biapplicative_Biapplicativeusd_Dict  = (fun (x: obj) -> x)

let Control_Biapplicative_bipure  = (fun (dict: obj) -> (match ((unbox (dict))) with | v -> (box ((Map.find "bipure" (unbox<Map<string, obj>> (v)))))))

let Control_Biapplicative_biapplicativeTuple  = (sharpurs_apply (box (Control_Biapplicative_Biapplicativeusd_Dict)) (box ((Map.add "bipure" (box ((fun (usd__arg1: obj) -> (fun (usd__arg2: obj) -> (box (Data_Tuple_Tupleusd_Ctor(usd__arg1, usd__arg2))))))) (Map.add "Biapply0" (box ((fun (_: obj) -> Control_Biapply_biapplyTuple))) Map.empty)))))
