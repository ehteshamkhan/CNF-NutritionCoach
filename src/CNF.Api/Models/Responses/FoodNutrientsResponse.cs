namespace CNF.Api.Models.Responses;

public sealed class FoodNutrientsResponse
{
    public int FoodCode { get; init; }

    public string FoodName { get; init; } = string.Empty;

    public IReadOnlyList<NutrientResponse> Nutrients { get; init; } =
        Array.Empty<NutrientResponse>();
}
