using System.ComponentModel.DataAnnotations;

namespace IntelligentAgents.DataLibraryAPI.Models;

public class ExecuteSelectQueryModel
{
    [Required]
    public string? SqlQuery { get; set; }
}
