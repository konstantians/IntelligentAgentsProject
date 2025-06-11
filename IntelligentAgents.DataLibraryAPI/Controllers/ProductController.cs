using IntelligentAgents.DataLibrary.DataAccess;
using IntelligentAgents.DataLibrary.Models.ResponseModels.ProductModels;
using Microsoft.AspNetCore.Mvc;

namespace IntelligentAgents.DataLibraryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductDataAccess _productDataAccess;

    public ProductController(IProductDataAccess productDataAccess)
    {
        _productDataAccess = productDataAccess;
    }

    [HttpGet("Amount/{amount}")]
    public async Task<IActionResult> GetProducts(int amount)
    {
        try
        {
            ReturnProductsAndCodeResponseModel response = await _productDataAccess.GetProductsAsync(amount);
            return Ok(response.Products);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(string id)
    {
        try
        {
            ReturnProductAndCodeResponseModel response = await _productDataAccess.GetProductByIdAsync(id);
            if (response.Product is null)
                return NotFound();

            return Ok(response.Product);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }


    [HttpGet("Name/{name}")]
    public async Task<IActionResult> GetProductByName(string name)
    {
        try
        {
            ReturnProductAndCodeResponseModel response = await _productDataAccess.GetProductByNameAsync(name);
            if (response.Product is null)
                return NotFound();

            return Ok(response.Product);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }
}
