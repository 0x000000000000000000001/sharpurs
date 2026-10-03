
let mutable checks = 0
let check label condition = if not condition then failwith label else checks <- checks + 1
let call fn x y = sharpurs_apply (sharpurs_apply fn (box x)) (box y) |> unbox<bool>
let cases = [
{{CASES}}
]
for x, y, less, greater, custom in cases do
    let before = fallbackCalls
    check "less matches JS" (call IntCompare_less x y = less)
    check "greater matches JS" (call IntCompare_greater x y = greater)
    check "native calls bypass dictionary" (fallbackCalls = before)
    check "custom order matches JS" (call IntCompare_custom x y = custom)
    check "custom order uses its dictionary" (fallbackCalls = before + 1)
    check "annotated alias matches JS" (call IntCompare_annotated x y = less)
    check "generic Int dispatch" (call (sharpurs_apply IntCompare_genericLess Data_Ord_ordInt) x y = less)
    check "partial comparator" (unbox<bool> (sharpurs_apply IntCompare_partial (box y)) = (5 < y))
check "String fallback" (call IntCompare_stringLess "a" "b")
check "Number fallback" (call IntCompare_numberLess 1.25 1.5)
events.Clear()
check "ordered comparison result" (call IntCompare_ordered 3 7)
check "left-to-right once" (List.ofSeq events = [1; 2])
printfn "int-comparison runtime: %d checks passed" checks
