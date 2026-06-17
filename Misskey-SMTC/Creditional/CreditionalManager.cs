using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Security.Credentials;

namespace Misskey_SMTC.Creditional;

internal class CreditionalManager
{
    private const string m_resourcePrefix = "Misskey_SMTP.Creditional";

    /// <summary>
    /// サーバーとトークンの組を保存（上書き対応）
    /// </summary>
    public static void SaveCredential(string serverUrl, string token)
    {
        var vault = new PasswordVault();
        var resourceName = $"{m_resourcePrefix}.{serverUrl}";

        // 既存エントリを削除してから追加（上書き）
        try
        {
            var existing = vault.Retrieve(resourceName, serverUrl);
            vault.Remove(existing);
        }
        catch (Exception) { /* 存在しなければ無視 */ }

        vault.Add(new PasswordCredential(
            resource: resourceName,
            userName: serverUrl,
            password: token
        ));
    }

    /// <summary>
    /// 保存済みのすべてのサーバー＋トークンを取得
    /// </summary>
    public static IReadOnlyList<(string Server, string Token)> LoadAllCredentials()
    {
        var vault = new PasswordVault();
        var results = new List<(string, string)>();

        try
        {
            // プレフィックスに一致するものをすべて取得
            var credentials = vault.FindAllByResource(m_resourcePrefix);
            // ↑ただしFindAllByResourceは部分一致不可なので、
            //   全件取得してフィルタする方が確実
        }
        catch (Exception) { }

        // 全件取得してフィルタ
        try
        {
            var all = vault.RetrieveAll();
            foreach (var cred in all.Where(c => c.Resource.StartsWith(m_resourcePrefix)))
            {
                cred.RetrievePassword(); // パスワードは明示的に取得が必要
                results.Add((cred.UserName, cred.Password));
            }
        }
        catch (Exception) { }

        return results;
    }

    public static IReadOnlyList<string> LoadAllServerUrls()
    {
        var vault = new PasswordVault();
        var results = new List<string>();
        try
        {
            var all = vault.RetrieveAll();
            foreach (var cred in all.Where(c => c.Resource.StartsWith(m_resourcePrefix)))
            {
                results.Add(cred.UserName);
            }
        }
        catch (Exception) { }
        return results;
    }

    /// <summary>
    /// 特定サーバーのトークンを取得
    /// </summary>
    public static string? LoadToken(string serverUrl)
    {
        var vault = new PasswordVault();
        var resourceName = $"{m_resourcePrefix}.{serverUrl}";

        try
        {
            var cred = vault.Retrieve(resourceName, serverUrl);
            cred.RetrievePassword();
            return cred.Password;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// 特定サーバーの認証情報を削除
    /// </summary>
    public static void RemoveCredential(string serverUrl)
    {
        var vault = new PasswordVault();
        var resourceName = $"{m_resourcePrefix}.{serverUrl}";

        try
        {
            var cred = vault.Retrieve(resourceName, serverUrl);
            vault.Remove(cred);
        }
        catch (Exception) { }
    }
}

