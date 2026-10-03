
open Microsoft.FSharp.Reflection
let mutable checks = 0
let check label ok = if not ok then failwith label else checks <- checks + 1
let apply (fn: obj) (arg: obj) : obj = (unbox<obj -> obj> fn) arg
let publicDepth tree = unbox<int> (apply AdtPilot_depth (box tree))
let publicRoot tree = unbox<int> (apply AdtPilot_rootValue (box tree))
let nativeEmpty = AdtPilot_empty_adt_native
let nativeOne = AdtPilot_singleton_adt_native
let nativeAsymmetric = AdtPilot_asymmetric_adt_native
check "native empty" (AdtPilot_depth_adt_native nativeEmpty = 0)
check "native singleton" (AdtPilot_depth_adt_native nativeOne = 1)
check "native asymmetry" (AdtPilot_depth_adt_native nativeAsymmetric = 3)
check "public empty" (publicDepth nativeEmpty = 0)
check "public singleton" (publicDepth nativeOne = 1)
check "public asymmetry" (publicDepth nativeAsymmetric = 3)
check "empty root" (publicRoot nativeEmpty = 0)
check "maximum Int" (publicRoot nativeOne = System.Int32.MaxValue)
check "minimum Int" (AdtPilot_rootValue_adt_native (AdtPilot_leftChild_adt_native nativeAsymmetric) = System.Int32.MinValue)
check "empty color" (not (AdtPilot_isRed_adt_native (AdtPilot_rootColor_adt_native nativeEmpty)))
check "singleton color" (not (AdtPilot_isRed_adt_native (AdtPilot_rootColor_adt_native nativeOne)))
check "nested red" (AdtPilot_isRed_adt_native (AdtPilot_rootColor_adt_native (AdtPilot_leftChild_adt_native nativeAsymmetric)))
let makeRed = apply AdtPilot_singletonWith AdtPilot_R
let redA = unbox<AdtPilot_Tree> (apply makeRed (box 7))
let redB = unbox<AdtPilot_Tree> (apply makeRed (box -9))
check "reused partial constructor first" (publicRoot redA = 7)
check "reused partial constructor second" (publicRoot redB = -9)
check "partial constructor color" (AdtPilot_isRed_adt_native (AdtPilot_rootColor_adt_native redA))
check "partial constructor depth" (publicDepth redA = 1)
let publicT = apply AdtPilot_T AdtPilot_B
let withLeft = apply publicT (box redA)
let withValue = apply withLeft (box 7)
let shared = unbox<AdtPilot_Tree> (apply withValue (box redA))
let sharedOther = unbox<AdtPilot_Tree> (apply withValue (box redB))
check "public constructor uses native layout" (AdtPilot_depth_adt_native shared = 2)
check "shared subtree identity" (System.Object.ReferenceEquals(AdtPilot_leftChild_adt_native shared, redA))
check "shared subtree survives other construction" (System.Object.ReferenceEquals(AdtPilot_leftChild_adt_native sharedOther, redA))
check "shared value unchanged" (publicRoot redA = 7)
check "public projection preserves identity" (System.Object.ReferenceEquals(apply AdtPilot_leftChild (box shared), box redA))
check "public Color result" (unbox<bool> (apply AdtPilot_isRed (apply AdtPilot_rootColor (box redA))))
check "public singleton reuses native value" (System.Object.ReferenceEquals(AdtPilot_singleton, box nativeOne))
let mutable skewed = AdtPilot_E_adt_native
for index in 0 .. 999 do skewed <- AdtPilot_T_adt_native AdtPilot_R_adt_native skewed index AdtPilot_E_adt_native
check "native recursion at depth 1000" (AdtPilot_depth_adt_native skewed = 1000)
check "public recursion at depth 1000" (publicDepth skewed = 1000)
check "native recursive construction keeps Int" (publicRoot skewed = 999)
let treeCases = FSharpType.GetUnionCases(typeof<AdtPilot_Tree>)
let fields = (treeCases |> Array.find (fun item -> item.Name = "AdtPilot_Tusd_Ctor")).GetFields()
check "Tree has exactly two constructors" (treeCases.Length = 2)
check "T has exactly four native fields" (fields.Length = 4)
check "native color field" (fields.[0].PropertyType = typeof<AdtPilot_Color>)
check "native left field" (fields.[1].PropertyType = typeof<AdtPilot_Tree>)
check "native Int field" (fields.[2].PropertyType = typeof<int>)
check "native right field" (fields.[3].PropertyType = typeof<AdtPilot_Tree>)
printfn "adt-kernel runtime: %d checks passed" checks
