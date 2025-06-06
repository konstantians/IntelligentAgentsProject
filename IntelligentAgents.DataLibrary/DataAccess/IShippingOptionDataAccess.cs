using IntelligentAgents.DataLibrary.Models.ResponseModels.ShippingOptionModels;

namespace IntelligentAgents.DataLibrary.DataAccess;
public interface IShippingOptionDataAccess
{
    Task<ReturnShippingOptionAndCodeResponseModel> GetShippingOptionByNameAsync(string name);
    Task<ReturnShippingOptionAndCodeResponseModel> GetShippingOptionByIdAsync(string id);
    Task<ReturnShippingOptionsAndCodeResponseModel> GetShippingOptionsAsync(int amount);
}