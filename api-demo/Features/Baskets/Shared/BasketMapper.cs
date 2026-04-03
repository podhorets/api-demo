using api_demo.Domain.Entities;

namespace api_demo.Features.Baskets.Shared;

public static class BasketMapper
{
    public static BasketResponse ToResponse(Basket basket)
    {
        return new BasketResponse(
            basket.Id,
            basket.Name,
            basket.Items.Select(BasketItemMapper.ToResponse).ToList(),
            basket.GetTotal(),
            basket.CreatedAt,
            basket.UpdatedAt);
    }
}