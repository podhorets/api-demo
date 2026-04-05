using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using api_demo.Common.Interfaces;
using api_demo.Common.Options;
using api_demo.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace api_demo.Infrastructure.Services;

public class TokenService(IOptions<JwtOptions> jwtOptions) : ITokenService
{
    private readonly JwtOptions _jwt = jwtOptions.Value;
    
    public string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("firstName", user.FirstName ?? string.Empty),
        };

        var key = new SymmetricSecurityKey(Convert.FromBase64String(_jwt.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);
        
        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken(Guid userId, DateTime utcNow) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
        CreatedAt = utcNow,
        ExpiresAt = utcNow.AddDays(7)
    };
}
