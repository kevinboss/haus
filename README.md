# Haus

> This project is being developed using AI development tools. However, every line of code is reviewed and approved by a human before being committed.

A command-line interface for [Home Assistant](https://www.home-assistant.io/) built on .NET 10.

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

## AI Disclaimer

This project uses AI development tools. Every line of code is reviewed and approved by a human before being committed.

## License

[GPL-3.0](LICENSE)
