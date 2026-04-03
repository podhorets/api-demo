using api_demo.Common.Exceptions;
using api_demo.Features.Baskets.Shared;
using api_demo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace api_demo.Features.Baskets.GetById;

public class GetBasketHandler(AppDbContext db)
{
    public async Task<GetBasketResponse> HandleAsync(Guid basketId, CancellationToken ct)
    {
        var basket = await db.Baskets
                         .AsNoTracking()
                         .Include(b => b.Items)
                         .FirstOrDefaultAsync(b => b.Id == basketId, ct)
                     ?? throw new NotFoundException($"Basket with ID '{basketId}' was not found.");

        return new GetBasketResponse(
            basket.Id,
            basket.Name,
            basket.Items.Select(BasketItemMapper.ToResponse).ToList(),
            basket.GetTotal(),
            basket.CreatedAt,
            basket.UpdatedAt);
    }
}