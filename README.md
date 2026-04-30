# labs-media-player

Cross-platform podcast lab in Uno: RSS enclosures and **MediaPlayerElement** on **Windows desktop** and **Android**. **WebAssembly** uses the same Skia shell as `labs-responsive-shell` with RSS + episode chrome; in-browser audio is not wired (blank-canvas risk with the media stack on WASM was the reason).

Optional Cloudflare Worker under `worker/` for WASM CORS.

## Try it on GitHub Pages

**Live:** https://yottaverseltd.github.io/labs-media-player/

**Prerequisites:** none for the WASM shell (RSS may need the Worker or a CORS-friendly feed). **Windows installer / portable zip** and **Android APK** download from the **`continuous`** prerelease only after a green **`ci`** run on **`main`**:
https://github.com/yottaverseltd/labs-media-player/releases/tag/continuous

**Troubleshooting:** If the live URL returns 404, open **Settings → Pages → Build and deployment** and set **Source** to **GitHub Actions**, then re-run the latest **deploy-pages** workflow.

If the WASM app stays on the splash screen or shows a blank page, hard-refresh; ensure the site URL is the repo Pages root (see **Try it on GitHub Pages**). Older builds could fail first paint on WASM when **MediaPlayerElement** was enabled there or when responsive layout skipped the initial grid pass — current `main` uses Skia-only WASM and applies the first narrow/wide layout reliably.

**Desktop (Windows):** https://github.com/yottaverseltd/labs-media-player/releases/tag/continuous — **recommended:** `labs-media-player-win-x64-setup.exe` (Inno installer, per-user install). **Portable:** `labs-media-player-win-x64-self-contained.zip` — unpack and run `LabsMediaPlayer.exe`; includes the .NET runtime (no separate Desktop Runtime). Older `framework-dependent` zips required [.NET 9 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/9.0) and often failed on double-click with no obvious error.

**Android:** https://github.com/yottaverseltd/labs-media-player/releases/tag/continuous — download `labs-media-player-net9.0-android.apk`. Sideload only; expect an ephemeral CI signing key (“unknown publisher”). Store / Play signing is out of scope for this lab; for your own builds, use your keystore and `dotnet publish` with standard Android signing properties.

**Source:** https://github.com/yottaverseltd/labs-media-player

Version-tagged [Releases](https://github.com/yottaverseltd/labs-media-player/releases) (`v*`) still receive the self-contained zip and installer from [`release-desktop.yml`](.github/workflows/release-desktop.yml).

### Portfolio / screenshots

If a marketing thumbnail or capture pipeline used a **fallback frame** while WASM was not rendering in headless automation, replace it by re-capturing against the **live** Pages URL once CI is green — do not pass off a placeholder as a verified runtime screenshot.

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

Publish Windows desktop (self-contained; output folder is runnable on any x64 PC without installing .NET):

```powershell
dotnet publish LabsMediaPlayer/LabsMediaPlayer.csproj -c Release -f net9.0-desktop -r win-x64 -p:SelfContained=true -p:TargetFrameworks=net9.0-desktop -o publish/desktop
```

Then build the installer (requires [Inno Setup 6](https://jrsoftware.org/isinfo.php) on PATH or default install path):

```powershell
& "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe" "$pwd\installer\LabsMediaPlayer.iss"
```

## Stack

- Uno single project, Skia renderer; **MediaPlayerElement** UnoFeature on desktop/Android only (browserwasm stays Skia-only for stable first paint).
- RSS via **System.ServiceModel.Syndication** (optional `itunes:duration` when present).
- **CI:** [`ci.yml`](.github/workflows/ci.yml) — builds wasm + desktop + android; on `main`, ships **continuous** prerelease assets and workflow artifacts.
- **Pages:** [`deploy-pages.yml`](.github/workflows/deploy-pages.yml) — WASM to GitHub Pages with `WasmShellWebAppBasePath=/labs-media-player/`, `.nojekyll` for `_framework`.

## License

MIT. See `LICENSE`.
