# Azure hosting — Bereans Path (product)

Goal: one public URL that serves the Vue app + ASP.NET API together.

## Architecture

1. `npm run build` in `src/bereans-path.web` → outputs to `src/BereansPath.Api/wwwroot`
2. `dotnet publish` the API (includes `wwwroot`)
3. Deploy that publish folder to **one** Azure App Service (Linux, .NET 8)

## Portal steps

1. Azure Portal → **Create resource** → **Web App**
2. Settings:
   - Runtime: **.NET 8**
   - OS: **Linux**
   - Name e.g. `bereans-path` → `https://bereans-path.azurewebsites.net`
3. Configuration → Application settings:
   - `ESV_API_KEY` = your key
   - `DatabaseProvider` = `Sqlite` (start simple) or `SqlServer` later
4. Deploy:
   - **Easiest:** Visual Studio / VS Code Azure App Service extension, or
   - Zip deploy the `dotnet publish` output, or
   - GitHub Actions (see below)

## Local publish check

```powershell
cd src/bereans-path.web
npm install
npm run build

cd ../BereansPath.Api
dotnet publish -c Release -o ../../publish
```

Run the published app locally:

```powershell
cd ../../publish
$env:ESV_API_KEY="your-key"
.\BereansPath.Api.exe
```

Open the printed URL — you should see the full product UI, not Swagger-only.

## Custom domain (later)

Point `bereans.messiahstudios.site` CNAME to your `*.azurewebsites.net` host and add the custom domain in App Service.

## Notes

- Keep secrets in App Settings / Key Vault — never in git.
- SQLite on Azure Linux App Service works for early product; move to Azure SQL when you need multi-instance scale.
- Diagnostics remain at `/logs` in the UI and `/api/diagnostics/logs` on the API.
