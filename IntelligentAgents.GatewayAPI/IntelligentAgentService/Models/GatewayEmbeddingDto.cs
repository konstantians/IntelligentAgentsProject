namespace IntelligentAgents.GatewayAPI.IntelligentAgentService.Models;

public class GatewayEmbeddingDto
{
    public string? Id { get; set; }
    public string? Description { get; set; }
    public float[]? Embedding { get; set; }
    public string? TableItsIn { get; set; }
}
