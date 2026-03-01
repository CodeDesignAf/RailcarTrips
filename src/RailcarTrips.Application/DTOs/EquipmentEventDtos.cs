namespace RailcarTrips.Application.DTOs;

public sealed record NewEquipmentEventDto(
    string EquipmentId,
    int CityId,
    string EventCode,
    DateTime EventUtcTime,
    DateTime? EventLocalTime);

public sealed record PersistableEquipmentEventDto(
    string EquipmentId,
    int CityId,
    string EventCode,
    DateTime EventUtcTime,
    DateTime? EventLocalTime,
    string NaturalKeyHash);

public sealed record EquipmentEventDto(
    long Id,
    string EquipmentId,
    int CityId,
    string EventCode,
    DateTime EventUtcTime,
    DateTime? EventLocalTime,
    string NaturalKeyHash);

public sealed record AddEquipmentEventsResultDto(
    int Inserted,
    int Duplicates,
    IReadOnlyCollection<string> AffectedEquipmentIds);
