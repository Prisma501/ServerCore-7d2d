#!/usr/bin/env bash
# Publishes development builds of ServerCore as GitHub pre-releases. RELEASING.md describes them.
#
#   scripts/dev-build.sh publish <channel> <sha> <dist>
#       stamp, zip and publish the mod in <dist>/Mods/ServerCore/, built from commit <sha>.
#       <channel> is stable or experimental (the rolling-<channel> release) or pr-<number>
#       (that PR's build, also posted as a comment on the PR).
#   scripts/dev-build.sh cleanup <pr-number>
#       delete the pr-<number> release and its tag
#
# Runs in CI (GH_TOKEN and GITHUB_REPOSITORY set) or locally with a logged-in gh. Needs gh and jq.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PIN="${ROOT}/game-version.json"
REPO="${GITHUB_REPOSITORY:-$(gh repo view --json nameWithOwner --jq .nameWithOwner)}"
COMMENT_MARKER="<!-- servercore-pr-build -->"

usage() {
  sed -n '2,11p' "${BASH_SOURCE[0]}" | sed 's/^# \{0,1\}//' >&2
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

cmd_publish() {
  local channel="$1" sha="$2" dist="$3" tag pr=""
  case "${channel}" in
    stable | experimental) tag="rolling-${channel}" ;;
    pr-[0-9]*) tag="${channel}"; pr="${channel#pr-}" ;;
    *) usage ;;
  esac

  local version="${channel}.${sha:0:7}"
  local asset="ServerCore-${tag}.zip"
  local url="https://github.com/${REPO}/releases/download/${tag}/${asset}"
  local game target
  game="$(jq -er .version "${PIN}")"
  target="$(game_target)"

  sed -i -E "s|(<Version value=\")[^\"]*|\1${version}|" "${dist}/Mods/ServerCore/ModInfo.xml"
  rm -f "${dist}/${asset}"
  (cd "${dist}" && zip -q -r -X "${asset}" Mods)
  local sha256
  sha256="$(sha256sum "${dist}/${asset}" | cut -d' ' -f1)"

  local source="the \`${channel}\` branch" lifetime="It is replaced on every push"
  if [[ -n "${pr}" ]]; then
    source="PR #${pr}"
    lifetime="${lifetime} and deleted when the PR closes"
  fi
  local notes="Development build of ${source} at ${sha}, for the ${target} game version (${game}).

${lifetime}. Not for production: use a [versioned release](https://github.com/${REPO}/releases) for that.

sha256: \`${sha256}\`"

  upsert_release "${tag}" "${sha}" "ServerCore ${tag}" "${notes}" "${dist}/${asset}"
  echo "Published ${url}"

  if [[ -n "${pr}" ]]; then
    upsert_comment "${pr}" "${COMMENT_MARKER}
### ServerCore build for this PR

**Download:** [\`${asset}\`](${url}) · **Version:** \`${version}\` · **Game:** ${target} (${game})

Built from ${sha} · sha256 \`${sha256:0:12}\`

Unzip it into the server folder so it becomes \`Mods/ServerCore/\`. This build is replaced on every push and deleted when the PR closes. _Not for production._"
    echo "Commented on PR #${pr}"
  fi
}

cmd_cleanup() {
  local tag="pr-$1"
  if release_exists "${tag}"; then
    gh release delete "${tag}" --repo "${REPO}" --cleanup-tag --yes
    echo "Deleted ${tag}"
  else
    echo "No ${tag} release to delete"
  fi
}

case "${1:-}" in
  publish) [[ $# -eq 4 ]] || usage; cmd_publish "$2" "$3" "$4" ;;
  cleanup) [[ $# -eq 2 && "$2" =~ ^[0-9]+$ ]] || usage; cmd_cleanup "$2" ;;
  *) usage ;;
esac
