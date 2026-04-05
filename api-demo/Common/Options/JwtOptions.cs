using System.ComponentModel.DataAnnotations;

namespace api_demo.Common.Options;

public class JwtOptions
{
    [Required]
    public string Secret { get; set; } = null!;
    [Required]
    public string Issuer { get; set; } = null!;
    [Required]
    public string Audience { get; set; } = null!;
    [Required]
    public int AccessTokenMinutes { get; set; }
    [Required]
    public int RefreshTokenDays { get; set; }
}