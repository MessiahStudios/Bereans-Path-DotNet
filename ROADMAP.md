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

**Local product is strong and mission-clear.** Path (What & Why), gated Reader flow, bookmarks with memoirs, Resources, Settings/themes/PWA, and a values-aligned church finder all work on `localhost:5173` + API `5068`.

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
- [x] Resources tab (audio first; methodology behind “Show details”)
- [x] **Path / What & Why** (mission, Acts 17, how the app is used)
- [x] Reader UX gate: book + chapter → Read unlocks audio, notes, bookmark together
- [x] System & Settings (appearance, health, optional logs)
- [x] Diagnostics / log viewer (API + Settings)
- [x] GitHub public repo
- [x] Single-host publish path (Vue builds into API `wwwroot`)
- [x] Church finder — values filter, detail panel, directions, seminary guidance
- [x] Suggest a church (persisted suggestions API)
- [x] Overpass proxied via API with mirrors + retries

### Now (shareable)
1. **Azure live URL** (+ optional `bereans.messiahstudios.site`) — see `docs/AZURE.md`
2. **README screenshots + short demo video**
3. Curate more church enrichment notes (website / seminary) for Phoenix West Valley favorites

### Later / explore
- [ ] Follow-along word highlight while audio plays *(not feasible with the current ESV iframe player — would need owned audio + timings or a different playback approach)*
- [ ] Simple auth / synced bookmarks across devices
- [ ] SQL Server in production
- [ ] Push notifications
- [ ] Admin review UI for church suggestions

---

## Build order (remaining)

| Step | Outcome |
|---|---|
| **C. Azure** | Always-on public URL you can share |
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
