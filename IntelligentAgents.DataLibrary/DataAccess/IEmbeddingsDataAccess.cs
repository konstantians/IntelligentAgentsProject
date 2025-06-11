using IntelligentAgents.DataLibrary.Models;
using IntelligentAgents.DataLibrary.Models.RequestModels;

namespace IntelligentAgents.DataLibrary.DataAccess;
public interface IEmbeddingsDataAccess
{
    Task<List<EmbeddingDTO>> GetEmbeddings();
    Task<List<EmbeddingDTO>> GetPaymentIdsAndEmbeddingsInfoAsync();
    Task<List<EmbeddingDTO>> GetProductIdsWithEmbeddingsInformationAsync();
    Task<List<EmbeddingDTO>> GetShippingIdsAndEmbeddingsInfoAsync();
    Task SetEmbeddings(SetEmbeddingsRequestModel setEmbeddingsRequestModel);
}