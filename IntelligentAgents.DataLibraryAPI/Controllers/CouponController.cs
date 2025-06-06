using IntelligentAgents.DataLibrary.DataAccess;
using IntelligentAgents.DataLibrary.Models.ResponseModels.CouponModels;
using Microsoft.AspNetCore.Mvc;

namespace IntelligentAgents.DataLibraryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CouponController : ControllerBase
{
    private readonly ICouponDataAccess _couponDataAccess;

    public CouponController(ICouponDataAccess couponDataAccess)
    {
        _couponDataAccess = couponDataAccess;
    }

    [HttpGet("Amount/{amount}")]
    public async Task<IActionResult> GetCoupons(int amount)
    {
        try
        {
            ReturnCouponsAndCodeResponseModel response = await _couponDataAccess.GetCouponsAsync(amount);
            return Ok(response.Coupons);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCouponById(string id)
    {
        try
        {
            ReturnCouponAndCodeResponseModel response = await _couponDataAccess.GetCouponByIdAsync(id);
            if (response.Coupon is null)
                return NotFound();

            return Ok(response.Coupon);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("userId/{userId}")]
    public async Task<IActionResult> GetCouponsOfUser(string userId)
    {
        try
        {
            ReturnUserCouponsAndCodeResponseModel response = await _couponDataAccess.GetCouponsOfUserAsync(userId);
            return Ok(response.UserCoupons);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }
}
