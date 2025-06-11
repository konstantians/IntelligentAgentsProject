
namespace IntelligentAgents.GatewayAPI.IntelligentAgentService.IntelligentAgentLogic;

public interface IAiAssistantService
{
    Task<string> AskToGenerateQueryAsync(string userRequest, int tableRows = 0, string modelId = "phi4-mini");
    Task<string> AskToValidateGeneratedQueryAsync(string generatedSqlQuery, string modelId = "phi4-mini");
    Task<string> AskForInterpretationOfReturnedJsonBasedOnSqlQueryAsync(string userRequest, string generatedSqlQuery, string jsonResultFromDataMicroservice, int tableRows = 0, string modelId = "phi4-mini");
    Task<string> AskToChooseEndpointAsync(string userRequest, string toolCategory, int tableRows = 0, string modelId = "phi4-mini");
    Task<string> AskForJsonInterpretation(string userRequest, string jsonResultFromDataMicroservice, string modelId = "phi4-mini");
    Task<string> AskToChooseToolAsync(string userRequest, string modelId = "phi4-mini");
}