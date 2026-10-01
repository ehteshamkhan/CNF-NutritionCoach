using CNF.Api.Models.Requests;
using CNF.Api.Models.Responses;
using CNF.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CNF.Api.Controllers;

[ApiController]
[Route("api/foods")]
public sealed class FoodsController : ControllerBase
{
    private readonly FoodService _foodService;

    public FoodsController(FoodService foodService)
    {
        _foodService = foodService;
    }

    [HttpGet("search")]
    [ProducesResponseType(
        typeof(FoodSearchResponse),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<FoodSearchResponse>> Search(
        [FromQuery] FoodSearchRequest request,
        CancellationToken cancellationToken)
    {
        string query =
            request.Q?.Trim() ?? string.Empty;

        if (query.Length < 2)
        {
            return BadRequest(
                new
                {
                    error =
                        "Query parameter 'q' must contain at least 2 characters."
                });
        }

        if (query.Length > 100)
        {
            return BadRequest(
                new
                {
                    error =
                        "Query parameter 'q' cannot exceed 100 characters."
                });
        }

        int limit =
            request.Limit <= 0
                ? 20
                : request.Limit;

        if (limit > 100)
        {
            limit = 100;
        }

        IReadOnlyList<FoodSearchResult> results =
            await _foodService.SearchFoodsAsync(
                query,
                limit,
                cancellationToken);

        return Ok(
            new FoodSearchResponse
            {
                Query = query,
                Count = results.Count,
                Items = results
            });
    }

    [HttpGet("{foodCode:int}")]
    [ProducesResponseType(
        typeof(FoodResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FoodResponse>> GetFood(
        int foodCode,
        CancellationToken cancellationToken)
    {
        if (foodCode <= 0)
        {
            return BadRequest(
                new
                {
                    error =
                        "Food code must be greater than zero."
                });
        }

        FoodResponse? food =
            await _foodService.GetFoodAsync(
                foodCode,
                cancellationToken);

        if (food is null)
        {
            return NotFound(
                new
                {
                    error =
                        "Food not found.",
                    foodCode
                });
        }

        return Ok(food);
    }

    [HttpGet("{foodCode:int}/nutrients")]
    [ProducesResponseType(
        typeof(FoodNutrientsResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FoodNutrientsResponse>> GetNutrients(
        int foodCode,
        CancellationToken cancellationToken)
    {
        if (foodCode <= 0)
        {
            return BadRequest(
                new
                {
                    error =
                        "Food code must be greater than zero."
                });
        }

        FoodNutrientsResponse? result =
            await _foodService.GetNutrientsAsync(
                foodCode,
                cancellationToken);

        if (result is null)
        {
            return NotFound(
                new
                {
                    error =
                        "Food not found.",
                    foodCode
                });
        }

        return Ok(result);
    }

    [HttpGet("{foodCode:int}/measures")]
    [ProducesResponseType(
        typeof(FoodMeasuresResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FoodMeasuresResponse>> GetMeasures(
        int foodCode,
        CancellationToken cancellationToken)
    {
        if (foodCode <= 0)
        {
            return BadRequest(
                new
                {
                    error =
                        "Food code must be greater than zero."
                });
        }

        FoodMeasuresResponse? result =
            await _foodService.GetMeasuresAsync(
                foodCode,
                cancellationToken);

        if (result is null)
        {
            return NotFound(
                new
                {
                    error =
                        "Food not found.",
                    foodCode
                });
        }

        return Ok(result);
    }

    [HttpGet("{foodCode:int}/measures/{measureCode:int}")]
    [ProducesResponseType(
        typeof(MeasureResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MeasureResponse>> GetMeasure(
        int foodCode,
        int measureCode,
        CancellationToken cancellationToken)
    {
        if (foodCode <= 0)
        {
            return BadRequest(
                new
                {
                    error =
                        "Food code must be greater than zero."
                });
        }

        if (measureCode <= 0)
        {
            return BadRequest(
                new
                {
                    error =
                        "Measure code must be greater than zero."
                });
        }

        MeasureResponse? result =
            await _foodService.GetMeasureAsync(
                foodCode,
                measureCode,
                cancellationToken);

        if (result is null)
        {
            return NotFound(
                new
                {
                    error =
                        "Measure not found for the specified food.",
                    foodCode,
                    measureCode
                });
        }

        return Ok(result);
    }
}
