# Debugging Guide

Tips for diagnosing PW Companion when things don't work (and they won't, until offsets are calibrated).

---

## Log Files

Logs live in `logs/` next to the executable (portable mode).

```
logs/pw-companion-20250612.log
```

Set minimum level in `appsettings.json`:

```json
"Logging": {
  "MinimumLevel": "Debug",
  "SarcasmInDebug": true
}
```

---

## Debug Window (F12)

| Panel | Shows |
|-------|-------|
| Status | Attach state, PID, WebSocket client count |
| Player State | Live X/Y/Z, map ID, facing |
| Offsets | Active version, poll interval |
| Activity Log | Manual inspection results |

**Inspect Memory** — resolves pointer chain and prints addresses. If chain fails, offsets need updating.

**Reload Offsets** — forces re-read of `offsets.json`.

---

## Common Issues

### Web map shows "Disconnected"

1. Is PW Companion running? (Check system tray.)
2. Is port 17847 free? Change in `appsettings.json` if conflicted.
3. Browser console: look for WebSocket errors.

### Player dot missing on map

1. Debug window: is player state valid?
2. If invalid → fix offsets (see `offsets-guide.md`).
3. If valid but wrong position → calibrate map transform in `data/maps.json`.

### Map image wrong / player off-map

Coordinate transform mismatch. Adjust in `data/maps.json`:

```json
{
  "originX": 0,
  "originZ": 0,
  "scale": 2,
  "imageWidth": 1024,
  "imageHeight": 1024
}
```

**Calibration procedure:**

1. Stand at a known landmark in-game.
2. Note X/Z from debug window.
3. Compare with where the dot appears on map.
4. Adjust `originX`, `originZ`, and `scale` until aligned.
5. Repeat at 2–3 landmarks for accuracy.

### Gathering nodes in wrong places

Node JSON uses world X/Z (same coordinate system as player). Re-record coordinates in-game and update `data/resources/map_*.json`.

---

## Hot-Reload Behavior

| File | Reload |
|------|--------|
| `offsets.json` | Automatic (polled every tick cycle) |
| `appsettings.json` | Requires app restart |
| `data/*.json` | Restart WebMap dev server / refresh |

---

## Performance Profiling

If CPU usage exceeds 5% idle:

- Increase `PollIntervalMs` in `offsets.json` (e.g. 200–500).
- Reduce log level to `Information`.
- Ensure only one WebSocket client connected.

---

## Graceful Degradation

When memory read fails, the companion:

1. Logs a debug/warning message with actionable hint
2. Broadcasts `isValid: false` to the map
3. Keeps WebSocket alive for status messages
4. Retries attach every 5 seconds

The butler does not crash. It judges silently.

---

## Reporting Issues (Personal Project)

Since this is personal tooling for Filipe, issues are tracked informally. When something breaks:

1. Capture log excerpt
2. Note offset version
3. Note PW client patch date
4. Fix offsets or map data

---

## Developer Commands

```bash
# Run tests
dotnet test PWCompanion.sln

# Run desktop app
dotnet run --project src/Companion/UI/PWCompanion.UI.csproj

# Run web map
cd src/WebMap && npm install && npm run dev

# Typecheck web map
cd src/WebMap && npm run typecheck
```
