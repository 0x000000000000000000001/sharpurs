# Post-H07: reliable fixture selection and recovery — 5 October 2026

**Validated.** `bin/test` validates its selection before preparing the shared
runner. The completed H01–H07 cycle remains at **100/100 points (100%)**.

## Reproduced defects

The original `./bin/test --skip-before=MissingFixture` returned exit **0** and
`Summary: 0 passed, 0 failed.` despite executing no fixture. Its range endpoints
were matched during execution without checking membership or order beforehand.

The same regression expectations also expose three related problems in the saved
runner: a blacklisted `--until` endpoint was bypassed by `continue`, a caller-relative
path became invalid after entering `tests/runner`, and an empty fixture inventory
attempted to copy/compile the literal `*.purs` after creating the workspace.

## Implementation

- [`bin/test`](../../bin/test) resolves the inventory and inclusive range before
  runner creation, locking, cache handling or a clean compiler build. Named helpers
  own selection diagnostics, unique bound lookup and blacklist membership.
- Empty, missing or ambiguous bounds, reversed ranges and selections with no
  runnable fixture return exit **1** with a diagnostic. Bounds refer to the selected
  inventory, including when the caller supplies an explicit file list.
- The range is sliced before exclusions are skipped, so a blacklisted endpoint
  still delimits execution. Bash's locale-dependent glob order and explicit file
  order are retained, as are the `skip_before=` and `until=` aliases and optional
  `.purs` suffixes.
- Caller-relative file paths are resolved before the runner changes directory.
  The regression uses a different caller cwd and a path containing spaces/Unicode.

## Verification

| Check | Result |
| --- | --- |
| Original missing-bound command | False success reproduced, exit 0 and zero executed fixtures |
| Same missing bound with corrected runner and `-c` | Exit 1 before build; full runner inventory, bytes, modes, links and mtimes identical |
| `node --test tests/tools.mjs` | **22/22 tests passed** |
| Four new tests against the saved original runner | **4/4 failed as expected**, identifying the defects above |
| Invalid selection cases | 14 cases cover empty/missing/ambiguous bounds, reversed ranges, bounds outside explicit selection, all-excluded ranges and `-c` |
| Valid isolated ranges | Six cases cover default order, both alias spellings, suffixes, explicit order and excluded start/end bounds |
| Relative path and empty inventory | Passed in isolated child processes |
| `bash -n bin/test` | Passed with macOS Bash 3.2.57 |

Two real runner invocations completed sequentially with the installed TAST
compiler, Sharpurs bundle and .NET SDK:

```bash
./bin/test --skip-before=PartialFunction --until=PartialTCO.purs
./bin/test NewtypeEff PartialFunction TCOMutRec PartialTCO \
  skip_before=NewtypeEff.purs until=TCOMutRec
```

The first ran **PartialFunction → PartialTCO**, with **2/2** successes. The second
ran **NewtypeEff → PartialFunction**, with **2/2** successes, then reported the
excluded **TCOMutRec** and stopped before PartialTCO. Both logs contain the expected
runtime `Done` output. These are **four executions of three distinct fixtures**.
The initially absent `.purmeta` remained absent after each run and both locks were
removed. The isolated tests separately check existing cache bytes and mtimes.

## Provenance and scope

The starting Sharpurs revision was
`6de433458d08d509859b3fa6d1e56be5e1e7ba82`. All **43 production-source hashes** match
the starting snapshot, and the installed compiler bundle retains SHA-256:

```text
5082ef568cd2d4c0f7831af3e264074d59063c4817ee7318d25a86f33897b5f0
```

This follow-up qualifies runner selection, recovery and the listed fixture
executions. The complete application and benchmark replay is documented in the
[H07 report](h07-2026-10-03.md); current bundle provenance is documented in the
[public-build follow-up](post-h07-build-2026-10-05.md).

## Evidence

The [machine-readable report](post-h07-fixture-selection-validation.json) records
the commands, outcomes, tools and hashes. The
[archive](../../../validation/post-h07-fixture-selection-2026-10-05.tar.gz) and its
[SHA-256 checksum](../../../validation/post-h07-fixture-selection-2026-10-05.sha256)
retain the original runner, validated sources/bundle, complete logs, actual Bash
inventory, runner/cache manifests and verification scripts. Each archive member's
bytes are checked after creation.
