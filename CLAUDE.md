# Haus — Home Assistant CLI

A command-line interface for Home Assistant built on .NET 10. Licensed GPL-3.0.

## Build & Run

```bash
dotnet build
dotnet run --project src/Haus -- <command> [options]
dotnet test
dotnet format
```

## Tech Stack

- **Runtime**: .NET 10 (C# 14)
- **CLI**: `Spectre.Console.Cli` + `Spectre.Console`
- **HA Communication**: `HassClient.WS` (WebSocket), `IHassApiClient` (REST)
- **Serialization**: `System.Text.Json`
- **DI**: `Microsoft.Extensions.DependencyInjection`
- **Testing**: xUnit v3

## Project Structure

```
src/Haus/
├── Auth/                    # OAuth2 PKCE, token storage, browser helper
├── Connection/              # IHassApiClient (REST), IHassConnection (WS)
├── Commands/                # CLI commands, grouped by API domain
│   ├── State/               #   haus state list|get|set|delete
│   └── ...                  #   (future: Service/, Event/, Device/, etc.)
├── Output/                  # OutputHelper (table vs JSON rendering)
└── Program.cs               # DI + command registration
```

## Architecture Rules

- **Commands** inherit `AsyncCommand<TSettings>` with nested `Settings` class. Thin handlers — delegate to services.
- **Use shared infrastructure.** REST commands use `IHassApiClient`, WebSocket commands use `IHassConnection`. Never create raw `HttpClient` or `HassWSApi` in a command.
- **No in-memory filtering.** Only expose CLI flags/options that map to actual API query parameters. If the API doesn't support filtering, neither does the CLI.
- **Output**: default human-readable tables, `--json` for machine-readable. Errors to stderr, data to stdout. Use `OutputHelper`.
- **Fail fast** with actionable messages (e.g., "Run `haus login` or set HASS_URL/HASS_TOKEN.").
- Commands grouped in subfolders by API domain (`Commands/State/`, `Commands/Service/`, etc.).
- `async Task` everywhere, no `async void`. Pass `CancellationToken` through all async I/O.
- HA entity models as records.
- Entity IDs follow `<domain>.<object_id>` format.

## Coding Conventions

- File-scoped namespaces
- Primary constructors where they improve readability
- `var` when type is obvious
- Nullable reference types enabled
- Methods ~30 lines max
- Async methods suffixed with `Async`
- No `#region` blocks
- Constants over magic strings/numbers
- C# 14 `field` keyword where it reduces boilerplate

## HassClient API Reference

- `HassWSApi.ConnectAsync(connectionParams)` — connect and authenticate
- `HassWSApi.ListStatesAsync()` — all entity states
- `HassWSApi.AddEventHandlerSubscriptionAsync(handler, "state_changed")` — real-time events
- `HassWSApi.CallServiceAsync(domain, service, data)` — control entities
- `HassWSApi.GetEntityRegistryEntriesAsync()` — entity metadata
- `HassWSApi.GetAreasAsync()` / `GetDevicesAsync()` — organizational data

## Configuration

OAuth2 browser login (`haus login`), or env vars: `HASS_URL`, `HASS_TOKEN`.
