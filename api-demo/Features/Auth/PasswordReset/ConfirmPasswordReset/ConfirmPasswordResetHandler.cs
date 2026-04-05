using api_demo.Common.Exceptions;
using api_demo.Domain.Entities;
using api_demo.Infrastructure.Persistence;
using api_demo.Infrastructure.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace api_demo.Features.Auth.PasswordReset.ConfirmPasswordReset;

public class ConfirmPasswordResetHandler(
    AppDbContext db,
    IPasswordHasher hasher,
    IValidator<ConfirmPasswordResetRequest> validator)
{
    public async Task HandleAsync(ConfirmPasswordResetRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) 
            throw new AppValidationException(validation);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, ct)
                   ?? throw new NotFoundException("Invalid reset request.");

        var tokens = await db.PasswordResetTokens
            .Where(t => t.UserId == user.Id && !t.IsUsed)
            .ToListAsync(ct);

        var match = tokens.FirstOrDefault(t =>
            t.IsValid && hasher.Verify(request.Token, t.TokenHash));

        if (match is null)
            throw new AppValidationException("Token", "Reset token is invalid or expired.");

        user.PasswordHash = hasher.Hash(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        match.IsUsed = true;

        var refreshTokens = await db.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null)
            .ToListAsync(ct);
        foreach (var rt in refreshTokens) 
            rt.RevokedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}