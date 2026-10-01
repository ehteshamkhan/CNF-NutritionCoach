namespace CNF.Api.Models.Responses;

public sealed class NutrientResponse
{
    public int NutrientCode { get; init; }

    public string NutrientName { get; init; } = string.Empty;

    public string NutrientSymbol { get; init; } = string.Empty;

    public string NutrientUnit { get; init; } = string.Empty;

    public decimal AmountPer100g { get; init; }

    public decimal? StandardError { get; init; }

    public decimal? Observations { get; init; }

    public int NutrientSourceCode { get; init; }

    public string? NutrientLastUpdatedDate { get; init; }
}
