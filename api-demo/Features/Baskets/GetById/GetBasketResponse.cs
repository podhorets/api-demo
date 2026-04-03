using api_demo.Features.Baskets.Shared;

namespace api_demo.Features.Baskets.GetById;

public record GetBasketResponse(
    Guid Id,
    string Name,
    List<BasketItemResponse> Items,
    decimal Total,
    DateTime CreatedAt,
    DateTime? UpdatedAt);