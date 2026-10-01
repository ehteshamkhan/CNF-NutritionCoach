namespace CNF.Api.Models.Responses;

public sealed class FoodSearchResult
{
    public int FoodCode { get; init; }

    public string FoodName { get; init; } = string.Empty;
}
