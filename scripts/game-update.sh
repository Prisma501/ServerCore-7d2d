#!/usr/bin/env bash
# Bumps game-version.json to the newest build on its Steam branch (public or
# latest_experimental). RELEASING.md describes when and how it runs.
#
#   scripts/game-update.sh           update game-version.json if Steam has a newer build
#   scripts/game-update.sh --check   only report whether it does
#
# Build ids and manifests come from api.steamcmd.net. The Assembly-CSharp.dll
# sha256 comes from downloading that one DLL with the new manifest, which also
# proves the manifest is real. The `version` label isn't on Steam: it is set to
# "X.Y.Z (build N)" when a vX.Y.Z Steam branch has the same build, otherwise
# "unknown (build N)", and needs a human to set the real label.
#
# In CI (GITHUB_OUTPUT set) it writes changed, old_buildid, new_buildid and version as outputs.

# shellcheck disable=SC2016 # jq filters use jq's own $variables
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DATA="${ROOT}/_data"
PIN="${ROOT}/game-version.json"
STEAM_API="https://api.steamcmd.net/v1/info"

# shellcheck source=scripts/lib/depotdownloader.sh
source "${ROOT}/scripts/lib/depotdownloader.sh"

command -v jq >/dev/null || { echo "error: jq is required" >&2; exit 1; }

check_only=false
case "${1:-}" in
  "") ;;
  --check) check_only=true ;;
  *) sed -n '2,14p' "${BASH_SOURCE[0]}" | sed 's/^# \{0,1\}//' >&2; exit 2 ;;
esac

output() {
  echo "$1=$2"
  if [[ -n "${GITHUB_OUTPUT:-}" ]]; then
    echo "$1=$2" >> "${GITHUB_OUTPUT}"
  fi
}

pin() { jq -er ".$1" "${PIN}"; }
APP="$(pin app)"
DEPOT="$(pin depot)"
BRANCH="$(pin branch)"
OLD_BUILDID="$(pin buildid)"

INFO="$(curl -fsSL "${STEAM_API}/${APP}")"
steam() { jq -er --arg app "${APP}" --arg depot "${DEPOT}" --arg branch "${BRANCH}" ".data[\$app].depots | $1" <<< "${INFO}"; }
NEW_BUILDID="$(steam '.branches[$branch].buildid')"
NEW_MANIFEST="$(steam '.[$depot].manifests[$branch].gid')"

output old_buildid "${OLD_BUILDID}"
output new_buildid "${NEW_BUILDID}"

if [[ "${NEW_BUILDID}" == "${OLD_BUILDID}" ]]; then
  echo "game-version.json is up to date: ${BRANCH} is at build ${OLD_BUILDID}"
  output changed false
  exit 0
fi
output changed true

# Steam keeps a vX.Y.Z branch per stable release; its name is the best label we can get.
LABEL="$(steam '.branches[$branch].buildid as $build
  | [.branches | to_entries[] | select(.key | test("^v[0-9.]+$")) | select(.value.buildid == $build) | .key[1:]]
  | first // "unknown"')"
VERSION="${LABEL} (build ${NEW_BUILDID})"
output version "${VERSION}"

echo "Steam's ${BRANCH} branch moved from build ${OLD_BUILDID} to ${NEW_BUILDID} (manifest ${NEW_MANIFEST})"
if [[ "${check_only}" == true ]]; then
  exit 0
fi

ensure_depotdownloader

DEPOT_DIR="${DATA}/game-update-depot"
FILELIST="${DATA}/game-update-filelist.txt"
mkdir -p "${DATA}"
echo 'regex:^7DaysToDieServer_Data/Managed/Assembly-CSharp\.dll$' > "${FILELIST}"
rm -rf "${DEPOT_DIR}"
depotdownloader \
  -app "${APP}" -depot "${DEPOT}" -manifest "${NEW_MANIFEST}" \
  -filelist "${FILELIST}" -dir "${DEPOT_DIR}" -validate
SHA256="$(sha256sum "${DEPOT_DIR}/7DaysToDieServer_Data/Managed/Assembly-CSharp.dll" | cut -d' ' -f1)"

updated="$(jq --indent 2 \
  --arg version "${VERSION}" --argjson buildid "${NEW_BUILDID}" \
  --arg manifest "${NEW_MANIFEST}" --arg sha "${SHA256}" \
  '.version = $version | .buildid = $buildid | .manifest = $manifest | .assemblyCSharpSha256 = $sha' \
  "${PIN}")"
echo "${updated}" > "${PIN}"
echo "Updated game-version.json to build ${NEW_BUILDID}; set its version label by hand"
