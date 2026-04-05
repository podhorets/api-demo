using api_demo.Common.Exceptions;
using api_demo.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace api_demo.Features.Auth.Logout;

public class LogoutHandler(
    AppDbContext db,
    IValidator<LogoutRequest> validator)
{
    public async Task HandleAsync(LogoutRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new AppValidationException(validation);

        var token = await db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && rt.RevokedAt == null, ct);

        if (token != null)
        {
            token.RevokedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
    }
}