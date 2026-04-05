namespace api_demo.Features.Auth.RefreshToken;

public record RefreshTokenRequest(string AccessToken, string RefreshToken);
