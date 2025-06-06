using IntelligentAgents.DataLibrary.DataAccess;
using IntelligentAgents.DataLibrary.Models.ResponseModels.PaymentOptionModels;
using Microsoft.AspNetCore.Mvc;

namespace IntelligentAgents.DataLibraryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]

public class PaymentOptionController : ControllerBase
{
    private readonly IPaymentOptionDataAccess _paymentOptionDataAccess;

    public PaymentOptionController(IPaymentOptionDataAccess paymentOptionDataAccess)
    {
        _paymentOptionDataAccess = paymentOptionDataAccess;
    }

    [HttpGet("Amount/{amount}")]
    public async Task<IActionResult> GetPaymentOptions(int amount)
    {
        try
        {
            ReturnPaymentOptionsAndCodeResponseModel response = await _paymentOptionDataAccess.GetPaymentOptionsAsync(amount);
            return Ok(response.PaymentOptions);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPaymentOptionById(string id)
    {
        try
        {
            ReturnPaymentOptionAndCodeResponseModel response = await _paymentOptionDataAccess.GetPaymentOptionByIdAsync(id);
            if (response.PaymentOption is null)
                return NotFound();

            return Ok(response.PaymentOption);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("Name/{name}")]
    public async Task<IActionResult> GetPaymentOptionByName(string name)
    {
        try
        {
            ReturnPaymentOptionAndCodeResponseModel response = await _paymentOptionDataAccess.GetPaymentOptionByNameAsync(name);
            if (response.PaymentOption is null)
                return NotFound();

            return Ok(response.PaymentOption);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }
}
