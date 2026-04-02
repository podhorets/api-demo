using FluentValidation;

namespace api_demo.Features.Baskets.Create;

public class CreateBasketValidator : AbstractValidator<CreateBasketRequest>
{
    public CreateBasketValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Basket name is required.")
            .MaximumLength(100).WithMessage("Basket name must not exceed 100 characters.");

        RuleForEach(x => x.Items)
            .SetValidator(new CreateBasketItemValidator())
            .When(x => x.Items is not null);
    }
}

public class CreateBasketItemValidator : AbstractValidator<CreateBasketItemRequest>
{
    public CreateBasketItemValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200);
        
        RuleFor(x => x.ItemNo)
            .GreaterThan(0).WithMessage("ItemNo must be at least 1.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be at least 1.")
            .LessThanOrEqualTo(9999).WithMessage("Quantity must not exceed 9999.");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("Unit price must be greater than zero.")
            .PrecisionScale(10, 2, false)
            .WithMessage("Unit price must have at most 2 decimal places.");
    }
}