namespace CNF.Api.Models.Responses;

public sealed class FoodSearchResponse
{
    public string Query { get; init; } = string.Empty;

    public int Count { get; init; }

    public IReadOnlyList<FoodSearchResult> Items { get; init; } =
        Array.Empty<FoodSearchResult>();
}
