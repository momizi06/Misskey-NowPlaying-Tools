using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Misskey_SMTC.Creditional;

public class ServerEntry : INotifyPropertyChanged
{
    private string m_serverUrl;
    private string m_displayName;
    private bool m_isDefault;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string ServerUrl
    {
        get => m_serverUrl;
        set => SetProperty(ref m_serverUrl, value);
    }

    public string DisplayName
    {
        get => string.IsNullOrEmpty(m_displayName) ? m_serverUrl : m_displayName;
        set => SetProperty(ref m_displayName, value);
    }

    /// <summary>表示名が別途設定されているか（TextBoxの初期値用）</summary>
    public string RawDisplayName
    {
        get => m_displayName;
        set => SetProperty(ref m_displayName, value);
    }

    public bool IsDefault
    {
        get => m_isDefault;
        set => SetProperty(ref m_isDefault, value);
    }

    public ServerEntry(string serverUrl, string displayName = "", bool isDefault = false)
    {
        m_serverUrl = serverUrl;
        m_displayName = displayName;
        m_isDefault = isDefault;
    }

    private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? name = null)
    {
        if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(storage, value))
            return false;
        storage = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        return true;
    }
}
