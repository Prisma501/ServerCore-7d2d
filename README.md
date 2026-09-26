# ServerCore for 7 Days to Die

ServerCore is a server-side mod for 7 Days to Die dedicated servers. It adds about 100 admin console commands, advanced land claims, region resets, chat control, teleports, vehicle recall and more.

ServerCore is the continuation of **PrismaCore**, the mod Prisma501 built and maintained for over ten years. It started as CPM, later became the CSMM Patrons Mod, and then PrismaCore. Prisma retired from 7D2D modding in September 2026 and handed the mod over to the community. It's now open source under the MIT licence.

> [!IMPORTANT]
> **Status: source handover in progress.** The source code hasn't been imported yet. The first release, **ServerCore 3.0.0**, will be PrismaCore 2.5 running on game version 3.3. Until then, keep using PrismaCore 2.5 on game 3.2. Follow progress in [Issues](https://github.com/gettakaro/ServerCore-7d2d/issues) and on the [Takaro Discord](https://aka.takaro.io/discord).

## Who maintains this

The [Takaro](https://takaro.io) team maintains ServerCore, but it's a community project first. Everything happens in the open: the code, the issues, the roadmap and the release process. Anyone can open an issue, propose a feature or send a pull request, and we want regular contributors to become maintainers.

ServerCore doesn't depend on Takaro. It works the same with Takaro, CSMM, any other server manager or plain console commands.

PrismaCore was closed source for its whole life. We think a mod this many servers depend on should be open, so that anyone can see how it works, fix it and keep it alive.

## A word from Prisma

> _Placeholder: Prisma's handover statement goes here once it's published._

Thank you, Prisma, for ten years of work and for letting the community carry it on.

## Compatibility promise

ServerCore is a drop-in replacement for PrismaCore 2.5. We renamed the product, not the interface. In 3.x, these stay exactly as they were:

- **Console commands**: every name and alias, including the `pc-` prefixed forms
- **Command output**: success and error text, word for word, typos included. Server managers and community modules parse it.
- **Log lines**: including the `[PrismaCore]` prefixed lines that CSMM, Takaro and community modules read
- **Data and config files**: `PrismaCoreSettings.xml`, `PrismaCoreStrings.xml` and the `PrismaCore*.db` databases keep their names and formats, so your claims, waypoints and settings carry over.

If we ever change any of these, it happens in a new major version, with a deprecation period and a migration note.

## Supported game versions

| ServerCore | PrismaCore | Game version |
|---|---|---|
| 3.0.0 (planned) | – | 3.3 |
| – | 2.5 (last PrismaCore release) | 3.2.0 b10 |

## Installation

1. Stop your server.
2. If you run PrismaCore, remove `Mods/PrismaCore`. Don't run both mods at the same time: they register the same commands.
3. Download the latest `ServerCore-<version>.zip` from [Releases](https://github.com/gettakaro/ServerCore-7d2d/releases).
4. Extract it so you have `Mods/ServerCore/` in your server's install directory.
5. Start the server.

Moving from PrismaCore? See the [migration guide](docs/MIGRATING-FROM-PRISMACORE.md).

## Features

A short overview. Run `pc-help` in the server console for the full command list.

| Area | Highlights |
|---|---|
| Advanced claims | Jail, PvP arenas, hostile-free zones and reverse claims (`ccc`), `arrest` / `release`, claim friends, land claim tools |
| World resets | Reset chunks, regions, unclaimed regions and RWG prefabs, remove sleeper volumes |
| Builder tools | Copy, export, render, fill and replace blocks and prefabs, with undo |
| Blood moon | Spawner control (`bms*`), blood-moon-aware shutdown and timers, `isbloodmoon` |
| Spawning | Targeted hordes (`th`), scouts, multiple entities |
| Chat | Hide chat commands (`hccp`), mute, chat names and colours, group colours, messages with a custom sender (`say2`, `pm2`) |
| Teleports | Home (`teleh`), move to player, coordinates or waypoint (`mv`, `mvw`), offline moves, waypoints (`wpc`) |
| Vehicles | Recall your minibike, jeep, drone, gyrocopter, bicycle, motorcycle, blimp or helicopter, take ownership, check contents |
| Items | Give into the backpack (`giveplus`), remove items (`rii`), wipe inventories (`wi`), banned items |
| Players | Reset level, skill points or player data, set death count, permadeath, reserved slots, buffs and skills listing |
| Web | Map rendering and the ClaimCreator web dashboard |

## Contributing

Contributions are welcome, from bug reports to new features. Read [CONTRIBUTING.md](CONTRIBUTING.md) to get started. Issues labelled [`good first issue`](https://github.com/gettakaro/ServerCore-7d2d/labels/good%20first%20issue) are a good place to begin.

## Community and support

- **Questions and ideas**: [GitHub Discussions](https://github.com/gettakaro/ServerCore-7d2d/discussions)
- **Bugs and feature requests**: [GitHub Issues](https://github.com/gettakaro/ServerCore-7d2d/issues)
- **Chat**: the [Takaro Discord](https://aka.takaro.io/discord), where the PrismaCore community already lives
- **Security issues**: see [SECURITY.md](SECURITY.md)

## Licence

[MIT](LICENSE). Copyright Prisma501 and the ServerCore contributors.
