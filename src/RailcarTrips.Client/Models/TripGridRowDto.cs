namespace RailcarTrips.Client.Models;

public sealed record TripGridRowDto(
    long Id,
    string EquipmentId,
    string Origin,
    string Destination,
    DateTime StartUtc,
    DateTime? EndUtc,
    decimal? TotalTripHours);
