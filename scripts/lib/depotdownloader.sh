# shellcheck shell=bash
# Pinned, hash-verified DepotDownloader, shared by the scripts that pull game files from Steam.
# Source it after setting DATA; call ensure_depotdownloader, then depotdownloader <args>.

DD_VERSION="3.4.0"
DD_URL="https://github.com/SteamRE/DepotDownloader/releases/download/DepotDownloader_${DD_VERSION}/DepotDownloader-linux-x64.zip"
DD_SHA256="a999dec66b4850fc961bd50366696d23c2d0fad7b18790e6a5647b2f19097a53"
DD_DIR="${DATA}/tools/depotdownloader-${DD_VERSION}"
DD_BIN="${DD_DIR}/DepotDownloader"

ensure_depotdownloader() {
  for cmd in curl unzip sha256sum; do
    command -v "${cmd}" >/dev/null || { echo "error: ${cmd} is required" >&2; exit 1; }
  done
  if [[ "$(uname -s)-$(uname -m)" != "Linux-x86_64" ]]; then
    echo "error: this script runs on Linux x86_64 only (use WSL or a Linux container elsewhere)" >&2
    exit 1
  fi
  [[ -x "${DD_BIN}" ]] && return 0

  echo "Downloading DepotDownloader ${DD_VERSION}"
  mkdir -p "${DD_DIR}"
  curl -fsSL "${DD_URL}" -o "${DD_DIR}/dd.zip"
  echo "${DD_SHA256}  ${DD_DIR}/dd.zip" | sha256sum --check --status \
    || { echo "error: DepotDownloader archive hash mismatch" >&2; rm -rf "${DD_DIR}"; exit 1; }
  unzip -q -o "${DD_DIR}/dd.zip" -d "${DD_DIR}"
  rm "${DD_DIR}/dd.zip"
  chmod +x "${DD_BIN}"
}

# Invariant globalization spares DepotDownloader an ICU dependency.
depotdownloader() {
  DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 "${DD_BIN}" "$@"
}
