using api_demo.Common.Interfaces;
using FluentAssertions;

namespace api_demo.UnitTests.Auth;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_AndVerify_CorrectPassword_ReturnsTrue()
    {
        var hash = _hasher.Hash("SecurePassword123!");
        _hasher.Verify("SecurePassword123!", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash("CorrectPassword!");
        _hasher.Verify("WrongPassword!", hash).Should().BeFalse();
    }

    [Fact]
    public void Hash_SamePassword_ProducesDifferentHashes()
    {
        var h1 = _hasher.Hash("SamePassword!");
        var h2 = _hasher.Hash("SamePassword!");
        h1.Should().NotBe(h2);
    }
}
