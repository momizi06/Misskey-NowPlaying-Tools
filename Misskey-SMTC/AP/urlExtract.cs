using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Misskey_SMTC.AP;

internal class urlExtract
{
#pragma warning disable IDE1006 // 命名スタイル
    private const string COUNTRY_CODE = "jp";
#pragma warning restore IDE1006 // 命名スタイル

    public static async Task<string> getAPUrl(string title, string artist, string album)
    {
        string text = Uri.EscapeDataString($"{title} {artist} {album}");
        string url = $"https://music.apple.com/{COUNTRY_CODE}/search?term={text}";
        try
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync(url);
            string pattern = $@"https://music\.apple\.com/{COUNTRY_CODE}/album/.*/.*\?i=[0-9]+";
            var match = System.Text.RegularExpressions.Regex.Match(response, pattern);
            return match.Success ? match.Value : "";
        }
        catch (Exception ex)
        {
            // ログ出力などのエラーハンドリング
            Console.WriteLine($"Error fetching Apple Music URL: {ex.Message}");
            return "";
        }
    }
}
