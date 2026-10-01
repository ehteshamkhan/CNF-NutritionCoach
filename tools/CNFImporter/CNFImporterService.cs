using System.Globalization;
using Microsoft.Data.Sqlite;

namespace CNFImporter;

public sealed class CNFImporterService
{
    private readonly ImportConfiguration _config;

    public CNFImporterService(
        ImportConfiguration config)
    {
        _config = config;
    }

    public async Task RunAsync(
        CancellationToken cancellationToken)
    {
        PrintHeader();

        RemoveBuildingDatabase();

        Console.WriteLine();
        Console.WriteLine("[1/7] Reading canonical food records...");

        IReadOnlyList<FoodRecord> foods =
            CNFParser.ReadFoods(
                _config.CanonicalFoodCsv);

        if (foods.Count != _config.ExpectedFoods)
        {
            throw new InvalidDataException(
                $"Canonical food count mismatch. " +
                $"Expected {_config.ExpectedFoods}, actual {foods.Count}.");
        }

        ValidateUniqueFoodCodes(foods);

        Console.WriteLine(
            $"[PASS] Canonical foods: {foods.Count:N0}");

        Console.WriteLine();
        Console.WriteLine("[2/7] Enumerating source files...");

        string[] measureFiles =
            Directory.GetFiles(
                _config.MeasureRoot,
                "cnf_measure_*.txt",
                SearchOption.TopDirectoryOnly);

        string[] nutrientFiles =
            Directory.GetFiles(
                _config.NutrientRoot,
                "cnf_nutrient_*.txt",
                SearchOption.TopDirectoryOnly);

        if (measureFiles.Length != 188)
        {
            throw new InvalidDataException(
                $"Expected 188 measure files, found {measureFiles.Length}.");
        }

        if (nutrientFiles.Length != 4897)
        {
            throw new InvalidDataException(
                $"Expected 4897 nutrient files, found {nutrientFiles.Length}.");
        }

        Console.WriteLine(
            $"[PASS] Measure files: {measureFiles.Length:N0}");

        Console.WriteLine(
            $"[PASS] Nutrient files: {nutrientFiles.Length:N0}");

        Console.WriteLine();
        Console.WriteLine("[3/7] Pre-validating source records...");

        IReadOnlyList<MeasureRecord> measures =
            CNFParser.ReadMeasures(measureFiles)
                .ToList();

        if (measures.Count != _config.ExpectedMeasures)
        {
            throw new InvalidDataException(
                $"Measure record count mismatch. " +
                $"Expected {_config.ExpectedMeasures}, actual {measures.Count}.");
        }

        ValidateMeasures(
            foods,
            measures);

        Console.WriteLine(
            $"[PASS] Measure records: {measures.Count:N0}");

        IReadOnlyList<MeasureTypeRecord> measureTypes =
            measures
                .GroupBy(x => x.MeasureTypeCode)
                .Select(
                    group =>
                    {
                        var first = group.First();

                        if (group.Any(
                                x =>
                                    !string.Equals(
                                        x.MeasureType,
                                        first.MeasureType,
                                        StringComparison.Ordinal)))
                        {
                            throw new InvalidDataException(
                                $"Measure type code {first.MeasureTypeCode} has conflicting names.");
                        }

                        return new MeasureTypeRecord(
                            first.MeasureTypeCode,
                            first.MeasureType);
                    })
                .OrderBy(x => x.MeasureTypeCode)
                .ToList();

        if (measureTypes.Count != _config.ExpectedMeasureTypes)
        {
            throw new InvalidDataException(
                $"Measure type count mismatch. " +
                $"Expected {_config.ExpectedMeasureTypes}, actual {measureTypes.Count}.");
        }

        Console.WriteLine(
            $"[PASS] Measure types: {measureTypes.Count:N0}");

        Console.WriteLine();
        Console.WriteLine("[4/7] Pre-validating nutrient records...");

        IReadOnlyList<NutrientRecord> nutrients =
            CNFParser.ReadNutrients(nutrientFiles)
                .ToList();

        if (nutrients.Count != _config.ExpectedNutrients)
        {
            throw new InvalidDataException(
                $"Nutrient record count mismatch. " +
                $"Expected {_config.ExpectedNutrients}, actual {nutrients.Count}.");
        }

        ValidateNutrients(
            foods,
            nutrients);

        Console.WriteLine(
            $"[PASS] Nutrient records: {nutrients.Count:N0}");

        Console.WriteLine();
        Console.WriteLine("[5/7] Creating SQLite building database...");

        await CreateSchemaAsync(
            cancellationToken);

        Console.WriteLine(
            "[PASS] SQLite schema created.");

        Console.WriteLine();
        Console.WriteLine("[6/7] Importing records...");

        await ImportFoodsAsync(
            foods,
            cancellationToken);

        await ImportMeasureTypesAsync(
            measureTypes,
            cancellationToken);

        await ImportMeasuresAsync(
            measures,
            cancellationToken);

        await ImportNutrientsAsync(
            nutrients,
            cancellationToken);

        Console.WriteLine();
        Console.WriteLine("[PASS] All source records inserted.");

        Console.WriteLine();
        Console.WriteLine("[7/7] Validating SQLite database...");

        var validator =
            new DatabaseValidator(_config);

        await using (
            var validationConnection =
                OpenConnection())
        {
            await validationConnection.OpenAsync(
                cancellationToken);

            await validator.ValidateAsync(
                validationConnection,
                cancellationToken);
        }

        Console.WriteLine();
        Console.WriteLine("[PASS] Building database fully validated.");

        PromoteBuildingDatabase();

        Console.WriteLine();
        Console.WriteLine("[PASS] Building database promoted to final database.");
        Console.WriteLine();
        Console.WriteLine($"Final database: {_config.FinalDatabase}");
    }

    private async Task CreateSchemaAsync(
        CancellationToken cancellationToken)
    {
        await using var connection =
            OpenConnection();

        await connection.OpenAsync(
            cancellationToken);

        string schemaPath =
            _config.SchemaFile;

        if (!File.Exists(schemaPath))
        {
            throw new FileNotFoundException(
                "schema.sql was not found.",
                schemaPath);
        }

        string sql =
            await File.ReadAllTextAsync(
                schemaPath,
                cancellationToken);

        await using var command =
            connection.CreateCommand();

        command.CommandText = sql;

        await command.ExecuteNonQueryAsync(
            cancellationToken);
    }

    private async Task ImportFoodsAsync(
        IReadOnlyList<FoodRecord> foods,
        CancellationToken cancellationToken)
    {
        await using var connection =
            OpenConnection();

        await connection.OpenAsync(
            cancellationToken);

	await using var transaction =
    		(SqliteTransaction)(SqliteTransaction)await connection.BeginTransactionAsync(
        cancellationToken);

        await using var command =
            connection.CreateCommand();

        command.Transaction =
            transaction;

        command.CommandText =
            """
            INSERT INTO Foods
            (
                FoodCode,
                FoodName
            )
            VALUES
            (
                $foodCode,
                $foodName
            );
            """;

        var foodCodeParameter =
            command.Parameters.Add(
                "$foodCode",
                SqliteType.Integer);

        var foodNameParameter =
            command.Parameters.Add(
                "$foodName",
                SqliteType.Text);

        foreach (FoodRecord food in foods)
        {
            foodCodeParameter.Value =
                food.FoodCode;

            foodNameParameter.Value =
                food.FoodName;

            await command.ExecuteNonQueryAsync(
                cancellationToken);
        }

        await transaction.CommitAsync(
            cancellationToken);

        Console.WriteLine(
            $"[PASS] Foods inserted: {foods.Count:N0}");
    }

    private async Task ImportMeasureTypesAsync(
        IReadOnlyList<MeasureTypeRecord> measureTypes,
        CancellationToken cancellationToken)
    {
        await using var connection =
            OpenConnection();

        await connection.OpenAsync(
            cancellationToken);

        await using var transaction =
              (SqliteTransaction)(SqliteTransaction)await connection.BeginTransactionAsync(
                cancellationToken);

        await using var command =
            connection.CreateCommand();

        command.Transaction =
            transaction;

        command.CommandText =
            """
            INSERT INTO MeasureTypes
            (
                MeasureTypeCode,
                MeasureType
            )
            VALUES
            (
                $code,
                $name
            );
            """;

        var codeParameter =
            command.Parameters.Add(
                "$code",
                SqliteType.Integer);

        var nameParameter =
            command.Parameters.Add(
                "$name",
                SqliteType.Text);

        foreach (MeasureTypeRecord item in measureTypes)
        {
            codeParameter.Value =
                item.MeasureTypeCode;

            nameParameter.Value =
                item.MeasureType;

            await command.ExecuteNonQueryAsync(
                cancellationToken);
        }

        await transaction.CommitAsync(
            cancellationToken);

        Console.WriteLine(
            $"[PASS] Measure types inserted: {measureTypes.Count:N0}");
    }

    private async Task ImportMeasuresAsync(
        IReadOnlyList<MeasureRecord> measures,
        CancellationToken cancellationToken)
    {
        await using var connection =
            OpenConnection();

        await connection.OpenAsync(
            cancellationToken);

        await using var transaction =
            (SqliteTransaction)await connection.BeginTransactionAsync(
                cancellationToken);

        await using var command =
            connection.CreateCommand();

        command.Transaction =
            transaction;

        command.CommandText =
            """
            INSERT INTO Measures
            (
                FoodCode,
                MeasureCode,
                Measure,
                MeasureTypeCode,
                MeasureType,
                WeightGrams
            )
            VALUES
            (
                $foodCode,
                $measureCode,
                $measure,
                $measureTypeCode,
                $measureType,
                $weightGrams
            );
            """;

        var foodCodeParameter =
            command.Parameters.Add(
                "$foodCode",
                SqliteType.Integer);

        var measureCodeParameter =
            command.Parameters.Add(
                "$measureCode",
                SqliteType.Integer);

        var measureParameter =
            command.Parameters.Add(
                "$measure",
                SqliteType.Text);

        var measureTypeCodeParameter =
            command.Parameters.Add(
                "$measureTypeCode",
                SqliteType.Integer);

        var measureTypeParameter =
            command.Parameters.Add(
                "$measureType",
                SqliteType.Text);

        var weightParameter =
            command.Parameters.Add(
                "$weightGrams",
                SqliteType.Text);

        foreach (MeasureRecord item in measures)
        {
            foodCodeParameter.Value =
                item.FoodCode;

            measureCodeParameter.Value =
                item.MeasureCode;

            measureParameter.Value =
                item.Measure;

            measureTypeCodeParameter.Value =
                item.MeasureTypeCode;

            measureTypeParameter.Value =
                item.MeasureType;

            weightParameter.Value =
                item.WeightGrams.ToString(
                    "G29",
                    CultureInfo.InvariantCulture);

            await command.ExecuteNonQueryAsync(
                cancellationToken);
        }

        await transaction.CommitAsync(
            cancellationToken);

        Console.WriteLine(
            $"[PASS] Measures inserted: {measures.Count:N0}");
    }

    private async Task ImportNutrientsAsync(
        IReadOnlyList<NutrientRecord> nutrients,
        CancellationToken cancellationToken)
    {
        int total =
            nutrients.Count;

        for (
            int offset = 0;
            offset < total;
            offset += _config.NutrientBatchSize)
        {
            int count =
                Math.Min(
                    _config.NutrientBatchSize,
                    total - offset);

            await using var connection =
                OpenConnection();

            await connection.OpenAsync(
                cancellationToken);

            await using var transaction =
                (SqliteTransaction)await connection.BeginTransactionAsync(
                    cancellationToken);

            await using var command =
                connection.CreateCommand();

            command.Transaction =
                transaction;

            command.CommandText =
                """
                INSERT INTO Nutrients
                (
                    FoodCode,
                    NutrientCode,
                    NutrientName,
                    NutrientSymbol,
                    NutrientUnit,
                    AmountPer100g,
                    StandardError,
                    Observations,
                    NutrientSourceCode,
                    NutrientLastUpdatedDate
                )
                VALUES
                (
                    $foodCode,
                    $nutrientCode,
                    $nutrientName,
                    $nutrientSymbol,
                    $nutrientUnit,
                    $amount,
                    $standardError,
                    $observations,
                    $sourceCode,
                    $lastUpdated
                );
                """;

            var foodCodeParameter =
                command.Parameters.Add(
                    "$foodCode",
                    SqliteType.Integer);

            var nutrientCodeParameter =
                command.Parameters.Add(
                    "$nutrientCode",
                    SqliteType.Integer);

            var nutrientNameParameter =
                command.Parameters.Add(
                    "$nutrientName",
                    SqliteType.Text);

            var nutrientSymbolParameter =
                command.Parameters.Add(
                    "$nutrientSymbol",
                    SqliteType.Text);

            var nutrientUnitParameter =
                command.Parameters.Add(
                    "$nutrientUnit",
                    SqliteType.Text);

            var amountParameter =
                command.Parameters.Add(
                    "$amount",
                    SqliteType.Text);

            var standardErrorParameter =
                command.Parameters.Add(
                    "$standardError",
                    SqliteType.Text);

            var observationsParameter =
                command.Parameters.Add(
                    "$observations",
                    SqliteType.Text);

            var sourceCodeParameter =
                command.Parameters.Add(
                    "$sourceCode",
                    SqliteType.Integer);

            var lastUpdatedParameter =
                command.Parameters.Add(
                    "$lastUpdated",
                    SqliteType.Text);

            for (int i = offset;
                 i < offset + count;
                 i++)
            {
                NutrientRecord item =
                    nutrients[i];

                foodCodeParameter.Value =
                    item.FoodCode;

                nutrientCodeParameter.Value =
                    item.NutrientCode;

                nutrientNameParameter.Value =
                    item.NutrientName;

                nutrientSymbolParameter.Value =
                    item.NutrientSymbol;

                nutrientUnitParameter.Value =
                    item.NutrientUnit;

                amountParameter.Value =
                    item.AmountPer100g.ToString(
                        "G29",
                        CultureInfo.InvariantCulture);

                standardErrorParameter.Value =
                    item.StandardError.HasValue
                        ? item.StandardError.Value.ToString(
                            "G29",
                            CultureInfo.InvariantCulture)
                        : DBNull.Value;

                observationsParameter.Value =
                    item.Observations.HasValue
                        ? item.Observations.Value.ToString(
                            "G29",
                            CultureInfo.InvariantCulture)
                        : DBNull.Value;

                sourceCodeParameter.Value =
                    item.NutrientSourceCode;

                lastUpdatedParameter.Value =
                    item.NutrientLastUpdatedDate.ToString(
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture);

                await command.ExecuteNonQueryAsync(
                    cancellationToken);
            }

            await transaction.CommitAsync(
                cancellationToken);

            int completed =
                offset + count;

            double percentage =
                completed * 100.0 / total;

            Console.WriteLine(
                $"[PASS] Nutrients batch: {completed:N0}/{total:N0} ({percentage:F1}%)");
        }
    }

    private SqliteConnection OpenConnection()
    {
        var builder =
            new SqliteConnectionStringBuilder
            {
                DataSource = _config.BuildingDatabase,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Default,
                Pooling = false
            };

        var connection =
            new SqliteConnection(
                builder.ToString());

        connection.DefaultTimeout =
            120;

        return connection;
    }

    private void PromoteBuildingDatabase()
    {
        if (!File.Exists(_config.BuildingDatabase))
        {
            throw new FileNotFoundException(
                "Building database does not exist.",
                _config.BuildingDatabase);
        }

        string? finalDirectory =
            Path.GetDirectoryName(
                _config.FinalDatabase);

        if (!string.IsNullOrWhiteSpace(finalDirectory) &&
            !Directory.Exists(finalDirectory))
        {
            Directory.CreateDirectory(
                finalDirectory);
        }

        if (File.Exists(_config.FinalDatabase))
        {
            string timestamp =
                DateTime.Now.ToString(
                    "yyyyMMdd-HHmmss",
                    CultureInfo.InvariantCulture);

            string backup =
                $"{_config.FinalDatabase}.{timestamp}.bak";

            File.Move(
                _config.FinalDatabase,
                backup);

            Console.WriteLine(
                $"[INFO] Existing database backed up: {backup}");
        }

        File.Move(
            _config.BuildingDatabase,
            _config.FinalDatabase);
    }

    private void RemoveBuildingDatabase()
    {
        foreach (
            string suffix in new[]
            {
                "",
                "-wal",
                "-shm"
            })
        {
            string path =
                _config.BuildingDatabase + suffix;

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        Console.WriteLine(
            "[INFO] Temporary building database cleared.");
    }

    private static void ValidateUniqueFoodCodes(
        IReadOnlyList<FoodRecord> foods)
    {
        int duplicates =
            foods
                .GroupBy(x => x.FoodCode)
                .Count(x => x.Count() > 1);

        if (duplicates != 0)
        {
            throw new InvalidDataException(
                $"Duplicate Food Codes detected: {duplicates}");
        }
    }

    private static void ValidateMeasures(
        IReadOnlyList<FoodRecord> foods,
        IReadOnlyList<MeasureRecord> measures)
    {
        var foodCodes =
            foods
                .Select(x => x.FoodCode)
                .ToHashSet();

        var duplicateKeys =
            measures
                .GroupBy(
                    x => new
                    {
                        x.FoodCode,
                        x.MeasureCode
                    })
                .Where(x => x.Count() > 1)
                .ToList();

        if (duplicateKeys.Count != 0)
        {
            throw new InvalidDataException(
                $"Duplicate measure natural keys detected: {duplicateKeys.Count}");
        }

        foreach (MeasureRecord measure in measures)
        {
            if (!foodCodes.Contains(
                    measure.FoodCode))
            {
                throw new InvalidDataException(
                    $"Measure references unknown Food Code {measure.FoodCode}.");
            }

            if (measure.WeightGrams < 0)
            {
                throw new InvalidDataException(
                    $"Negative measure weight for Food Code {measure.FoodCode}, " +
                    $"Measure Code {measure.MeasureCode}.");
            }
        }
    }

    private static void ValidateNutrients(
        IReadOnlyList<FoodRecord> foods,
        IReadOnlyList<NutrientRecord> nutrients)
    {
        var foodCodes =
            foods
                .Select(x => x.FoodCode)
                .ToHashSet();

        var duplicateKeys =
            nutrients
                .GroupBy(
                    x => new
                    {
                        x.FoodCode,
                        x.NutrientCode
                    })
                .Where(x => x.Count() > 1)
                .ToList();

        if (duplicateKeys.Count != 0)
        {
            throw new InvalidDataException(
                $"Duplicate nutrient natural keys detected: {duplicateKeys.Count}");
        }

        foreach (NutrientRecord nutrient in nutrients)
        {
            if (!foodCodes.Contains(
                    nutrient.FoodCode))
            {
                throw new InvalidDataException(
                    $"Nutrient references unknown Food Code {nutrient.FoodCode}.");
            }

            if (nutrient.StandardError.HasValue &&
                nutrient.StandardError.Value < 0)
            {
                throw new InvalidDataException(
                    $"Negative Standard Error for Food Code {nutrient.FoodCode}, " +
                    $"Nutrient Code {nutrient.NutrientCode}.");
            }

            if (nutrient.Observations.HasValue &&
                nutrient.Observations.Value < 0)
            {
                throw new InvalidDataException(
                    $"Negative Observations for Food Code {nutrient.FoodCode}, " +
                    $"Nutrient Code {nutrient.NutrientCode}.");
            }
        }
    }

    private static void PrintHeader()
    {
        Console.WriteLine();
        Console.WriteLine("============================================================");
        Console.WriteLine(" CNF NUTRITIONCOACH - SQLITE IMPORTER");
        Console.WriteLine("============================================================");
    }
}