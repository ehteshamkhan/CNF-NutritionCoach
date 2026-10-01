namespace CNF.Api.Models.Responses;

public sealed class FoodMeasuresResponse
{
    public int FoodCode { get; init; }

    public string FoodName { get; init; } = string.Empty;

    public IReadOnlyList<MeasureResponse> Measures { get; init; } =
        Array.Empty<MeasureResponse>();
}
