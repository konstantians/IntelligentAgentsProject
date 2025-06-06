using IntelligentAgents.MVC.ControllerUtilities;
using Microsoft.AspNetCore.Mvc;

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
    public IActionResult UserQuery(string userQuery)
    {
        string? authenticationCookie = Request.Cookies["IntelligentAgentsAuthenticationCookie"];
        if (authenticationCookie is not null && !HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("IntelligentAgentsAuthenticationCookie");
            return RedirectToAction("Index", "Home");
        }

        string? userId = authenticationCookie is not null ? HelperMethods.GetUserIdFromCookie(Request) : null;

        //TODO continue for authenticated users
        //TODO continue for unauthenticated users

        return Json("blabla");
    }
}
