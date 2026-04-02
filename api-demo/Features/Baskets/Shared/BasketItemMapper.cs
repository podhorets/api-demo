using api_demo.Domain.Entities;

namespace api_demo.Features.Baskets.Shared;

public static class BasketItemMapper
{
    public static BasketItemResponse ToResponse(this BasketItem basketItem)
    {
        return new BasketItemResponse(
            basketItem.Id,
            basketItem.ProductName,
            basketItem.ItemNo,
            basketItem.Quantity,
            basketItem.UnitPrice, 
            basketItem.GetTotal(),
            basketItem.CreatedAt);
    }
}