using System.ServiceModel.Syndication;
using System.Xml;

namespace LabsMediaPlayer.Rss;

internal static class PodcastRssParser
{
    private const string ITunesNs = "http://www.itunes.com/dtds/podcast-1.0.dtd";

    internal static ParsedPodcastFeed Parse(string xml)
    {
        using var reader = XmlReader.Create(new StringReader(xml));
        var feed = SyndicationFeed.Load(reader);

        var podcastTitle = feed.Title?.Text ?? "Podcast";

        var episodes = new List<PodcastEpisode>();
        foreach (var item in feed.Items)
        {
            var audio = PickAudioUri(item);
            if (audio is null)
            {
                continue;
            }

            var title = item.Title?.Text ?? "Episode";
            DateTimeOffset? published = item.PublishDate == default ? null : item.PublishDate;
            var duration = TryReadItunesDuration(item);

            episodes.Add(new PodcastEpisode(title, published, duration, audio));
        }

        return new ParsedPodcastFeed(podcastTitle, episodes);
    }

    private static Uri? PickAudioUri(SyndicationItem item)
    {
        foreach (var link in item.Links)
        {
            if (!string.Equals(link.RelationshipType, "enclosure", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (link.Uri is null)
            {
                continue;
            }

            var mt = link.MediaType ?? string.Empty;
            if (mt.StartsWith("audio/", StringComparison.OrdinalIgnoreCase) ||
                mt.Contains("mpeg", StringComparison.OrdinalIgnoreCase) ||
                mt.Contains("mp4", StringComparison.OrdinalIgnoreCase) ||
                mt.Contains("aac", StringComparison.OrdinalIgnoreCase))
            {
                return link.Uri;
            }
        }

        foreach (var link in item.Links)
        {
            if (string.Equals(link.RelationshipType, "enclosure", StringComparison.OrdinalIgnoreCase) && link.Uri is not null)
            {
                return link.Uri;
            }
        }

        return null;
    }

    private static TimeSpan? TryReadItunesDuration(SyndicationItem item)
    {
        foreach (var ext in item.ElementExtensions)
        {
            if (!string.Equals(ext.OuterName, "duration", StringComparison.Ordinal) ||
                !string.Equals(ext.OuterNamespace, ITunesNs, StringComparison.Ordinal))
            {
                continue;
            }

            using var er = ext.GetReader();
            er.MoveToContent();
            var text = er.ReadElementContentAsString();
            return PodcastDurationParser.Parse(text);
        }

        return null;
    }
}

internal sealed record ParsedPodcastFeed(string PodcastTitle, IReadOnlyList<PodcastEpisode> Episodes);
