# Bereans Path — Roadmap (Vue + ASP.NET rewrite)

**Goal:** Rebuild Bereans Path for portfolio + personal use using Triumph Tech / Rock-adjacent stack: **Vue 3**, **ASP.NET Core (C#)**, **REST APIs**, **Entity Framework**, **SQL Server**, **Bootstrap/CSS**.

**Reference app (do not modify as source of truth):**  
`Dev Projects/Bereans-Path` (Flask/Python PWA) — use for UX and feature behavior only.

**Status:** No prior `ROADMAP.md` existed in the Flask project. This file is the plan for the rewrite.

---

## Answers: GitHub & Azure

| Question | Answer |
|---|---|
| Do you need to give me your GitHub login? | **No.** You connect GitHub yourself when you're ready to push. I only need the **public repo URL** later for README/application links. |
| When do you create the GitHub repo? | **Early (Phase 1)** — empty or scaffold-only repo is fine. Push as you go. |
| When do you touch Azure? | **Last (Phase 5)** — optional. Build and run on your laptop first. Azure is only for a public live URL after the app works locally. |
| Is Azure required to apply? | **No.** Public GitHub + screenshots/video is enough. |

### Suggested order
1. Scaffold locally  
2. Create GitHub repo → push  
3. Build features on laptop  
4. Polish README  
5. *(Optional)* Deploy API (+ frontend) to Azure  
6. Apply with GitHub link (+ live URL if you have it)

---

## Target stack

| Layer | Choice |
|---|---|
| Frontend | Vue 3 + Vite + Bootstrap |
| Backend | ASP.NET Core Web API (C#) |
| Data | EF Core + SQL Server (LocalDB or Docker for dev) |
| Bible text | ESV API proxy on the server (`ESV_API_KEY` in user secrets / Azure App Settings) |
| Maps | Leaflet + OpenStreetMap / Overpass (same idea as Flask app) |
| Hosting (later) | Azure App Service (API); Static Web Apps or same host for Vue |

---

## v1 scope (definition of done)

Must work:

- [x] Read Scripture via ESV proxy (`GET /api/esv`)
- [x] Save / list / delete bookmarks (EF + SQL Server)
- [x] Church finder (map + nearby search)
- [x] Vue UI calling the API (CORS configured)
- [ ] Public GitHub repo with README (setup, screenshots, architecture)
- [x] No secrets in git

Nice to have (after v1):

- [ ] Themes (parity with Flask app)
- [ ] PWA / service worker
- [ ] Simple auth
- [ ] Azure live demo
- [ ] Custom domain e.g. `bereans.messiahstudios.site`

Out of scope for v1:

- Full Rock RMS clone
- Desktop installer
- Payments / complex church CMS

---

## Phases

### Phase 0 — Prep (done when this file exists)
- [x] Confirm rewrite on Vue + ASP.NET
- [x] Create rewrite project folder + this roadmap
- [x] Skim Flask app features once (`replit.md` + UI) and freeze v1 list above

### Phase 1 — Scaffold (Day 1)
- [x] `dotnet new webapi` → `src/BereansPath.Api`
- [x] Vue 3 + Vite app → `src/bereans-path.web`
- [x] Add EF Core + SQL Server packages
- [x] Solution file / folder layout
- [x] `.gitignore` (bin, obj, node_modules, user secrets, `.env`, `appsettings.Development.json` if it has secrets)
- [ ] Create **GitHub** repo → first push (scaffold + this `ROADMAP.md`)
- [x] Document local run commands in README stub

**GitHub:** do it here.  
**Azure:** skip.

### Phase 2 — Backend core (Day 1–2)
- [x] Models: `Bookmark`, optional `SavedChurch` / place
- [x] `AppDbContext` + EnsureCreated (migrations optional later)
- [x] `GET /api/esv` proxy (key via user secrets)
- [x] Bookmark CRUD endpoints
- [x] Seed data
- [x] Swagger / HTTP smoke test that APIs work

**GitHub:** commit when APIs work.  
**Azure:** skip.

### Phase 3 — Frontend core (Day 2–3)
- [x] App shell + Bootstrap layout
- [x] Reader view → calls `/api/esv`
- [x] Bookmarks view → CRUD against API
- [x] Dev proxy or env `VITE_API_BASE_URL`
- [x] Loading / empty / error states

**GitHub:** commit.  
**Azure:** skip.

### Phase 4 — Church finder (Day 3–4)
- [x] Map page (Leaflet)
- [x] Nearby church search (API route and/or Overpass, matching Flask behavior)
- [x] Optional: save a church to DB
- [ ] Mobile-friendly layout pass

**GitHub:** commit.  
**Azure:** still skip until local demo feels solid.

### Phase 5 — Portfolio polish (Day 5)
- [ ] README: why it exists, stack, architecture, screenshots, setup
- [ ] Short screen recording (60–90s)
- [ ] Tag a `v0.1` release on GitHub (optional)
- [ ] **Apply / share GitHub URL**

**Azure (optional, after local works):**
- [ ] Create Azure Web App (Linux, .NET 8)
- [ ] Set `ESV_API_KEY` in Application Settings
- [ ] Deploy API
- [ ] Host Vue (Static Web Apps, or build output on storage/CDN / same plan)
- [ ] Optional custom domain

### Phase 6 — Later enhancements
- [ ] Themes, PWA, auth
- [ ] Hardening for real users
- [ ] C# portfolio narrative for Triumph Tech application

---

## Local success check

Before Azure or applications:

1. API runs on laptop  
2. Vue runs on laptop  
3. Can read a passage, save a bookmark, find a church  
4. Repo is public and clones cleanly with documented setup  

---

## Azure checklist (when you get there)

Use your existing Azure account — treat this as a **new** Web App, not the old Rock-on-Linux attempt.

1. Resource group e.g. `rg-bereans-path`  
2. App Service — **.NET 8**, Linux  
3. SQL: Azure SQL **or** keep SQLite/LocalDB only for demos (SQL Server preferred for job story)  
4. App Setting: `ESV_API_KEY`  
5. Deploy from GitHub Actions or VS / `az webapp`  
6. Confirm HTTPS URL loads API; point Vue at that URL  

Python Flask apps can live on the **same Azure subscription** as separate App Services later — one account, many apps.

---

## Timeline (focused)

| Window | Outcome |
|---|---|
| 5–7 days | v1 locally + GitHub (apply-ready) |
| +2–3 days | Optional Azure live demo |

---

## Notes

- Keep the Flask project around as a **behavior reference**.  
- Prefer a working thin vertical slice over feature parity with every Flask nicety.  
- Orange accents welcome (Triumph Tech joke) — optional brand nod, not required.
