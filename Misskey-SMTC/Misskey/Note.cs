using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;

namespace Misskey_SMTC.Misskey;

internal class Note(string host, string token, string content)
{
    private readonly string m_host = host;
    private readonly string m_token = token;
    private readonly string m_content = content;

    internal async Task PostAsync()
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {m_token}");
        var payload = new
        {
            text = m_content
        };
        var json = JsonSerializer.Serialize(payload);
        try
        {
            var response = await client.PostAsync(
                $"https://{m_host}/api/notes/create",
                new StringContent(json, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to post note: {ex.Message}");
        }
    }
}

