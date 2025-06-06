
namespace IntelligentAgents.GatewayAPI.IntelligentAgentService.IntelligentAgentLogic.Connectors;

public interface IOllamaConnectorService
{
    Task<string> AskOllamaAsync(string userPrompt, string systemPrompt = "", string modelId = "phi4-mini");
}