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
                let regexStr = "^\\s*let\\s+(?:rec\\s+)?(?:``)?" + funcName + "(?:``)?(?=$|[\\s(=])([^=]*)=";
                let regex = new RegExp(regexStr, "m");
                let match = indentedContent.match(regex);
                if (match) {
                    let argsStr = match[1].trim();
                    if (argsStr !== "") {
                        let cleanedArgs = argsStr.replace(/\([^)]+\)/g, "arg").trim();
                        cleanedArgs = cleanedArgs.replace(/:.*/, "").trim();
                        if (cleanedArgs !== "") {
                           arity = cleanedArgs.split(/\s+/).length;
                        }
                    }
                }
                
                let wrapper = `let ${exportName} = `;
                if (arity === 0) {
                    wrapper += `box (${safeModuleName}.\`\`${funcName}\`\`)`;
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

export const appendCsFfiWrappersImpl = function(moduleName) {
    return function(requiredForeigns) {
        return function(content) {
            let newLines = [];
            let safeModuleName = moduleName.replace(/\./g, "_") + "_CS_FFI";
            let csharpNamespace = moduleName;
            
            for (let i = 0; i < requiredForeigns.length; i++) {
                const funcName = requiredForeigns[i];
                let exportName = moduleName.replace(/\./g, "_") + "_" + funcName;
                
                let arity = 0;
                // e.g. public static object getGreetingImpl(string name)
                // C# methods might be PascalCase, so we check case-insensitively for the name
                let regexStr = "public\\s+static\\s+(?:[\\w\\[\\]<>]+(?:\\s*\\?)?\\s+)?(" + funcName + ")\\s*\\((.*?)\\)";
                let regex = new RegExp(regexStr, "im");
                let match = content.match(regex);
                
                let actualMethodName = funcName;
                if (match) {
                    actualMethodName = match[1]; // Get the actual casing used in C#
                    let argsStr = match[2].trim();
                    if (argsStr !== "") {
                        arity = argsStr.split(",").length;
                    }
                }
                
                let wrapper = `let ${exportName} = `;
                if (arity === 0) {
                    wrapper += `box (${csharpNamespace}.FFI.${actualMethodName}())`;
                } else {
                    for (let j = 0; j < arity; j++) {
                        wrapper += `box (fun (arg${j}: obj) -> `;
                    }
                    wrapper += `box (${csharpNamespace}.FFI.${actualMethodName}(`;
                    for (let j = 0; j < arity; j++) {
                        if (j > 0) wrapper += ", ";
                        wrapper += `unbox arg${j}`;
                    }
                    wrapper += "))";
                    for (let j = 0; j < arity; j++) {
                        wrapper += `)`;
                    }
                }
                newLines.push(wrapper);
            }
            return newLines.join("\n") + "\n";
        };
    };
};

// Local `let rec` groups are emitted with marker characters instead of a fixed
// indentation: F#'s offside rule needs their continuation lines deeper than the
// enclosing `let`, which is only known once the surrounding text is laid out.
const REC_START = "\u0001";
const REC_INDENT = "\u0002";
const REC_END = "\u0003";

export const normalizeRecIndentImpl = (text) => {
    let out = "";
    let col = 0;
    const stack = [];
    for (let i = 0; i < text.length; i++) {
        const ch = text[i];
        if (ch === REC_START) {
            stack.push(col);
        } else if (ch === REC_END) {
            stack.pop();
        } else if (ch === REC_INDENT) {
            const base = stack.length > 0 ? stack[stack.length - 1] : col;
            const indent = " ".repeat(base + 2);
            out += indent;
            col += indent.length;
        } else {
            out += ch;
            if (ch === "\n") col = 0; else col++;
        }
    }
    return out;
};
