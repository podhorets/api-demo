namespace api_demo.Features.Auth.Signup;

public record SignupRequest(string Email, string Password, string? FirstName, string? LastName);
