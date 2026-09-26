# Releasing ServerCore

Maintainers only. See [ADR 2](adr/0002-branch-per-game-version.md) for why stable and experimental builds live on different branches.

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

There are no automated tests yet, so every release gets this manual check on a dedicated server running the target game version.

1. Install the build in `Mods/ServerCore/`, with an existing PrismaCore data set in place if you have one (settings, claims, waypoints).
2. Start the server and read the log:
   - the mod loader lists `ServerCore` (a branch build shows the placeholder version in `ModInfo.xml`; only the release stamps the real one)
   - the `[PrismaCore]` startup lines appear (settings and strings loaded, databases opened) and there are no exceptions from `ServerCore`
3. In the server console:
   - `version` lists ServerCore
   - `pc-help` prints the command list
   - `ccc` prints its help; create and remove one test claim with it
4. Open the Web UI at `http://<server>:<WebUI_Port>/` (by default the web dashboard port + 1, set in `PrismaCoreSettings.xml`), log in with Steam and check the map and your claims load.
5. Stop the server and check `PrismaCoreSettings.xml` still has your settings.

## Bumping the game version

1. Update `version`, `buildid`, `manifest` and `assemblyCSharpSha256` in `game-version.json` (SteamDB lists build ids and manifests for app 294420, depot 294422). Fetch once with the new manifest to get the `Assembly-CSharp.dll` sha256.
2. Run `./scripts/fetch-game-refs.sh` and build. Fix each compile break in its own commit, quoting the game API that changed.
3. Update the supported versions tables in `README.md` and `website/src/content/docs/project/compatibility.md`.

## Bumping the Web UI

1. Update `tag` and `sha256` in `web-ui-version.json` to the new [CPM-claim-creator](https://github.com/CatalysmsServerManager/CPM-claim-creator/releases) release (`gh release view <tag> -R CatalysmsServerManager/CPM-claim-creator --json assets` shows the digest).
2. Run `./scripts/fetch-web-ui.sh`, build, and check the Web UI in the smoke test.

## Steam certificates

`mod-assets/steam-rootca.cer` and `mod-assets/steam-intermediate.cer` are copied to the mod folder. The Web UI's Steam login uses them only when the server's own trust store rejects `steamcommunity.com`'s certificate chain. They hold the chain Steam serves today: ISRG Root YR (self-signed) and Let's Encrypt YR2, from [letsencrypt.org/certificates](https://letsencrypt.org/certificates/).

If Steam logins fail with certificate errors in the log (start the server with `-debugopenid` for details), check which intermediate Steam serves and update the files:

```bash
echo | openssl s_client -connect steamcommunity.com:443 -servername steamcommunity.com -showcerts
```
