using IntelligentAgents.DataLibrary.Models.ResponseModels.CategoryModels;

namespace IntelligentAgents.DataLibrary.DataAccess;

public interface ICategoryDataAccess
{
    Task<ReturnCategoriesAndCodeResponseModel> GetCategoriesAsync(int amount);
    Task<ReturnCategoryAndCodeResponseModel> GetCategoryByIdAsync(string id);
    Task<ReturnCategoryAndCodeResponseModel> GetCategoryByNameAsync(string name);
}