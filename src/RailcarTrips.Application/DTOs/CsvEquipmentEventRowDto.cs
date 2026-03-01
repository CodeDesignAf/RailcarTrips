namespace RailcarTrips.Application.DTOs;

public sealed record CsvEquipmentEventRowDto(
    string EquipmentId,
    string CityId,
    string EventCode,
    string EventTime);
