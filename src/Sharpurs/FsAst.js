const codeUnits = s => Array.from({ length: s.length }, (_, i) => s.charCodeAt(i));

function stringRepresentation(s) {
  const literal = JSON.stringify(s);
  return { literal, units: literal.includes("\\ud") ? codeUnits(s) : null };
}

export const escapeString = s => {
  const { literal, units } = stringRepresentation(s);
  return units
    ? "(new System.String([| " + units.map(c => "char " + c).join("; ") + " |]))"
    : literal;
};

// Parameterized active patterns accept literal arguments, not `new String`.
// F# replaces lone-surrogate escapes in string literals with U+FFFD. Encode
// those arguments as hex code units for the matching runtime active pattern.
export const patternString = s => {
  const { literal, units } = stringRepresentation(s);
  return units
    ? { utf16: true, code: JSON.stringify(units.map(unit => unit.toString(16).padStart(4, "0")).join("")) }
    : { utf16: false, code: literal };
};
