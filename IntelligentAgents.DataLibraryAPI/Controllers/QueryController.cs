using IntelligentAgents.DataLibrary.DataAccess;
using IntelligentAgents.DataLibraryAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace IntelligentAgents.DataLibraryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QueryController : ControllerBase
{
    private readonly IQueryDataAccess _queryDataAccess;

    public QueryController(IQueryDataAccess queryDataAccess)
    {
        _queryDataAccess = queryDataAccess;
    }

    [HttpPost]
    public async Task<IActionResult> ExecuteSelectQuery(ExecuteSelectQueryModel executeSelectQueryModel)
    {
        try
        {
            string returnedJson = await _queryDataAccess.ExecuteSelectQueryAsync(executeSelectQueryModel.SqlQuery!);
            return Ok(returnedJson);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }
}
