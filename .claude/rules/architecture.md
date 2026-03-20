# Architecture Rules

## CLI Command Model

Haus uses `Spectre.Console.Cli` for verb-based CLI dispatch.

- **Commands** inherit `AsyncCommand<TSettings>` with a nested `Settings` class. Each command is a thin handler that delegates to a service.
- **Services** handle HA communication and business logic. Commands depend on service interfaces.
- **Output formatters** handle rendering results to stdout (table, JSON, etc.).

## Feature Namespaces

Organize by feature, not by technical layer:

- `Haus.Auth` — OAuth2 PKCE, token storage, browser helper
- `Haus.Connection` — Shared HassWSApi connection wrapper
- `Haus.Commands` — CLI command definitions
- `Haus.Output` — Output formatting utilities
- `Haus` (root) — App entry point, host setup

Each feature should have its own service if it needs HA communication. A shared `IHassConnection` provides the underlying WebSocket connection.

## Async & Threading

- No `async void` — use `async Task` everywhere.
- Pass `CancellationToken` through all async I/O operations.
- Commands receive a `CancellationToken` from Spectre.Console.Cli and pass it through.

## Models

- HA entity models should be records where practical (immutable by default).

## Error Resilience

- CLI commands should fail fast with clear error messages — no silent swallowing.
- Connection failures should produce actionable messages (e.g., "Cannot connect to HA. Run `haus login` or set HASS_URL/HASS_TOKEN.").

## Output

- Default to human-readable table/text output.
- Support `--json` flag for machine-readable JSON output (pipeable to `jq`).
- Write errors to stderr, data to stdout.

## HassClient API Reference

HassClient handles connection, auth, reconnection, and message framing.

- `HassWSApi.ConnectAsync(connectionParams)` — connect and authenticate
- `HassWSApi.ListStatesAsync()` — fetch all entity states
- `HassWSApi.AddEventHandlerSubscriptionAsync(handler, "state_changed")` — real-time state changes
- `HassWSApi.CallServiceAsync(domain, service, data)` — control entities
- `HassWSApi.GetEntityRegistryEntriesAsync()` — rich entity metadata (area, device, platform)
- `HassWSApi.GetAreasAsync()` / `GetDevicesAsync()` — organizational data

Entity IDs follow `<domain>.<object_id>` format (e.g., `light.kitchen`, `sensor.temperature`).
