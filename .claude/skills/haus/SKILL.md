---
name: haus
description: Run Haus CLI commands to interact with Home Assistant (states, services, events, entities)
user-invocable: true
---

# Haus CLI Skill

Use the Haus CLI to interact with the user's Home Assistant instance. Run commands via `dotnet run --project src/Haus --`.

## Output Flags

All commands support these global output flags:
- `--json` — structured JSON for machine consumption
- `--porcelain` — plain tab-separated text for grep/cut/awk scripting

When piping output (e.g. through grep), use `--porcelain` for clean, parseable results. Default output uses Spectre.Console tables (pretty but not grep-friendly).

## Commands

### skill install — Install this skill into Claude Code
```bash
dotnet run --project src/Haus -- skill install [-f|--force]
```
Writes the embedded skill file to `~/.claude/skills/haus/SKILL.md`. The embedded version is generated at build time so the examples use the published `haus` binary rather than the dev-time invocation. `--force` overwrites an existing file.

### login — Authenticate with Home Assistant
```bash
dotnet run --project src/Haus -- login [url]
```
- `[url]` — HA URL (default: `HASS_URL` env var or `http://homeassistant.local:8123`)
- Opens a browser for OAuth2 login

### status — Check API connectivity
```bash
dotnet run --project src/Haus -- status
```

### state list — List all entity states
```bash
dotnet run --project src/Haus -- state list
```
To find entities in a specific domain (e.g. automations, lights), use `--porcelain` and pipe through grep:
```bash
dotnet run --project src/Haus -- state list --porcelain 2>/dev/null | grep automation
```

### state get — Get state and attributes of an entity
```bash
dotnet run --project src/Haus -- state get <entity_id>
```
Example: `state get automation.morning_routine`, `state get light.kitchen`

### state set — Set state of an entity
```bash
dotnet run --project src/Haus -- state set <entity_id> <state> [--attributes '<JSON>']
```
Example: `state set sensor.custom "42" --attributes '{"unit_of_measurement":"°C"}'`

### state delete — Remove an entity from the state machine
```bash
dotnet run --project src/Haus -- state delete <entity_id>
```

### event list — List event types
```bash
dotnet run --project src/Haus -- event list
```

### event fire — Fire a custom event
```bash
dotnet run --project src/Haus -- event fire <event_type> [--data '<JSON>' | --from-file <PATH>]
```
Example: `event fire my_event --data '{"key":"value"}'`. Use `--from-file=-` to read JSON from stdin.

### entity list — List all registered entities
```bash
dotnet run --project src/Haus -- entity list
```
Returns every entity in the entity registry, including disabled and hidden ones (unlike `state list`, which only returns entities with current state). Each row shows entity ID, display name, integration platform, area, and status (active/disabled/hidden).

### entity get — Show registry metadata for an entity
```bash
dotnet run --project src/Haus -- entity get <entity_id>
```
Returns registry metadata: platform, area, device, icon, category, disabled/hidden status. For runtime state, use `state get` instead.

### entity rename — Rename an entity's display name
```bash
dotnet run --project src/Haus -- entity rename <entity_id> <name>
```
Example: `entity rename sensor.temp_123 "Living Room Temperature"`

### entity rename-id — Rename an entity's ID
```bash
dotnet run --project src/Haus -- entity rename-id <old_entity_id> <new_entity_id>
```
Example: `entity rename-id automation.sunday_morning_cleaning automation.weekly_cleaning`. The new ID must share the same domain prefix. HA rewrites references in automations, scripts, and dashboards atomically. (`entity update --new-id` does the same thing as part of the kitchen-sink update command.)

### entity update — Update an entity's registry fields
```bash
dotnet run --project src/Haus -- entity update <entity_id> [--name <NAME>] [--icon <ICON>] [--area <AREA_ID>] [--new-id <ENTITY_ID>] [--disable|--enable] [--hide|--show]
```
At least one field is required. `--disable`/`--enable` and `--hide`/`--show` are mutually exclusive. Use `--new-id` to rename the entity ID itself (e.g. `sensor.temp_123` → `sensor.living_room_temp`).

### entity delete — Remove an entity from the registry
```bash
dotnet run --project src/Haus -- entity delete <entity_id>
```
Destructive. Removes the entity from the registry; some integrations will recreate it on next discovery.

### service list — List available services by domain
```bash
dotnet run --project src/Haus -- service list
```

### automation list — List all automations
```bash
dotnet run --project src/Haus -- automation list
```
Shows every `automation.*` entity with alias, on/off state, and last-changed timestamp. Use `--porcelain` for grep-friendly output.

### automation get — Get automation configuration
```bash
dotnet run --project src/Haus -- automation get <automation_id>
```
Example: `automation get automation.morning_routine`
Shows alias, state, mode, triggers, conditions, and actions summary. Use `--json` for the full config.

### automation trace — View recent execution traces
```bash
dotnet run --project src/Haus -- automation trace <automation_id> [--last | --run <run_id>]
```
Without flags: table of the last 10 runs (run ID, started, trigger, result, duration). With `--last`: expanded step tree of the most recent run (timestamps, container types, service calls, condition results, delays). With `--run <id>`: same expansion for a specific run. Closes the "why didn't my automation fire?" debugging loop — shows skipped conditions, not just successful fires. An automation that hasn't fired (yet) returns a clear empty-state message.

### automation toggle — Toggle an automation on/off
```bash
dotnet run --project src/Haus -- automation toggle <automation_id>
```
Example: `automation toggle automation.morning_routine`

### automation create — Create a new automation
```bash
dotnet run --project src/Haus -- automation create (--data '<JSON>' | --from-file <PATH>) [--id <ID>]
```
Provide the configuration via `--data` (inline JSON) or `--from-file` (path; use `--from-file=-` for stdin) — exactly one is required. `--id` sets the config ID; omit it to auto-generate a millisecond-timestamp ID (same convention as the HA UI). The new entity ID is derived from the alias. Fails if the chosen ID is already in use.

### automation update — Update an automation's configuration
```bash
dotnet run --project src/Haus -- automation update <automation_id> (--data '<JSON>' | --from-file <PATH>)
```
Use `automation get --json` to get the current config, modify it, then pass back via `--data` or `--from-file` (use `--from-file=-` for stdin — avoids shell-quoting pain when configs contain Jinja).

### automation delete — Delete an automation
```bash
dotnet run --project src/Haus -- automation delete <automation_id>
```
Example: `automation delete automation.old_routine`

### script list — List all scripts
```bash
dotnet run --project src/Haus -- script list
```
Shows every `script.*` entity with alias and last-triggered timestamp. Use `--porcelain` for grep-friendly output.

### script get — Get script configuration
```bash
dotnet run --project src/Haus -- script get <script_id>
```
Example: `script get script.notify_all_phones`
Shows alias, mode, fields, and a sequence summary. Use `--json` for the full config.

### script create — Create a new script
```bash
dotnet run --project src/Haus -- script create --id <ID> (--data '<JSON>' | --from-file <PATH>)
```
`--id` is the script's object ID (the part after `script.`); it becomes the entity name. Provide the configuration via `--data` (inline JSON) or `--from-file` (path; use `--from-file=-` for stdin). Useful for wrapping multi-target service calls (e.g. `notify.send_message` to several phones) into a single reusable script.

### script update — Update a script's configuration
```bash
dotnet run --project src/Haus -- script update <script_id> (--data '<JSON>' | --from-file <PATH>)
```
Use `script get --json` to get the current config, modify it, then pass back via `--data` or `--from-file`.

### script delete — Delete a script
```bash
dotnet run --project src/Haus -- script delete <script_id>
```
Example: `script delete script.old_routine`

### scene list — List all scenes
```bash
dotnet run --project src/Haus -- scene list
```
Shows every `scene.*` entity with name, type (config/runtime), entity count, and last-activated timestamp. **Config scenes** are persisted and editable via this CLI. **Runtime scenes** are created by automations using the `scene.create` service (e.g. for snapshot/restore) — they appear here for visibility but aren't editable or deletable; they disappear on HA restart.

### scene get — Get scene details
```bash
dotnet run --project src/Haus -- scene get <scene_id>
```
For config scenes: shows name, icon, entities and their target states. For runtime scenes: shows the entity list captured in the snapshot. Use `--json` for the full config body (config scenes only).

### scene create — Create a new scene
```bash
dotnet run --project src/Haus -- scene create (--data '<JSON>' | --from-file <PATH>) [--id <ID>]
```
JSON requires `name` and `entities` (a dict mapping entity_id → state string or `{state, ...attrs}` object). `--id` sets the config ID; omit to auto-generate. Fails if the chosen ID is already in use.

### scene update — Update a config scene
```bash
dotnet run --project src/Haus -- scene update <scene_id> (--data '<JSON>' | --from-file <PATH>)
```
Refuses runtime scenes with a clear error.

### scene delete — Delete a config scene
```bash
dotnet run --project src/Haus -- scene delete <scene_id>
```
Refuses runtime scenes with a clear error.

### scene activate — Activate a scene (apply target states)
```bash
dotnet run --project src/Haus -- scene activate <scene_id> [--transition <SECONDS>]
```
Wraps `scene.turn_on`. `--transition` applies to compatible entities (e.g. lights fade over N seconds).

### update list — List update entities and their availability
```bash
dotnet run --project src/Haus -- update list
```
Shows all `update.*` entities with installed version, latest version, and status (available/installing/skipped/up to date). Updates with state `on` are sorted first.

### zone list — List geofence zones
```bash
dotnet run --project src/Haus -- zone list
```
Shows every `zone.*` entity with name, coordinates, radius, person count inside, and whether it's editable via this CLI. Includes `zone.home`, UI-configured zones, and YAML-configured zones.

### zone get — Show full zone details
```bash
dotnet run --project src/Haus -- zone get <zone_id>
```
Example: `zone get zone.home`. Shows coordinates, radius, passive/active, icon, and currently-inside persons.

### zone update — Update a zone
```bash
dotnet run --project src/Haus -- zone update <zone_id> [--radius <METERS>] [--lat <LAT>] [--lng <LNG>] [--icon <ICON>] [--passive | --active] [--data '<JSON>' | --from-file <PATH>]
```
Partial flags merge with the current zone state; `--data`/`--from-file` replace the body wholesale (and cannot be combined with partial flags).

**zone.home** is special: it's linked to HA's installation coordinates (`config/core/update`), not the zone storage. Only `--radius`, `--lat`, `--lng` are supported on zone.home; `--icon`, `--passive`, `--data`, `--from-file` are rejected with a clear error.

YAML-configured zones are not editable via this command (HA's storage backend doesn't expose them).

### update install — Install an available update
```bash
dotnet run --project src/Haus -- update install <entity_id> [--version <VERSION>] [--backup]
```
Example: `update install update.home_assistant_core_update --backup`
Defaults to the latest version. `--backup` creates a backup first (only supported by some integrations).

### service call — Call a service
```bash
dotnet run --project src/Haus -- service call <domain.service> [--entity <entity_id>] [--data '<JSON>' | --from-file <PATH>]
```
Examples:
- `service call light.turn_on --entity light.kitchen`
- `service call light.turn_on --data '{"entity_id":"light.bedroom","brightness":200}'`
- `service call notify.mobile_app --from-file payload.json`

### log — Show recent errors and warnings
```bash
dotnet run --project src/Haus -- log [-n|--limit <COUNT>] [-l|--level <LEVEL>] [--with-trace]
```
Reads HA's in-memory `system_log` (the same buffer that powers Settings → System → Logs in the UI). Works on every install regardless of how HA is configured to write logs.
- `-n|--limit` — show only the most recent N entries
- `-l|--level` — filter by level: `error`, `warning`, `info`, `debug`
- `--with-trace` — include exception stack traces

Newest entries first.

### logbook list — List logbook entries
```bash
dotnet run --project src/Haus -- logbook list [-e <ENTITY>] [-s <DURATION>] [-u <ISO>]
```
- `-e|--entity` — filter to a single entity (e.g. `light.kitchen`)
- `-s|--since` — how far back to look. Duration shorthand: `30m`, `2h`, `1d`. Default: `1h`
- `-u|--until` — end timestamp (ISO 8601). Default: now

Useful for seeing "what just changed" in HA — automation triggers, state changes, and integration events with human-readable messages.

### history get — Get state history for an entity
```bash
dotnet run --project src/Haus -- history get <entity_id> [-s <DURATION>] [-u <ISO>] [--with-attributes] [--statistics <PERIOD>]
```
- `<entity_id>` — required, e.g. `device_tracker.unifi_express`
- `-s|--since` — how far back. Default: `1h`
- `-u|--until` — end timestamp (ISO 8601). Default: now
- `--with-attributes` — include full state attributes (default omits them for compactness)
- `--statistics <PERIOD>` — use recorder statistics (mean/min/max/sum per period) instead of raw state changes. Period: `5minute`, `hour`, `day`, `week`, `month`. Useful when state changes are sparse because the sensor reports in coarse steps — statistics aggregate per period even when no state change was logged. Requires `state_class: measurement` on the sensor.

### config check — Validate Home Assistant configuration
```bash
dotnet run --project src/Haus -- config check
```
Calls `/api/config/core/check_config`. Exits 0 if valid, 1 if invalid. Requires admin auth.

## Usage Notes

- Requires prior `haus login` or env vars `HASS_URL`/`HASS_TOKEN`
- Entity IDs follow `<domain>.<object_id>` format (e.g., `light.kitchen`, `automation.bedtime`)
- Home Assistant domains include: `automation`, `light`, `switch`, `sensor`, `binary_sensor`, `climate`, `cover`, `fan`, `media_player`, `vacuum`, `script`, `scene`, `input_boolean`, `input_number`, `input_select`, `input_text`, and more
- To control automations: use `automation toggle`, `service call automation.trigger --entity automation.<name>`, etc.
- Errors go to stderr — redirect with `2>/dev/null` when piping output
