using Misskey_SMTC.AP;
using Misskey_SMTC.Media;
using System.Threading.Tasks;
using Windows.Storage;

namespace Misskey_SMTC.Misskey;

public static class PostTemplate
{
    private const string m_settingsKey = "PostTemplate";
    private const string m_defaultTemplate = "#NowPlaying: {title} - {artist} / {album}\n{url}";

    public static string Template
    {
        get => ApplicationData.Current.LocalSettings.Values[m_settingsKey] as string
               ?? m_defaultTemplate;
        set => ApplicationData.Current.LocalSettings.Values[m_settingsKey] = value;
    }

    /// <summary>
    /// テンプレートにプレースホルダーを埋め込んで投稿文を生成する
    /// </summary>
    public static async Task<string> BuildAsync(NowPlayingViewModel song)
    {
        // {url} が含まれる場合のみ取得（Apple Music以外は空文字）
        var url = "";
        if (Template.Contains("{url}"))
        {
            if (song.Id?.StartsWith("AppleInc.AppleMusicWin") == true
                && song.Title != null && song.DisplayArtist != null && song.DisplayAlbum != null)
            {
                url = await urlExtract.getAPUrl(song.Title, song.DisplayArtist, song.DisplayAlbum);
            }
        }

        return Template
            .Replace("{title}", song.Title ?? "")
            .Replace("{subtitle}", song.Subtitle ?? "")
            .Replace("{artist}", song.DisplayArtist ?? "")
            .Replace("{album}", song.DisplayAlbum ?? "")
            .Replace("{albumartist}", song.AlbumArtist ?? "")
            .Replace("{tracknumber}", song.TrackNumber.ToString() ?? "")
            .Replace("{albumtrackcount}", song.AlbumTrackCount.ToString() ?? "")
            .Replace("{genres}", song.GenresText ?? "")
            .Replace("{url}", url)
            .Trim();  // {url}が空のとき末尾の改行を除去
    }
}
