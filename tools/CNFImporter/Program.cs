using CNFImporter;

var config =
    new ImportConfiguration
    {
        CnfRoot =
            @"C:\CNF",

        ValidatedRoot =
            @"C:\CNF\CNF_RETRIEVAL_VERIFIED",

        CanonicalFoodCsv =
            @"C:\CNF\food_name.csv",

        MeasureRoot =
            @"C:\CNF\CNF_RETRIEVAL_VERIFIED\Measures",

        NutrientRoot =
            @"C:\CNF\CNF_RETRIEVAL_VERIFIED\Nutrients",

        BuildingDatabase =
            @"C:\CNF-NutritionCoach\data\cnf.building.db",

        FinalDatabase =
            @"C:\CNF-NutritionCoach\data\cnf.db",

        SchemaFile =
            Path.Combine(
                AppContext.BaseDirectory,
                "Sql",
                "schema.sql"),

        ValidationSqlFile =
            Path.Combine(
                AppContext.BaseDirectory,
                "Sql",
                "validate.sql"),

        ExpectedFoods =
            5993,

        ExpectedMeasureTypes =
            3,

        ExpectedMeasures =
            29868,

        ExpectedNutrients =
            565409,

        NutrientBatchSize =
            10000
    };

using var cancellationTokenSource =
    new CancellationTokenSource();

Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationTokenSource.Cancel();
};

try
{
    var service =
        new CNFImporterService(config);

    await service.RunAsync(
        cancellationTokenSource.Token);

    Console.WriteLine();
    Console.WriteLine("============================================================");
    Console.WriteLine(" IMPORT RESULT: SUCCESS");
    Console.WriteLine("============================================================");
    Console.WriteLine();
    Console.WriteLine($"Database: {config.FinalDatabase}");
    Console.WriteLine();
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine();
    Console.Error.WriteLine("============================================================");
    Console.Error.WriteLine(" IMPORT RESULT: CANCELLED");
    Console.Error.WriteLine("============================================================");
    Console.Error.WriteLine();

    CleanupBuildingDatabase(
        config.BuildingDatabase);

    Environment.ExitCode = 2;
}
catch (Exception ex)
{
    Console.Error.WriteLine();
    Console.Error.WriteLine("============================================================");
    Console.Error.WriteLine(" IMPORT RESULT: FAIL");
    Console.Error.WriteLine("============================================================");
    Console.Error.WriteLine();
    Console.Error.WriteLine(ex.ToString());
    Console.Error.WriteLine();

    CleanupBuildingDatabase(
        config.BuildingDatabase);

    Console.Error.WriteLine(
        "Temporary building database cleanup attempted.");

    Environment.ExitCode = 1;
}

static void CleanupBuildingDatabase(
    string path)
{
    foreach (string suffix in new[]
    {
        "",
        "-wal",
        "-shm"
    })
    {
        string candidate =
            path + suffix;

        try
        {
            if (File.Exists(candidate))
            {
                File.Delete(candidate);
            }
        }
        catch
        {
            // Preserve the original failure.
        }
    }
}