# Azure hosting — Bereans Path (product)

**Live URL:** https://bereans.messiahstudios.site  
**Azure hostname:** `bereans-path-h9gpe2h9dyh0d9gu.canadacentral-01.azurewebsites.net`

Goal: one public URL that serves the Vue app + ASP.NET API together.

## Architecture

1. `npm run build` in `src/bereans-path.web` → outputs to `src/BereansPath.Api/wwwroot`
2. `dotnet publish` the API (includes `wwwroot`)
3. Deploy that publish folder to **one** Azure App Service (Linux, **.NET 10**)

## Portal settings

- Runtime: **.NET 10** · OS: **Linux**
- App setting: `ESV_API_KEY`
- Custom domain + managed SSL: `bereans.messiahstudios.site`
- Cloudflare DNS: CNAME `bereans` → Azure hostname (**DNS only**)

## Local publish

```powershell
cd src/bereans-path.web
npm install
npm run build

cd ../BereansPath.Api
dotnet publish -c Release -o ../../publish
```

## Deploy (Azure CLI)

```powershell
Compress-Archive -Path publish\* -DestinationPath publish.zip -Force
az login
az webapp deploy --resource-group <your-rg> --name bereans-path --src-path publish.zip --type zip
```

## Notes

- Keep secrets in App Settings — never in git.
- SQLite on App Service is fine for early product.
- Custom domains require Basic+ (already upgraded).
