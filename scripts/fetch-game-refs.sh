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

DD_VERSION="3.4.0"
DD_URL="https://github.com/SteamRE/DepotDownloader/releases/download/DepotDownloader_${DD_VERSION}/DepotDownloader-linux-x64.zip"
DD_SHA256="a999dec66b4850fc961bd50366696d23c2d0fad7b18790e6a5647b2f19097a53"
DD_DIR="${DATA}/tools/depotdownloader-${DD_VERSION}"

for cmd in curl unzip jq sha256sum; do
  command -v "${cmd}" >/dev/null || { echo "error: ${cmd} is required" >&2; exit 1; }
done
if [[ "$(uname -s)-$(uname -m)" != "Linux-x86_64" ]]; then
  echo "error: this script runs on Linux x86_64 only (use WSL or a Linux container elsewhere)" >&2
  exit 1
fi

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

if [[ ! -x "${DD_DIR}/DepotDownloader" ]]; then
  echo "Downloading DepotDownloader ${DD_VERSION}"
  mkdir -p "${DD_DIR}"
  curl -fsSL "${DD_URL}" -o "${DD_DIR}/dd.zip"
  echo "${DD_SHA256}  ${DD_DIR}/dd.zip" | sha256sum --check --status \
    || { echo "error: DepotDownloader archive hash mismatch" >&2; rm -rf "${DD_DIR}"; exit 1; }
  unzip -q -o "${DD_DIR}/dd.zip" -d "${DD_DIR}"
  rm "${DD_DIR}/dd.zip"
  chmod +x "${DD_DIR}/DepotDownloader"
fi

DEPOT_DIR="${DATA}/depot"
FILELIST="${DATA}/filelist.txt"
cat > "${FILELIST}" <<'EOF'
regex:^7DaysToDieServer_Data/Managed/.*\.dll$
regex:^Mods/0_TFP_Harmony/.*\.dll$
EOF

echo "Fetching game references for $(pin version) (app ${APP}, depot ${DEPOT}, manifest ${MANIFEST})"
DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 "${DD_DIR}/DepotDownloader" \
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
