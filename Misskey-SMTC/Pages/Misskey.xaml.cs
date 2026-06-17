using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Misskey_SMTC.Creditional;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Misskey_SMTC.Pages;

public sealed partial class Misskey : Page
{
    // 共有コレクションをそのまま参照（x:Bind用にpublicプロパティで公開）
    public ObservableCollection<ServerEntry> Servers => ServerStore.Servers;

    private CancellationTokenSource? m_authCts;

    public Misskey()
    {
        InitializeComponent();
        UpdateEmptyState();
        ServerStore.Servers.CollectionChanged += (_, _) => UpdateEmptyState();
    }

    private void UpdateEmptyState()
    {
        EmptyInfoBar.Visibility = ServerStore.Servers.Count == 0
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private async void OnLoginClicked(object sender, RoutedEventArgs e)
    {
        var serverUrl = ServerUrlInput.Text.Trim()
            .Replace("https://", "")
            .Replace("http://", "")
            .TrimEnd('/');

        if (string.IsNullOrEmpty(serverUrl))
        {
            SetStatus("サーバーURLを入力してください", isLoading: false);
            return;
        }

        m_authCts = new CancellationTokenSource();
        SetStatus("ブラウザで認証してください…", isLoading: true);
        SetFormEnabled(false);

        try
        {
            var result = await App.MisskeyAuth.GenerateTokenAsync(serverUrl, m_authCts.Token);

            if (result != null)
            {
                ServerStore.Add(serverUrl);
                ServerUrlInput.Text = "";
                AddServerExpander.IsExpanded = false;
                StatusCard.Visibility = Visibility.Collapsed;
            }
            else
            {
                SetStatus("認証に失敗しました", isLoading: false);
            }
        }
        catch (TaskCanceledException)
        {
            SetStatus("認証をキャンセルしました", isLoading: false);
        }
        finally
        {
            SetFormEnabled(true);
        }
    }

    private void OnCancelClicked(object sender, RoutedEventArgs e) => m_authCts?.Cancel();

    private void OnRemoveClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string serverUrl)
            ServerStore.Remove(serverUrl);
    }

    private void OnDisplayNameChanged(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb && tb.Tag is string serverUrl)
        {
            var entry = ServerStore.Servers.FirstOrDefault(s => s.ServerUrl == serverUrl);
            if (entry == null)
                return;

            var config = ServerSettings.Get(serverUrl);
            config.DisplayName = tb.Text.Trim();
            ServerSettings.Set(serverUrl, config);
            entry.RawDisplayName = tb.Text.Trim();
        }
    }

    private void OnDefaultServerToggled(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch ts && ts.IsOn && ts.Tag is string serverUrl)
        {
            // ServerStore 経由で他のエントリのIsDefaultをオフに
            ServerStore.SetDefault(serverUrl);

            // 設定を永続化
            var config = ServerSettings.Get(serverUrl);
            config.IsDefault = true;
            ServerSettings.Set(serverUrl, config);
        }
    }

    private void SetStatus(string message, bool isLoading)
    {
        StatusCard.Visibility = Visibility.Visible;
        StatusTextBlock.Text = message;
        AuthProgressRing.IsActive = isLoading;
    }

    private void SetFormEnabled(bool enabled)
    {
        LoginButton.IsEnabled = enabled;
        CancelButton.IsEnabled = !enabled;
        ServerUrlInput.IsEnabled = enabled;
    }
}
