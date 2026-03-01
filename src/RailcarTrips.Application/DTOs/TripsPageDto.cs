namespace RailcarTrips.Application.DTOs;

public sealed record TripsPageDto(
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyList<TripDto> Items);
