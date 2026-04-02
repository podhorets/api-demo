using api_demo.Features.Baskets.Shared;

namespace api_demo.Features.Baskets.Create;

public record CreateBasketResponse(
    Guid Id,
    string Name,
    List<BasketItemResponse> Items,
    decimal Total,
    DateTime CreatedAt);