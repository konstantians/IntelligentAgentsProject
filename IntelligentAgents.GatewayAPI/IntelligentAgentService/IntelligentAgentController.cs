using IntelligentAgents.GatewayAPI.HelperMethods;
using IntelligentAgents.GatewayAPI.IntelligentAgentService.IntelligentAgentLogic;
using IntelligentAgents.GatewayAPI.IntelligentAgentService.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace IntelligentAgents.GatewayAPI.IntelligentAgentService;

[ApiController]
[Route("api/[controller]")]
public class IntelligentAgentController : ControllerBase
{
    private readonly HttpClient dataHttpClient;
    private readonly HttpClient embeddingMicroserviceHttpClient;
    private readonly IUtilityMethods _utilityMethods;
    private readonly IAiAssistantService _aiAssistantService;
    private readonly IConfiguration _configuration;

    public IntelligentAgentController(IHttpClientFactory httpClientFactory, IUtilityMethods utilityMethods, IAiAssistantService aiAssistantService, IConfiguration configuration)
    {
        _utilityMethods = utilityMethods;
        _aiAssistantService = aiAssistantService;
        _configuration = configuration;
        dataHttpClient = httpClientFactory.CreateClient("DataApiClient");
        embeddingMicroserviceHttpClient = httpClientFactory.CreateClient("EmbeddingMicroserviceApiClient");
    }


    [HttpPost("withEmbeddings")]
    public async Task<IActionResult> AnswerUserQueryWithEmbeddings(AnswerUserQueryModel answerUserQueryModel)
    {
        string currentCallError = "EmbeddingMicroserviceError"; //used to discern what is throwing the connection error exception
        try
        {
            int returnedClosestDistancesCount;
            if (answerUserQueryModel.TablesRowsThatShouldBeReturned != 0)
                returnedClosestDistancesCount = answerUserQueryModel.TablesRowsThatShouldBeReturned;
            else if (!Int32.TryParse(_configuration["tableRows"], out returnedClosestDistancesCount))
                returnedClosestDistancesCount = 3;

            var response = await embeddingMicroserviceHttpClient.PostAsJsonAsync("createEmbeddings", new { idDescriptionPairs = new Dictionary<string, string> { ["userRequest"] = answerUserQueryModel.UserQuery! } });
            if (!response.IsSuccessStatusCode)
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "EmbeddingMicroserviceError", response = "" });

            string responseBody = await response.Content.ReadAsStringAsync();
            Dictionary<string, float[]> returnedIdEmbeddingPair = JsonSerializer.Deserialize<Dictionary<string, float[]>>(responseBody)!;
            float[] userRequestEmbedding = returnedIdEmbeddingPair.FirstOrDefault()!.Value;

            response = await dataHttpClient.GetAsync("Embedding");
            if (!response.IsSuccessStatusCode)
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "EmbeddingMicroserviceError", response = "" });

            responseBody = await response.Content.ReadAsStringAsync();
            var embeddingDtos = JsonSerializer.Deserialize<List<GatewayEmbeddingDto>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            Dictionary<string, float[]> idEmbeddingPairs = embeddingDtos
                .ToDictionary(dto => dto.Id!, dto => dto.Embedding!);

            Dictionary<string, float> chosenIdDistancePairs = idEmbeddingPairs
            .Select(pair => new
            {
                Id = pair.Key,
                Distance = _utilityMethods.CalculateCosineDistance(pair.Value, userRequestEmbedding)
            })
            .OrderBy(pair => pair.Distance)
            .Take(returnedClosestDistancesCount)
            .ToDictionary(pair => pair.Id, pair => pair.Distance);

            List<string> chosenIds = chosenIdDistancePairs.Select(pair => pair.Key).ToList();

            currentCallError = "DataMicroserviceError";
            StringBuilder returnedJsonStringBuilder = new StringBuilder();
            foreach (string chosenId in chosenIds)
            {
                string tableItIsIn = embeddingDtos.FirstOrDefault(embeddingDto => chosenId == embeddingDto.Id)!.TableItsIn!;
                response = null;
                if (tableItIsIn == "Products")
                    response = await dataHttpClient.GetAsync($"Product/{chosenId}");
                else if (tableItIsIn == "PaymentOptions")
                    response = await dataHttpClient.GetAsync($"PaymentOption/{chosenId}");
                else if (tableItIsIn == "ShippingOptions")
                    response = await dataHttpClient.GetAsync($"ShippingOption/{chosenId}");

                if (response is null || !response.IsSuccessStatusCode)
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "DataMicroserviceError", response = "" });

                returnedJsonStringBuilder.Append(await response.Content.ReadAsStringAsync());
            }

            //prepare response
            string finalResult = await _aiAssistantService.AskForJsonInterpretation(answerUserQueryModel.UserQuery!, returnedJsonStringBuilder.ToString(), modelId: answerUserQueryModel.ModelId ?? "phi4-mini");
            List<float[]> databaseEmbeddings = idEmbeddingPairs.Select(embedding => embedding.Value).ToList();
            if (finalResult == "Something went wrong with the system. Please try again in a bit with a different input if possible.")
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "LLMError", response = "" });

            string userRequestEmbeddingString = $"[{string.Join(", ", userRequestEmbedding.Select(f => f.ToString("F2")))}]"; //magic that makes float[] to string
            string databaseEmbeddingsString = string.Join(Environment.NewLine, databaseEmbeddings.Select(arrayOfFloats => $"[{string.Join(", ", arrayOfFloats.Select(f => f.ToString("F2")))}]")); //magic that makes List<float[]> into a string
            string chosenIdDistanceString = string.Join(Environment.NewLine, chosenIdDistancePairs.Select(pair => $"{pair.Key}: {pair.Value:F4}")); //actually no magic, this is pretty readable
            return Ok(new { error = "", stepTwo = userRequestEmbeddingString, stepThree = databaseEmbeddingsString, stepFour = chosenIdDistanceString, stepFive = returnedJsonStringBuilder.ToString(), response = finalResult });
        }
        catch (HttpRequestException ex)
        {
            // Connection error, DNS fail, refused connection, etc.
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                error = currentCallError,
                response = ex.Message
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Internal server error", response = "" });
        }
    }

    [HttpPost("withTools")]
    public async Task<IActionResult> AnswerUserQueryWithTools(AnswerUserQueryModel answerUserQueryModel)
    {
        try
        {
            int toolRetries = 5;
            string cleanedChosenTool = "";
            do
            {
                string chosenTool = await _aiAssistantService.AskToChooseToolAsync(answerUserQueryModel.UserQuery!, modelId: answerUserQueryModel.ModelId ?? "phi4-mini");
                if (chosenTool == "Something went wrong with the system. Please try again in a bit with a different input if possible.")
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "LLMError", response = "" });

                cleanedChosenTool = chosenTool.Replace("```", "").Replace("\n", " ").Trim();
                int firstSpaceIndex = cleanedChosenTool.IndexOf(' ');

                if (firstSpaceIndex != -1)
                    cleanedChosenTool = cleanedChosenTool.Substring(0, firstSpaceIndex);

                if (cleanedChosenTool != "Category" && cleanedChosenTool != "Product" && cleanedChosenTool != "Variant" && cleanedChosenTool != "Discount" &&
                    cleanedChosenTool != "PaymentOption" && cleanedChosenTool != "ShippingOption" && cleanedChosenTool != "Coupon")
                {
                    toolRetries--;
                    cleanedChosenTool = "";
                }
            } while (toolRetries > 0 && string.IsNullOrWhiteSpace(cleanedChosenTool));

            if (string.IsNullOrEmpty(cleanedChosenTool))
                return BadRequest(new { error = "ToolSelectionError", response = "" });

            int endpointRetries = 5;
            string returnedJson = "";
            string cleanedChosenEndpoint = "";
            do
            {
                string chosenEndpoint = await _aiAssistantService.AskToChooseEndpointAsync(answerUserQueryModel.UserQuery!, cleanedChosenTool,
                    tableRows: answerUserQueryModel.TablesRowsThatShouldBeReturned, modelId: answerUserQueryModel.ModelId ?? "phi4-mini");
                if (chosenEndpoint == "Something went wrong with the system. Please try again in a bit with a different input if possible.")
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "LLMError", response = "" });

                //remove backticks and \n and remove pointless white space
                cleanedChosenEndpoint = chosenEndpoint.Replace("```", "");
                cleanedChosenEndpoint = cleanedChosenEndpoint.Replace("\n", " ");
                int commaIndex = cleanedChosenEndpoint.IndexOf(',');
                //remove blabla that sometimes the AI even after validation adds
                if (commaIndex != -1)
                    cleanedChosenEndpoint = cleanedChosenEndpoint.Substring(0, commaIndex);

                cleanedChosenEndpoint = cleanedChosenEndpoint.Trim();

                var response = await dataHttpClient.GetAsync(cleanedChosenEndpoint);

                if (!response.IsSuccessStatusCode)
                    endpointRetries--;
                else
                    returnedJson = await response.Content.ReadAsStringAsync();
            } while (endpointRetries > 0 && string.IsNullOrWhiteSpace(returnedJson));

            if (string.IsNullOrEmpty(returnedJson))
                return BadRequest(new { error = "EndpointGenerationError", stepTwo = cleanedChosenEndpoint, toolSelectionRetries = $"{5 - toolRetries}", response = "" });

            string finalResult = await _aiAssistantService.AskForJsonInterpretation(answerUserQueryModel.UserQuery!, returnedJson, modelId: answerUserQueryModel.ModelId ?? "phi4-mini");
            if (finalResult == "Something went wrong with the system. Please try again in a bit with a different input if possible.")
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "LLMError", response = "" });

            return Ok(new
            {
                error = "",
                stepTwo = cleanedChosenTool,
                stepThree = cleanedChosenEndpoint,
                stepFour = returnedJson,
                toolSelectionRetries = $"{5 - toolRetries}",
                endpointGenerationRetries = $"{5 - endpointRetries}",
                response = finalResult
            });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                error = "DataMicroserviceError",
                response = ex.Message
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Internal server error", response = "" });
        }
    }

    [HttpPost("withSqlGeneration")]
    public async Task<IActionResult> AnswerUserQueryWithSqlGeneration(AnswerUserQueryModel answerUserQueryModel)
    {
        try
        {
            HttpResponseMessage? response = null;
            int retries = 5;
            string generatedSqlQuery = "";
            string validatedGenaratedSqlQuery = "";
            string validatedAndCleanedSqlQuery = "";
            string returnedJson = "";
            do
            {
                generatedSqlQuery = await _aiAssistantService.AskToGenerateQueryAsync(answerUserQueryModel.UserQuery!,
                    tableRows: answerUserQueryModel.TablesRowsThatShouldBeReturned, modelId: answerUserQueryModel.ModelId ?? "phi4-mini");
                if (generatedSqlQuery == "Something went wrong with the system. Please try again in a bit with a different input if possible.")
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "LLMError", response = "" });

                validatedGenaratedSqlQuery = await _aiAssistantService.AskToValidateGeneratedQueryAsync(generatedSqlQuery, modelId: answerUserQueryModel.ModelId ?? "phi4-mini");
                if (validatedGenaratedSqlQuery == "Something went wrong with the system. Please try again in a bit with a different input if possible.")
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "LLMError", response = "" });

                //remove backticks and \n and remove pointless white space
                validatedAndCleanedSqlQuery = validatedGenaratedSqlQuery.Replace("sql```", "");
                validatedAndCleanedSqlQuery = validatedAndCleanedSqlQuery.Replace("```", "");
                validatedAndCleanedSqlQuery = validatedAndCleanedSqlQuery.Replace("\n", " ");
                int semicolonIndex = validatedAndCleanedSqlQuery.IndexOf(';');
                //remove extra stuff that sometimes the AI even after validation adds
                if (semicolonIndex != -1)
                    validatedAndCleanedSqlQuery = validatedAndCleanedSqlQuery.Substring(0, semicolonIndex);
                validatedAndCleanedSqlQuery = validatedAndCleanedSqlQuery.Trim();

                response = await dataHttpClient.PostAsJsonAsync("Query", new { SqlQuery = validatedAndCleanedSqlQuery });
                if (response.IsSuccessStatusCode)
                    returnedJson = await response.Content.ReadAsStringAsync();
                else
                    retries--;
            } while (retries > 0 && string.IsNullOrWhiteSpace(returnedJson));

            if (string.IsNullOrWhiteSpace(returnedJson))
                return BadRequest(new { error = "SQLQueryGenerationError", stepTwo = generatedSqlQuery, stepThree = validatedGenaratedSqlQuery, stepFour = validatedAndCleanedSqlQuery });

            string cleanedJson = Regex.Replace(returnedJson, ",\"Embedding\"\\s*:\\s*\".*?\"\\s*,?", ""); //removes the embedding, because it throws of the LLM. Yes the Regex is magic
            string finalResult = await _aiAssistantService.AskForInterpretationOfReturnedJsonBasedOnSqlQueryAsync(answerUserQueryModel.UserQuery!, validatedGenaratedSqlQuery,
                cleanedJson, tableRows: answerUserQueryModel.TablesRowsThatShouldBeReturned, modelId: answerUserQueryModel.ModelId ?? "phi4-mini");
            if (finalResult == "Something went wrong with the system. Please try again in a bit with a different input if possible.")
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "LLMError", response = "" });

            return Ok(new { error = "", stepTwo = generatedSqlQuery, stepThree = validatedGenaratedSqlQuery, stepFour = validatedAndCleanedSqlQuery, sqlQueryGenerationRetries = $"{5 - retries}", stepFive = cleanedJson, response = finalResult });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                error = "DataMicroserviceError",
                response = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", response = $"{ex.Message}" });
        }
    }
}
