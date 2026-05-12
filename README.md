# Haus

> This project is being developed using AI development tools. However, every line of code is reviewed and approved by a human before being committed.

Scriptable command-line interface for [Home Assistant](https://www.home-assistant.io/).

![haus demo](docs/haus-demo.gif)

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

# Create a new automation from JSON
haus automation create --data "$(cat morning.json)"

# Check for available updates
haus update list

# Install an update
haus update install update.home_assistant_core_update

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
