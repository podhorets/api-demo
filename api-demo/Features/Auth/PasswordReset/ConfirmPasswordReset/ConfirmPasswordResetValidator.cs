using FluentValidation;

namespace api_demo.Features.Auth.PasswordReset.ConfirmPasswordReset;

public class ConfirmPasswordResetValidator : AbstractValidator<ConfirmPasswordResetRequest>
{
    public ConfirmPasswordResetValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(128);
    }
}