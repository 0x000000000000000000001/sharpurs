const fs = require('fs');

// Patch CodeGen.purs
let code = fs.readFileSync('src/Sharpurs/CodeGen.purs', 'utf8');

code = code.replace(
  /translateLit :: Map String Int -> Maybe String -> Literal \(Expr Ann\) -> FsExpr/g,
  `translateLit :: Map String Int -> Map String Int -> Maybe String -> Literal (Expr Ann) -> FsExpr`
);
code = code.replace(/translateLit adtCtors currentMod lit =/g, "translateLit adtCtors localEnv currentMod lit =");

code = code.replace(
  /translateCaseAlternative :: Map String Int -> Maybe String -> CaseAlternative Ann -> Array FsMatchCase/g,
  `translateCaseAlternative :: Map String Int -> Map String Int -> Maybe String -> CaseAlternative Ann -> Array FsMatchCase`
);
code = code.replace(/translateCaseAlternative adtCtors currentMod/g, "translateCaseAlternative adtCtors localEnv currentMod");

code = code.replace(
  /translateBinder :: Map String Int -> Maybe String -> Binder Ann -> FsPattern/g,
  `translateBinder :: Map String Int -> Map String Int -> Maybe String -> Binder Ann -> FsPattern`
);
code = code.replace(/translateBinder adtCtors currentMod/g, "translateBinder adtCtors localEnv currentMod");

code = code.replace(
  /translateLitBinder :: Map String Int -> Maybe String -> Literal \(Binder Ann\) -> FsPattern/g,
  `translateLitBinder :: Map String Int -> Map String Int -> Maybe String -> Literal (Binder Ann) -> FsPattern`
);
code = code.replace(/translateLitBinder adtCtors currentMod/g, "translateLitBinder adtCtors localEnv currentMod");

fs.writeFileSync('src/Sharpurs/CodeGen.purs', code);

// Patch Printer.purs
let printer = fs.readFileSync('src/Sharpurs/Printer.purs', 'utf8');

printer = printer.replace(
  /FsCtorApp name args ->/g,
  `FsDirectApp name args -> if Array.length args > 0 then "(box (" <> name <> " " <> String.joinWith " " (map (\\a -> "(unbox (" <> printExpr a <> "))") args) <> "))" else "(box " <> name <> ")"\n    FsCtorApp name args ->`
);

fs.writeFileSync('src/Sharpurs/Printer.purs', printer);
