using IntelligentAgents.DataLibrary.Models.ResponseModels.ProductModels;

namespace IntelligentAgents.DataLibrary.DataAccess;

public interface IProductDataAccess
{
    Task<ReturnProductAndCodeResponseModel> GetProductByIdAsync(string id);
    Task<ReturnProductAndCodeResponseModel> GetProductByNameAsync(string name);
    Task<ReturnProductsAndCodeResponseModel> GetProductsAsync(int amount);
    Task<ReturnProductsAndCodeResponseModel> GetProductsOfCategoryAsync(string categoryId, int amount);
}