using FluentValidation.Results;

namespace api_demo.Common.Exceptions;

public class AppValidationException : Exception
{
    public Dictionary<string, string[]> Errors { get; }

    public AppValidationException(ValidationResult result) : base("Validation failed.")
    {
        Errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());
    }

    public AppValidationException(string field, string message) : base("Validation failed.")
    {
        Errors = new Dictionary<string, string[]> { [field] = [message] };
    }
}