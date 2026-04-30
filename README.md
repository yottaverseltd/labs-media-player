# labs-media-player

Tagline: **Uno, audio, and restraint.**

Minimal Uno Platform podcast demo (Skia desktop + WebAssembly). Browse episodes from an RSS feed, pick enclosures, and play audio through `MediaPlayerElement` (HTML audio path on WASM, native stack on desktop).

## Default feed

Stable public RSS used in-app by default:

`https://feeds.npr.org/510289/podcast.xml` (NPR Planet Money)

Paste any other feed URL in the field and tap **Load feed**.

## CORS proxy (WASM)

Browsers block anonymous RSS reads across origins. Use the tiny Cloudflare Worker under `worker/`:

```powershell
cd worker
npm install -g wrangler
wrangler login
wrangler deploy
```

Your Worker URL looks like `https://labs-media-player-rss.<your-subdomain>.workers.dev`.

Paste that base into **CORS proxy base** in the app. The client requests:

`GET {proxy}/?url={Uri.EscapeDataString(feedUrl)}`

Desktop Skia can call the feed URL directly; the proxy remains optional.

## Live site

GitHub Pages deploy workflow publishes WASM with `WasmShellWebAppBasePath=/labs-media-player/`.

After the first successful run on `main`:

`https://yottaverseltd.github.io/labs-media-player/`

Replace org/user if the fork lives elsewhere.

## Build locally

```powershell
dotnet workload install wasm-tools
dotnet build LabsMediaPlayer/LabsMediaPlayer.csproj -f net9.0-desktop
dotnet publish LabsMediaPlayer/LabsMediaPlayer.csproj -c Release -f net9.0-browserwasm -p:WasmShellWebAppBasePath=/labs-media-player/
```

## Stack

- Uno single project, `net9.0-browserwasm` + `net9.0-desktop`, Skia renderer, `MediaPlayerElement` feature.
- RSS: `System.ServiceModel.Syndication` + optional `itunes:duration`.
- CI: `.github/workflows/deploy-pages.yml` installs `wasm-tools`, publishes WASM, uploads Pages artifact.

See `AGENTS.md` for editorial and code rules.
