namespace api_demo.Features.Baskets.Shared;

public record BasketResponse(
    Guid Id,
    string Name,
    List<BasketItemResponse> Items,
    decimal Total,
    DateTime CreatedAt,
    DateTime? UpdatedAt);