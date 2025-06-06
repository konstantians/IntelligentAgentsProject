using IntelligentAgents.DataLibrary.Models;
using IntelligentAgents.DataLibrary.Models.ResponseModels;
using IntelligentAgents.DataLibrary.Models.ResponseModels.CouponModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IntelligentAgents.DataLibrary.DataAccess;
public class CouponDataAccess : ICouponDataAccess
{
    private readonly AppDataDbContext _appDataDbContext;
    private readonly ILogger<CouponDataAccess> _logger;

    public CouponDataAccess(AppDataDbContext appDataDbContext, ILogger<CouponDataAccess> logger = null!)
    {
        _appDataDbContext = appDataDbContext;
        _logger = logger ?? NullLogger<CouponDataAccess>.Instance;
    }

    public async Task<ReturnCouponsAndCodeResponseModel> GetCouponsAsync(int amount)
    {
        try
        {
            List<Coupon> coupons = await _appDataDbContext.Coupons
                    .Include(c => c.UserCoupons)
                    .Take(amount)
                    .ToListAsync();

            return new ReturnCouponsAndCodeResponseModel(coupons, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetCouponsFailure"), ex, "An error occurred while retrieving the coupons. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnCouponAndCodeResponseModel> GetCouponByIdAsync(string id)
    {
        try
        {
            Coupon? foundCoupon = await _appDataDbContext.Coupons
                    .Include(c => c.UserCoupons)
                    .FirstOrDefaultAsync(coupon => coupon.Id == id);

            return new ReturnCouponAndCodeResponseModel(foundCoupon!, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetCouponByIdFailure"), ex, "An error occurred while retrieving the coupon with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnUserCouponsAndCodeResponseModel> GetCouponsOfUserAsync(string userId)
    {
        try
        {
            List<UserCoupon> userCoupons = await _appDataDbContext.UserCoupons
                    .Include(userCoupon => userCoupon.Coupon)
                    .Where(userCoupon => userCoupon.UserId == userId)
                    .ToListAsync();

            return new ReturnUserCouponsAndCodeResponseModel(userCoupons, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetCouponsOfUserFailure"), ex, "An error occurred while retrieving the coupons. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }
}
