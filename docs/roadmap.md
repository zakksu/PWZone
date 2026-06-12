# PW Companion — Roadmap

Personal companion for Perfect World 1.8.7 (The Classic Games). Timeline is flexible; foundation quality comes first.

---

## Phase 1 — Live Map + Gathering ✅ (MVP Foundation)

**Status:** Core delivered in this repo skeleton.

| Feature | Status |
|---------|--------|
| WPF tray desktop app | ✅ |
| Memory reader (read-only, pointer chains) | ✅ |
| Hot-reload `offsets.json` | ✅ |
| WebSocket broadcaster | ✅ |
| React + Leaflet live map | ✅ |
| Player dot + facing direction | ✅ |
| Static gathering nodes + filters + search | ✅ |
| Nearest resource highlight + distance + line | ✅ |
| Serilog structured logging | ✅ |
| Debug window (F12) | ✅ |
| xUnit tests + CI | ✅ |
| Sample map + resource JSON | ✅ |

**Remaining Phase 1 polish (post-skeleton):**
- [ ] Real offsets for TCG PW 1.8.7 build
- [ ] Real map tiles from sMAPtool for Archosaur + starter zones
- [ ] Coordinate calibration pass per map
- [ ] Resource data entry for commonly farmed nodes

---

## Phase 2 — Gathering God Mode

| Feature | Description |
|---------|-------------|
| Respawn timers | User-logged harvest timestamps → countdown per node |
| Harvest history | SQLite log of what/when/where |
| Heatmaps | Visual density of personal harvest activity |
| Farm routes | Saved circuits with estimated yield/time |
| Optimal path hints | Suggest next node based on respawn state |

**Architecture prep:** SQLite schema in `data/user-data/`, `IHarvestTracker` service, WebMap overlay layers.

---

## Phase 3 — Character Intelligence

| Feature | Description |
|---------|-------------|
| Inventory/equip offsets | Read gear and stats from memory |
| Build validator | Compare current build vs intended template |
| PWDatabase integration | Item/skill lookup for suggestions |
| DPS/sim tools | Local calculation engine |

**Architecture prep:** Extend `OffsetConfiguration` with modular offset groups; version per game patch.

---

## Phase 4 — Economy Slayer

| Feature | Description |
|---------|-------------|
| Manual price logger | Quick-entry UI for auction prices |
| Screenshot OCR | Optional Tesseract pipeline for price capture |
| Price history | SQLite time series per item |
| Arbitrage alerts | Cross-city spread detection |
| "Sell now" hints | Based on local history trends |

**Architecture prep:** `IPriceStore`, OCR adapter interface, alert notification service.

---

## Phase 5 — QoL Suite

| Feature | Description |
|---------|-------------|
| Calendar / dailies tracker | Reset timers, completion checkboxes |
| Quest helper | Map pins for objectives (data-driven JSON) |
| Instance timers | Track cooldowns and lockouts |
| Pain Point Radar | Position-based contextual tips |

---

## Phase 6 — Notes & Personality

| Feature | Description |
|---------|-------------|
| Location-based notes | Pin notes to map coordinates |
| Sarcasm mode | Butler-flavored note rendering |
| Export/import | Share note packs between installs |

---

## Phase 7 — Multi-Account

| Feature | Description |
|---------|-------------|
| Process picker | Select among running PW clients |
| Per-character profiles | Separate data namespaces |
| Quick switch | Tray menu account swap |

---

## Phase 8 — Advanced

| Feature | Description |
|---------|-------------|
| Local LLM chat | "Butler, best farm spot for iron?" |
| Exportable reports | PDF/CSV farming summaries |
| Voice commands | Optional Windows speech |
| External tool hooks | Webhook/API for community tools |

---

## Phase 9 — Accessibility & Polish

| Feature | Description |
|---------|-------------|
| Dark mode | ✅ (default in WebMap) |
| High-contrast theme | WebMap CSS theme switch |
| Keyboard navigation | Full map UI without mouse |
| Reduced motion | Respect `prefers-reduced-motion` |

---

## Principles (All Phases)

1. **Read-only** — never risk bans
2. **Personal use** — no monetization
3. **Extensible** — interfaces over concrete classes
4. **Tested** — no merge without green CI
5. **Delightful** — dry humor is a feature, not a bug
