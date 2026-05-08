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
dotnet run --project src/Haus -- event fire <event_type> [--data '<JSON>']
```
Example: `event fire my_event --data '{"key":"value"}'`

### entity rename — Rename an entity's display name
```bash
dotnet run --project src/Haus -- entity rename <entity_id> <name>
```
Example: `entity rename sensor.temp_123 "Living Room Temperature"`

### service list — List available services by domain
```bash
dotnet run --project src/Haus -- service list
```

### automation get — Get automation configuration
```bash
dotnet run --project src/Haus -- automation get <automation_id>
```
Example: `automation get automation.morning_routine`
Shows alias, state, mode, triggers, conditions, and actions summary. Use `--json` for the full config.

### automation toggle — Toggle an automation on/off
```bash
dotnet run --project src/Haus -- automation toggle <automation_id>
```
Example: `automation toggle automation.morning_routine`

### automation update — Update an automation's configuration
```bash
dotnet run --project src/Haus -- automation update <automation_id> --data '<JSON>'
```
Use `automation get --json` to get the current config, modify it, then pass back via `--data`.

### automation delete — Delete an automation
```bash
dotnet run --project src/Haus -- automation delete <automation_id>
```
Example: `automation delete automation.old_routine`

### update list — List update entities and their availability
```bash
dotnet run --project src/Haus -- update list
```
Shows all `update.*` entities with installed version, latest version, and status (available/installing/skipped/up to date). Updates with state `on` are sorted first.

### update install — Install an available update
```bash
dotnet run --project src/Haus -- update install <entity_id> [--version <VERSION>] [--backup]
```
Example: `update install update.home_assistant_core_update --backup`
Defaults to the latest version. `--backup` creates a backup first (only supported by some integrations).

### service call — Call a service
```bash
dotnet run --project src/Haus -- service call <domain.service> [--entity <entity_id>] [--data '<JSON>']
```
Examples:
- `service call light.turn_on --entity light.kitchen`
- `service call light.turn_on --data '{"entity_id":"light.bedroom","brightness":200}'`

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
dotnet run --project src/Haus -- history get <entity_id> [-s <DURATION>] [-u <ISO>] [--with-attributes]
```
- `<entity_id>` — required, e.g. `device_tracker.unifi_express`
- `-s|--since` — how far back. Default: `1h`
- `-u|--until` — end timestamp (ISO 8601). Default: now
- `--with-attributes` — include full state attributes (default omits them for compactness)

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
