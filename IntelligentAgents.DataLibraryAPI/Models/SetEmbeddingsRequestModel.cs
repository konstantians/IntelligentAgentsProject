using System.ComponentModel.DataAnnotations;

namespace IntelligentAgents.DataLibraryAPI.Models;

public class SetEmbeddingsRequestModel
{
    [Required]
    public Dictionary<string, string> IdDescriptionPairs { get; set; } = new Dictionary<string, string>();
}
