# PW Companion — What YOU Run (Filipe edition)

The butler handles code, GitHub, and builds. You only do this:

---

## One-time setup (5 minutes)

1. **Install .NET 8 SDK** — https://dotnet.microsoft.com/download  
   (Skip if you download the `.zip` from GitHub Actions instead.)

2. **Enable GitHub Pages** (optional — map works locally without this):  
   GitHub → **PWZone → Settings → Pages → Source → GitHub Actions**  
   Until then, the companion serves the map at **http://127.0.0.1:5173/**

3. **Run companion as Administrator once** (helps memory read):  
   Right-click `launch-companion.bat` → **Run as administrator**

---

## Every time you play PW

### Step 1 — Launch Perfect World
Log in with your character. Leave the game running.

### Step 2 — Launch the companion
Double-click:

```
launch-companion.bat
```

It sits in the **system tray** (bottom-right). The map opens in your browser automatically.

### Step 3 — Play
- **Map:** opens automatically at http://127.0.0.1:5173/ (served by the companion)
- **Debug (F12):** see if companion attached to `elementclient_64`
- **Tray menu:** reload offsets, reattach, exit

---

## What “working” looks like

| Debug window (F12) | Meaning |
|--------------------|---------|
| `Attached to elementclient_64` | Memory attach OK |
| `Invalid player state` | Attach OK but **offsets need calibration** (expected for now) |
| `Not attached` | PW not running or wrong process name |

Even with invalid coords, the map **offline demo** still works. Live dot needs offsets (next build phase).

---

## When the butler tells you to do something

| Message | You do |
|---------|--------|
| “Run launch-companion.bat” | Double-click it while PW is open |
| “Press F12 and tell me what you see” | Open debug window, copy status text |
| “Walk 10 meters in-game” | Move character for offset calibration |
| “Enable GitHub Pages” | One-time Settings step above |

---

## Links

- **Live map:** https://zakksu.github.io/PWZone/
- **Repo:** https://github.com/zakksu/PWZone
- **Download desktop app:** GitHub → Actions → Build Desktop Companion → latest green run → artifact `.zip`

---

## You do NOT need to

- Run `npm` or `npm run dev` (map is hosted on GitHub)
- Run PowerShell scripts (butler handles that)
- Edit code unless you want to

That’s it. Launch PW → launch companion → play.
