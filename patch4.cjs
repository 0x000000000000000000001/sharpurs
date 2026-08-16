const fs = require('fs');
let code = fs.readFileSync('src/Sharpurs/CodeGen.purs', 'utf8');

code = code.replace(
  'in keyword <> sName <> "_tco " <> argStrs <> " : obj = " <> bodyStr <> "\\n" <> "and " <> wrapper <> "\\n"',
  'in keyword <> sName <> "_tco " <> argStrs <> " : obj = box (" <> bodyStr <> ")\\n" <> "and " <> wrapper <> "\\n"'
);
code = code.replace(
  'in keyword <> sName <> " : obj = " <> printExprInline (translateExpr adtCtors recArities currentMod e) <> "\\n"',
  'in keyword <> sName <> " : obj = box (" <> printExprInline (translateExpr adtCtors recArities currentMod e) <> ")\\n"'
);

fs.writeFileSync('src/Sharpurs/CodeGen.purs', code);
