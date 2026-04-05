namespace api_demo.Features.Auth.Signup;

public record SignupResponse(Guid UserId, string Email, string AccessToken, string RefreshToken);
