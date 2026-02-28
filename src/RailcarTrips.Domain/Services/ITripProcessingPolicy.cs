using RailcarTrips.Domain.Entities;

namespace RailcarTrips.Domain.Services;

public interface ITripProcessingPolicy
{
    Trip OnReleasedWhileOpen(Trip openTrip, EquipmentEvent releasedEvent, TripProcessingResult result);
    void OnPlacedWithoutOpen(EquipmentEvent placedEvent, TripProcessingResult result);
}
