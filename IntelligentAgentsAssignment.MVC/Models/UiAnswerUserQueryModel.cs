using System.ComponentModel.DataAnnotations;

namespace IntelligentAgents.MVC.Models;

public class UiAnswerUserQueryModel
{
    [Required]
    public string? UserQuery { get; set; }

    [Required]
    public string? AiAssistantMode { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "The number of tables rows must be greater than 0.")]
    public int TablesRowsThatShouldBeReturned { get; set; }
    [Required]
    public string? ModelId { get; set; }
}
