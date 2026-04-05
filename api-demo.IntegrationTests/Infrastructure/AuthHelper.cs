using System.Net.Http.Json;
using System.Text.Json;

namespace api_demo.IntegrationTests.Infrastructure;

public class AuthHelper(HttpClient client)
{
    private static int _counter;

    public static string UniqueEmail() =>
        $"test{Interlocked.Increment(ref _counter)}_{Guid.NewGuid():N}@example.com";

    public async Task<string> SignupAndGetTokenAsync(string? email = null, string password = "TestPassword123!")
    {
        email ??= UniqueEmail();
        var resp = await client.PostAsJsonAsync("/api/v1/auth/signup",
            new { Email = email, Password = password });
        resp.EnsureSuccessStatusCode();
        var result = await resp.Content.ReadFromJsonAsync<JsonElement>();
        return result.GetProperty("accessToken").GetString()!;
    }

    public async Task<(string AccessToken, string RefreshToken)> SignupAsync(
        string? email = null, string password = "TestPassword123!")
    {
        email ??= UniqueEmail();
        var resp = await client.PostAsJsonAsync("/api/v1/auth/signup",
            new { Email = email, Password = password });
        resp.EnsureSuccessStatusCode();
        var result = await resp.Content.ReadFromJsonAsync<JsonElement>();
        return (
            result.GetProperty("accessToken").GetString()!,
            result.GetProperty("refreshToken").GetString()!
        );
    }
}
