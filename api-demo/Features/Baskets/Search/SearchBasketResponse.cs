namespace api_demo.Features.Baskets.Search;

public record SearchBasketResponse(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    DateTime? UpdatedAt);