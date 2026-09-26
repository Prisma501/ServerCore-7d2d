# Releasing ServerCore

Maintainers only. `main` builds for the stable game version and `experimental` for the experimental one, each with its own `game-version.json`. Fixes land on `main` first and are merged into `experimental`.

## Tags and branches

| Tag | Branch | Game version | GitHub release |
|---|---|---|---|
| `vX.Y.Z` (for example `v3.0.0`) | `main` | stable (`game-version.json` on `main`) | normal release |
| `vX.Y.Z-exp.N` (for example `v3.1.0-exp.1`) | `experimental` | experimental (`game-version.json` on `experimental`) | pre-release |

Pushing a tag runs `.github/workflows/release.yml`: it builds the mod, stamps the version into `ModInfo.xml`, zips `Mods/ServerCore/` as `ServerCore-<version>.zip` and publishes a GitHub release with the matching `CHANGELOG.md` section as notes. Any tag containing `-` becomes a pre-release. The release fails if the tagged commit isn't on the branch in the table: `main` for plain tags, `experimental` for tags with a `-`.

## Cutting a release

1. Make sure CI is green on the branch you release from.
2. In `CHANGELOG.md`, move the `Unreleased` entries into a `## [X.Y.Z] - YYYY-MM-DD` section (the release fails without one) and add its link at the bottom.
3. Run the smoke test below on the CI build of that commit (the `ServerCore` artifact of the Build workflow).
4. Tag and push: `git tag vX.Y.Z && git push origin vX.Y.Z`.
5. Check the release page: the zip is attached, the notes are right, and experimental builds show the pre-release badge.

## Smoke test

Every release gets this check on a dedicated server running the target game version. Most of it is automated by `scripts/dev-server.sh`, which runs a local server in Docker (see [CONTRIBUTING.md](CONTRIBUTING.md#run-a-dev-server)):

```sh
./scripts/dev-server.sh stable install   # once per game build; experimental for the experimental branch
gh run download <run-id> --name ServerCore --dir _data/ci-artifact
./scripts/dev-server.sh stable test --from _data/ci-artifact
```

`<run-id>` is the Build workflow run for the release commit (`gh run list --workflow build.yml --commit <sha>`). `--from` also takes a release zip. Without `--from`, `test` builds the checked-out commit instead.

`test` deploys the build, boots the server and fails if any of these fail:

- the mod loader lists `ServerCore` (a branch build shows the placeholder version in `ModInfo.xml`; only the release stamps the real one)
- the `[PrismaCore]` startup lines appear, ending with `Started ClaimCreator`, and no exception mentions `ServerCore`
- in the console, `version` lists ServerCore, `pc-help` prints the command list, `help ccc` prints its help, and `ccc` adds, lists and removes a test claim
- the Web UI port serves the ClaimCreator page
- `PrismaCoreSettings.xml` is written, and is still valid after the server stops

Then check by hand:

1. Run `./scripts/dev-server.sh stable up`, open the Web UI at http://127.0.0.1:8285/, log in with Steam and check the map and your claims load.
2. If you have an existing PrismaCore data set, copy it in and check the server boots with it: the `PrismaCore*.xml` / `.txt` files and `ClaimCreator_permissions.xml` go in `_data/dev-server/stable/saves/Saves/`, and the world's databases (`*.db` and `PrismaCoreMap/`) go in `_data/dev-server/stable/saves/Saves/Pregen06k01/ServerCoreDev/`.
3. Run `./scripts/dev-server.sh stable down`.

## Bumping the game version

1. Update `version`, `buildid`, `manifest` and `assemblyCSharpSha256` in `game-version.json` (SteamDB lists build ids and manifests for app 294420, depot 294422). Fetch once with the new manifest to get the `Assembly-CSharp.dll` sha256.
2. Run `./scripts/fetch-game-refs.sh` and build. Fix each compile break in its own commit, quoting the game API that changed.
3. Update the supported versions tables in `README.md` and `docs/project/compatibility.md`.

## Bumping the Web UI

1. Update `tag` and `sha256` in `web-ui-version.json` to the new [CPM-claim-creator](https://github.com/CatalysmsServerManager/CPM-claim-creator/releases) release (`gh release view <tag> -R CatalysmsServerManager/CPM-claim-creator --json assets` shows the digest).
2. Run `./scripts/fetch-web-ui.sh`, build, and check the Web UI in the smoke test.

## Steam certificates

`mod-assets/steam-rootca.cer` and `mod-assets/steam-intermediate.cer` are copied to the mod folder. The Web UI's Steam login uses them only when the server's own trust store rejects `steamcommunity.com`'s certificate chain. They hold the chain Steam serves today: ISRG Root YR (self-signed) and Let's Encrypt YR2, from [letsencrypt.org/certificates](https://letsencrypt.org/certificates/).

If Steam logins fail with certificate errors in the log (start the server with `-debugopenid` for details), check which intermediate Steam serves and update the files:

```bash
echo | openssl s_client -connect steamcommunity.com:443 -servername steamcommunity.com -showcerts
```
