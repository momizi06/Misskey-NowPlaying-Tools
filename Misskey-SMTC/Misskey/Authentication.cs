using Misskey_SMTC.Creditional;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Misskey_SMTC.Misskey;

internal class Authentication
{
    private readonly CreditionalManager m_credManager = new();
    private static readonly string[] m_permissions = ["write:notes"];

    // コールバックURIのスキーム（manifestと合わせる）
    private const string m_callbackScheme = "misskey-smtc";

    // セッション管理
    private string? m_pendingSessionId;
    private string? m_pendingHost;
    private TaskCompletionSource<MiAuth.Result?>? m_authTcs;

    /// <summary>
    /// トークン取得フローを開始する。
    /// App.xaml.cs から OnProtocolActivated を呼んでもらう必要がある。
    /// </summary>
    public async Task<MiAuth.Result?> GenerateTokenAsync(
        string serverUrl,
        CancellationToken cancellationToken = default)
    {
        using var authClient = new MiAuth.Client();

        m_authTcs = new TaskCompletionSource<MiAuth.Result?>();
        m_pendingHost = serverUrl;

        // キャンセル時に TCS を完了させる
        using var reg = cancellationToken.Register(
            () => m_authTcs.TrySetCanceled());

        var callbackUrl = $"{m_callbackScheme}://callback";

        m_pendingSessionId = await MiAuth.Client.StartAuthAsync(
            host: serverUrl,
            appName: "Misskey SMTC Tools",
            permissions: m_permissions,
            callbackUrl: callbackUrl
        );

        // コールバックが来るまで待機
        var result = await m_authTcs.Task;

        if (result != null)
        {
            CreditionalManager.SaveCredential(serverUrl, result.Token);
        }

        return result;
    }

    /// <summary>
    /// App.xaml.cs から呼び出す。
    /// URIの形式: misskey-smtc://callback?session={sessionId}
    /// </summary>
    public async Task OnProtocolActivatedAsync(Uri uri)
    {
        System.Diagnostics.Debug.WriteLine($"[MiAuth] Callback URI: {uri}");
        System.Diagnostics.Debug.WriteLine($"[MiAuth] _pendingSessionId: {m_pendingSessionId}");
        System.Diagnostics.Debug.WriteLine($"[MiAuth] _authTcs is null: {m_authTcs == null}");

        if (m_authTcs == null || m_pendingSessionId == null || m_pendingHost == null)
        {
            System.Diagnostics.Debug.WriteLine("[MiAuth] Early return: state is null");
            return;
        }


        if (m_authTcs == null || m_pendingSessionId == null || m_pendingHost == null)
            return;

        // クエリからセッションIDを取り出して一致確認
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        var sessionId = query["session"];
        System.Diagnostics.Debug.WriteLine($"[MiAuth] sessionId from callback: {sessionId}");
        System.Diagnostics.Debug.WriteLine($"[MiAuth] match: {sessionId == m_pendingSessionId}");

        if (sessionId != m_pendingSessionId)
            return;

        try
        {
            using var authClient = new MiAuth.Client();
            var result = await authClient.CheckTokenOnceAsync(m_pendingHost, sessionId);
            m_authTcs.TrySetResult(result);
        }
        catch (Exception ex)
        {
            m_authTcs.TrySetException(ex);
        }
        finally
        {
            m_pendingSessionId = null;
            m_pendingHost = null;
        }
    }
}
