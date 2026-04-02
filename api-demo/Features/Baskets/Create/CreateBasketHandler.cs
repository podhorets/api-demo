using api_demo.Common.Exceptions;
using api_demo.Domain.Entities;
using api_demo.Features.Baskets.Shared;
using api_demo.Infrastructure.Persistence;
using FluentValidation;

namespace api_demo.Features.Baskets.Create;

public class CreateBasketHandler(AppDbContext db, IValidator<CreateBasketRequest> validator)
{
    public async Task<CreateBasketResponse> HandleAsync(CreateBasketRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new AppValidationException(validation);

        var basket = new Basket(request.Name);

        if (request.Items is { Count: > 0 })
        {
            foreach (var item in request.Items)
            {
                basket.AddItem(item.ProductName, item.ItemNo, item.Quantity, item.UnitPrice);
            }
        }

        db.Baskets.Add(basket);
        await db.SaveChangesAsync(ct);

        return new CreateBasketResponse(
            basket.Id,
            basket.Name,
            basket.Items.Select(BasketItemMapper.ToResponse).ToList(),
            basket.GetTotal(),
            basket.CreatedAt);
    }
}