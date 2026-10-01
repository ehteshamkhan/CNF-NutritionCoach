namespace CNF.Api.Models.Responses;

public sealed class MeasureResponse
{
    public int MeasureCode { get; init; }

    public string Measure { get; init; } = string.Empty;

    public int MeasureTypeCode { get; init; }

    public string MeasureType { get; init; } = string.Empty;

    public decimal WeightGrams { get; init; }
}
