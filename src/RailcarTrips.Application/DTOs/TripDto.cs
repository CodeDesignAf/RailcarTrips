namespace RailcarTrips.Application.DTOs;

public sealed record TripDto(
    long Id,
    string EquipmentId,
    int OriginCityId,
    string OriginCityName,
    int? DestinationCityId,
    string? DestinationCityName,
    DateTime StartUtc,
    DateTime? EndUtc,
    decimal? TotalTripHours,
    string Status,
    string? InvalidReason);

public sealed record PagedTripsResult(
    IReadOnlyList<TripDto> Items,
    int TotalCount);
