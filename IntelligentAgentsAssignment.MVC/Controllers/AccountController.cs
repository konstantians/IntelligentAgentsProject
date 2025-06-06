using IntelligentAgents.MVC.ControllerUtilities;
using IntelligentAgents.MVC.Models;
using IntelligentAgentsAssignment.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace IntelligentAgents.MVC.Controllers;

public class AccountController : Controller
{
    private readonly HttpClient httpClient;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IHttpClientFactory httpClientFactory, ILogger<AccountController> logger)
    {
        httpClient = httpClientFactory.CreateClient("GatewayApiClient");
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> SignIn(SignInViewModel signInViewModel)
    {
        //if there is an access token just send them to homepage
        if (!string.IsNullOrEmpty(Request.Cookies["IntelligentAgentsAuthenticationCookie"]))
            return RedirectToAction("Index", "Home");

        //if the model state is invalid
        if (!ModelState.IsValid)
            return View("SignInAndSignUp");

        var apiSignInModel = new Dictionary<string, string>
        {
            { "email", signInViewModel.Email! },
            { "password", signInViewModel.Password! }
        };

        var response = await httpClient.PostAsJsonAsync("GatewayAuthentication/SignIn", apiSignInModel);
        var responseBody = await response.Content.ReadAsStringAsync();

        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, responseBody, "SignIn", "Account", responseBodyWasPassedIn: true);
        if (validationResult is not null)
            return validationResult;

        //if status code is 200
        Dictionary<string, string>? noErrorResponseObject = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody);
        if (noErrorResponseObject != null && noErrorResponseObject.TryGetValue("accessToken", out string? accessToken))
            SetUpAuthenticationCookie(accessToken, signInViewModel.RememberMe ? 30 : 0);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public IActionResult LogOut()
    {
        string? accessToken = Request.Cookies["IntelligentAgentsAuthenticationCookie"];
        if (string.IsNullOrEmpty(accessToken))
            return View("Error");

        Response.Cookies.Delete("IntelligentAgentsAuthenticationCookie");
        return RedirectToAction("Index", "Home");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private void SetUpAuthenticationCookie(string accessToken, int duration = 0)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            IsEssential = true
        };

        //no value means that the cookie will be destroyed when the browser closes
        if (duration != 0)
            cookieOptions.Expires = DateTimeOffset.Now.AddDays(30);

        Response.Cookies.Append("IntelligentAgentsAuthenticationCookie", accessToken, cookieOptions);
    }
}
