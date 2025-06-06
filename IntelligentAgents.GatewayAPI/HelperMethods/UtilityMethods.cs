using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace IntelligentAgents.GatewayAPI.HelperMethods;

public class UtilityMethods : IUtilityMethods
{
    public string? SetDefaultHeadersForClient(bool includeJWTAuthenticationHeaders, HttpClient httpClient, HttpRequest? httpRequest = null)
    {
        string? returnedAccessToken = null;
        if (includeJWTAuthenticationHeaders)
        {
            string authorizationHeader = httpRequest!.Headers["Authorization"]!;
            string accessToken = authorizationHeader.Substring("Bearer ".Length).Trim();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            returnedAccessToken = accessToken;
        }

        return returnedAccessToken;
    }

    public async Task<IActionResult> CommonHandlingForErrorCodesAsync(HttpResponseMessage response)
    {
        string responseBody = await response.Content.ReadAsStringAsync();
        //in the case there is no body
        if (string.IsNullOrEmpty(responseBody))
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return new UnauthorizedResult();
            else if (response.StatusCode == HttpStatusCode.Forbidden)
                return new StatusCodeResult(StatusCodes.Status403Forbidden);
            else if (response.StatusCode == HttpStatusCode.BadRequest)
                return new BadRequestResult();
            else if (response.StatusCode == HttpStatusCode.NotFound)
                return new NotFoundResult();
            else if (response.StatusCode == HttpStatusCode.MethodNotAllowed)
                return new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);
            //500 status codes errors
            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
            else if (response.StatusCode == HttpStatusCode.InternalServerError)
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);

            return new BadRequestResult(); //this will probably never happen
        }

        //otherwise
        var keyValue = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);
        keyValue!.TryGetValue("errorMessage", out object? errorMessageObject);
        string? errorMessage = errorMessageObject?.ToString() ?? null;
        keyValue!.TryGetValue("errors", out var errors);

        if (response.StatusCode == HttpStatusCode.Unauthorized && errorMessage is not null)
            return new UnauthorizedObjectResult(new
            {
                ErrorMessage = errorMessage
            });
        else if (response.StatusCode == HttpStatusCode.Unauthorized)
            return new UnauthorizedResult();
        else if (response.StatusCode == HttpStatusCode.Forbidden && errorMessage is not null)
            return new ObjectResult(new { ErrorMessage = errorMessage }) { StatusCode = StatusCodes.Status403Forbidden };
        else if (response.StatusCode == HttpStatusCode.Forbidden)
            return new StatusCodeResult(StatusCodes.Status403Forbidden);
        else if (response.StatusCode == HttpStatusCode.BadRequest && errorMessage is not null)
            return new BadRequestObjectResult(new { ErrorMessage = errorMessage });
        else if (response.StatusCode == HttpStatusCode.BadRequest && errors is not null) //this is for request validation errors
            return new BadRequestObjectResult(new { Errors = errors });
        else if (response.StatusCode == HttpStatusCode.NotFound && errorMessage is not null)
            return new NotFoundObjectResult(new { ErrorMessage = errorMessage });
        else if (response.StatusCode == HttpStatusCode.NotFound)
            return new NotFoundResult();
        else if (response.StatusCode == HttpStatusCode.MethodNotAllowed)
            return new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);
        //500 status codes errors
        else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            return new ObjectResult(new { ErrorMessage = errorMessage }) { StatusCode = StatusCodes.Status503ServiceUnavailable };
        else if (response.StatusCode == HttpStatusCode.InternalServerError)
            return new ObjectResult(new { ErrorMessage = errorMessage }) { StatusCode = StatusCodes.Status500InternalServerError };

        return new BadRequestResult(); //this will probably never happen
    }

    public async Task<HttpResponseMessage> MakeRequestWithRetriesForServerErrorAsync(Func<Task<HttpResponseMessage>> httpRequestCall, int maxRetries = 3)
    {
        HttpResponseMessage response = null!;
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            response = await httpRequestCall();
            if ((int)response.StatusCode < 500) // if it is success or client error then stop trying
                break;
        }

        return response;
    }
}