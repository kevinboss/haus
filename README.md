<h1 align="center">Haus</h1>

<p align="center">
  Scriptable command-line interface for <a href="https://www.home-assistant.io/">Home Assistant</a>.
</p>

<p align="center">
  <a href="https://aur.archlinux.org/packages/haus-bin"><img src="https://img.shields.io/aur/version/haus-bin?label=AUR" alt="AUR version"></a>
  <a href="https://github.com/kevinboss/haus/actions/workflows/publish.yaml"><img src="https://github.com/kevinboss/haus/actions/workflows/publish.yaml/badge.svg" alt="Build"></a>
  <a href="LICENSE"><img src="https://img.shields.io/github/license/kevinboss/haus" alt="License: GPL-3.0"></a>
  <a href="https://github.com/kevinboss/heartbeat"><img src="https://raw.githubusercontent.com/kevinboss/heartbeat/main/badges/kevinboss_haus.svg" alt="Heartbeat"></a>
</p>

> This project is being developed using AI development tools. However, every line of code is reviewed and approved by a human before being committed.

<p align="center">
  <img src="docs/haus-demo.gif" alt="haus demo">
</p>

## Usage

```bash
# Authenticate
haus login

# List entities
haus state list

# Get entity details
haus state get light.kitchen

# Call a service
haus service call light.turn_on --entity light.kitchen

# View automation config
haus automation get automation.morning_routine

# Toggle an automation
haus automation toggle automation.morning_routine

# Create a new automation from a JSON file (avoids shell-quoting Jinja)
haus automation create --from-file morning.json

# Create a reusable script (e.g. fan out notifications to multiple phones)
haus script create --id notify_all_phones --from-file notify_all.json

# List Lovelace dashboards (and edit their view config via JSON)
haus dashboard list
haus dashboard config get my-dashboard --json > config.json
haus dashboard config save my-dashboard --from-file config.json

# List the entity registry (includes disabled/hidden entities)
haus entity list

# Disable a noisy entity
haus entity update sensor.unused --disable

# Manage areas (Settings → Areas & Zones)
haus area list
haus area create --name "Living Room" --icon mdi:sofa
haus area update living_room --floor ground_floor

# Labels: create once, bulk-assign across many entities/areas
haus label create --name "Critical" --color red --icon mdi:alert
haus label assign critical --entity sensor.front_door --entity sensor.back_door --area garage

# Apply config changes without a full restart
haus hass reload automation
haus hass restart

# Check for available updates
haus update list

# Install an update
haus update install update.home_assistant_core_update

# List integrations (config entries) and tweak their options
haus integration list
haus integration get 01KKCVMQESC1RV1YQ39ANVHT78          # entry details + options schema
haus integration configure 01KKCVMQESC1RV1YQ39ANVHT78 --data '{"notify": [], ...}'
haus integration reload 01KKCVMQESC1RV1YQ39ANVHT78         # the UI's reload button
haus integration disable 01KKCVMQESC1RV1YQ39ANVHT78        # unload without deleting (enable to restore)
haus integration reauth 01K68HW3ZFF0B0X3XQ0ZZKNYJP         # complete a pending reauth (fixes 401s)
haus integration reconfigure 01K68HW3ZFF0B0X3XQ0ZZKNYJP    # change host/credentials proactively

# See what just changed (logbook entries from the last hour)
haus logbook list

# Filter logbook to a single entity
haus logbook list --entity automation.morning_routine --since 1d

# State history for an entity
haus history get device_tracker.phone --since 6h

# Validate Home Assistant configuration
haus config check

# Show recent errors and warnings
haus log --limit 10
haus log --level error

# Install the Haus skill into Claude Code
haus skill install

# Scriptable output
haus state list --porcelain | grep automation
haus state get sensor.temp --json
```

All commands support `--json` for structured output and `--porcelain` for plain tab-separated text.

## Build

```bash
dotnet build
dotnet run --project src/Haus -- <command>
dotnet test
```

## License

[GPL-3.0](LICENSE)
