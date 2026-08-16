const fs = require('fs');
let code = fs.readFileSync('src/Sharpurs/CodeGen.purs', 'utf8');

let newTranslateBind = `translateBind :: Map String Int -> Maybe String -> Bind Ann -> Array FsDecl
translateBind adtCtors currentMod = case _ of
  NonRec b -> expandBind adtCtors currentMod b
  Rec bindings -> 
    let
      recArities = Array.foldl (\\acc (Binding _ (Ident n) e) -> 
          let ext = extractArgs e
          in Map.insert (sanitizeName (fromMaybe "" currentMod <> "_" <> n)) (Array.length ext.args) acc
        ) Map.empty bindings

      makeRec (Binding _ (Ident n) e) isFirst =
        let ext = extractArgs e
            arity = Array.length ext.args
            prefix = case currentMod of
                  Just m -> m <> "_"
                  Nothing -> ""
            sName = sanitizeName (prefix <> n)
        in if arity > 0 then
             let 
               argStrs = String.joinWith " " (map (\\a -> "(" <> a <> ": obj)") ext.args)
               keyword = if isFirst then "let rec " else " and "
               bodyStr = printExprInline (translateExpr adtCtors recArities currentMod ext.body)
               curriedArgs = String.joinWith " " (map (\\a -> "(fun (" <> a <> ": obj) -> ") ext.args)
               closes = String.joinWith "" (map (\\_ -> ")") ext.args)
               wrapper = sName <> " = box " <> curriedArgs <> sName <> "_tco " <> String.joinWith " " ext.args <> closes
             in keyword <> sName <> "_tco " <> argStrs <> " : obj = " <> bodyStr <> "\\n" <> "and " <> wrapper <> "\\n"
           else
             let keyword = if isFirst then "let rec " else " and "
             in keyword <> sName <> " : obj = " <> printExprInline (translateExpr adtCtors recArities currentMod e) <> "\\n"
      
      recStr = Array.mapWithIndex (\\i bd -> makeRec bd (i == 0)) bindings
    in [FsRaw (String.joinWith "" recStr)]
  where
    expandBind :: Map String Int -> Maybe String -> Binding Ann -> Array FsDecl
    expandBind adtCtors currentMod (Binding _ (Ident name) val) =
      let prefix = case currentMod of
            Just m -> m <> "_"
            Nothing -> ""
      in [FsLet (sanitizeName (prefix <> name)) [] (translateExpr adtCtors Map.empty currentMod val)]`;

code = code.replace(/translateBind :: Map String Int -> Maybe String -> Bind Ann -> Array FsDecl[\s\S]*?in \[FsLet \(sanitizeName \(prefix <> name\)\) \[\] \(translateExpr adtCtors Map\.empty currentMod val\)]/, newTranslateBind);

fs.writeFileSync('src/Sharpurs/CodeGen.purs', code);
