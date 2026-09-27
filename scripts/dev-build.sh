#!/usr/bin/env bash
# Development builds of ServerCore. RELEASING.md describes them.
#
#   scripts/dev-build.sh stamp <version> <dist>
#       stamp <version> into <dist>/Mods/ServerCore/ModInfo.xml
#   scripts/dev-build.sh publish <stable|experimental> <sha> <dist>
#       stamp and zip the mod in <dist>/Mods/ServerCore/, built from commit <sha>, and publish
#       it as the rolling-<branch> pre-release
#   scripts/dev-build.sh comment <pr-number> <sha> <version> <artifact-url>
#       post or update the PR comment linking that PR's build artifact
#
# Runs in CI (GH_TOKEN and GITHUB_REPOSITORY set) or locally with a logged-in gh. Needs gh and jq.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PIN="${ROOT}/game-version.json"
REPO="${GITHUB_REPOSITORY:-$(gh repo view --json nameWithOwner --jq .nameWithOwner)}"
COMMENT_MARKER="<!-- servercore-pr-build -->"

usage() {
  sed -n '2,12p' "${BASH_SOURCE[0]}" | sed 's/^# \{0,1\}//' >&2
  exit 2
}

# The game build a checkout compiles against: stable or experimental, from game-version.json.
game_target() {
  case "$(jq -er .branch "${PIN}")" in
    public) echo stable ;;
    latest_experimental) echo experimental ;;
    *) echo "error: unknown Steam branch in ${PIN}" >&2; return 1 ;;
  esac
}

release_exists() {
  gh release view "$1" --repo "${REPO}" >/dev/null 2>&1
}

# Creates the pre-release <tag> at <sha>, or moves an existing one there and replaces its zip,
# so the download URL never changes.
upsert_release() {
  local tag="$1" sha="$2" title="$3" notes="$4" zip="$5"
  if release_exists "${tag}"; then
    gh api -X PATCH "repos/${REPO}/git/refs/tags/${tag}" -f sha="${sha}" -F force=true >/dev/null
    gh release upload "${tag}" "${zip}" --repo "${REPO}" --clobber
    gh release edit "${tag}" --repo "${REPO}" --title "${title}" --notes "${notes}" --prerelease
  else
    gh release create "${tag}" "${zip}" --repo "${REPO}" --target "${sha}" \
      --title "${title}" --notes "${notes}" --prerelease
  fi
}

# Edits the PR's build comment in place, or posts it the first time.
upsert_comment() {
  local pr="$1" body="$2" existing
  existing="$(gh api "repos/${REPO}/issues/${pr}/comments" --paginate \
    | jq -r --arg m "${COMMENT_MARKER}" '.[] | select(.body | startswith($m)) | .id' | head -n1)"
  if [[ -n "${existing}" ]]; then
    jq -n --arg body "${body}" '{body: $body}' \
      | gh api -X PATCH "repos/${REPO}/issues/comments/${existing}" --input - >/dev/null
  else
    jq -n --arg body "${body}" '{body: $body}' \
      | gh api -X POST "repos/${REPO}/issues/${pr}/comments" --input - >/dev/null
  fi
}

stamp() {
  sed -i -E "s|(<Version value=\")[^\"]*|\1${1}|" "${2}/Mods/ServerCore/ModInfo.xml"
}

cmd_publish() {
  local branch="$1" sha="$2" dist="$3"
  local tag="rolling-${branch}"
  local version="${branch}.${sha:0:7}"
  local asset="ServerCore-${tag}.zip"
  local game target
  game="$(jq -er .version "${PIN}")"
  target="$(game_target)"

  stamp "${version}" "${dist}"
  rm -f "${dist}/${asset}"
  (cd "${dist}" && zip -q -r -X "${asset}" Mods)
  local sha256
  sha256="$(sha256sum "${dist}/${asset}" | cut -d' ' -f1)"

  local notes="Development build of the \`${branch}\` branch at ${sha}, for the ${target} game version (${game}).

It is replaced on every push. Not for production: use a [versioned release](https://github.com/${REPO}/releases) for that.

sha256: \`${sha256}\`"

  upsert_release "${tag}" "${sha}" "ServerCore ${tag}" "${notes}" "${dist}/${asset}"
  echo "Published https://github.com/${REPO}/releases/download/${tag}/${asset}"
}

cmd_comment() {
  local pr="$1" sha="$2" version="$3" url="$4"
  local game target
  game="$(jq -er .version "${PIN}")"
  target="$(game_target)"
  upsert_comment "${pr}" "${COMMENT_MARKER}
### ServerCore build for this PR

**Download:** [ServerCore build artifact](${url}) (needs a GitHub login) · **Version:** \`${version}\` · **Game:** ${target} (${game})

Built from ${sha}. Extract the zip into the server folder so you get \`Mods/ServerCore/\`. This comment is updated on every push, and the artifact expires after 30 days. _Not for production._"
  echo "Commented on PR #${pr}"
}

case "${1:-}" in
  stamp) [[ $# -eq 3 ]] || usage; stamp "$2" "$3" ;;
  publish) [[ $# -eq 4 && "$2" =~ ^(stable|experimental)$ ]] || usage; cmd_publish "$2" "$3" "$4" ;;
  comment) [[ $# -eq 5 && "$2" =~ ^[0-9]+$ ]] || usage; cmd_comment "$2" "$3" "$4" "$5" ;;
  *) usage ;;
esac
