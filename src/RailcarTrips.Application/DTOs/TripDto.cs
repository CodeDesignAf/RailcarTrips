namespace RailcarTrips.Application.DTOs;

public sealed record TripDto(
    long Id,
    string EquipmentId,
    int OriginCityId,
    int? DestinationCityId,
    DateTime StartUtc,
    DateTime? EndUtc,
    decimal? TotalTripHours,
    string Status,
    string? InvalidReason);

public sealed record PagedTripsResult(
    IReadOnlyList<TripDto> Items,
    int TotalCount);
