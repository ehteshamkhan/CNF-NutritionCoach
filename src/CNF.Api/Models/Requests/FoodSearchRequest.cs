namespace CNF.Api.Models.Requests;

public sealed class FoodSearchRequest
{
    public string Q { get; init; } = string.Empty;

    public int Limit { get; init; } = 20;
}
