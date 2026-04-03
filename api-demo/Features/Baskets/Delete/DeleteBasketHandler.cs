using api_demo.Common.Exceptions;
using api_demo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace api_demo.Features.Baskets.Delete;

public class DeleteBasketHandler(AppDbContext db)
{
    public async Task HandleAsync(Guid basketId, CancellationToken ct)
    {
        var basket = await db.Baskets.FirstOrDefaultAsync(b => b.Id == basketId, ct)
                     ?? throw new NotFoundException($"Basket with ID '{basketId}' was not found.");

        if (basket.IsDeleted)
            throw new NotFoundException($"Basket with ID '{basketId}' was not found.");
        
        basket.Delete();        
        
        await db.SaveChangesAsync(ct);
    }
}