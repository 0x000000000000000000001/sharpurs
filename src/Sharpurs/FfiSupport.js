// Declaration recognition -> call shape -> boxed wrapper. Ffi.purs owns file
// selection and missing-file stubs; these pure functions only transform text.

/** @typedef {{ nativeName: string, parameterText: string }} Declaration */
/** @typedef {{ exportName: string, target: string, arity: number,
 *              convention: "curried" | "method" }} CallShape */

const modulePrefix = moduleName => moduleName.replace(/\./g, "_");

// The first matching module header is removed before nesting the source. The
// recognizer deliberately sees this same indented text as the emitted module.
function prepareFsharpSource(moduleName, content) {
    const body = content.replace(/^module\s+[a-zA-Z0-9_.]+[\s\n]*/m, "");
    const targetModule = modulePrefix(moduleName) + "_FFI";
    return {
        targetModule,
        content: "module " + targetModule + " =\n" +
            body.split("\n").map(line => "    " + line).join("\n"),
    };
}

/** @returns {Declaration | null} */
function recognizeFsharp(content, foreignName) {
    const pattern = "^\\s*let\\s+(?:rec\\s+)?(?:``)?" + foreignName +
        "(?:``)?(?=$|[\\s(=])([^=]*)=";
    const match = content.match(new RegExp(pattern, "m"));
    return match ? { nativeName: foreignName, parameterText: match[1].trim() } : null;
}

/** @returns {Declaration | null} */
function recognizeCsharp(content, foreignName) {
    const pattern = "public\\s+static\\s+(?:[\\w\\[\\]<>]+(?:\\s*\\?)?\\s+)?(" +
        foreignName + ")\\s*\\((.*?)\\)";
    const match = content.match(new RegExp(pattern, "im"));
    // Match case-insensitively, then retain the declaration's actual casing.
    return match ? { nativeName: match[1], parameterText: match[2].trim() } : null;
}

// A parenthesized F# parameter group counts as one argument. A return/value
// annotation ends the parameter text. Function-valued let bindings have arity 0.
function fsharpArity(declaration) {
    if (!declaration) return 0;
    const parameters = declaration.parameterText.replace(/\([^)]+\)/g, "arg").trim()
        .replace(/:.*/, "").trim();
    return parameters === "" ? 0 : parameters.split(/\s+/).length;
}

// The supported C# parameter list uses comma-separated, single-line parameters.
function csharpArity(declaration) {
    return !declaration || declaration.parameterText === "" ? 0 :
        declaration.parameterText.split(",").length;
}

/** @returns {CallShape} */
function fsharpCall(moduleName, targetModule, foreignName, declaration) {
    return {
        exportName: modulePrefix(moduleName) + "_" + foreignName,
        target: targetModule + ".``" + foreignName + "``",
        arity: fsharpArity(declaration),
        convention: "curried",
    };
}

/** @returns {CallShape} */
function csharpCall(moduleName, foreignName, declaration) {
    return {
        exportName: modulePrefix(moduleName) + "_" + foreignName,
        target: moduleName + ".FFI." + (declaration ? declaration.nativeName : foreignName),
        arity: csharpArity(declaration),
        convention: "method",
    };
}

/** @param {CallShape} call */
function renderWrapper(call) {
    const args = Array.from({ length: call.arity }, (_, index) => "arg" + index);
    const invocation = call.convention === "curried"
        ? call.target + args.map(arg => " (unbox " + arg + ")").join("")
        : call.target + "(" + args.map(arg => "unbox " + arg).join(", ") + ")";
    const closures = args.map(arg => "box (fun (" + arg + ": obj) -> ").join("");
    return "let " + call.exportName + " = " + closures + "box (" + invocation + ")" + ")".repeat(call.arity);
}

// With no recognized declaration, preserve the zero-arity convention: F# boxes
// the named value; C# invokes the named method. Missing files use Ffi's stubs.
export const appendFfiWrappersImpl = moduleName => requiredForeigns => content => {
    const source = prepareFsharpSource(moduleName, content);
    const wrappers = requiredForeigns.map(foreignName => renderWrapper(fsharpCall(
        moduleName, source.targetModule, foreignName, recognizeFsharp(source.content, foreignName),
    )));
    return source.content + "\n\n" + wrappers.join("\n") + "\n";
};

export const appendCsFfiWrappersImpl = moduleName => requiredForeigns => content => {
    const wrappers = requiredForeigns.map(foreignName => renderWrapper(csharpCall(
        moduleName, foreignName, recognizeCsharp(content, foreignName),
    )));
    return wrappers.join("\n") + "\n";
};
