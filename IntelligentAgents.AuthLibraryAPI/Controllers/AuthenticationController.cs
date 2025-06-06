using IntelligentAgents.AuthLibrary.AuthLogic;
using IntelligentAgents.AuthLibrary.Models.ResponseModels;
using IntelligentAgents.AuthLibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace IntelligentAgents.AuthLibraryAPI.Controllers;

/// <summary>
/// 
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationProcedures _authenticationProcedures;
    private readonly IConfiguration _configuration;
    private static readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public AuthenticationController(IAuthenticationProcedures authenticationProcedures, IConfiguration configuration)
    {
        _authenticationProcedures = authenticationProcedures;
        _configuration = configuration;
    }

    /// <summary>
    /// This endpoint is the most fundemental endpoint and simply retrieves the a corresponding user of the access confirmEmailToken. The validation of the confirmEmailToken occurs in the middlewares of the api, 
    /// but this method also checks that the access confirmEmailToken, even if is valid, it actually belongs to a user in the system/database. 
    /// </summary>
    /// <returns>An isntance of appuser that corresponds to the access confirmEmailToken with their fields that exist in the auth database filled, or it returns Unauthorized, BadRequest or other error codes depending
    /// on the error case.</returns>
    [HttpGet("GetUserByAccessToken")]
    [Authorize]
    public async Task<IActionResult> GetUserByAccessToken()
    {
        try
        {
            // Retrieve the Authorization header from the HTTP request
            string authorizationHeader = HttpContext.Request.Headers["Authorization"]!;
            string token = authorizationHeader.Substring("Bearer ".Length).Trim();

            ReturnUserAndCodeResponseModel returnCodeAndUserResponseModel = await _authenticationProcedures.GetCurrentUserByTokenAsync(token);
            if (returnCodeAndUserResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButUserNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInSystem" });
            else if (returnCodeAndUserResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountNotActivated)
                return Unauthorized(new { ErrorMessage = "UserAccountNotActivated" });
            else if (returnCodeAndUserResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountLocked)
                return Unauthorized(new { ErrorMessage = "UserAccountLocked" });

            return Ok(returnCodeAndUserResponseModel.AppUser);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }


    /// <summary>
    /// This endpoint is used to sign in a user using the email and password fields that were given as input and optionally the rememberMe field. An important thing to keep in mind is that if 
    /// the account is not activated, even if the credentials are correct the sign in will return an error, so the ConfirmEmail endpoint should be used first before a sign in can succeed.
    /// </summary>
    /// <param name="signInModel">This parameter consists of 2 necessary and valid parameters, which are the email and the password of the account and the optional boolean parameter RememberMe, 
    /// which dictates the duration of the validity of the access confirmEmailToken(if false 1 day, if true 30 days as it stands).</param>
    /// <returns>it returns Ok status code with the newly created access confirmEmailToken, or Unauthorized, Internal server status codes depending on the error case.</returns>
    [HttpPost("SignIn")]
    [AllowAnonymous]
    public async Task<IActionResult> SignIn([FromBody] ApiSignInRequestModel signInModel)
    {
        try
        {
            ReturnTokenAndCodeResponseModel returnCodeAndTokenResponseModel = await _authenticationProcedures.SignInAsync(signInModel.Email!, signInModel.Password!, signInModel.RememberMe);
            if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserNotFoundWithGivenEmail)
                return Unauthorized(new { ErrorMessage = "UserNotFoundWithGivenEmail" });
            else if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountLocked)
                return Unauthorized(new { ErrorMessage = "UserAccountLocked" });
            else if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountNotActivated)
                return Unauthorized(new { ErrorMessage = "UserAccountNotActivated" });
            else if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.InvalidCredentials)
                return Unauthorized(new { ErrorMessage = "InvalidCredentials" });

            return Ok(new { AccessToken = returnCodeAndTokenResponseModel.Token });
        }
        catch
        {
            return StatusCode(500);
        }
    }
}
