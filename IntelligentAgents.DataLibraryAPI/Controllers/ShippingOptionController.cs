using IntelligentAgents.DataLibrary.DataAccess;
using IntelligentAgents.DataLibrary.Models.ResponseModels.ShippingOptionModels;
using Microsoft.AspNetCore.Mvc;

namespace IntelligentAgents.DataLibraryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]

public class ShippingOptionController : ControllerBase
{
    private readonly IShippingOptionDataAccess _shippingOptionDataAccess;

    public ShippingOptionController(IShippingOptionDataAccess shippingOptionDataAccess)
    {
        _shippingOptionDataAccess = shippingOptionDataAccess;
    }

    [HttpGet("Amount/{amount}")]
    public async Task<IActionResult> GetShippingOptions(int amount)
    {
        try
        {
            ReturnShippingOptionsAndCodeResponseModel response = await _shippingOptionDataAccess.GetShippingOptionsAsync(amount);
            return Ok(response.ShippingOptions);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetShippingOptionById(string id)
    {
        try
        {
            ReturnShippingOptionAndCodeResponseModel response = await _shippingOptionDataAccess.GetShippingOptionByIdAsync(id);
            if (response.ShippingOption is null)
                return NotFound();

            return Ok(response.ShippingOption);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("Name/{name}")]
    public async Task<IActionResult> GetShippingOptionByName(string name)
    {
        try
        {
            ReturnShippingOptionAndCodeResponseModel response = await _shippingOptionDataAccess.GetShippingOptionByNameAsync(name);
            if (response.ShippingOption is null)
                return NotFound();

            return Ok(response.ShippingOption);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }
}
