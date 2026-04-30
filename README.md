# labs-media-player

Cross-platform podcast playback in Uno: browse an RSS feed, pick enclosure URLs, and listen through **MediaPlayerElement** (HTML audio on WebAssembly, native stack on Windows desktop).

**Live:** https://yottaverseltd.github.io/labs-media-player/

**Source:** https://github.com/yottaverseltd/labs-media-player

## Downloads

### Desktop (Windows)

Zip builds attach to **[GitHub Releases](https://github.com/yottaverseltd/labs-media-player/releases)** when you push a version tag (`v*`). The **`release-desktop`** workflow (see [`.github/workflows/release-desktop.yml`](.github/workflows/release-desktop.yml)) publishes **`labs-media-player-net9.0-desktop.zip`** from **`net9.0-desktop`**.

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

## License

MIT. See `LICENSE`.
