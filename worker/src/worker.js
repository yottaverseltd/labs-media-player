const cors = {
  "Access-Control-Allow-Origin": "*",
  "Access-Control-Allow-Methods": "GET, HEAD, OPTIONS",
  "Access-Control-Allow-Headers": "Content-Type",
};

export default {
  /**
   * GET https://<worker>/?url=<encoded-feed-url>
   * Proxies RSS/XML with permissive CORS for browser clients.
   */
  async fetch(request) {
    if (request.method === "OPTIONS") {
      return new Response(null, { status: 204, headers: cors });
    }

    if (request.method !== "GET" && request.method !== "HEAD") {
      return new Response("Method not allowed", { status: 405, headers: cors });
    }

    const feedUrl = new URL(request.url).searchParams.get("url");
    if (!feedUrl) {
      return new Response("Missing url query parameter", { status: 400, headers: cors });
    }

    let target;
    try {
      target = new URL(feedUrl);
    } catch {
      return new Response("Invalid url", { status: 400, headers: cors });
    }

    if (target.protocol !== "http:" && target.protocol !== "https:") {
      return new Response("Only http(s) feeds allowed", { status: 400, headers: cors });
    }

    try {
      const upstream = await fetch(target.toString(), {
        headers: { "User-Agent": "labs-media-player-rss-proxy/1.0" },
        redirect: "follow",
      });

      const body =
        request.method === "HEAD" ? null : await upstream.arrayBuffer();

      const contentType =
        upstream.headers.get("content-type") ?? "application/xml";

      return new Response(body, {
        status: upstream.status,
        headers: {
          ...cors,
          "Content-Type": contentType,
          "Cache-Control": "public, max-age=300",
        },
      });
    } catch (err) {
      const message = err instanceof Error ? err.message : String(err);
      return new Response(message, { status: 502, headers: cors });
    }
  },
};
