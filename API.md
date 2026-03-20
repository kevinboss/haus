# Haus CLI — API Endpoint Mapping

Reference: [Home Assistant REST API](https://developers.home-assistant.io/docs/api/rest) / [WebSocket API](https://developers.home-assistant.io/docs/api/websocket)
API Version: **Home Assistant 2026.3**

## Legend

| Status | Meaning |
|--------|---------|
| -      | Not started |
| WIP    | In progress |
| Done   | Implemented |
| Skip   | Not applicable for CLI |

---

## Auth & Connection

| Status | API Endpoint | CLI Command | Notes |
|--------|-------------|-------------|-------|
| - | OAuth2 PKCE flow | `haus login` | Browser-based login, stores token |
| - | `GET /api/` | `haus status` | Check API connectivity + auth |

## Entities & State

| Status | API Endpoint | CLI Command | Notes |
|--------|-------------|-------------|-------|
| - | `GET /api/states` | `haus state list` | List all entity states |
| - | `GET /api/states/<entity_id>` | `haus state get <entity_id>` | |
| - | `POST /api/states/<entity_id>` | `haus state set <entity_id>` | |
| - | `DELETE /api/states/<entity_id>` | `haus state delete <entity_id>` | |
| - | `config/entity_registry/list` (WS) | `haus entity list` | Rich metadata (area, device, platform) |
| - | `config/entity_registry/get` (WS) | `haus entity get <entity_id>` | |
| - | `config/entity_registry/update` (WS) | `haus entity update <entity_id>` | |
| - | `config/entity_registry/remove` (WS) | `haus entity remove <entity_id>` | |

> **`state` vs `entity`**: `state` operates on runtime state values (the HA state machine). `entity` operates on registry metadata (names, areas, labels, disabled status). This mirrors the HA API distinction between `/api/states` and the entity registry.

## Services

| Status | API Endpoint | CLI Command | Notes |
|--------|-------------|-------------|-------|
| - | `GET /api/services` | `haus service list` | List domains + available services |
| - | `POST /api/services/<domain>/<service>` | `haus service call <domain>.<service>` | Pass data via `--data` JSON or `--entity` shorthand |

## Events

| Status | API Endpoint | CLI Command | Notes |
|--------|-------------|-------------|-------|
| - | `GET /api/events` | `haus event list` | List event types |
| - | `POST /api/events/<event_type>` | `haus event fire <event_type>` | Pass data via `--data` JSON |
| - | `subscribe_events` (WS) | `haus event watch [event_type]` | Stream events to stdout (Ctrl+C to stop) |

## Devices

| Status | API Endpoint | CLI Command | Notes |
|--------|-------------|-------------|-------|
| - | `config/device_registry/list` (WS) | `haus device list` | |
| - | `config/device_registry/update` (WS) | `haus device update <device_id>` | |

## Areas

| Status | API Endpoint | CLI Command | Notes |
|--------|-------------|-------------|-------|
| - | `config/area_registry/list` (WS) | `haus area list` | |
| - | `config/area_registry/create` (WS) | `haus area create <name>` | |
| - | `config/area_registry/update` (WS) | `haus area update <area_id>` | |
| - | `config/area_registry/delete` (WS) | `haus area delete <area_id>` | |

## Floors

| Status | API Endpoint | CLI Command | Notes |
|--------|-------------|-------------|-------|
| - | `config/floor_registry/list` (WS) | `haus floor list` | |
| - | `config/floor_registry/create` (WS) | `haus floor create <name>` | |
| - | `config/floor_registry/update` (WS) | `haus floor update <floor_id>` | |
| - | `config/floor_registry/delete` (WS) | `haus floor delete <floor_id>` | |

## Labels

| Status | API Endpoint | CLI Command | Notes |
|--------|-------------|-------------|-------|
| - | `config/label_registry/list` (WS) | `haus label list` | |
| - | `config/label_registry/create` (WS) | `haus label create <name>` | |
| - | `config/label_registry/update` (WS) | `haus label update <label_id>` | |
| - | `config/label_registry/delete` (WS) | `haus label delete <label_id>` | |

## History & Logbook

| Status | API Endpoint | CLI Command | Notes |
|--------|-------------|-------------|-------|
| - | `GET /api/history/period/<timestamp>` | `haus history <entity_id>` | `--from`/`--to` flags for time range |
| - | `GET /api/logbook/<timestamp>` | `haus logbook` | `--entity`, `--from`/`--to` flags |
| - | `logbook/event_stream` (WS) | `haus logbook watch` | Stream live logbook events |

## Config

| Status | API Endpoint | CLI Command | Notes |
|--------|-------------|-------------|-------|
| - | `GET /api/config` | `haus config show` | |
| - | `config/core/update` (WS) | `haus config update` | |
| - | `POST /api/config/core/check_config` | `haus config check` | Validate configuration files |
| - | `GET /api/components` | `haus integration list` | Loaded integrations |

## Templates

| Status | API Endpoint | CLI Command | Notes |
|--------|-------------|-------------|-------|
| - | `POST /api/template` | `haus template <template>` | Render a Jinja2 template; also accepts `--file` |

## Error Log

| Status | API Endpoint | CLI Command | Notes |
|--------|-------------|-------------|-------|
| - | `GET /api/error_log` | `haus log` | Plaintext error log |
| - | `logger/log_info` (WS) | `haus log levels` | Show current log levels |
| - | `logger/log_level` (WS) | `haus log level <module> <level>` | Set log level |

## Automations & Scripts

| Status | API Endpoint | CLI Command | Notes |
|--------|-------------|-------------|-------|
| - | `trace/list` (WS) | `haus trace list [domain]` | List automation/script traces |
| - | `trace/get` (WS) | `haus trace get <domain> <item_id> <run_id>` | |

## Blueprints

| Status | API Endpoint | CLI Command | Notes |
|--------|-------------|-------------|-------|
| - | `blueprint/list` (WS) | `haus blueprint list <domain>` | |
| - | `blueprint/import` (WS) | `haus blueprint import <url>` | |
| - | `blueprint/save` (WS) | `haus blueprint save` | |
| - | `blueprint/delete` (WS) | `haus blueprint delete` | |

## Repairs

| Status | API Endpoint | CLI Command | Notes |
|--------|-------------|-------------|-------|
| - | `repairs/list_issues` (WS) | `haus repair list` | |
| - | `repairs/ignore_issue` (WS) | `haus repair ignore <issue_id>` | |

## Search

| Status | API Endpoint | CLI Command | Notes |
|--------|-------------|-------------|-------|
| - | `search/related` (WS) | `haus search <type> <id>` | Find related entities/devices/areas/automations |

## Conversation

| Status | API Endpoint | CLI Command | Notes |
|--------|-------------|-------------|-------|
| - | `POST /api/conversation/process` | `haus ask "<text>"` | Natural language intent processing |

## Statistics

| Status | API Endpoint | CLI Command | Notes |
|--------|-------------|-------------|-------|
| - | `recorder/list_statistic_ids` (WS) | `haus stats list` | |
| - | `recorder/statistics_during_period` (WS) | `haus stats get <statistic_id>` | `--from`/`--to` flags |

## Auth Management (Admin)

| Status | API Endpoint | CLI Command | Notes |
|--------|-------------|-------------|-------|
| - | `config/auth/list` (WS) | `haus user list` | |
| - | `config/auth/create` (WS) | `haus user create` | |
| - | `config/auth/delete` (WS) | `haus user delete <user_id>` | |
| - | `config/auth/update` (WS) | `haus user update <user_id>` | |

## Skipped Endpoints

These are skipped because they are integration-specific, internal, or not useful in a CLI context:

| API Endpoint | Reason |
|-------------|--------|
| `camera_proxy` | Binary image data — not useful in a terminal |
| `calendars` | Niche; can add later if needed |
| `config_entries/*` | Integration config management — complex UI flows |
| `lovelace/*` | Dashboard UI config — not relevant to CLI |
| `energy/*` | Highly specialized dashboard data |
| `assist_pipeline/*` | Voice pipeline — requires audio I/O |
| `hassio/*` | Supervisor API — has its own CLI (`ha`) |
| `mobile_app/*` | Mobile push notifications |
| `bluetooth/*` | BLE advertisement streams |
| `zha/*` | Zigbee-specific — very deep integration |
| `webhook/*` | Inbound webhook handling |
| `expose_entity/*` | Voice assistant exposure management |
| `labs/*` | Preview feature toggles |
| `weather/subscribe_forecast` | Streaming forecast — niche |
| `sensor/device_class_*`, `number/device_class_*` | Unit conversion metadata |
