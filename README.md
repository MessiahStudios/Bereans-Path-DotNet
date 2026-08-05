# Bereans Path (.NET rewrite)

Bible study + church finder rebuilt with **Vue 3**, **ASP.NET Core**, **EF Core**, and **Bootstrap** — portfolio piece aligned with church/ministry product work (e.g. Rock RMS stack family).

## Structure

```
src/
  BereansPath.Api/     ASP.NET Core Web API (C#, EF, REST)
  bereans-path.web/    Vue 3 + Vite frontend
ROADMAP.md
```

## Prerequisites

- .NET 8 SDK
- Node.js 20+
- Free [ESV API key](https://api.esv.org/) (optional for UI shell; required for Scripture fetch)

## Configure ESV key (API)

```powershell
cd src/BereansPath.Api
dotnet user-secrets init
dotnet user-secrets set "ESV_API_KEY" "your-key-here"
```

Or set environment variable `ESV_API_KEY`.

## Run locally

Terminal 1 — API (Swagger at http://localhost:5068/swagger):

```powershell
cd src/BereansPath.Api
dotnet run --launch-profile http
```

Terminal 2 — Vue (http://localhost:5173):

```powershell
cd src/bereans-path.web
npm install
npm run dev
```

## Database

Default: **SQLite** file `bereans.db` (created on first run).

To use **SQL Server** (LocalDB example), set in `appsettings.json` or user secrets:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BereansPath;Trusted_Connection=True;TrustServerCertificate=True"
}
```

## API surface

| Method | Path | Purpose |
|---|---|---|
| GET | `/api/esv?q=John+3:16` | ESV passage proxy |
| GET/POST/DELETE | `/api/bookmarks` | Bookmark CRUD |
| GET | `/api/churches/nearby?lat=&lon=` | Nearby churches (Overpass) |
| GET/POST/DELETE | `/api/churches/saved` | Saved churches |
| GET | `/api/diagnostics/health` | Env + whether ESV key is set |
| GET | `/api/diagnostics/logs` | Recent API log lines |

Vue **Logs** page: http://localhost:5173/logs (auto-refresh).  
Log file on disk: `src/BereansPath.Api/logs/bereans-api.log`

## Azure

Do this **after** local flows work — see `ROADMAP.md` Phase 5. Not required for a GitHub portfolio link.

## License / reference

Original Flask PWA lives separately under `Bereans-Path`. This rewrite reimplements behavior; it is not a line-by-line port.
