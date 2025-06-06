using IntelligentAgents.AuthLibrary.Models;
using IntelligentAgents.AuthLibrary.Models.ResponseModels;
using Microsoft.Extensions.Logging;

namespace IntelligentAgents.AuthLibrary.AuthLogic;

public interface IHelperMethods
{
    Task<bool> IsAccountLockedOut(AppUser appUser, EventId eventId, string loggingBodyText);
    bool IsEmailConfirmed(AppUser user, EventId eventId, string loggingText);
    Task<ReturnUserAndCodeResponseModel> StandardTokenAndUserValidationProcedures(string accessToken, EventId templateEvent);
}