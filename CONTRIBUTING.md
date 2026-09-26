# Contributing to ServerCore

Thanks for helping keep ServerCore alive. Every contribution counts: bug reports, testing on your server, docs fixes and code.

## Ways to help

- **Report bugs** with the [bug report template](https://github.com/gettakaro/ServerCore-7d2d/issues/new/choose). The game version, ServerCore version and console output matter most.
- **Test new game versions.** When a new 7D2D build comes out, reports from real servers are the fastest way to find what broke.
- **Propose features** in [Discussions](https://github.com/gettakaro/ServerCore-7d2d/discussions) first, so we can agree on the shape before anyone writes code.
- **Send pull requests.** Issues labelled `good first issue` are small and well-defined. `help wanted` means maintainers would welcome a PR.

## The compatibility rule

Server managers (Takaro, CSMM) and community modules call ServerCore commands and parse their output. Changing a string can silently break a module on thousands of servers. So, within a major version:

- Don't rename or remove console commands or aliases, including the `pc-` forms.
- Don't change command output text that tools may parse, even to fix a typo.
- Don't change log line formats, including the `[PrismaCore]` prefixed lines.
- Don't rename data or config files (`PrismaCore*.db`, `PrismaCoreSettings.xml`, `PrismaCoreStrings.xml`) or change their format incompatibly.

Adding new commands, new options and new output lines is fine. If a change to the above is really needed, open an issue first. It goes into the next major version with a deprecation period.

## Building locally

You need:
- the [.NET SDK](https://dotnet.microsoft.com/download) (8 or later)
- Linux x86_64 (on Windows, use WSL) with `curl`, `unzip`, `jq` and `sha256sum`

ServerCore compiles against the dedicated server's own assemblies. We never commit those to the repository. Fetch them from Steam instead:

```sh
./scripts/fetch-game-refs.sh
./scripts/fetch-web-ui.sh
```

The first downloads only the DLLs needed to compile (a few MB, not the whole server) into `_data/7dtd-binaries/`, anonymously, for the game build pinned in [`game-version.json`](game-version.json). The second downloads the ClaimCreator web UI release pinned in [`web-ui-version.json`](web-ui-version.json) into `_data/web-ui/`. Then build:

```sh
dotnet build -c Release
```

The build output lands in `Mods/ServerCore/`, ready to copy into a server.

When you need another game assembly, add it to the `GameReference` list in `ServerCore.csproj`. That points it at `_data/7dtd-binaries/`, so CI finds it too, and sets `Private="false"`, so the build never copies Steam's DLLs into `Mods/ServerCore/`. CI fails the build if one ends up there anyway.

```xml
<GameReference Include="Assembly-CSharp" />
```

## Testing your change

There are no automated in-game tests yet. Before opening a PR:

1. Run the build on a local dedicated server on the pinned game version (see [Run a dev server](#run-a-dev-server)).
2. Run the commands you changed, and check their output and the server log.
3. Say in the PR what you tested and on which game version.

### Run a dev server

`scripts/dev-server.sh` runs a 7D2D dedicated server in Docker, with your build in it. You need Docker with Compose, `rsync` and `xmllint`, about 17 GB of disk space per game version, and 8 GB of free RAM. The .NET SDK is optional here: without it the script builds in the SDK container.

```sh
./scripts/dev-server.sh stable install   # once: downloads the server pinned in game-version.json
./scripts/dev-server.sh stable test      # builds, deploys, boots and runs the smoke checks
```

`test --from <folder or zip>` tests a build you already have, such as a CI artifact, instead of building. `test` stops the server when the smoke checks are done. Run `up` to start it again, `logs` to follow the log and `down` to stop it. Telnet only listens inside the container, and the Web UI is on http://127.0.0.1:8285/. Replace `stable` with `experimental` for the head of Steam's `latest_experimental` branch (Web UI on port 8295), and use `experimental build-refs` to compile against it. To join from a game client on another machine, set `DEV_SERVER_BIND` to the host's LAN IP when you run `up` or `test`; by default the ports only listen on 127.0.0.1. Run one server at a time. The servers and their saves live in `_data/dev-server/`.

## Docs

The docs are Markdown files in [`docs/`](docs/). The site that publishes them lives in `website/` (Astro and Starlight), which reads `docs/` through the `website/src/content/docs` symlink, and is published to [gettakaro.github.io/ServerCore-7d2d](https://gettakaro.github.io/ServerCore-7d2d/). To preview it you need Node.js 22.12 or later:

```sh
cd website
npm install
npm run dev
```

If your change affects how ServerCore behaves, update the docs in the same PR. The changelog page on the site is generated from `CHANGELOG.md`, so don't edit `docs/project/changelog.md`.

## Pull requests

- Branch from `main`, and keep a PR to one change.
- Fill in the PR template.
- Add a line to `CHANGELOG.md` under `Unreleased`.
- Never commit game DLLs or other files from the game install.
- CI must pass: it fetches the game references and builds the mod.

A maintainer reviews every PR. We aim to respond within a week. If we haven't, ping us in the PR or on Discord.

## Bumping the game version

1. Update `buildid`, `manifest` and the `Assembly-CSharp.dll` sha256 in `game-version.json`.
2. Run `./scripts/fetch-game-refs.sh` and `dotnet build`, then fix what broke.
3. Test on a dev server and update the supported versions table in the README.

Stable game versions are built on `main`, the experimental game version on the `experimental` branch.

## Releases

Maintainers release by pushing a `vX.Y.Z` tag, or `vX.Y.Z-exp.N` on `experimental` for a pre-release. CI builds the mod, stamps the version into `ModInfo.xml`, zips `Mods/ServerCore/` and publishes a GitHub release with the changelog notes. [RELEASING.md](RELEASING.md) has the checklist and the smoke test. We use [semantic versioning](https://semver.org): patch for fixes, minor for new features, major for anything that breaks the compatibility rule.

## Code of conduct

Everyone taking part agrees to our [Code of Conduct](CODE_OF_CONDUCT.md).

## Licence

By contributing, you agree that your contributions are licensed under the [MIT licence](LICENSE).
