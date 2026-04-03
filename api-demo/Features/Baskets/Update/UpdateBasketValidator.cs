using FluentValidation;

namespace api_demo.Features.Baskets.Update;

public class UpdateBasketValidator : AbstractValidator<UpdateBasketRequest>
{
    public UpdateBasketValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100)
            .WithMessage("Basket name must not exceed 100 characters.")
            .When(x => x.Name is not null);
    }
}