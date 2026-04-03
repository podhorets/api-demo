using FluentValidation;

namespace api_demo.Features.Baskets.Search;

public class SearchBasketsValidator : AbstractValidator<SearchBasketsRequest>
{
    private static readonly string[] AllowedSortFields = ["createdAt", "name"];

    public SearchBasketsValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0.");
        
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.");
        
        RuleFor(x => x.SortBy)
            .Must(s => s != null && AllowedSortFields.Contains(s.ToLowerInvariant()))
            .WithMessage($"Sort field must be one of: {string.Join(", ", AllowedSortFields)}");

        RuleFor(x => x.SortDirection)
            .Must(d => d.Equals("asc", StringComparison.OrdinalIgnoreCase)
                       || d.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Sort direction must be 'asc' or 'desc'.");
    }
}