# Bereans Path — Product Roadmap

**North star:** A real, sharable Bible study + church-finder product people can open on phone or desktop — not just a resume stub.

**Live repo:** https://github.com/MessiahStudios/Bereans-Path-DotNet  
**Stack:** Vue 3 · ASP.NET Core · EF Core · Bootstrap · ESV API · OpenStreetMap

---

## Product definition of done

Someone can:

1. Open a **public HTTPS URL** (or install as PWA)
2. Read Scripture, bookmark it, find nearby churches
3. Switch themes and use it comfortably on **mobile**
4. Trust it enough to share with a friend / church volunteer

Job applications are a **side benefit**, not the finish line.

---

## Status

### Done
- [x] Vue + ASP.NET rewrite (Reader, Bookmarks, Churches, Logs)
- [x] ESV proxy + EF bookmarks + church search
- [x] Diagnostics / log viewer
- [x] GitHub public repo

### Now (product-ready track)
- [ ] Themes (Flask parity)
- [ ] PWA installability
- [ ] Mobile polish
- [ ] Single-host publish (API serves Vue `wwwroot`)
- [ ] Azure live URL (+ optional `bereans.messiahstudios.site`)
- [ ] README screenshots + short demo video

### Later
- [ ] Simple auth / synced bookmarks across devices
- [ ] SQL Server in production
- [ ] Study resources / audio (Flask parity extras)
- [ ] Push notifications

---

## Build order (current)

| Step | Outcome |
|---|---|
| **A. Themes + mobile + PWA** | Feels like a finished client app |
| **B. Single-host build** | One deployable artifact |
| **C. Azure** | Always-on public URL |
| **D. Showcase packaging** | Screenshots, demo clip, polished README |

Apply to jobs **after** at least A–C (or A + local demo video if Azure waits).

---

## Local run (dev)

```powershell
# API
cd src/BereansPath.Api
dotnet run --launch-profile http

# Vue
cd src/bereans-path.web
npm run dev
```

## Product publish (single host)

```powershell
cd src/bereans-path.web
npm run build
cd ../BereansPath.Api
dotnet run --launch-profile http
```

Then open the API URL — it serves the built SPA from `wwwroot`.

---

## Azure (when ready)

See `docs/AZURE.md`. One Linux App Service (.NET 8), set `ESV_API_KEY`, deploy published API (with `wwwroot` filled by Vue build).
