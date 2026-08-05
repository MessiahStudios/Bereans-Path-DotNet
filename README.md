# Bereans Path

<p align="center">
  <img src="src/bereans-path.web/public/bereans-path-logo.png" alt="Bereans Path" width="360" />
</p>

<p align="center">
  <strong>Searching the Scriptures daily to find the truth.</strong><br />
  <em>Acts 17:11</em>
</p>

<p align="center">
  <a href="https://bereans.messiahstudios.site"><strong>Live app</strong></a>
  ·
  <a href="https://messiahstudios.site">Messiah Studios</a>
  ·
  <a href="docs/AZURE.md">Azure hosting</a>
</p>

Bereans Path is a personal Scripture reader, bookmark journal, and values-aligned church finder. Read the Word, keep what God presses on your heart, and find Protestant and non-denominational churches that treat Scripture as the final authority — the same care the Bereans were commended for in Acts 17.

## What you can do

| Pillar | In the app |
| --- | --- |
| **Read** | Open the ESV (proxied server-side), pick book + chapter, then unlock audio, notes, and bookmark together |
| **Remember** | Bookmarks with notes and note memoirs when a note is updated |
| **Walk with** | Nearby churches via OpenStreetMap / Overpass, filtered and ranked for biblical-authority fit |
| **Grow** | Resources (audio first; study methodology behind details) and a Path / What & Why screen |

Also included: themes (including Faith / Scripture to match Messiah Studios), PWA install support, Settings + diagnostics, and suggest-a-church.

**Church finder note:** Results favor Protestant and non-denominational congregations. Traditions that rest on authorities beyond the closed canon of Scripture are set aside.

## Stack

- **Frontend:** Vue 3 · Vite · Bootstrap · PWA
- **Backend:** ASP.NET Core (.NET 10) · EF Core · SQLite (local / early Azure)
- **Integrations:** ESV API (server proxy) · Overpass (API-proxied church search)

One host in production: the Vue build lands in the API `wwwroot`, so Azure serves UI + API from a single App Service.

## Quick start (development)

```powershell
# Terminal 1 — API (http://localhost:5068)
cd src/BereansPath.Api
dotnet user-secrets set "ESV_API_KEY" "your-key"
dotnet run --launch-profile http

# Terminal 2 — Vue (http://localhost:5173)
cd src/bereans-path.web
npm install
npm run dev
```

## Product build (one host)

```powershell
cd src/bereans-path.web
npm install
npm run build

cd ../BereansPath.Api
dotnet run --launch-profile http
```

Open http://localhost:5068 — full UI + API together.

## Deploy

See [`docs/AZURE.md`](docs/AZURE.md). GitHub Actions can deploy on push to `main` when `AZURE_WEBAPP_PUBLISH_PROFILE` is set.

**Live:** https://bereans.messiahstudios.site

## Roadmap

See [`ROADMAP.md`](ROADMAP.md) — live URL is up; screenshots / short demo and further church enrichment are next.
