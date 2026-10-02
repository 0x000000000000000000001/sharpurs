module AdtLoweringSupport

type Lowering_Tree =
    | Lowering_Emptyusd_Ctor
    | Lowering_Nodeusd_Ctor of Lowering_Tree * int * Lowering_Tree

let events = ResizeArray<int>()
let sentinel = System.InvalidOperationException("lowering sentinel")
let constant = 42
let mark value = events.Add(value); value
let helper (value: int) = value + 1
let explode (_: int) : int = events.Add(99); raise sentinel
let explodeBool (_: bool) : bool = events.Add(99); raise sentinel
let combine (left: int) (right: int) = events.Add(77); left * 10 + right
let selfWorker (_: Lowering_Tree) (_: Lowering_Tree) (value: int) (_: bool) = value + 1
let Lowering_Node_adt_native left value right = Lowering_Nodeusd_Ctor(left, value, right)

let guarded action =
    try action()
    with ex -> raise (System.Reflection.TargetInvocationException(ex))
let mark_apply value = guarded (fun () -> mark value)
let helper_apply value = guarded (fun () -> helper value)
let explode_apply value = guarded (fun () -> explode value)
let explodeBool_apply value = guarded (fun () -> explodeBool value)
let combine_apply left right = guarded (fun () -> combine left right)

// These targets must never be chosen: recursion and constructors are unguarded.
let selfWorker_apply (_: Lowering_Tree) (_: Lowering_Tree) (_: int) (_: bool) : int = failwith "guarded self call"
let Lowering_Node_adt_native_apply (_: Lowering_Tree) (_: int) (_: Lowering_Tree) : Lowering_Tree = failwith "guarded constructor"
