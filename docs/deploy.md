# Deploy PW Companion

## One-time GitHub setup (5 minutes)

1. Push this repo to GitHub (create repo `PWZone` or any name).
2. **Settings → Pages → Build and deployment → Source:** GitHub Actions.
3. Push to `main` — workflows run automatically:
   - **Deploy Web Map** → live map URL
   - **Build Desktop Companion** → Windows `.zip` artifact
   - **CI** → tests on every push/PR

## Your public map URL

After the first successful **Deploy Web Map** run:

| Setup | URL |
|-------|-----|
| Default (project pages) | `https://YOUR_GITHUB_USER.github.io/PWZone/` |
| Custom domain | Add `src/WebMap/public/CNAME` with your domain, then `https://yourdomain.com/` |

Replace `PWZone` with your actual repo name if different.

## Custom domain (free GitHub Pages)

1. Copy `src/WebMap/public/CNAME.example` → `src/WebMap/public/CNAME`
2. Edit `CNAME` — one line: `pw.yourdomain.com`
3. In your domain DNS (where you bought the domain):
   - **A records** → GitHub Pages IPs: `185.199.108.153`, `185.199.109.153`, `185.199.110.153`, `185.199.111.153`
   - **OR CNAME** → `YOUR_GITHUB_USER.github.io`
4. GitHub repo → **Settings → Pages → Custom domain** → enter same domain
5. Push to `main` — deploy uses base path `/` automatically when `CNAME` exists

## Text to share (copy-paste)

```
PW Companion live map: https://YOUR_GITHUB_USER.github.io/PWZone/
Offline demo auto-starts. For live player tracking, run PW Companion desktop on your PC while playing.
```

## What works on the hosted map

| Feature | Hosted (GitHub Pages) | Local dev |
|---------|----------------------|-----------|
| Map + gathering nodes | Yes | Yes |
| Offline demo | Yes | Yes |
| Filters / search | Yes | Yes |
| Live player from PW | Needs desktop app on same PC | Yes |

Live tracking: hosted map tries `ws://127.0.0.1:17847/` on your machine when the desktop companion is running.

## Download desktop companion

1. GitHub → **Actions** → **Build Desktop Companion** → latest run
2. Download artifact **PWCompanion-win-x64.zip**
3. Unzip, run `PWCompanion.exe`, press F12 for debug

## Local build (without GitHub)

```powershell
powershell -File scripts/build-all.ps1
powershell -File scripts/build-webmap.ps1 -BasePath "/PWZone/"
```

Output: `src/WebMap/dist` and `publish/companion/`

## Manual redeploy

GitHub → **Actions** → **Deploy Web Map** → **Run workflow**

Optional input `base_path`: `/` for custom domain, `/RepoName/` for project pages.
