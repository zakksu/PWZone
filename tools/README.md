# Tools

## sMAPtool — Map extraction

PW Companion expects high-resolution map images exported via [sMAPtool](https://github.com/superstylin/sMAPtool) (or compatible tooling).

### Workflow

1. Extract map images from PW 1.8.7 client data.
2. Place images in `src/WebMap/public/maps/` (e.g. `map_1.png`).
3. Update `data/maps.json` with correct `originX`, `originZ`, `scale`, and dimensions.
4. Calibrate by comparing in-game coordinates with map position.

See `docs/debugging.md` for coordinate calibration tips.

## Offset discovery

Use Cheat Engine or x64dbg against a local PW 1.8.7 client (The Classic Games build) to find pointer chains. Document findings in `offsets.json` and `docs/offsets-guide.md`.

**Read-only rule:** Never write to game memory. Companion only uses `ReadProcessMemory`.
