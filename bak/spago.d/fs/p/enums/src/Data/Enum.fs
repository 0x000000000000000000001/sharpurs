let fromCharCode = box (fun (c: obj) -> box (char (unbox<int> c)))
let toCharCode = box (fun (c: obj) -> box (int (unbox<char> c)))
