---
title: Installation
description: How to install ServerCore on a 7 Days to Die dedicated server.
---

## Step-by-step guide

1. Stop your server.
2. If you run PrismaCore, remove `Mods/PrismaCore`. Don't run both mods at the same time: they register the same commands.
3. Download the latest `ServerCore-<version>.zip` from [Releases](https://github.com/gettakaro/ServerCore-7d2d/releases).
4. Extract the zip to a new folder, then copy that folder into your server's `Mods/` directory so you have `Mods/ServerCore/`. If the `Mods/` folder doesn't exist yet, you have to create it.

Your `Mods/` folder structure should look similar to this:

```
├── ServerCore
│   ├── Config
│   ├── ClaimCreator
│   ├── ServerCore.dll
│   ├── LiteDB.dll
│   ├── ModInfo.xml
│   ├── steam-intermediate.cer
│   └── steam-rootca.cer
```

:::caution
Make sure the ServerCore folder has the ModInfo.xml and ServerCore.dll file!

For Crossplay servers remove the Config folder. Console clients cannot connect if an xml mod is present!
:::

5. Restart your server. When upgrading ServerCore to a new version, make sure the server is off when replacing the dll!
6. Check if the mod is loaded by executing the `version` command on your server. If ServerCore shows up in this list, you're good to go!

Note that some hosting providers do not allow you to upload `.dll` files! In this case you will need to ask help from their customer support in order to install ServerCore.

Moving from PrismaCore? See the [migration guide](/ServerCore-7d2d/start-here/migrating-from-prismacore/).
