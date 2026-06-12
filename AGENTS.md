# PW Companion — Cursor agent instructions

## Dev workflow (terminal visibility)

Cursor agents **must** use the scripts in `scripts/` so terminal output and state are observable:

| Command | Purpose |
|---------|---------|
| `powershell -File scripts/status.ps1` | Refresh health snapshot → `.cursor/dev-state.json` |
| `powershell -File scripts/dev.ps1 -Target webmap` | Start map server (**foreground** — Cursor sees output) |
| `powershell -File scripts/dev.ps1 -Target companion` | Start desktop companion (demo mode) |
| `powershell -File scripts/test-all.ps1` | Run full test suite |

**Do not** start `npm run dev` in background unless monitoring via `status.ps1` and `logs/dev-session.log`.

## Observability files

- `.cursor/dev-state.json` — machine-readable health (ports, URLs, log tail, hints)
- `logs/dev-session.log` — timestamped dev output for agents

After any shell command that starts/stops services, run `scripts/status.ps1` and read `dev-state.json`.

## URLs

- Web map: http://127.0.0.1:5173/ (offline demo auto-starts)
- Companion health: http://127.0.0.1:17847/health
- WebSocket: ws://127.0.0.1:17847/

## Testing order

1. `scripts/test-all.ps1`
2. `scripts/dev.ps1 -Target webmap` (foreground)
3. Verify http://127.0.0.1:5173/ returns sidebar + map
4. Optionally `scripts/dev.ps1 -Target companion` in second terminal

## Adjusting course

If the user reports blank page / connection refused:

1. Run `scripts/status.ps1`
2. Read last 30 lines of `logs/dev-session.log`
3. Check `dev-state.json` → `webmap.running` and `webmap.lastError`
4. Restart with `scripts/dev.ps1 -Target webmap` in **foreground** (not background)

## Phase 1 scope

Read-only memory companion. Demo mode enabled by default until PW client is available.
