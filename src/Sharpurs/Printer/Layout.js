// F#'s offside rule requires recursive continuation lines to be deeper than
// the enclosing `let`. Its column is only known after the file is assembled.
export const recStart = "\u0001";
export const recIndent = "\u0002";
export const recEnd = "\u0003";

export const normalizeRecIndent = (text) => {
    let output = "";
    let column = 0;
    const enclosingColumns = [];
    for (const character of text) {
        if (character === recStart) {
            enclosingColumns.push(column);
        } else if (character === recEnd) {
            enclosingColumns.pop();
        } else if (character === recIndent) {
            const base = enclosingColumns.length > 0
                ? enclosingColumns[enclosingColumns.length - 1]
                : column;
            const indent = " ".repeat(base + 2);
            output += indent;
            column += indent.length;
        } else {
            output += character;
            column = character === "\n" ? 0 : column + character.length;
        }
    }
    return output;
};
