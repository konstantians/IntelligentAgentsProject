using System.ComponentModel.DataAnnotations;

namespace IntelligentAgents.GatewayAPI.IntelligentAgentService.Models;

public class AnswerUserQueryModel
{
    [Required]
    public string? UserQuery { get; set; }
}
