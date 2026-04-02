namespace api_demo.Features.Baskets.Shared;

public record BasketItemResponse(
    Guid Id,
    string ProductName,
    int ItemNo,
    int Quantity,
    decimal UnitPrice,
    decimal Total,
    DateTime CreatedAt);