import fs from 'fs';
let code = fs.readFileSync('bin/sharpurs.js', 'utf8');
let match = code.match(/var coreForeignSemantics = \/\* @__PURE__ \*\/ \(function\(\) \{[\s\S]*?\}\)\(\);/);
console.log(match ? "Found coreForeignSemantics" : "Not found");
