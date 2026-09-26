# 2. One branch per game version

Date: 2026-09-26

## Status

Accepted

## Context

We build for two game versions at once: the stable Steam branch (3.2.0 b10 today) and the experimental branch (3.3.0 EXP). The game's API changes between them, so some code has to differ.

The options were conditional compilation (`#if GAME_3_3` symbols with a build matrix over several `game-version.json` pins) or one branch per game version.

## Decision

`main` targets the stable game version. The `experimental` branch targets the experimental game version. Each branch has exactly one `game-version.json` and CI builds only that one; there is no matrix.

Stable releases (`v3.0.0`) are tagged on `main`. Experimental builds are tagged on `experimental` as `v3.1.0-exp.N`, and the release workflow publishes any tag containing `-` as a GitHub pre-release.

Fixes land on `main` first and are merged into `experimental`. When the experimental game version goes stable, `experimental` is merged into `main` and the next experimental cycle starts from there.

## Consequences

- The source stays free of `#if` blocks, and each branch builds and reads like a single-version mod.
- Every fix is merged forward from `main` to `experimental`; conflicts show up where the game API changed, which is where a human should look anyway.
- Compile fixes for a new game version are ordinary commits on `experimental`, reviewable on their own.
