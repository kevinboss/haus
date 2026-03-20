# Haus — Home Assistant CLI

A command-line interface for Home Assistant built on .NET 10. Licensed GPL-3.0.

## Project Overview

Haus is a CLI tool that wraps the Home Assistant REST and WebSocket APIs (using HassClient) to provide fast, scriptable access to a smart home from the terminal.

### Current Scope

- **Auth**: OAuth2 PKCE browser login, token storage
- **Connection**: HassWSApi wrapper

### Command Design

- `haus login` — OAuth2 browser login
- All other commands mirror the Home Assistant API 1:1 (endpoints map directly to CLI verbs/subcommands)

## Tech Stack

- **Runtime**: .NET 10 (LTS, C# 14)
- **CLI Framework + Console Output**: `Spectre.Console.Cli` + `Spectre.Console` (verb/option parsing, tables, markup, colors)
- **HA Communication**: [HassClient](https://github.com/vicfergar/HassClient) (`HassClient.WS` NuGet)
- **Serialization**: `System.Text.Json` (source-generated where possible)
- **DI / Config / Logging**: `Microsoft.Extensions.Hosting` + `Serilog` file sink
- **Testing**: xUnit v3 (`xunit.v3`)

## Project Structure

```
haus/
├── src/Haus/              # Main CLI application
│   ├── Auth/              # OAuth2 PKCE, token storage, browser helper
│   ├── Connection/        # HassWSApi wrapper, connection state
│   ├── Commands/          # CLI command definitions
│   ├── Output/            # Output formatting (table, json, etc.)
│   └── Program.cs         # Bootstrap + command registration
├── tests/Haus.Tests/      # xUnit v3 tests
└── Haus.sln
```

## Build & Run

```bash
dotnet build
dotnet run --project src/Haus -- <command> [options]
dotnet test
dotnet format
```

## Configuration

Connection via OAuth2 browser login (`haus login`), or environment variables:

Env var overrides: `HASS_URL`, `HASS_TOKEN`.
