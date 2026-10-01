namespace CNFImporter;

public sealed record FoodRecord(
    int FoodCode,
    string FoodName
);

public sealed record MeasureTypeRecord(
    int MeasureTypeCode,
    string MeasureType
);

public sealed record MeasureRecord(
    int FoodCode,
    string FoodName,
    int MeasureCode,
    string Measure,
    int MeasureTypeCode,
    string MeasureType,
    decimal WeightGrams
);

public sealed record NutrientRecord(
    int FoodCode,
    string FoodName,
    int NutrientCode,
    string NutrientName,
    string NutrientSymbol,
    string NutrientUnit,
    decimal AmountPer100g,
    decimal? StandardError,
    decimal? Observations,
    int NutrientSourceCode,
    DateTime NutrientLastUpdatedDate
);