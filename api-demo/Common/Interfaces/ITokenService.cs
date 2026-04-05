using System.Security.Claims;
using api_demo.Domain.Entities;

namespace api_demo.Common.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    RefreshToken GenerateRefreshToken(Guid userId, DateTime utcNow);
}