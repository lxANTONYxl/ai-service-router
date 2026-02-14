using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AIServiceRouter.Types;

namespace AIServiceRouter.Services;

public class CerebrasService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public string Name => "cerebras";

    public CerebrasService(IConfiguration configuration)
    {
        _apiKey = configuration["ApiKeys:Cerebras"] 
                  ?? throw new InvalidOperationException("Cerebras API key not configured");
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.cerebras.ai/v1/")
        };
    }

    public async Task<IAsyncEnumerable<string>> ChatAsync(List<ChatMessage> messages)
    {
        var request = new
        {
            model = "llama3.1-8b",
            messages = messages.Select(m => new { role = m.Role, content = m.Content }),
            stream = true,
            max_completion_tokens = 8192,
            temperature = 1,
            top_p = 1
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json"
            )
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        return StreamResponse(response);
    }
    
    private async IAsyncEnumerable<string> StreamResponse(HttpResponseMessage response)
    {
        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: ")) continue;
            if (line.Contains("[DONE]")) break;

            var json = line.Substring(6);
            var content = ParseContent(json);

            if (!string.IsNullOrEmpty(content))
                yield return content;
        }
    }

    private string? ParseContent(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("delta")
                .GetProperty("content")
                .GetString();
        }
        catch
        {
            return null;
        }
    }
}