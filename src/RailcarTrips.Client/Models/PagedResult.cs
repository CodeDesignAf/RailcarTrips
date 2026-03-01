namespace RailcarTrips.Client.Models;

public sealed record PagedResult<TItem>(
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyList<TItem> Items);
