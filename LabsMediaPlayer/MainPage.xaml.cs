using System.Collections.ObjectModel;
using System.Net.Http;
using LabsMediaPlayer.Rss;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
#if !__WASM__
using Microsoft.UI.Xaml.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
#endif
using Windows.Storage;

namespace LabsMediaPlayer;

public sealed partial class MainPage : Page
{
    private const double WideBreakpoint = 820d;
    private const string SettingsFeedKey = "labs.podcast.feed_url";
    private const string SettingsProxyKey = "labs.podcast.proxy_base";

    private static readonly HttpClient SharedHttp = new();

    private readonly ObservableCollection<PodcastEpisode> _episodes = new();
    private readonly DispatcherTimer _positionTimer;

#if !__WASM__
    private readonly MediaPlayerElement _playerElement;

    private bool _sliderProgrammatic;
    private bool _userScrubbing;
    private double _speed = 1d;
#endif

    private bool _isWideLayout;

    private string _podcastTitle = string.Empty;

    public MainPage()
    {
        InitializeComponent();

#if !__WASM__
        _playerElement = new MediaPlayerElement
        {
            AreTransportControlsEnabled = false,
            Width = 1,
            Height = 1,
            Opacity = 0,
            IsHitTestVisible = false,
        };
        NativeAudioHost.Child = _playerElement;
        _playerElement.MediaPlayer.PlaybackSession.PlaybackStateChanged += OnPlaybackStateChanged;
        _playerElement.MediaPlayer.MediaOpened += OnMediaOpened;

        _positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        _positionTimer.Tick += OnPositionTick;
#else
        WasmAudioHint.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        WasmAudioHint.Text =
            "Episode playback ships on Desktop and Android. Here: RSS loader, episodes list, chrome.";

        _positionTimer = new DispatcherTimer();

        SeekBackButton.IsEnabled = false;
        SeekForwardButton.IsEnabled = false;
        Speed1Button.IsEnabled = false;
        Speed125Button.IsEnabled = false;
        Speed15Button.IsEnabled = false;
        Speed2Button.IsEnabled = false;
        PositionSlider.IsEnabled = false;
        PlayPauseButton.IsEnabled = false;
#endif

        Loaded += OnLoaded;
        RootGrid.SizeChanged += OnRootSizeChanged;
        LoadButton.Click += OnLoadClicked;
#if !__WASM__
        PlayPauseButton.Click += OnPlayPauseClicked;
        SeekBackButton.Click += (_, _) => SeekRelative(TimeSpan.FromSeconds(-15));
        SeekForwardButton.Click += (_, _) => SeekRelative(TimeSpan.FromSeconds(30));
        Speed1Button.Click += (_, _) => ApplySpeed(1d);
        Speed125Button.Click += (_, _) => ApplySpeed(1.25d);
        Speed15Button.Click += (_, _) => ApplySpeed(1.5d);
        Speed2Button.Click += (_, _) => ApplySpeed(2d);
        PositionSlider.PointerPressed += OnScrubPointerPressed;
        PositionSlider.PointerReleased += OnScrubPointerReleased;
        PositionSlider.PointerCanceled += OnScrubPointerReleased;
        PositionSlider.ValueChanged += OnSliderValueChanged;
#endif
        EpisodeList.SelectionChanged += OnEpisodeSelected;

#if __WASM__
        RssHintText.Text =
            "Browsers block cross-origin RSS fetches. Paste a Cloudflare Worker base URL from worker/wrangler.toml, or publish from README, then Load.";
#else
        RssHintText.Text =
            "Desktop can fetch feeds directly. You can still use the Worker proxy if you prefer one consistent path.";
#endif
    }

    /// <summary>Bound list for the episode ListView.</summary>
    public ObservableCollection<PodcastEpisode> Episodes => _episodes;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        RestoreFields();
        ApplyLayout(RootGrid.ActualWidth >= WideBreakpoint);
#if !__WASM__
        UpdateSpeedChrome();
        UpdatePlayPauseGlyph();
#endif
    }

    private void OnRootSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ApplyLayout(e.NewSize.Width >= WideBreakpoint);
    }

    private void ApplyLayout(bool wide)
    {
        if (wide == _isWideLayout)
        {
            return;
        }

        _isWideLayout = wide;

        RootGrid.ColumnDefinitions.Clear();
        RootGrid.RowDefinitions.Clear();

        if (wide)
        {
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(320) });
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            RootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            Grid.SetColumn(EpisodeHost, 0);
            Grid.SetRow(EpisodeHost, 0);
            Grid.SetColumnSpan(EpisodeHost, 1);
            Grid.SetRowSpan(EpisodeHost, 1);

            Grid.SetColumn(ChromeHost, 1);
            Grid.SetRow(ChromeHost, 0);
            Grid.SetColumnSpan(ChromeHost, 1);
            Grid.SetRowSpan(ChromeHost, 1);
        }
        else
        {
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            RootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            RootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            Grid.SetColumn(EpisodeHost, 0);
            Grid.SetRow(EpisodeHost, 0);
            Grid.SetColumnSpan(EpisodeHost, 1);
            Grid.SetRowSpan(EpisodeHost, 1);

            Grid.SetColumn(ChromeHost, 0);
            Grid.SetRow(ChromeHost, 1);
            Grid.SetColumnSpan(ChromeHost, 1);
            Grid.SetRowSpan(ChromeHost, 1);
        }
    }

    private void RestoreFields()
    {
        try
        {
            var settings = ApplicationData.Current.LocalSettings.Values;
            if (settings.TryGetValue(SettingsFeedKey, out var feedObj) && feedObj is string feed && !string.IsNullOrWhiteSpace(feed))
            {
                FeedUrlBox.Text = feed;
            }
            else
            {
                FeedUrlBox.Text = FeedDefaults.DefaultFeedUrl;
            }

            if (settings.TryGetValue(SettingsProxyKey, out var proxyObj) && proxyObj is string proxy)
            {
                ProxyUrlBox.Text = proxy;
            }
        }
        catch
        {
            FeedUrlBox.Text = FeedDefaults.DefaultFeedUrl;
        }
    }

    private void PersistFields()
    {
        try
        {
            var settings = ApplicationData.Current.LocalSettings.Values;
            settings[SettingsFeedKey] = FeedUrlBox.Text.Trim();
            settings[SettingsProxyKey] = ProxyUrlBox.Text.Trim();
        }
        catch
        {
            // why: isolated storage can fail on some hosts; feed load still works in-memory
        }
    }

    private static Uri BuildFetchUri(string feedUrl, string proxyBase)
    {
        if (string.IsNullOrWhiteSpace(proxyBase))
        {
            return new Uri(feedUrl);
        }

        var b = proxyBase.TrimEnd('/');
        var q = Uri.EscapeDataString(feedUrl);
        return new Uri($"{b}/?url={q}");
    }

    private async void OnLoadClicked(object sender, RoutedEventArgs e)
    {
        StatusText.Text = string.Empty;
        LoadButton.IsEnabled = false;

        try
        {
            var feedUrl = FeedUrlBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(feedUrl))
            {
                StatusText.Text = "Enter an RSS URL.";
                return;
            }

#if __WASM__
            if (string.IsNullOrWhiteSpace(ProxyUrlBox.Text.Trim()))
            {
                StatusText.Text = "WASM needs a CORS-safe path. Paste your Worker base URL (see README), then Load.";
            }
#endif
            PersistFields();

            var uri = BuildFetchUri(feedUrl, ProxyUrlBox.Text.Trim());
            using var response = await SharedHttp.GetAsync(uri).ConfigureAwait(true);
            response.EnsureSuccessStatusCode();
            var xml = await response.Content.ReadAsStringAsync().ConfigureAwait(true);

            var parsed = PodcastRssParser.Parse(xml);
            _podcastTitle = parsed.PodcastTitle;
            NowPodcastTitle.Text = _podcastTitle;

            _episodes.Clear();
            foreach (var ep in parsed.Episodes)
            {
                _episodes.Add(ep);
            }

            StatusText.Text = _episodes.Count == 0
                ? "No playable enclosures found."
                : $"Loaded {_episodes.Count} episodes.";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Load failed: {ex.Message}";
        }
        finally
        {
            LoadButton.IsEnabled = true;
        }
    }

    private void OnEpisodeSelected(object sender, SelectionChangedEventArgs e)
    {
        if (EpisodeList.SelectedItem is not PodcastEpisode ep)
        {
            return;
        }

        NowEpisodeTitle.Text = ep.Title;
        NowPodcastTitle.Text = _podcastTitle;

#if !__WASM__
        _playerElement.MediaPlayer.Source = MediaSource.CreateFromUri(ep.AudioUri);
        _playerElement.MediaPlayer.PlaybackSession.PlaybackRate = _speed;
        _playerElement.MediaPlayer.Play();
#else
        StatusText.Text = "Episode selected — audio on Desktop/Android (WASM intentionally Skia-only).";
#endif
    }

#if !__WASM__
    private void OnMediaOpened(MediaPlayer sender, object args)
    {
        RefreshDurationUi();
    }

    private void OnPlaybackStateChanged(MediaPlaybackSession sender, object args)
    {
        _ = DispatcherQueue.TryEnqueue(() =>
        {
            UpdatePlayPauseGlyph();
            if (sender.PlaybackState == MediaPlaybackState.Playing)
            {
                _positionTimer.Start();
            }
            else
            {
                _positionTimer.Stop();
            }
        });
    }

    private void OnPlayPauseClicked(object sender, RoutedEventArgs e)
    {
        var session = _playerElement.MediaPlayer.PlaybackSession;
        if (session.PlaybackState == MediaPlaybackState.Playing)
        {
            _playerElement.MediaPlayer.Pause();
        }
        else
        {
            _playerElement.MediaPlayer.Play();
        }

        UpdatePlayPauseGlyph();
    }

    private void UpdatePlayPauseGlyph()
    {
        var session = _playerElement.MediaPlayer.PlaybackSession;
        PlayPauseIcon.Symbol = session.PlaybackState == MediaPlaybackState.Playing
            ? Symbol.Pause
            : Symbol.Play;
    }

    private void SeekRelative(TimeSpan delta)
    {
        var session = _playerElement.MediaPlayer.PlaybackSession;
        var next = session.Position + delta;
        if (next < TimeSpan.Zero)
        {
            next = TimeSpan.Zero;
        }

        var end = session.NaturalDuration;
        if (end != TimeSpan.Zero && next > end)
        {
            next = end;
        }

        session.Position = next;
        RefreshScrubberUi(force: true);
    }

    private void ApplySpeed(double rate)
    {
        _speed = rate;
        _playerElement.MediaPlayer.PlaybackSession.PlaybackRate = rate;
        UpdateSpeedChrome();
    }

    private void UpdateSpeedChrome()
    {
        void style(Button b, double target)
        {
            var on = Math.Abs(_speed - target) < 0.001;
            b.BorderBrush = on ? (Brush)Application.Current.Resources["AccentBrush"]! : (Brush)Application.Current.Resources["BorderBrush"]!;
            b.BorderThickness = new Thickness(on ? 2 : 1);
        }

        style(Speed1Button, 1d);
        style(Speed125Button, 1.25d);
        style(Speed15Button, 1.5d);
        style(Speed2Button, 2d);
    }

    private void OnPositionTick(object? sender, object e)
    {
        RefreshScrubberUi(force: false);
    }

    private void RefreshDurationUi()
    {
        var session = _playerElement.MediaPlayer.PlaybackSession;
        var d = session.NaturalDuration;
        _sliderProgrammatic = true;
        PositionSlider.Maximum = d == TimeSpan.Zero ? 1 : Math.Max(1, d.TotalSeconds);
        _sliderProgrammatic = false;
        RefreshScrubberUi(force: true);
    }

    private void RefreshScrubberUi(bool force)
    {
        if (_userScrubbing && !force)
        {
            return;
        }

        var session = _playerElement.MediaPlayer.PlaybackSession;
        var duration = session.NaturalDuration;
        var position = session.Position;

        _sliderProgrammatic = true;
        if (duration != TimeSpan.Zero)
        {
            PositionSlider.Maximum = Math.Max(1, duration.TotalSeconds);
            PositionSlider.Value = Math.Clamp(position.TotalSeconds, 0, PositionSlider.Maximum);
        }

        _sliderProgrammatic = false;

        TimeLabel.Text = $"{FormatClock(position)} / {FormatClock(duration)}";
    }

    private static string FormatClock(TimeSpan value) =>
        value.TotalHours >= 1
            ? value.ToString(@"h\:mm\:ss")
            : value.ToString(@"m\:ss");

    private void OnScrubPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        _userScrubbing = true;
    }

    private void OnScrubPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (!_userScrubbing)
        {
            return;
        }

        _userScrubbing = false;
        _playerElement.MediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(PositionSlider.Value);
        RefreshScrubberUi(force: true);
    }

    private void OnSliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        if (_sliderProgrammatic || !_userScrubbing)
        {
            return;
        }

        _playerElement.MediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(e.NewValue);
        TimeLabel.Text =
            $"{FormatClock(TimeSpan.FromSeconds(e.NewValue))} / {FormatClock(_playerElement.MediaPlayer.PlaybackSession.NaturalDuration)}";
    }
#endif
}

internal static class FeedDefaults
{
    /// <summary>NPR Planet Money public RSS (stable, permissive for demos).</summary>
    internal const string DefaultFeedUrl = "https://feeds.npr.org/510289/podcast.xml";
}
