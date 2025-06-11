using System.ComponentModel.DataAnnotations;

namespace IntelligentAgents.GatewayAPI.IntelligentAgentService.Models;

public class AnswerUserQueryModel
{
    [Required]
    public string? UserQuery { get; set; }
    public int TablesRowsThatShouldBeReturned { get; set; }
    public string? ModelId { get; set; }
}
