using FluentValidation;

namespace api_demo.Features.Baskets.UpdateItem;

public class UpdateItemValidator : AbstractValidator<UpdateItemRequest>
{
    public UpdateItemValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be at least 1.")
            .LessThanOrEqualTo(9999).WithMessage("Quantity must not exceed 9999.")
            .When(x => x.Quantity.HasValue);
    }
}