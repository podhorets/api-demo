using api_demo.Domain.Common;

namespace api_demo.Domain.Entities;

public class User : AuditableEntity
{
    public Guid Id { get; set; }
    
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public List<Basket> Baskets { get; set; } = [];
    public List<RefreshToken> RefreshTokens { get; set; } = [];
}