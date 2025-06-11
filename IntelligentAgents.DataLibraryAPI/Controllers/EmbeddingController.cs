using IntelligentAgents.DataLibrary.DataAccess;
using IntelligentAgents.DataLibrary.Models;
using IntelligentAgents.DataLibrary.Models.RequestModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace IntelligentAgents.DataLibraryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmbeddingController : ControllerBase
{
    private readonly IEmbeddingsDataAccess _embeddingsDataAccess;
    private readonly HttpClient embeddingMicroserviceHttpClient;

    public EmbeddingController(IEmbeddingsDataAccess embeddingsDataAccess, IHttpClientFactory httpClientFactory)
    {
        _embeddingsDataAccess = embeddingsDataAccess;
        embeddingMicroserviceHttpClient = httpClientFactory.CreateClient("EmbeddingMicroserviceApiClient");
    }

    [HttpPost]
    public async Task<IActionResult> SetEmbeddings()
    {
        try
        {
            List<EmbeddingDTO> embeddingDTOs = await _embeddingsDataAccess.GetEmbeddings();
            Dictionary<string, string> idDescriptionPairs = new Dictionary<string, string>();
            foreach (EmbeddingDTO embeddingDTO in embeddingDTOs)
                idDescriptionPairs.Add(embeddingDTO.Id!, embeddingDTO.Description!);

            var response = await embeddingMicroserviceHttpClient.PostAsJsonAsync("createEmbeddings", new { idDescriptionPairs = idDescriptionPairs });
            if (!response.IsSuccessStatusCode)
                return BadRequest(new { ErrorMessage = "Something went wrong with the Embedding Microservice. Make sure that it is running before running this endpoint" });

            string responseBody = await response.Content.ReadAsStringAsync();
            Dictionary<string, float[]> IdEmbeddingPairs = JsonSerializer.Deserialize<Dictionary<string, float[]>>(responseBody)!;

            await _embeddingsDataAccess.SetEmbeddings(new SetEmbeddingsRequestModel() { IdEmbeddingPairs = IdEmbeddingPairs });
            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetEmbeddings()
    {
        try
        {
            List<EmbeddingDTO> embeddingDTOs = await _embeddingsDataAccess.GetEmbeddings();
            return Ok(embeddingDTOs);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("GetProductEmbeddings")]
    public async Task<IActionResult> GetProductEmbeddings()
    {
        try
        {
            List<EmbeddingDTO> productEmbeddingDTOs = await _embeddingsDataAccess.GetProductIdsWithEmbeddingsInformationAsync();
            return Ok(productEmbeddingDTOs);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("GetPaymentOptionEmbeddings")]
    public async Task<IActionResult> GetShippingOptionsEmbeddings()
    {
        try
        {
            List<EmbeddingDTO> shippingOptionEmbeddingDTOs = await _embeddingsDataAccess.GetShippingIdsAndEmbeddingsInfoAsync();
            return Ok(shippingOptionEmbeddingDTOs);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("GetShippingOptionEmbeddings")]
    public async Task<IActionResult> GetPaymentOptionsEmbeddings()
    {
        try
        {
            List<EmbeddingDTO> paymentOptionEmbeddingDTOs = await _embeddingsDataAccess.GetPaymentIdsAndEmbeddingsInfoAsync();
            return Ok(paymentOptionEmbeddingDTOs);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }
}
