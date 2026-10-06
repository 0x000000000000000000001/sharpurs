// Run the test suite of every sibling sharpurs-* repository, mirroring the
// gopurs/tools/modtest-runner.mjs layout. Each sibling script keeps its own
// build, caches and cleanup; -c rebuilds the compiler once beforehand.
import { accessSync, constants, readdirSync, statSync } from "node:fs";
import { basename, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { runProcess } from "../scripts/support/process.mjs";

const root = fileURLToPath(new URL("../", import.meta.url));
const parent = resolve(root, "..");
const prefix = "sharpurs-";

class UsageError extends Error {}

class Interrupted extends Error {
  constructor(signal) {
    super(`Interrupted by ${signal}`);
    this.exitCode = signal === "SIGINT" ? 130 : 143;
  }
}

function parseOptions(args) {
  const options = { targets: [], clean: false, list: false, all: false, resume: null, help: false };
  for (let index = 0; index < args.length; index++) {
    const arg = args[index];
    if (arg === "--") {
      options.targets.push(...args.slice(index + 1));
      break;
    }
    if (arg === "-c" || arg === "--clean") options.clean = true;
    else if (arg === "--list") options.list = true;
    else if (arg === "--all") options.all = true;
    else if (arg === "--help" || arg === "-h") options.help = true;
    else if (arg === "--skip-before") {
      const value = args[++index];
      if (!value || value.startsWith("-")) throw new UsageError("--skip-before needs a name.");
      options.resume = value;
    } else if (arg.startsWith("--skip-before=") || arg.startsWith("skip_before=")) {
      options.resume = arg.slice(arg.indexOf("=") + 1);
      if (!options.resume) throw new UsageError("--skip-before needs a name.");
    } else if (arg.startsWith("-")) throw new UsageError(`Unknown option: ${arg}`);
    else options.targets.push(arg);
  }
  if (options.all && options.targets.length) throw new UsageError("Use --all or explicit names, not both.");
  return options;
}

function isFile(path) {
  return statSync(path, { throwIfNoEntry: false })?.isFile() ?? false;
}

function isExecutable(path) {
  if (!isFile(path)) return false;
  try {
    accessSync(path, constants.X_OK);
    return true;
  } catch {
    return false;
  }
}

function selectModules(options) {
  const available = readdirSync(parent, { withFileTypes: true })
    .filter(entry => entry.isDirectory() && entry.name.startsWith(prefix))
    .map(entry => entry.name)
    .filter(name => isExecutable(join(parent, name, "bin", "test")))
    .sort();
  const normalize = name => (name.startsWith(prefix) ? name : prefix + name);
  const candidates = options.targets.length
    ? options.targets.map(target => {
        const name = normalize(target);
        if (!available.includes(name)) throw new UsageError(`No executable module test: ${target}`);
        return name;
      })
    : available;
  let selected = [...new Set(candidates)];
  if (options.resume) {
    const index = selected.findIndex(name => name === normalize(options.resume));
    if (index < 0) throw new UsageError(`Resume target not found in selection: ${options.resume}`);
    selected = selected.slice(index);
  }
  if (!selected.length) throw new UsageError("No module tests selected.");
  return selected.map(name => join(parent, name));
}

const controller = new AbortController();
const interrupt = () => controller.abort("SIGINT");
const terminate = () => controller.abort("SIGTERM");
process.on("SIGINT", interrupt);
process.on("SIGTERM", terminate);

async function run(label, command, args, { cwd, env = process.env }) {
  if (controller.signal.aborted) throw new Interrupted(controller.signal.reason);
  console.log(`   [${label}] ${command} ${args.join(" ")}`);
  const result = await runProcess(command, args, { cwd, env, signal: controller.signal });
  if (result.cancelled) throw new Interrupted(controller.signal.reason);
  if (result.error) throw new Error(`${label}: ${result.error}`);
  if (!result.ok) throw new Error(`${label} failed (${result.signal ?? "exit " + result.code})`);
  console.log(`   [PASS] ${label}`);
  return result;
}

// Rebuild once through the same tool selection and commands as npm/prepare.
async function buildCompiler() {
  await run("build-sharpurs", process.execPath, [join(root, "scripts/build.mjs")], { cwd: root });
}

const usage = `Usage: ./bin/modtest [modules...] [--all] [--skip-before NAME] [--list] [-c]
With no module names, run all sibling sharpurs-* repositories with an executable bin/test.
Resume is inclusive; names accept either arrays or sharpurs-arrays.
-c rebuilds the compiler once, before starting the selected module scripts.
Each sibling script still controls its own build, caches, and cleanup.`;

try {
  const options = parseOptions(process.argv.slice(2));
  if (options.help) {
    console.log(usage);
  } else {
    const modules = selectModules(options);
    for (const directory of modules) console.log(basename(directory));
    if (!options.list) {
      console.log(`Selected ${modules.length} modules (${options.resume ? "resume" : options.targets.length ? "explicit selection" : "all"}).`);
      if (options.clean) await buildCompiler();
      for (const directory of modules) await run(basename(directory), "./bin/test", [], { cwd: directory });
      console.log(`Summary: ${modules.length} modules passed.`);
    }
  }
} catch (error) {
  console.error(`[FAILED] ${error.message}`);
  process.exitCode = error instanceof Interrupted ? error.exitCode : error instanceof UsageError ? 2 : 1;
} finally {
  process.off("SIGINT", interrupt);
  process.off("SIGTERM", terminate);
}
