import { findFfiFileImpl as f2 } from '../purescript-backend-optimizer/src/PureScript/Backend/Optimizer/FfiSupport.js';

// We must pass the correct extraSpagoDirs (which in Main.purs is just "bak/spago.d/fs/p")
let res = f2(".fs")(["bak/spago.d/fs/p"])(null)("Control.Extend")(".spago/p/control-6.0.0/src/Control/Extend.purs")();
console.log("Result:", res);
