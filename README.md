# PW Companion

**Filipe's Sarcastic PW Butler** — a personal, read-only companion for Perfect World 1.8.7 on [The Classic Games](https://pw187.theclassic.games).

Live map with player tracking, gathering node overlays, filters, nearest-resource highlighting, and dry humor in every log line.

> *"Player moved 3 meters. Riveting."*

---

## Features (Phase 1 MVP)

- **Windows desktop app** — attaches to `elementclient.exe` via read-only memory access
- **Live WebSocket stream** — player X/Y/Z, map ID, facing direction
- **Interactive map** — Leaflet.js with high-res tiles, dark theme
- **Gathering overlay** — herbs, ores, wood, plants with filters and search
- **Nearest resource** — distance, highlight, dashed line, zoom-to-nearest
- **Hot-reload offsets** — edit `offsets.json` without restart
- **Debug window (F12)** — memory inspection, live state, offset version
- **Structured logging** — Serilog to console + rolling file
- **Tests + CI** — xUnit, WebSocket integration tests, GitHub Actions

**Read-only. No automation. No ban risk. Personal use only.**

---

## Share via link (GitHub Pages)

Push to GitHub → map builds automatically → share one URL (no local server needed for the UI).

| Step | Action |
|------|--------|
| 1 | Push repo to GitHub |
| 2 | **Settings → Pages → Source:** GitHub Actions |
| 3 | Wait for **Deploy Web Map** workflow |
| 4 | Open `https://YOUR_USER.github.io/REPO_NAME/` |

Copy-paste text: see [SHARE.md](SHARE.md). Full guide: [docs/deploy.md](docs/deploy.md).

Custom domain: add `src/WebMap/public/CNAME` (see [CNAME.example](src/WebMap/public/CNAME.example)).

Desktop `.exe`: GitHub **Actions → Build Desktop Companion → download artifact**.

---

## Prerequisites

| Tool | Version |
|------|---------|
| [.NET SDK](https://dotnet.microsoft.com/download) | 8.0+ |
| [Node.js](https://nodejs.org/) | 18+ |
| Windows | 10/11 (x64) |
| Perfect World 1.8.7 | The Classic Games client |

---

## How to Run (Step-by-Step)

### 1. Clone and open

```bash
git clone <your-repo-url> PW-Companion
cd PW-Companion
```

### 2. Build the desktop companion

```bash
dotnet restore PWCompanion.sln
dotnet build PWCompanion.sln -c Release
```

### 3. Install and start the web map

```bash
cd src/WebMap
npm install
npm run dev
```

The map opens at **http://localhost:5173** (Vite dev server).

### 4. Launch Perfect World

Start the PW 1.8.7 client from The Classic Games and log in to a character.

### 5. Start PW Companion

```bash
# From repo root
dotnet run --project src/Companion/UI/PWCompanion.UI.csproj
```

Or run the built executable:

```
src/Companion/UI/bin/Release/net8.0-windows/PWCompanion.exe
```

The app lives in the **system tray**. It will:
- Auto-attach to `elementclient.exe`
- Start WebSocket on `ws://127.0.0.1:17847/`
- Optionally open the browser to the map (configurable)

### 6. Verify everything works

1. Open http://localhost:5173 — sidebar should show **Connected** (or use **Offline demo** if companion isn't running)
2. Press **F12** in the companion — Debug Window shows attach status
3. If player state is **Invalid**, update `offsets.json` (see [Offset Guide](#memory-offsets))
4. Walk in-game — player dot should move on the map (once offsets + map calibration are correct)

---

## Demo Mode (No Game Required)

Waiting for PW to download? You can test everything now.

### Option A — Web map only (easiest)

```bash
cd src/WebMap
npm install
npm run dev
```

Open http://localhost:5173 → click **Start offline demo**. A fake player walks the sample gathering circuit. No .NET, no game, no companion.

### Option B — Full pipeline (companion + map)

`appsettings.json` ships with `DemoMode.Enabled: true`. The desktop app broadcasts a simulated player over WebSocket — same as live mode, minus memory attach.

```bash
dotnet run --project src/Companion/UI/PWCompanion.UI.csproj
# plus npm run dev in src/WebMap
```

When PW is installed, set `"DemoMode": { "Enabled": false }` and restart the companion.

---

## Hotkeys & Tray Menu

| Action | Shortcut / Location |
|--------|---------------------|
| Debug window | **F12** |
| Open map | Tray → Open Map (double-click tray icon) |
| Reload offsets | Tray → Reload Offsets |
| Reattach to PW | Tray → Reattach to PW |
| Exit | Tray → Exit |

---

## Configuration

### appsettings.json

Located next to the executable (`src/Companion/UI/appsettings.json` in dev).

```json
{
  "Companion": {
    "WebSocket": { "Port": 17847, "Host": "127.0.0.1" },
    "Logging": { "MinimumLevel": "Information", "SarcasmInDebug": true },
    "Map": { "WebMapUrl": "http://localhost:5173", "AutoOpenBrowser": true },
    "Debug": { "EnableDebugWindow": true, "DebugHotkey": "F12" },
    "PortableMode": true
  }
}
```

### offsets.json

Memory pointer chains for player position. **Ships with placeholders** — must be filled for your client build.

See [docs/offsets-guide.md](docs/offsets-guide.md) for full instructions.

### Gathering data

| File | Purpose |
|------|---------|
| `data/maps.json` | Map IDs, coordinate transforms, tile paths |
| `data/resources/map_{id}.json` | Gathering nodes per map |
| `data/resources/template.json` | Copy to create new map files |
| `data/resources.schema.json` | JSON schema for validation |

---

## Memory Offsets

Placeholder offsets will **not** read real player data. Use Cheat Engine to find pointer chains for The Classic Games PW 1.8.7 build, then update `offsets.json`.

Quick checklist:
1. Find X/Y/Z floats by moving character
2. Pointer-scan for static chain through `elementclient.exe`
3. Find map ID (Int32) that changes on zone change
4. Update `offsets.json` → save → auto hot-reload
5. Confirm in Debug Window (F12)

Full guide: [docs/offsets-guide.md](docs/offsets-guide.md)

---

## Map Tiles

Sample SVG map included for development. Replace with real tiles:

1. Export maps using [sMAPtool](https://github.com/superstylin/sMAPtool)
2. Place in `src/WebMap/public/maps/` (e.g. `map_1.png`)
3. Update `data/maps.json` with dimensions and coordinate origin
4. Calibrate using [docs/debugging.md](docs/debugging.md)

---

## Project Structure

```
PW-Companion/
├── src/
│   ├── Companion/                  # C# desktop
│   │   ├── Core/                   # Memory, WebSocket, coordinates, DI
│   │   ├── UI/                     # WPF tray app + debug window
│   │   ├── Tests/                  # xUnit tests
│   │   └── Logging/                # Serilog + butler quips
│   └── WebMap/                     # Vite + React + Leaflet
│       ├── public/maps/            # Map tile images
│       └── src/                    # Components, hooks, data
├── data/                           # maps.json, resources/, user-data/
├── docs/                           # architecture, offsets, roadmap, debugging
├── tools/                          # sMAPtool notes, offset discovery
├── tests/                          # Integration test placeholder
├── .github/workflows/ci.yml
├── PWCompanion.sln
└── README.md
```

---

## Development

### Cursor agent workflow

See [AGENTS.md](AGENTS.md) for how Cursor reads terminal state and adjusts course.

| Script | Purpose |
|--------|---------|
| `scripts/status.ps1` | Health check → `.cursor/dev-state.json` |
| `scripts/dev.ps1 -Target webmap` | Start map (foreground, logged) |
| `scripts/dev.ps1 -Target companion` | Start desktop app (demo mode) |
| `scripts/build-all.ps1` | Production build (WebMap + companion if SDK present) |
| `scripts/build-webmap.ps1` | WebMap only (`-BasePath "/RepoName/"`) |

Logs: `logs/dev-session.log` · State: `.cursor/dev-state.json`

### Run tests

```bash
dotnet test PWCompanion.sln --collect:"XPlat Code Coverage"
```

### Lint web map

```bash
cd src/WebMap
npm run lint
npm run typecheck
```

### Production build

```bash
dotnet publish src/Companion/UI/PWCompanion.UI.csproj -c Release -o ./publish/companion
cd src/WebMap && npm run build
# Serve dist/ statically or keep using Vite preview
```

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| "Not attached" | Launch PW before or after companion — it retries every 5s |
| Web map disconnected | Ensure companion is running; check port 17847 |
| Invalid player state | Update `offsets.json` — see offsets guide |
| Player dot wrong position | Calibrate `data/maps.json` transform |
| Nodes in wrong place | Update coordinates in `data/resources/map_*.json` |
| High CPU | Increase `PollIntervalMs` in offsets.json |

Full debugging guide: [docs/debugging.md](docs/debugging.md)

---

## Architecture & Roadmap

- [docs/architecture.md](docs/architecture.md) — system design, protocols, layers
- [docs/roadmap.md](docs/roadmap.md) — Phase 2+ features (timers, economy, build validator, LLM butler)
- [docs/offsets-guide.md](docs/offsets-guide.md) — memory offset discovery
- [docs/debugging.md](docs/debugging.md) — logs, calibration, performance

---

## Philosophy

- **Read-only** — complements the game, never automates risky actions
- **Personal** — built for Filipe, zero monetization
- **Extensible** — Clean Architecture, DI, plugin-ready modules
- **Tested** — high coverage, CI on every push
- **Delightful** — dark theme, fast, veteran PW player personality

---

## License

MIT — personal use. See [LICENSE](LICENSE).

---

## Disclaimer

This tool reads game memory locally for personal informational purposes. Use at your own risk. Always respect The Classic Games terms of service. The butler accepts no responsibility for outdated offsets or missed herbs.
