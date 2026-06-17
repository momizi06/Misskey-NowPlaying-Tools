using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Media.Control;

namespace Misskey_SMTC.Media;

public sealed partial class NowPlayingViewModel : INotifyPropertyChanged
{
    private string? m_title;
    private string? m_subtitle;
    private string? m_artist;
    private string? m_albumArtist;
    private string? m_albumTitle;
    private int? m_trackNumber;
    private int? m_albumTrackCount;
    private IReadOnlyList<string>? m_genres;
    private string? m_playbackType;
    private string? m_id;
    private bool m_appleMusicCompat = true;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string? Title
    {
        get => m_title; set => SetProperty(ref m_title, value);
    }
    public string? Subtitle
    {
        get => m_subtitle; set => SetProperty(ref m_subtitle, value);
    }
    public string? Artist
    {
        get => m_artist; set => SetProperty(ref m_artist, value);
    }
    public string? AlbumArtist
    {
        get => m_albumArtist; set => SetProperty(ref m_albumArtist, value);
    }
    public string? AlbumTitle
    {
        get => m_albumTitle; set => SetProperty(ref m_albumTitle, value);
    }
    public int? TrackNumber
    {
        get => m_trackNumber; set => SetProperty(ref m_trackNumber, value);
    }
    public int? AlbumTrackCount
    {
        get => m_albumTrackCount; set => SetProperty(ref m_albumTrackCount, value);
    }
    public string? PlaybackType
    {
        get => m_playbackType; set => SetProperty(ref m_playbackType, value);
    }
    public string? Id
    {
        get => m_id; set => SetProperty(ref m_id, value);
    }

    public IReadOnlyList<string>? Genres
    {
        get => m_genres;
        set
        {
            if (SetProperty(ref m_genres, value))
                OnPropertyChanged(nameof(GenresText));
        }
    }

    public string GenresText => Genres == null ? string.Empty : string.Join(", ", Genres);

    /// <summary>
    /// Apple Music 互換モード（Id が AppleInc.AppleMusicWin で始まる場合に有効化できる）
    /// </summary>
    public bool AppleMusicCompat
    {
        get => m_appleMusicCompat;
        set
        {
            if (SetProperty(ref m_appleMusicCompat, value))
            {
                // トグル時に表示用プロパティを再通知
                OnPropertyChanged(nameof(DisplayArtist));
                OnPropertyChanged(nameof(DisplayAlbum));
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["AppleMusicCompat"] = value;
            }
        }
    }

    /// <summary>Apple Music 互換モードが有効かつ対象アプリのとき true</summary>
    private bool IsAppleMusicActive =>
        AppleMusicCompat && (Id?.StartsWith("AppleInc.AppleMusicWin") ?? false);

    /// <summary>表示用アーティスト名</summary>
    public string? DisplayArtist
    {
        get
        {
            if (IsAppleMusicActive && Artist != null)
            {
                var parts = Artist.Split(" — ");
                return parts.Length >= 1 ? parts[0] : Artist;
            }
            return Artist;
        }
    }

    /// <summary>表示用アルバム名（Apple Music 互換時は Artist フィールドから取得）</summary>
    public string? DisplayAlbum
    {
        get
        {
            if (IsAppleMusicActive && Artist != null)
            {
                var parts = Artist.Split(" — ");
                return parts.Length >= 2 ? parts[1] : AlbumTitle;
            }
            return AlbumTitle;
        }
    }

    public void UpdateFrom(
        GlobalSystemMediaTransportControlsSessionMediaProperties props,
        string? id)
    {
        Title = props.Title;
        Subtitle = props.Subtitle;
        Artist = props.Artist;
        AlbumArtist = props.AlbumArtist;
        AlbumTitle = props.AlbumTitle;
        TrackNumber = props.TrackNumber;
        AlbumTrackCount = props.AlbumTrackCount;
        Genres = props.Genres;
        PlaybackType = props.PlaybackType.ToString();
        Id = id;

        // 曲が変わったら Display 系も再通知
        OnPropertyChanged(nameof(DisplayArtist));
        OnPropertyChanged(nameof(DisplayAlbum));
    }

    public void Clear()
    {
        Title = Subtitle = Artist = AlbumArtist = AlbumTitle = PlaybackType = Id = string.Empty;
        TrackNumber = AlbumTrackCount = null;
        Genres = null;

        OnPropertyChanged(nameof(DisplayArtist));
        OnPropertyChanged(nameof(DisplayAlbum));
    }

    private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
            return false;
        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged(string? propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
