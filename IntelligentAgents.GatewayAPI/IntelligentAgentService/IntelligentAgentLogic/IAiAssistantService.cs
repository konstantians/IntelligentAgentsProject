
namespace IntelligentAgents.GatewayAPI.IntelligentAgentService.IntelligentAgentLogic;

public interface IAiAssistantService
{
    Task<string> AskToGenerateQueryAsync(string userRequest);
    Task<string> AskToValidateGeneratedQueryAsync(string generatedSqlQuery);
    Task<string> AskForInterpretationOfGeneratedQueryAsync(string userRequest, string generatedSqlQuery, string jsonResultFromDataMicroservice);
    Task<string> AskToCompareJsonAsync(string userRequest, List<string> generatedJsonSequences);
    Task<string> AskToChooseEndpointAsync(string userRequest, string toolCategory);
    Task<string> AskForJsonInterpretation(string userRequest, string jsonResultFromDataMicroservice);
    Task<string> AskToChooseToolAsync(string userRequest);
}