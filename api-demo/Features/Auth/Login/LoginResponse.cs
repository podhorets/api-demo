namespace api_demo.Features.Auth.Login;

public record LoginResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);
