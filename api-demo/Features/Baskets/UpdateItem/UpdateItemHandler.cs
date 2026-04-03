using api_demo.Common.Exceptions;
using api_demo.Features.Baskets.Shared;
using api_demo.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace api_demo.Features.Baskets.UpdateItem;

public class UpdateItemHandler(
    AppDbContext db,
    IValidator<UpdateItemRequest> validator)
{
    public async Task<BasketResponse> HandleAsync(
        Guid basketId, Guid itemId, UpdateItemRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new AppValidationException(validation);

        var basket = await db.Baskets
                         .Include(b => b.Items)
                         .FirstOrDefaultAsync(b => b.Id == basketId, ct)
                     ?? throw new NotFoundException($"Basket with ID '{basketId}' was not found.");

        if (request.Quantity.HasValue)
        {
            basket.UpdateItem(itemId, request.Quantity.Value);
            await db.SaveChangesAsync(ct);
        }
        
        return BasketMapper.ToResponse(basket);
    }
}