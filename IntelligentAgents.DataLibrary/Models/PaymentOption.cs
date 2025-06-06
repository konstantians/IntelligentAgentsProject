namespace IntelligentAgents.DataLibrary.Models;
public class PaymentOption
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? NameAlias { get; set; }
    public string? Description { get; set; }
    public decimal? ExtraCost { get; set; }
    public DateTime? CreatedAt { get; set; }
}

