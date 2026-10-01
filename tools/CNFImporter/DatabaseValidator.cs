using Microsoft.Data.Sqlite;

namespace CNFImporter;

public sealed class DatabaseValidator
{
    private readonly ImportConfiguration _config;

    public DatabaseValidator(
        ImportConfiguration config)
    {
        _config = config;
    }

    public async Task ValidateAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        Console.WriteLine();
        Console.WriteLine("============================================================");
        Console.WriteLine(" DATABASE VALIDATION");
        Console.WriteLine("============================================================");

        await CheckCountAsync(
            connection,
            "Foods",
            _config.ExpectedFoods,
            cancellationToken);

        await CheckCountAsync(
            connection,
            "MeasureTypes",
            _config.ExpectedMeasureTypes,
            cancellationToken);

        await CheckCountAsync(
            connection,
            "Measures",
            _config.ExpectedMeasures,
            cancellationToken);

        await CheckCountAsync(
            connection,
            "Nutrients",
            _config.ExpectedNutrients,
            cancellationToken);

        await CheckZeroAsync(
            connection,
            """
            SELECT COUNT(*)
            FROM Measures m
            LEFT JOIN Foods f
                ON f.FoodCode = m.FoodCode
            WHERE f.FoodCode IS NULL
            """,
            "Orphan measures",
            cancellationToken);

        await CheckZeroAsync(
            connection,
            """
            SELECT COUNT(*)
            FROM Nutrients n
            LEFT JOIN Foods f
                ON f.FoodCode = n.FoodCode
            WHERE f.FoodCode IS NULL
            """,
            "Orphan nutrients",
            cancellationToken);

        await CheckZeroAsync(
            connection,
            """
            SELECT COUNT(*)
            FROM Measures m
            LEFT JOIN MeasureTypes mt
                ON mt.MeasureTypeCode = m.MeasureTypeCode
            WHERE mt.MeasureTypeCode IS NULL
            """,
            "Orphan measure types",
            cancellationToken);

        await CheckZeroAsync(
            connection,
            """
            SELECT COUNT(*)
            FROM
            (
                SELECT FoodCode, MeasureCode
                FROM Measures
                GROUP BY FoodCode, MeasureCode
                HAVING COUNT(*) > 1
            )
            """,
            "Duplicate FoodCode + MeasureCode",
            cancellationToken);

        await CheckZeroAsync(
            connection,
            """
            SELECT COUNT(*)
            FROM
            (
                SELECT FoodCode, NutrientCode
                FROM Nutrients
                GROUP BY FoodCode, NutrientCode
                HAVING COUNT(*) > 1
            )
            """,
            "Duplicate FoodCode + NutrientCode",
            cancellationToken);

        await CheckZeroAsync(
            connection,
            """
            SELECT COUNT(*)
            FROM Foods
            WHERE trim(FoodName) = ''
            """,
            "Blank food names",
            cancellationToken);

        await CheckZeroAsync(
            connection,
            """
            SELECT COUNT(*)
            FROM Measures
            WHERE trim(Measure) = ''
            """,
            "Blank measure names",
            cancellationToken);

        await CheckZeroAsync(
            connection,
            """
            SELECT COUNT(*)
            FROM Nutrients
            WHERE trim(NutrientName) = ''
               OR trim(NutrientSymbol) = ''
               OR trim(NutrientUnit) = ''
            """,
            "Blank nutrient required fields",
            cancellationToken);

        await CheckZeroAsync(
            connection,
            """
            SELECT COUNT(*)
            FROM Measures
            WHERE WeightGrams < 0
            """,
            "Negative measure weights",
            cancellationToken);

        await CheckZeroAsync(
            connection,
            """
            SELECT COUNT(*)
            FROM Nutrients
            WHERE StandardError < 0
               OR Observations < 0
            """,
            "Negative nullable numeric values",
            cancellationToken);

        await CheckForeignKeysAsync(
            connection,
            cancellationToken);

        await CheckIntegrityAsync(
            connection,
            cancellationToken);

        Console.WriteLine();
        Console.WriteLine("[PASS] Database validation completed.");
    }

    private static async Task CheckCountAsync(
        SqliteConnection connection,
        string table,
        int expected,
        CancellationToken cancellationToken)
    {
        await using var command =
            connection.CreateCommand();

        command.CommandText =
            $"SELECT COUNT(*) FROM {table};";

        object? value =
            await command.ExecuteScalarAsync(
                cancellationToken);

        long actual =
            Convert.ToInt64(value);

        if (actual != expected)
        {
            throw new InvalidDataException(
                $"{table} count mismatch. Expected {expected}, actual {actual}.");
        }

        Console.WriteLine(
            $"[PASS] {table,-15} {actual:N0} / {expected:N0}");
    }

    private static async Task CheckZeroAsync(
        SqliteConnection connection,
        string sql,
        string name,
        CancellationToken cancellationToken)
    {
        await using var command =
            connection.CreateCommand();

        command.CommandText = sql;

        object? value =
            await command.ExecuteScalarAsync(
                cancellationToken);

        long count =
            Convert.ToInt64(value);

        if (count != 0)
        {
            throw new InvalidDataException(
                $"{name} validation failed. Count = {count}.");
        }

        Console.WriteLine(
            $"[PASS] {name}: 0");
    }

    private static async Task CheckForeignKeysAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command =
            connection.CreateCommand();

        command.CommandText =
            "PRAGMA foreign_key_check;";

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidDataException(
                "PRAGMA foreign_key_check reported a violation.");
        }

        Console.WriteLine(
            "[PASS] PRAGMA foreign_key_check");
    }

    private static async Task CheckIntegrityAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command =
            connection.CreateCommand();

        command.CommandText =
            "PRAGMA integrity_check;";

        object? result =
            await command.ExecuteScalarAsync(
                cancellationToken);

        string value =
            Convert.ToString(result) ?? string.Empty;

        if (!string.Equals(
                value,
                "ok",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                $"SQLite integrity_check failed: {value}");
        }

        Console.WriteLine(
            "[PASS] PRAGMA integrity_check = ok");
    }
}