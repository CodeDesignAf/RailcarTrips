namespace RailcarTrips.Client.Models;

public sealed record EquipmentEventDto(
    long Id,
    string EquipmentId,
    int CityId,
    string? CityName,
    string EventCode,
    DateTime EventUtcTime,
    DateTime? EventLocalTime);
