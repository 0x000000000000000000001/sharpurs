// Uses the current compiler's ADT constructors. Historical compiler oracles
// with a different runtime/dictionary keep their own explicit API adapters.
import assert from 'node:assert/strict';
import { join } from 'node:path';
import * as Aff from '../../output/Effect.Aff/index.js';
import * as Applicative from '../../output/Control.Applicative/index.js';
import { Left } from '../../output/Data.Either/index.js';
import { Cons } from '../../output/Data.List.Types/index.js';
import { Just, Nothing } from '../../output/Data.Maybe/index.js';
import * as Map from '../../output/Data.Map.Internal/index.js';
import * as Set from '../../output/Data.Set/index.js';
import * as C from '../../output/PureScript.Backend.Optimizer.CoreFn/index.js';
import * as App from '../../output/PureScript.Backend.Optimizer.App/index.js';
import * as Builder from '../../output/PureScript.Backend.Optimizer.Builder/index.js';
import * as Foreign from '../../output/PureScript.Backend.Optimizer.Semantics.Foreign/index.js';

export const runAff = action => new Promise((resolve, reject) => {
  Aff.runAff(result => () => result instanceof Left ? reject(result.value0) : resolve(result.value0))(action)();
});

export async function readCoreFn(directory) {
  const modules = await runAff(App.coreFnModulesFromOutput(join(directory, 'output')));
  const result = new globalThis.Map();
  for (let cursor = modules; cursor instanceof Cons; cursor = cursor.value1) result.set(cursor.value0.name, cursor.value0);
  return result;
}

// Builder writes .purmeta relative to cwd. Suites run as separate processes;
// keep this operation sequential within each process and restore cwd on error.
export async function optimizeCoreFn(directory, names) {
  const previous = process.cwd();
  process.chdir(directory);
  try {
    const pure = Applicative.pure(Aff.applicativeAff);
    const result = new globalThis.Map();
    await runAff(Builder.buildModules(Aff.monadEffectAff)({
      directives: await runAff(App.loadDirectives), rewriteLimit: 10000,
      analyzeCustom: _ => _ => Nothing.value,
      foreignSemantics: Map.filterKeys(C.ordQualified(C.ordIdent))(qualified => {
        const name = qualified.value0 instanceof Just ? qualified.value0.value0 : '';
        return !name.includes('Effect') && !name.includes('Control.Monad.ST');
      })(Foreign.coreForeignSemantics),
      traceIdents: Set.empty,
      onPrepareModule: _ => module => pure(module),
      onSkipModule: _ => _ => pure(Nothing.value),
      onCodegenModule: _ => core => backend => _ => {
        if (names.includes(backend.name)) {
          assert.ok(!result.has(backend.name), `Duplicate fixture callback: ${backend.name}`);
          result.set(backend.name, { core, backend });
        }
        return pure(undefined);
      },
    })(await runAff(App.coreFnModulesFromOutput(join(directory, 'output')))));
    for (const name of names) assert.ok(result.has(name), `Builder did not reach ${name}`);
    return result;
  } finally {
    process.chdir(previous);
  }
}
