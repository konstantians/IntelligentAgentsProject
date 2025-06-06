using IntelligentAgents.GatewayAPI.AuthMicroService.GatewayAuthentication.Models;
using IntelligentAgents.GatewayAPI.HelperMethods;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json;

namespace IntelligentAgents.GatewayAPI.AuthMicroService.GatewayAuthentication;

[ApiController]
[EnableRateLimiting("DefaultWindowLimiter")]
[Route("api/[controller]")]
public class GatewayAuthenticationController : ControllerBase
{
    //the general idea with redirectUrl is that it can contain a returnUrl that the frontend handler(the endpoint that is specified as the redirectUrl) can use to redirect the user too after its own processing
    private readonly HttpClient authHttpClient;
    private readonly HttpClient emailHttpClient;
    private readonly HttpClient dataHttpClient;
    private readonly IConfiguration _configuration;
    private readonly IUtilityMethods _utilityMethods;

    public GatewayAuthenticationController(IConfiguration configuration, IHttpClientFactory httpClientFactory, IUtilityMethods utilityMethods)
    {
        _configuration = configuration;
        _utilityMethods = utilityMethods;
        authHttpClient = httpClientFactory.CreateClient("AuthApiClient");
        emailHttpClient = httpClientFactory.CreateClient("EmailApiClient");
        dataHttpClient = httpClientFactory.CreateClient("DataApiClient");
    }

    [HttpGet("GetUserByAccessToken")]
    public async Task<IActionResult> GetUserByAccessToken(bool? includeCart, bool? includeCoupons, bool? includeOrders)
    {
        //check that an access token has been supplied, this check is made to avoid unnecessary requests
        if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
            !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

        //request to get the user
        _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, HttpContext.Request);
        HttpResponseMessage response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => authHttpClient.GetAsync("Authentication/GetUserByAccessToken")); //this contains retry logic

        if ((int)response.StatusCode >= 400)
            return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

        string? responseBody = await response.Content.ReadAsStringAsync();
        GatewayAppUser? appUser = JsonSerializer.Deserialize<GatewayAppUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient);

        if (includeCoupons.HasValue && includeCoupons.Value)
        {
            //get the user coupons
            response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => dataHttpClient.GetAsync($"Coupon/userId/{appUser!.Id}/includeDeactivated/true"));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            responseBody = await response.Content.ReadAsStringAsync();
            //TODO
            //List<GatewayUserCoupon>? userCoupons = JsonSerializer.Deserialize<List<GatewayUserCoupon>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            //appUser!.UserCoupons = userCoupons!;
        }

        return Ok(appUser);
    }

    [HttpPost("SignIn")]
    public async Task<IActionResult> SignIn([FromBody] GatewaySignInRequestModel signInModel)
    {
        try
        {
            //sign in user
            _utilityMethods.SetDefaultHeadersForClient(false, authHttpClient);
            HttpResponseMessage? response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() =>
                authHttpClient.PostAsJsonAsync("Authentication/SignIn", new { signInModel.Email, signInModel.Password, signInModel.RememberMe }));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            //return access accessToken
            string? responseBody = await response.Content.ReadAsStringAsync();
            JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody)!.TryGetValue("accessToken", out string? accessToken);
            return Ok(new { AccessToken = accessToken });
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }
}
