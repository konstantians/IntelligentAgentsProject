namespace IntelligentAgents.DataLibrary.Models.RequestModels;
public class SetEmbeddingsRequestModel
{
    public Dictionary<string, float[]> IdEmbeddingPairs { get; set; } = new Dictionary<string, float[]>();
}
