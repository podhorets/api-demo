using api_demo.Common.Exceptions;
using api_demo.Common.Interfaces;
using api_demo.Common.Options;
using api_demo.Infrastructure.Persistence;
using api_demo.Infrastructure.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace api_demo.Features.Auth.Login;

public class LoginHandler(
    AppDbContext db,
    IPasswordHasher hasher,
    ITokenService tokens,
    IValidator<LoginRequest> validator,
    IOptions<JwtOptions> jwtOptions)
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    
    public async Task<LoginResponse> HandleAsync(LoginRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new AppValidationException(validation);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, ct);

        if (user is null || !hasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var existingTokens = await db.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var token in existingTokens)
            token.RevokedAt = DateTime.UtcNow;

        var refreshToken = tokens.GenerateRefreshToken(user.Id, DateTime.UtcNow);
        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync(ct);

        return new LoginResponse(
            tokens.GenerateAccessToken(user),
            refreshToken.Token,
            DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes));
    }
}