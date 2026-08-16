const fs = require('fs');
let code = fs.readFileSync('src/Sharpurs/CodeGen.purs', 'utf8');

let transStart = code.indexOf('translateExpr ::');
let letStart = code.indexOf('ExprLet _ binds body ->', transStart);
let letEnd = code.indexOf('ExprUpdate _ obj props ->', letStart);

let newLetCode = `ExprLet _ binds body -> 
    let
      -- First, compute the new environment with arities of all recursive bindings
      newEnv = Array.foldl (\\acc b -> case b of
        Rec bindings -> Array.foldl (\\acc2 (Binding _ (Ident n) e) -> 
          let ext = extractArgs e 
          in if Array.length ext.args > 0 then Map.insert (sanitizeName n) (Array.length ext.args) acc2 else acc2
        ) acc bindings
        _ -> acc
      ) localEnv binds

      bindStrs = Array.concatMap (\\b -> case b of
        NonRec (Binding _ (Ident n) e) -> ["let " <> sanitizeName n <> " = " <> printExprInline (translateExpr adtCtors newEnv currentMod e) <> " in "]
        Rec bindings -> 
          let
            indent = "                                                                                                                                                                                                        "
            makeLocalRec (Binding _ (Ident n) e) idx =
              let
                ext = extractArgs e
                sName = sanitizeName n
              in if Array.length ext.args > 0 then
                let
                  argStrs = String.joinWith " " (map (\\a -> "(" <> a <> ": obj)") ext.args)
                  bodyStr = printExprInline (translateExpr adtCtors newEnv currentMod ext.body)
                  curriedArgs = String.joinWith "" (map (\\a -> "(fun (" <> a <> ": obj) -> ") ext.args)
                  closes = String.joinWith "" (map (const ")") ext.args)
                  keyword = if idx == 0 then "\\n" <> indent <> "let rec " else "\\n" <> indent <> "and "
                  wrapper = sName <> " = box (" <> curriedArgs <> sName <> "_tco " <> String.joinWith " " ext.args <> closes <> ")"
                in [ keyword <> sName <> "_tco " <> argStrs <> " : obj = box (" <> bodyStr <> ") ", "\\n" <> indent <> "and " <> wrapper <> " " ]
              else
                let
                  keyword = if idx == 0 then "\\n" <> indent <> "let rec " else "\\n" <> indent <> "and "
                in [ keyword <> sName <> " : obj = box (" <> printExprInline (translateExpr adtCtors newEnv currentMod e) <> ") " ]
            recStrs = Array.concat (Array.mapWithIndex (\\i b -> makeLocalRec b i) bindings)
          in ["\\n" <> indent <> String.joinWith "" recStrs <> "\\n" <> indent <> "in\\n" <> indent]
      ) binds
      
      bodyTerm = printExprInline (translateExpr adtCtors newEnv currentMod body) <> "\\n                                                                                                                                                                                                        )"
    in FsIdent ("(" <> String.joinWith "" bindStrs <> bodyTerm)
  `;

code = code.substring(0, letStart) + newLetCode + code.substring(letEnd);
fs.writeFileSync('src/Sharpurs/CodeGen.purs', code);
