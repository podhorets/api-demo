using FluentValidation;

namespace api_demo.Features.Baskets.Update;

public class UpdateBasketValidator : AbstractValidator<UpdateBasketRequest>
{
    public UpdateBasketValidator()
    {
        RuleFor(x => x.Name)
            .NotNull()
            .NotEmpty()
            .MinimumLength(1)
            .MaximumLength(100)
            .WithMessage("Basket name must not exceed 100 characters.");
    }
}