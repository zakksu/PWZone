# PW Companion — Architecture

**Codename:** Filipe's Sarcastic PW Butler  
**Target:** Perfect World 1.8.7 · The Classic Games (`pw187.theclassic.games`)  
**Principle:** Read-only companion. Never automates in-game actions.

---

## System Overview

```
┌─────────────────────┐     ReadProcessMemory      ┌──────────────────┐
│  elementclient.exe  │ ◄──────────────────────────│  PW Companion    │
│  (PW 1.8.7 client)  │      (pointer chains)      │  Desktop (WPF)   │
└─────────────────────┘                            └────────┬─────────┘
                                                            │
                                                   WebSocket│ ws://127.0.0.1:17847
                                                            ▼
                                                   ┌──────────────────┐
                                                   │  WebMap          │
                                                   │  Vite+React+     │
                                                   │  Leaflet         │
                                                   └──────────────────┘
```

The desktop app attaches to the running PW client, reads player state from memory using configurable offsets, and broadcasts JSON updates over a local WebSocket. The browser map renders the player position, gathering nodes, filters, and nearest-resource highlighting.

---

## Project Structure

| Path | Responsibility |
|------|----------------|
| `src/Companion/Core/` | Domain logic: memory, WebSocket, coordinates, data services |
| `src/Companion/UI/` | WPF tray app, debug window, hosting bootstrap |
| `src/Companion/Logging/` | Serilog configuration, butler personality helpers |
| `src/Companion/Tests/` | xUnit unit + integration tests |
| `src/WebMap/` | Browser map UI (Vite + React + Leaflet) |
| `data/` | Versioned JSON: maps, resources, user-data (SQLite later) |
| `docs/` | Architecture, offsets, roadmap, debugging |
| `tools/` | Map extraction and offset discovery notes |

---

## Clean Architecture Layers

### Core (no UI dependencies)

- **Models:** `PlayerState`, `OffsetConfiguration`, WebSocket DTOs
- **Memory:** `IMemoryReader` → `ProcessMemoryReader` (Win32 read-only)
- **Memory:** `IPlayerStateReader` → pointer chain resolution + field reads
- **Memory:** `IOffsetConfigurationProvider` → hot-reload from `offsets.json`
- **Map:** `CoordinateTransform` — world X/Z → Leaflet CRS.Simple coords
- **WebSocket:** `IWebSocketBroadcaster` — local HttpListener + WS fan-out
- **Services:** `CompanionHostedService` — attach/poll/broadcast loop
- **Data:** `IGatheringDataService` — loads `data/maps.json` and per-map resources

### UI (WPF)

- Microsoft.Extensions.Hosting for DI + background services
- System tray with map open, debug, offset reload, reattach
- `DebugWindow` (F12): live state, memory inspection, offset version

### WebMap (React)

- `usePlayerWebSocket` hook — reconnecting WS client
- `MapView` — Leaflet image overlay, player marker, resource markers, nearest line
- `Sidebar` — filters, search, connection status, nearest resource panel

---

## Communication Protocol

All messages are JSON over WebSocket (`ws://127.0.0.1:17847/`).

### `player_update`

```json
{
  "type": "player_update",
  "x": 512.0,
  "y": 10.0,
  "z": 768.0,
  "mapId": 1,
  "facing": 1.57,
  "timestamp": 1710000000000,
  "isValid": true
}
```

### `server_status`

```json
{
  "type": "server_status",
  "attached": true,
  "processName": "elementclient",
  "processId": 12345,
  "offsetVersion": "1.8.7-tcg-v1",
  "message": "Attached to elementclient. Let's find some herbs."
}
```

---

## Configuration

| File | Purpose | Hot-reload |
|------|---------|------------|
| `appsettings.json` | WebSocket port, logging, map URL, debug | Yes |
| `offsets.json` | Memory pointer chains and field offsets | Yes (polled) |
| `data/maps.json` | Map metadata and coordinate transforms | Restart web map |
| `data/resources/map_*.json` | Gathering spawn data per map | Restart web map |

**Portable mode:** When `PortableMode: true`, logs and user data stay relative to the executable directory.

---

## Read-Only Enforcement

1. `ProcessMemoryReader` opens process with `PROCESS_VM_READ` only — no write access flags.
2. No input simulation, packet injection, or memory writes anywhere in codebase.
3. Architecture review gate for Phase 2+ features (OCR, timers) — all remain external to game process.

---

## Performance Targets

- Poll interval: 100ms default (configurable in `offsets.json`)
- Idle CPU: < 5% on modern hardware
- WebSocket fan-out: O(clients) per tick, typically 1 client

---

## Future-Proofing (Phase 2+)

| Module | Extension point |
|--------|-----------------|
| Memory | `IMemoryReader` mock for tests; additional readers for inventory/skills |
| Data | SQLite in `data/user-data/` for timers, notes, price history |
| Plugins | `ICompanionModule` interface (planned) — register hosted services + UI panels |
| Multi-account | `IProcessSelector` — enumerate multiple `elementclient` instances |
| LLM Butler | Local API consumer in Core, chat panel in WebMap |

See `roadmap.md` for phased delivery plan.

---

## Testing Strategy

| Layer | Tool | Coverage |
|-------|------|----------|
| Coordinate math | xUnit | Unit |
| Offset JSON load/reload | xUnit | Unit |
| Memory reader (no process) | xUnit | Unit |
| WebSocket broadcast | xUnit | Integration (local WS) |
| Gathering data load | xUnit | Unit |
| Full attach pipeline | xUnit + mock `IMemoryReader` | Integration (planned) |

CI runs `dotnet test` + `npm run typecheck` + `npm run lint` on every push.

---

## Dependency Injection

Registration in `PWCompanion.Core.DependencyInjection`:

```csharp
services.AddSingleton<IMemoryReader, ProcessMemoryReader>();
services.AddSingleton<IOffsetConfigurationProvider>(...);
services.AddSingleton<IPlayerStateReader, PlayerStateReader>();
services.AddSingleton<IWebSocketBroadcaster, WebSocketBroadcaster>();
services.AddHostedService<CompanionHostedService>();
```

UI host calls `AddCompanionCore(contentRoot)` and `AddCompanionSettings(configuration)`.

---

## Security Notes

- WebSocket binds to `127.0.0.1` only — not exposed to LAN
- No telemetry, no external API calls in Phase 1
- Personal use only — no auth layer required for local tooling
