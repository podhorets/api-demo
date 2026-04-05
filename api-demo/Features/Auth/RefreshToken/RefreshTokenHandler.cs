using System.Security.Claims;
using api_demo.Common.Interfaces;
using api_demo.Common.Options;
using api_demo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace api_demo.Features.Auth.RefreshToken;

public class RefreshTokenHandler(AppDbContext db, ITokenService tokens, IOptions<JwtOptions> jwtOptions)
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    
    public async Task<RefreshTokenResponse> HandleAsync(
        RefreshTokenRequest request, CancellationToken ct)
    {
        var principal = tokens.GetPrincipalFromExpiredToken(request.AccessToken)
                        ?? throw new UnauthorizedAccessException("Invalid access token.");

        var userIdParsed = Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);
        if (!userIdParsed)
            throw new UnauthorizedAccessException("Invalid access token.");

        var storedToken = await db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt =>
                rt.Token == request.RefreshToken && rt.UserId == userId, ct);

        if (storedToken is null || !storedToken.IsActive)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        storedToken.RevokedAt = DateTime.UtcNow;
        var newRefreshToken = tokens.GenerateRefreshToken(userId, DateTime.UtcNow);
        db.RefreshTokens.Add(newRefreshToken);
        await db.SaveChangesAsync(ct);

        return new RefreshTokenResponse(
            tokens.GenerateAccessToken(storedToken.User),
            newRefreshToken.Token,
            DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes));
    }
}