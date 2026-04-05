using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using api_demo.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace api_demo.IntegrationTests.Tests;

public class AuthEndpointTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Signup_Returns201WithTokens()
    {
        var resp = await _client.PostAsJsonAsync("/api/v1/auth/signup", new
        {
            Email = AuthHelper.UniqueEmail(),
            Password = "StrongPass123!",
            FirstName = "John"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await resp.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
        result.GetProperty("refreshToken").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Signup_DuplicateEmail_Returns409()
    {
        var email = AuthHelper.UniqueEmail();
        await _client.PostAsJsonAsync("/api/v1/auth/signup",
            new { Email = email, Password = "StrongPass123!" });

        var resp = await _client.PostAsJsonAsync("/api/v1/auth/signup",
            new { Email = email, Password = "AnotherPass456!" });

        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Signup_WeakPassword_Returns400()
    {
        var resp = await _client.PostAsJsonAsync("/api/v1/auth/signup", new
        {
            Email = AuthHelper.UniqueEmail(),
            Password = "short"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await resp.Content.ReadFromJsonAsync<JsonElement>();
        error.GetProperty("code").GetString().Should().Be("VALIDATION_ERROR");
        error.GetProperty("errors").EnumerateObject().Should().NotBeEmpty();
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithTokens()
    {
        var email = AuthHelper.UniqueEmail();
        await _client.PostAsJsonAsync("/api/v1/auth/signup",
            new { Email = email, Password = "StrongPass123!" });

        var resp = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { Email = email, Password = "StrongPass123!" });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await resp.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
        result.GetProperty("refreshToken").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var email = AuthHelper.UniqueEmail();
        await _client.PostAsJsonAsync("/api/v1/auth/signup",
            new { Email = email, Password = "CorrectPass123!" });

        var resp = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { Email = email, Password = "WrongPass999!" });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_UnknownEmail_Returns401()
    {
        var resp = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { Email = "nobody@example.com", Password = "Pass123456!" });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_ValidTokenPair_ReturnsNewTokens()
    {
        var auth = new AuthHelper(_client);
        var (accessToken, refreshToken) = await auth.SignupAsync();

        var resp = await _client.PostAsJsonAsync("/api/v1/auth/refresh",
            new { AccessToken = accessToken, RefreshToken = refreshToken });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await resp.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
        result.GetProperty("refreshToken").GetString().Should().NotBeNullOrEmpty();
        result.GetProperty("refreshToken").GetString().Should().NotBe(refreshToken);
    }

    [Fact]
    public async Task Refresh_RevokedRefreshToken_Returns401()
    {
        var auth = new AuthHelper(_client);
        var (accessToken, refreshToken) = await auth.SignupAsync();

        var first = await _client.PostAsJsonAsync("/api/v1/auth/refresh",
            new { AccessToken = accessToken, RefreshToken = refreshToken });
        first.StatusCode.Should().Be(HttpStatusCode.OK);

        var second = await _client.PostAsJsonAsync("/api/v1/auth/refresh",
            new { AccessToken = accessToken, RefreshToken = refreshToken });
        second.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_RevokesRefreshToken()
    {
        var auth = new AuthHelper(_client);
        var (accessToken, refreshToken) = await auth.SignupAsync();

        var logoutResp = await _client.PostAsJsonAsync("/api/v1/auth/logout",
            new { RefreshToken = refreshToken });
        logoutResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var refreshResp = await _client.PostAsJsonAsync("/api/v1/auth/refresh",
            new { AccessToken = accessToken, RefreshToken = refreshToken });
        refreshResp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PasswordReset_FullFlow()
    {
        var email = AuthHelper.UniqueEmail();
        await _client.PostAsJsonAsync("/api/v1/auth/signup",
            new { Email = email, Password = "OldPassword123!" });

        var reqResp = await _client.PostAsJsonAsync("/api/v1/auth/password-reset/request",
            new { Email = email });
        reqResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var resetPayload = await reqResp.Content.ReadFromJsonAsync<JsonElement>();
        var resetToken = resetPayload.GetProperty("token").GetString();
        resetToken.Should().NotBeNullOrEmpty();

        var confirmResp = await _client.PostAsJsonAsync("/api/v1/auth/password-reset/confirm",
            new { Email = email, Token = resetToken, NewPassword = "NewPassword456!" });
        confirmResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var newLoginResp = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { Email = email, Password = "NewPassword456!" });
        newLoginResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var oldLoginResp = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { Email = email, Password = "OldPassword123!" });
        oldLoginResp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
