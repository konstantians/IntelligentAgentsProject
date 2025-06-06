using IntelligentAgents.DataLibrary.DataAccess;
using IntelligentAgents.DataLibrary.Models.ResponseModels.CategoryModels;
using Microsoft.AspNetCore.Mvc;

namespace IntelligentAgents.DataLibraryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryDataAccess _categoryDataAccess;

    public CategoryController(ICategoryDataAccess categoryDataAccess)
    {
        _categoryDataAccess = categoryDataAccess;
    }

    [HttpGet("Amount/{amount}")]
    public async Task<IActionResult> GetCategories(int amount)
    {
        try
        {
            ReturnCategoriesAndCodeResponseModel response = await _categoryDataAccess.GetCategoriesAsync(amount);
            return Ok(response.Categories);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById(string id)
    {
        try
        {
            ReturnCategoryAndCodeResponseModel response = await _categoryDataAccess.GetCategoryByIdAsync(id);
            if (response.Category is null)
                return NotFound();

            return Ok(response.Category);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("Name/{name}")]
    public async Task<IActionResult> GetCategoryByName(string name)
    {
        try
        {
            ReturnCategoryAndCodeResponseModel response = await _categoryDataAccess.GetCategoryByNameAsync(name);
            if (response.Category is null)
                return NotFound();

            return Ok(response.Category);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }
}
