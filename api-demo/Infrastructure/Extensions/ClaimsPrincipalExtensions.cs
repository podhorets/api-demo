using System.Security.Claims;

namespace api_demo.Infrastructure.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(claim) || !Guid.TryParse(claim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid authentication token.");
        }

        return userId;
    }
}