# labs-media-player

Cross-platform podcast playback in Uno: browse an RSS feed, pick enclosure URLs, and listen through **MediaPlayerElement** (HTML audio on WebAssembly, native stack on Windows desktop).

**Live:** https://yottaverseltd.github.io/labs-media-player/

**Source:** https://github.com/yottaverseltd/labs-media-player

## Downloads

### Desktop (Windows)

Zip builds attach to **[GitHub Releases](https://github.com/yottaverseltd/labs-media-player/releases)** when a maintainer pushes a version tag (`v*`). The **`release-desktop`** workflow (see [`.github/workflows/release-desktop.yml`](.github/workflows/release-desktop.yml)) publishes **`labs-media-player-net9.0-desktop.zip`** from **`net9.0-desktop`**.

### Mobile / APK

Not shipped from this repo. **`LabsMediaPlayer.csproj`** targets **`net9.0-browserwasm`** and **`net9.0-desktop`** only. On phones and tablets, use the **live WASM** link above in the browser (install as PWA if your browser offers it). There is **no APK** here.

_Suggested repo topics: `uno-platform`, `dotnet`, `wasm`, `skia`, `media-player`, `podcast`, `rss`_

## What you get

- **Browser:** Run the live URL or publish WASM locally with `-p:WasmShellWebAppBasePath=/labs-media-player/` so paths match GitHub Pages.
- **Desktop:** `dotnet run -f net9.0-desktop` or grab a Release zip after a tagged build.
- **RSS:** Default feed is NPR Planet Money (`https://feeds.npr.org/510289/podcast.xml`); paste any feed URL and tap **Load feed**.
- **Optional CORS helper:** Browsers block cross-origin RSS for anonymous requests. The tiny Cloudflare Worker under `worker/` proxies `GET {your-worker}/?url={encodedFeedUrl}`. Deploy with Wrangler, paste the Worker base into **CORS proxy base** in the app. Desktop Skia reads feeds directly; the proxy is WASM-only.

## Run locally

```powershell
dotnet workload install wasm-tools   # once, for WASM
dotnet restore
dotnet build -c Release
dotnet run --project LabsMediaPlayer/LabsMediaPlayer.csproj -f net9.0-desktop
```

Publish WASM (same base path as Pages):

```powershell
dotnet publish LabsMediaPlayer/LabsMediaPlayer.csproj -c Release -f net9.0-browserwasm -p:WasmShellWebAppBasePath=/labs-media-player/
```

## Stack

- Uno single project, Skia renderer, **`MediaPlayerElement`** feature.
- RSS via **`System.ServiceModel.Syndication`** (optional `itunes:duration` when present).
- **CI:** [.github/workflows/ci.yml](.github/workflows/ci.yml) — Release build + WASM publish artifact on push/PR.
- **Pages:** [.github/workflows/deploy-pages.yml](.github/workflows/deploy-pages.yml) — WASM to GitHub Pages with **`WasmShellWebAppBasePath=/labs-media-player/`**, `.nojekyll` for `_framework`.

### GitHub Pages (maintainers)

Workflow permissions align with [labs-responsive-shell](https://github.com/yottaverseltd/labs-responsive-shell): **`permissions: contents: read, pages: write, id-token: write`**, **`actions/upload-pages-artifact@v3`**, **`actions/deploy-pages@v4`**, **`WasmShellWebAppBasePath=/labs-media-player/`**. This workflow **omits `actions/configure-pages`** on purpose (see comments in [.github/workflows/deploy-pages.yml](.github/workflows/deploy-pages.yml)): that action either fails **GET Pages** when no site exists yet, or hits **POST create** with **`enablement: true`**, which often returns **`Resource not accessible by integration`** for organization `GITHUB_TOKEN`.

**One-time (GitHub UI):** in this repository open **Settings → Pages** and set **Build and deployment** source to **GitHub Actions**. That creates the Pages site so **`deploy-pages`** can attach the WASM artifact; the URL is **`https://yottaverseltd.github.io/labs-media-player/`**.

## License

MIT. See `LICENSE`.
