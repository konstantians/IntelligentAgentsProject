using IntelligentAgents.DataLibrary.DataAccess;
using IntelligentAgents.DataLibrary.Models.ResponseModels.VariantModels;
using Microsoft.AspNetCore.Mvc;

namespace IntelligentAgents.DataLibraryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VariantController : ControllerBase
{
    private readonly IVariantDataAccess _variantDataAccess;

    public VariantController(IVariantDataAccess variantDataAccess)
    {
        _variantDataAccess = variantDataAccess;
    }

    [HttpGet("Amount/{amount}")]
    public async Task<IActionResult> GetVariants(int amount)
    {
        try
        {
            ReturnVariantsAndCodeResponseModel response = await _variantDataAccess.GetVariantsAsync(amount);
            return Ok(response.Variants);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    //this endpoint understands queries like: domain/endpoint/includeDeactivated/true?skus=sku1,sku2,skue3
    //or domain/endpoint/includeDeactivated/true?skus=sku1&skus=sku2&skus=sku3
    [HttpGet("GetVariantsBySkus")]
    public async Task<IActionResult> GetVariantsBySkus([FromQuery] List<string> skus)
    {
        try
        {
            if (skus.Count == 1 && skus[0].Contains(','))
                skus = skus[0].Split(',').ToList();

            ReturnVariantsAndCodeResponseModel response = await _variantDataAccess.GetVariantByTheirSKUsAsync(skus);
            return Ok(response.Variants);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("Id/{id}")]
    public async Task<IActionResult> GetVariantById(string id)
    {
        try
        {
            ReturnVariantAndCodeResponseModel response = await _variantDataAccess.GetVariantByIdAsync(id);
            if (response.Variant is null)
                return NotFound();

            return Ok(response.Variant);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("Sku/{sku}")]
    public async Task<IActionResult> GetVariantBySku(string sku)
    {
        try
        {
            ReturnVariantAndCodeResponseModel response = await _variantDataAccess.GetVariantBySKUAsync(sku);
            if (response.Variant is null)
                return NotFound();

            return Ok(response.Variant);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }
}
