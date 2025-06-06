using Microsoft.AspNetCore.Identity;

namespace IntelligentAgents.GatewayAPI.AuthMicroService;

public class GatewayAppUser : IdentityUser
{
    //public List<GatewayUserCoupon> UserCoupons { get; set; } = new List<GatewayUserCoupon>();
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
