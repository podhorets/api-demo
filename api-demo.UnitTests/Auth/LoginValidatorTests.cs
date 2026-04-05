using api_demo.Features.Auth.Login;
using FluentAssertions;

namespace api_demo.UnitTests.Auth;

public class LoginValidatorTests
{
    private readonly LoginValidator _validator = new();

    [Fact]
    public void ValidRequest_ShouldPass()
    {
        var request = new LoginRequest("user@example.com", "anypassword");
        _validator.Validate(request).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void InvalidEmail_ShouldFail(string email)
    {
        var request = new LoginRequest(email, "anypassword");
        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void EmptyPassword_ShouldFail()
    {
        var request = new LoginRequest("user@example.com", "");
        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }
}
