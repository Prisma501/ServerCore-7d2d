# shellcheck shell=bash
# Release notes from CHANGELOG.md, shared by release.yml and scripts/release.sh.

# Prints the body of the "## [<version>]" section of CHANGELOG.md (or file $2): what the
# GitHub release for <version> shows as its notes. Prints nothing when there's no section.
changelog_notes() {
  awk -v v="$1" '
    $0 ~ "^## \\[" v "\\]" { found = 1; next }
    found && /^## \[/ { exit }
    found { print }
  ' "${2:-CHANGELOG.md}"
}
