# ServerCore

ServerCore is the open-source continuation of the PrismaCore mod for 7 Days to Die dedicated servers. This glossary fixes the words we use when talking about the handover, the game versions we build for, and what we promise not to change.

## Language

**Baseline**:
The PrismaCore 2.5 source exactly as Prisma handed it over, recorded as the first source commit.
_Avoid_: Import, original, legacy code

**Compatibility surface**:
Everything a server operator, server manager or community module can observe from outside the mod: console command names and aliases, command output text, `[PrismaCore]` log lines, and the names and formats of settings, strings and database files.
_Avoid_: API, interface, public contract

**Rename**:
Changing the mod's own identifiers (product name, mod folder, assembly, project, namespaces) from PrismaCore to ServerCore without touching the **Compatibility surface**.
_Avoid_: Rebrand, migration

**Stable target**:
The game version on Steam's public branch that a ServerCore release is built against.
_Avoid_: Current version, prod

**Experimental target**:
The game version on Steam's experimental branch that a ServerCore pre-release is built against.
_Avoid_: Beta, next version

**Game references**:
The dedicated server's managed assemblies that the mod compiles against, fetched per target and never committed.
_Avoid_: Binaries, 7dtd-binaries, DLLs

**Web UI**:
The ClaimCreator map front end, built from the separate CPM-claim-creator repository and bundled into the mod folder at release time.
_Avoid_: ClaimCreator repo, map, dashboard

**Web UI runtime files**:
Files the mod needs on disk for the **Web UI** to work that are not part of the front-end build: the two Steam certificate files and any session templates.
_Avoid_: Assets, certs

## Relationships

- The **Baseline** is the first commit; every later change is a diff against it
- A **Rename** never changes the **Compatibility surface**
- Each release is built against exactly one **Stable target** or **Experimental target**
- The **Web UI** and the **Web UI runtime files** together make up the ClaimCreator part of the mod folder

## Example dialogue

> **Dev:** "The **Rename** is done, should I also rename `PrismaCoreSettings.xml`?"
> **Maintainer:** "No. File names are part of the **Compatibility surface**. The **Rename** only touches our own identifiers."

> **Dev:** "Can I commit the **Game references** so the build works offline?"
> **Maintainer:** "Never. They are Steam's files. The fetch script gets them per target."

## Flagged ambiguities

- "Rename everything" was used to mean both code identifiers and user-visible names. Resolved: **Rename** covers identifiers only; the **Compatibility surface** stays PrismaCore-named until a major version says otherwise.
