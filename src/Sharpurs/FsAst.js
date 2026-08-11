export const escapeString = function (s) {
  let json = JSON.stringify(s);
  if (json.includes("\\ud")) {
    let chars = [];
    for (let i = 0; i < s.length; i++) {
      chars.push(s.charCodeAt(i));
    }
    return "(new System.String([| " + chars.map(c => "char " + c).join("; ") + " |]))";
  }
  return json;
};
