using api_demo.Infrastructure.Services;

namespace api_demo.Common.Interfaces;

public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}
