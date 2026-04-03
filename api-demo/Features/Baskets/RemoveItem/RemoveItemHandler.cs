using api_demo.Common.Exceptions;
using api_demo.Features.Baskets.Shared;
using api_demo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace api_demo.Features.Baskets.RemoveItem;

public class RemoveItemHandler(AppDbContext db)
{
    public async Task<BasketResponse> HandleAsync(
        Guid basketId, Guid itemId, CancellationToken ct)
    {
        var basket = await db.Baskets
                         .Include(b => b.Items)
                         .FirstOrDefaultAsync(b => b.Id == basketId, ct)
                     ?? throw new NotFoundException($"Basket with ID '{basketId}' was not found.");
        
        basket.RemoveItem(itemId);
        
        await db.SaveChangesAsync(ct);

        return BasketMapper.ToResponse(basket);
    }
}