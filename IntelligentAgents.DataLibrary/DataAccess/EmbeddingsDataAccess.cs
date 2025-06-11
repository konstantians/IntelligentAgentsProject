using IntelligentAgents.DataLibrary.Models;
using IntelligentAgents.DataLibrary.Models.RequestModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IntelligentAgents.DataLibrary.DataAccess;
public class EmbeddingsDataAccess : IEmbeddingsDataAccess
{
    private readonly AppDataDbContext _appDataDbContext;
    private readonly ILogger<EmbeddingsDataAccess> _logger;

    public EmbeddingsDataAccess(AppDataDbContext appDataDbContext, ILogger<EmbeddingsDataAccess> logger = null!)
    {
        _appDataDbContext = appDataDbContext;
        _logger = logger ?? NullLogger<EmbeddingsDataAccess>.Instance;
    }

    public async Task SetEmbeddings(SetEmbeddingsRequestModel setEmbeddingsRequestModel)
    {
        try
        {
            List<ShippingOption> shippingOptions = await _appDataDbContext.ShippingOptions.ToListAsync();
            List<PaymentOption> paymentOptions = await _appDataDbContext.PaymentOptions.ToListAsync();
            List<Product> products = await _appDataDbContext.Products.ToListAsync();
            foreach (KeyValuePair<string, float[]> keyValuePair in setEmbeddingsRequestModel.IdEmbeddingPairs)
            {
                ShippingOption? foundShippingOption = shippingOptions.FirstOrDefault(shippingOption => shippingOption.Id == keyValuePair.Key);
                if (foundShippingOption is not null)
                {
                    foundShippingOption.Embedding = string.Join(',', keyValuePair.Value);
                    continue;
                }

                PaymentOption? foundPaymentOption = paymentOptions.FirstOrDefault(paymentOption => paymentOption.Id == keyValuePair.Key);
                if (foundPaymentOption is not null)
                {
                    foundPaymentOption.Embedding = string.Join(',', keyValuePair.Value);
                    continue;
                }

                Product? foundProduct = products.FirstOrDefault(product => product.Id == keyValuePair.Key);
                if (foundProduct is not null)
                {
                    foundProduct.Embedding = string.Join(',', keyValuePair.Value);
                    continue;
                }
            }

            await _appDataDbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetShippingIdsAndEmbeddingsInfoFailure"), ex, "An error occurred while retrieving the shipping option embedding DTOs. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<List<EmbeddingDTO>> GetEmbeddings()
    {
        try
        {
            List<EmbeddingDTO> embeddingDtos = new List<EmbeddingDTO>();
            List<Product> products = await _appDataDbContext.Products.ToListAsync();
            List<ShippingOption> shippingOptions = await _appDataDbContext.ShippingOptions.ToListAsync();
            List<PaymentOption> paymentOptions = await _appDataDbContext.PaymentOptions.ToListAsync();
            foreach (var paymentOption in paymentOptions)
            {
                EmbeddingDTO paymentOptionEmbeddingDTO = new EmbeddingDTO();
                paymentOptionEmbeddingDTO.Id = paymentOption.Id;
                paymentOptionEmbeddingDTO.Description = paymentOption.Description;
                paymentOptionEmbeddingDTO.TableItsIn = "PaymentOptions";
                paymentOptionEmbeddingDTO.Embedding = paymentOption.Embedding is not null ? paymentOption.Embedding.Split(',').Select(embedding => float.Parse(embedding)).ToArray() : [0f];

                embeddingDtos.Add(paymentOptionEmbeddingDTO);
            }

            foreach (var shippingOption in shippingOptions)
            {
                EmbeddingDTO shippingOptionEmbeddingDTO = new EmbeddingDTO();
                shippingOptionEmbeddingDTO.Id = shippingOption.Id;
                shippingOptionEmbeddingDTO.Description = shippingOption.Description;
                shippingOptionEmbeddingDTO.TableItsIn = "ShippingOptions";
                shippingOptionEmbeddingDTO.Embedding = shippingOption.Embedding is not null ? shippingOption.Embedding.Split(',').Select(embedding => float.Parse(embedding)).ToArray() : [0f];

                embeddingDtos.Add(shippingOptionEmbeddingDTO);
            }

            foreach (var product in products)
            {
                EmbeddingDTO productEmbeddingDTO = new EmbeddingDTO();
                productEmbeddingDTO.Id = product.Id;
                productEmbeddingDTO.Description = product.Description;
                productEmbeddingDTO.TableItsIn = "Products";
                productEmbeddingDTO.Embedding = product.Embedding is not null ? product.Embedding.Split(',').Select(embedding => float.Parse(embedding)).ToArray() : [0f];

                embeddingDtos.Add(productEmbeddingDTO);
            }

            return embeddingDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetPaymentIdsAndEmbeddingsInfoFailure"), ex, "An error occurred while retrieving the payment options embeddings DTOs. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<List<EmbeddingDTO>> GetPaymentIdsAndEmbeddingsInfoAsync()
    {
        try
        {
            List<PaymentOption> paymentOptions = await _appDataDbContext.PaymentOptions.ToListAsync();
            List<EmbeddingDTO> paymentOptionEmbeddingDTOs = new List<EmbeddingDTO>();
            foreach (var paymentOption in paymentOptions)
            {
                EmbeddingDTO paymentOptionEmbeddingDTO = new EmbeddingDTO();
                paymentOptionEmbeddingDTO.Id = paymentOption.Id;
                paymentOptionEmbeddingDTO.Description = paymentOption.Description;
                paymentOptionEmbeddingDTO.TableItsIn = "PaymentOptions";
                paymentOptionEmbeddingDTO.Embedding = paymentOption.Embedding is not null ? paymentOption.Embedding.Split(',').Select(embedding => float.Parse(embedding)).ToArray() : [0f];

                paymentOptionEmbeddingDTOs.Add(paymentOptionEmbeddingDTO);
            }

            return paymentOptionEmbeddingDTOs;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetPaymentIdsAndEmbeddingsInfoFailure"), ex, "An error occurred while retrieving the payment options embeddings DTOs. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<List<EmbeddingDTO>> GetShippingIdsAndEmbeddingsInfoAsync()
    {
        try
        {
            List<ShippingOption> shippingOptions = await _appDataDbContext.ShippingOptions.ToListAsync();
            List<EmbeddingDTO> shippingOptionEmbeddingDTOs = new List<EmbeddingDTO>();
            foreach (var shippingOption in shippingOptions)
            {
                EmbeddingDTO shippingOptionEmbeddingDTO = new EmbeddingDTO();
                shippingOptionEmbeddingDTO.Id = shippingOption.Id;
                shippingOptionEmbeddingDTO.Description = shippingOption.Description;
                shippingOptionEmbeddingDTO.TableItsIn = "ShippingOptions";
                shippingOptionEmbeddingDTO.Embedding = shippingOption.Embedding is not null ? shippingOption.Embedding.Split(',').Select(embedding => float.Parse(embedding)).ToArray() : [0f];

                shippingOptionEmbeddingDTOs.Add(shippingOptionEmbeddingDTO);
            }

            return shippingOptionEmbeddingDTOs;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetShippingIdsAndEmbeddingsInfoFailure"), ex, "An error occurred while retrieving the shipping option embedding DTOs. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<List<EmbeddingDTO>> GetProductIdsWithEmbeddingsInformationAsync()
    {
        try
        {
            List<Product> products = await _appDataDbContext.Products.ToListAsync();
            List<EmbeddingDTO> productEmbeddingDTOs = new List<EmbeddingDTO>();
            foreach (var product in products)
            {
                EmbeddingDTO productEmbeddingDTO = new EmbeddingDTO();
                productEmbeddingDTO.Id = product.Id;
                productEmbeddingDTO.Description = product.Description;
                productEmbeddingDTO.TableItsIn = "Products";
                productEmbeddingDTO.Embedding = product.Embedding is not null ? product.Embedding.Split(',').Select(embedding => float.Parse(embedding)).ToArray() : [0f];

                productEmbeddingDTOs.Add(productEmbeddingDTO);
            }

            return productEmbeddingDTOs;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetProductIdsWithEmbeddingsInformationFailure"), ex, "An error occurred while retrieving the product embeddings DTOs. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }
}
