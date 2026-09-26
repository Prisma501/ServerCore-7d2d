import { existsSync, readFileSync, writeFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';

// Builds the changelog page from the repo's CHANGELOG.md (the source release.yml
// also reads) followed by the PrismaCore history, which stays frozen as published.
const changelogPath = fileURLToPath(new URL('../../CHANGELOG.md', import.meta.url));
const historyPath = fileURLToPath(new URL('../src/changelog/prismacore-history.md', import.meta.url));
const outputPath = fileURLToPath(new URL('../src/content/docs/project/changelog.md', import.meta.url));

const frontmatter = `---
title: Changelog
description: Every ServerCore release, followed by the full PrismaCore version history.
editUrl: https://github.com/gettakaro/ServerCore-7d2d/edit/main/CHANGELOG.md
tableOfContents:
  minHeadingLevel: 2
  maxHeadingLevel: 2
---
`;

const handover = `## PrismaCore version history

:::note[Handed over from PrismaCore]
ServerCore continues from PrismaCore 2.5 by Prisma501. The history below is kept exactly as it was published.
:::
`;

function render() {
	const changelog = readFileSync(changelogPath, 'utf8');
	const firstRelease = changelog.indexOf('\n## ');
	if (firstRelease === -1) throw new Error(`No "## " release heading found in ${changelogPath}`);
	const history = readFileSync(historyPath, 'utf8');
	return [frontmatter, changelog.slice(firstRelease + 1).trimEnd(), '', handover, history].join('\n');
}

function write() {
	const content = render();
	if (existsSync(outputPath) && readFileSync(outputPath, 'utf8') === content) return;
	writeFileSync(outputPath, content);
}

export default function changelog() {
	return {
		name: 'servercore-changelog',
		hooks: {
			'astro:config:setup': ({ addWatchFile }) => {
				addWatchFile(changelogPath);
				addWatchFile(historyPath);
				write();
			},
		},
	};
}
