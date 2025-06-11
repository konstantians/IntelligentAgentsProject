namespace IntelligentAgents.DataLibrary.Models.ResponseModels.ShippingOptionModels;
public class ShippingOptionEmbeddingDTO
{
    public string? Id { get; set; }
    public string? Description { get; set; }
    public float[]? Embedding { get; set; }
}
