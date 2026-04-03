namespace api_demo.Common.Models;

public record ApiErrorResponse(
    string Code,
    string Message,
    Dictionary<string, string[]>? Errors = null);