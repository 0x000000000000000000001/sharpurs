[<AutoOpen>]
module PureScript_Data_List_Internal

open System
open System.Collections.Generic

type Data_List_Internal_Set =
  | Data_List_Internal_Leafusd_Ctor
  | Data_List_Internal_Twousd_Ctor of obj * obj * obj
  | Data_List_Internal_Threeusd_Ctor of obj * obj * obj * obj * obj

type Data_List_Internal_TreeContext =
  | Data_List_Internal_TwoLeftusd_Ctor of obj * obj
  | Data_List_Internal_TwoRightusd_Ctor of obj * obj
  | Data_List_Internal_ThreeLeftusd_Ctor of obj * obj * obj * obj
  | Data_List_Internal_ThreeMiddleusd_Ctor of obj * obj * obj * obj
  | Data_List_Internal_ThreeRightusd_Ctor of obj * obj * obj * obj

type Data_List_Internal_KickUp =
  | Data_List_Internal_KickUpusd_Ctor of obj * obj * obj

let Data_List_Internal_Leaf  = (box Data_List_Internal_Leafusd_Ctor)

let Data_List_Internal_Two  = (fun (usd__arg1: obj) -> (fun (usd__arg2: obj) -> (fun (usd__arg3: obj) -> (box (Data_List_Internal_Twousd_Ctor(usd__arg1, usd__arg2, usd__arg3))))))

let Data_List_Internal_Three  = (fun (usd__arg1: obj) -> (fun (usd__arg2: obj) -> (fun (usd__arg3: obj) -> (fun (usd__arg4: obj) -> (fun (usd__arg5: obj) -> (box (Data_List_Internal_Threeusd_Ctor(usd__arg1, usd__arg2, usd__arg3, usd__arg4, usd__arg5))))))))

let Data_List_Internal_TwoLeft  = (fun (usd__arg1: obj) -> (fun (usd__arg2: obj) -> (box (Data_List_Internal_TwoLeftusd_Ctor(usd__arg1, usd__arg2)))))

let Data_List_Internal_TwoRight  = (fun (usd__arg1: obj) -> (fun (usd__arg2: obj) -> (box (Data_List_Internal_TwoRightusd_Ctor(usd__arg1, usd__arg2)))))

let Data_List_Internal_ThreeLeft  = (fun (usd__arg1: obj) -> (fun (usd__arg2: obj) -> (fun (usd__arg3: obj) -> (fun (usd__arg4: obj) -> (box (Data_List_Internal_ThreeLeftusd_Ctor(usd__arg1, usd__arg2, usd__arg3, usd__arg4)))))))

let Data_List_Internal_ThreeMiddle  = (fun (usd__arg1: obj) -> (fun (usd__arg2: obj) -> (fun (usd__arg3: obj) -> (fun (usd__arg4: obj) -> (box (Data_List_Internal_ThreeMiddleusd_Ctor(usd__arg1, usd__arg2, usd__arg3, usd__arg4)))))))

let Data_List_Internal_ThreeRight  = (fun (usd__arg1: obj) -> (fun (usd__arg2: obj) -> (fun (usd__arg3: obj) -> (fun (usd__arg4: obj) -> (box (Data_List_Internal_ThreeRightusd_Ctor(usd__arg1, usd__arg2, usd__arg3, usd__arg4)))))))

let Data_List_Internal_KickUp  = (fun (usd__arg1: obj) -> (fun (usd__arg2: obj) -> (fun (usd__arg3: obj) -> (box (Data_List_Internal_KickUpusd_Ctor(usd__arg1, usd__arg2, usd__arg3))))))

let rec Data_List_Internal_fromZipper  = (fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (Data_List_Types_Nilusd_Ctor, tree) -> (box (tree)) | (Data_List_Types_Consusd_Ctor(x, ctx), tree) -> (box ((match ((unbox (x))) with | Data_List_Internal_TwoLeftusd_Ctor(k1, right) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_Internal_fromZipper)) (box (ctx))))) (box ((box (Data_List_Internal_Twousd_Ctor(tree, k1, right)))))))) | Data_List_Internal_TwoRightusd_Ctor(left, k1) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_Internal_fromZipper)) (box (ctx))))) (box ((box (Data_List_Internal_Twousd_Ctor(left, k1, tree)))))))) | Data_List_Internal_ThreeLeftusd_Ctor(k1, mid, k2, right) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_Internal_fromZipper)) (box (ctx))))) (box ((box (Data_List_Internal_Threeusd_Ctor(tree, k1, mid, k2, right)))))))) | Data_List_Internal_ThreeMiddleusd_Ctor(left, k1, k2, right) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_Internal_fromZipper)) (box (ctx))))) (box ((box (Data_List_Internal_Threeusd_Ctor(left, k1, tree, k2, right)))))))) | Data_List_Internal_ThreeRightusd_Ctor(left, k1, mid, k2) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_Internal_fromZipper)) (box (ctx))))) (box ((box (Data_List_Internal_Threeusd_Ctor(left, k1, mid, k2, tree))))))))))))))

let Data_List_Internal_insertAndLookupBy  = (fun (comp: obj) -> (fun (k: obj) -> (fun (orig: obj) -> (let mutable up : obj = null in up <- (fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (Data_List_Types_Nilusd_Ctor, Data_List_Internal_KickUpusd_Ctor(left, k_prime, right)) -> (box ((box (Data_List_Internal_Twousd_Ctor(left, k_prime, right))))) | (Data_List_Types_Consusd_Ctor(x, ctx), kup) -> (box ((match (((unbox (x)), (unbox (kup)))) with | (Data_List_Internal_TwoLeftusd_Ctor(k1, right), Data_List_Internal_KickUpusd_Ctor(left, k_prime, mid)) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_Internal_fromZipper)) (box (ctx))))) (box ((box (Data_List_Internal_Threeusd_Ctor(left, k_prime, mid, k1, right)))))))) | (Data_List_Internal_TwoRightusd_Ctor(left, k1), Data_List_Internal_KickUpusd_Ctor(mid, k_prime, right)) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (Data_List_Internal_fromZipper)) (box (ctx))))) (box ((box (Data_List_Internal_Threeusd_Ctor(left, k1, mid, k_prime, right)))))))) | (Data_List_Internal_ThreeLeftusd_Ctor(k1, c, k2, d), Data_List_Internal_KickUpusd_Ctor(a, k_prime, b)) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (up)) (box (ctx))))) (box ((box (Data_List_Internal_KickUpusd_Ctor((box (Data_List_Internal_Twousd_Ctor(a, k_prime, b))), k1, (box (Data_List_Internal_Twousd_Ctor(c, k2, d))))))))))) | (Data_List_Internal_ThreeMiddleusd_Ctor(a, k1, k2, d), Data_List_Internal_KickUpusd_Ctor(b, k_prime, c)) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (up)) (box (ctx))))) (box ((box (Data_List_Internal_KickUpusd_Ctor((box (Data_List_Internal_Twousd_Ctor(a, k1, b))), k_prime, (box (Data_List_Internal_Twousd_Ctor(c, k2, d))))))))))) | (Data_List_Internal_ThreeRightusd_Ctor(a, k1, b, k2), Data_List_Internal_KickUpusd_Ctor(c, k_prime, d)) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (up)) (box (ctx))))) (box ((box (Data_List_Internal_KickUpusd_Ctor((box (Data_List_Internal_Twousd_Ctor(a, k1, b))), k2, (box (Data_List_Internal_Twousd_Ctor(c, k_prime, d))))))))))))))))); let mutable down : obj = null in down <- (fun (v: obj) -> (fun (v1: obj) -> (match (((unbox (v)), (unbox (v1)))) with | (ctx, Data_List_Internal_Leafusd_Ctor) -> (box ((Map.add "found" (box ((box false))) (Map.add "result" (box ((sharpurs_apply (box ((sharpurs_apply (box (up)) (box (ctx))))) (box ((box (Data_List_Internal_KickUpusd_Ctor((box Data_List_Internal_Leafusd_Ctor), k, (box Data_List_Internal_Leafusd_Ctor))))))))) Map.empty)))) | (ctx, Data_List_Internal_Twousd_Ctor(left, k1, right)) -> (box ((let v2 = (sharpurs_apply (box ((sharpurs_apply (box (comp)) (box (k))))) (box (k1))) in (match ((unbox (v2))) with | Data_Ordering_EQusd_Ctor -> (box ((Map.add "found" (box ((box true))) (Map.add "result" (box (orig)) Map.empty)))) | Data_Ordering_LTusd_Ctor -> (box ((sharpurs_apply (box ((sharpurs_apply (box (down)) (box ((box (Data_List_Types_Consusd_Ctor((box (Data_List_Internal_TwoLeftusd_Ctor(k1, right))), ctx)))))))) (box (left))))) | _ -> (box ((sharpurs_apply (box ((sharpurs_apply (box (down)) (box ((box (Data_List_Types_Consusd_Ctor((box (Data_List_Internal_TwoRightusd_Ctor(left, k1))), ctx)))))))) (box (right))))))))) | (ctx, Data_List_Internal_Threeusd_Ctor(left, k1, mid, k2, right)) -> (box ((let v2 = (sharpurs_apply (box ((sharpurs_apply (box (comp)) (box (k))))) (box (k1))) in (match ((unbox (v2))) with | Data_Ordering_EQusd_Ctor -> (box ((Map.add "found" (box ((box true))) (Map.add "result" (box (orig)) Map.empty)))) | c1 -> (box ((let v3 = (sharpurs_apply (box ((sharpurs_apply (box (comp)) (box (k))))) (box (k2))) in let v4 = c1 in (match (((unbox (v4)), (unbox (v3)))) with | (_, Data_Ordering_EQusd_Ctor) -> (box ((Map.add "found" (box ((box true))) (Map.add "result" (box (orig)) Map.empty)))) | (Data_Ordering_LTusd_Ctor, _) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (down)) (box ((box (Data_List_Types_Consusd_Ctor((box (Data_List_Internal_ThreeLeftusd_Ctor(k1, mid, k2, right))), ctx)))))))) (box (left))))) | (Data_Ordering_GTusd_Ctor, Data_Ordering_LTusd_Ctor) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (down)) (box ((box (Data_List_Types_Consusd_Ctor((box (Data_List_Internal_ThreeMiddleusd_Ctor(left, k1, k2, right))), ctx)))))))) (box (mid))))) | (_, _) -> (box ((sharpurs_apply (box ((sharpurs_apply (box (down)) (box ((box (Data_List_Types_Consusd_Ctor((box (Data_List_Internal_ThreeRightusd_Ctor(left, k1, mid, k2))), ctx)))))))) (box (right)))))))))))))))); (sharpurs_apply (box ((sharpurs_apply (box (down)) (box ((box Data_List_Types_Nilusd_Ctor)))))) (box (orig)))))))

let Data_List_Internal_emptySet  = (box Data_List_Internal_Leafusd_Ctor)
