using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Misskey_SMTC.Media;
using System;
using Windows.ApplicationModel.DataTransfer;

namespace Misskey_SMTC.Pages;

public sealed partial class Info : Page
{
    private readonly DispatcherTimer m_updateTimer;

    // App.Media.NowPlaying を直接参照（x:Bind用）
    public NowPlayingViewModel SongInfo => App.Media.NowPlaying;

    public Info()
    {
        InitializeComponent();

        m_updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        m_updateTimer.Tick += async (_, _) => await App.Media.RefreshAsync();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        m_updateTimer.Start();
        await App.Media.RefreshAsync();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        m_updateTimer.Stop();
    }

    private void CopyText_Click(object sender, RoutedEventArgs args)
    {
        if (sender is not Button btn || btn.Tag is not string infoContent)
            return;

        var package = new DataPackage();
        package.SetText(infoContent);
        Clipboard.SetContent(package);
    }
}
