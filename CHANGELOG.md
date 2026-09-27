# Changelog

All notable changes to ServerCore are documented here. The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project uses [semantic versioning](https://semver.org).

## [Unreleased]

## [3.1.0-exp.1] - 2026-09-26

This build is for game version 3.3.0 EXP b14.

### Changed

- `cvc` (check vehicle content), `rii` (remove an item from a player), `wi` (wipe a player's inventory) and the RegionReset banned-item check now use the player save and container format of game version 3.3.
- Kills made while riding a vehicle are logged with `meleeHandPlayer` as the weapon, because game version 3.3 no longer swaps the inventory when a player gets on a vehicle.
- New installs now send server chat messages as "Lara", a nod to Prisma501, the author of PrismaCore. Existing servers keep their saved name; change it with `scn <name>`.

## [3.0.0]

The first ServerCore release: PrismaCore 2.5 as open source, for game version 3.2.0 b10. It behaves like PrismaCore 2.5, apart from the fix below.

### Changed

- Renamed PrismaCore to ServerCore and released it as open source under the MIT licence. Console commands, aliases, command output, log lines and data files are unchanged.

### Added

- The ClaimCreator web UI (v2.2.0) ships in the release zip, with Steam login certificates that match the chain Steam serves today.
- Documentation site at [gettakaro.github.io/ServerCore-7d2d](https://gettakaro.github.io/ServerCore-7d2d/).

### Fixed

- Other mods, such as the Takaro connector, now receive player deaths, kills and leaves, and the server log again shows the game's `GMSG: Player '…' died` and `left the game` lines. PrismaCore claimed every game message and stopped both. Turning a message off with a `GMSG_*_Enabled` setting still hides it.

## [2.5] - PrismaCore

The last release of PrismaCore by Prisma501, for game version 3.2.0 b10. This is the baseline ServerCore continues from.

[Unreleased]: https://github.com/gettakaro/ServerCore-7d2d/commits/experimental
[3.1.0-exp.1]: https://github.com/gettakaro/ServerCore-7d2d/releases/tag/v3.1.0-exp.1
[3.0.0]: https://github.com/gettakaro/ServerCore-7d2d/releases/tag/v3.0.0
[2.5]: https://gettakaro.github.io/ServerCore-7d2d/project/changelog/#prismacore-version-history
