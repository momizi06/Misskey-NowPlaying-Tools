using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using Misskey_SMTC.Creditional;
using Misskey_SMTC.Media;
using Misskey_SMTC.Misskey;
using Misskey_SMTC.Pages.EditTemplate;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Misskey_SMTC.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class Post : Page, INotifyPropertyChanged
{
    public ObservableCollection<ServerEntry> Servers => ServerStore.Servers;
    public NowPlayingViewModel SongInfo => App.Media.NowPlaying;
    //private readonly CreditionalManager m_creditionalManager = new();
    private readonly DispatcherTimer m_updateTimer;
    public event PropertyChangedEventHandler? PropertyChanged;
    private EditTemplateWindow? m_editTemplateWindow;
    public bool CanPost => !string.IsNullOrEmpty(SongInfo.Id);


    public string TemplateText
    {
        get => PostTemplate.Template;
        set
        {
            PostTemplate.Template = value;
            OnPropertyChanged(nameof(PreviewText));
        }
    }

    public string PreviewText => PostTemplate.Template
        .Replace("{title}", SongInfo.Title ?? "（曲名）")
        .Replace("{artist}", SongInfo.DisplayArtist ?? "（アーティスト）")
        .Replace("{album}", SongInfo.DisplayAlbum ?? "（アルバム）")
        .Replace("{subtitle}", SongInfo.Subtitle ?? "（サブタイトル）")
        .Replace("{albumartist}", SongInfo.AlbumArtist ?? "（アルバムアーティスト）")
        .Replace("{tracknumber}", SongInfo.TrackNumber.ToString() ?? "（トラック番号）")
        .Replace("{albumtrackcount}", SongInfo.AlbumTrackCount.ToString() ?? "（総トラック数）")
        .Replace("{genres}", SongInfo.GenresText ?? "（ジャンル）")
        .Replace("{url}", "https://music.apple.com/...");

    public Post()
    {
        InitializeComponent();
        _ = ServerStore.Servers;

        App.Media.NowPlaying.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(NowPlayingViewModel.Id)
                               or nameof(NowPlayingViewModel.AppleMusicCompat)
                               or nameof(NowPlayingViewModel.DisplayArtist)
                               or nameof(NowPlayingViewModel.DisplayAlbum))
            {
                OnPropertyChanged(nameof(PreviewText));
            }

            if (e.PropertyName == nameof(NowPlayingViewModel.Id))
                OnPropertyChanged(nameof(CanPost));
        };

        m_updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        m_updateTimer.Tick += async (_, _) => await App.Media.RefreshAsync();
    }

    private async void OnPostNowPlaying(object sender, RoutedEventArgs e)
    {
        // Handle the case where no server is selected
        var selectedHost = ServerSelector.SelectedItem as ServerEntry ?? throw new InvalidOperationException("No server selected");

        var postContent = await PostTemplate.BuildAsync(SongInfo);

        var m_note = new Note(
        host: selectedHost.ServerUrl,
        token: CreditionalManager.LoadToken(selectedHost.ServerUrl)
               ?? throw new InvalidOperationException("No credential for selected server"),
        content: postContent);

        await m_note.PostAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                // Log or display the error message as needed
                System.Diagnostics.Debug.WriteLine(task.Exception?.GetBaseException());
                AppNotification notification = new AppNotificationBuilder() 
                .AddText("Post Failed")
                .AddText($"{task.Exception?.GetBaseException()}")
                .BuildNotification();

                AppNotificationManager.Default.Show(notification);

            }
            else
            {
                // Optionally handle successful post completion
                System.Diagnostics.Debug.WriteLine("Post successful");

                AppNotification notification = new AppNotificationBuilder()
                .AddText("Post successful")
                .AddText($"Success post NowPlaying note to {selectedHost.ServerUrl}")
                .BuildNotification();

                AppNotificationManager.Default.Show(notification);
            }
        });
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        m_updateTimer.Start();
        await App.Media.RefreshAsync();

        // デフォルトサーバーを自動選択
        var defaultEntry = ServerStore.Servers.FirstOrDefault(s => s.IsDefault);
        if (defaultEntry != null)
            ServerSelector.SelectedItem = defaultEntry;
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        m_updateTimer.Stop();
    }

    private void OnPropertyChanged(string name)
    => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private void OnEditTemplate(object sender, RoutedEventArgs e)
    {
        // 既に開いていたら前面に出す
        if (m_editTemplateWindow != null)
        {
            m_editTemplateWindow.Activate();
            return;
        }

        m_editTemplateWindow = new EditTemplateWindow();

        // 閉じたらプレビューを更新
        m_editTemplateWindow.Closed += (_, _) =>
        {
            m_editTemplateWindow = null;
            OnPropertyChanged(nameof(PreviewText));
        };

        m_editTemplateWindow.Activate();
    }

    public string GetDisplayName(string serverUrl)
    {
        var config = ServerSettings.Get(serverUrl);
        return string.IsNullOrEmpty(config.DisplayName) ? serverUrl : config.DisplayName;
    }
}
