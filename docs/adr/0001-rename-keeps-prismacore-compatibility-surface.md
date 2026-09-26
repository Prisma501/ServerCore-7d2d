# 1. The rename keeps the PrismaCore compatibility surface

Date: 2026-09-26

## Status

Accepted

## Context

ServerCore continues PrismaCore 2.5. Thousands of servers run PrismaCore, and server managers (Takaro, CSMM) and community modules talk to it through its console commands, their output, the `[PrismaCore]` log lines and its files: `PrismaCoreSettings.xml`, `PrismaCoreStrings.xml`, `PrismaCoreDonorSlots.xml`, `PrismaCorePermaDeath.xml` and the `PrismaCore*.db` databases.

The project needs its own name, and the source is full of PrismaCore: the assembly, the mod folder, namespaces, class names, the Harmony id, and 332 string literals.

## Decision

The rename covers the mod's own identifiers only: project, assembly (`ServerCore.dll`), mod folder (`Mods/ServerCore`), `ModInfo.xml`, namespaces (`ServerCore.*`), the `PrismaCoreSettings`/`PrismaCoreStrings` class names and the Harmony id (`io.takaro.servercore`).

Everything outside observers can see stays as it is, byte for byte: command names and aliases (including `pc-`), output text, `[PrismaCore]` log lines, file names, XML root and element names (including serialized fields like `PrismaCorePrefix`), buff names and the `sidprismacore` cookie. `CONTEXT.md` calls this the compatibility surface.

The rename ran on code only, never inside string literals or comments, and `scripts/compare-assemblies.sh --rename` proved that the only changed string literal is the Harmony id.

## Consequences

- A mod called ServerCore writes files and log lines called PrismaCore. That looks odd, and the docs explain it (FAQ, compatibility page).
- Servers swap `Mods/PrismaCore` for `Mods/ServerCore` and keep their data, settings and server manager setup.
- Changing any part of the compatibility surface needs a major version and a migration note in the changelog.
- New code must not reintroduce type names into persisted data (LiteDB `_type` discriminators, `xsi:type`), because the .NET names already differ from PrismaCore's.
