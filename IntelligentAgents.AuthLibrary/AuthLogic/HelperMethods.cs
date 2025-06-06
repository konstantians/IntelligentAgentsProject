using IntelligentAgents.AuthLibrary.Models;
using IntelligentAgents.AuthLibrary.Models.ResponseModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IntelligentAgents.AuthLibrary.AuthLogic;

public class HelperMethods : IHelperMethods
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<AuthenticationProcedures> _logger;

    public HelperMethods(UserManager<AppUser> userManager, ILogger<AuthenticationProcedures> logger = null!)
    {
        _userManager = userManager;
        _logger = logger ?? NullLogger<AuthenticationProcedures>.Instance;
    }

    public bool IsEmailConfirmed(AppUser appUser, EventId eventId, string loggingBodyText)
    {
        if (!appUser.EmailConfirmed)
        {
            _logger.LogWarning(eventId, loggingBodyText, appUser.Email);
            return false;
        }

        return true;
    }

    public async Task<bool> IsAccountLockedOut(AppUser appUser, EventId eventId, string loggingBodyText)
    {
        if (await _userManager.IsLockedOutAsync(appUser))
        {
            _logger.LogWarning(eventId, loggingBodyText, appUser.Email);
            return true;
        }

        return false;
    }

    public async Task<ReturnUserAndCodeResponseModel> StandardTokenAndUserValidationProcedures(string accessToken, EventId templateEvent)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(accessToken);
        string userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!;
        string userEmail = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value!;
        var appUser = await _userManager.FindByIdAsync(userId);
        if (appUser is null || appUser.Email != userEmail) //we also check the email to invalidate old access tokens in case the user changed their email account
        {
            _logger.LogWarning(new EventId(templateEvent.Id, templateEvent.Name + "FailureDueToValidTokenButUserNotInSystem"),
                "The token was valid, but it does not correspond to any user in the system and thus the process could not proceed. AccessToken={AccessToken}.", accessToken);
            return new ReturnUserAndCodeResponseModel(null!, LibraryReturnedCodes.ValidTokenButUserNotInSystem);
        }

        if (!IsEmailConfirmed(appUser, new EventId(templateEvent.Id + 1, templateEvent.Name + "FailureDueToUnconfirmedEmail"),
            "The email of the account was not confirmed and thus the process could not proceed. Email={Email}."))
            return new ReturnUserAndCodeResponseModel(null!, LibraryReturnedCodes.UserAccountNotActivated);

        if (await IsAccountLockedOut(appUser, new EventId(templateEvent.Id + 2, templateEvent.Name + "FailureDueToAccountBeingLocked"),
            "The account was locked out at the time of the process and thus the process could not proceed: Email={Email}."))
            return new ReturnUserAndCodeResponseModel(null!, LibraryReturnedCodes.UserAccountLocked);

        return new ReturnUserAndCodeResponseModel(appUser, LibraryReturnedCodes.NoError);
    }
}
