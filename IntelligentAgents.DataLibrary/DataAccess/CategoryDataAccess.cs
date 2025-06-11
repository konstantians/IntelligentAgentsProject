using IntelligentAgents.DataLibrary.Models;
using IntelligentAgents.DataLibrary.Models.ResponseModels;
using IntelligentAgents.DataLibrary.Models.ResponseModels.CategoryModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IntelligentAgents.DataLibrary.DataAccess;

public class CategoryDataAccess : ICategoryDataAccess
{
    private readonly AppDataDbContext _appDataDbContext;
    private readonly ILogger<CategoryDataAccess> _logger;

    public CategoryDataAccess(AppDataDbContext appDataDbContext, ILogger<CategoryDataAccess> logger = null!)
    {
        _appDataDbContext = appDataDbContext;
        _logger = logger ?? NullLogger<CategoryDataAccess>.Instance;
    }

    public async Task<ReturnCategoriesAndCodeResponseModel> GetCategoriesAsync(int amount)
    {
        try
        {
            List<Category> categories = await _appDataDbContext.Categories
                .Include(c => c.Products)
                    .ThenInclude(p => p.Variants)
                        .ThenInclude(v => v.Discount)
                .Take(amount)
                .ToListAsync();

            //make all embeddings null
            foreach (var category in categories)
                foreach (var product in category.Products)
                    product.Embedding = null;

            return new ReturnCategoriesAndCodeResponseModel(categories, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetCategoriesFailure"), ex, "An error occurred while retrieving the categories. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnCategoryAndCodeResponseModel> GetCategoryByIdAsync(string id)
    {
        try
        {
            Category? foundCategory = await _appDataDbContext.Categories
                .Include(c => c.Products)
                    .ThenInclude(p => p.Variants)
                        .ThenInclude(v => v.Discount)
                .FirstOrDefaultAsync(category => category.Id!.Contains(id));

            //make all embeddings null
            foreach (var product in foundCategory?.Products ?? new List<Product>())
                product.Embedding = null;

            return new ReturnCategoryAndCodeResponseModel(foundCategory!, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetCategoryByIdFailure"), ex, "An error occurred while retrieving the category with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnCategoryAndCodeResponseModel> GetCategoryByNameAsync(string name)
    {
        try
        {
            Category? foundCategory = await _appDataDbContext.Categories
                .Include(c => c.Products)
                    .ThenInclude(p => p.Variants)
                        .ThenInclude(v => v.Discount)
                .FirstOrDefaultAsync(category => category.Name!.Contains(name));

            //make all embeddings null
            foreach (var product in foundCategory?.Products ?? new List<Product>())
                product.Embedding = null;

            return new ReturnCategoryAndCodeResponseModel(foundCategory!, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetCategoryByNameFailure"), ex, "An error occurred while retrieving the category with Name={name}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", name, ex.Message, ex.StackTrace);
            throw;
        }
    }
}
