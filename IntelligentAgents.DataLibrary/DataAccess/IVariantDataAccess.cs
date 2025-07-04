﻿using IntelligentAgents.DataLibrary.Models.ResponseModels.VariantModels;

namespace IntelligentAgents.DataLibrary.DataAccess;

public interface IVariantDataAccess
{
    Task<ReturnVariantAndCodeResponseModel> GetVariantByIdAsync(string id);
    Task<ReturnVariantAndCodeResponseModel> GetVariantBySKUAsync(string sku);
    Task<ReturnVariantsAndCodeResponseModel> GetVariantsAsync(int amount);
}