---
name: release
description: Tag and publish a new GitHub release, triggering the publish workflow (builds linux-x64 binary and pushes haus-bin to AUR)
user-invocable: true
---

# Release Skill

Creates a new version tag, pushes it, and publishes a GitHub release. This triggers:
- `publish.yaml` — builds linux-x64 and win-x64 self-contained binaries, uploads as release assets
- `aur.yml` — updates the AUR `haus-bin` package
- `winget.yml` — submits an update PR to `microsoft/winget-pkgs` for `kevinboss.haus`

## 1. Validate preconditions
Run these in parallel:
- `git status` — working tree must be clean
- `git rev-parse --abbrev-ref HEAD` — must be on `main`
- `git fetch origin && git status -sb` — must be up to date with `origin/main`
- `gh secret list --repo kevinboss/haus` — confirm `AUR_SSH_PRIVATE_KEY` and `WINGET_TOKEN` are present
- `git tag --list 'v*' --sort=-v:refname | head -5` — show recent versions to help pick next

If any check fails, stop and report — do not create the tag.

## 2. Determine the version
- If the user passed a version as an argument (e.g. `/release v0.2.0` or `/release 0.2.0`), use it
- Otherwise, propose the next version based on the latest tag and the nature of commits since then (patch vs minor vs major)
- Tag format must be `vMAJOR.MINOR.PATCH` (SemVer with `v` prefix)
- Refuse to reuse an existing tag

## 3. Create and push the tag
```bash
git tag v<version>
git push origin v<version>
```

Do NOT amend or move an existing tag — always create a new version.

## 4. Create the GitHub release
```bash
gh release create v<version> --title "v<version>" --generate-notes
```

This marks the release as `published`, which triggers the publish workflow.

## 5. Monitor the workflow
```bash
gh run watch --repo kevinboss/haus
```
Or poll with `gh run list --workflow publish.yaml --limit 1`.

Report back:
- Release URL: `https://github.com/kevinboss/haus/releases/tag/v<version>`
- Expected tarball asset name: `haus-v<version>-linux-x64.tar.gz`
- AUR package URL: `https://aur.archlinux.org/packages/haus-bin`

## Important
- Never force-push a tag
- If the workflow fails, diagnose and either re-run the failed job or, if the tag is wrong, delete and recreate rather than amend
- Deleting a published release on GitHub is recoverable; deleting a pushed git tag requires `git push origin --delete v<version>` — only do this if explicitly asked
- AUR pushes are not reversible without pkgrel bumps; a failed AUR publish is safe to re-run once the underlying issue is fixed
