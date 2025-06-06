namespace IntelligentAgents.GatewayAPI.AuthMicroService.GatewayAuthentication.Models;

public class GatewaySignInRequestModel
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public bool RememberMe { get; set; }
}
