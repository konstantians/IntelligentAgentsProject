using IntelligentAgents.DataLibrary.Models;
using IntelligentAgents.DataLibrary.Models.ResponseModels;
using IntelligentAgents.DataLibrary.Models.ResponseModels.ShippingOptionModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IntelligentAgents.DataLibrary.DataAccess;
public class ShippingOptionDataAccess : IShippingOptionDataAccess
{
    private readonly AppDataDbContext _appDataDbContext;
    private readonly ILogger<ShippingOptionDataAccess> _logger;

    public ShippingOptionDataAccess(AppDataDbContext appDataDbContext, ILogger<ShippingOptionDataAccess> logger = null!)
    {
        _appDataDbContext = appDataDbContext;
        _logger = logger ?? NullLogger<ShippingOptionDataAccess>.Instance;
    }

    public async Task<ReturnShippingOptionsAndCodeResponseModel> GetShippingOptionsAsync(int amount)
    {
        try
        {
            List<ShippingOption> shippingOptions = await _appDataDbContext.ShippingOptions
                    .Take(amount)
                    .ToListAsync();

            return new ReturnShippingOptionsAndCodeResponseModel(shippingOptions, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetShippingOptionsFailure"), ex, "An error occurred while retrieving the shipping options. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnShippingOptionAndCodeResponseModel> GetShippingOptionByIdAsync(string id)
    {
        try
        {
            ShippingOption? foundShippingOption = await _appDataDbContext.ShippingOptions
                    .FirstOrDefaultAsync(shippingOption => shippingOption.Id!.Contains(id));

            return new ReturnShippingOptionAndCodeResponseModel(foundShippingOption!, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetShippingOptionByIdFailure"), ex, "An error occurred while retrieving the shipping option with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnShippingOptionAndCodeResponseModel> GetShippingOptionByNameAsync(string name)
    {
        try
        {
            ShippingOption? foundShippingOption = await _appDataDbContext.ShippingOptions
                    .FirstOrDefaultAsync(shippingOption => shippingOption.Name!.Contains(name));

            return new ReturnShippingOptionAndCodeResponseModel(foundShippingOption!, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetShippingOptionByNameFailure"), ex, "An error occurred while retrieving the shipping option with Name={name}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", name, ex.Message, ex.StackTrace);
            throw;
        }
    }
}
