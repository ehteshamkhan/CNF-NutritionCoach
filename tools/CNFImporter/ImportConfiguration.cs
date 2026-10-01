namespace CNFImporter;

public sealed class ImportConfiguration
{
    public string CnfRoot { get; init; } = string.Empty;

    public string ValidatedRoot { get; init; } = string.Empty;

    public string CanonicalFoodCsv { get; init; } = string.Empty;

    public string MeasureRoot { get; init; } = string.Empty;

    public string NutrientRoot { get; init; } = string.Empty;

    public string BuildingDatabase { get; init; } = string.Empty;

    public string FinalDatabase { get; init; } = string.Empty;

    public string SchemaFile { get; init; } = string.Empty;

    public string ValidationSqlFile { get; init; } = string.Empty;

    public int ExpectedFoods { get; init; }

    public int ExpectedMeasureTypes { get; init; }

    public int ExpectedMeasures { get; init; }

    public int ExpectedNutrients { get; init; }

    public int NutrientBatchSize { get; init; }
}