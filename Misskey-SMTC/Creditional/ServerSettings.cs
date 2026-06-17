using System.Collections.Generic;
using System.Text.Json;
using Windows.Storage;

namespace Misskey_SMTC.Creditional;

public class ServerConfig
{
    public string DisplayName { get; set; } = "";
    public bool IsDefault { get; set; } = false;
}

public static class ServerSettings
{
    private const string m_settingsKey = "ServerConfigs";

    private static Dictionary<string, ServerConfig> Load()
    {
        try
        {
            var json = ApplicationData.Current.LocalSettings.Values[m_settingsKey] as string;
            if (!string.IsNullOrEmpty(json))
                return JsonSerializer.Deserialize<Dictionary<string, ServerConfig>>(json)
                       ?? [];
        }
        catch { }
        return [];
    }

    private static void Save(Dictionary<string, ServerConfig> configs)
    {
        ApplicationData.Current.LocalSettings.Values[m_settingsKey] =
            JsonSerializer.Serialize(configs);
    }

    public static ServerConfig Get(string serverUrl)
    {
        var configs = Load();
        return configs.TryGetValue(serverUrl, out var config) ? config : new();
    }

    public static void Set(string serverUrl, ServerConfig config)
    {
        var configs = Load();

        // IsDefault をオンにした場合、他のサーバーのデフォルトを解除
        if (config.IsDefault)
        {
            foreach (var key in configs.Keys)
                configs[key].IsDefault = false;
        }

        configs[serverUrl] = config;
        Save(configs);
    }

    public static void Remove(string serverUrl)
    {
        var configs = Load();
        configs.Remove(serverUrl);
        Save(configs);
    }

    /// <summary>デフォルトサーバーのURLを返す。未設定なら null</summary>
    public static string? GetDefaultServer()
    {
        var configs = Load();
        foreach (var (url, config) in configs)
            if (config.IsDefault)
                return url;
        return null;
    }
}
