using Misskey_SMTC.Creditional;
using System.Collections.ObjectModel;
using System.Linq;

namespace Misskey_SMTC.Creditional;

public static class ServerStore
{
    private static ObservableCollection<ServerEntry>? _servers;

    public static ObservableCollection<ServerEntry> Servers
        => _servers ??= LoadFromCredential();

    private static ObservableCollection<ServerEntry> LoadFromCredential()
    {
        var list = new ObservableCollection<ServerEntry>();
        try
        {
            foreach (var url in Creditional.CreditionalManager.LoadAllServerUrls())
            {
                var config = ServerSettings.Get(url);
                list.Add(new ServerEntry(url, config.DisplayName, config.IsDefault));
            }
        }
        catch { }
        return list;
    }

    public static void Add(string serverUrl)
    {
        if (Servers.Any(s => s.ServerUrl == serverUrl))
            return;
        var config = ServerSettings.Get(serverUrl);
        Servers.Add(new ServerEntry(serverUrl, config.DisplayName, config.IsDefault));
    }

    public static void Remove(string serverUrl)
    {
        Creditional.CreditionalManager.RemoveCredential(serverUrl);
        ServerSettings.Remove(serverUrl);
        var target = Servers.FirstOrDefault(s => s.ServerUrl == serverUrl);
        if (target != null)
            Servers.Remove(target);
    }

    public static void SetDefault(string serverUrl)
    {
        foreach (var entry in Servers)
            entry.IsDefault = entry.ServerUrl == serverUrl;
    }
}
