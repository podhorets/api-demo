using api_demo.Common.Exceptions;
using api_demo.Common.Interfaces;
using api_demo.Domain.Entities;
using api_demo.Infrastructure.Persistence;
using api_demo.Infrastructure.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace api_demo.Features.Auth.Signup;

public class SignupHandler(
    AppDbContext db,
    IPasswordHasher hasher,
    ITokenService tokens,
    IValidator<SignupRequest> validator)
{
    public async Task<SignupResponse> HandleAsync(SignupRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new AppValidationException(validation);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(u => u.Email == normalizedEmail, ct))
            throw new ConflictException("A user with this email already exists.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            PasswordHash = hasher.Hash(request.Password),
            FirstName = request.FirstName?.Trim(),
            LastName = request.LastName?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var refreshToken = tokens.GenerateRefreshToken(user.Id, DateTime.UtcNow);

        db.Users.Add(user);
        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync(ct);

        return new SignupResponse(
            user.Id, user.Email,
            tokens.GenerateAccessToken(user),
            refreshToken.Token);
    }
}