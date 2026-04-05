using api_demo.Common.Exceptions;
using api_demo.Common.Models;
using api_demo.Domain.Entities;
using api_demo.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace api_demo.Features.Baskets.Search;

public class SearchBasketsHandler(AppDbContext db, IValidator<SearchBasketsRequest> validator)
{
    public async Task<PagedResponse<SearchBasketResponse>> HandleAsync(
        SearchBasketsRequest request, Guid userId, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new AppValidationException(validation);
        
        IQueryable<Basket> query = db.Baskets.Where(b => b.UserId == userId);

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var search = request.Query.Trim();
            query = query.Where(b => EF.Functions.ILike(b.Name, $"%{search}%"));
        }

        if (request.CreatedAfter.HasValue)
            query = query.Where(b => b.CreatedAt >= request.CreatedAfter.Value);

        if (request.CreatedBefore.HasValue)
            query = query.Where(b => b.CreatedAt <= request.CreatedBefore.Value);
        
        var isAsc = request.SortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase);
        query = request.SortBy.ToLowerInvariant() switch
        {
            "name" => isAsc
                ? query.OrderBy(b => b.Name)
                : query.OrderByDescending(b => b.Name),
            "createdAt" => isAsc
                ? query.OrderBy(b => b.CreatedAt)
                : query.OrderByDescending(b => b.CreatedAt),
            _ => query
        };

        var totalCount = await query.CountAsync(ct);

        var baskets = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);
        
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new PagedResponse<SearchBasketResponse>(
            baskets.Select(b => new SearchBasketResponse(b.Id, b.Name, b.CreatedAt, b.UpdatedAt)).ToList(),
            request.Page,
            request.PageSize,
            totalCount,
            totalPages);
    }
}