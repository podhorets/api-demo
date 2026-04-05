using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using api_demo.Common.Options;
using api_demo.Domain.Entities;
using api_demo.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace api_demo.UnitTests.Auth;

public class TokenServiceTests
{
    private readonly TokenService _sut;

    private static readonly string TestSecret = Convert.ToBase64String(new byte[32]);

    public TokenServiceTests()
    {
        var options = Options.Create(new JwtOptions
        {
            Secret = TestSecret,
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenMinutes = 15,
            RefreshTokenDays = 7,
            PasswordResetTokenMinutes = 60
        });
        _sut = new TokenService(options);
    }

    [Fact]
    public void GenerateAccessToken_ContainsExpectedClaims()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", FirstName = "John" };

        var token = _sut.GenerateAccessToken(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Claims.Should().Contain(c =>
            c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id.ToString());
        jwt.Claims.Should().Contain(c =>
            c.Type == ClaimTypes.Email && c.Value == user.Email);
        jwt.Claims.Should().Contain(c =>
            c.Type == "firstName" && c.Value == "John");
    }

    [Fact]
    public void GenerateAccessToken_SetsCorrectIssuerAndAudience()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com" };

        var token = _sut.GenerateAccessToken(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Issuer.Should().Be("TestIssuer");
        jwt.Audiences.Should().Contain("TestAudience");
    }

    [Fact]
    public void GenerateAccessToken_ReturnsNonEmptyString()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com" };

        var token = _sut.GenerateAccessToken(user);

        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateRefreshToken_HasCorrectExpiry()
    {
        var userId = Guid.NewGuid();
        var utcNow = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);

        var refreshToken = _sut.GenerateRefreshToken(userId, utcNow);

        refreshToken.ExpiresAt.Should().Be(utcNow.AddDays(7));
    }

    [Fact]
    public void GenerateRefreshToken_SetsCorrectUserId()
    {
        var userId = Guid.NewGuid();

        var refreshToken = _sut.GenerateRefreshToken(userId, DateTime.UtcNow);

        refreshToken.UserId.Should().Be(userId);
    }

    [Fact]
    public void GenerateRefreshToken_HasNonEmptyToken()
    {
        var refreshToken = _sut.GenerateRefreshToken(Guid.NewGuid(), DateTime.UtcNow);

        refreshToken.Token.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public void GetPrincipalFromExpiredToken_ValidToken_ReturnsPrincipal()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com" };
        var token = _sut.GenerateAccessToken(user);

        var principal = _sut.GetPrincipalFromExpiredToken(token);

        principal.Should().NotBeNull();
        principal!.FindFirst(ClaimTypes.Email)!.Value.Should().Be(user.Email);
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_NotValidJwt_ThrowsUnauthorized()
    {
        var act = () => _sut.GetPrincipalFromExpiredToken("not.a.jwt");

        act.Should().Throw<UnauthorizedAccessException>();
    }
}
