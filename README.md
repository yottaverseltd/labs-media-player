# labs-media-player

Cross-platform podcast lab in Uno: RSS enclosures and **MediaPlayerElement** (HTML audio on WebAssembly, native stacks on Windows desktop and Android). Optional Cloudflare Worker under `worker/` for WASM CORS.

**Live:** https://yottaverseltd.github.io/labs-media-player/

**Desktop (Windows):** https://github.com/yottaverseltd/labs-media-player/releases/tag/continuous — download `labs-media-player-net9.0-desktop.zip` (refreshed on every successful `main` build).

**Android:** https://github.com/yottaverseltd/labs-media-player/releases/tag/continuous — download `labs-media-player-net9.0-android.apk`. Sideload only; expect an ephemeral CI signing key (“unknown publisher”).

**Source:** https://github.com/yottaverseltd/labs-media-player

Version-tagged [Releases](https://github.com/yottaverseltd/labs-media-player/releases) (`v*`) still receive desktop zips from [`release-desktop.yml`](.github/workflows/release-desktop.yml).

## What you get

- **Browser:** Use the live URL or publish WASM locally with `-p:WasmShellWebAppBasePath=/labs-media-player/` to match GitHub Pages.
- **Desktop / Android:** Grab binaries from the **continuous** prerelease, or run locally with `dotnet run -f net9.0-desktop` / deploy the APK from CI.
- **RSS:** Default feed is NPR Planet Money (`https://feeds.npr.org/510289/podcast.xml`); paste any feed URL and tap **Load feed**.
- **CORS helper:** Browsers may block cross-origin RSS. The Worker proxies `GET {your-worker}/?url={encodedFeedUrl}`. Desktop reads feeds directly; the proxy is mainly for WASM.

## Run locally

```powershell
dotnet workload install wasm-tools android   # once per machine
dotnet restore
dotnet build -c Release
dotnet run --project LabsMediaPlayer/LabsMediaPlayer.csproj -f net9.0-desktop
```

Publish WASM (same base path as Pages):

```powershell
dotnet publish LabsMediaPlayer/LabsMediaPlayer.csproj -c Release -f net9.0-browserwasm -p:WasmShellWebAppBasePath=/labs-media-player/
```

## Stack

- Uno single project, Skia renderer, **MediaPlayerElement** feature.
- RSS via **System.ServiceModel.Syndication** (optional `itunes:duration` when present).
- **CI:** [`ci.yml`](.github/workflows/ci.yml) — builds wasm + desktop + android; on `main`, ships **continuous** prerelease assets and workflow artifacts.
- **Pages:** [`deploy-pages.yml`](.github/workflows/deploy-pages.yml) — WASM to GitHub Pages with `WasmShellWebAppBasePath=/labs-media-player/`, `.nojekyll` for `_framework`.

## License

MIT. See `LICENSE`.
