using IntelligentAgents.GatewayAPI.HelperMethods;
using IntelligentAgents.GatewayAPI.IntelligentAgentService.IntelligentAgentLogic;
using IntelligentAgents.GatewayAPI.IntelligentAgentService.Models;
using Microsoft.AspNetCore.Mvc;

namespace IntelligentAgents.GatewayAPI.IntelligentAgentService;

[ApiController]
[Route("api/[controller]")]
public class IntelligentAgentController : ControllerBase
{
    private readonly HttpClient authHttpClient;
    private readonly HttpClient dataHttpClient;
    private readonly IConfiguration _configuration;
    private readonly IUtilityMethods _utilityMethods;
    private readonly IAiAssistantService _aiAssistantService;

    public IntelligentAgentController(IConfiguration configuration, IHttpClientFactory httpClientFactory, IUtilityMethods utilityMethods, IAiAssistantService aiAssistantService)
    {
        _configuration = configuration;
        _utilityMethods = utilityMethods;
        _aiAssistantService = aiAssistantService;
        authHttpClient = httpClientFactory.CreateClient("AuthApiClient");
        dataHttpClient = httpClientFactory.CreateClient("DataApiClient");
    }

    [HttpPost("withTools")]
    public async Task<IActionResult> AnswerUserQueryWithPlugin(AnswerUserQueryModel answerUserQueryModel)
    {
        try
        {
            string chosenTool = await _aiAssistantService.AskToChooseToolAsync(answerUserQueryModel.UserQuery!);
            string cleanedChosenTool = chosenTool.Replace("```", "").Replace("\n", " ").Trim();
            int firstSpaceIndex = cleanedChosenTool.IndexOf(' ');
            if (firstSpaceIndex != -1)
                cleanedChosenTool = cleanedChosenTool.Substring(0, firstSpaceIndex);

            if (cleanedChosenTool != "Category" && cleanedChosenTool != "Product" && cleanedChosenTool != "Variant" && cleanedChosenTool != "Discount" &&
                cleanedChosenTool != "PaymentOption" && cleanedChosenTool != "ShippingOption" && cleanedChosenTool != "Coupon")
            {
                return BadRequest(new { error = "Something went wrong with the system. Please try again in a bit with a different input if possible.", response = "" });
                /*
                retries--;
                continue;*/
            }

            int retries = 5;
            string returnedJson = "";
            do
            {
                string chosenEndpoint = await _aiAssistantService.AskToChooseEndpointAsync(answerUserQueryModel.UserQuery!, cleanedChosenTool);

                //remove backticks and \n and remove pointless white space
                string cleanedChosenEndpoint = chosenEndpoint.Replace("```", "");
                cleanedChosenEndpoint = cleanedChosenEndpoint.Replace("\n", " ");
                int commaIndex = cleanedChosenEndpoint.IndexOf(',');
                //remove blabla that sometimes the AI even after validation adds
                if (commaIndex != -1)
                    cleanedChosenEndpoint = cleanedChosenEndpoint.Substring(0, commaIndex);

                cleanedChosenEndpoint = cleanedChosenEndpoint.Trim();

                _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient);
                var response = await dataHttpClient.GetAsync(cleanedChosenEndpoint);

                if (!response.IsSuccessStatusCode)
                    retries--;
                else
                    returnedJson = await response.Content.ReadAsStringAsync();
            } while (retries > 0 && string.IsNullOrWhiteSpace(returnedJson));

            if (string.IsNullOrEmpty(returnedJson))
                return BadRequest(new { error = "Something went wrong with the system. Please try again in a bit with a different input if possible.", response = "" });

            string finalResult = await _aiAssistantService.AskForJsonInterpretation(answerUserQueryModel.UserQuery!, returnedJson);
            if (finalResult == "Something went wrong with the system. Please try again in a bit with a different input if possible.")
                return BadRequest(new { error = "Something went wrong with the system. Please try again in a bit with a different input if possible.", response = "" });

            return Ok(new { error = "", response = finalResult });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Internal server error", response = "" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AnswerUserQuery(AnswerUserQueryModel answerUserQueryModel)
    {
        try
        {
            HttpResponseMessage? response = null;
            int retries = 5;
            string validatedGenaratedSqlQuery = "";
            List<string> selectedJson = new List<string>();
            do
            {
                validatedGenaratedSqlQuery = await CommonAswerUserQueryPart(answerUserQueryModel.UserQuery!);
                if (validatedGenaratedSqlQuery == "Something went wrong with the system. Please try again in a bit with a different input if possible.")
                    return BadRequest(new { error = "Something went wrong with the system. Please try again in a bit with a different input if possible.", response = "" });

                _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient);
                response = await dataHttpClient.PostAsJsonAsync("Query", new { SqlQuery = validatedGenaratedSqlQuery });

                if (response.IsSuccessStatusCode)
                    selectedJson.Add(await response.Content.ReadAsStringAsync());

                retries--;
            } while (retries > 0 && selectedJson.Count < 2);

            //if it has failed after 4 retries then just returned that the AI failed
            if (!selectedJson.Any())
                return BadRequest(new { error = $"Data API returned status {response.StatusCode}", response = "" });

            string finalJson = selectedJson[0]!;
            /*if (selectedJson.Count > 1)
            {
                finalJson = await _aiAssistantService.AskToCompareJsonAsync(answerUserQueryModel.UserQuery!, selectedJson);
                if (validatedGenaratedSqlQuery == "Something went wrong with the system. Please try again in a bit with a different input if possible.")
                    finalJson = selectedJson[0]!;
            }*/

            if (string.IsNullOrWhiteSpace(finalJson) || finalJson.Trim() == "{}")
                finalJson = "Nothing returned";

            string finalResult = await _aiAssistantService.AskForInterpretationOfGeneratedQueryAsync(answerUserQueryModel.UserQuery!, validatedGenaratedSqlQuery, finalJson);
            if (finalResult == "Something went wrong with the system. Please try again in a bit with a different input if possible.")
                return BadRequest(new { error = finalResult, response = "" });

            return Ok(new { error = "", response = finalResult });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Internal server error", response = "" });
        }
    }

    private async Task<string> CommonAswerUserQueryPart(string userQuery)
    {
        string generatedSqlQuery = await _aiAssistantService.AskToGenerateQueryAsync(userQuery);
        if (generatedSqlQuery == "Something went wrong with the system. Please try again in a bit with a different input if possible.")
            return generatedSqlQuery;

        string validatedGenaratedSqlQuery = await _aiAssistantService.AskToValidateGeneratedQueryAsync(generatedSqlQuery);
        if (validatedGenaratedSqlQuery == "Something went wrong with the system. Please try again in a bit with a different input if possible.")
            return validatedGenaratedSqlQuery;

        //remove backticks and \n and remove pointless white space
        validatedGenaratedSqlQuery = validatedGenaratedSqlQuery.Replace("```", "");
        validatedGenaratedSqlQuery = validatedGenaratedSqlQuery.Replace("\n", " ");
        int semicolonIndex = validatedGenaratedSqlQuery.IndexOf(';');
        //remove blabla that sometimes the AI even after validation adds
        if (semicolonIndex != -1)
            validatedGenaratedSqlQuery = validatedGenaratedSqlQuery.Substring(0, semicolonIndex);

        validatedGenaratedSqlQuery = validatedGenaratedSqlQuery.Trim();

        return validatedGenaratedSqlQuery;
    }
}
