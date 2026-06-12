# Memory Offsets Guide — PW 1.8.7 (The Classic Games)

The companion reads player position and map ID from game memory using pointer chains defined in `offsets.json`. **Placeholder values ship with the repo** — you must discover and enter real offsets for your client build.

---

## Quick Start

1. Launch Perfect World (The Classic Games client).
2. Open PW Companion — it auto-attaches to `elementclient.exe`.
3. Press **F12** to open the Debug Window.
4. If player coordinates show as invalid, update `offsets.json`.
5. Save the file — offsets hot-reload without restarting the app.

---

## offsets.json Structure

```json
{
  "Version": "1.8.7-tcg-v1",
  "ProcessName": "elementclient",
  "ModuleName": "elementclient.exe",
  "PollIntervalMs": 100,
  "PlayerBase": {
    "BaseOffset": 0,
    "Offsets": [0x1234567, 0x40, 0x10],
    "Description": "Pointer chain to player object"
  },
  "PositionX": { "Offset": 0x0, "Description": "Player X" },
  "PositionY": { "Offset": 0x4, "Description": "Player Y (height)" },
  "PositionZ": { "Offset": 0x8, "Description": "Player Z" },
  "MapId": { "Offset": 0x10, "Description": "Current map ID" },
  "Facing": { "Offset": 0x14, "Description": "Facing radians", "Optional": true }
}
```

### Fields

| Field | Description |
|-------|-------------|
| `ProcessName` | Process name without `.exe` |
| `ModuleName` | Main module for base address lookup |
| `PlayerBase.BaseOffset` | Offset from module base to start of chain |
| `PlayerBase.Offsets` | Array of pointer offsets `[base+off] → read ptr → +off → ...` |
| `PositionX/Y/Z.Offset` | Byte offset from resolved player address |
| `MapId.Offset` | Int32 map/world identifier |
| `Facing.Offset` | Float radians (optional) |

---

## Finding Offsets (Cheat Engine)

### 1. Attach to elementclient.exe

Run PW, open Cheat Engine, attach to `elementclient.exe`.

### 2. Find position floats

- Move character in a straight line along X axis.
- Scan for **Float** values that change predictably.
- Repeat for Z (north/south movement).
- Y is typically height/altitude.

### 3. Find pointer chain

- Right-click found address → "Pointer scan for this address".
- Filter for static paths through `elementclient.exe` module.
- Prefer chains with 2–4 levels; fewer is more stable across sessions.

### 4. Find map ID

- Change maps (teleport or walk across zone boundary).
- Scan for **4 Bytes** (Int32) that changes with map.
- Often near player struct or in a separate world-manager object.

### 5. Validate

- Stand still: values stable.
- Move: X/Z update smoothly.
- Change map: MapId updates.
- Restart client: chain still resolves (static base).

---

## Translating to offsets.json

Example CE pointer path:

```
"elementclient.exe"+0x00ABCDEF → [+0x40] → [+0x10] → position
```

Becomes:

```json
"PlayerBase": {
  "BaseOffset": 0xABCDEF,
  "Offsets": [0x40, 0x10]
}
```

If position X is at the final address + 0x0:

```json
"PositionX": { "Offset": 0 }
```

---

## Versioning

Update `"Version"` whenever offsets change. The debug window and WebSocket status broadcast this string so you know which config is active.

Recommended naming: `1.8.7-tcg-YYYYMMDD` or `1.8.7-tcg-patch-N`.

---

## Troubleshooting

| Symptom | Likely cause | Fix |
|---------|--------------|-----|
| "Not attached" | PW not running | Launch game first |
| Invalid player state | Wrong pointer chain | Re-scan with CE |
| X/Y/Z garbage | Wrong field offsets | Adjust Position* offsets |
| Map ID wrong | Offset points to wrong field | Re-find map ID |
| Works then breaks after patch | Static offsets changed | Full re-scan |
| Access denied | Permissions | Run companion as admin (rare) |

Log hint from the butler: *"Offsets probably outdated — see docs/offsets-guide.md"*

---

## Phase 3+ Offsets (Future)

Additional offset groups will be added as separate JSON sections:

```json
"Inventory": { ... },
"Equipment": { ... },
"Skills": { ... }
```

Each group hot-reloads independently.

---

## Safety Reminder

**Read only.** Never use CE or any tool to write memory while playing on a live server if you care about account safety. PW Companion itself never writes — but your offset discovery workflow is your responsibility.
