using System.Security.Cryptography;
using api_demo.Common.Exceptions;
using api_demo.Common.Options;
using api_demo.Domain.Entities;
using api_demo.Infrastructure.Persistence;
using api_demo.Infrastructure.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace api_demo.Features.Auth.PasswordReset.RequestPasswordReset;

public class RequestPasswordResetHandler(
    AppDbContext db,
    IPasswordHasher hasher,
    ILogger<RequestPasswordResetHandler> logger,
    IValidator<RequestPasswordResetRequest> validator,
    IOptions<JwtOptions> jwtOptions)
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<object> HandleAsync(
        RequestPasswordResetRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new AppValidationException(validation);
        
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, ct);

        if (user is null)
        {
            logger.LogWarning("Password reset requested for unknown email: {Email}", normalizedEmail);
            return new { Message = "If the email exists, a reset token has been generated." };
        }

        var notUsedPwResetTokens = await db.PasswordResetTokens
            .Where(t => t.UserId == user.Id && !t.IsUsed)
            .ToListAsync(ct);
        
        foreach (var token in notUsedPwResetTokens) 
            token.IsUsed = true;

        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = hasher.Hash(rawToken),
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.PasswordResetTokenMinutes),
            CreatedAt = DateTime.UtcNow
        };

        db.PasswordResetTokens.Add(resetToken);
        await db.SaveChangesAsync(ct);
        
        var resetLink = $"(baseUrl)/reset-password?token={Uri.EscapeDataString(rawToken)}&email={Uri.EscapeDataString(user.Email)}";

        // TODO In production: send email
        // await _emailService.SendAsync(user.Email, "Reset Your Password", $"Click here to reset your password: {resetLink}");

        // Return demo response
        return new
        {
            Message = $"Demo: password reset link would be emailed. Example link: {resetLink}",
            Token = rawToken,
            ExpiresAt = resetToken.ExpiresAt
        };
    }
}