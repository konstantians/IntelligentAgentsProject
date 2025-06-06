using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;

namespace IntelligentAgents.DataLibrary.DataAccess;
public class QueryDataAccess : IQueryDataAccess
{
    private readonly AppDataDbContext _appDataDbContext;
    private readonly ILogger<QueryDataAccess> _logger;

    public QueryDataAccess(AppDataDbContext appDataDbContext, ILogger<QueryDataAccess> logger = null!)
    {
        _appDataDbContext = appDataDbContext;
        _logger = logger ?? NullLogger<QueryDataAccess>.Instance;
    }

    public async Task<string> ExecuteSelectQueryAsync(string sqlQuery)
    {
        try
        {
            using var connection = _appDataDbContext.Database.GetDbConnection();
            await connection.OpenAsync();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = sqlQuery;

            using var reader = await cmd.ExecuteReaderAsync();

            var results = new List<Dictionary<string, object>>();

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var colName = reader.GetName(i);
                    var value = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
                    row[colName] = value!;
                }

                results.Add(row);
            }

            string json = JsonSerializer.Serialize(results);

            return json;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "ExecuteSelectQueryFailure"), ex, "An error occurred while executing the query. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

}
