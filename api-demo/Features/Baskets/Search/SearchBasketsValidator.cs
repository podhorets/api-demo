using FluentValidation;

namespace api_demo.Features.Baskets.Search;

public class SearchBasketsValidator : AbstractValidator<SearchBasketsRequest>
{
    public SearchBasketsValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0.");
        
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.SortDirection)
            .Must(d => d.Equals("asc", StringComparison.OrdinalIgnoreCase)
                       || d.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Sort direction must be 'asc' or 'desc'.");
    }
}