using Microsoft.Data.Sqlite;

namespace CNF.Api.Data;

public sealed class CnfDatabase
{
    private readonly string _databasePath;

    public CnfDatabase(
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        string? configuredConnectionString =
            configuration.GetConnectionString("CnfDatabase");

        string? configuredDataSource = null;

        if (!string.IsNullOrWhiteSpace(
            configuredConnectionString))
        {
            try
            {
                var configuredBuilder =
                    new SqliteConnectionStringBuilder(
                        configuredConnectionString);

                configuredDataSource =
                    configuredBuilder.DataSource;
            }
            catch
            {
                configuredDataSource = null;
            }
        }

        if (!string.IsNullOrWhiteSpace(configuredDataSource))
        {
            _databasePath =
                Path.GetFullPath(
                    Path.IsPathRooted(configuredDataSource)
                        ? configuredDataSource
                        : Path.Combine(
                            environment.ContentRootPath,
                            configuredDataSource));
        }
        else
        {
            _databasePath =
                Path.GetFullPath(
                    Path.Combine(
                        environment.ContentRootPath,
                        "..",
                        "..",
                        "data",
                        "cnf.db"));
        }

        if (!File.Exists(_databasePath))
        {
            throw new FileNotFoundException(
                "The CNF SQLite database was not found.",
                _databasePath);
        }
    }

    public string DatabasePath =>
        _databasePath;

    public SqliteConnection CreateConnection()
    {
        var builder =
            new SqliteConnectionStringBuilder
            {
                DataSource = _databasePath,
                Mode = SqliteOpenMode.ReadOnly,
                Cache = SqliteCacheMode.Shared,
                Pooling = true
            };

        return new SqliteConnection(
            builder.ToString());
    }
}
