namespace api_demo.Features.Auth.PasswordReset.ConfirmPasswordReset;

public record ConfirmPasswordResetRequest(string Email, string Token, string NewPassword);
