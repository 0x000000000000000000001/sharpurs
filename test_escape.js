import { escapeString } from "./src/Sharpurs/FsAst.js";
console.log(escapeString("\n"));
console.log(Buffer.from(escapeString("\n")).toString("hex"));
