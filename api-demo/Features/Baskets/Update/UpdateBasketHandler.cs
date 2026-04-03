using api_demo.Common.Exceptions;
using api_demo.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace api_demo.Features.Baskets.Update;

public class UpdateBasketHandler(AppDbContext db, IValidator<UpdateBasketRequest> validator)
{
    public async Task<UpdateBasketResponse> HandleAsync(
        Guid basketId, UpdateBasketRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new AppValidationException(validation);

        var basket = await db.Baskets.FirstOrDefaultAsync(b => b.Id == basketId, ct) 
                     ?? throw new NotFoundException($"Basket with ID '{basketId}' was not found.");
        
        if (request.Name is not null)
            basket.Update(request.Name);
        
        await db.SaveChangesAsync(ct);

        return new UpdateBasketResponse(basket.Id, basket.Name);
    }
}