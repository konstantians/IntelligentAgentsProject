using System.Text.Json;

namespace IntelligentAgents.GatewayAPI.IntelligentAgentService.IntelligentAgentLogic.Connectors;

public class OllamaConnectorService : IOllamaConnectorService
{
    private readonly HttpClient OllamaHttpClient;

    public OllamaConnectorService(IHttpClientFactory httpClientFactory)
    {
        OllamaHttpClient = httpClientFactory.CreateClient("OllamaApiClient");
    }

    public async Task<string> AskOllamaAsync(string userPrompt, string systemPrompt = "", string modelId = "phi4-mini")
    {
        var payload = new
        {
            model = modelId,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            stream = false
        };

        var response = await OllamaHttpClient.PostAsJsonAsync("chat", payload);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Ollama error: {response.StatusCode}");

        var json = await response.Content.ReadAsStringAsync();

        //the content we want is inside { "message": { "content": "...."}}. We can get it with the following method
        using JsonDocument jsonDocument = JsonDocument.Parse(json);
        var content = jsonDocument.RootElement.GetProperty("message").GetProperty("content").GetString();

        return content?.Trim() ?? "";
    }
}
