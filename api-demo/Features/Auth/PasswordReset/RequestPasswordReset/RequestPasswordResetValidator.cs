using FluentValidation;

namespace api_demo.Features.Auth.PasswordReset.RequestPasswordReset;

public class RequestPasswordResetValidator : AbstractValidator<RequestPasswordResetRequest>
{
    public RequestPasswordResetValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
    }
}