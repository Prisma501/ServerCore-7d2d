#!/usr/bin/env bash
# Cuts a ServerCore release. RELEASING.md describes the process.
#
#   scripts/release.sh prepare <version>   move the Unreleased changelog entries into <version>,
#                                          commit that on release/<version> and open a PR
#   scripts/release.sh publish <version>   once that PR is merged: wait for CI, smoke-test its
#                                          build, tag v<version> and check the GitHub release
#
# Plain versions (3.0.0) release from main, versions with a '-' (3.1.0-exp.1) from experimental.
# Both commands run from that branch. They need git and gh; publish also needs what
# scripts/dev-server.sh needs.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DATA="${ROOT}/_data"
PIN="${ROOT}/game-version.json"
ARTIFACT="${DATA}/ci-artifact"

# shellcheck source=scripts/lib/changelog.sh
source "${ROOT}/scripts/lib/changelog.sh"

usage() {
  sed -n '2,12p' "${BASH_SOURCE[0]}" | sed 's/^# \{0,1\}//' >&2
  exit 2
}

die() {
  echo "error: $*" >&2
  exit 1
}

[[ $# -eq 2 ]] || usage
COMMAND="$1"
VERSION="$2"

if [[ ! "${VERSION}" =~ ^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)(-[0-9A-Za-z]+(\.[0-9A-Za-z]+)*)?$ ]]; then
  die "${VERSION} isn't a version like 3.0.0 or 3.1.0-exp.1 (leave out the v)"
fi
TAG="v${VERSION}"
BRANCH=main
[[ "${VERSION}" == *-* ]] && BRANCH=experimental

cd "${ROOT}"
for cmd in git gh jq; do
  command -v "${cmd}" >/dev/null || die "${cmd} is required"
done

require_branch() {
  local current
  current="$(git symbolic-ref --short -q HEAD || true)"
  [[ "${current}" == "${BRANCH}" ]] \
    || die "${VERSION} releases from ${BRANCH}, but ${current:-a detached HEAD} is checked out"
}

require_clean() {
  [[ -z "$(git status --porcelain)" ]] || die "the working tree has changes: commit or stash them first"
}

# Asks until the answer is one of $2 (default y/n) and prints it.
ask() {
  local question="$1" choices="${2:-y/n}" answer
  while true; do
    read -r -p "${question} [${choices}] " answer || exit 1
    answer="${answer,,}"
    if [[ -n "${answer}" && "/${choices}/" == *"/${answer}/"* ]]; then
      printf '%s\n' "${answer}"
      return
    fi
  done
}

# --- prepare -------------------------------------------------------------------------------

cmd_prepare() {
  require_branch
  require_clean
  local release_branch="release/${VERSION}"
  if git rev-parse -q --verify "refs/heads/${release_branch}" >/dev/null; then
    die "branch ${release_branch} already exists"
  fi
  command -v python3 >/dev/null || die "python3 is required"

  python3 "${ROOT}/scripts/lib/changelog-release.py" CHANGELOG.md "${VERSION}" "$(date +%F)"
  git switch -c "${release_branch}"
  git add CHANGELOG.md
  git commit -m "Release ${VERSION}"
  git --no-pager show --stat --patch HEAD

  if [[ "$(ask "Push ${release_branch} and open a PR against ${BRANCH}?")" != y ]]; then
    echo "Not pushed. ${release_branch} is committed locally; push it and open the PR yourself, or delete it."
    return
  fi
  git push -u origin "${release_branch}"
  gh pr create --base "${BRANCH}" --head "${release_branch}" --title "Release ${VERSION}" \
    --body "Moves the Unreleased changelog entries into ${VERSION}. After merging, run \`scripts/release.sh publish ${VERSION}\` on ${BRANCH}."
}

# --- publish -------------------------------------------------------------------------------

# Sets TARGET: the dev server whose game build game-version.json pins.
resolve_target() {
  local pin_branch
  pin_branch="$(jq -er .branch "${PIN}")"
  case "${pin_branch}" in
    public) TARGET=stable ;;
    latest_experimental) TARGET=experimental ;;
    *) die "game-version.json pins Steam branch ${pin_branch}; only public and latest_experimental have a dev server" ;;
  esac
}

preflight() {
  require_branch
  require_clean
  git fetch --quiet origin "${BRANCH}"
  [[ "$(git rev-parse HEAD)" == "$(git rev-parse "origin/${BRANCH}")" ]] \
    || die "${BRANCH} isn't the same as origin/${BRANCH}: pull or push first"

  if git rev-parse -q --verify "refs/tags/${TAG}" >/dev/null; then
    die "tag ${TAG} already exists locally"
  fi
  if git ls-remote --exit-code --tags origin "refs/tags/${TAG}" >/dev/null; then
    die "tag ${TAG} already exists on origin"
  fi

  [[ -n "$(changelog_notes "${VERSION}")" ]] \
    || die "CHANGELOG.md has no [${VERSION}] section: run scripts/release.sh prepare ${VERSION} first"
  grep -qE "^## \[${VERSION//./\\.}\] - [0-9]{4}-[0-9]{2}-[0-9]{2}$" CHANGELOG.md \
    || die "the [${VERSION}] section in CHANGELOG.md has no date: run scripts/release.sh prepare ${VERSION} first"

  resolve_target
  SHA="$(git rev-parse HEAD)"
  echo "Releasing ${TAG} from ${BRANCH} at ${SHA}, smoke-tested on the ${TARGET} dev server"
}

# Sets RUN_ID to the Build workflow run for HEAD, once it has passed.
wait_for_ci() {
  local run
  run="$(gh run list --workflow build.yml --commit "${SHA}" --json databaseId,status,conclusion --jq '.[0] // empty')"
  [[ -n "${run}" ]] || die "no Build workflow run for ${SHA}: CI runs when the commit is pushed to ${BRANCH}"
  RUN_ID="$(jq -r .databaseId <<<"${run}")"

  echo "Waiting for Build run ${RUN_ID}"
  gh run watch "${RUN_ID}" --exit-status >/dev/null || die "Build run ${RUN_ID} didn't pass: gh run view ${RUN_ID}"
  [[ "$(gh run view "${RUN_ID}" --json conclusion --jq .conclusion)" == success ]] \
    || die "Build run ${RUN_ID} didn't pass: gh run view ${RUN_ID}"
  echo "Build run ${RUN_ID} passed"
}

smoke_test() {
  rm -rf "${ARTIFACT}"
  gh run download "${RUN_ID}" --name ServerCore --dir "${ARTIFACT}"
  # install is a no-op when the installed server already matches the pin, and updates it otherwise.
  "${ROOT}/scripts/dev-server.sh" "${TARGET}" install
  "${ROOT}/scripts/dev-server.sh" "${TARGET}" test --from "${ARTIFACT}"
}


push_tag() {
  echo
  echo "Tag:    ${TAG}"
  echo "Commit: $(git log -1 --format='%H %s')"
  echo "Branch: ${BRANCH}"
  local typed
  read -r -p "Type ${TAG} to tag this commit and push the tag: " typed
  [[ "${typed}" == "${TAG}" ]] || die "not tagged"

  git tag -a "${TAG}" -m "ServerCore ${VERSION}"
  git push origin "${TAG}"
}

check_release() {
  local run_id="" i
  echo "Waiting for the Release workflow run for ${TAG}"
  for ((i = 0; i < 60; i++)); do
    run_id="$(gh run list --workflow release.yml --branch "${TAG}" --json databaseId --jq '.[0].databaseId // empty')"
    [[ -n "${run_id}" ]] && break
    sleep 5
  done
  [[ -n "${run_id}" ]] || die "no Release workflow run for ${TAG} after 5 minutes: check GitHub Actions"
  gh run watch "${run_id}" --exit-status >/dev/null || die "Release run ${run_id} failed: gh run view ${run_id}"

  local release zip="ServerCore-${VERSION}.zip" dir="${DATA}/release/${TAG}" failures=()
  release="$(gh release view "${TAG}" --json assets,isPrerelease,url)"

  if jq -e --arg zip "${zip}" 'any(.assets[]; .name == $zip)' <<<"${release}" >/dev/null; then
    rm -rf "${dir}"
    gh release download "${TAG}" --pattern "${zip}" --dir "${dir}"
    # Matches the stamp build.yml writes into ModInfo.xml.
    unzip -p "${dir}/${zip}" Mods/ServerCore/ModInfo.xml | grep -qF "<Version value=\"${VERSION}\"" \
      || failures+=("Mods/ServerCore/ModInfo.xml in ${zip} doesn't have version ${VERSION}")
  else
    failures+=("the release has no ${zip}")
  fi

  local expected=false
  [[ "${VERSION}" == *-* ]] && expected=true
  [[ "$(jq -r .isPrerelease <<<"${release}")" == "${expected}" ]] \
    || failures+=("the release should have isPrerelease=${expected}")

  if ((${#failures[@]})); then
    printf 'error: %s\n' "${failures[@]}" >&2
    exit 1
  fi
  echo
  echo "Released ${TAG}: $(jq -r .url <<<"${release}")"
  echo "Check the notes on the release page read right."
}

cmd_publish() {
  preflight
  wait_for_ci
  smoke_test
  push_tag
  check_release
}

case "${COMMAND}" in
  prepare) cmd_prepare ;;
  publish) cmd_publish ;;
  *) usage ;;
esac
