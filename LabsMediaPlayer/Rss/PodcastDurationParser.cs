using System.Globalization;

namespace LabsMediaPlayer.Rss;

internal static class PodcastDurationParser
{
    internal static TimeSpan? Parse(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        raw = raw.Trim();

        if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var totalSeconds))
        {
            return TimeSpan.FromSeconds(totalSeconds);
        }

        var parts = raw.Split(':');
        try
        {
            return parts.Length switch
            {
                2 => new TimeSpan(0, int.Parse(parts[0], CultureInfo.InvariantCulture), int.Parse(parts[1], CultureInfo.InvariantCulture)),
                3 => new TimeSpan(int.Parse(parts[0], CultureInfo.InvariantCulture), int.Parse(parts[1], CultureInfo.InvariantCulture), int.Parse(parts[2], CultureInfo.InvariantCulture)),
                _ => null,
            };
        }
        catch
        {
            return null;
        }
    }
}
