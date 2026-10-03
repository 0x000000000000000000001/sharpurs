// Suite-owned F# snippets, assembled explicitly around generated code in JS.
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';

export function fillSlots(template, slots, label = 'F# fixture') {
  const used = new Set();
  const result = template.replace(/\{\{([^{}]+)\}\}/g, (_, name) => {
    assert.ok(Object.hasOwn(slots, name), `${label}: missing slot ${name}`);
    assert.ok(!used.has(name), `${label}: repeated slot ${name}`);
    assert.equal(typeof slots[name], 'string', `${label}: slot ${name} must be text`);
    used.add(name);
    return slots[name];
  });
  for (const name of Object.keys(slots)) assert.ok(used.has(name), `${label}: unused slot ${name}`);
  return result;
}

export async function fsharpFixture(path, slots = {}) {
  return fillSlots(await readFile(new URL(`../fixtures/${path}`, import.meta.url), 'utf8'), slots, path);
}
