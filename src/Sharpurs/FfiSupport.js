export const appendFfiWrappersImpl = function(moduleName) {
    return function(requiredForeigns) {
        return function(content) {
            content = content.replace(/^module\s+[a-zA-Z0-9_.]+[\s\n]*/m, '');
            let newLines = [];
            
            let safeModuleName = moduleName.replace(/\./g, "_") + "_FFI";
            let indentedContent = "module " + safeModuleName + " =\n" + 
                                  content.split("\n").map(line => "    " + line).join("\n");
            
            for (let i = 0; i < requiredForeigns.length; i++) {
                const funcName = requiredForeigns[i];
                let exportName = moduleName.replace(/\./g, "_") + "_" + funcName;
                
                let arity = 0;
                let regexStr = "^\\s*let\\s+(?:rec\\s+)?(?:``)?" + funcName + "(?:``)?\\s+(.*?)(?:=|:)";
                let regex = new RegExp(regexStr, "m");
                let match = indentedContent.match(regex);
                if (match) {
                    let argsStr = match[1].trim();
                    if (argsStr !== "") {
                        let cleanedArgs = argsStr.replace(/\([^)]+\)/g, "arg").trim();
                        if (cleanedArgs !== "") {
                           arity = cleanedArgs.split(/\s+/).length;
                        }
                    }
                }
                
                let wrapper = `let ${exportName} = `;
                if (arity === 0) {
                    wrapper += `box ${safeModuleName}.\`\`${funcName}\`\``;
                } else {
                    for (let j = 0; j < arity; j++) {
                        wrapper += `box (fun (arg${j}: obj) -> `;
                    }
                    wrapper += `box (${safeModuleName}.\`\`${funcName}\`\``;
                    for (let j = 0; j < arity; j++) {
                        wrapper += ` (unbox arg${j})`;
                    }
                    wrapper += ")";
                    for (let j = 0; j < arity; j++) {
                        wrapper += `)`;
                    }
                }
                newLines.push(wrapper);
            }
            return indentedContent + "\n\n" + newLines.join("\n") + "\n";
        };
    };
};
