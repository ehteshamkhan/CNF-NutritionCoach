using CNF.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace CNF.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    private readonly CnfDatabase _database;

    public HealthController(CnfDatabase database)
    {
        _database = database;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        CancellationToken cancellationToken)
    {
        await using SqliteConnection connection =
            _database.CreateConnection();

        await connection.OpenAsync(
            cancellationToken);

        int foods =
            await GetCountAsync(
                connection,
                "SELECT COUNT(*) FROM Foods;",
                cancellationToken);

        int measures =
            await GetCountAsync(
                connection,
                "SELECT COUNT(*) FROM Measures;",
                cancellationToken);

        int nutrients =
            await GetCountAsync(
                connection,
                "SELECT COUNT(*) FROM Nutrients;",
                cancellationToken);

        return Ok(
            new
            {
                status = "healthy",
                database =
                    Path.GetFileName(
                        _database.DatabasePath),
                foods,
                measures,
                nutrients
            });
    }

    private static async Task<int> GetCountAsync(
        SqliteConnection connection,
        string sql,
        CancellationToken cancellationToken)
    {
        await using SqliteCommand command =
            connection.CreateCommand();

        command.CommandText = sql;

        object? value =
            await command.ExecuteScalarAsync(
                cancellationToken);

        return Convert.ToInt32(value);
    }
}
