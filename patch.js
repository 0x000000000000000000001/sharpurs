const fs = require('fs');
let code = fs.readFileSync('src/Sharpurs/CodeGen.purs', 'utf8');

// 1. Update printExprInline to handle FsDirectApp
code = code.replace(
  /FsCtorApp name args ->/g,
  `FsDirectApp name args -> if Array.length args > 0 then "(box (" <> name <> " " <> String.joinWith " " (map (\a -> "(unbox (" <> printExprInline a <> "))") args) <> "))" else "(box " <> name <> ")"\n  FsCtorApp name args ->`
);

// 2. Add extractArgs helper
code = code.replace(
  /translateExpr :: Map String Int -> Maybe String -> Expr Ann -> FsExpr/g,
  `extractArgs :: Expr Ann -> { args :: Array String, body :: Expr Ann }
extractArgs (ExprAbs _ (Ident arg) body) = 
  let next = extractArgs body
  in { args: Array.cons (sanitizeName arg) next.args, body: next.body }
extractArgs e = { args: [], body: e }

translateExpr :: Map String Int -> Map String Int -> Maybe String -> Expr Ann -> FsExpr`
);

// 3. Update translateExpr usages
code = code.replace(/translateExpr adtCtors currentMod/g, "translateExpr adtCtors localEnv currentMod");
code = code.replace(/translateExpr adtCtors Map.empty currentMod/g, "translateExpr adtCtors localEnv currentMod"); // in case

code = code.replace(
  /in \[FsLet \(sanitizeName \(prefix <> name\)\) \[\] \(translateExpr adtCtors localEnv currentMod val\)]/g,
  `in [FsLet (sanitizeName (prefix <> name)) [] (translateExpr adtCtors Map.empty currentMod val)]`
);

code = code.replace(
  /in { name: sanitizeName \(prefix <> n\), args: \[\], expr: translateExpr adtCtors localEnv currentMod e }\) binds\)]/g,
  `in { name: sanitizeName (prefix <> n), args: [], expr: translateExpr adtCtors Map.empty currentMod e }) binds)]`
);

// 4. Update ExprApp
let exprAppStr = `  ExprApp _ _ _ -> 
    let flat = flattenApp expr
    in case flat.fn of
      ExprConstructor _ _ (Ident name) _ -> 
        let fqName = case currentMod of
                       Just cmod -> String.replaceAll (Pattern ".") (Replacement "_") cmod <> "_" <> name
                       Nothing -> name
        in case Map.lookup (sanitizeName fqName) adtCtors of
           Just arity -> generateConstructorLambda (sanitizeName fqName) arity (map (translateExpr adtCtors localEnv currentMod) flat.args)
           Nothing -> FsCtorApp (sanitizeName fqName <> "usd_Ctor") (map (translateExpr adtCtors localEnv currentMod) flat.args)
      ExprVar _ qi ->
        let nameStr = unwrap (unQualified qi) in
        let fqName = case qi of
              Qualified (Just modName) _ -> String.replaceAll (Pattern ".") (Replacement "_") (unwrap modName) <> "_" <> nameStr
              Qualified Nothing _ -> case currentMod of
                                       Just cmod -> String.replaceAll (Pattern ".") (Replacement "_") cmod <> "_" <> nameStr
                                       Nothing -> nameStr
        in case Map.lookup (sanitizeName fqName) localEnv of
          Just arity ->
             if Array.length flat.args == arity then
                FsDirectApp (sanitizeName fqName <> "_tco") (map (translateExpr adtCtors localEnv currentMod) flat.args)
             else if Array.length flat.args > arity then
                let tcoArgs = Array.take arity flat.args
                    restArgs = Array.drop arity flat.args
                    baseCall = FsDirectApp (sanitizeName fqName <> "_tco") (map (translateExpr adtCtors localEnv currentMod) tcoArgs)
                in Array.foldl (\acc arg -> FsApp acc [translateExpr adtCtors localEnv currentMod arg]) baseCall restArgs
             else
                FsApp (translateExpr adtCtors localEnv currentMod flat.fn) (map (translateExpr adtCtors localEnv currentMod) flat.args)
          Nothing ->
            case Map.lookup (sanitizeName fqName) adtCtors of
              Just arity -> generateConstructorLambda (sanitizeName fqName) arity (map (translateExpr adtCtors localEnv currentMod) flat.args)
              Nothing -> FsApp (translateExpr adtCtors localEnv currentMod flat.fn) (map (translateExpr adtCtors localEnv currentMod) flat.args)
      _ -> FsApp (translateExpr adtCtors localEnv currentMod flat.fn) (map (translateExpr adtCtors localEnv currentMod) flat.args)`;

code = code.replace(/  ExprApp _ _ _ ->[\s\S]*?_ -> FsApp \(translateExpr adtCtors localEnv currentMod flat\.fn\) \(map \(translateExpr adtCtors localEnv currentMod\) flat\.args\)/, exprAppStr);

// 5. Update ExprLet
let exprLetStr = `  ExprLet _ binds body -> 
    let
      recArities = Array.foldl (\acc b -> case b of
        Rec bindings -> Array.foldl (\acc2 (Binding _ (Ident n) e) -> 
            let ext = extractArgs e
            in Map.insert (sanitizeName n) (Array.length ext.args) acc2
          ) acc bindings
        _ -> acc
      ) localEnv binds

      bindStrs = Array.concatMap (\b -> case b of
        NonRec (Binding _ (Ident n) e) -> ["let " <> sanitizeName n <> " = " <> printExprInline (translateExpr adtCtors recArities currentMod e) <> " in "]
        Rec bindings -> 
          let
            makeRec (Binding _ (Ident n) e) isFirst =
              let ext = extractArgs e
                  arity = Array.length ext.args
                  sName = sanitizeName n
              in if arity > 0 then
                   let 
                     argStrs = String.joinWith " " (map (\a -> "(" <> a <> ": obj)") ext.args)
                     keyword = if isFirst then "let rec " else " and "
                     bodyStr = printExprInline (translateExpr adtCtors recArities currentMod ext.body)
                     curriedArgs = String.joinWith " " (map (\a -> "(fun (" <> a <> ": obj) -> ") ext.args)
                     closes = String.joinWith "" (map (\_ -> ")") ext.args)
                     wrapper = sName <> " = box " <> curriedArgs <> sName <> "_tco " <> String.joinWith " " ext.args <> closes
                   in keyword <> sName <> "_tco " <> argStrs <> " : obj = " <> bodyStr <> "\\n" <> "and " <> wrapper <> "\\n"
                 else
                   let keyword = if isFirst then "let rec " else " and "
                   in keyword <> sName <> " : obj = " <> printExprInline (translateExpr adtCtors recArities currentMod e) <> "\\n"
            recStr = Array.mapWithIndex (\i bd -> makeRec bd (i == 0)) bindings
          in [String.joinWith "" recStr <> "in "]
      ) binds
    in FsIdent ("(" <> String.joinWith "" bindStrs <> printExprInline (translateExpr adtCtors recArities currentMod body) <> ")")`;

code = code.replace(/  ExprLet _ binds body ->[\s\S]*?in FsIdent \("\(" <> String\.joinWith "" bindStrs <> printExprInline \(translateExpr adtCtors localEnv currentMod body\) <> "\)"\)/, exprLetStr);

fs.writeFileSync('src/Sharpurs/CodeGen.purs', code);
