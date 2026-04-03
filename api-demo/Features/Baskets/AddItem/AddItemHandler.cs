using api_demo.Common.Exceptions;
using api_demo.Features.Baskets.Shared;
using api_demo.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace api_demo.Features.Baskets.AddItem;

public class AddItemHandler(
    AppDbContext db,
    IValidator<AddItemRequest> validator)
{
    public async Task<BasketResponse> HandleAsync(
        Guid basketId, AddItemRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new AppValidationException(validation);

        var basket = await db.Baskets
                         .Include(b => b.Items)
                         .FirstOrDefaultAsync(b => b.Id == basketId, ct)
                     ?? throw new NotFoundException($"Basket with ID '{basketId}' was not found.");
        
        basket.AddItem(request.ProductName, request.ItemNo, request.Quantity, request.UnitPrice);

        await db.SaveChangesAsync(ct);

        return BasketMapper.ToResponse(basket);
    }
}