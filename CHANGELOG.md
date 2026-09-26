# Changelog

All notable changes to ServerCore are documented here. The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project uses [semantic versioning](https://semver.org).

## [Unreleased]

## [3.0.0] - 2026-09-26

The first ServerCore release: PrismaCore 2.5 as open source, for game version 3.2.0 b10. It behaves like PrismaCore 2.5, apart from the fix below.

### Changed

- Renamed PrismaCore to ServerCore and released it as open source under the MIT licence. Console commands, aliases, command output, log lines and data files are unchanged.
- New installs now send server chat messages as "Lara", a nod to Prisma501, the author of PrismaCore. Existing servers keep their saved name; change it with `scn <name>`.

### Added

- The ClaimCreator web UI (v2.2.0) ships in the release zip, with Steam login certificates that match the chain Steam serves today.
- Documentation site at [gettakaro.github.io/ServerCore-7d2d](https://gettakaro.github.io/ServerCore-7d2d/).

### Fixed

- Other mods, such as the Takaro connector, now receive player deaths, kills and leaves, and the server log again shows the game's `GMSG: Player '…' died` and `left the game` lines. PrismaCore claimed every game message and stopped both. Turning a message off with a `GMSG_*_Enabled` setting still hides it.

## [2.5] - PrismaCore

The last release of PrismaCore by Prisma501, for game version 3.2.0 b10. This is the baseline ServerCore continues from.

[Unreleased]: https://github.com/gettakaro/ServerCore-7d2d/commits/main
[3.0.0]: https://github.com/gettakaro/ServerCore-7d2d/releases/tag/v3.0.0
[2.5]: https://gettakaro.github.io/ServerCore-7d2d/project/changelog/#prismacore-version-history
