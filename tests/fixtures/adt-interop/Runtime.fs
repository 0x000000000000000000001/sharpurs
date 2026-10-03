
let mutable checks = 0
let check label ok = if not ok then failwith label else checks <- checks + 1
let apply = sharpurs_apply
let make fn color left value right = apply (apply (apply (apply fn color) (box left)) (box value)) (box right) |> unbox<AdtPilot_Tree>
let leaf value = AdtPilot_T_adt_native AdtPilot_R_adt_native AdtPilot_E_adt_native value AdtPilot_E_adt_native
let red = leaf 7
let made = make AdtConsumer_saturated AdtPilot_B red 11 red
check "saturated consumer native depth" (AdtPilot_depth_adt_native made = 2)
check "saturated consumer Int" (AdtPilot_rootValue_adt_native made = 11)
check "saturated consumer left identity" (System.Object.ReferenceEquals(AdtPilot_leftChild_adt_native made, red))
let partial = apply AdtConsumer_partial (box red)
let make11 = apply partial (box 11)
let first = apply make11 (box red) |> unbox<AdtPilot_Tree>
let second = apply make11 AdtPilot_E |> unbox<AdtPilot_Tree>
check "partial constructor reusable first" (AdtPilot_depth_adt_native first = 2)
check "partial constructor reusable second" (AdtPilot_depth_adt_native second = 2)
check "partial constructor retained identity" (System.Object.ReferenceEquals(AdtPilot_leftChild_adt_native second, red))
for value in [System.Int32.MinValue; -1; 0; 7; System.Int32.MaxValue] do
    for fn in [AdtConsumer_throughGeneric; AdtConsumer_throughPartial] do
        let tree = apply fn (box value) |> unbox<AdtPilot_Tree>
        check "constructor as polymorphic value Int" (AdtPilot_rootValue_adt_native tree = value)
        check "consumer projection native Int" (unbox<int> (apply AdtConsumer_rootValue (box tree)) = value)
        check "constructor as polymorphic value depth" (AdtPilot_depth_adt_native tree = 1)
check "consumer left projection identity" (System.Object.ReferenceEquals(apply AdtConsumer_leftChild (box made), red))
check "consumer Color projection" (unbox<AdtPilot_Color> (apply AdtConsumer_rootColor (box made)) = AdtPilot_B_adt_native)
let black value = AdtPilot_T_adt_native AdtPilot_B_adt_native AdtPilot_E_adt_native value AdtPilot_E_adt_native
let deep = AdtPilot_T_adt_native AdtPilot_B_adt_native red 11 (black 99)
check "nested color/tree/Int pattern hit" (unbox<int> (apply AdtConsumer_deepPattern (box deep)) = 99)
check "nested Int pattern miss" (unbox<int> (apply AdtConsumer_deepPattern (box (AdtPilot_T_adt_native AdtPilot_B_adt_native (leaf 8) 11 (black 99)))) = 0)
check "nested color pattern miss" (unbox<int> (apply AdtConsumer_deepPattern (box (AdtPilot_T_adt_native AdtPilot_R_adt_native red 11 (black 99)))) = 0)
check "nested tree pattern empty miss" (unbox<int> (apply AdtConsumer_deepPattern AdtPilot_E) = 0)
check "named child projection identity" (System.Object.ReferenceEquals(apply AdtConsumer_namedChild (box deep), red))
check "named child fallback" (unbox<AdtPilot_Tree> (apply AdtConsumer_namedChild AdtPilot_E) = AdtPilot_E_adt_native)
let shared = apply AdtConsumer_shared (box red) |> unbox<AdtPilot_Tree>
match shared with
| AdtPilot_Tusd_Ctor(_, left, _, right) ->
    check "shared children identity" (System.Object.ReferenceEquals(left, right))
    check "shared input identity" (System.Object.ReferenceEquals(left, red))
| _ -> failwith "shared consumer value was not a node"
check "producer native depth accepts consumer result" (AdtPilot_depth_adt_native shared = 2)
check "consumer calls producer wrapper" (unbox<int> (apply AdtConsumer_roundTrip (box red)) = 2)
check "input retains original value" (AdtPilot_rootValue_adt_native red = 7)
let boxed = apply AdtConsumer_wrap (box made)
check "local boxed ADT retains native child identity" (System.Object.ReferenceEquals(apply AdtConsumer_unwrap boxed, made))
check "local boxed ADT retains boxed Int" (unbox<int> (apply AdtConsumer_boxedValue boxed) = 42)
check "boxed outer and native inner pattern hit" (unbox<int> (apply AdtConsumer_boxedPattern boxed) = 7)
check "boxed outer and native inner pattern miss" (unbox<int> (apply AdtConsumer_boxedPattern (apply AdtConsumer_wrap (box red))) = 0)
match unbox<AdtConsumer_ConsumerBox> boxed with
| AdtConsumer_ConsumerBoxusd_Ctor(child, value) ->
    check "local boxed constructor retains object fields" ((child :? AdtPilot_Tree) && (value :? int))
events.Clear()
let ordered = apply AdtConsumer_orderedConstruction (box red) |> unbox<AdtPilot_Tree>
check "saturated arguments evaluate left to right once" (List.ofSeq events = [1; 2; 3; 4])
check "ordered construction native depth" (AdtPilot_depth_adt_native ordered = 2)
events.Clear()
let orderedPartial = apply AdtConsumer_orderedPartial (box red)
check "supplied partial arguments evaluate eagerly" (List.ofSeq events = [1; 2])
let reusedPartial = apply orderedPartial (box 17)
check "partial retains arguments without reevaluation" (List.ofSeq events = [1; 2])
let orderedFirst = apply reusedPartial (box red) |> unbox<AdtPilot_Tree>
let orderedSecond = apply reusedPartial AdtPilot_E |> unbox<AdtPilot_Tree>
check "repeated partial application keeps captured argument evaluations" (List.ofSeq events = [1; 2])
check "partial first result retains native Int" (AdtPilot_rootValue_adt_native orderedFirst = 17)
check "partial second result retains native Int" (AdtPilot_rootValue_adt_native orderedSecond = 17)
check "partial result retains left input identity" (System.Object.ReferenceEquals(AdtPilot_leftChild_adt_native orderedSecond, red))
printfn "adt-interop runtime: %d checks passed" checks
