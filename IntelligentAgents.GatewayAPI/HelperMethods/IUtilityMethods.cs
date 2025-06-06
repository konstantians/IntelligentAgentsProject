using Microsoft.AspNetCore.Mvc;

namespace IntelligentAgents.GatewayAPI.HelperMethods;

public interface IUtilityMethods
{
    Task<IActionResult> CommonHandlingForErrorCodesAsync(HttpResponseMessage response);
    Task<HttpResponseMessage> MakeRequestWithRetriesForServerErrorAsync(Func<Task<HttpResponseMessage>> httpRequestCall, int maxRetries = 3);
    string? SetDefaultHeadersForClient(bool includeJWTAuthenticationHeaders, HttpClient httpClient, HttpRequest? httpRequest = null);
}