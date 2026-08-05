# Bereans Path

A product-minded Bible study + church finder app: **Vue 3** frontend, **ASP.NET Core** API, **EF Core**, themes, and PWA install support.

**Repo:** https://github.com/MessiahStudios/Bereans-Path-DotNet

## Features

- Scripture reader (ESV API proxied server-side)
- Bookmarks with optional notes (EF Core)
- Nearby church finder (OpenStreetMap Overpass from the browser — no API key)
- Resources for new believers (ElevenLabs audio + study methodology + trusted links)
- Theme switcher (Faith matches Messiah Studios brand, plus more)
- Diagnostics / live logs
- PWA-ready build
- Single-host product publish (API serves the Vue app from `wwwroot`)

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

## Azure

See [`docs/AZURE.md`](docs/AZURE.md).

## Roadmap

See [`ROADMAP.md`](ROADMAP.md) — product-ready track (themes → PWA → live URL).
