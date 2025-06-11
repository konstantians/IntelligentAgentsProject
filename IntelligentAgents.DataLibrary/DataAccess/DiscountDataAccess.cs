using IntelligentAgents.DataLibrary.Models;
using IntelligentAgents.DataLibrary.Models.ResponseModels;
using IntelligentAgents.DataLibrary.Models.ResponseModels.DiscountModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IntelligentAgents.DataLibrary.DataAccess;

public class DiscountDataAccess : IDiscountDataAccess
{
    private readonly AppDataDbContext _appDataDbContext;
    private readonly ILogger<DiscountDataAccess> _logger;

    public DiscountDataAccess(AppDataDbContext appDataDbContext, ILogger<DiscountDataAccess> logger = null!)
    {
        _appDataDbContext = appDataDbContext;
        _logger = logger ?? NullLogger<DiscountDataAccess>.Instance;
    }

    public async Task<ReturnDiscountsAndCodeResponseModel> GetDiscountsAsync(int amount)
    {
        try
        {
            List<Discount> discounts = await _appDataDbContext.Discounts
                    .Include(d => d.Variants)
                        .ThenInclude(v => v.Product)
                            .ThenInclude(p => p!.Categories)
                    .Take(amount)
                    .ToListAsync();

            //make all embeddings null
            foreach (var discount in discounts)
                foreach (var variant in discount?.Variants ?? new List<Variant>())
                    variant.Product!.Embedding = null;


            return new ReturnDiscountsAndCodeResponseModel(discounts, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetDiscountsFailure"), ex, "An error occurred while retrieving the discounts. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnDiscountAndCodeResponseModel> GetDiscountByIdAsync(string id)
    {
        try
        {
            Discount? foundDiscount = await _appDataDbContext.Discounts
                    .Include(d => d.Variants)
                        .ThenInclude(v => v.Product)
                            .ThenInclude(p => p!.Categories)
                    .FirstOrDefaultAsync(discount => discount.Id!.Contains(id));

            //make all embeddings null
            foreach (var variant in foundDiscount?.Variants ?? new List<Variant>())
                variant.Product!.Embedding = null;

            return new ReturnDiscountAndCodeResponseModel(foundDiscount!, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetDiscountByIdFailure"), ex, "An error occurred while retrieving the discount with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnDiscountAndCodeResponseModel> GetDiscountByNameAsync(string name)
    {
        try
        {
            Discount? foundDiscount = await _appDataDbContext.Discounts
                    .Include(d => d.Variants)
                        .ThenInclude(v => v.Product)
                            .ThenInclude(p => p!.Categories)
                    .FirstOrDefaultAsync(discount => discount.Name!.Contains(name));

            //make all embeddings null
            foreach (var variant in foundDiscount?.Variants ?? new List<Variant>())
                variant.Product!.Embedding = null;

            return new ReturnDiscountAndCodeResponseModel(foundDiscount!, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetDiscountByNameFailure"), ex, "An error occurred while retrieving the discount with Name={name}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", name, ex.Message, ex.StackTrace);
            throw;
        }
    }
}
