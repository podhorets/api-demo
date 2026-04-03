using FluentValidation;

namespace api_demo.Features.Baskets.AddItem;

public class AddItemValidator : AbstractValidator<AddItemRequest>
{
    public AddItemValidator()
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