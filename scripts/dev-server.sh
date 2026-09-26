#!/usr/bin/env bash
# Local 7D2D dedicated servers for smoke-testing ServerCore builds.
#
#   scripts/dev-server.sh <stable|experimental> <command>
#
# Commands:
#   install [--force]  download the game server (~17 GB) into _data/dev-server/<target>/
#   deploy [--from P]  build the mod (or take the build in folder or zip P, e.g. the CI
#                      artifact) and copy it into the server's Mods/ServerCore/
#   up | down | logs   start, stop and remove, or follow the server
#   smoke              run the RELEASING.md smoke checks against the running server, then stop it
#   test [--from P]    stop, deploy, start and smoke in one go
#   build-refs         (experimental only) compile against the installed experimental server
#
# stable installs the exact build pinned in game-version.json, and refuses to run (apart from
# down and logs) when that pin isn't for Steam's public branch. experimental installs the pin
# when it is for latest_experimental (the experimental git branch), and otherwise the current
# head of latest_experimental. installed.json records what was installed.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DATA="${ROOT}/_data"
PIN="${ROOT}/game-version.json"
COMPOSE_FILE="${ROOT}/dev-server/compose.yml"
SDK_IMAGE="mcr.microsoft.com/dotnet/sdk:8.0@sha256:78235e09001f52b6592c458ac010775ebac6725422e80cd0c1650590f67b2743"

# shellcheck source=scripts/lib/depotdownloader.sh
source "${ROOT}/scripts/lib/depotdownloader.sh"

usage() {
  sed -n '2,18p' "${BASH_SOURCE[0]}" | sed 's/^# \{0,1\}//' >&2
  exit 2
}

[[ $# -ge 2 ]] || usage
TARGET="$1"
COMMAND="$2"
shift 2

case "${TARGET}" in
  stable) SERVER_PORT=26910 WEBUI_PORT=8285 ;;
  experimental) SERVER_PORT=26920 WEBUI_PORT=8295 ;;
  *) usage ;;
esac

TARGET_DIR="${DATA}/dev-server/${TARGET}"
SERVERFILES="${TARGET_DIR}/serverfiles"
SAVES="${TARGET_DIR}/saves"
INSTALLED="${TARGET_DIR}/installed.json"
TELNET_PORT=8081
# Ports listen on loopback unless DEV_SERVER_BIND names another address, e.g. the host's
# LAN IP so a game client on another machine can join.
export DEV_SERVER_BIND="${DEV_SERVER_BIND:-127.0.0.1}"
READY_TIMEOUT="${READY_TIMEOUT:-900}"
CLAIM_ID="servercore-smoke"

# The image maps its server user to these, so files it writes into _data stay ours.
export PUID="${PUID:-$(id -u)}" PGID="${PGID:-$(id -g)}"

compose() { docker compose -f "${COMPOSE_FILE}" "$@"; }

pin_branch() { jq -er .branch "${PIN}"; }

# Whether the target installs the build pinned in game-version.json.
pinned() { [[ "${TARGET}" == stable || "$(pin_branch)" == latest_experimental ]]; }

# A stable server on another branch's pin would smoke-test a release on the wrong game build.
require_public_pin() {
  command -v jq >/dev/null || { echo "error: jq is required" >&2; exit 1; }
  local branch
  branch="$(pin_branch)"
  if [[ "${TARGET}" == stable && "${branch}" != public ]]; then
    echo "error: game-version.json pins Steam branch ${branch}, but stable runs the public branch: use scripts/dev-server.sh experimental" >&2
    exit 1
  fi
}

require_installed() {
  [[ -f "${INSTALLED}" ]] || { echo "error: ${TARGET} isn't installed: run scripts/dev-server.sh ${TARGET} install" >&2; exit 1; }
}

# Runs dotnet on the host, or in the SDK image when the host has none.
dotnet_run() {
  if command -v dotnet >/dev/null; then
    (cd "${ROOT}" && dotnet "$@")
    return
  fi
  mkdir -p "${DATA}/nuget"
  docker run --rm -u "$(id -u):$(id -g)" \
    -e HOME=/tmp -e DOTNET_CLI_TELEMETRY_OPTOUT=1 -e DOTNET_NOLOGO=1 -e NUGET_PACKAGES="${DATA}/nuget" \
    -v "${ROOT}:${ROOT}" -w "${ROOT}" \
    "${SDK_IMAGE}" dotnet "$@"
}

# --- install -------------------------------------------------------------------------------

# Sets APP DEPOT MANIFEST BUILDID VERSION for the target.
resolve_build() {
  APP="$(jq -er .app "${PIN}")"
  DEPOT="$(jq -er .depot "${PIN}")"
  if pinned; then
    MANIFEST="$(jq -er .manifest "${PIN}")"
    BUILDID="$(jq -er .buildid "${PIN}")"
    VERSION="$(jq -er .version "${PIN}")"
    return
  fi
  # No experimental pin on this branch: take the branch head from Steam's public app info and
  # download that exact manifest, so what installed.json records is what was installed.
  local info
  info="$(curl -fsS "https://api.steamcmd.net/v1/info/${APP}")"
  BUILDID="$(jq -er ".data.\"${APP}\".depots.branches.latest_experimental.buildid" <<<"${info}")"
  MANIFEST="$(jq -er ".data.\"${APP}\".depots.\"${DEPOT}\".manifests.latest_experimental.gid" <<<"${info}")"
  VERSION="latest_experimental"
}

render_config() {
  local rendered
  rendered="$(sed -e "s/@TARGET@/${TARGET}/" -e "s/@SERVER_PORT@/${SERVER_PORT}/" "${ROOT}/dev-server/serverconfig.xml")"
  # LinuxGSM starts the server with sdtdserver.xml; serverconfig.xml is kept in step so
  # anyone reading the game's own config file sees the same values.
  printf '%s\n' "${rendered}" > "${SERVERFILES}/sdtdserver.xml"
  printf '%s\n' "${rendered}" > "${SERVERFILES}/serverconfig.xml"
}

cmd_install() {
  local force=false
  [[ "${1:-}" == --force ]] && force=true

  local reuse=false
  if [[ -f "${INSTALLED}" && "${force}" == false ]]; then
    reuse=true
    if pinned && [[ "$(jq -r .manifest "${INSTALLED}")" != "$(jq -er .manifest "${PIN}")" ]]; then
      echo "The installed ${TARGET} build isn't the one pinned in game-version.json: updating it"
      reuse=false
    fi
  fi

  if [[ "${reuse}" == true ]]; then
    echo "${TARGET} is already installed (install --force downloads again):"
  else
    resolve_build
    ensure_depotdownloader
    # No marker while the files are in flux: a failed download must not pass for an install.
    rm -f "${INSTALLED}"
    mkdir -p "${SERVERFILES}" "${SAVES}" "${TARGET_DIR}/log"
    echo "Installing ${TARGET} ${VERSION} (app ${APP}, depot ${DEPOT}, build ${BUILDID}, manifest ${MANIFEST})"
    depotdownloader -app "${APP}" -depot "${DEPOT}" -manifest "${MANIFEST}" -dir "${SERVERFILES}" -validate

    local sha
    sha="$(sha256sum "${SERVERFILES}/7DaysToDieServer_Data/Managed/Assembly-CSharp.dll" | cut -d' ' -f1)"
    if pinned && [[ "${sha}" != "$(jq -er .assemblyCSharpSha256 "${PIN}")" ]]; then
      echo "error: Assembly-CSharp.dll doesn't match the sha256 in game-version.json" >&2
      exit 1
    fi

    # The image reinstalls the game with SteamCMD when this file is missing.
    echo "Installed by scripts/dev-server.sh. If this file is missing, the image reinstalls the server." \
      > "${SERVERFILES}/DONT_REMOVE.txt"
    jq -n --arg target "${TARGET}" --arg version "${VERSION}" --argjson buildid "${BUILDID}" \
      --arg manifest "${MANIFEST}" --arg sha "${sha}" \
      '{target: $target, version: $version, buildid: $buildid, manifest: $manifest, assemblyCSharpSha256: $sha}' \
      > "${INSTALLED}"
  fi
  jq . "${INSTALLED}"

  render_config
  compose pull "${TARGET}"
}

# --- deploy --------------------------------------------------------------------------------

# Prints the one folder under $1 that holds the mod: the root of a CI artifact, or
# Mods/ServerCore/ in a release zip.
find_mod_folder() {
  local found
  found="$(find "$1" -maxdepth 3 -name ServerCore.dll -printf '%h\n')"
  if [[ -z "${found}" || "$(wc -l <<<"${found}")" -ne 1 || ! -f "${found}/ModInfo.xml" ]]; then
    echo "error: expected exactly one folder with ServerCore.dll and ModInfo.xml in $1, found: ${found:-none}" >&2
    exit 1
  fi
  printf '%s\n' "${found}"
}

cmd_deploy() {
  require_installed
  local from="" mod
  if [[ "${1:-}" == --from ]]; then
    from="${2:?--from needs a folder or zip}"
  elif [[ $# -gt 0 ]]; then
    usage
  fi

  if [[ -z "${from}" ]]; then
    "${ROOT}/scripts/fetch-game-refs.sh"
    "${ROOT}/scripts/fetch-web-ui.sh"
    dotnet_run build ServerCore.csproj -c Release
    mod="${ROOT}/Mods/ServerCore"
  elif [[ -f "${from}" ]]; then
    local unpacked
    unpacked="$(mktemp -d)"
    # shellcheck disable=SC2064 # expand now: the local is gone by the time the trap runs
    trap "rm -rf '${unpacked}'" EXIT
    unzip -q "${from}" -d "${unpacked}"
    mod="$(find_mod_folder "${unpacked}")"
  else
    mod="$(find_mod_folder "${from}")"
  fi

  # The mod keeps its settings and databases in the save root, not in its own folder, so
  # the folder can mirror the build exactly.
  mkdir -p "${SERVERFILES}/Mods/ServerCore"
  rsync -a --delete "${mod}/" "${SERVERFILES}/Mods/ServerCore/"
  echo "Deployed ${from:-Mods/ServerCore/} to ${SERVERFILES}/Mods/ServerCore/"
}

# --- up / down / logs ----------------------------------------------------------------------

cmd_up() {
  require_installed
  render_config
  compose up -d "${TARGET}"
  echo "${TARGET} is starting: game on ${DEV_SERVER_BIND}:${SERVER_PORT}, Web UI on http://${DEV_SERVER_BIND}:${WEBUI_PORT}/"
}

cmd_down() {
  compose rm --stop --force "${TARGET}"
}

cmd_logs() {
  compose logs --follow "${TARGET}"
}

# --- smoke ---------------------------------------------------------------------------------

RESULTS=()
FAILURES=()

check() {
  local name="$1" status="$2" detail="${3:-}"
  RESULTS+=("$(printf '%-4s %s' "${status}" "${name}")")
  if [[ "${status}" == FAIL ]]; then
    FAILURES+=("--- ${name}" "${detail}")
  fi
}

server_log() { compose logs --no-color --no-log-prefix "${TARGET}"; }

container_running() { [[ -n "$(compose ps --status running --quiet "${TARGET}")" ]]; }

# Sends one console command over the loopback-only telnet and prints what comes back
# (which includes any log lines the server writes meanwhile).
console() {
  # shellcheck disable=SC2016 # expanded by the container shell
  compose exec -T "${TARGET}" bash -c \
    '{ sleep 1; printf "%s\n" "$1"; sleep 3; printf "exit\n"; } | nc -q 2 127.0.0.1 "$2"' _ "$1" "${TELNET_PORT}"
}

expect_console() {
  local name="$1" command="$2" pattern="$3" out
  out="$(console "${command}")"
  if grep -qF -- "${pattern}" <<<"${out}"; then
    check "${name}" PASS
  else
    check "${name}" FAIL "\`${command}\` didn't print \"${pattern}\". Output:"$'\n'"${out}"
  fi
}

wait_ready() {
  local deadline=$((SECONDS + READY_TIMEOUT))
  while ((SECONDS < deadline)); do
    if ! container_running; then
      check "server ready" FAIL "the container isn't running. Last log lines:"$'\n'"$(server_log | tail -n 40)"
      return 1
    fi
    if server_log | grep -q 'StartGame done'; then
      check "server ready" PASS
      return 0
    fi
    sleep 5
  done
  check "server ready" FAIL "no 'StartGame done' within ${READY_TIMEOUT}s. Last log lines:"$'\n'"$(server_log | tail -n 40)"
  return 1
}

# Prints each exception in the log whose message or stack trace mentions ServerCore.
servercore_exceptions() {
  awk '
    function flush() { if (block != "" && block ~ /ServerCore/) printf "%s\n", block; block = "" }
    / EXC |[A-Za-z]Exception(:|[[:space:]]|$)/ { flush(); block = $0; next }
    block != "" && /^[[:space:]]+(at |---)|^  at / { block = block "\n" $0; next }
    { flush() }
    END { flush() }
  '
}

report() {
  echo
  echo "Smoke results (${TARGET}):"
  printf '  %s\n' "${RESULTS[@]}"
  if ((${#FAILURES[@]})); then
    echo
    echo "Failures:"
    printf '%s\n' "${FAILURES[@]}"
    return 1
  fi
}

cmd_smoke() {
  require_installed
  local log out settings="${SAVES}/Saves/PrismaCoreSettings.xml"

  if ! wait_ready; then
    report
    return 1
  fi
  log="$(server_log)"

  if grep -qF "Initialized code in mod 'ServerCore'" <<<"${log}"; then
    check "mod loader loads ServerCore" PASS
  else
    check "mod loader loads ServerCore" FAIL "the mod loader didn't initialize ServerCore. [MODS] lines:"$'\n'"$(grep -F '[MODS]' <<<"${log}")"
  fi

  if grep -qF '[PrismaCore] Started ClaimCreator' <<<"${log}"; then
    check "[PrismaCore] startup lines" PASS
  else
    check "[PrismaCore] startup lines" FAIL "no '[PrismaCore] Started ClaimCreator'. [PrismaCore] lines:"$'\n'"$(grep -F 'PrismaCore' <<<"${log}")"
  fi

  expect_console "console: version lists ServerCore" "version" "ServerCore"
  expect_console "console: pc-help lists commands" "pc-help" "*** List of PrismaCore Mod Commands ***"
  expect_console "console: ccc help" "help ccc" "Add/Remove/List/Configure advanced claims"

  # A test claim far from spawn; any leftover from an aborted run is removed first.
  console "ccc remove ${CLAIM_ID}" >/dev/null
  expect_console "console: ccc add claim" "ccc add ${CLAIM_ID} 2000 2010 2010 2000 0" "Claim for steamid=${CLAIM_ID} added"
  expect_console "console: ccc list shows claim" "ccc list" "${CLAIM_ID}:"
  expect_console "console: ccc remove claim" "ccc remove ${CLAIM_ID}" "Claim for steamid=${CLAIM_ID} has been removed"

  local page
  page="$(mktemp)"
  if curl -fsSL "http://${DEV_SERVER_BIND}:${WEBUI_PORT}/" -o "${page}" 2>"${page}.err" \
    && cmp -s "${page}" "${SERVERFILES}/Mods/ServerCore/ClaimCreator/index.html"; then
    check "Web UI serves ClaimCreator index.html" PASS
  else
    check "Web UI serves ClaimCreator index.html" FAIL "GET http://${DEV_SERVER_BIND}:${WEBUI_PORT}/ returned:"$'\n'"$(cat "${page}.err"; head -c 2000 "${page}")"
  fi
  rm -f "${page}" "${page}.err"

  if [[ -f "${settings}" ]]; then
    check "PrismaCoreSettings.xml written" PASS
  else
    check "PrismaCoreSettings.xml written" FAIL "${settings} doesn't exist"
  fi

  # Scanned last, so exceptions raised by the console and Web UI checks count too.
  out="$(server_log | servercore_exceptions)"
  if [[ -z "${out}" ]]; then
    check "no ServerCore exceptions" PASS
  else
    check "no ServerCore exceptions" FAIL "${out}"
  fi

  echo "Stopping ${TARGET} to check the settings survive shutdown"
  compose stop "${TARGET}" >/dev/null
  if out="$(xmllint --noout "${settings}" 2>&1)"; then
    check "PrismaCoreSettings.xml intact after stop" PASS
  else
    check "PrismaCoreSettings.xml intact after stop" FAIL "${out}"
  fi

  report
}

cmd_test() {
  cmd_down
  cmd_deploy "$@"
  cmd_up
  cmd_smoke
}

# --- build-refs ----------------------------------------------------------------------------

cmd_build_refs() {
  [[ "${TARGET}" == experimental ]] || { echo "error: build-refs is for experimental only; stable builds with dotnet build" >&2; exit 2; }
  require_installed
  local refs="${TARGET_DIR}/refs" out="${TARGET_DIR}/build/" log="${TARGET_DIR}/build.log" status=0

  rm -rf "${refs}"
  mkdir -p "${refs}"
  cp "${SERVERFILES}"/7DaysToDieServer_Data/Managed/*.dll "${SERVERFILES}"/Mods/0_TFP_Harmony/*.dll "${refs}/"
  "${ROOT}/scripts/fetch-web-ui.sh"

  echo "Building against $(jq -r '"build \(.buildid), manifest \(.manifest)"' "${INSTALLED}")"
  dotnet_run build ServerCore.csproj -c Release -p:GameReferences="${refs}" -p:OutputPath="${out}" 2>&1 \
    | tee "${log}" || status=$?

  echo
  local errors
  # MSBuild prints each error twice; strip the project suffix so the copies collapse.
  errors="$(grep -E 'error CS[0-9]+' "${log}" | sed -E "s#^${ROOT}/##; s/ \[.*csproj\]$//" | sort -u || true)"
  if [[ -n "${errors}" ]]; then
    echo "$(wc -l <<<"${errors}") compile errors, $(grep -oE 'error CS[0-9]+: .*' <<<"${errors}" | sort -u | wc -l) distinct, against the experimental build (full log: ${log}):"
    printf '%s\n' "${errors}"
  elif ((status == 0)); then
    echo "ServerCore compiles against the experimental build: ${out}"
  fi
  return "${status}"
}

case "${COMMAND}" in
  down | logs) ;;
  *) require_public_pin ;;
esac

case "${COMMAND}" in
  install) cmd_install "$@" ;;
  deploy) cmd_deploy "$@" ;;
  up) cmd_up ;;
  down) cmd_down ;;
  logs) cmd_logs ;;
  smoke) cmd_smoke ;;
  test) cmd_test "$@" ;;
  build-refs) cmd_build_refs ;;
  *) usage ;;
esac
