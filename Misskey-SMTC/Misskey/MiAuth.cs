using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;

namespace Misskey_SMTC.Misskey;

internal partial class MiAuth
{
    internal partial class Client : IDisposable
    {
        private readonly HttpClient m_httpClient;

        public Client()
        {
            m_httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        // -------------------------------------------------------
        // Step 1 & 2: ブラウザを開いて認証ページに誘導
        // -------------------------------------------------------

        /// <summary>
        /// 認証URLを生成してブラウザで開く。セッションIDを返す。
        /// </summary>
        public static async Task<string> StartAuthAsync(
            string host,
            string appName,
            string[] permissions,
            string? iconUrl = null,
            string? callbackUrl = null)
        {
            var sessionId = Guid.NewGuid().ToString();

            var permissionParam = Uri.EscapeDataString(string.Join(",", permissions));
            var url = $"https://{host}/miauth/{sessionId}?name={Uri.EscapeDataString(appName)}&permission={permissionParam}";

            if (iconUrl != null)
                url += $"&icon={Uri.EscapeDataString(iconUrl)}";

            if (callbackUrl != null)
                url += $"&callback={Uri.EscapeDataString(callbackUrl)}";

            // WinUI3でデフォルトブラウザを開く
            await Launcher.LaunchUriAsync(new Uri(url));

            return sessionId;
        }

        /// <summary>
        /// コールバックURL経由で受け取ったセッションIDでトークンをチェック（1回だけ）
        /// </summary>
        public async Task<Result?> CheckTokenOnceAsync(
            string host,
            string sessionId,
            CancellationToken cancellationToken = default)
        {
            var checkUrl = $"https://{host}/api/miauth/{sessionId}/check";

            var response = await m_httpClient.PostAsync(checkUrl, content: null, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<CheckResponse>(
                cancellationToken: cancellationToken);

            if (string.IsNullOrEmpty(result?.Token))
                return null;

            return new Result
            {
                Token = result.Token,
                User = result.User
            };
        }

        public void Dispose() => m_httpClient.Dispose();
    }

    // -------------------------------------------------------
    // レスポンス / 結果モデル
    // -------------------------------------------------------

    internal class CheckResponse
    {
        [JsonPropertyName("ok")]
        public bool Ok
        {
            get; set;
        }

        [JsonPropertyName("token")]
        public string? Token
        {
            get; set;
        }

        [JsonPropertyName("user")]
        public MisskeyUser? User
        {
            get; set;
        }
    }

    public class Result
    {
        public required string Token
        {
            get; init;
        }
        public MisskeyUser? User
        {
            get; init;
        }
    }

    public class MisskeyUser
    {
        [JsonPropertyName("id")]
        public string? Id
        {
            get; set;
        }

        [JsonPropertyName("name")]
        public string? Name
        {
            get; set;
        }

        [JsonPropertyName("username")]
        public string? Username
        {
            get; set;
        }
    }


}
