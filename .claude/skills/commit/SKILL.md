---
name: commit
description: Create a conventional commit for staged and unstaged changes
user-invocable: true
---

# Commit Skill

## 1. Gather context
Run these in parallel:
- `git status` (never use `-uall`)
- `git diff` and `git diff --cached` to see all changes
- `git log --oneline -10` to match existing commit style

## 2. Stage files
- Stage relevant files by name — never use `git add -A` or `git add .`
- Never stage files that may contain secrets (`.env`, tokens, credentials)

## 3. Propose commits
- If changes span multiple logical units, propose splitting into separate commits
- Show the user a numbered list of proposed commits with: message, files, and rationale
- Wait for user approval before proceeding

## 4. Write a conventional commit message

Format: `<type>(<scope>): <short description>`

**Types:**
- `feat` — new feature or capability
- `fix` — bug fix
- `refactor` — code change that neither fixes a bug nor adds a feature
- `docs` — documentation only
- `style` — formatting, whitespace, no code change
- `test` — adding or updating tests
- `chore` — build config, dependencies, tooling
- `perf` — performance improvement
- `ci` — CI/CD pipeline changes

**Scopes (project-specific):**
- `auth` — OAuth2 PKCE, token storage, browser helper
- `connection` — HassWSApi wrapper, REST client, connection state
- `state` — state list/get/set/delete commands
- `service` — service list/call commands
- `event` — event list/fire commands
- `automation` — automation get/toggle/update/delete commands
- `update` — update list command (Home Assistant update entities)
- `log` — error log command
- `logbook` — logbook list command
- `history` — state history command
- `config` — DI registration, hosting setup, config check command

Scope is optional — omit if the change spans many areas.

**Rules:**
- Subject line ONLY — nothing else after it
- Subject line under 72 characters
- Imperative mood ("add", not "added" or "adds")
- No period at the end of the subject

## 5. Commit
```bash
git commit -m "type(scope): subject line here"
```

## 6. Verify
Run `git status` after committing to confirm success.

## Important
- NEVER amend unless explicitly asked — always create new commits
- NEVER push unless explicitly asked
- If a pre-commit hook fails, fix the issue and create a NEW commit (do not amend)
