using RailcarTrips.Application.DTOs;

namespace RailcarTrips.Application.UseCases;

public interface IGetTripEventsQuery
{
    Task<IReadOnlyList<EquipmentEventDto>> ExecuteAsync(
        long tripId,
        CancellationToken cancellationToken = default);
}
