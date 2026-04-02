namespace api_demo.Common.Exceptions;

public class AppValidationException(string field, string message) : Exception("Validation failed.")
{
    public Dictionary<string, string[]> Errors { get; } = new() { [field] = [message] };
}