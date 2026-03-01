using RailcarTrips.Application.DTOs;

namespace RailcarTrips.Application.Abstractions.Persistence;

public interface IEquipmentEventRepository
{
    Task<AddEquipmentEventsResultDto> AddRangeIgnoringDuplicatesAsync(
        IEnumerable<PersistableEquipmentEventDto> events,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EquipmentEventDto>> GetByEquipmentIdAsync(
        string equipmentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EquipmentEventDto>> GetByTripIdAsync(
        long tripId,
        CancellationToken cancellationToken = default);
}
