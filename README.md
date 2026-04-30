# labs-media-player

Cross-platform podcast lab in Uno: RSS enclosures and **MediaPlayerElement** (HTML audio on WebAssembly, native stacks on Windows desktop and Android). Optional Cloudflare Worker under `worker/` for WASM CORS.

## Try it on GitHub Pages

**Live:** https://yottaverseltd.github.io/labs-media-player/

**Prerequisites:** none for the WASM shell (RSS may need the Worker or a CORS-friendly feed). **Desktop zip** and **Android APK** download from the **`continuous`** prerelease only after a green **`ci`** run on **`main`**:
https://github.com/yottaverseltd/labs-media-player/releases/tag/continuous

**Troubleshooting:** If the live URL returns 404, open **Settings → Pages → Build and deployment** and set **Source** to **GitHub Actions**, then re-run the latest **deploy-pages** workflow.

**Desktop (Windows):** https://github.com/yottaverseltd/labs-media-player/releases/tag/continuous — download `labs-media-player-net9.0-desktop.zip` (refreshed on every successful `main` build).

**Android:** https://github.com/yottaverseltd/labs-media-player/releases/tag/continuous — download `labs-media-player-net9.0-android.apk`. Sideload only; expect an ephemeral CI signing key (“unknown publisher”).

**Source:** https://github.com/yottaverseltd/labs-media-player

Version-tagged [Releases](https://github.com/yottaverseltd/labs-media-player/releases) (`v*`) still receive desktop zips from [`release-desktop.yml`](.github/workflows/release-desktop.yml).

## What you get

- **Browser:** Use the live URL or publish WASM locally with `-p:WasmShellWebAppBasePath=/labs-media-player/` to match GitHub Pages.
- **Desktop / Android:** Grab binaries from the **continuous** prerelease, or run locally with `dotnet run -f net9.0-desktop` / deploy the APK from CI.
- **iOS:** Build on a **Mac** with Xcode (`net9.0-ios`); see **iOS (macOS + Xcode)** below. There is no signed IPA in CI without Apple Developer signing setup.
- **RSS:** Default feed is NPR Planet Money (`https://feeds.npr.org/510289/podcast.xml`); paste any feed URL and tap **Load feed**.
- **CORS helper:** Browsers may block cross-origin RSS. The Worker proxies `GET {your-worker}/?url={encodedFeedUrl}`. Desktop reads feeds directly; the proxy is mainly for WASM.

## Run locally

```powershell
dotnet workload install wasm-tools android   # once per machine
dotnet restore
dotnet build -c Release
dotnet run --project LabsMediaPlayer/LabsMediaPlayer.csproj -f net9.0-desktop
```

## iOS (macOS + Xcode)

**IPA / TestFlight / App Store** need an [Apple Developer Program](https://developer.apple.com/programs/) membership, distribution signing certificates, and provisioning profiles. This repo does **not** ship signed IPAs from CI until you add those secrets and workflows yourself.

**Local build on a Mac** (install [.NET 9 SDK](https://dotnet.microsoft.com/download), [Xcode](https://developer.apple.com/xcode/), and the iOS workload):

```bash
dotnet workload install ios wasm-tools android   # once per machine
cd /path/to/labs-media-player
dotnet restore
dotnet build LabsMediaPlayer/LabsMediaPlayer.csproj -c Release -f net9.0-ios
dotnet publish LabsMediaPlayer/LabsMediaPlayer.csproj -c Release -f net9.0-ios -o ./publish/ios
```

Uno Platform reference: [Publishing your app for iOS](https://platform.uno/docs/articles/uno-publishing-ios.html).

**CI:** The [ios-build.yml](.github/workflows/ios-build.yml) workflow runs on **`workflow_dispatch`** only (`macos-14`), builds `net9.0-ios`, and **uploads a `*.app` artifact when the build produces one** (unsigned; if no `.app` is emitted without signing, the upload step may skip with a warning).

Publish WASM (same base path as Pages):

```powershell
dotnet publish LabsMediaPlayer/LabsMediaPlayer.csproj -c Release -f net9.0-browserwasm -p:WasmShellWebAppBasePath=/labs-media-player/
```

## Stack

- Uno single project, Skia renderer, **MediaPlayerElement** feature.
- RSS via **System.ServiceModel.Syndication** (optional `itunes:duration` when present).
- **CI:** [`ci.yml`](.github/workflows/ci.yml) — builds wasm + desktop + android; on `main`, ships **continuous** prerelease assets and workflow artifacts.
- **CI iOS:** [`ios-build.yml`](.github/workflows/ios-build.yml) — manual `net9.0-ios` build on **macOS** only; no **TestFlight** without **Apple** signing secrets.
- **Pages:** [`deploy-pages.yml`](.github/workflows/deploy-pages.yml) — WASM to GitHub Pages with `WasmShellWebAppBasePath=/labs-media-player/`, `.nojekyll` for `_framework`.

## License

MIT. See `LICENSE`.
