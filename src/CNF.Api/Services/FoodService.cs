using System.Globalization;
using CNF.Api.Data;
using CNF.Api.Models.Responses;
using Microsoft.Data.Sqlite;

namespace CNF.Api.Services;

public sealed class FoodService
{
    private readonly CnfDatabase _database;

    public FoodService(CnfDatabase database)
    {
        _database = database;
    }

    public async Task<IReadOnlyList<FoodSearchResult>> SearchFoodsAsync(
        string query,
        int limit,
        CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT
                FoodCode,
                FoodName
            FROM Foods
            WHERE FoodName LIKE '%' || @Query || '%' COLLATE NOCASE
            ORDER BY FoodName, FoodCode
            LIMIT @Limit;
            """;

        await using SqliteConnection connection =
            _database.CreateConnection();

        await connection.OpenAsync(
            cancellationToken);

        await using SqliteCommand command =
            connection.CreateCommand();

        command.CommandText = sql;

        command.Parameters.Add(
            new SqliteParameter(
                "@Query",
                query));

        command.Parameters.Add(
            new SqliteParameter(
                "@Limit",
                limit));

        var results =
            new List<FoodSearchResult>();

        await using SqliteDataReader reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        while (await reader.ReadAsync(
            cancellationToken))
        {
            results.Add(
                new FoodSearchResult
                {
                    FoodCode =
                        reader.GetInt32(0),

                    FoodName =
                        reader.GetString(1)
                });
        }

        return results;
    }

    public async Task<FoodResponse?> GetFoodAsync(
        int foodCode,
        CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT
                FoodCode,
                FoodName
            FROM Foods
            WHERE FoodCode = @FoodCode;
            """;

        await using SqliteConnection connection =
            _database.CreateConnection();

        await connection.OpenAsync(
            cancellationToken);

        await using SqliteCommand command =
            connection.CreateCommand();

        command.CommandText = sql;

        command.Parameters.Add(
            new SqliteParameter(
                "@FoodCode",
                foodCode));

        await using SqliteDataReader reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        if (!await reader.ReadAsync(
            cancellationToken))
        {
            return null;
        }

        return new FoodResponse
        {
            FoodCode =
                reader.GetInt32(0),

            FoodName =
                reader.GetString(1)
        };
    }

    public async Task<FoodNutrientsResponse?> GetNutrientsAsync(
        int foodCode,
        CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT
                f.FoodCode,
                f.FoodName,
                n.NutrientCode,
                n.NutrientName,
                n.NutrientSymbol,
                n.NutrientUnit,
                n.AmountPer100g,
                n.StandardError,
                n.Observations,
                n.NutrientSourceCode,
                n.NutrientLastUpdatedDate
            FROM Foods f
            INNER JOIN Nutrients n
                ON n.FoodCode = f.FoodCode
            WHERE f.FoodCode = @FoodCode
            ORDER BY n.NutrientCode;
            """;

        await using SqliteConnection connection =
            _database.CreateConnection();

        await connection.OpenAsync(
            cancellationToken);

        await using SqliteCommand command =
            connection.CreateCommand();

        command.CommandText = sql;

        command.Parameters.Add(
            new SqliteParameter(
                "@FoodCode",
                foodCode));

        await using SqliteDataReader reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        FoodNutrientsResponse? response = null;

        var nutrients =
            new List<NutrientResponse>();

        while (await reader.ReadAsync(
            cancellationToken))
        {
            response ??=
                new FoodNutrientsResponse
                {
                    FoodCode =
                        reader.GetInt32(0),

                    FoodName =
                        reader.GetString(1)
                };

            nutrients.Add(
                new NutrientResponse
                {
                    NutrientCode =
                        reader.GetInt32(2),

                    NutrientName =
                        reader.GetString(3),

                    NutrientSymbol =
                        reader.GetString(4),

                    NutrientUnit =
                        reader.GetString(5),

                    AmountPer100g =
                        GetDecimal(reader, 6),

                    StandardError =
                        GetNullableDecimal(reader, 7),

                    Observations =
                        GetNullableDecimal(reader, 8),

                    NutrientSourceCode =
                        reader.GetInt32(9),

                    NutrientLastUpdatedDate =
                        reader.IsDBNull(10)
                            ? null
                            : reader.GetString(10)
                });
        }

        if (response is null)
        {
            return null;
        }

        return new FoodNutrientsResponse
        {
            FoodCode =
                response.FoodCode,

            FoodName =
                response.FoodName,

            Nutrients =
                nutrients
        };
    }

    public async Task<FoodMeasuresResponse?> GetMeasuresAsync(
        int foodCode,
        CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT
                f.FoodCode,
                f.FoodName,
                m.MeasureCode,
                m.Measure,
                m.MeasureTypeCode,
                m.MeasureType,
                m.WeightGrams
            FROM Foods f
            INNER JOIN Measures m
                ON m.FoodCode = f.FoodCode
            WHERE f.FoodCode = @FoodCode
            ORDER BY m.MeasureCode;
            """;

        await using SqliteConnection connection =
            _database.CreateConnection();

        await connection.OpenAsync(
            cancellationToken);

        await using SqliteCommand command =
            connection.CreateCommand();

        command.CommandText = sql;

        command.Parameters.Add(
            new SqliteParameter(
                "@FoodCode",
                foodCode));

        await using SqliteDataReader reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        FoodMeasuresResponse? response = null;

        var measures =
            new List<MeasureResponse>();

        while (await reader.ReadAsync(
            cancellationToken))
        {
            response ??=
                new FoodMeasuresResponse
                {
                    FoodCode =
                        reader.GetInt32(0),

                    FoodName =
                        reader.GetString(1)
                };

            measures.Add(
                new MeasureResponse
                {
                    MeasureCode =
                        reader.GetInt32(2),

                    Measure =
                        reader.GetString(3),

                    MeasureTypeCode =
                        reader.GetInt32(4),

                    MeasureType =
                        reader.GetString(5),

                    WeightGrams =
                        GetDecimal(reader, 6)
                });
        }

        if (response is null)
        {
            return null;
        }

        return new FoodMeasuresResponse
        {
            FoodCode =
                response.FoodCode,

            FoodName =
                response.FoodName,

            Measures =
                measures
        };
    }

    public async Task<MeasureResponse?> GetMeasureAsync(
        int foodCode,
        int measureCode,
        CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT
                MeasureCode,
                Measure,
                MeasureTypeCode,
                MeasureType,
                WeightGrams
            FROM Measures
            WHERE FoodCode = @FoodCode
              AND MeasureCode = @MeasureCode;
            """;

        await using SqliteConnection connection =
            _database.CreateConnection();

        await connection.OpenAsync(
            cancellationToken);

        await using SqliteCommand command =
            connection.CreateCommand();

        command.CommandText = sql;

        command.Parameters.Add(
            new SqliteParameter(
                "@FoodCode",
                foodCode));

        command.Parameters.Add(
            new SqliteParameter(
                "@MeasureCode",
                measureCode));

        await using SqliteDataReader reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        if (!await reader.ReadAsync(
            cancellationToken))
        {
            return null;
        }

        return new MeasureResponse
        {
            MeasureCode =
                reader.GetInt32(0),

            Measure =
                reader.GetString(1),

            MeasureTypeCode =
                reader.GetInt32(2),

            MeasureType =
                reader.GetString(3),

            WeightGrams =
                GetDecimal(reader, 4)
        };
    }

    private static decimal GetDecimal(
        SqliteDataReader reader,
        int ordinal)
    {
        object value =
            reader.GetValue(ordinal);

        return Convert.ToDecimal(
            value,
            CultureInfo.InvariantCulture);
    }

    private static decimal? GetNullableDecimal(
        SqliteDataReader reader,
        int ordinal)
    {
        if (reader.IsDBNull(ordinal))
        {
            return null;
        }

        return GetDecimal(
            reader,
            ordinal);
    }
}
