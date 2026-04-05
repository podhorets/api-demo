using api_demo.Common.Models;
using api_demo.Features.Auth.Login;
using api_demo.Features.Auth.Logout;
using api_demo.Features.Auth.PasswordReset.ConfirmPasswordReset;
using api_demo.Features.Auth.PasswordReset.RequestPasswordReset;
using api_demo.Features.Auth.RefreshToken;
using api_demo.Features.Auth.Signup;
using Microsoft.AspNetCore.Mvc;

namespace api_demo.Features.Auth;

[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    [HttpPost("signup")]
    [ProducesResponseType(typeof(SignupResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Signup(
        [FromBody] SignupRequest request,
        [FromServices] SignupHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        [FromServices] LoginHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(request, ct);
        return Ok(result);
    }
    
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        [FromServices] LogoutHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(request, ct);
        return NoContent();
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        [FromServices] RefreshTokenHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(request, ct);
        return Ok(result);
    }

    /// <summary>
    /// Demo endpoint: password reset link would be emailed 
    /// </summary>
    /// <param name="request">request</param>
    /// <param name="handler">handler</param>
    /// <param name="ct">cancellation token</param>
    /// <returns>Demo: link, Production: empty</returns>
    [HttpPost("password-reset/request")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RequestPasswordReset(
        [FromBody] RequestPasswordResetRequest request,
        [FromServices] RequestPasswordResetHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(request, ct);
        return Ok(result);
    }

    [HttpPost("password-reset/confirm")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmPasswordReset(
        [FromBody] ConfirmPasswordResetRequest request,
        [FromServices] ConfirmPasswordResetHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(request, ct);
        return Ok(new { Message = "Password has been reset successfully." });
    }
}