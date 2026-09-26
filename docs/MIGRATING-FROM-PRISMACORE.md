# Migrating from PrismaCore to ServerCore

> [!NOTE]
> This is a draft. ServerCore 3.0.0 hasn't been released yet. The steps will be confirmed once it is.

ServerCore is the continuation of PrismaCore 2.5. It's built to replace it with nothing else to change: same commands, same output, same data files.

## Before you start

- **Game version.** ServerCore 3.0.0 targets game 3.3. PrismaCore 2.5 only works on 3.2.0 b10. Upgrade the game and swap the mod in the same maintenance window.
- **Other mods.** Anything that worked with PrismaCore 2.5 should work with ServerCore. The one exception is PrismaCore itself: never run both.

## Steps

1. **Stop the server.**
2. **Back up your PrismaCore data.** ServerCore keeps using the same files, but take a copy anyway. None of them are in the mod folder:
   - In the save game root (the `Saves` folder): `PrismaCoreSettings.xml`, `PrismaCoreStrings.xml`, `PrismaCoreDonorSlots.xml`, `PrismaCorePermaDeath.xml`
   - In each world's save folder: the `PrismaCore*.db` files (players, claims, waypoints, teleports on spawn, group colours, and vehicle and drone owners)
3. **Remove `Mods/PrismaCore`.**
4. **Install ServerCore**: extract `ServerCore-<version>.zip` so you have `Mods/ServerCore/`.
5. **Upgrade the game** to the supported version, if you haven't already.
6. **Start the server** and check the log for ServerCore loading. Run `pc-help` in the console to confirm the commands are there.

Your settings, claims, waypoints and other data are read from the same files as before.

## Server managers and modules

- **Takaro**: nothing to change. The 7 Days to Die server type with "Use PrismaCore/CPM" turned on keeps working, because the commands and log lines are unchanged.
- **CSMM**: nothing to change, for the same reason.
- **Community modules** that call PrismaCore commands keep working unchanged.

## FAQ

**Why are the files still called PrismaCore?**
Renaming them would break existing servers and the tools that read them. We renamed the product, not the interface. See the compatibility promise in the [README](../README.md#compatibility-promise).

**Can I go back to PrismaCore?**
Only on game 3.2.0 b10. Restore your backup and put `Mods/PrismaCore` back.

**Something doesn't work the way it did in PrismaCore.**
That's a bug. Please [open an issue](https://github.com/gettakaro/ServerCore-7d2d/issues/new/choose) with the command you ran and its output.
