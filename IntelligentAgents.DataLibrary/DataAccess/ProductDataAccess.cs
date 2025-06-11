using IntelligentAgents.DataLibrary.Models;
using IntelligentAgents.DataLibrary.Models.ResponseModels;
using IntelligentAgents.DataLibrary.Models.ResponseModels.ProductModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IntelligentAgents.DataLibrary.DataAccess;

public class ProductDataAccess : IProductDataAccess
{
    private readonly AppDataDbContext _appDataDbContext;
    private readonly ILogger<ProductDataAccess> _logger;

    public ProductDataAccess(AppDataDbContext appDataDbContext, ILogger<ProductDataAccess> logger = null!)
    {
        _appDataDbContext = appDataDbContext;
        _logger = logger ?? NullLogger<ProductDataAccess>.Instance;
    }

    public async Task<ReturnProductsAndCodeResponseModel> GetProductsAsync(int amount)
    {
        try
        {
            List<Product> products = await _appDataDbContext.Products
                .Include(p => p.Categories)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Discount)
                .Take(amount)
                .Select(product => new Product
                {
                    Id = product.Id,
                    Code = product.Code,
                    Name = product.Name,
                    Description = product.Description,
                    CreatedAt = product.CreatedAt,
                    Categories = product.Categories,
                    Variants = product.Variants
                })
                .ToListAsync();
            return new ReturnProductsAndCodeResponseModel(products, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetProductsFailure"), ex, "An error occurred while retrieving the products. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnProductAndCodeResponseModel> GetProductByIdAsync(string id)
    {
        try
        {
            Product? foundProduct = await _appDataDbContext.Products
                .Where(product => product.Id!.Contains(id))
                .Include(p => p.Categories)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Discount)
                .Select(product => new Product
                {
                    Id = product.Id,
                    Code = product.Code,
                    Name = product.Name,
                    Description = product.Description,
                    CreatedAt = product.CreatedAt,
                    Categories = product.Categories,
                    Variants = product.Variants
                }).FirstOrDefaultAsync();

            return new ReturnProductAndCodeResponseModel(foundProduct!, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetProductByIdFailure"), ex, "An error occurred while retrieving the product with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnProductAndCodeResponseModel> GetProductByNameAsync(string name)
    {
        try
        {
            Product? foundProduct = await _appDataDbContext.Products
                .Where(product => product.Name!.Contains(name))
                .Include(p => p.Categories)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Discount)
                .Select(product => new Product
                {
                    Id = product.Id,
                    Code = product.Code,
                    Name = product.Name,
                    Description = product.Description,
                    CreatedAt = product.CreatedAt,
                    Categories = product.Categories,
                    Variants = product.Variants
                }).FirstOrDefaultAsync();

            return new ReturnProductAndCodeResponseModel(foundProduct!, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetProductByNameFailure"), ex, "An error occurred while retrieving the product with Name={name}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", name, ex.Message, ex.StackTrace);
            throw;
        }
    }
}
