#!/usr/bin/env bash
# Fetches the dedicated server assemblies ServerCore compiles against, for the
# game build pinned in game-version.json, into _data/7dtd-binaries/.
#
# Only the managed DLLs are downloaded (a few MB), anonymously, with a pinned and
# hash-verified DepotDownloader. The files are Steam's, not ours: never commit them.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DATA="${ROOT}/_data"
REFS="${DATA}/7dtd-binaries"
PIN="${ROOT}/game-version.json"

# shellcheck source=scripts/lib/depotdownloader.sh
source "${ROOT}/scripts/lib/depotdownloader.sh"

command -v jq >/dev/null || { echo "error: jq is required" >&2; exit 1; }

pin() { jq -er ".$1" "${PIN}"; }
APP="$(pin app)"
DEPOT="$(pin depot)"
MANIFEST="$(pin manifest)"
EXPECTED_SHA256="$(pin assemblyCSharpSha256)"

assembly_matches() {
  [[ -f "${REFS}/Assembly-CSharp.dll" ]] \
    && echo "${EXPECTED_SHA256}  ${REFS}/Assembly-CSharp.dll" | sha256sum --check --status
}

if assembly_matches; then
  echo "Game references for $(pin version) are already in ${REFS}"
  exit 0
fi

ensure_depotdownloader

DEPOT_DIR="${DATA}/depot"
FILELIST="${DATA}/filelist.txt"
cat > "${FILELIST}" <<'EOF'
regex:^7DaysToDieServer_Data/Managed/.*\.dll$
regex:^Mods/0_TFP_Harmony/.*\.dll$
EOF

echo "Fetching game references for $(pin version) (app ${APP}, depot ${DEPOT}, manifest ${MANIFEST})"
depotdownloader \
  -app "${APP}" -depot "${DEPOT}" -manifest "${MANIFEST}" \
  -filelist "${FILELIST}" -dir "${DEPOT_DIR}" -validate

rm -rf "${REFS}"
mkdir -p "${REFS}"
cp "${DEPOT_DIR}"/7DaysToDieServer_Data/Managed/*.dll "${REFS}/"
cp "${DEPOT_DIR}"/Mods/0_TFP_Harmony/*.dll "${REFS}/"

if ! assembly_matches; then
  echo "error: Assembly-CSharp.dll doesn't match the sha256 in game-version.json" >&2
  exit 1
fi
echo "Game references are in ${REFS} ($(find "${REFS}" -name '*.dll' | wc -l) DLLs)"
