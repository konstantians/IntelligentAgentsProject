using IntelligentAgents.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace IntelligentAgentsAssignment.MVC.Controllers;
public class HomeController : Controller
{
    //TODO fix the navbar and the footer... They are cooked
    private readonly HttpClient httpClient;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IHttpClientFactory httpClientFactory, ILogger<HomeController> logger)
    {
        httpClient = httpClientFactory.CreateClient("GatewayApiClient");
        _logger = logger;
    }

    public IActionResult Index()
    {
        //TODO Add 2 options. One option is the anonymous one and the other option is the non anonymous one
        return View();
    }

    //this should be called with AJAX
    [HttpPost]
    public async Task<IActionResult> AnswerUserQuery([FromBody] UiAnswerUserQueryModel uiAnswerUserQueryModel)
    {
        //probably validation or some shit
        try
        {
            HttpResponseMessage response;
            if (uiAnswerUserQueryModel.AiAssistantMode == "Rag")
                response = await httpClient.PostAsJsonAsync("IntelligentAgent/withEmbeddings", new
                {
                    userQuery = uiAnswerUserQueryModel.UserQuery,
                    tablesRowsThatShouldBeReturned = uiAnswerUserQueryModel.TablesRowsThatShouldBeReturned,
                    modeId = uiAnswerUserQueryModel.ModelId
                });
            else if (uiAnswerUserQueryModel.AiAssistantMode == "Tool")
                response = await httpClient.PostAsJsonAsync("IntelligentAgent/withTools", new
                {
                    userQuery = uiAnswerUserQueryModel.UserQuery,
                    tablesRowsThatShouldBeReturned = uiAnswerUserQueryModel.TablesRowsThatShouldBeReturned,
                    modeId = uiAnswerUserQueryModel.ModelId
                });
            else if (uiAnswerUserQueryModel.AiAssistantMode == "Sql")
                response = await httpClient.PostAsJsonAsync("IntelligentAgent/withSqlGeneration", new
                {
                    userQuery = uiAnswerUserQueryModel.UserQuery,
                    tablesRowsThatShouldBeReturned = uiAnswerUserQueryModel.TablesRowsThatShouldBeReturned,
                    modeId = uiAnswerUserQueryModel.ModelId
                });
            else
                return BadRequest();

            string responseBody = await response.Content.ReadAsStringAsync();

            //this deals with 5xx errors
            if (response.StatusCode == HttpStatusCode.InternalServerError)
                return StatusCode(500);
            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                Dictionary<string, string> responseObject = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody)!;
                return StatusCode(503, new { error = responseObject!["error"], response = "" });
            }

            //this deals with 4xx errors with empty response bodies
            if ((int)response.StatusCode >= 400 && string.IsNullOrEmpty(responseBody))
                return BadRequest(new { error = "UnknownError", response = "" });
            //this deals with 4xx errors with non-empty response bodies
            else if ((int)response.StatusCode >= 400)
            {
                try
                {
                    Dictionary<string, string> responseObject = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody)!;
                    responseObject!.TryGetValue("error", out string? error);
                    responseObject!.TryGetValue("stepTwo", out string? stepTwo);
                    responseObject!.TryGetValue("stepThree", out string? stepThree);
                    responseObject!.TryGetValue("stepFour", out string? stepFour);
                    responseObject!.TryGetValue("toolSelectionRetries", out string? toolSelectionRetriesValue);
                    if (stepFour is not null)
                        return BadRequest(new { error = error, stepTwo = stepTwo, stepThree = stepThree, stepFour = stepFour });
                    else if (stepTwo is not null)
                        return BadRequest(new { error = error, stepTwo = stepTwo, toolSelectionRetries = toolSelectionRetriesValue, response = "" });
                    else
                        return BadRequest(new { error = error, response = "" });
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Unexpected front end error");
                    return BadRequest(new { error = "UnknownError", response = "" });
                }
            }

            var successResponseObject = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody);
            successResponseObject!.TryGetValue("stepFive", out string? stepFive);
            successResponseObject!.TryGetValue("toolSelectionRetries", out string? toolSelectionRetries);
            successResponseObject!.TryGetValue("endpointGenerationRetries", out string? endpointGenerationRetries);
            successResponseObject!.TryGetValue("sqlQueryGenerationRetries", out string? sqlQueryGenerationRetries);

            return Ok(new
            {
                error = "NoError",
                stepTwo = successResponseObject!["stepTwo"],
                stepThree = successResponseObject!["stepThree"],
                stepFour = successResponseObject!["stepFour"],
                stepFive = stepFive,
                response = successResponseObject!["response"],
                toolSelectionRetries = toolSelectionRetries,
                endpointGenerationRetries = endpointGenerationRetries,
                sqlQueryGenerationRetries = sqlQueryGenerationRetries
            });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                error = "GatewayAPIError",
                response = ex.Message
            });
        }
        catch (Exception)
        {
            return StatusCode(500, "Internal Controller Error");
            throw;
        }
    }
}
