using IntelligentAgents.DataLibrary.Models.ResponseModels.CouponModels;

namespace IntelligentAgents.DataLibrary.DataAccess;
public interface ICouponDataAccess
{
    Task<ReturnCouponAndCodeResponseModel> GetCouponByIdAsync(string id);
    Task<ReturnCouponsAndCodeResponseModel> GetCouponsAsync(int amount);
    Task<ReturnUserCouponsAndCodeResponseModel> GetCouponsOfUserAsync(string userId);
}