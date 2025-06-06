using IntelligentAgents.DataLibrary.Models;
using IntelligentAgents.DataLibrary.Models.ResponseModels;
using IntelligentAgents.DataLibrary.Models.ResponseModels.PaymentOptionModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IntelligentAgents.DataLibrary.DataAccess;
public class PaymentOptionDataAccess : IPaymentOptionDataAccess
{
    private readonly AppDataDbContext _appDataDbContext;
    private readonly ILogger<PaymentOptionDataAccess> _logger;

    public PaymentOptionDataAccess(AppDataDbContext appDataDbContext, ILogger<PaymentOptionDataAccess> logger = null!)
    {
        _appDataDbContext = appDataDbContext;
        _logger = logger ?? NullLogger<PaymentOptionDataAccess>.Instance;
    }

    public async Task<ReturnPaymentOptionsAndCodeResponseModel> GetPaymentOptionsAsync(int amount)
    {
        try
        {
            List<PaymentOption> paymentOptions = await _appDataDbContext.PaymentOptions.Take(amount).ToListAsync();

            return new ReturnPaymentOptionsAndCodeResponseModel(paymentOptions, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetPaymentOptionsFailure"), ex, "An error occurred while retrieving the payment options. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnPaymentOptionAndCodeResponseModel> GetPaymentOptionByNameAsync(string name)
    {
        try
        {
            PaymentOption? foundPaymentOption = await _appDataDbContext.PaymentOptions
                    .FirstOrDefaultAsync(paymentOption => paymentOption.Name!.Contains(name));

            return new ReturnPaymentOptionAndCodeResponseModel(foundPaymentOption!, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetPaymentOptionByNameFailure"), ex, "An error occurred while retrieving the payment option with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", name, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnPaymentOptionAndCodeResponseModel> GetPaymentOptionByIdAsync(string id)
    {
        try
        {
            PaymentOption? foundPaymentOption = await _appDataDbContext.PaymentOptions
                    .FirstOrDefaultAsync(paymentOption => paymentOption.Id!.Contains(id));

            return new ReturnPaymentOptionAndCodeResponseModel(foundPaymentOption!, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetPaymentOptionByIdFailure"), ex, "An error occurred while retrieving the payment option with Name={name}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", id, ex.Message, ex.StackTrace);
            throw;
        }
    }
}
