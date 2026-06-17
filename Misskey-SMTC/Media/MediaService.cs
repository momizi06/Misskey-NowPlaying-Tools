using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WindowsMediaController;

namespace Misskey_SMTC.Media;

/// <summary>
/// アプリ全体で共有する MediaManager ラッパー（シングルトン）
/// </summary>
public sealed partial class MediaService : IDisposable
{
    private MediaManager? m_mediaManager;
    private bool m_isUpdating;

    public NowPlayingViewModel NowPlaying { get; } = new();

    /// <summary>曲情報が更新されたとき</summary>
    public event EventHandler? NowPlayingUpdated;

    public void Start()
    {
        if (m_mediaManager != null)
            return;

        m_mediaManager = new MediaManager();
        m_mediaManager.Start();

        m_mediaManager.OnAnySessionOpened += HandleSessionChanged;
        m_mediaManager.OnAnySessionClosed += HandleSessionChanged;
        m_mediaManager.OnFocusedSessionChanged += HandleSessionChanged;
    }

    public void Stop()
    {
        if (m_mediaManager == null)
            return;

        m_mediaManager.OnAnySessionOpened -= HandleSessionChanged;
        m_mediaManager.OnAnySessionClosed -= HandleSessionChanged;
        m_mediaManager.OnFocusedSessionChanged -= HandleSessionChanged;
    }

    private async void HandleSessionChanged(MediaManager.MediaSession _)
        => await RefreshAsync();

    public async Task RefreshAsync()
    {
        if (m_mediaManager == null || m_isUpdating)
            return;

        m_isUpdating = true;
        try
        {
            var session = m_mediaManager.GetFocusedSession();
            if (session == null)
            {
                NowPlaying.Clear();
            }
            else
            {
                var props = await session.ControlSession.TryGetMediaPropertiesAsync();
                NowPlaying.UpdateFrom(props, session.Id);
            }

            NowPlayingUpdated?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[MediaService] Failed to refresh: {ex.Message}");
        }
        finally
        {
            m_isUpdating = false;
        }
    }

    public void Dispose() => Stop();
}
