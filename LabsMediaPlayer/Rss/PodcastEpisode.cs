namespace LabsMediaPlayer.Rss;

/// <summary>Parsed podcast episode from RSS enclosure + optional iTunes duration.</summary>
public sealed record PodcastEpisode(
    string Title,
    DateTimeOffset? Published,
    TimeSpan? Duration,
    Uri AudioUri)
{
    /// <summary>Compact subtitle for list rows (date and duration).</summary>
    public string DetailLine =>
        $"{FormatDate(Published)}  ·  {FormatDuration(Duration)}";

    private static string FormatDate(DateTimeOffset? published) =>
        published is { } d ? d.ToLocalTime().ToString("yyyy-MM-dd") : "-";

    private static string FormatDuration(TimeSpan? duration) =>
        duration is not { } d ? "-" : d.TotalHours >= 1
            ? d.ToString(@"h\:mm\:ss", System.Globalization.CultureInfo.InvariantCulture)
            : d.ToString(@"m\:ss", System.Globalization.CultureInfo.InvariantCulture);
}
