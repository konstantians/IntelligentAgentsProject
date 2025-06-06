using IntelligentAgents.DataLibrary.Models;
using IntelligentAgents.DataLibrary.Models.ResponseModels;
using IntelligentAgents.DataLibrary.Models.ResponseModels.VariantModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IntelligentAgents.DataLibrary.DataAccess;

public class VariantDataAccess : IVariantDataAccess
{
    private readonly AppDataDbContext _appDataDbContext;
    private readonly ILogger<VariantDataAccess> _logger;

    public VariantDataAccess(AppDataDbContext appDataDbContext, ILogger<VariantDataAccess> logger = null!)
    {
        _appDataDbContext = appDataDbContext;
        _logger = logger ?? NullLogger<VariantDataAccess>.Instance;
    }

    public async Task<ReturnVariantsAndCodeResponseModel> GetVariantsAsync(int amount)
    {
        try
        {
            //because it might not be very clear, the variant can have many variant images with each variant filteredImage having only one filteredImage. The reason why it is clunky it is because IsThumbnail property needs to be in the bridge table
            List<Variant> variants = await _appDataDbContext.Variants
                .Include(v => v.Discount)
                .Include(v => v.Product)
                    .ThenInclude(p => p!.Categories)
                .Take(amount)
                .ToListAsync();

            return new ReturnVariantsAndCodeResponseModel(variants, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetVariantsFailure"), ex, "An error occurred while retrieving the variants. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnVariantsAndCodeResponseModel> GetVariantByTheirSKUsAsync(List<string> skus)
    {
        try
        {
            //because it might not be very clear, the variant can have many variant images with each variant filteredImage having only one filteredImage. The reason why it is clunky it is because IsThumbnail property needs to be in the bridge table
            List<Variant> variants = await _appDataDbContext.Variants
                .Include(v => v.Discount)
                .Include(v => v.Product)
                    .ThenInclude(p => p!.Categories)
                .Where(variant => skus.Contains(variant.SKU!))
                .ToListAsync();

            return new ReturnVariantsAndCodeResponseModel(variants, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetVariantByTheirSKUsAsync"), ex, "An error occurred while retrieving the variants. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }


    public async Task<ReturnVariantAndCodeResponseModel> GetVariantByIdAsync(string id)
    {
        try
        {
            Variant? foundVariant = await _appDataDbContext.Variants
                .Include(v => v.Discount)
                .Include(v => v.Product)
                    .ThenInclude(p => p!.Categories)
                .FirstOrDefaultAsync(variant => variant.Id!.Contains(id));

            return new ReturnVariantAndCodeResponseModel(foundVariant!, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetVariantByIdFailure"), ex, "An error occurred while retrieving the variant with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnVariantAndCodeResponseModel> GetVariantBySKUAsync(string sku)
    {
        try
        {
            Variant? foundVariant = await _appDataDbContext.Variants
                .Include(v => v.Discount)
                .Include(v => v.Product)
                    .ThenInclude(p => p!.Categories)
                .FirstOrDefaultAsync(variant => variant.SKU!.Contains(sku));

            return new ReturnVariantAndCodeResponseModel(foundVariant!, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetVariantBySkuFailure"), ex, "An error occurred while retrieving the variant with SKU={sku}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", sku, ex.Message, ex.StackTrace);
            throw;
        }
    }
}
