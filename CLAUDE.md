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
- **HA Communication**: in-house `IHassApiClient` (REST, in `Haus.Rest`) + `IHassWebSocketClient` (WebSocket, in `Haus.Ws`). No third-party HA SDK.
- **Serialization**: `System.Text.Json`
- **DI**: `Microsoft.Extensions.DependencyInjection`
- **Testing**: xUnit v3

## Project Structure

```
src/
├── Haus.Hass/               # Shared: ITokenProvider, HassJsonOptions
├── Haus.Rest/               # IHassApiClient + HassApiClient (HTTP wrapper)
├── Haus.Ws/                 # IHassWebSocketClient + HassWebSocketClient (WS wrapper)
└── Haus/                    # CLI orchestrator
    ├── Auth/                #   OAuth2 PKCE, token storage, browser helper
    │                        #   AuthService implements both IAuthService and ITokenProvider
    ├── Commands/            #   CLI commands, grouped by API domain
    │   ├── State/           #     haus state list|get|set|delete
    │   └── ...
    ├── Output/              #   OutputHelper (table vs JSON rendering)
    └── Program.cs           #   DI + command registration
```

Dependency direction: `Haus → {Haus.Rest, Haus.Ws} → Haus.Hass`. Both libraries take an `ITokenProvider` (not `IAuthService`) so they don't know about the CLI's OAuth2 flow.

## Architecture Rules

- **Mirror nearby components.** Before adding or modifying a command, read 1-2 similar ones in the same or an adjacent folder. Match their flag names (`--from-file`, `--config-id`, `--object-id`), Settings structure, validation style, error messages, and output shape. New components should look like they belong next to the existing ones — divergences should be intentional, not accidental.
- **Commands** inherit `AsyncCommand<TSettings>` with nested `Settings` class. Thin handlers — delegate to services.
- **Use shared infrastructure.** REST commands inject `IHassApiClient` (from `Haus.Rest`), WebSocket commands inject `IHassWebSocketClient` (from `Haus.Ws`). Never create raw `HttpClient` or `ClientWebSocket` in a command.
- **No in-memory filtering via flags.** Don't expose CLI flags/options that imply API-level filtering when the API doesn't support it. Domain-scoped list commands (e.g. `automation list`, `script list` filtering `/api/states` to a single domain) are fine — the command name itself defines the scope, not a user-supplied filter.
- **Each entity-management branch exposes `list`.** Branches that manage configurable entities (`automation`, `script`, `entity`, `state`, ...) should offer a `list` subcommand alongside `get`/`create`/`update`/`delete`. Browsing the inventory is a primary use case and shouldn't require piping `state list` through `grep`.
- **Output**: three modes via `OutputHelper.WriteResult(settings, data, humanOutput, porcelainOutput)`:
  - **Default**: human-friendly output using Spectre.Console. Always invest in making this readable — summarize complex data (e.g. show trigger/action summaries, not raw JSON). Never dump raw JSON as the human output.
  - **`--porcelain`**: plain tab-separated text for grep/cut/awk scripting. Lists use `OutputHelper.WriteColumns` (TSV with header). Key-value uses `OutputHelper.WriteKeyValue` (key\tvalue). Actions output bare identifiers.
  - **`--json`**: structured JSON for machine consumption.
  - Errors to stderr, data to stdout. `HausSettings` implements `IOutputSettings`.
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

## Skills

- When adding or modifying CLI commands, update `.claude/skills/haus/SKILL.md` to reflect the changes.
- When adding a new command scope, add it to `.claude/skills/commit/SKILL.md` under **Scopes**.
- When adding user-facing features, update `README.md` to reflect the changes.

## Configuration

OAuth2 browser login (`haus login`), or env vars: `HASS_URL`, `HASS_TOKEN`.
