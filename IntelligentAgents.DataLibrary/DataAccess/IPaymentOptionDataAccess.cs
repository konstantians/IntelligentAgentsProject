using IntelligentAgents.DataLibrary.Models.ResponseModels.PaymentOptionModels;

namespace IntelligentAgents.DataLibrary.DataAccess;
public interface IPaymentOptionDataAccess
{
    Task<ReturnPaymentOptionAndCodeResponseModel> GetPaymentOptionByIdAsync(string id);
    Task<ReturnPaymentOptionAndCodeResponseModel> GetPaymentOptionByNameAsync(string name);
    Task<ReturnPaymentOptionsAndCodeResponseModel> GetPaymentOptionsAsync(int amount);
}