#!/usr/bin/env bash
# Compares our build against Prisma's PrismaCore.dll to show the source import
# changed nothing functional. One-off audit tool, not a CI gate.
#
# Usage: scripts/compare-assemblies.sh [--rename] <PrismaCore.dll> [ServerCore.dll]
#
# Decompiles both assemblies with ilspycmd (in the .NET 8 SDK container) and
# diffs sorted lists of types, members, string literals, console command names
# and assembly references, plus the decompiled C# per type. Compiler-generated
# names (closures, lambdas, iterators) are normalized because their numbering
# depends on compile order. LiteDB.dll is compared by sha256 when it sits next
# to both DLLs.
#
# Types deleted on purpose (dead code, see REMOVED_TYPES) are dropped from
# Prisma's side before diffing.
#
# --rename maps Prisma's identifiers to ours (PrismaCore namespaces, the
# PrismaCoreSettings/PrismaCoreStrings classes, RemoveItemConsoleByTree) before
# diffing types and members. String literals are never mapped.
#
# Needs Docker and the game references (scripts/fetch-game-refs.sh). Output
# lands in _data/compare/; the exit code is non-zero when any diff is non-empty.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ILSPY_VERSION="9.1.0.7988"
SDK_IMAGE="mcr.microsoft.com/dotnet/sdk:8.0"
# Unreferenced types removed from the source; matched against full type names.
REMOVED_TYPES='PrismaCore\.JSON\.JsonManualBuilder\b'

RENAME=false
if [[ "${1:-}" == "--rename" ]]; then RENAME=true; shift; fi
if [[ $# -lt 1 || $# -gt 2 ]]; then
  echo "usage: $0 [--rename] <PrismaCore.dll> [ServerCore.dll]" >&2
  exit 2
fi
THEIRS="$(realpath "$1")"
OURS="$(realpath "${2:-${ROOT}/Mods/ServerCore/ServerCore.dll}")"
[[ -d "${ROOT}/_data/7dtd-binaries" ]] || { echo "error: run scripts/fetch-game-refs.sh first" >&2; exit 1; }

OUT="${ROOT}/_data/compare"
rm -rf "${OUT}/theirs" "${OUT}/ours" "${OUT}/diff"
mkdir -p "${OUT}/theirs" "${OUT}/ours" "${OUT}/diff"
cp "${THEIRS}" "${OUT}/theirs/input.dll"
cp "${OURS}" "${OUT}/ours/input.dll"

docker run --rm -u "$(id -u):$(id -g)" -e HOME=/tmp -e DOTNET_CLI_TELEMETRY_OPTOUT=1 \
  -v "${ROOT}":/src -w /src "${SDK_IMAGE}" bash -euc "
    ILSPY=_data/tools/ilspycmd-${ILSPY_VERSION}/ilspycmd
    [[ -x \${ILSPY} ]] || dotnet tool install ilspycmd --version ${ILSPY_VERSION} \
      --tool-path _data/tools/ilspycmd-${ILSPY_VERSION} >/dev/null
    for side in theirs ours; do
      dir=_data/compare/\${side}
      \${ILSPY} -r _data/7dtd-binaries -il \${dir}/input.dll > \${dir}/module.il
      \${ILSPY} -r _data/7dtd-binaries -l cisde \${dir}/input.dll > \${dir}/types.raw
      \${ILSPY} -r _data/7dtd-binaries -p -o \${dir}/cs \${dir}/input.dll >/dev/null
    done"

normalize_generated() {
  sed -E \
    -e 's/<>c__DisplayClass[0-9_]+/<>c__DisplayClassN/g' \
    -e 's/b__[0-9]+(_[0-9]+)?/b__N/g' \
    -e 's/d__[0-9]+/d__N/g' \
    -e 's/<>9__[0-9_]+/<>9__N/g' \
    -e 's/<>8__locals[0-9]+/<>8__localsN/g' \
    -e 's/<>[0-9]+__/<>N__/g'
}

# shellcheck disable=SC2329 # called through ${map}
map_identifiers() {
  if [[ "${RENAME}" == true ]]; then
    sed -E \
      -e 's/\bPrismaCore(Settings|Strings)\b/ServerCore\1/g' \
      -e 's/\bPrismaCore\b/ServerCore/g' \
      -e 's/\bRemoveItemConsoleByTree\b/ServerCore.CustomCommands/g'
  else
    cat
  fi
}

# Tracks the full (nested) type name while walking the IL. Prints one line per
# type member, and with STRINGS=1 one line per ldstr, both as "<type> :: <text>".
il_walk() {
  awk -v strings="${STRINGS:-0}" '
    function path(  i, p) { p = stack[1]; for (i = 2; i <= depth; i++) p = p "/" stack[i]; return p }
    function flush() { gsub(/[ \t]+/, " ", decl); if (!strings) print path() " :: " decl; decl = ""; collecting = 0 }
    /^[ \t]*\.class / { name = $NF; gsub(/\x27/, "", name); stack[++depth] = name; next }
    /^[ \t]*} \/\/ end of class / { depth--; next }
    collecting && /^[ \t]*\{[ \t]*$/ { flush(); next }
    collecting { decl = decl " " $0; next }
    /^[ \t]*\.method / { collecting = 1; decl = $0; next }
    !strings && /^[ \t]*\.(field|property|event) / { d = $0; gsub(/[ \t]+/, " ", d); sub(/ = .*/, "", d); print path() " :: " d }
    strings && /^[ \t]*IL_[0-9a-f]+: ldstr / { s = $0; sub(/^[ \t]*IL_[0-9a-f]+: /, "", s); print path() " :: " s }
  ' "$1"
}

members() { il_walk "$1"; }
string_literals() { STRINGS=1 il_walk "$1"; }

# Applies a filter to the "<type>" half of "<type> :: <text>" lines only, so
# identifier mapping never touches string literals.
on_type() {
  local tmp
  tmp="$(mktemp -d)"
  cat > "${tmp}/in"
  sed 's/ :: .*//' "${tmp}/in" | "$@" > "${tmp}/types"
  sed 's/^[^:]* :: / :: /' "${tmp}/in" > "${tmp}/rest"
  paste -d '' "${tmp}/types" "${tmp}/rest"
  rm -rf "${tmp}"
}

# The string literals inside every getCommands() body, per type.
command_names() {
  awk '
    /^[ \t]*\.method .*$/ { header = $0; inmethod = 1; want = 0 }
    inmethod && /getCommands \(\)/ { want = 1 }
    want && /ldstr / { s = $0; sub(/.*ldstr /, "", s); names = names " " s }
    /\} \/\/ end of method / { if (want) { t = $0; sub(/.*end of method /, "", t); sub(/::.*/, "", t); print t " ::" names }
                               inmethod = 0; want = 0; names = "" }
  ' "$1"
}

assembly_refs() {
  grep -hE '<(Reference Include|HintPath)' "$1"/cs/*.csproj \
    | sed -E -e 's/^[[:space:]]+//' -e 's#<HintPath>.*/([^/]+)</HintPath>#<HintPath>\1</HintPath>#'
}

for side in theirs ours; do
  dir="${OUT}/${side}"
  if [[ "${side}" == theirs ]]; then
    map="map_identifiers"; drop=(grep -Ev "${REMOVED_TYPES}")
  else
    map="cat"; drop=("cat")
  fi
  normalize_generated < "${dir}/types.raw" | { "${drop[@]}" || true; } | ${map} | sort > "${dir}/types.txt"
  members "${dir}/module.il" | { "${drop[@]}" || true; } | normalize_generated | ${map} | sort > "${dir}/members.txt"
  string_literals "${dir}/module.il" | { "${drop[@]}" || true; } | on_type normalize_generated | on_type ${map} | sort > "${dir}/strings.txt"
  command_names "${dir}/module.il" | on_type ${map} | sort > "${dir}/commands.txt"
  assembly_refs "${dir}" | sort > "${dir}/references.txt"
  # Decompiled C#, one file per type, flattened so namespace folders don't matter.
  mkdir -p "${dir}/cs-flat"
  find "${dir}/cs" -name '*.cs' ! -path '*/Properties/*' | while read -r f; do
    flat="$(echo "${f#"${dir}/cs/"}" | tr '/' '.')"
    [[ "${side}" == theirs ]] && echo "${flat%.cs}" | grep -qE "${REMOVED_TYPES}" && continue
    normalize_generated < "${f}" | ${map} > "${dir}/cs-flat/$(echo "${flat}" | ${map})"
  done
done

status=0
summary() {
  local name="$1" file="${OUT}/diff/$1.diff"
  local count
  count=$(grep -cE '^([<>]|Only in )' "${file}" || true)
  printf '%-12s %s\n' "${name}" "$([[ "${count}" -eq 0 ]] && echo "identical" || echo "${count} lines differ (${file#"${ROOT}/"})")"
  [[ "${count}" -eq 0 ]] || status=1
}

echo "Comparing $(basename "${THEIRS}") (theirs) with $(basename "${OURS}") (ours)$([[ "${RENAME}" == true ]] && echo ', identifiers mapped')"
for list in types members strings commands references; do
  diff "${OUT}/theirs/${list}.txt" "${OUT}/ours/${list}.txt" > "${OUT}/diff/${list}.diff" || true
  summary "${list}"
done
diff -r "${OUT}/theirs/cs-flat" "${OUT}/ours/cs-flat" > "${OUT}/diff/csharp.diff" || true
summary csharp

theirs_litedb="$(dirname "${THEIRS}")/LiteDB.dll"
ours_litedb="$(dirname "${OURS}")/LiteDB.dll"
if [[ -f "${theirs_litedb}" && -f "${ours_litedb}" ]]; then
  if [[ "$(sha256sum < "${theirs_litedb}")" == "$(sha256sum < "${ours_litedb}")" ]]; then
    printf '%-12s %s\n' LiteDB.dll "identical ($(sha256sum < "${ours_litedb}" | cut -c1-16)...)"
  else
    printf '%-12s %s\n' LiteDB.dll "sha256 differs"
    status=1
  fi
fi

echo "Removed on purpose, excluded from the diffs: ${REMOVED_TYPES}"
for prop in AssemblyName PlatformTarget; do
  printf '%-12s %s -> %s (expected)\n' "${prop}" \
    "$(grep -ohP "(?<=<${prop}>)[^<]+" "${OUT}"/theirs/cs/*.csproj || echo none)" \
    "$(grep -ohP "(?<=<${prop}>)[^<]+" "${OUT}"/ours/cs/*.csproj || echo none)"
done
exit "${status}"
