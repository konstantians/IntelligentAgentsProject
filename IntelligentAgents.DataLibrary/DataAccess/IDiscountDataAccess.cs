using IntelligentAgents.DataLibrary.Models.ResponseModels.DiscountModels;

namespace IntelligentAgents.DataLibrary.DataAccess;

public interface IDiscountDataAccess
{
    Task<ReturnDiscountAndCodeResponseModel> GetDiscountByIdAsync(string id);
    Task<ReturnDiscountAndCodeResponseModel> GetDiscountByNameAsync(string name);
    Task<ReturnDiscountsAndCodeResponseModel> GetDiscountsAsync(int amount);
}