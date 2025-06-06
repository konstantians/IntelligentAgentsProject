using IntelligentAgents.AuthLibrary.Models.ResponseModels;

namespace IntelligentAgents.AuthLibrary.AuthLogic;

public interface IAuthenticationProcedures
{
    Task<ReturnUserAndCodeResponseModel> GetCurrentUserByTokenAsync(string token);
    Task<ReturnTokenAndCodeResponseModel> SignInAsync(string username, string password, bool isPersistent);
}