[<AutoOpen>]
module PureScript_Main

open System
open System.Collections.Generic

type Main_X =
  | Main_Xusd_Ctor

let Main_X  = (box Main_Xusd_Ctor)

let Main_Cusd_Dict  = (box (fun (x1: obj) -> (box x1)))

let Main_x  = (box Main_Xusd_Ctor)

let Main_test  = (box (fun (dictMonad: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Control_Applicative_pure))) (box ((sharpurs_apply (box ((Map.find "Applicative0" (unbox<Map<string, obj>> ((box dictMonad)))))) (box ((box Prim_undefined))))))))) (box ((box ((Map.add "x" (box ((box Main_x))) Map.empty))))))))

let Main_main  = (sharpurs_apply (box ((box Effect_Console_log))) (box ((box "Done"))))

let Main_cA  = (sharpurs_apply (box ((box Main_Cusd_Dict))) (box ((box ((Map.add "c" (box ((box (fun (x1: obj) -> (box (fun (v: obj) -> (box x1))))))) Map.empty))))))

let Main_c  = (box (fun (dict: obj) -> (match ((unbox ((box dict)))) with | v -> ((Map.find "c" (unbox<Map<string, obj>> ((box v))))))))

let Main_c1  = (sharpurs_apply (box ((box Main_c))) (box ((box Main_cA))))

let Main_test2  = (box (fun (dictMonad: obj) -> (sharpurs_apply (box ((sharpurs_apply (box ((box Control_Applicative_pure))) (box ((sharpurs_apply (box ((Map.find "Applicative0" (unbox<Map<string, obj>> ((box dictMonad)))))) (box ((box Prim_undefined))))))))) (box ((box ((Map.add "ccc" (box ((box Main_c1))) Map.empty))))))))
