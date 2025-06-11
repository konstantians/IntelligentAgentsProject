﻿using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace IntelligentAgents.MVC.ControllerUtilities;

public static class HelperMethods
{
    public static async Task<IActionResult?> CommonErrorValidation(Controller controller, ILogger logger, HttpResponseMessage response, string? responseBody, string redirectToAction,
        string redirectToController, object? routeValues = null, bool responseBodyWasPassedIn = false)
    {
        //this deals with 5xx errors
        if (response.StatusCode == HttpStatusCode.InternalServerError)
            return controller.RedirectToAction("Error500");
        else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            return controller.RedirectToAction("Error503");
        else if ((int)response.StatusCode >= 500)
            return controller.RedirectToAction("Error500");

        //this deals with 4xx errors with empty response bodies
        responseBody = responseBodyWasPassedIn ? responseBody : await response.Content.ReadAsStringAsync();
        if ((int)response.StatusCode >= 400 && string.IsNullOrEmpty(responseBody))
        {
            controller.TempData["UnknownError"] = true;
            return controller.RedirectToAction(redirectToAction, redirectToController, routeValues: routeValues);
        }
        //this deals with 4xx errors with non-empty response bodies
        else if ((int)response.StatusCode >= 400)
        {
            try
            {
                var responseObject = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody!);
                responseObject!.TryGetValue("errorMessage", out string? errorMessage);
                controller.TempData[errorMessage ?? "UnknownError"] = true;
                return controller.RedirectToAction(redirectToAction, redirectToController, routeValues: routeValues);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Unexpected front end error");
                controller.TempData["UnknownError"] = true;
                return controller.RedirectToAction(redirectToAction, redirectToController, routeValues: routeValues);
            }
        }

        return null;
    }

}
