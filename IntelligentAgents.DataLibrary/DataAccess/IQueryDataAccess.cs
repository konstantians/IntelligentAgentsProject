
namespace IntelligentAgents.DataLibrary.DataAccess;

public interface IQueryDataAccess
{
    Task<string> ExecuteSelectQueryAsync(string sqlQuery);
}