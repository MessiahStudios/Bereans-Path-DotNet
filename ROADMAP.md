# Bereans Path — Product Roadmap

**North star:** A real, sharable Bible study + church-finder product people can open on phone or desktop — not just a resume stub.

**Live repo:** https://github.com/MessiahStudios/Bereans-Path-DotNet  
**Stack:** Vue 3 · ASP.NET Core · EF Core · Bootstrap · ESV API · OpenStreetMap / Overpass

---

## Product definition of done

Someone can:

1. Open a **public HTTPS URL** (or install as PWA)
2. Read Scripture, bookmark it, find nearby churches that fit Bereans Path values
3. Switch themes and use it comfortably on **mobile**
4. Trust it enough to share with a friend / church volunteer

Job applications are a **side benefit**, not the finish line.

---

## Where we are (Aug 2026)

**Local product is strong.** Reader, bookmarks (with note memoirs), Resources, Settings/themes/PWA, and a values-aligned church finder all work on `localhost:5173` + API `5068`.

**Next gate to “shareable product”:** Azure live URL, then screenshots / short demo.

---

## Status

### Done
- [x] Vue + ASP.NET rewrite (Reader, Bookmarks, Churches)
- [x] ESV proxy + EF bookmarks + saved churches
- [x] Bookmark note memoirs (archive prior notes on update)
- [x] Themes (Faith / Scripture — Messiah Studios look)
- [x] PWA installability (`vite-plugin-pwa`)
- [x] Mobile polish on core screens
- [x] Resources tab (study methodology, audio, external links)
- [x] System & Settings (appearance, health, optional logs)
- [x] Diagnostics / log viewer (API + Settings)
- [x] GitHub public repo
- [x] Single-host publish path (Vue builds into API `wwwroot`)
- [x] **Church finder — values filter** (Protestant / non-denom preferred; exclude Catholic, Eastern Orthodox, LDS, JW, etc.)
- [x] **Church detail panel** (website, directions, seminary guidance, save)
- [x] **Suggest a church** (persisted suggestions API)
- [x] Overpass proxied via API with mirrors + retries (fixes 406 / busy 504s)

### Now
- [ ] Azure live URL (+ optional `bereans.messiahstudios.site`)
- [ ] README screenshots + short demo video
- [ ] Curate more church enrichment notes (website / seminary) for Phoenix West Valley favorites

### Later
- [ ] Simple auth / synced bookmarks across devices
- [ ] SQL Server in production
- [ ] Push notifications
- [ ] Admin review UI for church suggestions

---

## Build order (remaining)

| Step | Outcome |
|---|---|
| **C. Azure** | Always-on public URL |
| **D. Showcase packaging** | Screenshots, demo clip, polished README |

Apply to jobs **after** C (or with a local demo video if Azure waits).

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

- UI: http://localhost:5173/
- API: http://localhost:5068/

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
