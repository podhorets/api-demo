namespace api_demo.Features.Baskets.Search;

public record SearchBasketsRequest(
    string? Query,
    DateTime? CreatedAfter,
    DateTime? CreatedBefore,
    string SortBy = "createdAt",
    string SortDirection = "desc",
    int Page = 1,
    int PageSize = 20);