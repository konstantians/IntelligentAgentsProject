using IntelligentAgents.DataLibrary.DataAccess;
using IntelligentAgents.DataLibrary.Models.ResponseModels.DiscountModels;
using Microsoft.AspNetCore.Mvc;

namespace IntelligentAgents.DataLibraryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiscountController : ControllerBase
{
    private readonly IDiscountDataAccess _discountDataAccess;

    public DiscountController(IDiscountDataAccess discountDataAccess)
    {
        _discountDataAccess = discountDataAccess;
    }

    [HttpGet("Amount/{amount}")]
    public async Task<IActionResult> GetDiscounts(int amount)
    {
        try
        {
            ReturnDiscountsAndCodeResponseModel response = await _discountDataAccess.GetDiscountsAsync(amount);
            return Ok(response.Discounts);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDiscountById(string id)
    {
        try
        {
            ReturnDiscountAndCodeResponseModel response = await _discountDataAccess.GetDiscountByIdAsync(id);
            if (response.Discount is null)
                return NotFound();

            return Ok(response.Discount);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("Name/{name}")]
    public async Task<IActionResult> GetDiscountByName(string name)
    {
        try
        {
            ReturnDiscountAndCodeResponseModel response = await _discountDataAccess.GetDiscountByNameAsync(name);
            if (response.Discount is null)
                return NotFound();

            return Ok(response.Discount);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }
}
