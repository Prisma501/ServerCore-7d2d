#!/usr/bin/env bash
# Fetches the Web UI (the ClaimCreator front end) pinned in web-ui-version.json
# into _data/web-ui/dist/, which the build copies to Mods/ServerCore/ClaimCreator/.
#
# The release asset is downloaded anonymously and checked against the pinned sha256.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DEST="${ROOT}/_data/web-ui"
PIN="${ROOT}/web-ui-version.json"

for cmd in curl unzip jq sha256sum; do
  command -v "${cmd}" >/dev/null || { echo "error: ${cmd} is required" >&2; exit 1; }
done

pin() { jq -er ".$1" "${PIN}"; }
REPOSITORY="$(pin repository)"
TAG="$(pin tag)"
ASSET="$(pin asset)"
SHA256="$(pin sha256)"
STAMP="${DEST}/.pinned"

if [[ -f "${STAMP}" && "$(cat "${STAMP}")" == "${SHA256}" && -f "${DEST}/dist/index.html" ]]; then
  echo "Web UI ${TAG} is already in ${DEST}/dist"
  exit 0
fi

echo "Fetching Web UI ${TAG} (${REPOSITORY}, ${ASSET})"
rm -rf "${DEST}"
mkdir -p "${DEST}"
curl -fsSL "https://github.com/${REPOSITORY}/releases/download/${TAG}/${ASSET}" -o "${DEST}/${ASSET}"
echo "${SHA256}  ${DEST}/${ASSET}" | sha256sum --check --status \
  || { echo "error: ${ASSET} doesn't match the sha256 in web-ui-version.json" >&2; rm -rf "${DEST}"; exit 1; }
unzip -q "${DEST}/${ASSET}" -d "${DEST}"
rm "${DEST}/${ASSET}"
[[ -f "${DEST}/dist/index.html" ]] || { echo "error: ${ASSET} has no dist/index.html" >&2; exit 1; }
echo "${SHA256}" > "${STAMP}"
echo "Web UI is in ${DEST}/dist ($(find "${DEST}/dist" -type f | wc -l) files)"
