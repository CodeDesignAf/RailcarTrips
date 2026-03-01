using RailcarTrips.Application.Abstractions.Persistence;
using RailcarTrips.Application.DTOs;

namespace RailcarTrips.Application.UseCases;

public class GetTripEventsQuery : IGetTripEventsQuery
{
    private readonly IEquipmentEventRepository _eventRepository;

    public GetTripEventsQuery(IEquipmentEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public Task<IReadOnlyList<EquipmentEventDto>> ExecuteAsync(
        long tripId,
        CancellationToken cancellationToken = default)
    {
        return _eventRepository.GetByTripIdAsync(tripId, cancellationToken);
    }
}
