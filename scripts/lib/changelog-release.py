#!/usr/bin/env python3
"""Moves the Unreleased entries of CHANGELOG.md into a dated release section.

    scripts/lib/changelog-release.py <CHANGELOG.md> <version> <YYYY-MM-DD>

Used by scripts/release.sh prepare. If "## [<version>]" already exists without a date, the
Unreleased entries are folded into it: entries under a "###" heading it already has are added
to that heading, other headings are added after its own. The file is left with an empty
"## [Unreleased]" section and a link for <version> at the bottom.
"""
import re
import sys

REPO = "https://github.com/gettakaro/ServerCore-7d2d"
LINK = re.compile(r"^\[[^\]]+\]: ")


def fail(message):
    sys.exit(f"error: {message}")


def trim(lines):
    """Drops the blank lines at both ends."""
    start, end = 0, len(lines)
    while start < end and not lines[start].strip():
        start += 1
    while end > start and not lines[end - 1].strip():
        end -= 1
    return lines[start:end]


def split(lines, prefix):
    """Splits lines at each line starting with prefix: (lines before, [[heading, *body], ...])."""
    before, parts = [], []
    for line in lines:
        if line.startswith(prefix):
            parts.append([line])
        elif parts:
            parts[-1].append(line)
        else:
            before.append(line)
    return before, parts


def parse_section(body):
    """A section's body as (intro lines, {### heading: entry lines}), headings in order."""
    intro, parts = split(body, "### ")
    return trim(intro), {part[0]: trim(part[1:]) for part in parts}


def render_section(heading, intro, subsections):
    out = [heading, ""]
    if intro:
        out += intro + [""]
    for sub, entries in subsections.items():
        out += [sub, ""] + entries + [""]
    return out


def main():
    path, version, date = sys.argv[1:]
    with open(path, encoding="utf-8") as f:
        lines = f.read().split("\n")
    if lines[-1] == "":
        lines.pop()

    # Link references at the end of the file stay out of the last section.
    cut = len(lines)
    while cut > 0 and (not lines[cut - 1].strip() or LINK.match(lines[cut - 1])):
        cut -= 1
    links = trim(lines[cut:])
    head, sections = split(lines[:cut], "## ")

    headings = [s[0] for s in sections]
    if "## [Unreleased]" not in headings:
        fail(f"{path} has no '## [Unreleased]' section")
    unreleased = headings.index("## [Unreleased]")
    new_intro, new_subsections = parse_section(sections[unreleased][1:])

    target = next((i for i, h in enumerate(headings) if h.startswith(f"## [{version}]")), None)
    if target is None:
        if not new_intro and not new_subsections:
            fail(f"nothing to release: {path} has no Unreleased entries and no [{version}] section")
        intro, subsections = [], {}
        target = unreleased + 1
        sections.insert(target, [])
    elif headings[target] != f"## [{version}]":
        fail(f"{path} already has a dated section for {version}: {headings[target]}")
    else:
        intro, subsections = parse_section(sections[target][1:])

    intro += new_intro
    for sub, entries in new_subsections.items():
        subsections[sub] = subsections.get(sub, []) + entries
    if not any(subsections.values()) and not intro:
        fail(f"nothing to release: the [{version}] section and Unreleased are both empty")

    sections[target] = render_section(f"## [{version}] - {date}", intro, subsections)
    sections[unreleased] = ["## [Unreleased]", ""]

    link = f"[{version}]: {REPO}/releases/tag/v{version}"
    links = [line for line in links if not line.startswith(f"[{version}]: ")]
    after = next((i + 1 for i, line in enumerate(links) if line.startswith("[Unreleased]: ")), 0)
    links.insert(after, link)

    out = head + [line for section in sections for line in section]
    out = trim(out) + [""] + links
    with open(path, "w", encoding="utf-8") as f:
        f.write("\n".join(out) + "\n")


if __name__ == "__main__":
    main()
